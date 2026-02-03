using System;
using _SLIME.BaseScripts;
using _SLIME.Boss;
using _SLIME.Core.Audio.FMOD.Scripts;
using _SLIME.GameLoop;
using _SLIME.Slime;
using DG.Tweening;
using FMODUnity;
using UnityEngine;
using NaughtyAttributes;

namespace _SLIME.Projectiles
{
    public enum SpellState { Spawning, Flying, Hit, Deflected }
    
    public class Spell : ProjectMonoBehavior
    {
        private static readonly int Hit = Animator.StringToHash("Hit");
        private static readonly int BossMove = Animator.StringToHash("BossMove");
        private static readonly int PlayerMove = Animator.StringToHash("SlimeMove");

        [SerializeField] private SpellComp comp;

        [SerializeField]
        private BaseBossConfigurations[] bossConfigurations;
        private SpellBossAttributes _bossAttributes;
        private SpellState _currentState;
        private float _lobElapsedTime;
        private bool _lobPhaseComplete;
        private SpellBeforeSpawn _waitingForSpellBefore;
        private float _deflectLobElapsedTime;
        private bool _deflectLobGoingDown;
        private Vector2 _deflectP0;
        private Vector2 _deflectP2;
        private float _deflectDuration;
        private float _deflectDestroyLineY;
        private SpellSlimeAttributes _deflectAttributes;
        
        [MinMaxSlider(0.1f, 10f)]
        [SerializeField] private Vector2 spellScaleFactor;
        
        [SerializeField] private EventReference spellShootSFX;

        private void OnDisable()
        {
            transform.DOKill();
        }

        public void BossSetup(SpellBossAttributes attributes)
        {
            _bossAttributes = attributes;
            comp.rb.linearVelocity = Vector2.zero;
            comp.rb.bodyType = RigidbodyType2D.Kinematic;
            Vector2 dir = (attributes.targetPosition - attributes.spawnPosition).normalized;
            float z = Mathf.Atan2(dir.x, -dir.y) * Mathf.Rad2Deg;
            if (z < 0f) z += 360f;
            transform.rotation = Quaternion.Euler(0f, 0f, z);
            comp.spellHead.transform.rotation = Quaternion.Euler(0f, 0f, z);
            comp.spellBody.localScale = attributes.scaleStart;
        }

        public void SetWaitingForSpawn(SpellBeforeSpawn spellBefore)
        {
            _waitingForSpellBefore = spellBefore;
        }

        public bool HasStartedFlying()
        {
            return _currentState == SpellState.Flying;
        }

        private void Update()
        {
            if (_waitingForSpellBefore == null) return;
            if (!_waitingForSpellBefore.GetState()) return;

            var spellBefore = _waitingForSpellBefore;
            _waitingForSpellBefore = null;
            Vector3 spawnPos = spellBefore.comp.spawnPoint.position;
            transform.position = spawnPos;
            Destroy(spellBefore.gameObject);
            comp.animator.Rebind();
            comp.animator.Play("Spawn", 0, 0f);
        }
        
        public void Deflect(SpellSlimeAttributes attributes)
        {
            if (_currentState != SpellState.Flying) return;
            comp.spellBody.DOKill();
            _currentState = SpellState.Deflected;
            comp.collider.gameObject.layer = GetLayerFromMask(attributes.layerMask);
            comp.animator.SetTrigger(PlayerMove);
            SFXPlayer.Play(spellShootSFX);
            _deflectAttributes = attributes;

            float finalPower = comp.rb.linearVelocity.magnitude * attributes.deflectionPower;
            Vector2 dir = attributes.direction.normalized;

            if (attributes.deflectLobArcHeight > 0f && attributes.deflectLobUpDuration > 0f)
            {
                comp.rb.bodyType = RigidbodyType2D.Kinematic;
                comp.rb.linearVelocity = Vector2.zero;
                if (comp.collider != null) comp.collider.enabled = false;

                _deflectP0 = transform.position;
                _deflectDestroyLineY = GetDeflectDestroyLineY();
                float t = Mathf.Abs(dir.y) > 0.01f ? (_deflectDestroyLineY - _deflectP0.y) / dir.y : 5f;
                if (t <= 0f) t = 5f;
                _deflectP2 = _deflectP0 + dir * Mathf.Max(t, 2f);
                _deflectP2.y = _deflectDestroyLineY;

                _deflectLobElapsedTime = 0f;
                _deflectLobGoingDown = false;
                float upDuration = attributes.deflectLobUpDuration;
                float totalDist = Vector2.Distance(_deflectP0, _deflectP2);
                _deflectDuration = upDuration + Mathf.Max(0.1f, totalDist / Mathf.Max(0.1f, finalPower));
            }
            else
            {
                comp.rb.bodyType = RigidbodyType2D.Dynamic;
                comp.rb.AddForce(dir * finalPower, ForceMode2D.Impulse);
                PlayScaleDown(finalPower, attributes);
            }
        }

        private float GetDeflectDestroyLineY()
        {
            var boss = FindObjectOfType<BossBrain>();
            if (boss != null && boss.spawnDeps.spawnPoint != null)
                return boss.spawnDeps.spawnPoint.position.y;
            return transform.position.y - 10f;
        }
        
        private void Shoot()
        {
            _currentState = SpellState.Flying;
            Vector2 dir = (_bossAttributes.targetPosition - _bossAttributes.spawnPosition).normalized;
            float speed = _bossAttributes.moveSpeed;

            if (_bossAttributes.lobArcHeight > 0f)
            {
                _lobElapsedTime = 0f;
                _lobPhaseComplete = false;
            }
            else
                comp.rb.linearVelocity = dir * speed;

            comp.animator.SetTrigger(BossMove);
            PlayScaleUp();
        }

        private void FixedUpdate()
        {
            if (_currentState == SpellState.Deflected && _deflectAttributes.deflectLobArcHeight > 0f)
            {
                UpdateDeflectLob();
                return;
            }
            if (_currentState != SpellState.Flying || _bossAttributes.lobArcHeight <= 0f) return;

            Vector2 p0 = _bossAttributes.spawnPosition;
            Vector2 p2 = _bossAttributes.targetPosition;
            float dist = Vector2.Distance(p0, p2);
            float duration = Mathf.Max(0.01f, dist / _bossAttributes.moveSpeed);
            _lobElapsedTime += Time.fixedDeltaTime;
            float t = _lobElapsedTime / duration;

            if (t < 1f)
            {
                Vector2 p1 = (p0 + p2) * 0.5f + Vector2.up * _bossAttributes.lobArcHeight;
                Vector2 pos = (1f - t) * (1f - t) * p0 + 2f * (1f - t) * t * p1 + t * t * p2;
                Vector2 vel = (2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1)) / duration;
                comp.rb.MovePosition(pos);
                comp.rb.linearVelocity = vel;
            }
            else if (!_lobPhaseComplete)
            {
                _lobPhaseComplete = true;
                Vector2 dir = (p2 - p0).normalized;
                comp.rb.linearVelocity = dir * _bossAttributes.moveSpeed;
            }
        }

        private void UpdateDeflectLob()
        {
            _deflectLobElapsedTime += Time.fixedDeltaTime;
            float upDuration = _deflectAttributes.deflectLobUpDuration;

            if (_deflectLobElapsedTime < upDuration)
            {
                float t = _deflectLobElapsedTime / upDuration;
                Vector2 peak = (_deflectP0 + _deflectP2) * 0.5f + Vector2.up * _deflectAttributes.deflectLobArcHeight;
                Vector2 pos = Vector2.Lerp(_deflectP0, peak, t);
                comp.rb.MovePosition(pos);
            }
            else
            {
                if (!_deflectLobGoingDown)
                {
                    _deflectLobGoingDown = true;
                    if (comp.collider != null) comp.collider.enabled = true;
                }
                float downElapsed = _deflectLobElapsedTime - upDuration;
                float downDuration = Mathf.Max(0.1f, _deflectDuration - upDuration);
                float t = downElapsed / downDuration;
                Vector2 peak = (_deflectP0 + _deflectP2) * 0.5f + Vector2.up * _deflectAttributes.deflectLobArcHeight;
                Vector2 pos = Vector2.Lerp(peak, _deflectP2, t);
                comp.rb.MovePosition(pos);

                if (pos.y <= _deflectDestroyLineY || t >= 1f)
                {
                    OnHitFinished();
                }
            }
        }

        private void PlayScaleUp()
        {
            float duration;
            float closeThreshold = _bossAttributes.scaleUpCloseDistanceThreshold > 0 ? _bossAttributes.scaleUpCloseDistanceThreshold : 2f;
            float closeDuration = _bossAttributes.scaleUpDurationWhenClose > 0 ? _bossAttributes.scaleUpDurationWhenClose : 0.08f;
            if (GetMinDistanceToSlime() < closeThreshold)
                duration = Mathf.Max(0.02f, closeDuration);
            else
            {
                float factor = _bossAttributes.scaleUpDurationFactor > 0 ? _bossAttributes.scaleUpDurationFactor : 5f;
                duration = Mathf.Max(0.05f, factor / Mathf.Max(0.1f, _bossAttributes.moveSpeed));
            }
            var curve = _bossAttributes.scaleUpCurve;
            if (curve == null || curve.keys.Length == 0)
                curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            comp.spellBody.DOScale(Vector3.one, duration).SetEase(curve).SetUpdate(UpdateType.Normal);
        }

        private float GetMinDistanceToSlime()
        {
            if (SlimeData.instance == null) return float.MaxValue;
            Vector3 pos = transform.position;
            float min = float.MaxValue;
            if (!SlimeData.instance.SideADead)
                min = Mathf.Min(min, Vector3.Distance(pos, SlimeData.instance.SideATransform.position));
            if (!SlimeData.instance.SideBDead)
                min = Mathf.Min(min, Vector3.Distance(pos, SlimeData.instance.SideBTransform.position));
            return min;
        }

        private void PlayScaleDown(float finalPower, SpellSlimeAttributes attributes)
        {
            float factor = attributes.scaleDownDurationFactor > 0 ? attributes.scaleDownDurationFactor : 3f;
            float duration = Mathf.Max(0.05f, factor / Mathf.Max(0.1f, finalPower));
            Vector3 target = attributes.scaleDownTarget;
            var curve = attributes.scaleDownCurve;
            if (curve == null || curve.keys.Length == 0)
                curve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            comp.spellBody.DOScale(target, duration).SetEase(curve).SetUpdate(UpdateType.Normal)
                .OnComplete(() => HandleImpact(null));
        }
        
        public void OnSpawnFinished()
        {
            if (_currentState != SpellState.Spawning) return;
            Shoot();
        }
        
        public void OnHitFinished()
        {
            comp.spellBody.DOKill();
            Destroy(transform.parent.gameObject);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_currentState != SpellState.Flying && _currentState != SpellState.Deflected) return;
            
            var rig = other.attachedRigidbody;
            if (rig && rig.TryGetComponent<IHealth>(out IHealth h))
            {
                HandleImpact(h);
            }
            else if (other.CompareTag("Wall")) 
            {
                HandleImpact(null); 
            }
        }

        private void HandleImpact(IHealth target)
        {
            _currentState = SpellState.Hit;
            var currentSpeed = comp.rb.linearVelocity.magnitude;
            comp.rb.linearVelocity = Vector2.zero;
            comp.rb.bodyType = RigidbodyType2D.Kinematic;
            if (target != null)
            {
                
                target.TakeDamage(Mathf.CeilToInt(currentSpeed));
            }
            HitParticleScaleChange(currentSpeed);
            comp.animator.SetTrigger(Hit);
        }

        private void HitParticleScaleChange(float currentSpeed)
        {
            
            float expectedSpeed = BossBrain.bossConfigurations ? BossBrain.bossConfigurations.PhaseSettings.expectedAvgSpeedOfSpells
                    : currentSpeed;
            float minScale = spellScaleFactor.x;
            float maxScale = spellScaleFactor.y;
            float midScale = (minScale + maxScale) * 0.5f;
            
            float speedRatio = currentSpeed / expectedSpeed;
            
            float scale;
            if (speedRatio <= 1f)
            {
                scale = Mathf.Lerp(minScale, midScale, speedRatio);
            }
            else
            {
                float t = (speedRatio - 1f);
                scale = Mathf.Lerp(midScale, maxScale, t);
            }
            
            scale = Mathf.Clamp(scale, minScale, maxScale);
            
            foreach (var h in comp.spellHit)
            {
                h.localScale = Vector3.one * scale;
            }
        }


        private int GetLayerFromMask(LayerMask mask)
        {
            int layerNumber = 0;
            int layer = mask.value;
            while (layer > 1)
            {
                layer >>= 1;
                layerNumber++;
            }
            return layerNumber;
        }
    }
}
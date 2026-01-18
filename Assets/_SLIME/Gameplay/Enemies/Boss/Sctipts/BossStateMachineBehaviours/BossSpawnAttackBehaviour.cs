using _SLIME.Projectiles;
using _SLIME.Slime;
using UnityEngine;


namespace _SLIME.Boss
{
    public class BossSpawnAttackBehaviour : BossBaseBehaviour
    {
        private static readonly int AttackFinished = Animator.StringToHash("AttackFinished");

        private int _spellCounter;
        private float _timer;
        

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            TotalAttacksPreformed++;
            _spellCounter = 0;
            _timer = 0f;
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            if (_spellCounter >= Data.bossConfigurations.SpawnAttack.spellsToCast)
            {
                animator.SetTrigger(AttackFinished);
            }

            _timer += Time.deltaTime;

            if (_timer >= Data.bossConfigurations.SpawnAttack.spawnInterval)
            {
                SpawnItem();
                _spellCounter++;
                _timer = 0f;
            }
        }
        
        private void SpawnItem()
        {
            Transform leftSpawnPoint = Data.leftSpawnPoint;
            Transform rightSpawnPoint = Data.rightSpawnPoint;
            
            float randomX = Random.Range(leftSpawnPoint.position.x, rightSpawnPoint.position.x);
            float fixedY = leftSpawnPoint.position.y;
            
            GameObject item = Instantiate(Data.bossConfigurations.SpawnAttack.projectilePrefab,
                new Vector2(randomX, fixedY), Quaternion.identity);
            Spell spell = item.GetComponentInChildren<Spell>();
            
            Vector3 target = GetTargetPosition();
            Vector3 direction = (target - item.transform.position).normalized;
            spell.BossSetup(new SpellBossAttributes
            {
                direction = direction,
                moveSpeed = Data.bossConfigurations.SpawnAttack.spellSpeed
            });
        }
        
        private Vector3 GetTargetPosition()
        {
            Vector3 slime1Pos = SlimeData.instance.SideATransform.position;
            Vector3 slime2Pos = SlimeData.instance.SideBTransform.position;

            if (SlimeData.instance.SideBDead) return slime1Pos;
            if (SlimeData.instance.SideADead) return slime2Pos;
            
            Vector3 spawnPos = Data.leftSpawnPoint.position;
            float dist1 = Vector3.Distance(spawnPos, slime1Pos);
            float dist2 = Vector3.Distance(spawnPos, slime2Pos);
            
            return dist1 < dist2 ? slime1Pos : slime2Pos;
        }
    }
}
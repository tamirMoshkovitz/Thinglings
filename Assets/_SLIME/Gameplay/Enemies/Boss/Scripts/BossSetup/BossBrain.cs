using System;
using System.Collections;
using System.Collections.Generic;
using _SLIME.BaseScripts;
using _SLIME.Core.Audio.FMOD.Scripts;
using _SLIME.GameLoop;
using _SLIME.Slime;
using _SLIME.UI;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using EventType = _SLIME.UI.EventType;

public enum BossStates
{
    FarState,
    CloseState,
    LaserState,
    WaterState,
    OutsideState,
}

namespace _SLIME.Boss
{
    public class BossBrain : ProjectMonoBehavior, IHealth
    {
        private static readonly int Die = Animator.StringToHash("Die");
        
        [Header("Camera Setup")] 
        public Camera mainCamera;
        
        [Header("Boss Data Setup")]
        public static BaseBossConfigurations bossConfigurations;

        public FloatingMagic floatingAttributes;
        
        [SerializeField] BaseBossConfigurations firstPhaseConfigurations;
        [SerializeField] BaseBossConfigurations secondPhaseConfigurations;
        [SerializeField] BaseBossConfigurations tunnelPhaseConfigurations;
        
        public GameObject waterStateBrain;
        public GameObject bossCloseColliders;
        public GameObject bossFarColliders;
        public GameObject bossLaserColliders;
        
        [Header("Hands Attack Setup")] 
        public List<GameObject> leftHandSplines;
        public List<GameObject> rightHandSplines;

        [SerializeField] public List<GameObject> specialBottomHands;
        [SerializeField] public List<GameObject> specialTopHands;
        [SerializeField] public PlayerInCenterDetector centerDetector;
        
        [Header("Spawn Setup")]
        [SerializeField] private SpawnDeps spawnDeps;

        public Transform SpawnPoint =>
            CurrentPhase == BossPhaseType.TunnelPhase
                ? spawnDeps.spawnPointTunnelPhase
                : spawnDeps.spawnPointFirstPhases;
        [HideInInspector] public bool slimesConnected = false;
        
        [Header("Health Setup")] 
        public Image bossHealthBar;
        public GameObject bossHealthBarCanvas;
        [HideInInspector] 
        public float currentHealth;

        [Header("Animation Setup")]
        public Animator animator;
        
        [Header("Animator Tunnel Setup")]
        public AnimatorOverrideController tunnelOverrideController;
        
        
        [FormerlySerializedAs("laserAttackGameObject")] [Header("Laser Attack Setup")] 
        public GameObject laserAttackGameObjectPhase1;
        public GameObject laserAttackGameObjectPhase2;
        
        
        [Header("Light House Attack Setup")] 
        public GameObject lightHouseAttackGameObject;


        [Header("Little Bosses Attack Setup")] 
        public GameObject littleBossLeft;
        public GameObject littleBossRight;
        
        [Header("SFX")]
        [SerializeField] private EventReference hitSFX;
        
        public static BossStates BossState = BossStates.FarState;
        private static readonly int CloseHit = Animator.StringToHash("CloseHit");
        private static readonly int FarHit = Animator.StringToHash("FarHit");
        private static readonly int LaserHit = Animator.StringToHash("LaserHit");
        
        public IcicleSpawner spawner;
        
        public static event Action CloseState;
        public static event Action FarState;
        
        

        public StateMachine StateMachine { get; private set; }
        public BossPhaseType CurrentPhase
        {
            get
            {
                var current = StateMachine?.CurrentState;
                if (current == FirstPhaseState) return BossPhaseType.FirstPhase;
                if (current == SecondPhaseState) return BossPhaseType.SecondPhase;
                if (current == TunnelPhaseState) return BossPhaseType.TunnelPhase;
                return BossPhaseType.FirstPhase;
            }
        }
        public State FirstPhaseState { get; private set; }
        public State SecondPhaseState { get; private set; }
        
        public State TunnelPhaseState { get; private set; }
        
        public bool WaterStateActivated { get; set;}
        
        public bool IsTakingDamage { get;  set; }

        public int HitCounter { get; set; }
        public float firstFloatDistance { get; set; }

        private void Start()
        {
            
            StateMachine = new StateMachine(this);
            
            FirstPhaseState = new FirstPhaseState(StateMachine, this, firstPhaseConfigurations);
            SecondPhaseState = new SecondPhaseState(StateMachine, this, secondPhaseConfigurations);
            TunnelPhaseState = new TunnelPhaseState(StateMachine, this, tunnelPhaseConfigurations);

            BossPhaseType savedPhase = BossPhaseType.FirstPhase;
            
            if (BossCheckpointManager.Instance != null)
            {
                savedPhase = BossCheckpointManager.Instance.CurrentSavedPhase;
            }

            switch (savedPhase)
            {

                case BossPhaseType.FirstPhase:
                    bossConfigurations = firstPhaseConfigurations;
                    currentHealth = bossConfigurations.CoreSettings.maxHealth;
                    StateMachine.Initialize(FirstPhaseState);
                    break;
                case BossPhaseType.SecondPhase:
                    bossConfigurations = secondPhaseConfigurations;
                    currentHealth = bossConfigurations.CoreSettings.maxHealth;
                    StateMachine.Initialize(SecondPhaseState);
                    break;
                case BossPhaseType.TunnelPhase:
                    bossConfigurations = tunnelPhaseConfigurations;
                    currentHealth = bossConfigurations.CoreSettings.maxHealth;
                    StateMachine.Initialize(TunnelPhaseState);
                    break;
            }
            if (!mainCamera) mainCamera = Camera.main;
           

            var allBehaviours = animator.GetBehaviours<BossBaseBehaviour>();
            foreach (var behaviour in allBehaviours)
            {
                behaviour.Initialize(this);
            }

            
            if (bossHealthBar) bossHealthBar.fillAmount = currentHealth / firstPhaseConfigurations.CoreSettings.maxHealth;
        }

       
        private void OnEnable(){
            SlimeEvents.SlimeConnected += OnSlimeConnected;
            SlimeEvents.SlimeTears += OnSlimeTears;
            GameEvents.FmodPhaseThree?.Invoke();
        }
        
        private void Update()
        {
            StateMachine.CurrentState.LogicUpdate();
        }
        
        private void OnDisable()
        {
            SlimeEvents.SlimeConnected -= OnSlimeConnected;
            SlimeEvents.SlimeTears -= OnSlimeTears;
        }
        
        private void OnSlimeConnected() => slimesConnected = true;
        private void OnSlimeTears() => slimesConnected = false;

        public void TakeDamage(float damage) 
        {
            SFXPlayer.Play(hitSFX);
            float denominator = bossConfigurations.PhaseSettings.targetHitsToKill * bossConfigurations.PhaseSettings.expectedAvgSpeedOfSpells;
            if (denominator == 0) denominator = 1;

            float finalDamageF = damage * (StateMachine.CurrentState.EnterHealth - bossConfigurations.PhaseSettings.lowerHealthThreshold)
                / denominator;
            
            ApplyDamage(finalDamageF);
        }
        

        // this stupid to separate them, but I did for damage from water is different from spell damage
        public void ApplyDamage(float finalDamage)
        {
            finalDamage = Mathf.RoundToInt(finalDamage);
            currentHealth -= finalDamage;
            if (bossHealthBar) bossHealthBar.fillAmount = currentHealth / firstPhaseConfigurations.CoreSettings.maxHealth;

            if (BossState != BossStates.WaterState && bossHealthBar.gameObject.activeInHierarchy)
            {
                // PopupEventsRenderer.OnRenderPointsAbove(new RenderEvent
                // {
                //     eventType = EventType.BossHealth,
                //     value = -finalDamage,
                //     fatherTransform = null,
                //     position = bossHealthBar.GetComponent<HealthBarTipFollower>().GetTipPosition(),
                //     OnFinish = null
                // });
            }

            GameEvents.EnemyGotBricked?.Invoke();
            if(!IsTakingDamage){ // we dont want twice trigger
                switch (BossState)
                {
                    case BossStates.FarState:
                        animator.SetTrigger(FarHit);
                        break;
                    case BossStates.CloseState:
                        animator.SetTrigger(CloseHit);
                        break;
                    case BossStates.LaserState:
                        animator.SetTrigger(LaserHit);
                        break;
                }
            }
            HitCounter++;
            if (BossState != BossStates.WaterState) IsTakingDamage = true;
        }


        public void BossCloseState()
        {
            if (BossState == BossStates.CloseState) return;
            BossState = BossStates.CloseState;
            bossCloseColliders.SetActive(true);
            bossFarColliders.SetActive(false);
            bossHealthBarCanvas.SetActive(false);
            bossLaserColliders.SetActive(false);
            CloseState?.Invoke();
        }
        
        public void BossFarState()
        {
            if (BossState == BossStates.FarState) return;
            BossState = BossStates.FarState;
            bossCloseColliders.SetActive(false);
            bossFarColliders.SetActive(true);
            bossHealthBarCanvas.SetActive(true);
            bossLaserColliders.SetActive(false);
            FarState?.Invoke();
        }
        
        public void BossOutsideState()
        {
            if (BossState == BossStates.OutsideState) return;
            BossState = BossStates.OutsideState;
            bossCloseColliders.SetActive(false);
            bossFarColliders.SetActive(false);
            bossLaserColliders.SetActive(false);
            bossHealthBarCanvas.SetActive(false);
        }

        public void BossLaserState()
        {
            if (BossState == BossStates.LaserState) return;
            BossState = BossStates.LaserState;
            bossLaserColliders.SetActive(true);
            bossCloseColliders.SetActive(false);
            bossFarColliders.SetActive(false);
            bossHealthBarCanvas.SetActive(false);
            FarState?.Invoke();
        }
        
        public void BossWaterState()
        {
            if (BossState == BossStates.WaterState) return;
            BossState = BossStates.WaterState;
            bossCloseColliders.SetActive(false);
            bossFarColliders.SetActive(false);
            bossLaserColliders.SetActive(false);
            bossHealthBarCanvas.SetActive(false);
        }

        public void SavePhaseCheckpoint(BossPhaseType phaseToSave)
        {
            if (BossCheckpointManager.Instance)
            {
                BossCheckpointManager.Instance.SaveCheckpoint(phaseToSave);
                
                switch (phaseToSave)
                {
                    case BossPhaseType.FirstPhase: bossConfigurations = firstPhaseConfigurations; break;
                    case BossPhaseType.SecondPhase: bossConfigurations = secondPhaseConfigurations; break;
                    case BossPhaseType.TunnelPhase: bossConfigurations = tunnelPhaseConfigurations; break;
                }
            }
        }

        

        
    }
}
using System;
using System.Collections.Generic;
using _SLIME.BaseScripts;
using _SLIME.GameLoop;
using _SLIME.Slime;
using _SLIME.UI;
using UnityEngine;
using UnityEngine.UI;
using EventType = _SLIME.UI.EventType;

public enum BossStates
{
    FarState,
    CloseState,
    LaserState,
    WaterState
}

namespace _SLIME.Boss
{
    public class BossBrain : ProjectMonoBehavior, IHealth
    {
        private static readonly int Die = Animator.StringToHash("Die");
        
        [Header("Camera Setup")] 
        public Camera mainCamera;
        
        [Header("Boss Data Setup")]
        // this is static to make life easier 
        public static BaseBossConfigurations bossConfigurations; 
        
        
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
        [SerializeField] public List<GameObject> specialLeftHandSplines;
        [SerializeField] public List<GameObject> specialRightHandSplines;
        [SerializeField] public PlayerInCenterDetector centerDetector;
        
        [Header("Spawn Setup")]
        public SpawnDeps spawnDeps;
        [HideInInspector] public bool slimesConnected = false;
        
        [Header("Health Setup")] 
        public Image bossHealthBar;
        [HideInInspector] 
        public float currentHealth;

        [Header("Animation Setup")]
        public Animator animator;

        [Header("Laser Attack Setup")] 
        public GameObject laserAttackGameObject;
        
        
        [Header("Light House Attack Setup")] 
        public GameObject lightHouseAttackGameObject;
        
        public static BossStates BossState = BossStates.FarState;
        private static readonly int CloseHit = Animator.StringToHash("CloseHit");
        private static readonly int FarHit = Animator.StringToHash("FarHit");
        private static readonly int LaserHit = Animator.StringToHash("LaserHit");
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
            
            PopupEventsRenderer.OnRenderPointsAbove(new RenderEvent
            {
                eventType = EventType.BossHealth,
                value = -finalDamage,
                fatherTransform = null,
                position = transform.position,
                OnFinish = null
            });
            GameEvents.EnemyGotBricked?.Invoke();
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
            HitCounter++;
            if (BossState != BossStates.WaterState) IsTakingDamage = true;
        }


        public void BossCloseState()
        {
            if (BossState == BossStates.CloseState) return;
            BossState = BossStates.CloseState;
            bossCloseColliders.SetActive(true);
            bossFarColliders.SetActive(false);
            CloseState?.Invoke();
        }
        
        public void BossFarState()
        {
            if (BossState == BossStates.FarState) return;
            BossState = BossStates.FarState;
            bossCloseColliders.SetActive(false);
            bossFarColliders.SetActive(true);
            FarState?.Invoke();
        }

        public void BossLaserState()
        {
            if (BossState == BossStates.LaserState) return;
            BossState = BossStates.LaserState;
            bossLaserColliders.SetActive(true);
            bossCloseColliders.SetActive(false);
            bossFarColliders.SetActive(false);
            FarState?.Invoke();
        }
        
        public void BossWaterState()
        {
            if (BossState == BossStates.WaterState) return;
            BossState = BossStates.WaterState;
            bossCloseColliders.SetActive(false);
            bossFarColliders.SetActive(false);
            bossLaserColliders.SetActive(false);
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
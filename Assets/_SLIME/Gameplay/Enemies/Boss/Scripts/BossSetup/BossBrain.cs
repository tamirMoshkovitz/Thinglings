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
        public BaseBossConfigurations bossConfigurations; 
        
        [SerializeField] BaseBossConfigurations startingPhaseConfigurations;
        [SerializeField] BaseBossConfigurations firstPhaseConfigurations;
        [SerializeField] BaseBossConfigurations secondPhaseConfigurations;
        [SerializeField] BaseBossConfigurations thirdPhaseConfigurations;
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
        public Transform leftSpawnPoint;
        public Transform rightSpawnPoint;
        [HideInInspector] public bool slimesConnected = false;
        
        [Header("Health Setup")] 
        public Image bossHealthBar;
        [HideInInspector] 
        public float currentHealth;

        [Header("Animation Setup")]
        public Animator animator;

        [Header("Laser Attack Setup")] 
        public GameObject laserAttackGameObject;
        
        public static BossStates BossState = BossStates.FarState;
        public static event Action CloseState;
        public static event Action FarState;

        private StateMachine StateMachine { get; set; }
        public State FirstPhaseState { get; private set; }
        public State SecondPhaseState { get; private set; }
        public State ThirdPhaseState { get; private set; }
        public State StartingPhaseState { get; private set; }
        
        public State TunnelPhaseState { get; private set; }
        
        public bool WaterStateActivated { get; set;}

        private void Start()
        {
            StateMachine = new StateMachine();
            
            StartingPhaseState = new StartingPhaseState(StateMachine, this, startingPhaseConfigurations);
            FirstPhaseState = new FirstPhaseState(StateMachine, this, firstPhaseConfigurations);
            SecondPhaseState = new SecondPhaseState(StateMachine, this, secondPhaseConfigurations);
            ThirdPhaseState = new ThirdPhaseState(StateMachine, this, thirdPhaseConfigurations);
            TunnelPhaseState = new TunnelPhaseState(StateMachine, this, tunnelPhaseConfigurations);
            
            BossPhaseType savedPhase = BossPhaseType.Starting;
            
            if (BossCheckpointManager.Instance != null)
            {
                savedPhase = BossCheckpointManager.Instance.CurrentSavedPhase;
            }

            switch (savedPhase)
            {

                case BossPhaseType.FirstPhase:
                    bossConfigurations = firstPhaseConfigurations;
                    StateMachine.Initialize(FirstPhaseState);
                    break;
                case BossPhaseType.SecondPhase:
                    bossConfigurations = secondPhaseConfigurations;
                    StateMachine.Initialize(SecondPhaseState);
                    break;
                case BossPhaseType.ThirdPhase:
                    bossConfigurations = thirdPhaseConfigurations;
                    StateMachine.Initialize(ThirdPhaseState);
                    break;
                case BossPhaseType.TunnelPhase:
                    bossConfigurations = tunnelPhaseConfigurations;
                    StateMachine.Initialize(TunnelPhaseState);
                    break;
                case BossPhaseType.Starting:
                default:
                    bossConfigurations = startingPhaseConfigurations;
                    StateMachine.Initialize(StartingPhaseState);
                    break;
                
            }
            if (!mainCamera) mainCamera = Camera.main;
            
            if (BossCheckpointManager.Instance != null && BossCheckpointManager.Instance.CurrentSavedPhase != BossPhaseType.Starting)
            {
                currentHealth = bossConfigurations.PhaseSettings.upperHealthThreshold;
            }
            else
            {
                currentHealth = bossConfigurations.CoreSettings.maxHealth;
            }

            var allBehaviours = animator.GetBehaviours<BossBaseBehaviour>();
            foreach (var behaviour in allBehaviours)
            {
                behaviour.Initialize(this);
            }
            
            if (bossHealthBar) bossHealthBar.fillAmount = currentHealth / bossConfigurations.CoreSettings.maxHealth;
        }

       

        private void OnEnable(){
            SlimeEvents.SlimeConnected += OnSlimeConnected;
            SlimeEvents.SlimeTears += OnSlimeTears;
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

            float finalDamageF = damage * (bossConfigurations.PhaseSettings.upperHealthThreshold - bossConfigurations.PhaseSettings.lowerHealthThreshold)
                / denominator;
                
            int finalDamage = Mathf.RoundToInt(finalDamageF);
            currentHealth -= finalDamage;


            if (bossHealthBar) bossHealthBar.fillAmount = currentHealth / bossConfigurations.CoreSettings.maxHealth;
            
            PopupEventsRenderer.OnRenderPointsAbove(new RenderEvent
             {
                 eventType = EventType.BossHealth,
                 value = -finalDamage,
                 fatherTransform = null,
                 position = transform.position,
                 OnFinish = null
             });
            GameEvents.EnemyGotBricked?.Invoke();
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
                    case BossPhaseType.ThirdPhase: bossConfigurations = thirdPhaseConfigurations; break;
                    case BossPhaseType.TunnelPhase: bossConfigurations = tunnelPhaseConfigurations; break;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (leftSpawnPoint == null || rightSpawnPoint == null) return;
            Gizmos.color = Color.green;
            Vector3 center = (leftSpawnPoint.position + rightSpawnPoint.position) / 2;
            Vector3 size = new Vector3(Mathf.Abs(rightSpawnPoint.position.x -leftSpawnPoint.position.x), 1f, 1f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
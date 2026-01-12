using System;
using System.Collections.Generic;
using _SLIME.BaseScripts;
using _SLIME.GameLoop;
using _SLIME.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using EventType = _SLIME.UI.EventType;

public enum BossStates
{
    FarState,
    CloseState
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
        
        public GameObject waterStateBrain;
        
        public GameObject bossCloseColliders;
        public GameObject bossFarColliders;
        
        [Header("Hands Attack Setup")] 
        [Tooltip("List of left hand splines")]
        public List<GameObject> leftHandSplines;
        [Tooltip("List of right hand splines")]
        public List<GameObject> rightHandSplines;
        
        [Header("Spawn Setup")]
        public Transform leftSpawnPoint;
        public Transform rightSpawnPoint;
        
        [Header("Health Setup")] 
        public Image bossHealthBar;
        [HideInInspector] 
        public float currentHealth;

        [Header("Animation Setup")] 
        [SerializeField] private Animator animator;

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
            
        public bool WaterStateActivated { get; set;}
        private void Awake()
        {
            StateMachine = new StateMachine();
            StartingPhaseState = new StartingPhaseState(StateMachine, this, startingPhaseConfigurations);
            FirstPhaseState = new FirstPhaseState(StateMachine, this, firstPhaseConfigurations);
            SecondPhaseState = new SecondPhaseState(StateMachine, this, secondPhaseConfigurations);
            ThirdPhaseState = new ThirdPhaseState(StateMachine, this, thirdPhaseConfigurations);
            StateMachine.Initialize(StartingPhaseState);
        }

        private void Start()
        {
            if (!mainCamera) mainCamera = Camera.main;
            currentHealth = bossConfigurations.CoreSettings.maxHealth;

            var allBehaviours = animator.GetBehaviours<BossBaseBehaviour>();
            foreach (var behaviour in allBehaviours)
            {
                behaviour.Initialize(this);
            }
        }
        
        private void Update()
        {
            StateMachine.CurrentState.LogicUpdate();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            if (bossHealthBar) bossHealthBar.fillAmount = currentHealth / bossConfigurations.CoreSettings.maxHealth;
            PopupEventsRenderer.OnRenderPointsAbove(new RenderEvent
             {
                 eventType = EventType.BossHealth,
                 value = -damage,
                 fatherTransform = null,
                 position = transform.position,
                 OnFinish = null
             });
            GameEvents.EnemyGotBricked?.Invoke();
            
            // if (currentHealth <= 0)
            //     GetComponent<Animator>().SetTrigger(Die);
        }
        
        //TODO: Reset with the game (doesn't go back to the base far state on reset)
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
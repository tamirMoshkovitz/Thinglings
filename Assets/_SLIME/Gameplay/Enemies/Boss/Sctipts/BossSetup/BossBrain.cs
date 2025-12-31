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
        
        [FormerlySerializedAs("bossSettings")] [Header("Boss Data Setup")]
        public BaseBossConfigurations bossConfigurations;
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
        
        public static BossStates _bossState = BossStates.FarState;
        
        public static event Action CloseState;
        public static event Action FarState;
    

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

        public void BossCloseState()
        {
            if (_bossState == BossStates.CloseState) return;
            _bossState = BossStates.CloseState;
            bossCloseColliders.SetActive(true);
            bossFarColliders.SetActive(false);
            CloseState?.Invoke();
        }
        
        public void BossFarState()
        {
            if (_bossState == BossStates.FarState) return;
            _bossState = BossStates.FarState;
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
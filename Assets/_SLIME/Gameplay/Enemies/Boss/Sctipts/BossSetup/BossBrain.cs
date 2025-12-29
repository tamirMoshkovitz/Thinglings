using System.Collections.Generic;
using _SLIME.BaseScripts;
using _SLIME.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using EventType = _SLIME.UI.EventType;


namespace _SLIME.Boss
{
    public class BossBrain : ProjectMonoBehavior, IHealth
    {
        private static readonly int Die = Animator.StringToHash("Die");
        
        [Header("Camera Setup")] 
        public Camera mainCamera;
        
        [FormerlySerializedAs("bossSettings")] [Header("Boss Data Setup")]
        public BaseBossConfigurations bossConfigurations;
        public Collider2D bossCloseCollider;
        public Collider2D bossFarCollider;
        
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

        void Start()
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
            
            // if (currentHealth <= 0)
            //     GetComponent<Animator>().SetTrigger(Die);
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
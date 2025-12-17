using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    // TODO: Move to an independent Scriptable Object for the boss 

    #region Boss Settings - Move to scriptable object
    
    private static readonly int Die = Animator.StringToHash("Die");
    
    [Header("Core References")]
    public Transform bossRoot;       
    public Collider2D bossCollider;
    public Camera mainCamera;      

    [Header("Movement Path")]
    public Transform startPoint;
    public Transform endPoint;
    
    [Header("Spline Hand Attack Setup")]
    public List<GameObject> leftHandSplines; 
    public List<GameObject> rightHandSplines;
    
    [Tooltip("How long the hand takes to complete the path")]
    public float handAttackDuration = 3.0f; 
    
    [Tooltip("Wait time between individual smashes")]
    public float handCooldown = 0.5f;

    [Header("Movement Settings")]
    public float moveSpeed = 10f;       
    public float hoverStrength = 0.5f; 
    public float hoverDuration = 1f;   

    [Header("Projectiles")]
    public GameObject fallingItemPrefab; 
    public Transform spawnAreaLeft;
    public Transform spawnAreaRight;

    [Header("Health")]
    public Image bossHealthBar;
    public float maxHealth = 100f;
    [HideInInspector] public float currentHealth;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    
    [Header("Laser Array Attack Setup")]
    public LaserAttackLogic laserArrayScript; 
    public float laserRotationSpeed = 50f;
    public float laserStaggerDelay = 0.2f;
    public float laserActiveDuration = 3.0f;
    public LaserAttackLogic.AnimationProfile laserGrowProfile;
    public LaserAttackLogic.AnimationProfile laserDissolveProfile;
    
    #endregion
    
    // TODO: Move to an independent Scriptable Object for the boss 
    
    void Start()
    {
        if (!mainCamera) mainCamera = Camera.main;
        currentHealth = maxHealth;
        
        var allBehaviours = animator.GetBehaviours<BossBaseBehaviour>();
        foreach (var behaviour in allBehaviours)
        {
            behaviour.Initialize(this);
        }
    }
    
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if(bossHealthBar) bossHealthBar.fillAmount = currentHealth / maxHealth;
        // if (currentHealth <= 0)
        //     GetComponent<Animator>().SetTrigger(Die);
    }

    private void OnDrawGizmos()
    {
        if (startPoint == null || endPoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startPoint.position, endPoint.position);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(startPoint.position, 1f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(endPoint.position, 1f);

        if (spawnAreaLeft == null || spawnAreaRight == null) return;
        Gizmos.color = Color.green;
        Vector3 center = (spawnAreaLeft.position + spawnAreaRight.position) / 2;
        Vector3 size = new Vector3(Mathf.Abs(spawnAreaRight.position.x - spawnAreaLeft.position.x), 1f, 1f);
        Gizmos.DrawWireCube(center, size);
    }
}
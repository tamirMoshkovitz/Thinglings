using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    [Header("Core References")]
    public Transform bossRoot;       
    public Collider2D bossCollider;
    public Camera mainCamera;      

    [Header("Movement Path")]
    public Transform startPoint; // Hiding Spot (Guaranteed)
    public Transform endPoint;   // Attack Spot (Guaranteed)

    [Header("Player Targets")]
    public Transform playerLeftSlime;  
    public Transform playerRightSlime; 

    [Header("The Hands")]
    public Transform leftHand;  
    public Transform rightHand; 

    [Header("Hand Smash Settings")]
    public float prepareHeight = 8f;   
    public float floorHeight = -2f;    
    public float hoverHeight = 5f;     
    
    [Header("Timing")]
    public float riseDuration = 0.5f;
    public float trackDuration = 1.0f; 
    public float smashDuration = 0.2f;
    public float recoverDuration = 1.0f;
    public float trackSpeed = 10f;

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
    
    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        currentHealth = maxHealth;

        Animator anim = GetComponent<Animator>();
        var allBehaviours = anim.GetBehaviours<BossBaseBehaviour>();
        foreach (var behaviour in allBehaviours)
        {
            behaviour.Initialize(this);
        }
    }
    
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if(bossHealthBar != null) bossHealthBar.fillAmount = currentHealth / maxHealth;
        if (currentHealth <= 0) GetComponent<Animator>().SetTrigger("Die");
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

        if (spawnAreaLeft != null && spawnAreaRight != null)
        {
            Gizmos.color = Color.green;
            Vector3 center = (spawnAreaLeft.position + spawnAreaRight.position) / 2;
            Vector3 size = new Vector3(Mathf.Abs(spawnAreaRight.position.x - spawnAreaLeft.position.x), 1f, 1f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
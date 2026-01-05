using _SLIME.BaseScripts;
using _SLIME.Projectiles;
using UnityEngine;

namespace _SLIME.Dev_Scripts
{
    /// <summary>
    /// Simple test script to verify Bullet and BulletMonoPool functionality.
    /// Tests pool creation, bullet activation, movement, and return to pool.
    /// </summary>
    public class BulletTester : ProjectMonoBehavior
    {
        [Header("Test Configuration")]
        [Tooltip("The bullet pool to test.")]
        [SerializeField] private BulletMonoPool bulletPool;
        
        [Tooltip("Target transform for bullets to aim at.")]
        [SerializeField] private Transform testTarget;
        
        [Tooltip("Starting position for test bullets.")]
        [SerializeField] private Vector2 startPosition = new Vector2(0, 0);
        
        [Tooltip("Bullet movementSpeed for testing.")]
        [SerializeField] private float testSpeed = 5f;
        
        [Tooltip("Bullet buffer distance.")]
        [SerializeField] private float testBuffer = 0.5f;
        
        [Tooltip("Turn smoothness (0-1).")]
        [SerializeField] private float testTurnSmoothness = 0.1f;
        
        [Tooltip("Bullet damage amount.")]
        [SerializeField] private float testDamage = 10f;
        
        [Header("Test Controls")]
        [Tooltip("Interval between test bullet spawns (seconds).")]
        [SerializeField] private float spawnInterval = 1f;
        
        [Tooltip("Automatically spawn test bullets.")]
        [SerializeField] private bool autoSpawn = false;
        
        private Bullet _currentTestBullet;
        private Vector2 _lastBulletPosition;
        private int _bulletsSpawned = 0;
        private int _bulletsReturned = 0;
        private float _lastSpawnTime = 0f;

        private void Start()
        {
            if (bulletPool == null)
            {
                Debug.LogError("[BulletTester] BulletPool is not assigned!");
                return;
            }
            
            if (testTarget == null)
            {
                Debug.LogWarning("[BulletTester] Test target is not assigned. Creating a test target at (5, 5).");
                GameObject targetObj = new GameObject("TestTarget");
                targetObj.transform.position = new Vector3(5, 5, 0);
                testTarget = targetObj.transform;
            }
            
            _lastSpawnTime = Time.time;
            Debug.Log("[BulletTester] Test initialized. Press Space to spawn a test bullet.");
        }

        private void OnEnable()
        {
            if (autoSpawn)
            {
                InvokeRepeating(nameof(SpawnTestBullet), spawnInterval, spawnInterval);
            }
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(SpawnTestBullet));
        }

        private void Update()
        {
            // Manual spawn with Space key
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SpawnTestBullet();
            }
            
            // Auto spawn with timer (backup method)
            if (autoSpawn && Time.time - _lastSpawnTime >= spawnInterval)
            {
                SpawnTestBullet();
                _lastSpawnTime = Time.time;
            }
            
            // Track bullet movement
            if (_currentTestBullet != null && _currentTestBullet.gameObject.activeSelf)
            {
                Vector2 currentPos = _currentTestBullet.transform.position;
                float distanceMoved = Vector2.Distance(currentPos, _lastBulletPosition);
                
                if (distanceMoved > 0.01f)
                {
                    Debug.Log($"[BulletTester] Bullet is moving. Distance: {Vector2.Distance(currentPos, startPosition):F2}");
                }
                
                _lastBulletPosition = currentPos;
            }
        }

        /// <summary>
        /// Spawns a test bullet from the pool and activates it.
        /// </summary>
        [ContextMenu("Spawn Test Bullet")]
        public void SpawnTestBullet()
        {
            if (bulletPool == null)
            {
                Debug.LogError("[BulletTester] Cannot spawn: BulletPool is null!");
                return;
            }
            
            if (testTarget == null)
            {
                Debug.LogError("[BulletTester] Cannot spawn: Test target is null!");
                return;
            }
            
            // Get bullet from pool
            Bullet bullet = bulletPool.Get();
            if (bullet == null)
            {
                Debug.LogError("[BulletTester] Failed to get bullet from pool!");
                return;
            }
            
            // Create initialization data
            BulletInitData initData = new BulletInitData(
                target: testTarget,
                startPosition: startPosition,
                speed: testSpeed,
                buffer: testBuffer,
                pool: bulletPool,
                turnSmoothness: testTurnSmoothness,
                damage: testDamage
            );
            
            // Activate the bullet
            bullet.Activate(initData);
            
            _currentTestBullet = bullet;
            _lastBulletPosition = bullet.transform.position;
            _bulletsSpawned++;
            _lastSpawnTime = Time.time;
            
            Debug.Log($"[BulletTester] Spawned bullet #{_bulletsSpawned} at {startPosition}. Target: {testTarget.position}");
        }

        /// <summary>
        /// Manually returns the current test bullet to the pool.
        /// </summary>
        [ContextMenu("Return Bullet to Pool")]
        public void ReturnBulletToPool()
        {
            if (_currentTestBullet != null && _currentTestBullet.gameObject.activeSelf)
            {
                bulletPool.Return(_currentTestBullet);
                _bulletsReturned++;
                Debug.Log($"[BulletTester] Manually returned bullet to pool. Total returned: {_bulletsReturned}");
                _currentTestBullet = null;
            }
            else
            {
                Debug.LogWarning("[BulletTester] No active bullet to return.");
            }
        }

        /// <summary>
        /// Prints test statistics.
        /// </summary>
        [ContextMenu("Print Test Stats")]
        public void PrintTestStats()
        {
            Debug.Log($"[BulletTester] Statistics:\n" +
                     $"  Bullets Spawned: {_bulletsSpawned}\n" +
                     $"  Bullets Returned: {_bulletsReturned}\n" +
                     $"  Active Bullets: {_bulletsSpawned - _bulletsReturned}");
        }

        private void OnGUI()
        {
            // Simple on-screen test controls
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Bullet Tester", GUI.skin.box);
            GUILayout.Label($"Spawned: {_bulletsSpawned} | Returned: {_bulletsReturned}");
            
            if (GUILayout.Button("Spawn Bullet (or press Space)"))
            {
                SpawnTestBullet();
            }
            
            if (GUILayout.Button("Return Bullet to Pool"))
            {
                ReturnBulletToPool();
            }
            
            bool newAutoSpawn = GUILayout.Toggle(autoSpawn, "Auto Spawn");
            if (newAutoSpawn != autoSpawn)
            {
                autoSpawn = newAutoSpawn;
                if (autoSpawn)
                {
                    CancelInvoke(nameof(SpawnTestBullet));
                    InvokeRepeating(nameof(SpawnTestBullet), spawnInterval, spawnInterval);
                }
                else
                {
                    CancelInvoke(nameof(SpawnTestBullet));
                }
            }
            
            GUILayout.EndArea();
        }
    }
}


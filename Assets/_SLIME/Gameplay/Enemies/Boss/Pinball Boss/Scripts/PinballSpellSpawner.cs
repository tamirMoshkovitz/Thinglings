using _SLIME.BaseScripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace _SLIME.Gameplay.Enemies.Boss.Pinball_Boss.Scripts
{
    public class PinballSpellSpawner: ProjectMonoBehavior
    {
        [SerializeField] private GameObject pinballPrefab;
        [SerializeField] private float spawnInterval = 5f;
        [SerializeField] private float spellSpeed = 5f;
        [SerializeField] private Transform slimeRight;
        [SerializeField] private Transform slimeLeft;
        
        private float _timer;
        
        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= spawnInterval)
            {
                _timer = 0f;
                Vector3 spawnPosition = transform.position;
                SpawnPinball(spawnPosition);
            }
        }
        
        private void SpawnPinball(Vector3 spawnPosition)
        {
            GameObject spell = Instantiate(pinballPrefab, spawnPosition, Quaternion.identity);
            Vector3 spawnTarget = (slimeRight.position + slimeLeft.position) / 2f;
            Vector2 direction = (spawnTarget - spawnPosition).normalized;
            Rigidbody2D rb = spell.GetComponent<Rigidbody2D>();
            rb.linearVelocity = direction * spellSpeed;
        }
    }
}
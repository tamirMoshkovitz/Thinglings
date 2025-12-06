using Audio;
using UnityEngine;

namespace _SLIME.Dev_Scripts
{
    public class CubeSpawner : ProjectMonoBehavior
    {
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private GameObject cubePrefab;
        
        private void OnEnable()
        {
            InvokeRepeating(nameof(SpawnCube), 1f, spawnInterval);
        }
        
        private void OnDisable()
        {
            CancelInvoke(nameof(SpawnCube));
        }
        
        private void SpawnCube()
        {
            Vector3 spawnPosition = new Vector3(Random.Range(-8f, 8f), 6f, 0f);
            Instantiate(cubePrefab, spawnPosition, Quaternion.identity);
        }
    }
}
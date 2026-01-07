using System;
using System.Collections.Specialized;
using _SLIME.BaseScripts;
using _SLIME.GameLoop;
using _SLIME.Slime;
using UnityEngine;
using UnityEngine.Serialization;

namespace _SLIME.Gameplay.Enemies.Boss.Pinball_Boss.Scripts
{
    public class PinballSpellSpawner: ProjectMonoBehavior
    {
        [SerializeField] private GameObject notHarmfulPinballPrefab;
        [SerializeField] private GameObject harmfulPinballPrefab;
        [SerializeField] private GameObject classicSpellPrefab;
        [SerializeField] private  SlimeBrain slimeBrain;
        [SerializeField] private float spawnInterval = 5f;
        [SerializeField] private float spellSpeed = 5f;
        [SerializeField] private Transform slimeRight;
        [SerializeField] private Transform slimeLeft;

        private int _brokenIcicles = 0;
        private float _timer;

        private GameObject PinballPrefab
        {
            get
            {
                if (_brokenIcicles >= 2)
                {
                    return classicSpellPrefab;
                }
                if (_brokenIcicles >= 1)
                {
                    return harmfulPinballPrefab;
                }
                return notHarmfulPinballPrefab;
            }
        }
        
        private bool IsOneSlimeDead => slimeBrain?.Data.OneSlimeDead ?? false;

        private void OnEnable()
        {
            GameEvents.IcicleCrumbled += OnIcicleCrumble;
        }
        private void OnDisable()
        {
            GameEvents.IcicleCrumbled -= OnIcicleCrumble;
        }
        
        private void Update()
        {
            if (IsOneSlimeDead) // Stop spawning if one slime is dead + reset timer for delay after revival
            {
                _timer = 0f;
            }
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
            GameObject spell = Instantiate(PinballPrefab, spawnPosition, Quaternion.identity);
            Vector3 spawnTarget = (slimeRight.position + slimeLeft.position) / 2f;
            Vector2 direction = (spawnTarget - spawnPosition).normalized;
            Rigidbody2D rb = spell.GetComponent<Rigidbody2D>();
            rb.linearVelocity = direction * spellSpeed;
        }
        
        private void OnIcicleCrumble()
        {
            _brokenIcicles++;
        }
    }
}
using UnityEngine;


namespace _SLIME.Boss
{
    public class BossSpellsAttackBehaviour : BossBaseBehaviour
    {
        [Header("Settings")] public int spellsToCast = 5;
        public float spawnInterval = 0.5f;

        private int _spellCounter;
        private float _timer;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            _spellCounter = 0;
            _timer = 0f;
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            if (_spellCounter >= spellsToCast)
            {
                animator.SetTrigger("Hide");
            }

            _timer += Time.deltaTime;

            if (_timer >= spawnInterval)
            {
                SpawnItem();
                _spellCounter++;
                _timer = 0f;
            }
        }

        private void SpawnItem()
        {
            float randomX = Random.Range(data.spawnAreaLeft.position.x, data.spawnAreaRight.position.x);
            float fixedY = data.spawnAreaLeft.position.y;

            GameObject item = Instantiate(data.fallingItemPrefab, new Vector2(randomX, fixedY), Quaternion.identity);
            Destroy(item, 5f);
        }
    }
}
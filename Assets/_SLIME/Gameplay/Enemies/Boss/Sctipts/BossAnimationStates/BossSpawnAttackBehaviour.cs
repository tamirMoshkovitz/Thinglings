using UnityEngine;


namespace _SLIME.Boss
{
    public class BossSpawnAttackBehaviour : BossBaseBehaviour
    {
        private static readonly int AttackFinished = Animator.StringToHash("AttackFinished");

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

            if (_spellCounter >= Data.bossSettings.SpawnAttack.spellsToCast)
            {
                animator.SetTrigger(AttackFinished);
            }

            _timer += Time.deltaTime;

            if (_timer >= Data.bossSettings.SpawnAttack.spawnInterval)
            {
                SpawnItem();
                _spellCounter++;
                _timer = 0f;
            }
        }
        
        // Todo: change to fit the spawner so it wont be heavy on performance
        private void SpawnItem()
        {
            Transform leftSpawnPoint = Data.leftSpawnPoint;
            Transform rightSpawnPoint = Data.rightSpawnPoint;
            
            float randomX = Random.Range(leftSpawnPoint.position.x, rightSpawnPoint.position.x);
            float fixedY = leftSpawnPoint.position.y;
            
            GameObject item = Instantiate(Data.bossSettings.SpawnAttack.projectilePrefab, new Vector2(randomX, fixedY), Quaternion.identity);
            Destroy(item, Data.bossSettings.SpawnAttack.spellLifeTime);
        }
    }
}
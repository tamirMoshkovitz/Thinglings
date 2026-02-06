using UnityEngine;
using Random = UnityEngine.Random;

namespace _SLIME.Boss
{
    public class LittleBossesPreformBehaviour : BossBaseBehaviour
    {
        private static readonly int AttackFinished = Animator.StringToHash("AttackFinished");

        private OneSpellShotLogic _oneSpellShotLogic;
        private float _timer;
        private float _currentDelay;

        public override void Initialize(BossBrain brain)
        {
            base.Initialize(brain);
            _oneSpellShotLogic = new OneSpellShotLogic(
                BossBrain.bossConfigurations.SpawnAttack.projectilePrefab,
                BossBrain.bossConfigurations.SpawnAttack.projectileBeforeSpawnPrefab,
                Data
            );
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            Data.BossFarState();
            Data.littleBossLeft.SetActive(true);
            Data.littleBossRight.SetActive(true);
            _timer = 0f;
            _currentDelay = 0f;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!Data.littleBossLeft.activeInHierarchy && !Data.littleBossRight.activeInHierarchy)
            {
                animator.SetTrigger(AttackFinished);
                return;
            }

            if (_oneSpellShotLogic.IsActive)
            {
                _oneSpellShotLogic.UpdateAttack();
                return;
            }

            if (Data.IsTakingDamage) return;
            if (Data.WaterStateActivated )
            {

                return;
            }

            _timer += Time.deltaTime;
            if (_timer >= _currentDelay)
            {
                _oneSpellShotLogic.Attack(BossBrain.bossConfigurations.SpawnAttack.spellSettings);
                _timer = 0f;
                var delayRange = BossBrain.bossConfigurations.SpawnAttack.spawnSettings.delayBetweenAttacks;
                _currentDelay = Random.Range(delayRange.x, delayRange.y);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
            _oneSpellShotLogic.Reset();
        }
    }
}
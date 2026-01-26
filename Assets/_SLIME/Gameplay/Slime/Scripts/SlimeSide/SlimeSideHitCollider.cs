using _SLIME.BaseScripts;

namespace _SLIME.Slime
{
    public class SlimeSideHitCollider: ProjectMonoBehavior, IHealth
    {
        private SlimeSideHealth _slimeSideHealth;
        
        public void Init(SlimeSideHealth slimeSideHealth)
        {
            _slimeSideHealth = slimeSideHealth;
        }
        public void TakeDamage(float damage = 0F)
        {
            _slimeSideHealth.TakeDamage(damage);
        }
    }
}
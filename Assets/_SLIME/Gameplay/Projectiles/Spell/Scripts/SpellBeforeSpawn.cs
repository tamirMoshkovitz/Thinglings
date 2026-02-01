using _SLIME.BaseScripts;
using UnityEngine;

namespace _SLIME.Projectiles
{
    public class SpellBeforeSpawn: ProjectMonoBehavior
    {
        [SerializeField] public SpellBeforeSpawnComp comp;
        
        
        public bool GetState()
        {    
            AnimatorStateInfo stateInfo = comp.animator.GetCurrentAnimatorStateInfo(0);
            return !stateInfo.IsName("intro spell");
        }

        public Vector3 GetSpawnPoint()
        {
            return comp.spawnPoint.position;
        }
    }
}
using _SLIME.BaseScripts;
using UnityEngine;

namespace _SLIME.Scenes.Ending.Scripts
{
    public class TriggerEndAfterTransition: ProjectMonoBehavior
    {
        [SerializeField] private GameObject end;

        public void ActivateEnd()
        {
            end.SetActive(true);
        }
    }
}
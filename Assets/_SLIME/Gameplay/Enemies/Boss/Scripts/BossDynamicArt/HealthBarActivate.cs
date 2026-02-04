using _SLIME.BaseScripts;
using UnityEngine;

namespace _SLIME.Boss
{
    public class HealthBarActivate: ProjectMonoBehavior
    {
        public GameObject bossHealthBarCanvas;
        public void EnableHealthBar()
        {
            bossHealthBarCanvas.SetActive(true);
        }
        
        public void DisableHealthBar()
        {
            bossHealthBarCanvas.SetActive(false);
        }
    }
}
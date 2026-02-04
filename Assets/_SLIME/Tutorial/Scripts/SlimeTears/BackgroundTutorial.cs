using _SLIME.BaseScripts;
using UnityEngine;

namespace _SLIME.Tutorial
{
    public class BackgroundTutorial: ProjectMonoBehavior
    {
        [SerializeField] private GameObject background;
        [SerializeField] private GameObject boss;
        public void OnTurnBackgroundOn()
        {
            background.SetActive(true);
            boss.SetActive(true);
        }
    }
}
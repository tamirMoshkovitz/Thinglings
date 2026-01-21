using _SLIME.BaseScripts;
using UnityEngine;

namespace _SLIME.Tutorial
{
    public class BackgroundTutorial: ProjectMonoBehavior
    {
        [SerializeField] private GameObject background;
        public void OnTurnBackgroundOn()
        {
            background.SetActive(true);
        }
    }
}
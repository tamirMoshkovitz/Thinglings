using _SLIME.BaseScripts;
using _SLIME.GameLoop;
using UnityEngine;

namespace _SLIME.Core.MenuSettings.Scripts
{
    public class MenuController: ProjectMonoBehavior
    {
        [Header("UI References")]
        [SerializeField] private GameObject _optionsMenuPanel; 
        
        public static bool IsGamePaused = false;

        public void OnOptionsButtonPressed()
        {
            if (IsGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        private void PauseGame()
        {
            _optionsMenuPanel.SetActive(true);
            Time.timeScale = 0f; 
            IsGamePaused = true;
        
        }

        private void ResumeGame()
        {
            _optionsMenuPanel.SetActive(false); 
            Time.timeScale = 1f; 
            IsGamePaused = false;
            
        }

        public void QuitGame()
        {
            Debug.Log("Quitting Game...");
            Application.Quit();
        }
        
        public void LoadMainMenu()
        {
            Time.timeScale = 1f; 
            IsGamePaused = false;
            SceneLoader.LoadScene(SceneType.StartScene); 
        }
    }
}
using System.Collections;
using _SLIME.BaseScripts;
using _SLIME.Core.ControllerRumble.Scripts;
using _SLIME.GameLoop;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _SLIME.Core.MenuSettings.Scripts
{
    public enum GameMode
    {
        Easy,
        Medium,
        Hard
    }
    
    public enum GameTime
    {
        Start,
        Game
    }
    
    public enum Controller
    {
        PS4,
        XBOX,
        PS5
    }
    public class MenuController: ProjectMonoBehavior
    {
        [Header("UI References")]
        [SerializeField] private GameObject _optionsMenuPanel;
        [SerializeField] private GameObject gameVolume;
        [SerializeField] private GameObject gameMenuPanel;
        [SerializeField] private GameObject startMenuPanel;
        [SerializeField] private GameObject startVolume;
        public static GameMode gameMode = GameMode.Easy;
        public static Controller controller = Controller.PS4;
        [SerializeField] private EventSystem eventSystem;
        
        public static GameTime gameTime = GameTime.Game;
        private GameTime _lastGameTime;
        public static bool IsGamePaused = false;


        private FMOD.Studio.VCA _masterVca;
        private FMOD.Studio.Bus _sfxBus;

        void Awake()
        {
            _masterVca = FMODUnity.RuntimeManager.GetVCA("vca:/Master");
            _sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
            _lastGameTime = gameTime;
        }
        
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
            GamepadWrapper.ResetHaptics();
            SetMusicPause(true);
        }

        private void ResumeGame()
        {
            _optionsMenuPanel.SetActive(false); 
            Time.timeScale = 1f; 
            IsGamePaused = false;
            SetMusicPause(false);
        }

        private void SetMusicPause(bool pause)
        {
            _sfxBus.setPaused(pause);
        }

      
        
        public void LoadMainMenu()
        {
            Time.timeScale = 1f; 
            _optionsMenuPanel.SetActive(false); 
            IsGamePaused = false;
            GameEvents.ResetGame?.Invoke();
            SceneLoader.LoadScene(SceneType.StartScene); 
        }
        
        
        public void ChangeVolume(float value)
        {
            _masterVca.setVolume(value);
            gameVolume.GetComponentInChildren<Slider>().value = value;
            startVolume.GetComponentInChildren<Slider>().value = value;
        }
        
        
        void Update()
        {
            if(gameTime == _lastGameTime) return;
            _lastGameTime = gameTime;
            gameMenuPanel.SetActive(gameTime == GameTime.Game);
            startMenuPanel.SetActive(gameTime == GameTime.Start);
            eventSystem.SetSelectedGameObject(
                gameTime == GameTime.Game ? gameVolume : startVolume);
        }
        
       
    }

    
}
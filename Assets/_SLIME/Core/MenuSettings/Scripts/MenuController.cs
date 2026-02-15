using System.Collections;
using _SLIME.BaseScripts;
using _SLIME.Core.ControllerRumble.Scripts;
using _SLIME.GameLoop;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
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
        
        
        public static GameTime gameTime = GameTime.Start;
        private GameTime _lastGameTime;
        public static bool IsGamePaused = false;


        private FMOD.Studio.VCA _masterVca;
        private FMOD.Studio.Bus _sfxBus;

        void Awake()
        {
            _masterVca = FMODUnity.RuntimeManager.GetVCA("vca:/Master");
            _sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
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
                UpdatePanelsForCurrentGameTimeIfNeeded();
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

        public void ExitGame()
        {
            Application.Quit();
        }
        
        
        private void UpdatePanelsForCurrentGameTimeIfNeeded()
        {
          
            bool isGame = (gameTime == GameTime.Game);
            gameMenuPanel.SetActive(isGame);
            startMenuPanel.SetActive(!isGame);

            GameObject targetObj = isGame ? gameVolume : startVolume;
            StartCoroutine(SelectButtonSafely(targetObj));
        }
        
        // פונקציית עזר לביצוע הבחירה בצורה בטוחה
        IEnumerator SelectButtonSafely(GameObject btn)
        {
            // חובה: קודם כל מנקים את הבחירה הנוכחית!
            // זה מכריח את יוניטי "להתאפס" ומפחית באגים ב-Build
            EventSystem.current.SetSelectedGameObject(null);
            yield return null;
            
            var inputModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
            
            inputModule.enabled = false;
            while (inputModule.enabled)
            {
                yield return null; // חובה לחכות כדי שהכיבוי ייתפס
            }
            inputModule.enabled = true;
            while (inputModule.enabled == false)
            {
                yield return null; // חובה לחכות כדי שהכיבוי ייתפס
            }
            
            yield return null; // חובה לחכות כדי שההדלקה תיתפס
        
        
            _optionsMenuPanel.SetActive(true);
            // מחכים פריים אחד כדי שהפאנל יסיים להיפתח
            while (_optionsMenuPanel.activeInHierarchy == false)
            { 
                yield return null;
            }
            if (btn != null && btn.activeInHierarchy)
            {
                Debug.Log(btn.name);
                btn.GetComponent<Selectable>().Select(); 
        
                // ליתר ביטחון, מוודאים שזה נתפס גם במערכת הראשית
                btn.GetComponent<Selectable>().OnSelect(null);
            }
            
            
            
            
        }
        
       
    }

    
}
using UnityEngine;
using UnityEngine.UI;
namespace _SLIME.Core.MenuSettings.Scripts
{
   

    public class GameModeSelector : MonoBehaviour
    {
        public GameMode modeToSet; 

      
        public void UpdateGameMode(bool isOn)
        {
          
            if (isOn)
            {
                MenuController.gameMode = modeToSet;
               
            }
        }
    }
}
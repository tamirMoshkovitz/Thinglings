using UnityEngine;
using UnityEngine.UI;
namespace _SLIME.Core.MenuSettings.Scripts
{
   

    public class ControllerSelector : MonoBehaviour
    {
        public Controller modeToSet; 

      
        public void UpdateController(bool isOn)
        {
          
            if (isOn)
            {
                MenuController.controller = modeToSet;
               
            }
        }
    }
}
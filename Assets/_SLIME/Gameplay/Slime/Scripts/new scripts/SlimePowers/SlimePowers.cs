
using _SLIME.Gameplay.Slime.Scripts.new_scripts;
using _SLIME.Gameplay.Slime.Scripts.new_scripts.SlimePowers;
using UnityEngine;

namespace _SLIME.Gameplay.Slime.SlimePowers
{
    
    [System.Serializable]
    public struct PowerComponents
    {
        public TriggerSensor connectionsTriggerSensor;
    }
    public class SlimePowers
    {
        private readonly SlimeConfiguration _slimeConfig;
        private readonly ISlimePower _trampolinePower;
        private readonly PowerComponents _powerComponents;

        public SlimePowers(SlimeConfiguration slimeConfiguration, PowerComponents powerComponents)
        {
            _slimeConfig = slimeConfiguration;
            _trampolinePower =
                new TrampolinePower(
                    slimeConfiguration.TrampolinePowerSettings);
            _powerComponents = powerComponents;
            _powerComponents.connectionsTriggerSensor.OnTriggerEntered += 
                _trampolinePower.Activate;
        }
        
        

        public void Update()
        {
            _trampolinePower.Update();
        }
    }
    
    
    
}
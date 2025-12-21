
using _SLIME.BaseScripts;
using UnityEngine;

namespace _SLIME.Slime
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

        public SlimePowers(SlimeConfiguration slimeConfiguration, PowerComponents powerComponents, SlimeData slimeData)
        {
            _slimeConfig = slimeConfiguration;
            _trampolinePower =
                new TrampolinePower(slimeConfiguration.TrampolinePowerSettings, slimeData);
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
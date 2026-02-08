
using System;
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
        private readonly ISlimePower _sparkPower;
        private readonly PowerComponents _powerComponents;
        private readonly SlimeData _slimeData;

        public SlimePowers(SlimeConfiguration slimeConfiguration, PowerComponents powerComponents, SlimeData slimeData,
            SparkPowerDep sparkPowerDep)
        {
            _slimeConfig = slimeConfiguration;
            _slimeData = slimeData;
            _trampolinePower =
                new TrampolinePower(slimeConfiguration.TrampolinePowerSettings, slimeData);
            _sparkPower =
                new SparkPower(slimeConfiguration.SparkPowerSettings, sparkPowerDep, slimeData);
            _powerComponents = powerComponents;
            _powerComponents.connectionsTriggerSensor.OnTriggerEntered += 
                _trampolinePower.Activate;
            // SlimeEvents.SlimeTears += CheckSpark;
        }

        private void CheckSpark()
        {
            if (_slimeData.OneSlimeDead) return;
            _sparkPower.Activate(Vector2.zero, null); // Parm' are not relevant
        }


        public void Update()
        {
            _trampolinePower.Update();
            _sparkPower.Update();
        }

        public void OnEnable()
        {
           
        }

        public void OnDisable()
        {
            _powerComponents.connectionsTriggerSensor.OnTriggerEntered -= 
                _trampolinePower.Activate;
            SlimeEvents.SlimeTears -= CheckSpark;
        }
    }
    
    
    
}

using FMOD;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using Debug = UnityEngine.Debug;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class ControlledSfx : MonoBehaviour
{
    [SerializeField] private EventReference eventRef;

    private EventInstance instance;
    public void Play()
    {
        if (!instance.isValid())
        {
            instance = RuntimeManager.CreateInstance(eventRef);
        }
        instance.start();
    }

    public RESULT SetParameter(string parameter, float value)
    {
        if (instance.isValid())
        {
            return instance.setParameterByName(parameter, value);
        }
        return RESULT.ERR_INVALID_PARAM;
    }

    public void Stop(bool fadeOut = true)
    {
        if (instance.isValid())
            instance.stop(fadeOut ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
    }

    void OnDestroy()
    {
        if (instance.isValid());
        {
            instance.stop(STOP_MODE.IMMEDIATE);
            instance.release();
        }
    }
    
    public string DebugData(string parameter)
    {
        if (!instance.isValid()) return "invalid instance";
        
        string debugInfo = "ControlledSfx Debug Info:\n";
        debugInfo += $"Event Reference: {eventRef}\n";

        float paramValue;
        
        if (instance.getParameterByName(parameter, out paramValue) == FMOD.RESULT.OK)
        {
            debugInfo += $"{parameter} Parameter: {paramValue}\n";
        }
        
        float volume;
        instance.getVolume(out volume);
        debugInfo += "\nvolume: " + volume + "\n";

        return debugInfo;
    }
}
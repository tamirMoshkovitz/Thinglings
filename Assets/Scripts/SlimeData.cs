using UnityEngine;

public class SlimeData
{
    private SlimeBehavior slime;
    private static bool reachedMaxStretch = true;
    private bool _connected;
    
    public SlimeData(SlimeBehavior slime)
    {
        this.slime = slime;
    }
    
    public float MaxStretch => slime.maxStretch;
    public float ConnectionDistance => slime.connectionDistance;

    public bool ReachedMaxStretch 
    {
        get => reachedMaxStretch;
        set => reachedMaxStretch = value;
    }

    public bool Connected
    {
        get
        {
            if (!ReachedMaxStretch ||
                Vector3.Distance(slime.leftCenter.position, slime.rightCenter.position) < slime.connectionDistance)
            {
                Connected = true;
                ReachedMaxStretch = false;
                return _connected;
            }

            Connected = false;
            return _connected;
        }
        private set
        {
            if (_connected == false && value == true)
            {
                // Just connected
                GameEvents.slimeConnected?.Invoke();
            }
            _connected = value;
        }
    }

    public float Distance => Vector3.Distance(slime.leftCenter.position, slime.rightCenter.position);

    public float StretchRatio => Distance / slime.maxStretch;
}
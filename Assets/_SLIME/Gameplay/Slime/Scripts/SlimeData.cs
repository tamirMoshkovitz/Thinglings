using UnityEngine;

public class SlimeData
{
    private readonly SlimeBehavior _slime;
    private static bool _reachedMaxStretch = true;
    private bool _connected;
    private static SlimeData _instance;
    
    private  SlimeData(SlimeBehavior slime)
    {
        _slime = slime;
    }

    public static SlimeData Instance(SlimeBehavior slime)
    {
        if (_instance == null)
        {
            _instance = new SlimeData(slime);
        }
        return _instance;
    }

    public static SlimeData Instance()
    {
        if (_instance == null)
        {
            Debug.LogError("SlimeData instance is not initialized. Call Instance(SlimeBehavior slime) first.");
        }
        return _instance;
    }
    
    public float MaxStretch => _slime.maxStretch;
    public float ConnectionDistance => _slime.connectionDistance;

    public bool ReachedMaxStretch 
    {
        get => _reachedMaxStretch;
        set => _reachedMaxStretch = value;
    }

    public bool Connected
    {
        get
        {
            if (!ReachedMaxStretch ||
                Vector3.Distance(_slime.leftCenter.position, _slime.rightCenter.position) < _slime.connectionDistance)
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

    public float Distance => Vector3.Distance(_slime.leftCenter.position, _slime.rightCenter.position);

    public float StretchRatio => Distance / _slime.maxStretch;
    
    public Vector2 GetShotDirection(Vector2 position)
    {
        Vector2 a = _slime.leftCenter.position;
        Vector2 b = _slime.rightCenter.position;
        Vector2 ab = b - a;
        float t = Vector2.Dot(position - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        Vector2 closestPoint = a + ab * t;
        return (position - closestPoint);
    }
}
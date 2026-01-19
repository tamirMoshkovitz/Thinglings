using UnityEngine;
using _SLIME.Slime;


public class PlayerInCenterDetector : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float timeToTrigger = 3.0f;
    
    private float _timer;
    private int _totalPlayersInside;
    public bool IsReadyToFire => _timer >= timeToTrigger;

    private void Update()
    {
        if (_totalPlayersInside == 2)
        {
            _timer += Time.deltaTime;
        }
    }

    public void ResetTrigger()
    {
        _timer = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _totalPlayersInside += 1;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _totalPlayersInside -= 1;
        _timer = 0f;
    }
}
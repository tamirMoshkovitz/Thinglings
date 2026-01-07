using UnityEngine;
using UnityEngine.Serialization;

public class TurnOffGameobjectAfterTime : MonoBehaviour
{
    [SerializeField] private float totalTime;

    private float _timer;
    
    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= 10)
        {
            _timer = 0;
            gameObject.SetActive(false);
        }
    }
}
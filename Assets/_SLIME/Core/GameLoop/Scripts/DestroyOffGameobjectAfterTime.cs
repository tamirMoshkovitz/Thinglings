using UnityEngine;
using UnityEngine.Serialization;

public class DestroyOffGameobjectAfterTime : MonoBehaviour
{
    [SerializeField] private float totalTime;

    private float _timer;

    private void OnEnable()
    {
        _timer = 0f;
    }
    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= 10)
        {
            _timer = 0;
            TurnOff();
        }
    }

    public virtual void TurnOff()
    {
        Destroy(gameObject);
    }
}
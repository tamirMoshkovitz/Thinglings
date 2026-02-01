using UnityEngine;
namespace _SLIME.GameLoop
{

    [RequireComponent(typeof(Camera))]
    public class CameraSync : MonoBehaviour
    {
        private Camera _myCamera;

        private void Awake()
        {
            _myCamera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
          
            Camera activeCamera = Camera.main;

            
            if (activeCamera == null || activeCamera == _myCamera) return;
            
            transform.position = activeCamera.transform.position;
            transform.rotation = activeCamera.transform.rotation;

            _myCamera.orthographic = activeCamera.orthographic;

            if (activeCamera.orthographic)
            {
                _myCamera.orthographicSize = activeCamera.orthographicSize;
            }
            else
            {
                _myCamera.fieldOfView = activeCamera.fieldOfView;
            }
        }
    }
}
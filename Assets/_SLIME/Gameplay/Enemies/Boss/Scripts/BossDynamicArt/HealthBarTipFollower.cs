using _SLIME.BaseScripts;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class HealthBarTipFollower : ProjectMonoBehavior
{
    [Header("References")] public Image healthBarImage;
    public Transform rotator;
    public Transform tipVisual;

    [Header("Settings")] public float minAngle;
    public float maxAngle;

    void Update()
    {
        float currentAngle = Mathf.Lerp(minAngle, maxAngle, healthBarImage.fillAmount);
        rotator.localEulerAngles = new Vector3(0, 0, currentAngle);
    }


    public Vector3 GetTipPosition()
    {
        Transform target = tipVisual;
        
        Canvas rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        
        if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
           
            Vector3 screenPos = target.position;
            
            screenPos.z = Mathf.Abs(Camera.main.transform.position.z);

            return Camera.main.ScreenToWorldPoint(screenPos);
        }
        else
        {
            return target.position;
        }
    }

}
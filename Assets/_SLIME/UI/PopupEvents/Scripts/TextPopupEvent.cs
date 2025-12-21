using System.Collections;
using TMPro;
using UnityEngine;

namespace _SLIME.UI
{
    public class TextPopupEvent: IPopupEvent
    {
        public void Render(RenderEvent renderEvent, UIConfiguration.PopUpSettings popUpSettings, MonoBehaviour mono)
        {
            if (!popUpSettings.text)
            {
                Debug.LogWarning("Text popup event is disabled");
            }
            var textObj = SetUpPosition(renderEvent, popUpSettings, out var localPos);
            SetUpText(renderEvent, popUpSettings, textObj);
            
            mono.StartCoroutine(AnimateText(textObj, localPos,renderEvent,popUpSettings));
        }

        private void SetUpText(RenderEvent renderEvent, UIConfiguration.PopUpSettings popUpSettings,
            TextMeshProUGUI textObj)
        {
                if(renderEvent.value > 0) textObj.text = "+" + renderEvent.value;
                else textObj.text = renderEvent.value.ToString();
        }

        private TextMeshProUGUI SetUpPosition(RenderEvent renderEvent, UIConfiguration.PopUpSettings popUpSettings, out Vector3 localPos)
        {
            Vector3 worldPosition = renderEvent.position;
            var textObj = MonoBehaviour.Instantiate(popUpSettings.textPrefab, renderEvent.fatherTransform);
            localPos = renderEvent.fatherTransform.InverseTransformPoint(worldPosition);
            textObj.rectTransform.localPosition = localPos;
            return textObj;
        }

        private IEnumerator AnimateText(TextMeshProUGUI textObj, Vector3 startPosition,
            RenderEvent renderEvent, UIConfiguration.PopUpSettings popUpSettings)
        {
            float elapsedTime = 0f;
            float moveUpDistance = popUpSettings.GetMoveDistance();
            Vector3 endPosition = startPosition + Vector3.up * moveUpDistance;
            Color startColor = textObj.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
            float timeUntilDispawn = popUpSettings.TimeUntilDispawn;
            float fadeTime = popUpSettings.GetFadeTime();
            while (elapsedTime < timeUntilDispawn + fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / timeUntilDispawn;
                textObj.rectTransform.localPosition = Vector3.Lerp(startPosition, endPosition, progress);
                    
                if (elapsedTime > timeUntilDispawn)
                {
                    float fadeProgress = (elapsedTime - timeUntilDispawn) / fadeTime;
                    textObj.color = Color.Lerp(startColor, endColor, fadeProgress);
                }
                yield return null;
            }
            Object.Destroy(textObj.gameObject);
        }
        
        
    }
}
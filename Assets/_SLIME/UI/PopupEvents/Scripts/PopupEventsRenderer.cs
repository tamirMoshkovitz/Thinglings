using System;
using System.Collections.Generic;
using UnityEngine;

namespace _SLIME.UI
{
    
    public enum EventType
    {
        BossHealth
    }

    public struct RenderEvent
    {
        public EventType eventType;
        public Vector3   position;
        public float value;
        public Action OnFinish;
        public Transform fatherTransform; 
    }
    public class PopupEventsRenderer: MonoBehaviour
    {
        public static event Action<RenderEvent>                   RenderPopUp;
        [SerializeField]                  private Transform       worldSpaceCanvasTransform;
        private readonly Dictionary<EventType, (IPopupEvent,UIConfiguration.PopUpSettings)> _renderEventsMap = 
            new Dictionary<EventType, (IPopupEvent,UIConfiguration.PopUpSettings)>();
        [SerializeField] private UIConfiguration uiConfiguration;

        public void Awake()
        {
            TextPopupEvent _textPopupEvent = new TextPopupEvent();
            _renderEventsMap.Add(EventType.BossHealth, (_textPopupEvent,uiConfiguration.bossHealthPopUp));
        }

        private void OnEnable()
        {
            RenderPopUp += Render;
        }

        private void OnDisable()
        {
            RenderPopUp -= Render;
        }

        private void Render(RenderEvent renderEvent)
        {
            if(renderEvent.fatherTransform == null) renderEvent.fatherTransform = worldSpaceCanvasTransform;
            if(_renderEventsMap.ContainsKey(renderEvent.eventType))
                _renderEventsMap[renderEvent.eventType].Item1.Render(renderEvent,_renderEventsMap[renderEvent.eventType].Item2,this);
        }

        public static void OnRenderPointsAbove(RenderEvent obj)
        {
            RenderPopUp?.Invoke(obj);
        }
    }

    
}
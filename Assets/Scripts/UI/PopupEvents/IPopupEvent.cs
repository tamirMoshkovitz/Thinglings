
using UnityEngine;

namespace UI
{
    public interface IPopupEvent
    {
        public void Render(RenderEvent renderEvent, UIConfiguration.PopUpSettings popUpSettings, MonoBehaviour mono);
    }
}
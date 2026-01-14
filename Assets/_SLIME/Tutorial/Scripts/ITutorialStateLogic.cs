using System.Collections;

namespace _SLIME.Tutorial
{
    public interface ITutorialStateLogic
    {
        IEnumerator Start();
        void OnDisable();
    }
}

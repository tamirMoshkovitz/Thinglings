namespace _SLIME.Slime
{
    public interface ISlimeBehaviorComponent
    {
        void OnDestroy();
        void UpdateStretch();
        void OnSlimeTears();
        void OnSlimeConnected();
        void OnTearFinished();
        void OnPauseGame();
        void OnResumeGame();
    }
}
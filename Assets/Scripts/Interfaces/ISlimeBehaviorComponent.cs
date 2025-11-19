namespace Player.Interfaces
{
    public interface ISlimeBehaviorComponent
    {
        ISlimeBehaviorComponent Awake(SlimeData slimeData);
        void OnDestroy();
        void UpdateStretch();
        void OnSlimeTears();
        void OnTearFinished();
        void OnPauseGame();
        void OnResumeGame();
    }
}
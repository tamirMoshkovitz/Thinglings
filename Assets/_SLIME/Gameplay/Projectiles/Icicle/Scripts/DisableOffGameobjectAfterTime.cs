namespace _SLIME.Gameplay.Projectiles.Icicle.Scripts
{
    public class DisableOffGameobjectAfterTime : DestroyOffGameobjectAfterTime
    {
        public override void TurnOff()
        {
            gameObject.SetActive(false);
        }
    }
}
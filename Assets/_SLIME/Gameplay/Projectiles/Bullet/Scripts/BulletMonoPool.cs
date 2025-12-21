using _SLIME.Generics;


namespace _SLIME.Projectiles
{
    /// <summary>
    /// Object pool for managing bullet instances to improve performance.
    /// Reuses bullet objects instead of creating and destroying them.
    /// </summary>
    public class BulletMonoPool : MonoPool<Bullet>
    {
    }
}
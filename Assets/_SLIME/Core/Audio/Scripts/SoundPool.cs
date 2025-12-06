using Generics;

namespace Audio
{
    /// <summary>
    /// Pool for SoundObject instances, enabling efficient reuse of audio sources.
    /// </summary>
    public class SoundPool : MonoPool<SoundObject>
    {
        // No additional logic needed; inherits pooling behavior from SPCMonoPool.
    }
}
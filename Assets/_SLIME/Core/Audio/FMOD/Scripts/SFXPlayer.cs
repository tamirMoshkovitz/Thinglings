using FMODUnity;

namespace _SLIME.Core.Audio.FMOD.Scripts
{
    public static class SFXPlayer
    {
        public static void Play(EventReference sfx)
        {
            if (!sfx.IsNull)
                RuntimeManager.PlayOneShot(sfx);
        }
    }
}
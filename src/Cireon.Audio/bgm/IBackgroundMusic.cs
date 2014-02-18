namespace Cireon.Audio
{
    public interface IBackgroundMusic
    {
        void Update(float elapsedTimeS);
        void Start();
        void Stop();

        void OnVolumeChanged(float volume);
        void OnPitchChanged(float pitch);
    }
}
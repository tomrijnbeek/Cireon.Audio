using System;

namespace Cireon.Audio
{
    public interface IBackgroundMusic
    {
        void Update(float elapsedTimeS);
        void Start();
        void Stop();
        void FadeOut(float length, Action callback);

        void OnVolumeChanged(float volume);
        void OnPitchChanged(float pitch);
    }
}
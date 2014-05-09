using System;

namespace Cireon.Audio
{
    /// <summary>
    /// A background music controller that knows how to play background music.
    /// </summary>
    public interface IBackgroundMusic
    {
        /// <summary>
        /// Updates the background music controller.
        /// </summary>
        /// <param name="elapsedTimeS">The elapsed time in seconds since the last update.</param>
        void Update(float elapsedTimeS);
        /// <summary>
        /// Starts playing this background music controller.
        /// </summary>
        void Start();
        /// <summary>
        /// Stops playing this background music controller.
        /// </summary>
        void Stop();
        /// <summary>
        /// Fades out this background music controller.
        /// </summary>
        /// <param name="length">The length of this fade.</param>
        /// <param name="callback">The method that should be called when the fade is finished.</param>
        void FadeOut(float length, Action callback);

        /// <summary>
        /// Handles a change in music volume.
        /// </summary>
        /// <param name="volume">The new music volume.</param>
        void OnVolumeChanged(float volume);
        /// <summary>
        /// Handles a change in pitch.
        /// </summary>
        /// <param name="pitch">The new pitch.</param>
        void OnPitchChanged(float pitch);
    }
}
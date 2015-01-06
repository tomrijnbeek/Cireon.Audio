using System;

namespace Cireon.Audio
{
    /// <summary>
    /// A background music controller that repeats a single file.
    /// </summary>
    sealed public class SingleSongBackgroundMusic : IBackgroundMusic
    {
        private readonly Song song;

        private float globalVolume, localVolume = 1;
        private FadeDefinition currentFade;
        private Action fadeCallback;

        /// <summary>
        /// Creates a new background music controller that repeats a single file.
        /// </summary>
        /// <param name="s">The song to play.</param>
        public SingleSongBackgroundMusic(Song s)
        {
            this.song = s;
            this.song.Looping = true;
        }

        /// <summary>
        /// Creates a new background music controller that repeats a single file.
        /// </summary>
        /// <param name="file">The file containing the song to play.</param>
        public SingleSongBackgroundMusic(string file)
            : this(new Song(file)) { }

        /// <summary>
        /// Updates the background music controller.
        /// </summary>
        /// <param name="elapsedTimeS">The elapsed time in seconds since the last update.</param>
        public void Update(float elapsedTimeS)
        {
            if (this.currentFade != null)
            {
                this.currentFade.Update(elapsedTimeS);
                this.localVolume = this.currentFade.CurrentVolume;
                this.OnVolumeChanged(this.globalVolume);

                if (this.currentFade.Finished)
                {
                    this.currentFade = null;

                    if (this.fadeCallback != null)
                    {
                        this.fadeCallback();
                        this.fadeCallback = null;
                    }
                }
            }
        }

        /// <summary>
        /// Starts playing this background music controller.
        /// </summary>
        public void Start()
        {
            this.song.Play();
        }

        /// <summary>
        /// Stops playing this background music controller.
        /// </summary>
        public void Stop()
        {
            this.song.Stop();
            this.localVolume = 1;
        }

        /// <summary>
        /// Fades out this background music controller.
        /// </summary>
        /// <param name="length">The length of this fade.</param>
        /// <param name="callback">The method that should be called when the fade is finished.</param>
        public void FadeOut(float length, Action callback)
        {
            this.currentFade = new FadeDefinition(length, this.localVolume, 0);
            this.fadeCallback = callback;
        }

        /// <summary>
        /// Handles a change in music volume.
        /// </summary>
        /// <param name="volume">The new music volume.</param>
        public void OnVolumeChanged(float volume)
        {
            this.globalVolume = volume;
            this.song.Volume = this.globalVolume * this.localVolume;
        }

        /// <summary>
        /// Handles a change in pitch.
        /// </summary>
        /// <param name="pitch">The new pitch.</param>
        public void OnPitchChanged(float pitch)
        {
            this.song.Pitch = pitch;
        }

        /// <summary>
        /// Handles a change in low pass gain.
        /// </summary>
        /// <param name="lowPassGain">The new low pass gain.</param>
        public void OnLowPassGainChanged(float lowPassGain)
        {
            this.song.LowPassGain = lowPassGain;
        }
    }
}
using System;
using System.Net.NetworkInformation;
using OpenTK.Audio;

namespace Cireon.Audio
{
    /// <summary>
    /// Main audio manager.
    /// Keeps track of several sub-managers and provides global access to audio-related objects.
    /// </summary>
    public sealed class AudioManager
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static AudioManager Instance { get; private set; }

        /// <summary>
        /// Initializes the singleton instance.
        /// </summary>
        public static void Initialize()
        {
            AudioManager.Instance = new AudioManager();
        }

        /// <summary>
        /// Disposes the singleton instance.
        /// Includes clearing all buffers and releasing all OpenAL resources.
        /// </summary>
        public static void Dispose()
        {
            AudioManager.Instance.dispose();
        }

        /// <summary>
        /// The manager that keeps track of all OpenAL Sources.
        /// </summary>
        public readonly SourceManager SourceManager;

        private readonly AudioContext context;

        private IBackgroundMusic currentBGM;

        private float masterVolume, musicVolume, effectsVolume;

        /// <summary>
        /// The master volume that is applied to both the music and soundeffects.
        /// </summary>
        public float MasterVolume
        {
            get { return this.masterVolume; }
            set
            {
                if (this.masterVolume == value)
                    return;

                this.masterVolume = value;
                this.onMusicVolumeChanged();
                this.onEffectsVolumeChanged();
            }
        }
        /// <summary>
        /// The volume that is used for playing the music.
        /// </summary>
        public float MusicVolume
        {
            get { return this.musicVolume; }
            set
            {
                if (this.musicVolume == value)
                    return;

                this.musicVolume = value;
                this.onMusicVolumeChanged();
            }
        }
        /// <summary>
        /// The volume that is used for playing the sound effects.
        /// </summary>
        public float EffectsVolume
        {
            get { return this.effectsVolume; }
            set
            {
                if (this.effectsVolume == value)
                    return;

                this.effectsVolume = value;
                this.onEffectsVolumeChanged();
            }
        }

        private float pitch = 1;

        public float Pitch
        {
            get { return this.pitch; }
            set
            {
                if (this.pitch == value)
                    return;

                this.pitch = value;
                this.onPitchChanged();
            }
        }

        private AudioManager()
        {
            this.context = new AudioContext();
            OggStreamer.Initialize();

            this.SourceManager = new SourceManager();

            this.masterVolume = 1;
            this.musicVolume = 1;
            this.effectsVolume = 1;
        }

        /// <summary>
        /// Updates all audio-related resources.
        /// </summary>
        /// <param name="elapsedTimeS">The elapsed time in seconds since the last update.</param>
        public void Update(float elapsedTimeS)
        {
            this.SourceManager.Update();
            if (this.currentBGM != null)
                this.currentBGM.Update(elapsedTimeS);
        }

        #region Volumes
        private void onMusicVolumeChanged()
        {
            if (this.currentBGM != null)
                this.currentBGM.OnVolumeChanged(this.MasterVolume * this.MusicVolume);
        }

        private void onEffectsVolumeChanged()
        {

        }
        #endregion

        #region Pitch
        private void onPitchChanged()
        {
            if (this.currentBGM != null)
                this.currentBGM.OnPitchChanged(this.Pitch);
        }
        #endregion

        /// <summary>
        /// Sets the background music to the specified controller.
        /// </summary>
        /// <param name="bgm">The background music controller.</param>
        public void SetBGM(IBackgroundMusic bgm)
        {
            if (this.currentBGM != null)
                this.currentBGM.Stop();

            this.currentBGM = bgm;

            if (this.currentBGM != null)
            {
                this.onMusicVolumeChanged();
                this.onPitchChanged();
                this.currentBGM.Start();
            }
        }

        /// <summary>
        /// Sets the background music to the specified controller, fading out the old background music.
        /// </summary>
        /// <param name="bgm"></param>
        /// <param name="fadeDuration"></param>
        public void SetBGMWithFadeOut(IBackgroundMusic bgm, float fadeDuration = 0.5f)
        {
            if (this.currentBGM == null)
            {
                this.SetBGM(bgm);
                return;
            }

            this.currentBGM.FadeOut(fadeDuration, () => this.SetBGM(bgm));
        }

        public void SetBGMWithFade(IBackgroundMusic bgm, float fadeDuration = 0.25f)
        {
            throw new NotImplementedException();
        }

        public void SetBGMWithCrossFade(IBackgroundMusic bgm, float fadeDuration = 0.5f)
        {
            throw new NotImplementedException();
        }

        private void dispose()
        {
            this.SourceManager.Dispose();
            OggStreamer.DisposeInstance();
            this.context.Dispose();
        }
    }
}

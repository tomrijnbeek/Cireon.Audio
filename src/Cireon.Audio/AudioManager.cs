using System;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Cireon.Audio
{
    /// <summary>
    /// Main audio manager.
    /// Keeps track of several sub-managers and provides global access to audio-related objects.
    /// </summary>
    public sealed class AudioManager : IDisposable
    {
        private static AudioManager instance;

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static AudioManager Instance
        {
            get
            {
                if (AudioManager.instance == null)
                    throw new Exception("You have to initialise the audio manager before accessing it.");
                return AudioManager.instance;
            }
        }

        /// <summary>
        /// Initializes the singleton instance.
        /// </summary>
        public static void Initialize()
        {
            AudioManager.instance = new AudioManager();
        }

        private bool disposed;

        private readonly AudioContext context;
        private readonly EffectsExtension efx;
        private readonly SourceManager sourceManager;

        private IBackgroundMusic currentBGM;
        private float masterVolume, musicVolume, effectsVolume, pitch, lowPassGain;

        /// <summary>
        /// The manager that keeps track of all OpenAL Sources.
        /// </summary>
        public SourceManager SourceManager
        {
            get { return this.sourceManager; }
        }

        /// <summary>
        /// The OpenAL Effects extension.
        /// </summary>
        public EffectsExtension Efx
        {
            get { return this.efx; }
        }

        /// <summary>
        /// The (default) master volume that is applied to both the music and soundeffects.
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
        /// The default volume that is used for playing the music.
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
        /// The default volume that is used for playing the sound effects.
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

        /// <summary>
        /// The default pitch audio is played with.
        /// </summary>
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

        /// <summary>
        /// The gain of the overarching low pass filter.
        /// </summary>
        public float LowPassGain
        {
            get { return this.lowPassGain; }
            set
            {
                if (this.lowPassGain == value)
                    return;

                this.lowPassGain = value;
                this.onLowPassGainChanged();
            }
        }

        private AudioManager()
        {
            this.context = new AudioContext();
            this.efx = new EffectsExtension();
            OggStreamer.Initialize();

            this.sourceManager = new SourceManager();

            this.masterVolume = 1;
            this.musicVolume = 1;
            this.effectsVolume = 1;
            this.pitch = 1;
            this.lowPassGain = 1;
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

        #region Low Pass Gain
        private void onLowPassGainChanged()
        {
            if (this.currentBGM != null)
                this.currentBGM.OnLowPassGainChanged(this.LowPassGain);
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
                this.onLowPassGainChanged();
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

        /// <summary>
        /// Sets the background music to the specified controller, fading out the old background music and fading in the new one.
        /// </summary>
        /// <param name="bgm"></param>
        /// <param name="fadeDuration"></param>
        public void SetBGMWithFade(IBackgroundMusic bgm, float fadeDuration = 0.25f)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the background music to the specified controller, crossfading between the two.
        /// </summary>
        /// <param name="bgm"></param>
        /// <param name="fadeDuration"></param>
        public void SetBGMWithCrossFade(IBackgroundMusic bgm, float fadeDuration = 0.5f)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Disposes the audio manager and all subsystems.
        /// </summary>
        public void Dispose()
        {
            if (this.disposed)
                return;

            this.sourceManager.Dispose();
            OggStreamer.DisposeInstance();
            this.context.Dispose();
            this.disposed = true;
        }
    }
}

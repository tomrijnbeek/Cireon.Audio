using System;
using System.Collections.Generic;
using NVorbis;
using NVorbis.OpenTKSupport;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace BeardGame.Audio
{
    sealed class AudioManager
    {
        public static AudioManager Instance { get; private set; }

        public static void Initialize(string sfxPath)
        {
            AudioManager.Instance = new AudioManager(sfxPath);
        }

        public static void Dispose()
        {
            AudioManager.Instance.dispose();
        }

        public readonly ALSourceManager SourceManager;
        public readonly SoundLibrary Sounds;

        private readonly AudioContext context;
        private readonly OggStreamer streamer;

        private BackgroundMusic currentBGM;

        private float masterVolume, musicVolume, effectsVolume;

        public float MasterVolume
        {
            get { return this.masterVolume; }
            set
            {
                this.masterVolume = value;
                this.onMusicVolumeChanged();
                this.onEffectsVolumeChanged();
            }
        }
        public float MusicVolume
        {
            get { return this.musicVolume; }
            set
            {
                this.musicVolume = value;
                this.onMusicVolumeChanged();
            }
        }
        public float EffectsVolume
        {
            get { return this.effectsVolume; }
            set
            {
                this.effectsVolume = value;
                this.onEffectsVolumeChanged();
            }
        }

        private AudioManager(string sfxPath)
        {
            this.context = new AudioContext();
            this.streamer = new OggStreamer();

            this.SourceManager = new ALSourceManager();
            this.Sounds = new SoundLibrary(sfxPath);

            this.masterVolume = 1;
            this.musicVolume = 1;
            this.effectsVolume = 1;
        }

        public void Update(float elapsedTimeS)
        {
            this.SourceManager.Update();
        }

        #region Volumes
        private void onMusicVolumeChanged()
        {
            if (this.currentBGM != null)
                this.currentBGM.Volume = this.masterVolume * this.musicVolume;
        }

        private void onEffectsVolumeChanged()
        {

        }
        #endregion

        public void SetBGM(string file)
        {
            if (this.currentBGM != null)
                this.currentBGM.Dispose();

            this.currentBGM = new BackgroundMusic(file);
            this.currentBGM.Volume = this.masterVolume * this.musicVolume;
            this.currentBGM.Play();
        }

        private void dispose()
        {
            if (this.currentBGM != null)
                this.currentBGM.Dispose();

            this.SourceManager.Dispose();
            this.streamer.Dispose();
            this.context.Dispose();
        }
    }
}

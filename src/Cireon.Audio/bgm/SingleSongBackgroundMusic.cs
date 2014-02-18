using System;

namespace Cireon.Audio
{
    sealed public class SingleSongBackgroundMusic : IBackgroundMusic
    {
        private readonly Song song;

        private float globalVolume, localVolume = 1;
        private FadeDefinition currentFade;
        private Action fadeCallback;

        public SingleSongBackgroundMusic(Song s)
        {
            this.song = s;
            this.song.Looping = true;
        }

        public SingleSongBackgroundMusic(string file)
            : this(new Song(file)) { }

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

        public void Start()
        {
            this.song.Play();
        }

        public void Stop()
        {
            this.song.Stop();
            this.localVolume = 1;
        }

        public void FadeOut(float time, Action callback)
        {
            this.currentFade = new FadeDefinition(time, this.localVolume, 0);
            this.fadeCallback = callback;
        }

        public void OnVolumeChanged(float volume)
        {
            this.globalVolume = volume;
            this.song.Volume = this.globalVolume * this.localVolume;
        }

        public void OnPitchChanged(float pitch)
        {
            this.song.Pitch = pitch;
        }
    }
}
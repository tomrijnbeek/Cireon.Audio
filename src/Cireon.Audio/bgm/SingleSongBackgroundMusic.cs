namespace Cireon.Audio
{
    sealed public class SingleSongBackgroundMusic : IBackgroundMusic
    {
        private readonly Song song;

        public SingleSongBackgroundMusic(Song s)
        {
            this.song = s;
            this.song.Looping = true;
        }

        public SingleSongBackgroundMusic(string file)
            : this(new Song(file)) { }

        public void Update(float elapsedTimeS)
        {
            
        }

        public void Start()
        {
            this.song.Play();
        }

        public void Stop()
        {
            this.song.Stop();
        }

        public void OnVolumeChanged(float volume)
        {
            this.song.Volume = volume;
        }

        public void OnPitchChanged(float pitch)
        {
            this.song.Pitch = pitch;
        }
    }
}
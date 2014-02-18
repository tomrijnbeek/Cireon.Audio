namespace Cireon.Audio
{
    /// <summary>
    /// Wrapper class for background music.
    /// </summary>
    public sealed class Song
    {
        private readonly OggStream stream;

        /// <summary>
        /// True if the stream has been initialised and ready to play.
        /// </summary>
        public bool Prepared
        {
            get { return this.stream.Ready; }
        }

        private bool finished;

        /// <summary>
        /// Whether the song has finished playing.
        /// </summary>
        public bool FinishedPlaying
        {
            get { return this.finished; }
        }

        /// <summary>
        /// The volume at which the song is played.
        /// </summary>
        public float Volume
        {
            get { return this.stream.Volume; }
            set { this.stream.Volume = value; }
        }

        /// <summary>
        /// The pitch at which the song is played.
        /// </summary>
        public float Pitch
        {
            get { return this.stream.Pitch; }
            set { this.stream.Pitch = value; }
        }

        /// <summary>
        /// Whether the song should be looping.
        /// </summary>
        public bool Looping
        {
            get { return this.stream.IsLooped; }
            set { this.stream.IsLooped = value; }
        }

        /// <summary>
        /// Creates a new song from a file.
        /// </summary>
        /// <param name="file">The filename of the ogg-file that contains the music.</param>
        public Song(string file)
        {
            this.stream = new OggStream(file);
            this.stream.Prepare();
            this.stream.Finished += (sender, args) => this.finished = !this.Looping;
        }

        /// <summary>
        /// Starts playing the song.
        /// </summary>
        public void Play()
        {
            this.finished = false;
            this.stream.Play();
        }

        /// <summary>
        /// Pauses playing the song.
        /// </summary>
        public void Pause()
        {
            this.stream.Pause();
        }

        /// <summary>
        /// Stops playing the song.
        /// </summary>
        public void Stop()
        {
            this.stream.Stop();
        }

        /// <summary>
        /// Stops the song and frees up the allocated resources.
        /// </summary>
        public void Dispose()
        {
            this.Stop();
            this.stream.Dispose();
        }
    }
}

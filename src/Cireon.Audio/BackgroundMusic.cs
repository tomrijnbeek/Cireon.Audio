namespace Cireon.Audio
{
    /// <summary>
    /// Wrapper class for background music.
    /// </summary>
    public sealed class BackgroundMusic
    {
        private readonly OggStream stream;

        private bool prepared;
        /// <summary>
        /// True if the stream has been initialised and ready to play.
        /// </summary>
        public bool Prepared { get { return this.prepared; } }

        /// <summary>
        /// The volume at which the background music is played.
        /// </summary>
        public float Volume
        {
            get { return this.stream.Volume; }
            set { this.stream.Volume = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file">The filename of the ogg-file that contains the music.</param>
        public BackgroundMusic(string file)
        {
            this.stream = new OggStream(file);
            this.stream.Prepare();
        }

        /// <summary>
        /// Updates the background music.
        /// </summary>
        /// <param name="elapsedTimeS">Elapsed time in seconds since the last update.</param>
        public void Update(float elapsedTimeS)
        {
            this.prepared = this.stream.Ready;
        }

        /// <summary>
        /// Starts playing the background music.
        /// </summary>
        public void Play()
        {
            this.stream.Play();
        }

        /// <summary>
        /// Stops the background music and frees up the allocated resources.
        /// </summary>
        public void Dispose()
        {
            this.stream.Stop();
            this.stream.Dispose();
        }
    }
}

namespace Cireon.Audio
{
    /// <summary>
    /// Wrapper class for sound effects.
    /// </summary>
    public sealed class SoundEffect
    {
        private readonly SoundBuffer buffer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file">The filename of the ogg-file that contains the sound effect.</param>
        public SoundEffect(string file)
        {
            this.buffer = SoundBuffer.FromFile(file);
        }

        /// <summary>
        /// Plays a single loop of the sound effect.
        /// </summary>
        public void Play()
        {
            // Bind the buffers to a source and play it
            var source = AudioManager.Instance.SourceManager.RequestSource();
            source.QueueBuffer(this.buffer);
            source.Volume = AudioManager.Instance.EffectsVolume;
            source.Play();
        }
    }
}

using System.IO;

namespace Cireon.Audio
{
    /// <summary>
    /// Wrapper class for sound effects.
    /// </summary>
    public sealed class SoundFile
    {
        private readonly SoundBuffer buffer;

        /// <summary>
        /// Loads a new soundfile from a file.
        /// </summary>
        /// <param name="file">The filename of the ogg-file that contains the sound effect.</param>
        public SoundFile(string file)
        {
            this.buffer = SoundBuffer.FromFile(file);
        }

        /// <summary>
        /// Loads a new soundfile from a file.
        /// </summary>
        /// <param name="stream">The filestream containing the sound effect in ogg-format.</param>
        public SoundFile(Stream stream)
        {
            this.buffer = SoundBuffer.FromFile(stream);
        }

        /// <summary>
        /// Plays a single loop of the sound effect.
        /// </summary>
        public void Play()
        {
            this.GenerateSource().Play();
        }

        /// <summary>
        /// Generates an audio source, fills its buffers and sets its properties to the default values.
        /// </summary>
        /// <returns>The source filled with the right buffers with the default properties set.</returns>
        public Source GenerateSource()
        {
            var source = AudioManager.Instance.SourceManager.RequestSource();
            source.QueueBuffer(this.buffer);
            source.Volume = AudioManager.Instance.EffectsVolume;
            source.Pitch = AudioManager.Instance.Pitch;
            return source;
        }
    }
}

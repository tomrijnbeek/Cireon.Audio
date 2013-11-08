using System;
using NVorbis;
using NVorbis.OpenTKSupport;

namespace Cireon.Audio
{
    public sealed class BackgroundMusic
    {
        private readonly OggStream stream;

        private bool prepared;
        public bool Prepared { get { return this.prepared; } }

        public float Volume
        {
            get { return this.stream.Volume; }
            set { this.stream.Volume = value; }
        }

        public BackgroundMusic(string file, bool startMuted = false)
        {
            this.stream = new OggStream(file);
            this.stream.Prepare();
        }

        public void Update(float elapsedTimeS)
        {
            this.prepared = this.stream.Ready;
        }

        public void Play()
        {
            this.stream.Play();
        }

        public void Dispose()
        {
            this.stream.Stop();
            this.stream.Dispose();
        }
    }
}

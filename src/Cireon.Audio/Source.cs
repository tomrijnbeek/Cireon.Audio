using System;
using OpenTK.Audio.OpenAL;

namespace Cireon.Audio
{
    public enum SourceState
    {
        Stopped,
        Playing,
        Paused
    }

    public sealed class Source
    {
        public readonly int Handle;

        public int ProcessedBuffers
        {
            get
            {
                int processedBuffers;
                AL.GetSource(this.Handle, ALGetSourcei.BuffersProcessed, out processedBuffers);
                return processedBuffers;
            }
        }

        public int QueuedBuffers
        {
            get
            {
                int queuedBuffers;
                AL.GetSource(this.Handle, ALGetSourcei.BuffersQueued, out queuedBuffers);
                return queuedBuffers;
            }
        }

        public ALSourceState State
        {
            get { return AL.GetSourceState(this.Handle); }
        }

        public bool FinishedPlaying
        {
            get { return this.ProcessedBuffers >= this.QueuedBuffers; }
        }

        public float Volume
        {
            get
            {
                float gain;
                AL.GetSource(this.Handle, ALSourcef.Gain, out gain);
                return gain;
            }
            set { AL.Source(this.Handle, ALSourcef.Gain, value); }
        }

        public Source()
        {
            this.Handle = AL.GenSource();
            ALHelper.Check();
        }

        public void QueueBuffers(int bufferLength, int[] bufferIDs)
        {
            AL.SourceQueueBuffers(this.Handle, bufferLength, bufferIDs);
            ALHelper.Check();
        }

        public void Play()
        {
            AL.SourcePlay(this.Handle);
            ALHelper.Check();
        }

        public void Pause()
        {
            AL.SourcePause(this.Handle);
            ALHelper.Check();
        }

        public void Stop()
        {
            AL.SourceStop(this.Handle);
            ALHelper.Check();
        }

        public void Dispose()
        {
            if (this.State != ALSourceState.Stopped)
                this.Stop();

            AL.DeleteSource(this.Handle);
            ALHelper.Check();
        }

        static public implicit operator int(Source source)
        {
            return source.Handle;
        }
    }
}

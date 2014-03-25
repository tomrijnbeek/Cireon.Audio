using OpenTK.Audio.OpenAL;

namespace Cireon.Audio
{
    /// <summary>
    /// Wrapper class for OpenAL Sources.
    /// </summary>
    public sealed class Source
    {
        /// <summary>
        /// OpenAL source handle.
        /// </summary>
        public readonly int Handle;

        private bool disposed;

        /// <summary>
        /// The amount of buffers the source has already played.
        /// </summary>
        public int ProcessedBuffers
        {
            get
            {
                int processedBuffers;
                AL.GetSource(this.Handle, ALGetSourcei.BuffersProcessed, out processedBuffers);
                ALHelper.Check();
                return processedBuffers;
            }
        }

        /// <summary>
        /// The total amount of buffers to source has queued to play.
        /// </summary>
        public int QueuedBuffers
        {
            get
            {
                int queuedBuffers;
                AL.GetSource(this.Handle, ALGetSourcei.BuffersQueued, out queuedBuffers);
                ALHelper.Check();
                return queuedBuffers;
            }
        }

        /// <summary>
        /// The current state of this source.
        /// </summary>
        public ALSourceState State
        {
            get
            {
                var state = AL.GetSourceState(this.Handle);
                ALHelper.Check();
                return state;
            }
        }

        /// <summary>
        /// Whether the source is finished playing all queued buffers.
        /// </summary>
        public bool FinishedPlaying
        {
            get { return this.disposed || this.ProcessedBuffers >= this.QueuedBuffers; }
        }

        private float volume;
        /// <summary>
        /// The volume at which the source plays its buffers.
        /// </summary>
        public float Volume
        {
            get { return this.volume; }
            set
            {
                AL.Source(this.Handle, ALSourcef.Gain, this.volume = value);
                ALHelper.Check();
            }
        }

        private float pitch;

        public float Pitch
        {
            get { return this.pitch; }
            set
            {
                AL.Source(this.Handle, ALSourcef.Pitch, this.pitch = value);
                ALHelper.Check();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Source()
        {
            this.Handle = AL.GenSource();
            ALHelper.Check();
            this.pitch = 1;
        }

        public void QueueBuffer(SoundBuffer buffer)
        {
            this.QueueBuffers(buffer.Handles.Length, buffer.Handles);
        }

        /// <summary>
        /// Adds buffers to the end of the buffer queue of this source.
        /// </summary>
        /// <param name="bufferID">The handle to the OpenAL buffer.</param>
        public void QueueBuffer(int bufferID)
        {
            AL.SourceQueueBuffer(this.Handle, bufferID);
            ALHelper.Check();
        }

        /// <summary>
        /// Adds buffers to the end of the buffer queue of this source.
        /// </summary>
        /// <param name="bufferLength">The length each buffer has.</param>
        /// <param name="bufferIDs">The handles to the OpenAL buffers.</param>
        public void QueueBuffers(int bufferLength, int[] bufferIDs)
        {
            AL.SourceQueueBuffers(this.Handle, bufferLength, bufferIDs);
            ALHelper.Check();
        }

        /// <summary>
        /// Removes all the buffers from the source.
        /// </summary>
        public void UnqueueBuffers()
        {
            AL.SourceUnqueueBuffers(this.Handle, this.QueuedBuffers);
            ALHelper.Check();
        }

        public void UnqueueBuffers(int[] bufferIDs)
        {
            AL.SourceUnqueueBuffers(this.Handle, this.QueuedBuffers, bufferIDs);
            ALHelper.Check();
        }

        /// <summary>
        /// Removes all the processed buffers from the source.
        /// </summary>
        /// <returns>An integer of buffer handles that are removed.</returns>
        public int[] UnqueueProcessedBuffers()
        {
            var unqueued = AL.SourceUnqueueBuffers(this.Handle, this.ProcessedBuffers);
            ALHelper.Check();
            return unqueued;
        }

        /// <summary>
        /// Starts playing the source.
        /// </summary>
        public void Play()
        {
            AL.SourcePlay(this.Handle);
            ALHelper.Check();
        }

        /// <summary>
        /// Pauses playing the source.
        /// </summary>
        public void Pause()
        {
            AL.SourcePause(this.Handle);
            ALHelper.Check();
        }

        /// <summary>
        /// Stops playing the source.
        /// </summary>
        public void Stop()
        {
            AL.SourceStop(this.Handle);
            ALHelper.Check();
        }

        /// <summary>
        /// Stops playing the source and frees allocated resources.
        /// </summary>
        public void Dispose()
        {
            if (this.disposed)
                return;

            if (this.State != ALSourceState.Stopped)
                this.Stop();

            AL.DeleteSource(this.Handle);
            ALHelper.Check();
            this.disposed = true;
        }

        /// <summary>
        /// Casts the source to an integer.
        /// </summary>
        /// <param name="source">The source that should be casted.</param>
        /// <returns>The OpenAL handle of the source.</returns>
        static public implicit operator int(Source source)
        {
            return source.Handle;
        }
    }
}

using System;
using OpenTK.Audio.OpenAL;

namespace Cireon.Audio
{
    /// <summary>
    /// Wrapper class for OpenAL Sources.
    /// </summary>
    public sealed class Source : IDisposable
    {
        /// <summary>
        /// OpenAL source handle.
        /// </summary>
        public readonly int Handle;

        /// <summary>
        /// True if the source is disposed and thus no longer available.
        /// </summary>
        public bool Disposed { get; private set; }

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
            get { return this.ProcessedBuffers >= this.QueuedBuffers && !this.Repeating; }
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
        /// <summary>
        /// The pitch at which the source plays its buffers.
        /// </summary>
        public float Pitch
        {
            get { return this.pitch; }
            set
            {
                AL.Source(this.Handle, ALSourcef.Pitch, this.pitch = value);
                ALHelper.Check();
            }
        }

        private bool repeating;
        /// <summary>
        /// Whether the source should repeat itself or not.
        /// </summary>
        public bool Repeating
        {
            get { return this.repeating; }
            set
            {
                AL.Source(this.Handle, ALSourceb.Looping, this.repeating = value);
                ALHelper.Check();
            }
        }

        /// <summary>
        /// Creates a new OpenAL source.
        /// </summary>
        public Source()
        {
            this.Handle = AL.GenSource();
            ALHelper.Check();

            this.volume = 1;
            this.pitch = 1;
        }

        /// <summary>
        /// Adds a new sound buffer to be played by this source.
        /// </summary>
        /// <param name="buffer"></param>
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

        /// <summary>
        /// Removes the specified buffers from the source queue.
        /// </summary>
        /// <param name="bufferIDs"></param>
        [Obsolete]
        public void UnqueueBuffers(int[] bufferIDs)
        {
            AL.SourceUnqueueBuffers(this.Handle, this.QueuedBuffers, bufferIDs);
            ALHelper.Check();
        }

        /// <summary>
        /// Removes all the processed buffers from the source.
        /// </summary>
        /// <returns>An integer array of buffer handles that are removed.</returns>
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
            if (this.Disposed)
                return;

            if (this.State != ALSourceState.Stopped)
                this.Stop();

            AL.DeleteSource(this.Handle);
            ALHelper.Check();
            this.Disposed = true;
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

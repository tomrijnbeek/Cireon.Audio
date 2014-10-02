using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NVorbis;
using OpenTK.Audio.OpenAL;

namespace Cireon.Audio
{
    /// <summary>
    /// An object representing an ogg-filestream.
    /// </summary>
    public class OggStream : IDisposable
    {
        private const int defaultBufferCount = 3;

        internal readonly object StopMutex = new object();
        internal readonly object PrepareMutex = new object();

        /// <summary>
        /// The source that is used to play the contents from this filestream.
        /// </summary>
        public readonly Source Source;
        internal readonly SoundBuffer Buffer;

        private readonly Stream underlyingStream;

        internal VorbisReader Reader;
        /// <summary>
        /// Whether this stream is ready to start playing.
        /// </summary>
        public bool Ready { get; private set; }
        internal bool Preparing { get; private set; }

        /// <summary>
        /// The amount of buffers currently queued.
        /// </summary>
        public int BufferCount { get; private set; }

        /// <summary>
        /// An event that gets fired when the stream finished playing.
        /// </summary>
        public EventHandler Finished;

        /// <summary>
        /// The volume of this oggstream.
        /// </summary>
        public float Volume
        {
            get { return this.Source.Volume; }
            set { this.Source.Volume = value; }
        }

        /// <summary>
        /// The pitch of this oggstream.
        /// </summary>
        public float Pitch
        {
            get { return this.Source.Pitch; }
            set { this.Source.Pitch = value; }
        }

        /// <summary>
        /// Whether this stream plays the file repeatedly.
        /// </summary>
        public bool IsLooped { get; set; }

        /// <summary>
        /// Creates a new ogg-filestream.
        /// </summary>
        /// <param name="filename">The ogg-file.</param>
        /// <param name="bufferCount">The amount of buffers to use.</param>
        public OggStream(string filename, int bufferCount = defaultBufferCount) : this(File.OpenRead(filename), bufferCount) { }

        /// <summary>
        /// Creates a new ogg-filestream.
        /// </summary>
        /// <param name="stream">The ogg-filestream.</param>
        /// <param name="bufferCount">The amount of buffers to use.</param>
        public OggStream(Stream stream, int bufferCount = defaultBufferCount)
        {
            this.BufferCount = bufferCount;

            this.Buffer = new SoundBuffer(bufferCount);
            this.Source = new Source();

            //if (ALHelper.XRam.IsInitialized)
            //{
            //    ALHelper.XRam.SetBufferMode(BufferCount, ref alBufferIds[0], XRamExtension.XRamStorage.Hardware);
            //    ALHelper.Check();
            //}

            this.Volume = 1;
            this.Pitch = 1;

            this.underlyingStream = stream;
        }

        /// <summary>
        /// Prepares this stream for playing by filling the first buffers.
        /// </summary>
        public void Prepare()
        {
            if (this.Preparing) return;

            var state = this.Source.State;

            lock (this.StopMutex)
            {
                switch (state)
                {
                    case ALSourceState.Playing:
                    case ALSourceState.Paused:
                        return;

                    case ALSourceState.Stopped:
                        lock (this.PrepareMutex)
                        {
                            this.Reader.DecodedTime = TimeSpan.Zero;
                            this.Ready = false;
                            this.empty();
                        }
                        break;
                }

                if (this.Ready) return;

                lock (this.PrepareMutex)
                {
                    this.Preparing = true;
                    this.open(true);
                }
            }
        }

        /// <summary>
        /// Starts playing this stream.
        /// </summary>
        public void Play()
        {
            var state = this.Source.State;

            switch (state)
            {
                case ALSourceState.Playing: return;
                case ALSourceState.Paused:
                    this.Resume();
                    return;
            }

            this.Prepare();

            this.Source.Play();

            this.Preparing = false;

            OggStreamer.Instance.AddStream(this);
        }

        /// <summary>
        /// Pauses playing this stream.
        /// </summary>
        public void Pause()
        {
            if (this.Source.State != ALSourceState.Playing)
                return;

            OggStreamer.Instance.RemoveStream(this);
            this.Source.Pause();
        }

        /// <summary>
        /// Resumes playing this stream after it is paused.
        /// </summary>
        public void Resume()
        {
            if (this.Source.State != ALSourceState.Paused)
                return;

            OggStreamer.Instance.AddStream(this);
            this.Source.Play();
        }

        /// <summary>
        /// Stops playing this stream.
        /// </summary>
        public void Stop()
        {
            if (!this.Source.Disposed)
            {
                var state = this.Source.State;
                if (state == ALSourceState.Playing || state == ALSourceState.Paused)
                {
                    this.stopPlayback();
                }
            }

            lock (this.StopMutex)
            {
                this.NotifyFinished();
                OggStreamer.Instance.RemoveStream(this);
            }
        }

        /// <summary>
        /// Disposes this stream.
        /// </summary>
        public void Dispose()
        {
            var state = this.Source.State;
            if (state == ALSourceState.Playing || state == ALSourceState.Paused)
                this.stopPlayback();

            lock (this.PrepareMutex)
            {
                OggStreamer.Instance.RemoveStream(this);

                if (state != ALSourceState.Initial)
                    this.empty();

                this.close();

                this.underlyingStream.Dispose();
            }

            this.Source.Dispose();
            this.Buffer.Dispose();

            ALHelper.Check();
        }

        private void stopPlayback()
        {
            this.Source.Stop();
        }

        /// <summary>
        /// Call the Finished event.
        /// </summary>
        public void NotifyFinished()
        {
            var callback = this.Finished;
            if (callback != null)
            {
                callback(this, EventArgs.Empty);
                this.Finished = null;  // This is not typical...  Usually we count on whatever code added the event handler to also remove it
            }
        }

        private void empty()
        {
            int queued = this.Source.QueuedBuffers;
            if (queued <= 0) return;

            try
            {
                this.Source.UnqueueBuffers();
            }
            catch (InvalidOperationException)
            {
                // This is a bug in the OpenAL implementation
                // Salvage what we can
                if (this.Source.ProcessedBuffers > 0)
                    this.Source.UnqueueProcessedBuffers();

                // Try turning it off again?
                this.Source.Stop();

                this.empty();
            }
        }

        private void open(bool precache = false)
        {
            this.underlyingStream.Seek(0, SeekOrigin.Begin);
            this.Reader = new VorbisReader(this.underlyingStream, false);

            if (precache)
            {
                // Fill first buffer synchronously
                OggStreamer.Instance.FillBuffer(this, this.Buffer.Handles[0]);
                this.Source.QueueBuffer(this.Buffer.Handles[0]);

                // Schedule the others asynchronously
                OggStreamer.Instance.AddStream(this);
            }

            this.Ready = true;
        }

        private void close()
        {
            if (this.Reader != null)
            {
                this.Reader.Dispose();
                this.Reader = null;
            }
            this.Ready = false;
        }
    }

    /// <summary>
    /// The manager that performs all the actual streaming.
    /// </summary>
    public class OggStreamer : IDisposable
    {
        private const float defaultUpdateRate = 10;
        private const int defaultBufferSize = 44100;

        private static readonly object singletonMutex = new object();

        private readonly object iterationMutex = new object();
        private readonly object readMutex = new object();

        private readonly float[] readSampleBuffer;
        private readonly short[] castBuffer;

        private readonly HashSet<OggStream> streams = new HashSet<OggStream>();
        private readonly List<OggStream> threadLocalStreams = new List<OggStream>();

        private Thread underlyingThread;
        private volatile bool cancelled;

        /// <summary>
        /// The amount of times per second the streamer checks all the streams.
        /// </summary>
        public float UpdateRate { get; private set; }
        /// <summary>
        /// The buffer size.
        /// </summary>
        public int BufferSize { get; private set; }

        private static OggStreamer instance;
        /// <summary>
        /// The singleton instance of the OggStreamer.
        /// </summary>
        public static OggStreamer Instance
        {
            get { lock (OggStreamer.singletonMutex) return OggStreamer.instance; }
        }

        /// <summary>
        /// Constructs an OggStreamer that plays ogg files in the background
        /// </summary>
        /// <param name="bufferSize">Buffer size</param>
        /// <param name="updateRate">Number of times per second to update</param>
        /// <param name="internalThread">True to use an internal thread, false to use your own thread, in which case use must call EnsureBuffersFilled periodically</param>
        public static void Initialize(int bufferSize = OggStreamer.defaultBufferSize,
            float updateRate = OggStreamer.defaultUpdateRate, bool internalThread = true)
        {
            lock (OggStreamer.singletonMutex)
            {
                OggStreamer.instance = new OggStreamer(bufferSize, updateRate, internalThread);
            }
        }

        /// <summary>
        /// Dispose the singleton instance.
        /// </summary>
        public static void DisposeInstance()
        {
            OggStreamer.instance.Dispose();
            OggStreamer.instance = null;
        }

        private OggStreamer(int bufferSize, float updateRate, bool internalThread)
        {
            if (internalThread)
            {
                this.underlyingThread = new Thread(this.ensureBuffersFilled) { Priority = ThreadPriority.Lowest };
                this.underlyingThread.Start();
            }
            else
            {
                // no need for this, user is in charge
                updateRate = 0;
            }

            this.UpdateRate = updateRate;
            this.BufferSize = bufferSize;

            this.readSampleBuffer = new float[bufferSize];
            this.castBuffer = new short[bufferSize];
        }

        /// <summary>
        /// Disposes the oggstreamer instance.
        /// </summary>
        public void Dispose()
        {
            lock (OggStreamer.singletonMutex)
            {
                this.cancelled = true;
                lock (this.iterationMutex)
                    this.streams.Clear();
                this.underlyingThread = null;
            }
        }

        /// <summary>
        /// Adds a new stream to be manager by the streamer.
        /// </summary>
        /// <param name="stream">The new stream to be managed.</param>
        /// <returns>Whether the stream was added succesfully.</returns>
        public bool AddStream(OggStream stream)
        {
            lock (this.iterationMutex)
                return this.streams.Add(stream);
        }

        /// <summary>
        /// Removes a stream from the manager.
        /// </summary>
        /// <param name="stream">The stream to be removed.</param>
        /// <returns>Whether the stream was removed succesfully.</returns>
        public bool RemoveStream(OggStream stream)
        {
            lock (this.iterationMutex)
                return this.streams.Remove(stream);
        }

        /// <summary>
        /// Fills the buffers of the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="bufferId">The id of the buffer to be filled.</param>
        /// <returns></returns>
        public bool FillBuffer(OggStream stream, int bufferId)
        {
            int readSamples;
            lock (this.readMutex)
            {
                readSamples = stream.Reader.ReadSamples(this.readSampleBuffer, 0, this.BufferSize);
                SoundBuffer.CastBuffer(this.readSampleBuffer, this.castBuffer, readSamples);
            }
            AL.BufferData(bufferId, stream.Reader.Channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16, this.castBuffer,
                          readSamples * sizeof(short), stream.Reader.SampleRate);
            ALHelper.Check();

            return readSamples != this.BufferSize;
        }

        /// <summary>
        /// Ensures that all the buffers of all streams are properly filled.
        /// </summary>
        private void ensureBuffersFilled()
        {
            do
            {
                this.threadLocalStreams.Clear();
                lock (this.iterationMutex) this.threadLocalStreams.AddRange(streams);

                foreach (var stream in this.threadLocalStreams)
                {
                    lock (stream.PrepareMutex)
                    {
                        lock (this.iterationMutex)
                            if (!this.streams.Contains(stream))
                                continue;

                        if (stream.Source.Disposed)
                            continue;

                        bool finished = false;

                        int queued = stream.Source.QueuedBuffers;
                        int processed = stream.Source.ProcessedBuffers;

                        // When there are no unprocessed buffers and the buffer fully filled, no need to do anything.
                        if (processed == 0 && queued == stream.BufferCount) continue;

                        int[] tempBuffers;
                        if (processed > 0)
                            tempBuffers = stream.Source.UnqueueProcessedBuffers();
                        else
                            tempBuffers = stream.Buffer.Handles.Skip(queued).ToArray();

                        int bufIdx = 0;
                        for (; bufIdx < tempBuffers.Length; bufIdx++)
                        {
                            finished |= this.FillBuffer(stream, tempBuffers[bufIdx]);

                            if (!finished) continue;

                            if (stream.IsLooped)
                            {
                                stream.Reader.DecodedTime = TimeSpan.Zero;
                                if (bufIdx == 0)
                                {
                                    // we didn't have any buffers left over, so let's start from the beginning on the next cycle...
                                }
                            }
                            else
                            {
                                lock (stream.StopMutex) stream.NotifyFinished();
                                this.streams.Remove(stream);
                                break;
                            }
                        }

                        stream.Source.QueueBuffers(bufIdx, tempBuffers);

                        if (finished && !stream.IsLooped)
                            continue;
                    }

                    lock (stream.StopMutex)
                    {
                        if (stream.Preparing) continue;

                        lock (this.iterationMutex)
                            if (!this.streams.Contains(stream))
                                continue;

                        var state = stream.Source.State;
                        if (state == ALSourceState.Stopped)
                            stream.Source.Play();
                    }
                }

                if (this.UpdateRate > 0)
                {
                    Thread.Sleep((int)(1000 / this.UpdateRate));
                }
            }
            while (this.underlyingThread != null && !this.cancelled);
        }
    }
}
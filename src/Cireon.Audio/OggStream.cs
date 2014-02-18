using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NVorbis;
using OpenTK.Audio.OpenAL;

namespace Cireon.Audio
{
    public class OggStream : IDisposable
    {
        private const int DefaultBufferCount = 3;

        internal readonly object StopMutex = new object();
        internal readonly object PrepareMutex = new object();

        public readonly Source Source;
        internal readonly SoundBuffer Buffer;

        private readonly Stream underlyingStream;

        internal VorbisReader Reader;
        public bool Ready { get; private set; }
        internal bool Preparing { get; private set; }

        public int BufferCount { get; private set; }

        public EventHandler Finished;

        public float Volume
        {
            get { return this.Source.Volume; }
            set { this.Source.Volume = value; }
        }

        public float Pitch
        {
            get { return this.Source.Pitch; }
            set { this.Source.Pitch = value; }
        }

        public bool IsLooped { get; set; }

        public OggStream(string filename, int bufferCount = DefaultBufferCount) : this(File.OpenRead(filename), bufferCount) { }
        public OggStream(Stream stream, int bufferCount = DefaultBufferCount)
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

            this.underlyingStream = stream;
        }

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

                if (!this.Ready)
                {
                    lock (this.PrepareMutex)
                    {
                        this.Preparing = true;
                        this.Open(true);
                    }
                }
            }
        }

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

        public void Pause()
        {
            if (this.Source.State != ALSourceState.Playing)
                return;

            OggStreamer.Instance.RemoveStream(this);
            this.Source.Pause();
        }

        public void Resume()
        {
            if (this.Source.State != ALSourceState.Paused)
                return;

            OggStreamer.Instance.AddStream(this);
            this.Source.Play();
        }

        public void Stop()
        {
            var state = this.Source.State;
            if (state == ALSourceState.Playing || state == ALSourceState.Paused)
            {
                this.stopPlayback();
            }

            lock (this.StopMutex)
            {
                this.NotifyFinished();
                OggStreamer.Instance.RemoveStream(this);
            }
        }

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

                this.Close();

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
            if (queued > 0)
            {
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
        }

        internal void Open(bool precache = false)
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

        internal void Close()
        {
            if (this.Reader != null)
            {
                this.Reader.Dispose();
                this.Reader = null;
            }
            this.Ready = false;
        }
    }

    public class OggStreamer : IDisposable
    {
        private const float DefaultUpdateRate = 10;
        private const int DefaultBufferSize = 44100;

        private static readonly object singletonMutex = new object();

        private readonly object iterationMutex = new object();
        private readonly object readMutex = new object();

        private readonly float[] readSampleBuffer;
        private readonly short[] castBuffer;

        private readonly HashSet<OggStream> streams = new HashSet<OggStream>();
        private readonly List<OggStream> threadLocalStreams = new List<OggStream>();

        private Thread underlyingThread;
        private volatile bool cancelled;

        public float UpdateRate { get; private set; }
        public int BufferSize { get; private set; }

        private static OggStreamer instance;
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
        public static void Initialize(int bufferSize = OggStreamer.DefaultBufferSize,
            float updateRate = OggStreamer.DefaultUpdateRate, bool internalThread = true)
        {
            lock (OggStreamer.singletonMutex)
            {
                OggStreamer.instance = new OggStreamer(bufferSize, updateRate, internalThread);
            }
        }

        public static void DisposeInstance()
        {
            OggStreamer.instance.Dispose();
            OggStreamer.instance = null;
        }

        private OggStreamer(int bufferSize, float updateRate, bool internalThread)
        {
            if (internalThread)
            {
                this.underlyingThread = new Thread(this.EnsureBuffersFilled) { Priority = ThreadPriority.Lowest };
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

        public bool AddStream(OggStream stream)
        {
            lock (this.iterationMutex)
                return this.streams.Add(stream);
        }
        public bool RemoveStream(OggStream stream)
        {
            lock (this.iterationMutex)
                return this.streams.Remove(stream);
        }

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

        public void EnsureBuffersFilled()
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

                            if (finished)
                            {
                                if (stream.IsLooped)
                                {
                                    stream.Reader.DecodedTime = TimeSpan.Zero;
                                    if (bufIdx == 0)
                                    {
                                        // we didn't have any buffers left over, so let's start from the beginning on the next cycle...
                                        continue;
                                    }
                                }
                                else
                                {
                                    lock (stream.StopMutex) stream.NotifyFinished();
                                    this.streams.Remove(stream);
                                    break;
                                }
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
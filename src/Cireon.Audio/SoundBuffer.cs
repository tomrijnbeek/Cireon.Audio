using System;
using System.Collections.Generic;
using System.IO;
using NVorbis;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL;

namespace Cireon.Audio
{
    /// <summary>
    /// A wrapper class for a set of audiobuffers.
    /// </summary>
    public class SoundBuffer : IDisposable
    {
        /// <summary>
        /// Disposal state of this buffer.
        /// </summary>
        public bool Disposed { get; private set; }

        /// <summary>
        /// List of OpenAL buffer handles.
        /// </summary>
        public readonly int[] Handles;

        /// <summary>
        /// Generates a new sound buffer of the given size.
        /// </summary>
        /// <param name="amount">The amount of buffers to reserve.</param>
        public SoundBuffer(int amount)
        {
            this.Handles = ALHelper.Eval(AL.GenBuffers, amount);
        }

        /// <summary>
        /// Generates a news sound buffer and fills it.
        /// </summary>
        /// <param name="buffers">The content of the buffers.</param>
        /// <param name="format">The format the buffers are in.</param>
        /// <param name="sampleRate">The samplerate of the buffers.</param>
        public SoundBuffer(IList<short[]> buffers, ALFormat format, int sampleRate)
            : this(buffers.Count)
        {
            this.FillBuffer(buffers, format, sampleRate);
        }

        /// <summary>
        /// Fills the buffer with new data.
        /// </summary>
        /// <param name="index">The starting index from where to fill the buffer.</param>
        /// <param name="data">The new content of the buffers.</param>
        /// <param name="format">The format the buffers are in.</param>
        /// <param name="sampleRate">The samplerate of the buffers.</param>
        public void FillBuffer(int index, short[] data, ALFormat format, int sampleRate)
        {
            if (index < 0 || index >= this.Handles.Length)
                throw new ArgumentOutOfRangeException("index");

            ALHelper.Call(() => AL.BufferData(this.Handles[index], format, data, data.Length * sizeof(short), sampleRate));
        }

        /// <summary>
        /// Fills the buffer with new data.
        /// </summary>
        /// <param name="data">The new content of the buffers.</param>
        /// <param name="format">The format the buffers are in.</param>
        /// <param name="sampleRate">The samplerate of the buffers.</param>
        public void FillBuffer(IList<short[]> data, ALFormat format, int sampleRate)
        {
            this.FillBuffer(0, data, format, sampleRate);
        }

        /// <summary>
        /// Fills the buffer with new data.
        /// </summary>
        /// <param name="index">The starting index from where to fill the buffer.</param>
        /// <param name="data">The new content of the buffers.</param>
        /// <param name="format">The format the buffers are in.</param>
        /// <param name="sampleRate">The samplerate of the buffers.</param>
        public void FillBuffer(int index, IList<short[]> data, ALFormat format, int sampleRate)
        {
            if (index < 0 || index >= this.Handles.Length)
                throw new ArgumentOutOfRangeException("index");
            if (data.Count > this.Handles.Length)
                throw new ArgumentException("This data does not fit in the buffer.", "data");

            for (int i = 0; i < data.Count; i++)
                this.FillBuffer((index + i) % this.Handles.Length, data[i], format, sampleRate);
        }

        /// <summary>
        /// Disposes the buffer.
        /// </summary>
        public void Dispose()
        {
            if (this.Disposed)
                return;

            ALHelper.Call(AL.DeleteBuffers, this.Handles);

            this.Disposed = true;
        }

        /// <summary>
        /// Creates a new soundbuffer from preloaded sound data.
        /// </summary>
        /// <returns>A SoundBuffer object containing the specified sound data.</returns>
        /// <param name="data">The preloaded sound data.</param>
        public static SoundBuffer FromData(SoundBufferData data)
        {
            return new SoundBuffer(data.Buffers, data.Format, data.SampleRate);
        }

        /// <summary>
        /// Creates a new soundbuffer from an ogg-file.
        /// </summary>
        /// <param name="file">The file to load the data from.</param>
        /// <returns>A SoundBuffer object containing the data from the specified file.</returns>
        public static SoundBuffer FromOgg(string file)
        {
            return SoundBuffer.FromOgg(File.OpenRead(file));
        }

        /// <summary>
        /// Creates a new soundbuffer from an ogg-file.
        /// </summary>
        /// <param name="file">The file to load the data from.</param>
        /// <returns>A SoundBuffer object containing the data from the specified file.</returns>
        public static SoundBuffer FromOgg(Stream file)
        {
            return SoundBuffer.FromData(SoundBufferData.FromOgg(file));
        }

        /// <summary>
        /// Creates a new soundbuffer from an uncompressed wave-file.
        /// </summary>
        /// <param name="file">The file to load the data from.</param>
        /// <returns>A SoundBuffer object containing the data from the specified file.</returns>
        public static SoundBuffer FromWav(string file)
        {
            return SoundBuffer.FromWav(File.OpenRead(file));
        }

        /// <summary>
        /// Creates a new soundbuffer from an uncompressed wave-file.
        /// </summary>
        /// <param name="file">The file to load the data from.</param>
        /// <returns>A SoundBuffer object containing the data from the specified file.</returns>
        public static SoundBuffer FromWav(Stream file)
        {
            return SoundBuffer.FromData(SoundBufferData.FromWav(file));
        }

        /// <summary>
        /// Casts the buffer read by the vorbis reader to an Int16 buffer.
        /// </summary>
        /// <param name="inBuffer">The buffer as read by the vorbis reader.</param>
        /// <param name="outBuffer">A reference to the output buffer.</param>
        /// <param name="length">The length of the buffer.</param>
        [Obsolete("This method is obsolete. Please use SoundBufferData.CastBuffer() instead.")]
        public static void CastBuffer(float[] inBuffer, short[] outBuffer, int length)
        {
            SoundBufferData.CastBuffer(inBuffer, outBuffer, length);
        }

        /// <summary>
        /// Casts the buffer to an integer array.
        /// </summary>
        /// <param name="buffer">The buffer that should be casted.</param>
        /// <returns>The OpenAL handles of the buffers.</returns>
        static public implicit operator int[](SoundBuffer buffer)
        {
            return buffer.Handles;
        }
    }
}

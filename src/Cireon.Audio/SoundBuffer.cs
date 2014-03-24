using System;
using System.Collections.Generic;
using System.IO;
using NVorbis;
using OpenTK.Audio.OpenAL;

namespace Cireon.Audio
{
    public class SoundBuffer
    {
        /// <summary>
        /// List of OpenAL buffer handles.
        /// </summary>
        public readonly int[] Handles;

        public SoundBuffer(int amount)
        {
            this.Handles = AL.GenBuffers(amount);
            ALHelper.Check();
        }

        public SoundBuffer(IList<short[]> buffers, ALFormat format, int sampleRate)
            : this(buffers.Count)
        {
            this.FillBuffer(buffers, format, sampleRate);
        }

        public void FillBuffer(int index, short[] data, ALFormat format, int sampleRate)
        {
            if (index < 0 || index >= this.Handles.Length)
                throw new ArgumentOutOfRangeException("index");

            AL.BufferData(this.Handles[index], format, data, data.Length * sizeof(short), sampleRate);
            ALHelper.Check();
        }

        public void FillBuffer(IList<short[]> data, ALFormat format, int sampleRate)
        {
            this.FillBuffer(0, data, format, sampleRate);
        }

        public void FillBuffer(int index, IList<short[]> data, ALFormat format, int sampleRate)
        {
            if (index < 0 || index >= this.Handles.Length)
                throw new ArgumentOutOfRangeException("index");
            if (data.Count > this.Handles.Length)
                throw new ArgumentException("This data does not fit in the buffer.", "data");

            for (int i = 0; i < data.Count; i++)
                this.FillBuffer((index + i) % this.Handles.Length, data[i], format, sampleRate);
        }

        public void Dispose()
        {
            AL.DeleteBuffers(this.Handles);
            ALHelper.Check();
        }

        public static SoundBuffer FromFile(string file)
        {
            return SoundBuffer.FromFile(File.OpenRead(file));
        }

        public static SoundBuffer FromFile(Stream file)
        {
            var buffers = new List<short[]>();

            ALFormat format;
            int sampleRate;

            using (var vorbis = new VorbisReader(file, true))
            {
                // Save format and samplerate for playback
                format = vorbis.Channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
                sampleRate = vorbis.SampleRate;

                var buffer = new float[16384];
                int count;

                while ((count = vorbis.ReadSamples(buffer, 0, buffer.Length)) > 0)
                {
                    // Sample value range is -0.99999994f to 0.99999994f
                    // Samples are interleaved (chan0, chan1, chan0, chan1, etc.)

                    // Use the OggStreamer method to cast to the right format
                    var castBuffer = new short[16384];
                    SoundBuffer.CastBuffer(buffer, castBuffer, count);
                    buffers.Add(castBuffer);
                }
            }

            return new SoundBuffer(buffers, format, sampleRate);
        }

        /// <summary>
        /// Casts the buffer read by the vorbis reader to an Int16 buffer.
        /// </summary>
        /// <param name="inBuffer">The buffer as read by the vorbis reader.</param>
        /// <param name="outBuffer">A reference to the output buffer.</param>
        /// <param name="length">The length of the buffer.</param>
        public static void CastBuffer(float[] inBuffer, short[] outBuffer, int length)
        {
            for (int i = 0; i < length; i++)
            {
                var temp = (int)(32767f * inBuffer[i]);
                if (temp > Int16.MaxValue) temp = Int16.MaxValue;
                else if (temp < Int16.MinValue) temp = Int16.MinValue;
                outBuffer[i] = (short)temp;
            }
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

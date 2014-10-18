using System;
using System.Collections.Generic;
using System.IO;
using NVorbis;
using OpenTK.Audio.OpenAL;

namespace Cireon.Audio
{
    /// <summary>
    /// A wrapper class containing the extracted and/or decoded data of a sound.
    /// </summary>
    public class SoundBufferData
    {
        #region Members
        private readonly IList<short[]> buffers;
        private readonly ALFormat format;
        private readonly int sampleRate;

        internal IList<short[]> Buffers { get { return this.buffers;} }
        internal ALFormat Format { get { return this.format; } }
        internal int SampleRate { get { return this.sampleRate; } }
        #endregion

        #region Constructor
        private SoundBufferData(IList<short[]> buffers, ALFormat format, int sampleRate)
        {
            this.buffers = buffers;
            this.format = format;
            this.sampleRate = sampleRate;
        }
        #endregion

        #region Static creation functions
        /// <summary>
        /// Extract the bifferdata from an ogg-file.
        /// </summary>
        /// <param name="file">The file to load the data from.</param>
        /// <returns>A SoundBufferData object containing the data from the specified file.</returns>
        public static SoundBufferData FromOgg(Stream file)
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
                    var castBuffer = new short[count];
                    SoundBufferData.CastBuffer(buffer, castBuffer, count);
                    buffers.Add(castBuffer);
                }
            }

            return new SoundBufferData(buffers, format, sampleRate);
        }

        /// <summary>
        /// Extracts the bufferdata from an uncompressed wave-file.
        /// </summary>
        /// <param name="file">The file to load the data from.</param>
        /// <returns>A SoundBufferData object containing the data from the specified file.</returns>
        public static SoundBufferData FromWav(Stream file)
        {
            using (var reader = new BinaryReader(file))
            {
                // RIFF header
                var signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riffChunckSize = reader.ReadInt32();

                var format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                var formatSignature = new string(reader.ReadChars(4));
                if (formatSignature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int formatChunkSize = reader.ReadInt32();
                int audioFormat = reader.ReadInt16();
                int numChannels = reader.ReadInt16();
                int sampleRate = reader.ReadInt32();
                int byteRate = reader.ReadInt32();
                int blockAlign = reader.ReadInt16();
                int bitsPerSample = reader.ReadInt16();

                if (formatChunkSize > 16)
                    reader.ReadBytes(formatChunkSize - 16);

                var dataSignature = new string(reader.ReadChars(4));

                if (dataSignature != "data")
                    throw new NotSupportedException("Only uncompressed wave files are supported.");

                int dataChunkSize = reader.ReadInt32();

                var alFormat = SoundBufferData.getSoundFormat(numChannels, bitsPerSample);

                var data = reader.ReadBytes((int)reader.BaseStream.Length);
                var buffers = new List<short[]>();
                int count;
                int i = 0;
                const int bufferSize = 16384;

                while ((count = (Math.Min(data.Length, (i + 1) * bufferSize * 2) - i * bufferSize * 2) / 2) > 0)
                {
                    var buffer = new short[bufferSize];
                    SoundBufferData.convertBuffer(data, buffer, count, i * bufferSize * 2);
                    buffers.Add(buffer);
                    i++;
                }

                return new SoundBufferData(buffers, alFormat, sampleRate);
            }
        }
        #endregion

        #region Static helper functions
        private static void convertBuffer(byte[] inBuffer, short[] outBuffer, int length, int inOffset = 0)
        {
            for (int i = 0; i < length; i++)
                outBuffer[i] = BitConverter.ToInt16(inBuffer, inOffset + 2 * i);
        }

        private static ALFormat getSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
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
                outBuffer[i] = (short) temp;
            }
        }
        #endregion
    }
}


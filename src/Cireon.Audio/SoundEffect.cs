using System;
using System.Collections.Generic;
using NVorbis;
using NVorbis.OpenTKSupport;
using OpenTK.Audio.OpenAL;

namespace BeardGame.Audio
{
    sealed class SoundEffect
    {
        private readonly int[] bufferIDs;

        private readonly ALFormat format;
        private readonly int sampleRate;

        public SoundEffect(string file)
        {
            var buffers = new List<short[]>();

            using (var vorbis = new VorbisReader(file))
            {
                // Save format and samplerate for playback
                this.format = vorbis.Channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
                this.sampleRate = vorbis.SampleRate;

                var buffer = new float[16384];
                int count;

                while ((count = vorbis.ReadSamples(buffer, 0, buffer.Length)) > 0)
                {
                    // Sample value range is -0.99999994f to 0.99999994f
                    // Samples are interleaved (chan0, chan1, chan0, chan1, etc.)
                    
                    // Use the OggStreamer method to cast to the right format
                    var castBuffer = new short[16384];
                    OggStreamer.CastBuffer(buffer, castBuffer, count);
                    buffers.Add(castBuffer);
                }
            }

            // Prefill AL buffers
            this.bufferIDs = AL.GenBuffers(buffers.Count);
            for (int i = 0; i < buffers.Count; i++)
            {
                AL.BufferData(bufferIDs[i], this.format, buffers[i], buffers[i].Length * sizeof(short), this.sampleRate);
                ALHelper.Check();
            }
        }

        public void Play()
        {
            // Bind the buffers to a source and play it
            int sourceID = AudioManager.Instance.SourceManager.RequestSourceHandle();
            AL.SourceQueueBuffers(sourceID, this.bufferIDs.Length, this.bufferIDs);
            AL.Source(sourceID, ALSourcef.Gain, AudioManager.Instance.EffectsVolume);
            AL.SourcePlay(sourceID);
            ALHelper.Check();
        }
    }
}

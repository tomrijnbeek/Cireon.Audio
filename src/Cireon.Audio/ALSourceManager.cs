using System;
using System.Collections.Generic;
using OpenTK.Audio.OpenAL;

namespace Cireon.Audio
{
    public class ALSourceManager
    {
        private readonly List<int> sourceHandles = new List<int>();

        public ALSourceManager() { }

        public int RequestSourceHandle()
        {
            int sourceHandle = AL.GenSource();
            this.sourceHandles.Add(sourceHandle);
            ALHelper.Check();
            return sourceHandle;
        }

        public void Update()
        {
            List<int> finishedSources = new List<int>(this.sourceHandles.Count);

            foreach (int i in this.sourceHandles)
            {
                int queuedBuffers;
                AL.GetSource(i, ALGetSourcei.BuffersQueued, out queuedBuffers);

                int processedBuffers;
                AL.GetSource(i, ALGetSourcei.BuffersProcessed, out processedBuffers);

                // Finished playing, clear up the source handle
                if (queuedBuffers <= processedBuffers)
                {
                    AL.SourceStop(i);
                    AL.DeleteSource(i);
                    finishedSources.Add(i);

                    ALHelper.Check();
                }
            }

            foreach (int i in finishedSources)
                this.sourceHandles.Remove(i);
        }

        public void Dispose()
        {
            foreach (int i in this.sourceHandles)
            {
                AL.SourceStop(i);
                AL.DeleteSource(i);

                ALHelper.Check();
            }

            this.sourceHandles.Clear();
        }
    }
}

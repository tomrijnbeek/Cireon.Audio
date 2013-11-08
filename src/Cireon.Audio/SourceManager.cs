using System;
using System.Collections.Generic;
using OpenTK.Audio.OpenAL;

namespace Cireon.Audio
{
    public sealed class SourceManager
    {
        private readonly List<Source> sources = new List<Source>();

        public SourceManager() { }

        public Source RequestSource()
        {
            Source s = new Source();
            this.sources.Add(s);
            return s;
        }

        public void Update()
        {
            var finishedSources = new List<Source>(this.sources.Count);

            foreach (var s in this.sources)
            {
                // Finished playing, clear up the source handle
                if (s.FinishedPlaying)
                {
                    s.Dispose();
                    finishedSources.Add(s);
                }
            }

            foreach (var s in finishedSources)
                this.sources.Remove(s);
        }

        public void Dispose()
        {
            foreach (var s in this.sources)
                s.Dispose();

            this.sources.Clear();
        }
    }
}

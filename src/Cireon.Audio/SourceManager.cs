using System;
using System.Collections.Generic;
using OpenTK.Audio.OpenAL;

namespace Cireon.Audio
{
    /// <summary>
    /// Manager of all active sources.
    /// </summary>
    public sealed class SourceManager
    {
        private readonly List<Source> sources = new List<Source>();

        /// <summary>
        /// 
        /// </summary>
        public SourceManager() { }

        /// <summary>
        /// Allocates a new source.
        /// </summary>
        /// <returns>The source wrapper for the allocated source.</returns>
        public Source RequestSource()
        {
            Source s = new Source();
            this.sources.Add(s);
            return s;
        }

        /// <summary>
        /// Updates all sources.
        /// </summary>
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

        /// <summary>
        /// Disposes all sources.
        /// </summary>
        public void Dispose()
        {
            foreach (var s in this.sources)
                s.Dispose();

            this.sources.Clear();
        }
    }
}

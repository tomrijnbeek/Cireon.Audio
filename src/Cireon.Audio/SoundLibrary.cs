using System;
using System.Collections.Generic;
using System.IO;

namespace Cireon.Audio
{
    /// <summary>
    /// A collection of pre-buffered sound effects.
    /// </summary>
    public sealed class SoundLibrary
    {
        private readonly Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sfxPath">Path containing all soundeffects that should be pre-buffered.</param>
        public SoundLibrary(string sfxPath)
        {
            if (!Directory.Exists(sfxPath))
            {
                Console.WriteLine("Warning: soundeffects folder missing");
                return;
            }

            var files = Directory.GetFiles(sfxPath, "*.ogg", SearchOption.AllDirectories);
            foreach (var file in files)
                this.soundEffects.Add(Path.GetFileNameWithoutExtension(file), new SoundEffect(file));
        }

        /// <summary>
        /// Accesses a sound effect.
        /// </summary>
        /// <param name="name">The name of the sound effect.</param>
        /// <returns>The SoundEffect corresponding to the given name.</returns>
        public SoundEffect this[string name]
        {
            get
            {
                if (this.soundEffects.ContainsKey(name))
                    return this.soundEffects[name];
                else
                {
                    Console.WriteLine("Warning: sound effect \"{0}\" not found.", name);
                    return null;
                }
            }
        }
    }
}

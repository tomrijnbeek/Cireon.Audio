using System;
using System.Collections.Generic;
using System.IO;

namespace Cireon.Audio
{
    sealed class SoundLibrary
    {
        private readonly Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();

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

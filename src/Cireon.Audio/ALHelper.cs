using System;
using OpenTK.Audio.OpenAL;

namespace Cireon.Audio
{
    static class ALHelper
    {
        public static readonly XRamExtension XRam = new XRamExtension();
        public static readonly EffectsExtension Efx = new EffectsExtension();

        public static void Check()
        {
            ALError error;
            if ((error = AL.GetError()) != ALError.NoError)
                throw new InvalidOperationException(AL.GetErrorString(error));
        }
    }
}

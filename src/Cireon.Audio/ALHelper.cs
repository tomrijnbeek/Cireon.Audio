using System;
using OpenTK.Audio.OpenAL;

namespace Cireon.Audio
{
    /// <summary>
    /// OpenAL Helper class.
    /// </summary>
    public static class ALHelper
    {
        /// <summary>
        /// Checks whether OpenAL has thrown an error and throws an exception if so.
        /// </summary>
        public static void Check()
        {
#if DEBUG
            ALError error;
            if ((error = AL.GetError()) != ALError.NoError)
                throw new InvalidOperationException(AL.GetErrorString(error));
#endif
        }
    }
}

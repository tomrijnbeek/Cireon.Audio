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

        /// <summary>
        /// Calls a function and then checks for an OpenAL error.
        /// </summary>
        /// <param name="function">The function to be called.</param>
        public static void Call(Action function)
        {
            function();
            ALHelper.Check();
        }

        /// <summary>
        /// Calls a function and then checks for an OpenAL error.
        /// </summary>
        /// <typeparam name="TParameter">The type of the parameter of the function.</typeparam>
        /// <param name="function">The function to be called.</param>
        /// <param name="parameter">The parameter to be passed to the function.</param>
        public static void Call<TParameter>(Action<TParameter> function, TParameter parameter)
        {
            function(parameter);
            ALHelper.Check();
        }
    }
}

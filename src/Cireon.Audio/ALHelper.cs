﻿using System;
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
        public static void Call(this Action function)
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
        public static void Call<TParameter>(this Action<TParameter> function, TParameter parameter)
        {
            function(parameter);
            ALHelper.Check();
        }

        /// <summary>
        /// Evaluates a function and then checks for an OpenAL error.
        /// </summary>
        /// <param name="function">The function to be evaluated.</param>
        /// <typeparam name="TReturn">The type of the return value.</typeparam>
        public static TReturn Eval<TReturn>(this Func<TReturn> function)
        {
            var val = function();
            ALHelper.Check();
            return val;
        }

        /// <summary>
        /// Evaluates a function and then checks for an OpenAL error.
        /// </summary>
        /// <param name="function">The function to be evaluated.</param>
        /// <param name="parameter">The type of the parameter of the function.</param>
        /// <typeparam name="TParameter">The type of the parameter of the function.</typeparam>
        /// <typeparam name="TReturn">The type of the return value.</typeparam>
        public static TReturn Eval<TParameter, TReturn>(this Func<TParameter, TReturn> function, TParameter parameter)
        {
            var val = function(parameter);
            ALHelper.Check();
            return val;
        }
    }
}

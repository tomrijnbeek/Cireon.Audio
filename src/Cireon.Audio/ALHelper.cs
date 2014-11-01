using System;
using System.IO;
using OpenTK.Audio.OpenAL;

namespace Cireon.Audio
{
    /// <summary>
    /// OpenAL Helper class.
    /// </summary>
    public static class ALHelper
    {
        private const string logFile = "audio.log";

        /// <summary>
        /// The ways in which the ALHelper can handle errors when it encounters one.
        /// </summary>
        [Flags]
        public enum ALErrorHandlingBehaviour
        {
            /// <summary>
            /// Do nothing.
            /// </summary>
            None = 0,
            /// <summary>
            /// Output the error to the console.
            /// </summary>
            Console = 1,
            /// <summary>
            /// Output the error to a file (audio.log).
            /// </summary>
            File = 2,
            /// <summary>
            /// Throw an exception.
            /// </summary>
            Exception = 4
        }

        private static ALErrorHandlingBehaviour errorHandlingBehaviour = ALErrorHandlingBehaviour.Console;
        
        /// <summary>
        /// Sets the behaviour of the OpenAL helper when it encounters an error.
        /// </summary>
        /// <param name="behaviour">The behaviour.</param>
        public static void SetErrorHandlingBehaviour(ALErrorHandlingBehaviour behaviour)
        {
            ALHelper.errorHandlingBehaviour = behaviour;
        }

        /// <summary>
        /// Checks whether OpenAL has thrown an error and throws an exception if so.
        /// </summary>
        public static void Check()
        {
            ALError error;
            if ((error = AL.GetError()) == ALError.NoError)
                return;

            string errorString = AL.GetErrorString(error);

            if (ALHelper.errorHandlingBehaviour.HasFlag(ALErrorHandlingBehaviour.Console))
            {
                Console.WriteLine("Audio error: {0} {1}", errorString, Environment.StackTrace);
            }

            if (ALHelper.errorHandlingBehaviour.HasFlag(ALErrorHandlingBehaviour.File))
            {
                if (!File.Exists(ALHelper.logFile))
                    File.Create(ALHelper.logFile);
                File.AppendAllLines(ALHelper.logFile,
                    new[]
                    {
                        string.Format("[{0}] [error] {1} {2}", DateTime.Now.ToString("ddd MMM dd HH:mm:ss"), errorString,
                            Environment.StackTrace)
                    });
            }

            if (ALHelper.errorHandlingBehaviour.HasFlag(ALErrorHandlingBehaviour.Exception))
            {
                throw new InvalidOperationException(errorString);
            }
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

        /// <summary>
        /// Evaluates a function and then checks for an OpenAL error.
        /// </summary>
        /// <param name="function">The function to be evaluated.</param>
        /// <typeparam name="TReturn">The type of the return value.</typeparam>
        public static TReturn Eval<TReturn>(Func<TReturn> function)
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
        public static TReturn Eval<TParameter, TReturn>(Func<TParameter, TReturn> function, TParameter parameter)
        {
            var val = function(parameter);
            ALHelper.Check();
            return val;
        }
    }
}

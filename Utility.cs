namespace Bps.Common
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    public static class Utility
    {
        /// <summary>
        /// Disposes of the given object and sets it to null.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="obj">Specify the object to dispose of.</param>
        public static void DisposeOf<T>(ref T obj) where T: class , IDisposable
        {
            if (obj == null) return;

            obj.Dispose();
            obj = null;
        }

        /// <summary>
        /// Runs a given action safely within a try-catch block.
        /// </summary>
        /// <param name="action">Specify your action to run.</param>
        public static void RunSafely(Action action)
        {
            if (action == null) return;

            try
            {
                action();
            }
            catch (Exception) {; }
        }

        /// <summary>
        /// Runs a given action safely within a try-catch block and logs any possible exception.
        /// </summary>
        /// <param name="action">Specify your action to run.</param>
        public static void RunLoggedSafely(Action action)
        {
            if (action == null) return;

            try
            {
                action();
            }
            catch (Exception ex)
            {
                Exceptions.Handle(ex);
            }
        }

        /// <summary>
        /// Serializes an object to the specified stream.
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <typeparam name="output">Specify the output stream</typeparam>
        /// <param name="value">The object value.</param>
        public static void Serialize<T>(Stream output, T value)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(output, value);
        }

        /// <summary>
        /// De-serializes an string using the specified formatter.
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="input">The serialized input stream.</param>
        /// <returns>the de-serialized object</returns>
        public static T Deserialize<T>(Stream input) where T: class
        {
            var formatter = new BinaryFormatter();
            // De-serialize to an object of type T
            return formatter.Deserialize(input) as T;
        }
    }

}

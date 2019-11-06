namespace Bps.Common
{
    using System;

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
    }

}

namespace BPS.Common.Threading
{
    using System;
    using System.Threading;

    public class ThreadEvents : IDisposable
    {
        #region Fields

        private bool disposed;

        #endregion

        #region Constructor and Finalizer

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadEvents"/> class.
        /// </summary>
        public ThreadEvents()
        {
            CouldResume = new ManualResetEvent(true);
            ShouldStop = new ManualResetEvent(false);
            Stopped = new ManualResetEvent(false);
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                if (CouldResume != null)
                {
                    CouldResume.Dispose();
                    CouldResume = null;
                }
                if (ShouldStop != null)
                {
                    ShouldStop.Dispose();
                    ShouldStop = null;
                }
                if (Stopped != null)
                {
                    Stopped.Dispose();
                    Stopped = null;
                }
            }

            disposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #endregion

        #region Properties

        public ManualResetEvent CouldResume { get; protected set; }

        public ManualResetEvent ShouldStop { get; protected set; }

        public ManualResetEvent Stopped { get; protected set; }

        #endregion

    }
}
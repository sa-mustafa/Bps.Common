namespace Bps.Common.Threading
{
    using Bps.Common;
    using System;
    using System.Threading;

    public delegate bool SingleProcessorThreadStart();
    public delegate bool MultipleProcessorThreadStart(int index);

    /// <summary>
    /// Defines a single processing task running in a thread.
    /// </summary>
    public class Processor : IDisposable
    {
        #region Fields

        private bool disposed;
        private ProcessorInfo info;

        #endregion

        #region Constructor & Finalizer

        /// <summary>
        /// Initializes a new instance of the <see cref="Processor"/> class.
        /// </summary>
        /// <param name="name">The processor name.</param>
        /// <param name="index">The processor index.</param>
        public Processor(string name, int index = 0)
        {
            info = new ProcessorInfo(name, index);
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing) Utility.DisposeOf(ref info);
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

        /// <summary>
        /// Gets the processor name.
        /// </summary>
        public string Name { get { return info.Name; } }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Processor"/> is running.
        /// </summary>
        public bool Running
        {
            get
            {
                if (info == null || info.Thread == null)
                    return false;

                if (info.Thread.ThreadState != ThreadState.Running)
                    return false;

                return !info.Events.Stopped.WaitOne(0);
            }
        }

        /// <summary>
        /// Gets the managed thread identifier.
        /// </summary>
        public int ManagedThreadId => info.Thread.ManagedThreadId;

        #endregion

        #region Methods

        /// <summary>
        /// Checks whether the processor is allowed to resume.
        /// </summary>
        /// <param name="timeout">The timeout to wait.</param>
        /// <returns>True if the processor could resume; false otherwise.</returns>
        public bool CouldResume(TimeSpan? timeout) => info.Events.CouldResume.WaitOne(timeout ?? Timeout.InfiniteTimeSpan);

        /// <summary>
        /// Checks whether the processor should stop.
        /// </summary>
        /// <param name="timeout">The timeout to wait.</param>
        /// <returns>True if the processor should stop; false otherwise.</returns>
        public bool ShouldStop(TimeSpan? timeout) => info.Events.ShouldStop.WaitOne(timeout ?? Timeout.InfiniteTimeSpan);

        /// <summary>
        /// Finishes the processor thread gracefully.
        /// </summary>
        /// <param name="result">Specify the thread return value.</param>
        /// <returns>Returns the given result.</returns>
        public bool Finish(bool result)
        {
            info.Events.Stopped.Set();
            return result;
        }

        /// <summary>
        /// Pauses the processor thread.
        /// </summary>
        public void Pause() => info.Events.CouldResume.Reset();

        /// <summary>
        /// Removes the processor thread.
        /// </summary>
        /// <param name="timeout">The timeout to wait before resorting to abort.</param>
        /// <returns>True if the function succeeds; false otherwise.</returns>
        public bool Remove(TimeSpan? timeout)
        {
            try
            {
                info.Events.ShouldStop.Set();
                if (WaitToStop(timeout))
                    return true;

                info.Thread.Abort();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                info.Events.Stopped.Set();
            }
        }

        /// <summary>
        /// Resumes the processor thread.
        /// </summary>
        public void Resume() => info.Events.CouldResume.Set();

        /// <summary>
        /// Runs the processor thread.
        /// </summary>
        /// <param name="coreIndex">Index of the processor core; zero means all cores allowed.</param>
        /// <param name="priority">The thread priority.</param>
        /// <param name="threadProc">The thread procedure.</param>
        /// <param name="startPaused">Starts the thread in paused state.</param>
        /// <returns>True if the function succeeds; false otherwise.</returns>
        public bool Run(int coreIndex, ThreadPriority priority, Delegate threadProc, bool startPaused = false)
        {
            try
            {
                info.Set(coreIndex, priority, threadProc, new Thread(DefaultThreadStart));
                if (startPaused) info.Events.CouldResume.Set();
                info.Thread.Start(info);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Runs the processor thread.
        /// </summary>
        /// <param name="affinityMask">The thread affinity mask.</param>
        /// <param name="priority">The thread priority.</param>
        /// <param name="threadProc">The thread procedure.</param>
        /// <param name="startPaused">Starts the thread in paused state.</param>
        /// <returns>True if the function succeeds; false otherwise.</returns>
        public bool Run(UIntPtr affinityMask, ThreadPriority priority, Delegate threadProc, bool startPaused = false)
        {
            try
            {
                info.Set(affinityMask, priority, threadProc, new Thread(DefaultThreadStart));
                if (startPaused) info.Events.CouldResume.Set();
                info.Thread.Start(info);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Stops the processor thread.
        /// </summary>
        public void Stop() => info.Events.ShouldStop.Set();

        /// <summary>
        /// Waits for a specified time while checking for processor signals.
        /// </summary>
        /// <param name="timeout">Specify the timeout for idling period.</param>
        /// <param name="accuracySeconds">Specify the waiting time accuracy in seconds.</param>
        /// <returns>True if the idle period lapses; false if the processor receives stop signal.</returns>
        public bool WaitFor(TimeSpan? timeout, double accuracySeconds = 1)
        {
            var sleepTime = TimeSpan.FromSeconds(accuracySeconds * 0.5);
            var clocker = System.Diagnostics.Stopwatch.StartNew();
            while (clocker.Elapsed.CompareTo(timeout) < 0)
            {
                CouldResume(TimeSpan.Zero);
                if (ShouldStop(TimeSpan.Zero))
                    return false;
                Thread.Sleep(sleepTime);
            }
            return true;
        }

        /// <summary>
        /// Waits for a specified time while checking for processor signals and an optional predicate function.
        /// </summary>
        /// <param name="timeout">Specify the timeout for idling period.</param>
        /// <param name="Check">Specify a predicate function to check during the waiting period.</param>
        /// <param name="accuracySeconds">Specify the waiting time accuracy in seconds.</param>
        /// <returns>True if the idle period lapses without receiving stop signal or positive predicate function; false otherwise.</returns>
        public bool WaitFor(TimeSpan? timeout, Func<bool> Check, double accuracySeconds = 1)
        {
            if (Check == null) throw new ArgumentNullException(nameof(Check));
            var sleepTime = TimeSpan.FromSeconds(accuracySeconds * 0.5);
            var clocker = System.Diagnostics.Stopwatch.StartNew();
            while (clocker.Elapsed.CompareTo(timeout) < 0)
            {
                CouldResume(TimeSpan.Zero);
                if (ShouldStop(TimeSpan.Zero) || Check())
                    return false;
                Thread.Sleep(sleepTime);
            }
            return true;
        }

        /// <summary>
        /// Waits for a specified time for the processor to stop.
        /// </summary>
        /// <param name="timeout">The timeout to wait for the processor; specify null to wait indefinitely.</param>
        /// <returns>True if the processor is stopped; false when the processor is still running after the timeout.</returns>
        public bool WaitToStop(TimeSpan? timeout)
        {
            if (!Running) return true;
            return info.Events.Stopped.WaitOne(timeout ?? Timeout.InfiniteTimeSpan);
        }

        private static void DefaultThreadStart(object obj)
        {
            try
            {
                var pi = obj as ProcessorInfo;
                //Native.SetThreadAffinityMask(Native.GetCurrentThread(), pi.AffinityMask);
                Thread.CurrentThread.Priority = pi.Priority;

                // Run the single thread procedure.
                if (pi.ThreadProc is SingleProcessorThreadStart start1)
                    start1();
                else if (pi.ThreadProc is MultipleProcessorThreadStart start2)
                    start2(pi.Index);
            }
            catch (Exception ex)
            {
                Exceptions.Handle(ex);
            }
        }

        #endregion

    }
}
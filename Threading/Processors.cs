namespace BPS.Common.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Defines a group of processing tasks running as a bunch of threads.
    /// </summary>
    public class Processors : IDisposable
    {

        #region Fields

        private bool disposed;
        private readonly ThreadLocal<Processor> current;
        private readonly List<Processor> processorList;

        #endregion

        #region Constructor & Finalizer

        /// <summary>
        /// Initializes a new instance of the <see cref="Processors"/> class.
        /// </summary>
        /// <param name="name">The processor name.</param>
        public Processors(string name)
        {
            Name = name;
            current = new ThreadLocal<Processor>(Factory);
            processorList = new List<Processor>();
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                processorList.ForEach(proc => proc.Dispose());
                processorList.Clear();
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

        /// <summary>
        /// Gets the current processor in the context of running thread.
        /// </summary>
        public Processor Current
        {
            get
            {
                return current.Value;
            }
        }

        /// <summary>
        /// Gets the processor class name.
        /// </summary>
        public string Name { get; protected set; }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a processor thread.
        /// </summary>
        /// <param name="coreIndex">Index of the processor core; zero means all cores allowed.</param>
        /// <param name="priority">The thread priority.</param>
        /// <param name="threadProc">The thread procedure.</param>
        /// <param name="startPaused">Starts the thread in paused state.</param>
        /// <returns>True if the function succeeds; false otherwise.</returns>
        public bool Add(int coreIndex, ThreadPriority priority, MultipleProcessorThreadStart threadProc, bool startPaused = false)
        {
            Processor processor = null;
            try
            {
                processor = new Processor(Name, processorList.Count);
                processor.Run(coreIndex, priority, threadProc, startPaused);
                lock (processorList)
                {
                    processorList.Add(processor);
                }
            }
            catch (Exception)
            {
                processor?.Dispose();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Adds a processor thread.
        /// </summary>
        /// <param name="afinityMask">Index of the processor core.</param>
        /// <param name="priority">The thread priority.</param>
        /// <param name="threadProc">The thread procedure.</param>
        /// <param name="startPaused">Starts the thread in paused state.</param>
        /// <returns>True if the function succeeds; false otherwise.</returns>
        public bool Add(UIntPtr afinityMask, ThreadPriority priority, MultipleProcessorThreadStart threadProc, bool startPaused = false)
        {
            Processor processor = null;
            try
            {
                processor = new Processor(Name, processorList.Count);
                processor.Run(afinityMask, priority, threadProc, startPaused);
                lock (processor)
                {
                    processorList.Add(processor);
                }
            }
            catch (Exception)
            {
                processor?.Dispose();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Pauses all processor threads.
        /// </summary>
        public void PauseAll()
        {
            processorList.ForEach(proc => proc.Pause());
        }

        /// <summary>
        /// Resumes all processor threads.
        /// </summary>
        public void ResumeAll()
        {
            processorList.ForEach(proc => proc.Resume());
        }

        /// <summary>
        /// Removes all processor threads.
        /// </summary>
        /// <param name="timeout">The timeout to wait before resorting to abort.</param>
        public void RemoveAll(TimeSpan? timeout)
        {
            lock (processorList)
            {
                processorList.ForEach(proc => proc.Remove(timeout));
                processorList.Clear();
            }
        }

        private Processor Factory()
        {
            return processorList.Find(proc => proc.ManagedThreadId == Thread.CurrentThread.ManagedThreadId);
        }

        #endregion

    }
}
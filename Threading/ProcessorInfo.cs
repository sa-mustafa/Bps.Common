namespace BPS.Common.Threading
{
    using System;
    using System.Threading;

    /// <summary>
    /// The processor information class.
    /// </summary>
    public class ProcessorInfo : IDisposable
    {
        #region Constructor & Finalizer

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessorInfo"/> class.
        /// </summary>
        /// <param name="name">The processor name.</param>
        /// <param name="index">The processor index.</param>
        public ProcessorInfo(string name, int index = 0)
        {
            Name = name;
            Index = index;
            Events = new ThreadEvents();
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (Events != null)
            {
                Events.Dispose();
                Events = null;
            }
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
        /// Gets the processor affinity mask.
        /// </summary>
        public UIntPtr AffinityMask { get; protected set; }

        /// <summary>
        /// Gets the index of the processor core.
        /// </summary>
        //public int CoreIndex { get; protected set; }

        /// <summary>
        /// Gets the processor events.
        /// </summary>
        public ThreadEvents Events { get; protected set; }

        /// <summary>
        /// Gets the processor index.
        /// </summary>
        public int Index { get; protected set; }

        /// <summary>
        /// Gets the processor name.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the processor thread object.
        /// </summary>
        public Thread Thread { get; protected set; }

        /// <summary>
        /// Gets the processor thread priority.
        /// </summary>
        public ThreadPriority Priority { get; protected set; }

        /// <summary>
        /// Gets the thread procedure.
        /// </summary>
        public Delegate ThreadProc { get; protected set; }

        #endregion

        #region Methods

        /// <summary>
        /// Computes the affinity mask of a given core index.
        /// </summary>
        /// <param name="coreIndex">Index of the processor core.</param>
        /// <returns>The affinity mask associated with the given processor core.</returns>
        public static UIntPtr ComputeAffinityMask(int coreIndex)
        {
            if (coreIndex != 0)
                return new UIntPtr(1UL << coreIndex - 1);

            return new UIntPtr(0xFFFFFFFFFFFFFFFFUL);
        }

        /// <summary>
        /// Sets the information details.
        /// </summary>
        /// <param name="coreIndex">Index of the processor core; zero means all cores allowed.</param>
        /// <param name="priority">The thread priority.</param>
        /// <param name="threadProc">The thread procedure.</param>
        /// <param name="thread">The thread object.</param>
        public void Set(int coreIndex, ThreadPriority priority, Delegate threadProc, Thread thread)
        {
            AffinityMask = ComputeAffinityMask(coreIndex);
            Priority = priority;
            ThreadProc = threadProc;
            Thread = thread;
        }

        /// <summary>
        /// Sets the information details.
        /// </summary>
        /// <param name="affinityMask">The thread affinity mask.</param>
        /// <param name="priority">The thread priority.</param>
        /// <param name="threadProc">The thread procedure.</param>
        /// <param name="thread">The thread object.</param>
        public void Set(UIntPtr affinityMask, ThreadPriority priority, Delegate threadProc, Thread thread)
        {
            AffinityMask = affinityMask;
            Priority = priority;
            ThreadProc = threadProc;
            Thread = thread;
        }

        #endregion

    }
}
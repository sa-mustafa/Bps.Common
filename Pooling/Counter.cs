namespace Bps.Common.Pooling
{
    using System.Threading;

    /// <summary>
    /// Counter for diagnostic variables.
    /// </summary>
    public struct Counter
    {

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Counter"/> struct.
        /// </summary>
        /// <param name="value">Specify the counter value.</param>
        public Counter(int value)
        {
            this.value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the counter value.
        /// </summary>
        public int Value
        {
            get { return value; }
        }
        private int value;

        public static implicit operator int(Counter c)
        {
            return c.Value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Increments the counter.
        /// </summary>
        public void Increment()
        {
            Interlocked.Increment(ref value);
        }

        /// <summary>
        /// Decrements the counter.
        /// </summary>
        public void Decrement()
        {
            Interlocked.Decrement(ref value);
        }

        #endregion

    }

}
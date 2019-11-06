namespace BPS.Common.Pooling
{
    public class Diagnostics
    {
        #region Properties

        /// <summary>
        /// Gets the total count of live instances, both in the pool and in use.
        /// </summary>
        public Counter TotalLiveInstancesCount;

        /// <summary>
        /// Gets the count of object reset failures occurred while the pool tried to re-add the object into the pool.
        /// </summary>
        public Counter ObjectResetFailedCount;

        /// <summary>
        /// Gets the total count of object that has been picked up by the GC, and returned to pool. 
        /// </summary>
        public Counter ObjectRessurectionCount;

        /// <summary>
        /// Gets the total count of successful accesses. The pool had a spare object to provide to the user without creating it on demand.
        /// </summary>
        public Counter PoolObjectHitCount;

        /// <summary>
        /// Gets the total count of unsuccessful accesses. The pool had to create an object in order to satisfy the user request. If the number is high, consider increasing the object minimum limit.
        /// </summary>
        public Counter PoolObjectMissCount;

        /// <summary>
        /// Gets the total number of pooled objected created
        /// </summary>
        public Counter TotalObjectsCreated;

        /// <summary>
        /// Gets the total number of objects destroyed, both in case of an pool overflow, and state corruption.
        /// </summary>
        public Counter TotalObjectsDestroyed;

        /// <summary>
        /// Gets the number of objects been destroyed because the pool was full at the time of returning the object to the pool.
        /// </summary>
        public Counter PoolOverflowCount;

        /// <summary>
        /// Gets the total count of objects that been successfully returned to the pool
        /// </summary>
        public Counter ReturnedToPoolCount;

        #endregion
    }
}
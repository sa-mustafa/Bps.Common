namespace Bps.Common.Pooling
{
    using System;

    /// <summary>
    /// Wrapper class for objects intended for pooling.
    /// </summary>
    /// <typeparam name="T">The object type in pool.</typeparam>
    public class PooledObjectWrapper<T> : PooledObject
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledObjectWrapper{T}"/> class.
        /// </summary>
        /// <param name="resource">The resource that will be pooled.</param>
        public PooledObjectWrapper(T resource)
        {
            Resource = resource;
        }

        #endregion

        #region Properties

        public Action<T> WrapperReleaseResourcesAction { get; set; }

        public Action<T> WrapperResetStateAction { get; set; }

        public T Resource { get; private set; }

        #endregion

        #region Methods

        protected override void OnReleaseResources()
        {
            WrapperReleaseResourcesAction?.Invoke(Resource);
        }

        protected override void OnResetState()
        {
            WrapperResetStateAction?.Invoke(Resource);
        }

        #endregion

    }
}
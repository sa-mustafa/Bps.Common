/* 
 * Generic Object Pool Implementation
 *  
 * Implemented by Ofir Makmal, 28/1/2013
 *
 * My Blog: Blogs.microsoft.co.il/blogs/OfirMakmal
 * Email:   Ofir.Makmal@gmail.com
 * 
 */

namespace Bps.Common.Pooling
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;

    /// <summary>
    /// Generic object pool
    /// </summary>
    /// <typeparam name="T">The type of the object that which will be managed by the pool. The pooled object have to be a sub-class of PooledObject.</typeparam>
    public class ObjectPool<T> where T : PooledObject
    {

        #region Fields & Constants

        private const int DefaultPoolSize = 10;

        // Pool internal data structure
        private ConcurrentQueue<T> pooledObjects;

        // Action to be passed to the pooled objects to allow them to return to the pool
        private Action<PooledObject, bool> returnToPoolAction;

        #endregion

        #region Constructors & Finalizer

        /// <summary>
        /// Initializes a new pool with default settings.
        /// </summary>
        public ObjectPool()
        {
            InitializePool(DefaultPoolSize, null);
        }

        /// <summary>
        /// Initializes a new pool with specified minimum pool size and maximum pool size
        /// </summary>
        /// <param name="size">The pool size limit.</param>
        public ObjectPool(int size)
        {
            InitializePool(size, null);
        }

        /// <summary>
        /// Initializes a new pool with specified factory method.
        /// </summary>
        /// <param name="factory">The factory method that will be used to create new objects.</param>
        public ObjectPool(Func<T> factory)
        {
            InitializePool(DefaultPoolSize, factory);
        }

        /// <summary>
        /// Initializes a new pool with specified factory method and minimum and maximum size.
        /// </summary>
        /// <param name="size">The pool size limit.</param>
        /// <param name="factory">The factory method that will be used to create new objects.</param>
        public ObjectPool(int size, Func<T> factory)
        {
            InitializePool(size, factory);
        }

        private void InitializePool(int size, Func<T> factory)
        {
            // Validating pool limits, exception is thrown if invalid
            if (size <= 0)
                throw new ArgumentException("Pool size must be greater than zero.");

            // Assigning properties
            this.factoryMethod = factory;
            this.poolSize = size;

            // Initializing the internal pool data structure
            pooledObjects = new ConcurrentQueue<T>();

            // Creating a new instance for the Diagnostics class
            Diagnostics = new Diagnostics();

            // Setting the action for returning to the pool to be integrated in the pooled objects
            returnToPoolAction = ReturnObjectToPool;

            // Initializing objects in pool
            while (ObjectsInPoolCount < PoolSize)
            {
                pooledObjects.Enqueue(CreatePooledObject());
            }
        }

        ~ObjectPool()
        {
            // The pool is going down, releasing the resources for all objects in pool
            foreach (var item in pooledObjects)
            {
                DestroyPooledObject(item);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Diagnostics class for the current Object Pool.
        /// </summary>
        public Diagnostics Diagnostics { get; private set; }

        /// <summary>
        /// Gets the Factory method that will be used for creating new objects. 
        /// </summary>
        public Func<T> FactoryMethod
        {
            get { return factoryMethod; }
        }
        private Func<T> factoryMethod;

        /// <summary>
        /// Gets the count of the objects currently in the pool.
        /// </summary>
        public int ObjectsInPoolCount
        {
            get { return pooledObjects.Count; }
        }

        /// <summary>
        /// Gets the number of objects in the pool.
        /// </summary>
        public int PoolSize
        {
            get { return poolSize; }
            protected set
            {
                if (value <= 0)
                    throw new ArgumentException("Pool size must be greater than zero.");
                poolSize = value;
            }
        }
        private int poolSize;

        #endregion

        #region Methods

        /// <summary>
        /// Get a monitored object from the pool. 
        /// </summary>
        /// <returns>An object from the pool.</returns>
        public T GetObject()
        {
            T dequeuedObject = null;
            if (pooledObjects.TryDequeue(out dequeuedObject))
            {
                // Diagnostics update
                Diagnostics.PoolObjectHitCount.Increment();
                return dequeuedObject;
            }
            else
            {
                // This should not happen normally, but could be happening when there is stress on the pool
                // No available objects in pool, create a new one and return it to the caller
                Debug.Print("Pool is empty, consider increasing the number of pooled objects.");
                // Diagnostics update
                Diagnostics.PoolObjectMissCount.Increment();
                return CreatePooledObject();
            }
        }

        private T CreatePooledObject()
        {
            T newObject;
            if (FactoryMethod != null)
            {
                newObject = FactoryMethod();
            }
            else
            {
                // Throws an exception if the type doesn't have default constructor - on purpose! I could've added a generic constraint with new (), but I didn't want to limit the user and force a parameterless constructor.
                newObject = (T)Activator.CreateInstance(typeof(T));
            }

            // Diagnostics update
            Diagnostics.TotalObjectsCreated.Increment();

            // Setting the 'return to pool' action in the newly created pooled object
            newObject.ReturnToPool = returnToPoolAction;
            return newObject;
        }

        private void DestroyPooledObject(PooledObject objectToDestroy)
        {
            // Making sure that the object is only disposed once (in case of application shutting down and we don't control the order of the finalization)
            if (!objectToDestroy.Disposed)
            {
                // Deterministically release object resources, never mind the result, we are destroying the object
                objectToDestroy.ReleaseResources();
                objectToDestroy.Disposed = true;

                // Diagnostics update
                Diagnostics.TotalObjectsDestroyed.Increment();
            }

            // The object is being destroyed, resources have been already released deterministically, so we do not need the finalizer to fire
            GC.SuppressFinalize(objectToDestroy);
        }

        private void ReturnObjectToPool(PooledObject objectToReturnToPool, bool reRegisterForFinalization)
        {
            T returnedObject = (T)objectToReturnToPool;

            // Diagnostics update
            if (reRegisterForFinalization) Diagnostics.ObjectRessurectionCount.Increment();

            // Checking that the pool is not full
            if (ObjectsInPoolCount < PoolSize)
            {
                // Reset the object state (if implemented) before returning it to the pool. If reseting the object have failed, destroy the object
                if (!returnedObject.ResetState())
                {
                    // Diagnostics update
                    Diagnostics.ObjectResetFailedCount.Increment();
                    DestroyPooledObject(returnedObject);
                    return;
                }

                // re-registering for finalization - in case of resurrection (called from Finalize method)
                if (reRegisterForFinalization)
                    GC.ReRegisterForFinalize(returnedObject);

                // Diagnostics update
                Diagnostics.ReturnedToPoolCount.Increment();

                // Adding the object back to the pool 
                pooledObjects.Enqueue(returnedObject);
            }
            else
            {
                // Diagnostics update
                Diagnostics.PoolOverflowCount.Increment();

                //The Pool's upper limit has exceeded, there is no need to add this object back into the pool and we can destroy it.
                DestroyPooledObject(returnedObject);
            }
        }

        #endregion

    }
}
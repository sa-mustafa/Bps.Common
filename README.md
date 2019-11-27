# Bps.Common

This repo contains the most common utility needed for most of the projects I did recently. Each project required:

- Exception handling & logging mostly with [NLog](https://github.com/NLog/NLog).
- Handling single instance running for services
- Flexible threading mechanism with [Processor](./Threading/Processor.cs) & [Processors](./Threading/Processors.cs)
- and object pooling to avoid memory fragmentation.

This project is battle tested initially in many projects: MAFIS and its variants, DBScan-v1 & DBScan-v2. These projects performed biometric enrollment & identification on facial images.

The threading system in this project is its strongest point and provides you with a system way better than .Net [Threading](https://docs.microsoft.com/en-us/dotnet/standard/threading/using-threads-and-threading), [ThreadPool](https://docs.microsoft.com/en-us/dotnet/api/system.threading.threadpool?view=netframework-4.8), [Task Parallel Library](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl) (TPL) mechanisms.

To create a single processing thread (optionally running on all cores with AboveNormal priority), use the following code snippet:

```C#
	// Create a database manager thread. Naming the thread helps ease the debugging/logging tasks. 
	dbManager = new Processor("Database Manager");
  dbManager.Run(0, System.Threading.ThreadPriority.AboveNormal, (SingleProcessorThreadStart)DatabaseManagerThreadProc);
```

The Run() function sets the thread running. To pause or stop the thread, use the Pause()/Stop() functions. To completely stop a thread, use the Remove() function.

Inside the thread's procedure, you may use the following pattern for execution. This pattern helps the thread respond to external commands (Pause/Resume/Stop/Remove).

```C#
bool DatabaseManagerThreadProc()
{
  #region Prep up the Thread
  // Initialize the thread local variables
  TimeSpan lapse = TimeSpan.FromSeconds(1);
  #endregion
  #region Main Processing Loop
  while (true)
  {
      try
      {
          dbManager.CouldResume(TimeSpan.MaxValue);
          if (dbManager.ShouldStop(TimeSpan.Zero))
          {
              // The thread should terminate gracefully.
              // Log the thread performance parameters...
              break;
          }

          // The thread main loop comes here.
          // Do not forget to put the thread 
          // to sleep when it has nothing to do!
          Thread.Sleep(lapse);
      }
      catch (Exception ex)
      {
          // Log the exception using the common exception handler.
          Exceptions.Handle(ex);
      }
    }
    #endregion
    #region Finalize the Thread Processing
    // Perform finishing tasks
    // ...
    return dbManager.Finish(true);
    #endregion
}
```

To create a group of processing threads (preferably running on specified core with AboveNormal priority), use the following code snippet:

```C#
	// Create a database manager thread. Naming the thread helps ease the debugging/logging tasks. 
	enrollment = new Processor("Enrollment Processors");
  enrollment.Add(1, System.Threading.ThreadPriority.BelowNormal, EnrollProcessorsThreadProc);
```

The Add() function creates and sets one thread running. To pause or resume the thread group, use the PauseAll()/ResumeAll() functions. To completely stop the thread group, use the RemoveAll() function.

Inside the thread's procedure, you may use the following pattern for execution. This pattern agian helps the thread group respond to external commands.

```C#
bool EnrollProcessorsThreadProc(int index)
{
  #region Prep up the Thread
  // Initialize the thread local variables
  TimeSpan lapse = TimeSpan.FromMilliseconds(10);
  Processor processor = enrollment.Current;
  #endregion
  #region Main Processing Loop
  while (true)
  {
      try
      {
          processor.CouldResume(TimeSpan.MaxValue);
          if (processor.ShouldStop(TimeSpan.Zero))
          {
              // The thread should terminate gracefully.
              // Log the thread performance parameters...
              break;
          }

          // The thread main loop comes here.
          // Do not forget to put the thread 
          // to sleep when it has nothing to do!
          Thread.Sleep(lapse);
      }
      catch (Exception ex)
      {
          // Log the exception using the common exception handler.
          Exceptions.Handle(ex);
      }
    }
    #endregion
    #region Finalize the Thread Processing
    // Perform finishing tasks
    // ...
    return processor.Finish(true);
    #endregion
}
```

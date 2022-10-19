namespace Bps.Common
{
    using System;
    using System.Linq;
    using System.Runtime.ExceptionServices;
    using System.Collections.Generic;

    public class Exceptions
    {
        #region Types

        public enum Levels
        {
            Trace = 0,
            Debug,
            Info,
            Warning,
            Error,
            Fatal
        }

        #endregion

        #region Fields

        // NLog logging object
        private static NLog.Logger logger;

        private static List<string> filters = new List<string>();

        #endregion

        #region Properties

        public NLog.Logger Logger { get { return logger; } }

        #endregion

        #region Methods

        public static void AddFilter(string message)
        {
            filters.Add(message);
        }

        public static void DeleteFilter(string message)
        {
            filters.Remove(message);
        }

        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public static void Handle(Exception ex)
        {
            try
            {
                if (logger == null || ex == null)
                    return;

                var isFiltered = filters.Any(msg => msg == ex.Message);
                if (isFiltered)
                    return;

                logger.Error(ex);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Logs information into log file.
        /// </summary>
        /// <param name="text">The message text.</param>
        /// <param name="level">The log level.</param>
        public static void Log(string text, Levels level = Levels.Trace)
        {
            try
            {
                switch (level)
                {
                    case Levels.Trace:
                        if (logger.IsTraceEnabled)
                            logger.Trace(text);
                        break;
                    case Levels.Debug:
                        if (logger.IsDebugEnabled)
                            logger.Debug(text);
                        break;
                    case Levels.Info:
                        if (logger.IsInfoEnabled)
                            logger.Info(text);
                        break;
                    case Levels.Warning:
                        if (logger.IsWarnEnabled)
                            logger.Warn(text);
                        break;
                    case Levels.Error:
                        if (logger.IsErrorEnabled)
                            logger.Error(text);
                        break;
                    case Levels.Fatal:
                        if (logger.IsFatalEnabled)
                            logger.Fatal(text);
                        break;
                }
            }
            catch (Exception ex)
            {
                Handle(ex);
            }
        }

        public static void Log(string format, object arg0, Levels level)
        {
            Log(string.Format(format, arg0), level);
        }

        public static void Log(string format, object arg0, object arg1, Levels level)
        {
            Log(string.Format(format, arg0, arg1), level);
        }

        public static void Log(string format, object arg0, object arg1, object arg2, Levels level)
        {
            Log(string.Format(format, arg0, arg1, arg2), level);
        }

        public static void Log(string format, object arg0, object arg1, object arg2, object arg3, Levels level)
        {
            Log(string.Format(format, arg0, arg1, arg2, arg3), level);
        }

        public static void Trace(string format, object arg0)
        {
            Log(string.Format(format, arg0), Levels.Trace);
        }

        public static void Trace(string format, object arg0, object arg1)
        {
            Log(string.Format(format, arg0, arg1), Levels.Trace);
        }

        public static void Trace(string format, object arg0, object arg1, object arg2)
        {
            Log(string.Format(format, arg0, arg1, arg2), Levels.Trace);
        }

        public static void Trace(string format, object arg0, object arg1, object arg2, object arg3)
        {
            Log(string.Format(format, arg0, arg1, arg2, arg3), Levels.Trace);
        }

        /// <summary>
        /// Sets up exception and logging handlers.
        /// </summary>
        /// <param name="config">Specify the log configuration file.</param>
        public static void Setup(string config = "Nlog.config")
        {
            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(config);
            logger = NLog.LogManager.GetCurrentClassLogger();
            SetupUnhandled();
        }

        /// <summary>
        /// Setups the un-handled exception handlers.
        /// </summary>
        public static void SetupUnhandled()
        {
            AppDomain.CurrentDomain.UnhandledException += DomainUnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += DomainFirstChanceException;
        }

        /// <summary>
        /// Shuts down the exception logging.
        /// </summary>
        public static void Shutdown()
        {
            NLog.LogManager.Shutdown();
        }

        private static void DomainFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            Handle(e.Exception);
        }

        private static void DomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Handle(e.ExceptionObject as Exception);
        }

        #endregion
    }
}
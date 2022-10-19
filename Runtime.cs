namespace Bps.Common
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading;

    public static class Runtime
    {
        #region Fields

        private static Mutex appMutex;

        #endregion

        #region Methods

        /// <summary>
        /// Gets the currently running application's directory.
        /// </summary>
        /// <returns>the running application path.</returns>
        public static string GetAppDir()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        /// <summary>
        /// Gets the currently running application's path.
        /// </summary>
        /// <returns>the running application path.</returns>
        public static string GetAppPath()
        {
            return Assembly.GetEntryAssembly().Location;
        }

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        /// <returns>the product attribute as application name.</returns>
        public static string GetAppName()
        {
            var attrib = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>();
            return attrib?.Product;
        }

        /// <summary>
        /// Gets the application title.
        /// </summary>
        /// <returns>the title attribute as application title.</returns>
        public static string GetAppTitle()
        {
            var attrib = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyTitleAttribute>();
            return attrib?.Title;
        }

        /// <summary>
        /// Ensures the application is large-address-aware; i.e. can use more than 2GB memory.
        /// </summary>
        /// <param name="exePath">The executable path.</param>
        /// <returns>true if the application supports more than 2GB memory; false otherwise.</returns>
        /// <remarks>Use 'EditBin /LARGEADDRESSAWARE ExePath' to enable this option manually.</remarks>
        public static bool EnsureLargeAddressAwareApp(string exePath)
        {
            using (var fs = File.OpenRead(exePath))
            {
                const int IMAGE_FILE_LARGE_ADDRESS_AWARE = 0x20;
                using (var br = new BinaryReader(fs))
                {
                    if (br.ReadInt16() != 0x5A4D)   // No MZ Header
                        return false;

                    br.BaseStream.Position = 0x3C;
                    //Get the PE header location.
                    br.BaseStream.Position = br.ReadInt32();
                    if (br.ReadInt32() != 0x4550)   //No PE header
                        return false;

                    br.BaseStream.Position += 0x12;
                    return (br.ReadInt16() & IMAGE_FILE_LARGE_ADDRESS_AWARE) == IMAGE_FILE_LARGE_ADDRESS_AWARE;
                }
            }
        }

        /// <summary>
        /// Ensures the application is running in a single instance.
        /// </summary>
        /// <param name="uniqueName">Specify a unique name for the application instance.</param>
        /// <returns>true if application runs in a single instance; false otherwise.</returns>
        public static bool EnsureSingleInstanceApp(string uniqueName)
        {
            try
            {
                if (appMutex != null)
                    return true;

                appMutex = new Mutex(true, uniqueName, out bool justCreated);
                if (!justCreated)
                {
                    appMutex.Dispose();
                    appMutex = null;
                }
                return justCreated;
            }
            catch (Exception ex)
            {
                Exceptions.Handle(ex);
                return false;
            }
        }

        #endregion

    }
}
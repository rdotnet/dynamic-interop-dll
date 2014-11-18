using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicInterop
{
    public static class PlatformUtility
    {
        /// <summary>
        /// Is the platform a unix like (Unix or MacOX)
        /// </summary>
        public static bool IsUnix
        {
            get
            {
                var p = GetPlatform();
                return p == PlatformID.MacOSX || p == PlatformID.Unix;
            }
        }

        /// <summary>
        /// Gets the platform on which the current process runs.
        /// </summary>
        /// <remarks>
        /// <see cref="Environment.OSVersion"/>'s platform is not <see cref="PlatformID.MacOSX"/> even on Mac OS X.
        /// This method returns <see cref="PlatformID.MacOSX"/> when the current process runs on Mac OS X.
        /// This method uses UNIX's uname command to check the operating system,
        /// so this method cannot check the OS correctly if the PATH environment variable is changed (will returns <see cref="PlatformID.Unix"/>).
        /// </remarks>
        /// <returns>The current platform.</returns>
        public static PlatformID GetPlatform()
        {
            if (!curPlatform.HasValue)
            {
                var platform = Environment.OSVersion.Platform;
                if (platform != PlatformID.Unix)
                {
                    curPlatform = platform;
                }
                else
                {
                    try
                    {
                        var kernelName = ExecCommand("uname", "-s");
                        curPlatform = (kernelName == "Darwin" ? PlatformID.MacOSX : platform);
                    }
                    catch (Win32Exception)
                    { // probably no PATH to uname.
                        curPlatform = platform;
                    }
                }
            }
            return curPlatform.Value;
        }

        private static PlatformID? curPlatform = null;

    }
}

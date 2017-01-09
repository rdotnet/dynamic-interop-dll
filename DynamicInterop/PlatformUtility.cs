using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;

namespace DynamicInterop
{
    /// <summary>
    /// Helper class with functions whose behavior may be depending on the platform 
    /// </summary>
    public static class PlatformUtility
    {       
        /// <summary>
        /// Is the platform unix-like (Unix or MacOX)
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

        /// <summary>
        /// Execute a command in a new process
        /// </summary>
        /// <param name="processName">Process name e.g. "uname"</param>
        /// <param name="arguments">Arguments e.g. "-s"</param>
        /// <returns>The output of the command to the standard output stream</returns>
        public static string ExecCommand(string processName, string arguments)
        {
            using (var proc = new Process())
            {
                proc.StartInfo.FileName = processName;
                proc.StartInfo.Arguments = arguments;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                var kernelName = proc.StandardOutput.ReadLine();
                proc.WaitForExit();
                return kernelName;
            }
        }

        /// <summary>
        /// Gets a message saying the current platform is not supported
        /// </summary>
        /// <returns>The platform not supported message.</returns>
        public static string GetPlatformNotSupportedMsg()
        {
            return string.Format("Platform {0} is not supported.", Environment.OSVersion.Platform.ToString());
        }

        /// <summary>
        /// Given a DLL short file name, find all the matching occurences in directories as stored in an environment variable such as the PATH.
        /// </summary>
        /// <returns>One or more full file names found to exist</returns>
        /// <param name="dllName">short file name.</param>
        /// <param name="envVarName">Environment variable name - default PATH</param>
        public static string[] FindFullPathEnvVar(string dllName, string envVarName="PATH")
        {
            var searchPaths = (Environment.GetEnvironmentVariable(envVarName) ?? "").Split(Path.PathSeparator);
            return FindFullPath (dllName, searchPaths);
        }

        /// <summary>
        /// Given a DLL short file name, find all the matching occurences in directories.
        /// </summary>
        /// <returns>One or more full file names found to exist</returns>
        /// <param name="dllName">short file name.</param>
        /// <param name="directories">Directories in which to search for matching file names</param>
        public static string[] FindFullPath(string dllName, params string[] directories)
        {
            return directories.Select(directory => Path.Combine(directory, dllName)).Where(File.Exists).ToArray();
        }

        public static string FindFirstFullPath(string nativeLibFilename, string libname = "native library", string envVarName = "")
        {
            if (string.IsNullOrEmpty(nativeLibFilename) || !Path.IsPathRooted(nativeLibFilename))
                nativeLibFilename = findFirstFullPath(nativeLibFilename, envVarName);
            else if (!File.Exists(nativeLibFilename))
                throw new DllNotFoundException(string.Format("Could not find specified file {0} to load as {1}", nativeLibFilename, libname));
            return nativeLibFilename;
        }

        private static string findFirstFullPath(string shortFileName, string envVarName = "")
        {
            if (string.IsNullOrEmpty(shortFileName))
                throw new ArgumentNullException("shortFileName");

            string libSearchPathEnvVar = envVarName;
            if (string.IsNullOrEmpty(libSearchPathEnvVar))
                libSearchPathEnvVar = (Environment.OSVersion.Platform == PlatformID.Win32NT ? "PATH" : "LD_LIBRARY_PATH");
            var candidates = PlatformUtility.FindFullPathEnvVar(shortFileName, libSearchPathEnvVar);
            if ((candidates.Length == 0) && (Environment.OSVersion.Platform == PlatformID.Win32NT))
                if (File.Exists(shortFileName))
                    candidates = new string[] { shortFileName };
            if (candidates.Length == 0)
                throw new DllNotFoundException(string.Format("Could not find native library named '{0}' within the directories specified in the '{1}' environment variable", shortFileName, libSearchPathEnvVar));
            else
                return candidates[0];
        }

        public static string CreateLibraryFileName(string libraryName)
        {
            if (string.IsNullOrEmpty(libraryName))
                throw new ArgumentNullException("libraryName");
            return 
                (Environment.OSVersion.Platform == PlatformID.Win32NT ? 
                libraryName + ".dll" : 
                "lib" + libraryName + ".so");
        }
    }
}

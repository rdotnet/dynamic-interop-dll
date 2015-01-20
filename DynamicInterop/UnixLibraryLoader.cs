using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

namespace DynamicInterop
{
    [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    internal class UnixLibraryLoader : IDynamicLibraryLoader
    {
        public IntPtr LoadLibrary(string filename)
        {
            return InternalLoadLibrary(filename);
        }

        /// <summary>
        /// Gets the last error. NOTE: according to http://tldp.org/HOWTO/Program-Library-HOWTO/dl-libraries.html, returns NULL if called more than once after dlopen.
        /// </summary>
        /// <returns>The last error.</returns>
        public string GetLastError()
        {
            return dlerror();
        }

        /// <summary>
        /// Unloads a library
        /// </summary>
        /// <param name="handle">The pointer resulting from loading the library</param>
        /// <returns>True if the function dlclose returned 0</returns>
        public bool FreeLibrary(IntPtr handle)
        {
            // according to the manual page on a Debian box
            // The function dlclose() returns 0 on success, and nonzero on error.
            var status = dlclose(handle);
            return status == 0;
        }

        public IntPtr GetFunctionAddress(IntPtr hModule, string lpProcName)
        {
            return dlsym(hModule, lpProcName);
        }

        internal static IntPtr InternalLoadLibrary(string filename)
        {
            const int RTLD_LAZY = 0x1;
//            if (filename.StartsWith ("/")) {
//                return dlopen (filename, RTLD_LAZY);
//            }
//            var searchPaths = getSearchPaths ("LD_LIBRARY_PATH");
//            searchPaths.AddRange (getSearchPaths ("PATH"));
//            var dll = searchPaths.Select (directory => Path.Combine (directory, filename)).FirstOrDefault (File.Exists);
//            if (dll == null) {
//                throw new DllNotFoundException ("Could not find the file: " + filename + " on the search path.  Checked these directories:\n "
//                + String.Join ("\n", searchPaths));
//            }

            var result = dlopen (filename, RTLD_LAZY);
            return result;

        }

        static List<string> getSearchPaths(string pathsEnvVar)
        {
            var searchPaths = (Environment.GetEnvironmentVariable (pathsEnvVar) ?? "").Split (Path.PathSeparator).ToList ();
            return searchPaths;
        }

        [DllImport("libdl")]
        private static extern IntPtr dlopen([MarshalAs(UnmanagedType.LPStr)] string filename, int flag);

        [DllImport("libdl")]
        [return: MarshalAs(UnmanagedType.LPStr)]
        private static extern string dlerror();

        [DllImport("libdl", EntryPoint = "dlclose")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern int dlclose(IntPtr hModule);

        [DllImport("libdl", EntryPoint = "dlsym")]
        private static extern IntPtr dlsym(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);
    }
}

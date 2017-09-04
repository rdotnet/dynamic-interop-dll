using System;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

namespace DynamicInterop
{
    [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    internal class WindowsLibraryLoader : IDynamicLibraryLoader
    {
        public IntPtr LoadLibrary(string filename)
        {
            //new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            var handle = Win32.LoadLibrary(filename);
            if (handle == IntPtr.Zero)
            {
                var error = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                Console.WriteLine(error);
            }
            return handle;
        }

        public string GetLastError()
        {
            // see for instance http://blogs.msdn.com/b/shawnfa/archive/2004/09/10/227995.aspx 
            // and http://blogs.msdn.com/b/adam_nathan/archive/2003/04/25/56643.aspx
            // TODO: does this work as expected with Mono+Windows stack?
            return new Win32Exception().Message;
        }

        public bool FreeLibrary(IntPtr handle)
        {
            return Win32.FreeLibrary(handle);
        }

        public IntPtr GetFunctionAddress(IntPtr hModule, string lpProcName)
        {
            return Win32.GetProcAddress(hModule, lpProcName);
        }

        public static string GetShortPath(string path)
        {
            var shortPath = new StringBuilder(Win32.MaxPathLength);
            Win32.GetShortPathName(path, shortPath, Win32.MaxPathLength);
            return shortPath.ToString();
        }
    }

    internal static class Win32
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule,
                                                   [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        public const int MaxPathLength = 248; //MaxPath is 248. MaxFileName is 260.

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName([MarshalAs(UnmanagedType.LPTStr)] string path,
                                                  [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath,
                                                  int shortPathLength);
    }
}

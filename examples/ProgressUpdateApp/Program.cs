using DynamicInterop;
using System;

namespace ProgressUpdateApp
{
    class Program
    {
        static UnmanagedDll myNativeDll = null;

        static void Main(string[] args)
        {
            //            Environment.CurrentDirectory
            //"C:\\src\\github_jm\\dynamic-interop-dll\\examples\\ProgressUpdateApp"

            myNativeDll = new UnmanagedDll(@"..\x64\Debug\progress_update_native.dll");

            Console.WriteLine("Register with the C++ native library the C# function to call back...");
            CallbackHandlers.SetProgressUpdateCallback();
            Console.WriteLine("About to call native task...");
            LaunchLongLivedNativeTask();
            Console.WriteLine("Native task finished and returned");
        }

        private static void LaunchLongLivedNativeTask()
        {
            var doTask = myNativeDll.GetFunction<DoLongLivedTask>("do_long_lived_task");
            doTask();
        }

        /// <summary> The dotnet signature of the native function </summary>
        private delegate bool DoLongLivedTask();
    }
}

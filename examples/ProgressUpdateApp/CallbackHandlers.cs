using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ProgressUpdateApp
{
    public static class CallbackHandlers
    {
        static CallbackHandlers()
        {
        }
        public static void SetProgressUpdateCallback()
        {
            RegisterProgressUpdateCallback(ProgressUpdateHandler);
        }

        /// <summary> The method called from C++ to notify of a progress update</summary>
        public static void ProgressUpdateHandler(double progress)
        {
            Console.WriteLine("Progress value is: " + progress.ToString());
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void ProgressUpdateCallback(double progressValue);
        private static ProgressUpdateCallback callback = null;

        public static void RegisterProgressUpdateCallback(Action<double> whatToDo)
        {
            // Many thanks to http://www.codeproject.com/Tips/318140/How-to-make-a-callback-to-Csharp-from-C-Cplusplus
            // for providing a solution that worked, in terms of what calling convention to use. 
            // I tried a few things before that - there is a lot of other (incorrect?) information
            // that did not work (probably my bad, still).
            callback = null;
            callback = new ProgressUpdateCallback(whatToDo);
            RegisterProgressUpdateCallbackNative(callback);
        }

        [DllImport("progress_update_native", EntryPoint = "register_progress_update_callback")]
        public static extern void RegisterProgressUpdateCallbackNative([MarshalAs(UnmanagedType.FunctionPtr)] ProgressUpdateCallback call);

    }
}

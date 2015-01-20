using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DynamicInterop
{
    /// <summary>
    /// A proxy for unmanaged dynamic link library (DLL).
    /// </summary>
    public class UnmanagedDll : IDisposable
    {
        private SafeHandleUnmanagedDll handle;

        // /// <summary>
        // /// Gets whether the current handle is equal to the invalid handle
        // /// </summary>
        // public override bool IsInvalid
        // {
        //     get { return handle == IntPtr.Zero; }
        // }
        
        /// <summary>
        /// Creates a proxy for the specified dll.
        /// </summary>
        /// <param name="dllName">The DLL's name.</param>
        public UnmanagedDll(string dllName)
        {
            if (dllName == null)
            {
                throw new ArgumentNullException("dllName", "The name of the library to load is a null reference");
            }
            if (dllName == string.Empty)
            {
                throw new ArgumentException("The name of the library to load is an empty string", "dllName");
            }

            handle = new SafeHandleUnmanagedDll(dllName); 

            if (handle.IsInvalid)
            {
                // Retrieve the last error as soon as possible, 
                // to limit the risk of another call to the dynamic loader overriding the error message;
                var nativeError = handle.GetLastError();
                ReportLoadLibError(dllName, nativeError);
            }
            Filename = dllName;
        }

        /// <summary>
        /// Gets the Dll file name used for this native Dll wrapper.
        /// </summary>
        public string Filename { get; private set; }


        private void ReportLoadLibError(string dllName, string nativeError)
        {
            ThrowFailedLibraryLoad (dllName, nativeError);
/*
 * string dllFullName = dllName;
            if (File.Exists(dllFullName))
                ThrowFailedLibraryLoad(dllFullName);
            else
            {
                // This below assumes that the PATH environment variable is what is relied on
                // TODO: check whether there is more to it: http://msdn.microsoft.com/en-us/library/ms682586.aspx

                // Also some pointers to relevant information if we want to check whether the attempt to load 
                // was made on a 32 or 64 bit library
                // For Windows:
                // http://stackoverflow.com/questions/1345632/determine-if-an-executable-or-library-is-32-or-64-bits-on-windows
                // http://www.neowin.net/forum/topic/732648-check-if-exe-is-x64/?p=590544108#entry590544108
                // Linux, and perhaps MacOS; the 'file' command seems the way to go.
                // http://stackoverflow.com/questions/5665228/in-linux-determine-if-a-a-library-archive-32-bit-or-64-bit

                dllFullName = FindFullPath(dllName, throwIfNotFound: true);
                ThrowFailedLibraryLoad(dllFullName);
            }
            */
        }
            
        [Obsolete("This message is likely to be too distribution specific", true)]
        private string createLdLibPathMsg()
        {
            if (!PlatformUtility.IsUnix)
                return null;
            //var sampleldLibPaths = "/usr/local/lib/R/lib:/usr/local/lib:/usr/lib/jvm/java-7-openjdk-amd64/jre/lib/amd64/server";
            var ldLibPathEnv = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH");
            string msg = Environment.NewLine + Environment.NewLine;
            if (string.IsNullOrEmpty(ldLibPathEnv))
                msg = msg + "The environment variable LD_LIBRARY_PATH is not set.";
            else
                msg = msg + string.Format("The environment variable LD_LIBRARY_PATH is set to {0}.", ldLibPathEnv);

            msg = msg + " For some Unix-like operating systems you may need to set or modify the variable LD_LIBRARY_PATH BEFORE launching the application.";
            //msg = msg + " You can get the value as set by the R console application for your system, with the statement Sys.getenv('LD_LIBRARY_PATH'). For instance from your shell prompt:";
            //msg = msg + Environment.NewLine;
            //msg = msg + "Rscript -e \"Sys.getenv('LD_LIBRARY_PATH')\"";
            //msg = msg + Environment.NewLine;
            //msg = msg + "export LD_LIBRARY_PATH=/usr/the/paths/you/just/got/from/Rscript";
            msg = msg + Environment.NewLine + Environment.NewLine;

            return msg;
        }

        private void ThrowFailedLibraryLoad(string dllFullName, string nativeError)
        {
            var strMsg = string.Format("This {0}-bit process failed to load the library {1}",
                                       (Environment.Is64BitProcess ? "64" : "32"), dllFullName);
            if (!string.IsNullOrEmpty(nativeError))
                strMsg = strMsg + string.Format(". Native error message is '{0}'", nativeError);
            else
                strMsg = strMsg + ". No further error message from the dynamic library loader";

//            var ldLibPathMsg = createLdLibPathMsg();
//            if (!string.IsNullOrEmpty(ldLibPathMsg))
//                strMsg = strMsg + string.Format(". {0}", ldLibPathMsg);
            throw new Exception(strMsg);
        }

        private Dictionary<string, object> delegateFunctionPointers = new Dictionary<string, object>();

        /// <summary>
        /// Creates the delegate function for the specified function defined in the DLL.
        /// </summary>
        /// <typeparam name="TDelegate">The type of delegate. The name of the native function is assumed to be the same as the delegate type name.</typeparam>
        /// <returns>The delegate.</returns>
        public TDelegate GetFunction<TDelegate>()
           where TDelegate : class
        {
            return GetFunction<TDelegate>(typeof(TDelegate).Name);
        }

        /// <summary>
        /// Creates the delegate function for the specified function defined in the DLL.
        /// </summary>
        /// <typeparam name="TDelegate">The type of delegate.</typeparam>
        /// <param name="entryPoint">The name of the function exported by the DLL</param>
        /// <returns>The delegate.</returns>
        public TDelegate GetFunction<TDelegate>(string entryPoint)
           where TDelegate : class
        {
            if (string.IsNullOrEmpty(entryPoint))
                throw new ArgumentNullException("entryPoint", "Native function name cannot be null or empty");
            Type delegateType = typeof(TDelegate);
            if (delegateFunctionPointers.ContainsKey(entryPoint))
                return (TDelegate)delegateFunctionPointers[entryPoint];
            if (!delegateType.IsSubclassOf(typeof(Delegate)))
            {
                throw new InvalidCastException();
            }
            IntPtr function = GetFunctionAddress(entryPoint);
            if (function == IntPtr.Zero)
            {
                throwEntryPointNotFound(entryPoint);
            }
            var dFunc = Marshal.GetDelegateForFunctionPointer(function, delegateType) as TDelegate;
            delegateFunctionPointers.Add(entryPoint, dFunc);
            return dFunc;
        }

        private void throwEntryPointNotFound(string entryPoint)
        {
            throw new EntryPointNotFoundException(string.Format("Function {0} not found in native library {1}", entryPoint, this.Filename));
        }

        /// <summary>
        /// Gets the address of a native function entry point.
        /// </summary>
        /// <returns>The function address.</returns>
        /// <param name="lpProcName">name of the function in the native library</param>
        public IntPtr GetFunctionAddress(string lpProcName)
        {
            return handle.GetFunctionAddress(lpProcName);
        }

        /// <summary>
        /// Gets the handle of the specified entry.
        /// </summary>
        /// <param name="entryPoint">The name of function.</param>
        /// <returns>The handle.</returns>
        public IntPtr DangerousGetHandle(string entryPoint)
        {
            if (string.IsNullOrEmpty(entryPoint))
            {
                throw new ArgumentNullException("The entry point cannot be null or an empty string", "entryPoint");
            }
            return GetFunctionAddress(entryPoint);
        }

        /// <summary>
        /// Frees the native library this objects represents
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            handle.Dispose();
        }

        /// <summary>
        /// Dispose of this library.
        /// </summary>
        /// <remarks>Call <see cref="Dispose()"/> when you are finished using the <see cref="DynamicInterop.UnmanagedDll"/>. The
        /// <see cref="Dispose()"/> method leaves the <see cref="DynamicInterop.UnmanagedDll"/> in an unusable state.
        /// After calling <see cref="Dispose()"/>, you must release all references to the
        /// <see cref="DynamicInterop.UnmanagedDll"/> so the garbage collector can reclaim the memory that the
        /// <see cref="DynamicInterop.UnmanagedDll"/> was occupying.</remarks>
        public void Dispose()
        {
            Dispose(true);
        }


        IntPtr checkedGetSymbolHandle(string symbolName)
        {
            var addr = this.DangerousGetHandle (symbolName);
            if (IntPtr.Zero == addr)
                throw new ArgumentException (string.Format ("Could not retrieve a pointer for the symbol '{0}' in file '{1}'", symbolName, Filename));
            return addr;
        }

        /// <summary>
        /// Writes an int32 value to the address of a symbol in the library. 
        /// </summary>
        /// <param name="symbolName">Symbol name.</param>
        /// <param name="value">Value.</param>
        /// <remarks>Throws an <exception cref="System.ArgumentException">ArgumentException</exception> if the symbol is not exported by the library</remarks>
        public void WriteInt32(string symbolName, int value)
        {
            var addr = checkedGetSymbolHandle (symbolName);
            Marshal.WriteInt32(addr, value);
        }

        /// <summary>
        /// Reads an int32 value from the address of a symbol in the library. 
        /// </summary>
        /// <returns>The value for this symbol, read as an int32</returns>
        /// <param name="symbolName">Symbol name.</param>
        /// <remarks>Throws an <exception cref="System.ArgumentException">ArgumentException</exception> if the symbol is not exported by the library</remarks>
        public int GetInt32(string symbolName)
        {
            var addr = checkedGetSymbolHandle (symbolName);
            return Marshal.ReadInt32(addr);
        }

        /// <summary>
        /// Writes an int64 value to the address of a symbol in the library. 
        /// </summary>
        /// <param name="symbolName">Symbol name.</param>
        /// <param name="value">Value.</param>
        /// <remarks>Throws an <exception cref="System.ArgumentException">ArgumentException</exception> if the symbol is not exported by the library</remarks>
        public void WriteInt64(string symbolName, long value)
        {
            var addr = checkedGetSymbolHandle (symbolName);
            Marshal.WriteInt64(addr, value);
        }

        /// <summary>
        /// Reads an int64 value from the address of a symbol in the library. 
        /// </summary>
        /// <returns>The value for this symbol, read as an int64</returns>
        /// <param name="symbolName">Symbol name.</param>
        /// <remarks>Throws an <exception cref="System.ArgumentException">ArgumentException</exception> if the symbol is not exported by the library</remarks>
        public long GetInt64(string symbolName)
        {
            var addr = checkedGetSymbolHandle (symbolName);
            return Marshal.ReadInt64(addr);
        }

        /// <summary>
        /// Writes an IntPtr value to the address of a symbol in the library. 
        /// </summary>
        /// <param name="symbolName">Symbol name.</param>
        /// <param name="value">Value.</param>
        /// <remarks>Throws an <exception cref="System.ArgumentException">ArgumentException</exception> if the symbol is not exported by the library</remarks>
        public void WriteIntPtr(string symbolName, IntPtr value)
        {
            var addr = checkedGetSymbolHandle (symbolName);
            Marshal.WriteIntPtr(addr, value);
        }

        /// <summary>
        /// Reads an IntPtr value from the address of a symbol in the library. 
        /// </summary>
        /// <returns>The value for this symbol, read as an IntPtr</returns>
        /// <param name="symbolName">Symbol name.</param>
        /// <remarks>Throws an <exception cref="System.ArgumentException">ArgumentException</exception> if the symbol is not exported by the library</remarks>
        public IntPtr GetIntPtr(string symbolName)
        {
            var addr = checkedGetSymbolHandle (symbolName);
            return Marshal.ReadIntPtr(addr);
        }

        /// <summary>
        /// Writes a Byte value to the address of a symbol in the library. 
        /// </summary>
        /// <param name="symbolName">Symbol name.</param>
        /// <param name="value">Value.</param>
        /// <remarks>Throws an <exception cref="System.ArgumentException">ArgumentException</exception> if the symbol is not exported by the library</remarks>
        public void WriteByte(string symbolName, Byte value)
        {
            var addr = checkedGetSymbolHandle (symbolName);
            Marshal.WriteByte(addr, value);
        }

        /// <summary>
        /// Reads a byte value from the address of a symbol in the library. 
        /// </summary>
        /// <returns>The value for this symbol, read as a byte</returns>
        /// <param name="symbolName">Symbol name.</param>
        /// <remarks>Throws an <exception cref="System.ArgumentException">ArgumentException</exception> if the symbol is not exported by the library</remarks>
        public Byte GetByte(string symbolName)
        {
            var addr = checkedGetSymbolHandle (symbolName);
            return Marshal.ReadByte(addr);
        }

        /// <summary>
        /// Reads a string value from the address of a symbol in the library. 
        /// </summary>
        /// <returns>The value for this symbol, read as an ANSI string</returns>
        /// <param name="symbolName">Symbol name.</param>
        /// <remarks>Throws an <exception cref="System.ArgumentException">ArgumentException</exception> if the symbol is not exported by the library</remarks>
        public string GetAnsiString(string symbolName)
        {
            var addr = checkedGetSymbolHandle (symbolName);
            return Marshal.PtrToStringAnsi(addr);
        }           
    }
}

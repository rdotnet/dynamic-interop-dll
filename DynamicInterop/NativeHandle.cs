using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace DynamicInterop
{

    /// <summary> Interface for native handle.</summary>
    /// <remarks>This is similar in intent to the BCL SafeHandle, but with release 
    ///          behaviors that are more desirable in particular circumstances.
    ///          </remarks>
    public interface INativeHandle : IDisposable
    {
        /// <summary> Returns the value of the handle.</summary>
        ///
        /// <returns> The handle.</returns>
        IntPtr GetHandle();

        /// <summary>Manually increments the reference counter</summary>
        void AddRef();

        /// <summary>Manually decrements the reference counter. Triggers disposal if count reaches is zero.</summary>
        void Release();
    }

    /// <summary> A stub implementation for the INativeHandle interface </summary>
    public abstract class NativeHandle : INativeHandle
    {
        /// <summary> Specialised constructor for use only by derived class.</summary>
        ///
        /// <param name="pointer">         The handle, value of the pointer to the native object</param>
        /// <param name="currentRefCount"> (Optional) Number of pre-existing references for the native object</param>
        /// <remarks>If a native object was created prior to its use by .NET, its lifetime may need to extend its use 
        ///          from .NET. In practice the scenario is unlikely</remarks>
        protected NativeHandle(IntPtr pointer, int currentRefCount = 0)
        {
            SetHandle(pointer, currentRefCount);
        }

        /// <summary> Specialised default constructor for use only by derived class. 
        ///           Defers setting the handle to the derived class</summary>
        protected NativeHandle()
        {
        }

        private bool IsValidHandle(IntPtr pointer)
        {
            return pointer != IntPtr.Zero;
        }

        /// <summary> Sets a handle.</summary>
        ///
        /// <exception cref="ArgumentException"> Thrown when a pointer is a Zero pointer
        ///                                      .</exception>
        ///
        /// <param name="pointer">         The handle, value of the pointer to the native object</param>
        /// <param name="currentRefCount"> (Optional) Number of pre-existing references for the native object</param>
        /// <remarks>If a native object was created prior to its use by .NET, its lifetime may need to extend its use 
        ///          from .NET. In practice the scenario is unlikely</remarks>
        protected void SetHandle(IntPtr pointer, int currentRefCount = 0)
        {
            if (!IsValidHandle(pointer))
                throw new ArgumentException(string.Format("pointer '{0}' is not valid", pointer.ToString()));
            handle = pointer;
            ReferenceCount = currentRefCount + 1;
        }

        /// <summary> Finaliser. Triggers the disposal of this object if not manually done.</summary>
        ~NativeHandle()
        {
            if(!Disposed)
                Dispose();
        }

        protected abstract bool ReleaseHandle();

        public int ReferenceCount
        {
            get;
            private set;
        }

        public bool Disposed
        {
            get { return IsInvalid; }
        }

        protected IntPtr handle;

        public bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        public void Dispose()
        {
            if (Disposed)
                return;
            if (ReferenceCount <= 0)
                if (ReleaseHandle())
                    handle = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }

        public IntPtr GetHandle()
        {
            return handle;
        }

        public void AddRef()
        {
            ReferenceCount++;
        }

        public void Release()
        {
            ReferenceCount--;
            if (ReferenceCount <= 0)
                Dispose();
        }

    }

}

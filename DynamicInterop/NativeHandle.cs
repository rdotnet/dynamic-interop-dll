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

        /// <summary> Releases the native resource for this handle.</summary>
        ///
        /// <returns> True if it succeeds, false if it fails.</returns>
        protected abstract bool ReleaseHandle();

        /// <summary> Gets the number of references to the native resource for this handle.</summary>
        ///
        /// <value> The number of references.</value>
        public int ReferenceCount
        {
            get;
            private set;
        }

        /// <summary> Gets a value indicating whether this handle has been disposed of already</summary>
        ///
        /// <value> True if disposed, false if not.</value>
        public bool Disposed
        {
            get { return IsInvalid; }
        }

        /// <summary> The handle to the native resource.</summary>
        protected IntPtr handle;

        /// <summary> Gets a value indicating whether this handle is invalid.</summary>
        public bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        /// <summary> If the reference counts allows it, release the resource refered to by this handle.</summary>
        public void Dispose()
        {
            if (Disposed)
                return;
            if (ReferenceCount <= 0)
                if (ReleaseHandle())
                {
                    handle = IntPtr.Zero;
                    GC.SuppressFinalize(this);
                }
        }

        /// <summary> Returns the value of the handle.</summary>
        ///
        /// <returns> The handle.</returns>
        public IntPtr GetHandle()
        {
            return handle;
        }

        /// <summary> Manually increments the reference counter.</summary>
        public void AddRef()
        {
            ReferenceCount++;
        }

        /// <summary> Manually decrements the reference counter. Triggers disposal if count reaches is zero.</summary>
        public void Release()
        {
            ReferenceCount--;
            if (ReferenceCount <= 0)
                Dispose();
        }

    }

}

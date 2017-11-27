using System;
using System.Runtime.InteropServices;

namespace DynamicInterop
{
    /// <summary> 
    /// Extra methods on top of System.Runtime.InteropServices.Marshal for allocating unmanaged memory, copying unmanaged
    /// memory blocks, and converting managed to unmanaged types</summary>
    class MarshalExtra
    {
        /// <summary> Allocates memory from the unmanaged memory of the process for a given type</summary>
        ///
        /// <typeparam name="T"> A type that has an equivalent in the unmanaged world e.g. int or a struct Point </typeparam>
        ///
        /// <returns> A pointer to the newly allocated memory. This memory must be released using the FreeHGlobal(IntPtr) method, or related.</returns>
        public static IntPtr AllocHGlobal<T>()
        {
            int iSize = Marshal.SizeOf(typeof(T));
            return Marshal.AllocHGlobal(iSize);
        }

        /// <summary> Marshals data from an unmanaged block of memory to a newly allocated managed object of the type specified by a generic type parameter. 
        ///           Note it is almost superseded in .NET Framework 4.5.1 and later versions; consider your needs</summary>
        ///
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///                                      illegal values.</exception>
        ///
        /// <typeparam name="T"> The type of the object to which the data is to be copied. This must be a structure.</typeparam>
        /// <param name="ptr">   A pointer to an unmanaged block of memory.</param>
        /// <param name="cleanup"> (Optional) If true, free the native memory block pointed to by ptr. This feature is handy in generated marshalling code.</param>
        ///
        /// <returns> A managed object that contains the data that the ptr parameter points to</returns>
        public static T PtrToStructure<T>(IntPtr ptr, bool cleanup = false) where T : struct
        {
            if (ptr == IntPtr.Zero)
                throw new ArgumentException("pointer must not be IntPtr.Zero");
            T result = (T)Marshal.PtrToStructure(ptr, typeof(T));
            if (cleanup) Marshal.FreeHGlobal(ptr);
            return result;
        }

        /// <summary>Marshals data from a managed object of a specified type to an unmanaged block of memory.
        ///           Note it is almost superseded in .NET Framework 4.5.1 and later versions; consider your needs</summary>
        ///
        /// <typeparam name="T"> The type of the managed object.</typeparam>
        /// <param name="structure"> A managed object that holds the data to be marshaled. The object must be a structure.</param>
        ///
        /// <returns> A pointer to a newly allocated unmanaged block of memory.</returns>
        public static IntPtr StructureToPtr<T>(T structure) where T : struct
        {
            IntPtr ptr = IntPtr.Zero;
            T localStruct = structure;
            int iSize = Marshal.SizeOf(typeof(T));
            ptr = Marshal.AllocHGlobal(iSize);
            Marshal.StructureToPtr<T>(localStruct, ptr, false);
            return ptr;
        }

        /// <summary> Frees all substructures of a specified type that the specified unmanaged memory block points to.</summary>
        ///
        /// <typeparam name="T"> The type of the managed object.</typeparam>
        /// <param name="ptr">           A pointer to an unmanaged block of memory.</param>
        /// <param name="managedObject"> [in,out] The managed object.</param>
        /// <param name="copy">          (Optional) True to copy.</param>
        public static void FreeNativeStruct<T>(IntPtr ptr, ref T managedObject, bool copy = false) where T : struct
        {
            if (ptr == IntPtr.Zero)
                return; //?
            if (copy)
            {
                managedObject = (T)Marshal.PtrToStructure(ptr, typeof(T));
                // Marshal.PtrToStructure<T>(ptr, managedObject);
            }
            Marshal.FreeHGlobal(ptr);
        }

        public static IntPtr ArrayOfStructureToPtr<T>(T[] managedObjects) where T : struct
        {
            int structSize = Marshal.SizeOf<T>();
            IntPtr result = Marshal.AllocHGlobal(managedObjects.Length * structSize);

            for (int i = 0; i < managedObjects.Length; i++)
            {
                int offset = i * structSize;
                Marshal.StructureToPtr(managedObjects[i], IntPtr.Add(result, offset), false);
            }
            return result;
        }

        public static void FreeNativeArrayOfStruct<T>(IntPtr ptr, ref T[] managedObjects, bool copy = false) where T : struct
        {
            if (ptr == IntPtr.Zero)
                return; 
            Marshal.FreeHGlobal(ptr);
        }
    }
}

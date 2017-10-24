using System;
using System.IO;
using System.Security.Permissions;
using Xunit;
using System.Linq;

namespace DynamicInterop.Tests
{
    public class NativeTestLib // : ICustomNativeApi
    {

        public NativeTestLib()
        {
            if (NativeLib == null)
            { // can we use a static constructor, or interplay with load time?
                var fname = PlatformUtility.CreateLibraryFileName("test_native_library");

                var testLibPathEnv = "DynamicInteropTestLibPath";
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(testLibPathEnv)))
                {
                    string guess = GuessTestLibPath();
                    if (Directory.Exists(guess))
                        Environment.SetEnvironmentVariable(testLibPathEnv, guess);
                    else
                    {
                        //testLibPathEnv = "";
                        string msg = "You need to set an environment variable e.g.: " +
                            @"set DynamicInteropTestLibPath=C:\src\dynamic-interop-dll\x64\Debug";
                        throw new Exception(msg);
                    }
                }
                string nativeLibFilename = PlatformUtility.FindFirstFullPath(fname, "test_delegate_library DLL", testLibPathEnv);
                NativeLib = new UnmanagedDll(nativeLibFilename);
            }
        }

        public string GuessTestLibPath()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                bool is64bits = System.Environment.Is64BitProcess;
                //AppDomain.CurrentDomain.BaseDirectory
                //"C:\\src\\github_jm\\dynamic-interop-dll\\DynamicInterop.Tests\\bin\\Debug\\netcoreapp2.0\\"
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string d = baseDir.Trim(Path.DirectorySeparatorChar);
                string baseName = d.Split(Path.DirectorySeparatorChar).Last();
                string dirName = Directory.GetParent(baseDir).FullName;
                int maxIter = 24;
                while (!string.IsNullOrEmpty(baseName) && !(baseName == "dynamic-interop-dll") && maxIter > 0)
                {
                    d = dirName.Trim(Path.DirectorySeparatorChar);
                    baseName = d.Split(Path.DirectorySeparatorChar).Last();
                    dirName = Directory.GetParent(d).FullName;
                    maxIter--;
                }
                if (is64bits)
                    return Path.Combine(d, "x64", "Debug");
                else
                    return Path.Combine(d, "Win32", "Debug");
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public static UnmanagedDll NativeLib { get; private set; }

        private delegate IntPtr create_dog_delegate();
        private delegate IntPtr create_dog_owner_delegate(IntPtr dog);
        private delegate int get_dog_refcount_delegate(IntPtr dog);
        private delegate int get_dog_owner_refcount_delegate(IntPtr owner);
        private delegate void say_walk_delegate(IntPtr owner);
        private delegate void release_delegate(IntPtr obj);
        private delegate int num_dogs_delegate();
        private delegate int num_owners_delegate();

        internal IntPtr create_dog()
        {
            return NativeLib.GetFunction<create_dog_delegate>("create_dog")();
        }

        internal IntPtr create_owner(Dog obj)
        {
            IntPtr objPtr = obj.GetHandle();
            return NativeLib.GetFunction<create_dog_owner_delegate>("create_owner")(objPtr);
        }

        internal int get_dog_refcount(Dog dog)
        {
            IntPtr objPtr = dog.GetHandle();
            return NativeLib.GetFunction<get_dog_refcount_delegate>("get_dog_refcount")(objPtr);
        }

        internal int get_dog_owner_refcount(DogOwner owner)
        {
            IntPtr objPtr = owner.GetHandle();
            return NativeLib.GetFunction<get_dog_owner_refcount_delegate>("get_owner_refcount")(objPtr);
        }

        internal void release(CustomNativeHandle obj)
        {
            IntPtr objPtr = obj.GetHandle();
            NativeLib.GetFunction<release_delegate>("release")(objPtr);
        }

        internal void say_walk(DogOwner owner)
        {
            IntPtr objPtr = owner.GetHandle();
            NativeLib.GetFunction<say_walk_delegate>("say_walk")(objPtr);
        }

        internal static int num_dogs()
        {
            return NativeLib.GetFunction<num_dogs_delegate>("num_dogs")();
        }

        internal static int num_owners()
        {
            return NativeLib.GetFunction<num_owners_delegate>("num_owners")();
        }
    }

    [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    public abstract class CustomNativeHandle : NativeHandle
    {
        internal NativeTestLib api { get; private set; }
        protected CustomNativeHandle(IntPtr pointer, int currentRefCount = 0)
            : this()
        {
        }

        protected CustomNativeHandle()
            : base()
        {
            this.api = new NativeTestLib();
        }

        protected override bool ReleaseHandle()
        {
            this.api.release(this);
            return true;
        }
    }

    [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    public class Dog : CustomNativeHandle
    {
        internal Dog(IntPtr pointer) : base(pointer)
        {
        }

        internal Dog() : base()
        {
            SetHandle(api.create_dog());
        }

        public int NativeReferenceCount
        {
            get { return api.get_dog_refcount(this); }
        }

        public static int NumNativeInstances
        {
            get { return NativeTestLib.num_dogs(); }
        }
    }

    public class DogOwner : CustomNativeHandle
    {
        internal DogOwner(Dog dog) : base()
        {
            SetHandle(api.create_owner(dog));
            this.dog = dog;
            dog.AddRef();
        }
        private Dog dog;

        public int NativeReferenceCount
        {
            get { return api.get_dog_owner_refcount(this); }
        }

        internal void SayWalk()
        {
            api.say_walk(this);
        }

        public static int NumNativeInstances
        {
            get { return NativeTestLib.num_owners(); }
        }

        protected override bool ReleaseHandle()
        {
            dog.Release();
            return base.ReleaseHandle();
        }
    }

    [Collection("Swift Bindings tests (to prevent parallel unit test runs)")]
    public class ReferenceCountingWrappers
    {
        [Fact]
        public void TestNativeObjectReferenceCounting()
        {
            Dog dog = new Dog();
            Assert.Equal(1, dog.ReferenceCount);
            Assert.Equal(1, dog.NativeReferenceCount);
            dog.AddRef();
            Assert.Equal(2, dog.ReferenceCount);
            Assert.Equal(1, dog.NativeReferenceCount);
            DogOwner owner = new DogOwner(dog);
            Assert.Equal(1, owner.ReferenceCount);
            Assert.Equal(3, dog.ReferenceCount);
            Assert.Equal(1, dog.NativeReferenceCount);
            dog.Release();
            Assert.Equal(1, owner.ReferenceCount);
            Assert.Equal(2, dog.ReferenceCount);
            Assert.Equal(1, dog.NativeReferenceCount);
            dog.Release();
            Assert.Equal(1, owner.ReferenceCount);
            Assert.Equal(1, owner.NativeReferenceCount);
            Assert.Equal(1, dog.ReferenceCount);
            Assert.Equal(1, dog.NativeReferenceCount);
            Assert.False(dog.IsInvalid);
            owner.SayWalk();
            owner.Release();
            Assert.Equal(0, owner.ReferenceCount);
            Assert.Equal(0, dog.ReferenceCount);
            // Cannot check on the native ref count - deleted objects. 
            // TODO think of a simple way to test these
            //Assert.Equal(0, owner.NativeReferenceCount);
            //Assert.Equal(0, dog.NativeReferenceCount);
            Assert.True(dog.IsInvalid);
            Assert.True(owner.IsInvalid);
        }

        [Fact]
        public void TestNativeHandleFinalizers()
        {
            NativeTestLib lib = new NativeTestLib();
            int initDogCount = Dog.NumNativeInstances;

            Dog dog = new Dog();
            Assert.Equal(initDogCount + 1, Dog.NumNativeInstances);
            Assert.Equal(1, dog.ReferenceCount);
            Assert.Equal(1, dog.NativeReferenceCount);
            dog = null;
            CallGC();
            CallGC();
            // 2017-10-24 On windows, I cannot get the above calls to trigger GCs. 
            // At least not in debug mode from VS via the test explorer.
            // A Watchpoint but the other tests suggest this is a false flag.
            Assert.Equal(initDogCount, Dog.NumNativeInstances);
        }

        [Fact]
        public void TestNativeHandleDisposal()
        {
            NativeTestLib lib = new NativeTestLib();
            int initDogCount = Dog.NumNativeInstances;

            Dog dog = new Dog();
            Assert.Equal(initDogCount + 1, Dog.NumNativeInstances);
            Assert.Equal(1, dog.ReferenceCount);
            Assert.Equal(1, dog.NativeReferenceCount);
            dog.Dispose();
            Assert.Equal(initDogCount, Dog.NumNativeInstances);
        }

        /// <summary>
        /// Use intended only for unit tests.
        /// </summary>
        public static long CallGC()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            return GC.GetTotalMemory(true);
        }

    }
}

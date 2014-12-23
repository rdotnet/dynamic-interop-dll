using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace DynamicInterop.Tests
{
    public class WrapDynamicLibrary
    {
        [Fact]
        public void ArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => CreateLib(null));
        }

        [Fact]
        public void ArgumentEmpty()
        {
            Assert.Throws<ArgumentException>(() => CreateLib(""));
        }

        [Fact]
        public void InvalidFileName()
        {
            Assert.Throws<DllNotFoundException>(() => CreateLib("SomeVeryUnlikelyName.dll"));
        }

        private void CreateLib(string fname)
        {
            using (var dll = new UnmanagedDll(fname))
            {
                // do nothing.
            }
        }

        [Fact]
        public void WellKnownSystemDll()
        {
            if (PlatformUtility.IsUnix)
                TestLoadLibc();
            else if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                TestLoadKernel32();
            else
                throw new NotSupportedException(PlatformUtility.GetPlatformNotSupportedMsg());
        }

        private void TestLoadLibc()
        {
            // TODO: obviously need to generalize this. 

            //nm -D --defined-only /lib/x86_64-linux-gnu/libc.so.6 | less
            using (var dll = new UnmanagedDll("/lib/x86_64-linux-gnu/libc.so.6"))
            {
                var m = dll.GetFunction<malloc>();
                Assert.NotNull(m);
            }
        }

        private void TestLoadKernel32()
        {
            using (var dll = new UnmanagedDll("kernel32.dll"))
            {
                var beep = dll.GetFunction<Beep>();
                Assert.NotNull(beep);
                //beep(400, 1000);
                var areFileApisAnsi = dll.GetFunction<AreFileApisANSI>();
                Assert.DoesNotThrow(() => areFileApisAnsi());
            }
        }
            
//        #if 32_BITS
//        private delegate bool malloc(int size);
//        #else
        private delegate bool malloc(long size);
//        #endif

        private delegate bool Beep(uint dwFreq, uint dwDuration);
        private delegate bool AreFileApisANSI();

        
    }
}

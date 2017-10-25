
[![Dev Build Status](https://travis-ci.org/screig/dynamic-interop-dll.svg?branch=devel )](https://github.com/screig/dynamic-interop-dll)
[![Master Build Status](https://travis-ci.org/screig/dynamic-interop-dll.svg?branch=master )](https://github.com/screig/dynamic-interop-dll)


dynamic-interop-dll
===================

Facilities to load native DLLs from .NET, on Unix, Windows or MacOS.

# Purpose

This library is designed to dynamic libraries (called shared libraries on unix-type of platforms). The loading mechanism adapts to the operating system it is running on. It is an offshoot from the [R.NET](http://rdotnet.codeplex.com) project (source code now hosted [on github](https://github.com/jmp75/rdotnet)). 

# Installation

You can install this library in your project(s) [as a NuGet package](https://www.nuget.org/packages/DynamicInterop)

# License

This library is covered as of version 0.7.3 by the [MIT license](https://github.com/jmp75/dynamic-interop-dll/blob/3055f27f46d1b794572bcd944eaebbd4f960b9a6/License.txt).

# Usage

An extract from the unit tests:

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

    private delegate bool Beep(uint dwFreq, uint dwDuration);
    private delegate bool AreFileApisANSI();


# Related work

I did notice that [a related library](https://github.com/Boyko-Karadzhov/Dynamic-Libraries) with some overlapping features has been released just a week ago... While I want to explore possibilities to merge these libraries, I have pressing needs to release present library release as a stand-alone package.

# Workaround for some Linux platforms

You normally do not need to install this workaround on most Linux platform. If, however, you get a message 'invalid caller' message from the dlerror function, read on.

On Linux, DynamicInterop calls the dlopen function in "libdl" via P/Invoke to try to load the shared native library, On at least one instance of one Linux flavour (CentOS), it fails and 'dlerror' returns the message 'invalid caller'. See https://rdotnet.codeplex.com/workitem/73 for detailed information. Why this is an "invalid caller" could not be determined. While the exact cause if this failure is unclear, a thin wrapper library around libdl.so works around this issue.

You build and install this workaround with the following commands:

    DYNINTEROP_BIN_DIR=~/my/path/to/DynamicInteropBinaries
    cd libdlwrap
    make
    less sample.config # skim the comments, for information
    cp sample.config $DYNINTEROP_BIN_DIR/DynamicInterop.dll.config
    cp libdlwrap.so  $DYNINTEROP_BIN_DIR/

# Acknowledgements

* Kosei designed and implemented the original multi-platform library loading
* evolvedmicrobe contributed the first implementation that did not require OS-specific conditional compilation.
* Daniel Collins found the workaround necessary for the library loading to work some Linux platforms, via Mono.
* jmp75 refactored, tested, merged contributions and separated from the original R.NET implementation.

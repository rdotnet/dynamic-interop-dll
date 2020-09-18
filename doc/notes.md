
Dev notes and logging references of interest
A bit confused by the expected name of the xml tags in the new csproj files. 
[ms doc](https://docs.microsoft.com/en-us/nuget/guides/create-net-standard-packages-vs2017) suggests using things like `<PackageVersion>` and the like, but using the project properties panel in VS2017 generates only `<Version>`. There is a list in [msbuild-targets](https://docs.microsoft.com/en-us/nuget/schema/msbuild-targets) but this does not aleviate my confusion.


[package versioning](https://docs.microsoft.com/en-us/nuget/reference/package-versioning) note the breaking changes with semantic versioning.

[.net standard version table](http://immo.landwerth.net/netstandard-versions/#) has the merit of conciseness

[.net standard faq](https://github.com/dotnet/standard/blob/master/docs/faq.md)

[NuGet metadata properties for the new csproj](https://docs.microsoft.com/en-us/dotnet/core/tools/csproj#nuget-metadata-properties)

## 2020-09

Preempting the arrival of .NET 5.0. Using the RC1. Installed using snap on Debian.

* [TFMs](https://github.com/dotnet/designs/blob/main/accepted/2020/net5/net5.md)
* [The future of .NET Standard](https://devblogs.microsoft.com/dotnet/the-future-of-net-standard/)

"warning NETSDK1138: The target framework 'netcoreapp2.0' is out of support and will not receive security updates in the future"

```
arting test execution, please wait...
A total of 1 test files matched the specified pattern.
The active test run was aborted. Reason: Test host process crashed

Test Run Aborted.
/snap/dotnet-sdk/100/sdk/5.0.100-rc.1.20452.10/Microsoft.TestPlatform.targets(32,5): error MSB4181: The "Microsoft.TestPlatform.Build.Tasks.VSTestTask" task returned false but did not log an error. [/home/per202/src/github_jm/dynamic-interop-dll/DynamicInterop.Tests/DynamicInterop.Tests.csproj]
```

```sh
dotnet list DynamicInterop_csharp.sln package
cd DynamicInterop.Tests
dotnet list DynamicInterop.Tests.csproj package
dotnet list DynamicInterop.Tests.csproj package
dotnet add DynamicInterop.Tests.csproj package xunit
```

Dev notes and logging references of interest
A bit confused by the expected name of the xml tags in the new csproj files. 
[ms doc](https://docs.microsoft.com/en-us/nuget/guides/create-net-standard-packages-vs2017) suggests using things like `<PackageVersion>` and the like, but using the project properties panel in VS2017 generates only `<Version>`. There is a list in [msbuild-targets](https://docs.microsoft.com/en-us/nuget/schema/msbuild-targets) but this does not aleviate my confusion.


[package versioning](https://docs.microsoft.com/en-us/nuget/reference/package-versioning) note the breaking changes with semantic versioning.

[.net standard version table](http://immo.landwerth.net/netstandard-versions/#) has the merit of conciseness

[.net standard faq](https://github.com/dotnet/standard/blob/master/docs/faq.md)

[NuGet metadata properties for the new csproj](https://docs.microsoft.com/en-us/dotnet/core/tools/csproj#nuget-metadata-properties)
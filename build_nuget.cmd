@echo off

if not defined BuildConfiguration set BuildConfiguration=Release
set THIS_DIR=%~d0%~p0
if not defined pkg_dir set pkg_dir=%THIS_DIR%

set pack_options=-IncludeReferencedProjects -Verbosity normal -Properties Configuration=%BuildConfiguration% -OutputDirectory %pkg_dir%
nuget pack %THIS_DIR%DynamicInterop\DynamicInterop.csproj %pack_options%

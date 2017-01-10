@echo off

@set BuildConfiguration=%1
@set BuildPlatform=%2
@set MSB=%3
@set Mode=%4

@if not "%BuildConfiguration%"=="Release" if not "%BuildConfiguration%"=="Debug" set BuildConfiguration=Release
@if not "%BuildPlatform%"=="x64" if not "%BuildPlatform%"=="Win32" set BuildPlatform=x64
if exist %SwiftBuildDir%\common_setup.cmd call %SwiftBuildDir%\common_setup.cmd

if not defined VS140COMNTOOLS goto build_tag
set VSCOMNTOOLS=%VS140COMNTOOLS%
set VSDEVENV="%VSCOMNTOOLS%..\..\VC\vcvarsall.bat"
@if not exist %VSDEVENV% goto error_no_vcvarsall
@call %VSDEVENV%
set MSB="C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"

:build_tag

@if not defined Mode set Mode=Build

set THIS_DIR=%~d0%~p0
if not defined pkg_dir set pkg_dir=%THIS_DIR%

@set build_options=/t:%Mode% /p:Configuration=%BuildConfiguration% /p:Platform=%BuildPlatform% /m  /consoleloggerparameters:ErrorsOnly

set pack_options=-IncludeReferencedProjects -Verbosity normal -Properties Configuration=%BuildConfiguration% -OutputDirectory %pkg_dir%

%MSB% %THIS_DIR%DynamicInterop.sln %build_options%
nuget pack %THIS_DIR%DynamicInterop\DynamicInterop.nuspec %pack_options%

goto end
:error_no_vcvarsall
@echo ERROR: Cannot find file %VSDEVENV%.
@set exit_code=1
@goto end

:end
exit /b %exit_code%

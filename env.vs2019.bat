rem Customize your build environment and save the modified copy to env.bat

set WEASEL_ROOT=%CD%

rem REQUIRED: path to Boost source directory
if not defined BOOST_ROOT set BOOST_ROOT=%WEASEL_ROOT%\deps\boost_1_78_0

rem OPTIONAL: architecture, Visual Studio version and platform toolset
rem set ARCH=Win32
set BJAM_TOOLSET=msvc-14.2
set CMAKE_GENERATOR="Visual Studio 16 2019"
set PLATFORM_TOOLSET=v142

rem NOTE for ARM64 builds (`build.bat arm64`): install the Visual Studio
rem   component "MSVC v142 - VS 2019 C++ ARM64 build tools".
rem   The ARM64X redirector (arm64x_wrapper) additionally needs the ARM64EC
rem   toolchain, which requires VS 2022 17.4+ (without it, weaselARM64X.dll/.ime
rem   will not be produced).

rem OPTIONAL: path to additional build tools
rem set DEVTOOLS_PATH=%ProgramFiles%\Git\cmd;%ProgramFiles%\Git\usr\bin;%ProgramFiles%\CMake\bin;

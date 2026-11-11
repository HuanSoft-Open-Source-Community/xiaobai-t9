rem Customize your build environment and save the modified copy to env.bat

set WEASEL_ROOT=%CD%

rem REQUIRED: path to Boost source directory
if not defined BOOST_ROOT set BOOST_ROOT=%WEASEL_ROOT%\deps\boost_1_78_0

rem OPTIONAL: architecture, Visual Studio version and platform toolset
rem set ARCH=Win32
set BJAM_TOOLSET=msvc-14.3
set CMAKE_GENERATOR="Visual Studio 17 2022"
set PLATFORM_TOOLSET=v143

rem NOTE for ARM64 builds (`build.bat arm64`): install the Visual Studio
rem   components "MSVC v143 - VS 2022 C++ ARM64/ARM64EC build tools".
rem   The ARM64EC toolchain requires VS 2022 17.4+ (used by arm64x_wrapper
rem   to produce weaselARM64X.dll/.ime).

rem OPTIONAL: path to additional build tools
rem set DEVTOOLS_PATH=%ProgramFiles%\Git\cmd;%ProgramFiles%\Git\usr\bin;%ProgramFiles%\CMake\bin;

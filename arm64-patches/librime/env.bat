rem Customize your build environment and save the modified copy to env.bat

rem REQUIRED: path to Boost source directory
set BOOST_ROOT=C:\Libraries\boost_1_84_0

rem OPTIONAL: Visual Studio version and platform toolset
rem switched to VS2022 toolchain (v143) for ARM64 builds - 2026-10-14
set BJAM_TOOLSET=msvc-14.3
set CMAKE_GENERATOR="Visual Studio 17 2022"
set CMAKE_GENERATOR_INSTANCE=C:/BuildTools2022
set PLATFORM_TOOLSET=v143

rem OPTIONAL: path to additional build tools
rem set DEVTOOLS_PATH=%ProgramFiles%\Git\cmd;%ProgramFiles%\Git\usr\bin;%ProgramFiles%\CMake\bin;

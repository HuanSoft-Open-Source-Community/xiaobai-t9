@echo off
setlocal enabledelayedexpansion

call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\VC\Auxiliary\Build\vcvarsall.bat" x64
if errorlevel 1 goto error

cd /d c:\weasel

set BOOST_ROOT=C:\Libraries\boost_1_84_0
set PLATFORM_TOOLSET=v142
set WEASEL_ROOT=c:\weasel
set RIME_ROOT=c:\weasel\librime
set ARCH=x64

echo === Step 1: Build deps (x64) ===
cd %RIME_ROOT%
if not exist env.bat copy %WEASEL_ROOT%\env.bat env.bat
call build.bat deps release
if errorlevel 1 (
  echo ERROR: Failed to build deps
  goto error
)

echo === Step 2: Configure librime (x64) ===
cd %RIME_ROOT%
cmake . -Bbuild -G "Visual Studio 16 2019" -A x64 -T v142 -DCMAKE_CONFIGURATION_TYPES:STRING="Release" -DCMAKE_BUILD_TYPE:STRING="Release" -DCMAKE_USER_MAKE_RULES_OVERRIDE:PATH="%RIME_ROOT%\cmake\c_flag_overrides.cmake" -DCMAKE_USER_MAKE_RULES_OVERRIDE_CXX:PATH="%RIME_ROOT%\cmake\cxx_flag_overrides.cmake" -DCMAKE_EXE_LINKER_FLAGS_INIT:STRING="-llibcmt" -DCMAKE_MSVC_RUNTIME_LIBRARY="MultiThreaded" -DBUILD_STATIC=ON -DBUILD_SHARED_LIBS=ON -DBUILD_TEST=OFF -DENABLE_LOGGING=ON -DCMAKE_PREFIX_PATH:PATH="%RIME_ROOT%" -DCMAKE_INSTALL_PREFIX:PATH="%RIME_ROOT%\dist_x64"
if errorlevel 1 (
  echo ERROR: CMake configuration failed
  goto error
)

echo === Step 3: Build librime (x64, single-threaded to avoid PDB conflicts) ===
cmake --build build --config Release --target install -- /m:1
if errorlevel 1 (
  echo ERROR: Librime build failed
  goto error
)

echo === Step 4: Copy x64 artifacts ===
if not exist %WEASEL_ROOT%\lib64 mkdir %WEASEL_ROOT%\lib64
copy /Y %RIME_ROOT%\dist_x64\lib\rime.lib %WEASEL_ROOT%\lib64\
if errorlevel 1 goto error
copy /Y %RIME_ROOT%\dist_x64\bin\rime.dll %WEASEL_ROOT%\output\
if errorlevel 1 goto error
copy /Y %RIME_ROOT%\dist_x64\include\rime_*.h %WEASEL_ROOT%\include\
if errorlevel 1 goto error

echo === x64 BUILD SUCCEEDED ===
goto exit

:error
echo === BUILD FAILED ===
exit /b 1

:exit
exit /b 0

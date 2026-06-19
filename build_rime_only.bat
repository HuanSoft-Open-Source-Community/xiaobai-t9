@echo off
call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\VC\Auxiliary\Build\vcvarsall.bat" x64
cd /d c:\weasel\librime
set BOOST_ROOT=C:\Libraries\boost_1_84_0
set RIME_ROOT=c:\weasel\librime
echo === Configure librime (x64, /Z7) ===
cmake . -Bbuild -G "Visual Studio 16 2019" -A x64 -T v142 -DCMAKE_CONFIGURATION_TYPES:STRING="Release" -DCMAKE_BUILD_TYPE:STRING="Release" -DCMAKE_USER_MAKE_RULES_OVERRIDE:PATH="%RIME_ROOT%\cmake\c_flag_overrides.cmake" -DCMAKE_USER_MAKE_RULES_OVERRIDE_CXX:PATH="%RIME_ROOT%\cmake\cxx_flag_overrides.cmake" -DCMAKE_EXE_LINKER_FLAGS_INIT:STRING="-llibcmt" -DCMAKE_MSVC_RUNTIME_LIBRARY="MultiThreaded" -DBUILD_STATIC=ON -DBUILD_SHARED_LIBS=ON -DBUILD_TEST=OFF -DENABLE_LOGGING=ON -DCMAKE_PREFIX_PATH:PATH="%RIME_ROOT%" -DCMAKE_INSTALL_PREFIX:PATH="%RIME_ROOT%\dist_x64"
if errorlevel 1 goto error
echo === Build librime (x64, /m:1) ===
cmake --build build --config Release --target install -- /m:1
if errorlevel 1 goto error
echo === Copy artifacts ===
if not exist c:\weasel\lib64 mkdir c:\weasel\lib64
copy /Y dist_x64\lib\rime.lib c:\weasel\lib64\
copy /Y dist_x64\bin\rime.dll c:\weasel\output\
copy /Y dist_x64\include\rime_*.h c:\weasel\include\
echo === SUCCEEDED ===
goto exit
:error
echo === FAILED ===
exit /b 1
:exit
exit /b 0

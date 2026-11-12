@echo off
setlocal
cd /d "%~dp0"

call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\VC\Auxiliary\Build\vcvars64.bat" >nul 2>&1
if errorlevel 1 (
  echo [ERROR] vcvars64 failed
  exit /b 1
)

cl /nologo /std:c++17 /EHsc /MT /O2 /W3 ^
  /I"%~dp0cfg" ^
  /I"C:\weasel\librime\src" ^
  /I"C:\weasel\librime\include" ^
  /I"C:\Libraries\boost_1_84_0" ^
  build_predict.cc predict_db.cc ^
  /Fe:build_predict.exe ^
  /link "C:\weasel\librime\dist_x64\lib\rime.lib" "C:\weasel\librime\lib_x64\marisa.lib"

if errorlevel 1 (
  echo BUILD_FAILED
  exit /b 1
)
echo BUILD_OK

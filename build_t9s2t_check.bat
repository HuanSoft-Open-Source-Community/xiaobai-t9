@echo off
setlocal
cd /d "%~dp0"
set "MSB=C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
if not exist "%MSB%" set "MSB=C:\BuildTools2022\MSBuild\Current\Bin\MSBuild.exe"
"%MSB%" "%~dp0t9s2t\t9s2t\t9s2t.csproj" /p:Configuration=Debug /p:Platform=AnyCPU /m /v:m
if errorlevel 1 (
  echo BUILD_FAILED
  exit /b 1
)
echo BUILD_OK
exit /b 0

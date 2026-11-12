@echo off
setlocal
rem Usage: build_ui.bat <path-to-csproj> [Configuration]
rem Build a C# project with the VS2019 toolchain (handles .resx correctly).

set "PROJ=%~1"
if "%PROJ%"=="" (
  echo [ERROR] Missing project path.
  exit /b 2
)
set "CFG=%~2"
if "%CFG%"=="" set "CFG=Debug"

set "MSB=C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
if not exist "%MSB%" set "MSB=C:\BuildTools2022\MSBuild\Current\Bin\MSBuild.exe"
if not exist "%MSB%" (
  echo [ERROR] MSBuild.exe not found.
  exit /b 3
)

"%MSB%" "%PROJ%" /p:Configuration=%CFG% /p:Platform=AnyCPU /m /v:m /nologo
if errorlevel 1 (
  echo BUILD_FAILED
  exit /b 1
)
echo BUILD_OK
exit /b 0

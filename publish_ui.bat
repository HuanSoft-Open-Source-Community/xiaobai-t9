@echo off
setlocal
rem Build the five UI programs in Release and copy them into output\,
rem so the installer always packs the latest binaries.
rem Usage: publish_ui.bat

set "ROOT=%~dp0"
set "MSB=C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
if not exist "%MSB%" set "MSB=C:\BuildTools2022\MSBuild\Current\Bin\MSBuild.exe"
if not exist "%MSB%" (
  echo [ERROR] MSBuild.exe not found.
  exit /b 3
)

call :one "%ROOT%helpme\helpme\helpme.csproj"      "%ROOT%output\helpme.exe"
call :one "%ROOT%t9configui\t9configui\t9configui.csproj" "%ROOT%output\t9configui.exe"
call :one "%ROOT%t9skin\t9skin\t9skin\t9skin.csproj"  "%ROOT%output\t9skin.exe"
call :one "%ROOT%update\update\update.csproj"      "%ROOT%output\update.exe"
call :one "%ROOT%t9s2t\t9s2t\t9s2t.csproj"          "%ROOT%output\t9s2t\t9s2t.exe"

echo.
echo === output 目录现有产物 ===
for %%f in (output\helpme.exe output\t9configui.exe output\t9skin.exe output\update.exe output\t9s2t\t9s2t.exe) do (
  for %%t in ("%%f") do echo   %%~nxt  %%~tt  %%~zt bytes
)
pause
exit /b 0

:one
echo.
echo === %~nx1 ===
"%MSB%" "%~1" /p:Configuration=Release /p:Platform=AnyCPU /m /v:m /nologo
if errorlevel 1 (
  echo [FAILED] %~nx1
  exit /b 1
)
set "SRC=%~dp1bin\Release\%~nx1"
if exist "%SRC%" (
  copy /y "%SRC%" "%~2" >nul
  echo   copied -^> %~2
) else (
  echo [WARN] 未找到 %SRC%
)
exit /b 0

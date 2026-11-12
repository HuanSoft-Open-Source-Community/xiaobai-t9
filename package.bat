@echo off
setlocal
cd /d "%~dp0"

rem ============================================================
rem  One-click installer build.
rem  - no argument : version = today's date (YYYY.MM.DD)
rem  - with arg    : version = the argument, e.g. package.bat 2026.11.20
rem  Output: output\archives\xiaobait9-<version>-installer.exe
rem ============================================================

set "NSIS=C:\Program Files\NSIS_v3.03_Build_20180211-yhxs3344\Bin\makensis.exe"
if not exist "%NSIS%" set "NSIS=C:\Program Files (x86)\NSIS\makensis.exe"
if not exist "%NSIS%" (
  echo [ERROR] makensis.exe not found.
  echo         Edit NSIS path in this file.
  pause
  exit /b 1
)

if not exist "output\install.nsi" (
  echo [ERROR] output\install.nsi not found.
  pause
  exit /b 1
)

echo.
echo === Building installer ===
if "%~1"=="" (
  "%NSIS%" /V2 "output\install.nsi"
) else (
  "%NSIS%" /V2 /DWEASEL_VERSION=%~1 "output\install.nsi"
)
if errorlevel 1 (
  echo.
  echo [FAILED] Build error. See messages above.
  pause
  exit /b 1
)

echo.
echo === Done. Newest packages in output\archives ===
for /f "delims=" %%f in ('dir /b /o-d "output\archives\xiaobait9-*-installer.exe" 2^>nul') do (
  for %%t in ("output\archives\%%f") do echo   %%~nxt   %%~tt   %%~zt bytes
)

echo.
echo Opening output\archives ...
start "" "output\archives"
exit /b 0

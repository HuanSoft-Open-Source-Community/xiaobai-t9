@echo off
setlocal
cd /d "%~dp0"

rem Build WeaselTSF + WeaselIME (Win32 and x64, Release) to verify the CapsLock digit fix.
rem Products: output\weasel.dll / weasel.ime / weaselx64.dll / weaselx64.ime

set "MSB=C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
if not exist "%MSB%" set "MSB=C:\BuildTools2022\MSBuild\Current\Bin\MSBuild.exe"
if not exist "%MSB%" (
  echo [ERROR] MSBuild.exe not found. Install VS2019 or BuildTools2022.
  pause
  exit /b 1
)

echo.
echo === WeaselTSF ^(Win32 Release^) ===
"%MSB%" weasel.sln /t:WeaselTSF /p:Configuration=Release /p:Platform=Win32 /m /v:m
if errorlevel 1 goto fail

echo.
echo === WeaselIME ^(Win32 Release^) ===
"%MSB%" weasel.sln /t:WeaselIME /p:Configuration=Release /p:Platform=Win32 /m /v:m
if errorlevel 1 goto fail

echo.
echo === WeaselTSF ^(x64 Release^) ===
"%MSB%" weasel.sln /t:WeaselTSF /p:Configuration=Release /p:Platform=x64 /m /v:m
if errorlevel 1 goto fail

echo.
echo === WeaselIME ^(x64 Release^) ===
"%MSB%" weasel.sln /t:WeaselIME /p:Configuration=Release /p:Platform=x64 /m /v:m
if errorlevel 1 goto fail

echo.
echo === Done. Output files ===
for %%f in (output\weasel.dll output\weasel.ime output\weaselx64.dll output\weaselx64.ime) do if exist "%%f" (
  for %%t in ("%%f") do echo   %%~nxt  %%~tt  %%~zt bytes
)
echo.
echo Test: CapsLock ON + top-row 5 should type 5, not start a Chinese composition.
pause
exit /b 0

:fail
echo.
echo [ERROR] Build failed. Scroll up for details.
pause
exit /b 1

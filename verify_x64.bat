@echo off
call C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\VC\Auxiliary\Build\vcvarsall.bat x64 >nul 2>&1

echo === Checking lib64\rime.lib ===
if exist c:\weasel\lib64\rime.lib (
    echo File exists
    dumpbin /headers c:\weasel\lib64\rime.lib | findstr /i machine
) else (
    echo File NOT found!
)

echo.
echo === Checking output\rime.dll ===
if exist c:\weasel\output\rime.dll (
    echo File exists
    dumpbin /headers c:\weasel\output\rime.dll | findstr /i machine
) else (
    echo File NOT found!
)

echo.
echo === Checking lib\rime.lib (Win32) ===
if exist c:\weasel\lib\rime.lib (
    echo File exists
    dumpbin /headers c:\weasel\lib\rime.lib | findstr /i machine
) else (
    echo File NOT found
)

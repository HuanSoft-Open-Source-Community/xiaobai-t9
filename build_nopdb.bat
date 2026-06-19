@echo off
call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\VC\Auxiliary\Build\vcvarsall.bat" x64
cd /d c:\weasel\librime
echo === Build librime (x64, no compiler PDB) ===
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

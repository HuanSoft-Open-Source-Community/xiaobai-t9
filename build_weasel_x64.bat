@echo off
call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\VC\Auxiliary\Build\vcvarsall.bat" x64 >nul 2>&1

echo Building WeaselServer Release x64
msbuild "c:\weasel\weasel.sln" /t:WeaselServer /p:Configuration=Release /p:Platform=x64 /m:4 /v:minimal

echo Exit code: %errorlevel%
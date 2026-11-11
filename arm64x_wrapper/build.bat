
@echo off
setlocal

REM Build the ARM64X (CHPE) wrappers weaselARM64X.dll / weaselARM64X.ime.
REM The arm64 cl.exe accepts /arm64EC directly, so instead of calling
REM vsdevcmd (which needs reg.exe - blocked in some environments and may
REM kill the process tree) we set up the toolchain with the manual wrapper.
call "C:\weasel_build\vcvarsall_wrapper.bat" arm64
if errorlevel 1 exit /b 1

REM Build dummy object files
cl.exe /c /Fo:dummy.o dummy.c || exit /b 1
cl.exe /c /arm64EC /Fo:dummy_x64.o dummy.c || exit /b 1

REM Build weasel.dll wrapper
link.exe /lib /machine:x64 /def:WeaselTSF_x64.def /out:WeaselTSF_x64.lib /ignore:4104
link.exe /lib /machine:arm64 /def:WeaselTSF_arm64.def /out:WeaselTSF_arm64.lib /ignore:4104
link.exe /dll /noentry /machine:arm64x /defArm64Native:WeaselTSF_arm64.def /def:WeaselTSF_x64.def ^
  /out:weaselARM64X.dll dummy.o dummy_x64.o WeaselTSF_x64.lib WeaselTSF_arm64.lib /ignore:4104

REM Build weasel.ime wrapper
link.exe /lib /machine:x64 /def:WeaselIME_x64.def /out:WeaselIME_x64.lib /ignore:4104
link.exe /lib /machine:arm64 /def:WeaselIME_arm64.def /out:WeaselIME_arm64.lib /ignore:4104
link.exe /dll /noentry /machine:arm64x /defArm64Native:WeaselIME_arm64.def /def:WeaselIME_x64.def ^
  /out:weaselARM64X.ime dummy.o dummy_x64.o WeaselIME_x64.lib WeaselIME_arm64.lib /ignore:4104

endlocal
exit /b

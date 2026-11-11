@echo off

setlocal

if not exist env.bat copy env.bat.template env.bat

if exist env.bat call env.bat

if not defined WEASEL_ROOT set WEASEL_ROOT=%CD%

if not defined VERSION_MAJOR set VERSION_MAJOR=0
if not defined VERSION_MINOR set VERSION_MINOR=17
if not defined VERSION_PATCH set VERSION_PATCH=4

if not defined WEASEL_VERSION set WEASEL_VERSION=%VERSION_MAJOR%.%VERSION_MINOR%.%VERSION_PATCH%
if not defined WEASEL_BUILD set WEASEL_BUILD=0

rem use numeric build version for release build
set PRODUCT_VERSION=%WEASEL_VERSION%.%WEASEL_BUILD%
rem for non-release build, try to use git commit hash as product build version
if not defined RELEASE_BUILD (
  rem check if git is installed and available, then get the short commit id of head
  git --version >nul 2>&1
  if not errorlevel 1 (
    for /f "delims=" %%i in ('git tag --sort=-creatordate ^| findstr /r "%WEASEL_VERSION%"') do (
      set LAST_TAG=%%i
      goto found_tag
    )
    :found_tag
    for /f "delims=" %%i in ('git rev-list %LAST_TAG%..HEAD --count') do (
      set WEASEL_BUILD=%%i
    )
    rem get short commmit id of head
    for /F %%i in ('git rev-parse --short HEAD') do (set PRODUCT_VERSION=%WEASEL_VERSION%.%WEASEL_BUILD%.%%i)
  )
)

rem FILE_VERSION is always 4 numbers; same as PRODUCT_VERSION in release build
if not defined FILE_VERSION set FILE_VERSION=%WEASEL_VERSION%.%WEASEL_BUILD%

echo PRODUCT_VERSION=%PRODUCT_VERSION%
echo WEASEL_VERSION=%WEASEL_VERSION%
echo WEASEL_BUILD=%WEASEL_BUILD%
echo WEASEL_ROOT=%WEASEL_ROOT%
echo WEASEL_BUNDLED_RECIPES=%WEASEL_BUNDLED_RECIPES%
echo.

if defined GITHUB_ENV (
	setlocal enabledelayedexpansion
	echo git_ref_name=%PRODUCT_VERSION%>>!GITHUB_ENV!
)

if defined BOOST_ROOT (
  if exist "%BOOST_ROOT%\boost" goto boost_found
)
echo Error: Boost not found! Please set BOOST_ROOT in env.bat.
exit /b 1

:boost_found
echo BOOST_ROOT=%BOOST_ROOT%
echo.

if not defined BJAM_TOOLSET (
  rem the number actually means platform toolset, not %VisualStudioVersion%
  set BJAM_TOOLSET=msvc-14.2
)

if not defined PLATFORM_TOOLSET (
  set PLATFORM_TOOLSET=v142
)

if defined DEVTOOLS_PATH set PATH=%DEVTOOLS_PATH%%PATH%

set build_config=Release
set build_option=/t:Build
set build_boost=0
set boost_build_variant=release
set build_data=0
set build_opencc=0
set build_rime=0
set rime_build_variant=release
set build_weasel=0
set build_installer=0
set build_arm64=0

rem parse the command line options
:parse_cmdline_options
  if "%1" == "" goto end_parsing_cmdline_options
  if "%1" == "debug" (
    set build_config=Debug
    set boost_build_variant=debug
    set rime_build_variant=debug
  )
  if "%1" == "release" (
    set build_config=Release
    set boost_build_variant=release
    set rime_build_variant=release
  )
  if "%1" == "rebuild" set build_option=/t:Rebuild
  if "%1" == "boost" set build_boost=1
  if "%1" == "data" set build_data=1
  if "%1" == "opencc" set build_opencc=1
  if "%1" == "rime" set build_rime=1
  if "%1" == "librime" set build_rime=1
  if "%1" == "weasel" set build_weasel=1
  if "%1" == "installer" set build_installer=1
  if "%1" == "arm64" set build_arm64=1
  if "%1" == "all" (
    set build_boost=1
    set build_data=1
    set build_opencc=1
    set build_rime=1
    set build_weasel=1
    set build_installer=1
    set build_arm64=1
  )
  shift
  goto parse_cmdline_options
:end_parsing_cmdline_options

if %build_weasel% == 0 (
if %build_boost% == 0 (
if %build_data% == 0 (
if %build_opencc% == 0 (
if %build_rime% == 0 (
  set build_weasel=1
)))))

rem quit WeaselServer.exe before building
cd /d %WEASEL_ROOT%
if exist output\weaselserver.exe (
  output\weaselserver.exe /q
)

rem check ARM64 build tools before any ARM/ARM64 build step
if %build_arm64% == 1 (
  call :check_arm64
  if errorlevel 1 exit /b 1
)

rem build booost
if %build_boost% == 1 (
  call :build_boost
  if errorlevel 1 exit /b 1
  cd /d %WEASEL_ROOT%
)

rem -------------------------------------------------------------------------
rem build librime x64 and Win32
if %build_rime% == 1 (
  if not exist librime\build.bat (
    git submodule update --init --recursive
  )
  cd %WEASEL_ROOT%\librime
  rem clean cache before building
  for %%a in ( build dist lib ^
    deps\glog\build ^
    deps\googletest\build ^
    deps\leveldb\build ^
    deps\marisa-trie\build ^
    deps\opencc\build ^
    deps\yaml-cpp\build ) do (
      if exist %%a rd /s /q %%a
  )

  rem build x64 librime
  set ARCH=x64
  call :build_librime_platform x64 %WEASEL_ROOT%\lib64 %WEASEL_ROOT%\output
  rem build Win32 librime
  set ARCH=Win32
  call :build_librime_platform Win32 %WEASEL_ROOT%\lib %WEASEL_ROOT%\output\Win32
  rem build ARM64 librime (native WeaselServer; enabled by `build.bat arm64`)
  if %build_arm64% == 1 (
    if not exist %WEASEL_ROOT%\lib64arm mkdir %WEASEL_ROOT%\lib64arm
    if not exist %WEASEL_ROOT%\output\ARM64 mkdir %WEASEL_ROOT%\output\ARM64
    set ARCH=ARM64
    call :build_librime_platform ARM64 %WEASEL_ROOT%\lib64arm %WEASEL_ROOT%\output\ARM64
    if errorlevel 1 exit /b 1
  )
  rem clean the modified file
  rem git checkout .
  rem git submodule foreach git checkout .
)

rem -------------------------------------------------------------------------
if %build_weasel% == 1 (
  if not exist output\data\essay.txt (
    set build_data=1
  )
  if not exist output\data\opencc\TSCharacters.ocd* (
    set build_opencc=1
  )
)
if %build_data% == 1 call :build_data
if %build_opencc% == 1 call :build_opencc_data

if %build_weasel% == 0 goto end

cd /d %WEASEL_ROOT%

set WEASEL_PROJECT_PROPERTIES=BOOST_ROOT^
  PLATFORM_TOOLSET^
  VERSION_MAJOR^
  VERSION_MINOR^
  VERSION_PATCH^
  PRODUCT_VERSION^
  FILE_VERSION

cscript.exe render.js weasel.props %WEASEL_PROJECT_PROPERTIES%

del msbuild*.log

if %build_arm64% == 1 (

  msbuild.exe weasel.sln %build_option% /p:Configuration=%build_config% /p:Platform="ARM" /fl6
  if errorlevel 1 goto error
  msbuild.exe weasel.sln %build_option% /p:Configuration=%build_config% /p:Platform="ARM64" /fl5
  if errorlevel 1 goto error
)

msbuild.exe weasel.sln %build_option% /p:Configuration=%build_config% /p:Platform="x64" /fl2
if errorlevel 1 goto error
msbuild.exe weasel.sln %build_option% /p:Configuration=%build_config% /p:Platform="Win32" /fl1
if errorlevel 1 goto error

if %build_arm64% == 1 (
  pushd arm64x_wrapper
  call build.bat
  if errorlevel 1 goto error
  popd

  copy arm64x_wrapper\weaselARM64X.dll output
  if errorlevel 1 goto error
  copy arm64x_wrapper\weaselARM64X.ime output
  if errorlevel 1 goto error
)

if %build_installer% == 1 (
  "%ProgramFiles(x86)%"\NSIS\Bin\makensis.exe ^
  /DWEASEL_VERSION=%WEASEL_VERSION% ^
  /DWEASEL_BUILD=%WEASEL_BUILD% ^
  /DPRODUCT_VERSION=%PRODUCT_VERSION% ^
  output\install.nsi
  if errorlevel 1 goto error
)

goto end

rem ---------------------------------------------------------------------------
rem check ARM64 build tools are present (used by `build.bat arm64`)
:check_arm64
  set VSWHERE=
  if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" set VSWHERE="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
  if not defined VSWHERE exit /b 0
  set ARM64_TOOLCHAIN=
  for /f "usebackq tokens=*" %%i in (`%VSWHERE% -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.ARM64 -property installationPath`) do set ARM64_TOOLCHAIN=%%i
  if defined ARM64_TOOLCHAIN goto check_arm64_have_tools
  echo.
  echo Error: MSVC ARM64 build tools not found, cannot build ARM64 targets.
  echo   Install the Visual Studio component for ARM64 C++ builds:
  echo     - VS2019: "MSVC v142 - VS 2019 C++ ARM64 build tools"
  echo     - VS2022: "MSVC v143 - VS 2022 C++ ARM64/ARM64EC build tools"
  echo   Note: arm64x_wrapper also needs the ARM64EC toolchain (VS 2022 17.4+).
  echo.
  exit /b 1
:check_arm64_have_tools
  rem dependency check: ARM64 WinSparkle (required to link the native WeaselServer)
  if exist "%WEASEL_ROOT%\lib64arm\WinSparkle.lib" exit /b 0
  echo.
  echo Error: lib64arm\WinSparkle.lib not found.
  echo   WinSparkle ARM64 files are required to link the native ARM64 WeaselServer.
  echo   Download the official WinSparkle release zip ^(0.9.x, contains ARM64/^), then copy:
  echo     ARM64\Release\WinSparkle.lib  -^>  lib64arm\WinSparkle.lib
  echo     ARM64\Release\WinSparkle.dll  -^>  output\ARM64\WinSparkle.dll
  echo.
  exit /b 1

rem build boost
:build_boost
  set BJAM_OPTIONS_COMMON=-j%NUMBER_OF_PROCESSORS%^
    --with-filesystem^
    --with-json^
    --with-locale^
    --with-regex^
    --with-serialization^
    --with-system^
    --with-thread^
    define=BOOST_USE_WINAPI_VERSION=0x0603^
    toolset=%BJAM_TOOLSET%^
    link=static^
    runtime-link=static^
    --build-type=complete
  
  set BJAM_OPTIONS_X86=%BJAM_OPTIONS_COMMON%^
    architecture=x86^
    address-model=32
  
  set BJAM_OPTIONS_X64=%BJAM_OPTIONS_COMMON%^
    architecture=x86^
    address-model=64
  
  set BJAM_OPTIONS_ARM32=%BJAM_OPTIONS_COMMON%^
    define=BOOST_USE_WINAPI_VERSION=0x0A00^
    architecture=arm^
    address-model=32
  
  set BJAM_OPTIONS_ARM64=%BJAM_OPTIONS_COMMON%^
    define=BOOST_USE_WINAPI_VERSION=0x0A00^
    architecture=arm^
    address-model=64
  
  cd /d %BOOST_ROOT%
  if not exist b2.exe call bootstrap.bat
  if errorlevel 1 goto error
  b2 %BJAM_OPTIONS_X86% stage %BOOST_COMPILED_LIBS%
  if errorlevel 1 goto error
  b2 %BJAM_OPTIONS_X64% stage %BOOST_COMPILED_LIBS%
  if errorlevel 1 goto error
  
  if %build_arm64% == 1 (
    b2 %BJAM_OPTIONS_ARM32% stage %BOOST_COMPILED_LIBS%
    if errorlevel 1 goto error
    b2 %BJAM_OPTIONS_ARM64% stage %BOOST_COMPILED_LIBS%
    if errorlevel 1 goto error
  )
  exit /b

rem ---------------------------------------------------------------------------
:build_data
  copy %WEASEL_ROOT%\LICENSE.txt output\
  copy %WEASEL_ROOT%\README.md output\README.txt
  copy %WEASEL_ROOT%\plum\rime-install.bat output\
  set plum_dir=plum
  set rime_dir=output/data
  set WSLENV=plum_dir:rime_dir
  bash plum/rime-install %WEASEL_BUNDLED_RECIPES%
  if errorlevel 1 goto error
  exit /b

rem ---------------------------------------------------------------------------
:build_opencc_data
  if not exist %WEASEL_ROOT%\librime\share\opencc\TSCharacters.ocd2 (
    cd %WEASEL_ROOT%\librime
    call build.bat deps %rime_build_variant%
    if errorlevel 1 goto error
  )
  cd %WEASEL_ROOT%
  if not exist output\data\opencc mkdir output\data\opencc
  copy %WEASEL_ROOT%\librime\share\opencc\*.* output\data\opencc\
  if errorlevel 1 goto error
  exit /b

rem ---------------------------------------------------------------------------
rem %1 : ARCH
rem %2 : push | pop , push to backup when pop to restore
:stash_build
  pushd %WEASEL_ROOT%\librime
  for %%a in ( build dist lib ^
    deps\glog\build ^
    deps\googletest\build ^
    deps\leveldb\build ^
    deps\marisa-trie\build ^
    deps\opencc\build ^
    deps\yaml-cpp\build ) do (
    if "%2"=="push" (
      if exist %%a  move %%a %%a_%1 
    )
    if "%2"=="pop" (
      if exist %%a_%1  move %%a_%1 %%a 
    )
  )
  popd
  exit /b

rem ---------------------------------------------------------------------------
rem %1 : ARCH
rem %2 : target_path of rime.lib, base %WEASEL_ROOT% or abs path
rem %3 : target_path of rime.dll, base %WEASEL_ROOT% or abs path
:build_librime_platform
  rem restore backuped %1 build
  call :stash_build %1 pop

  cd %WEASEL_ROOT%\librime
  if not exist env.bat (
    copy %WEASEL_ROOT%\env.bat env.bat
  )
  if not exist lib\opencc.lib (
    call build.bat deps %rime_build_variant%
    if errorlevel 1 (
      call :stash_build %1 push
      goto error
    )
  )
  call build.bat %rime_build_variant%
  if errorlevel 1 (
    call :stash_build %1 push
    goto error
  )

  cd %WEASEL_ROOT%\librime
  call :stash_build %1 push

  copy /Y %WEASEL_ROOT%\librime\dist_%1\include\rime_*.h %WEASEL_ROOT%\include\
  if errorlevel 1 goto error
  copy /Y %WEASEL_ROOT%\librime\dist_%1\lib\rime.lib %2\
  if errorlevel 1 goto error
  copy /Y %WEASEL_ROOT%\librime\dist_%1\lib\rime.dll %3\
  if errorlevel 1 goto error

  exit /b
rem ---------------------------------------------------------------------------

:error

echo error building weasel...

:end
cd %WEASEL_ROOT%

rd /S /Q Builds\Current
set ts=%DATE:~6,4%-%DATE:~3,2%-%DATE:~0,2%

:: Build 64 Bit
rd /S /Q Builds\Current
rd /s /q CloudSeed\bin\Release
msbuild CloudSeed.sln /p:Configuration=Release,Platform=x64

mkdir Builds\Current
xcopy CloudSeed\bin\Release\*.dll Builds\Current\CloudSeed\ /s /e /h


:: Create Bridge dlls
cd Builds\Current
..\..\Binaries\SharpSoundDevice\x64\Release\BridgeGenerator.exe CloudSeed\CloudSeed.dll CloudSeed.dll

:: Copy In Factory Presets
xcopy "..\..\Factory Programs" "CloudSeed\Programs\Factory Programs\" /s /e /h

del ..\CloudSeed-%ts%-NoInstall.zip
7z a ..\CloudSeed-%ts%-NoInstall.zip -r *.*
cd..
cd..

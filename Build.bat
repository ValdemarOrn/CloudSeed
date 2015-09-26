rd /S /Q Builds\Current
set ts=%DATE:~11%-%DATE:~8,2%-%DATE:~5,2%

:: Build 32 Bit
rd /s /q CloudSeed\bin\Release
msbuild CloudSeed.sln /p:Configuration=Release,Platform=x86

mkdir Builds\Current\x86
xcopy CloudSeed\bin\Release\*.dll Builds\Current\x86\CloudSeed\ /s /e /h


:: Build 64 Bit
rd /s /q CloudSeed\bin\Release
msbuild CloudSeed.sln /p:Configuration=Release,Platform=x64

mkdir Builds\Current\x64
xcopy CloudSeed\bin\Release\*.dll Builds\Current\x64\CloudSeed\ /s /e /h


:: Create Bridge dlls
cd Builds\Current
..\..\Binaries\SharpSoundDevice\x86\Release\BridgeGenerator.exe CloudSeed\x86\CloudSeed.dll x86\CloudSeed.VST.x86.dll
..\..\Binaries\SharpSoundDevice\x64\Release\BridgeGenerator.exe CloudSeed\x64\CloudSeed.dll x64\CloudSeed.VST.x64.dll

:: Copy In Factory Presets
xcopy "..\..\Factory Programs" "x86\CloudSeed\Programs\Factory Programs\" /s /e /h
xcopy "..\..\Factory Programs" "x64\CloudSeed\Programs\Factory Programs\" /s /e /h

del ..\CloudSeed-%ts%.zip
7z a ..\CloudSeed-%ts%.zip -r *.*
cd..
cd..
rd /S /Q Builds\Current
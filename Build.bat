rd /S /Q Builds\Current

rd Biquad\bin /s /q
rd MidiScript\bin /s /q
rd MrFuzz\bin /s /q
rd Nearfield\bin /s /q
rd Rodent.V2\bin /s /q
rd RXG100\bin /s /q
rd SmashMaster\bin /s /q

msbuild SharpSoundPlugins.sln /p:Configuration=Debug
msbuild SharpSoundPlugins.sln /p:Configuration=Release

set ts=%DATE:~11%-%DATE:~8,2%-%DATE:~5,2%

rd Builds\Current /s /q
mkdir Builds\Current
xcopy Biquad\bin\Release\*.dll Builds\Current\Biquad\ /s /e /h
xcopy MidiScript\bin\Release\*.dll Builds\Current\MidiScript\ /s /e /h
xcopy MrFuzz\bin\Release\*.dll Builds\Current\MrFuzz\ /s /e /h
xcopy Nearfield\bin\Release\*.dll Builds\Current\Nearfield\ /s /e /h
xcopy Rodent.V2\bin\Release\*.dll Builds\Current\Rodent.V2\ /s /e /h
xcopy RXG100\bin\Release\*.dll Builds\Current\RXG100\ /s /e /h
xcopy SmashMaster\bin\Release\*.dll Builds\Current\SmashMaster\ /s /e /h

cd Builds\Current

..\..\Binaries\SharpSoundDevice\x86\Release\BridgeGenerator.exe Biquad\Biquad.dll Biquad.VST.x86.dll
..\..\Binaries\SharpSoundDevice\x86\Release\BridgeGenerator.exe MidiScript\MidiScript.dll MidiScript.VST.x86.dll
..\..\Binaries\SharpSoundDevice\x86\Release\BridgeGenerator.exe MrFuzz\MrFuzz.dll MrFuzz.VST.x86.dll
..\..\Binaries\SharpSoundDevice\x86\Release\BridgeGenerator.exe Nearfield\Nearfield.dll Nearfield.VST.x86.dll
..\..\Binaries\SharpSoundDevice\x86\Release\BridgeGenerator.exe Rodent.V2\Rodent.V2.dll Rodent.V2.VST.x86.dll
..\..\Binaries\SharpSoundDevice\x86\Release\BridgeGenerator.exe RXG100\RXG100.dll RXG100.VST.x86.dll
..\..\Binaries\SharpSoundDevice\x86\Release\BridgeGenerator.exe SmashMaster\SmashMaster.dll SmashMaster.VST.x86.dll

..\..\Binaries\SharpSoundDevice\x64\Release\BridgeGenerator.exe Biquad\Biquad.dll Biquad.VST.x64.dll
..\..\Binaries\SharpSoundDevice\x64\Release\BridgeGenerator.exe MidiScript\MidiScript.dll MidiScript.VST.x64.dll
..\..\Binaries\SharpSoundDevice\x64\Release\BridgeGenerator.exe MrFuzz\MrFuzz.dll MrFuzz.VST.x64.dll
..\..\Binaries\SharpSoundDevice\x64\Release\BridgeGenerator.exe Nearfield\Nearfield.dll Nearfield.VST.x64.dll
..\..\Binaries\SharpSoundDevice\x64\Release\BridgeGenerator.exe Rodent.V2\Rodent.V2.dll Rodent.V2.VST.x64.dll
..\..\Binaries\SharpSoundDevice\x64\Release\BridgeGenerator.exe RXG100\RXG100.dll RXG100.VST.x64.dll
..\..\Binaries\SharpSoundDevice\x64\Release\BridgeGenerator.exe SmashMaster\SmashMaster.dll SmashMaster.VST.x64.dll

cd Builds\Current
rm ../SharpSoundPlugins-%ts%.zip
7z a ../SharpSoundPlugins-%ts%.zip -r *.*
cd..
cd..
rd /S /Q Builds\Current
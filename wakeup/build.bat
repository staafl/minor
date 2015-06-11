:: embed resource file alarm.wav
:: link to System.Winforms
:: use 2.0 compiler if possible
:: http://stackoverflow.com/questions/4036754/why-does-only-the-first-line-of-this-windows-batch-file-execute-but-all-three-li 2013-03-05
@call csc /target:winexe /out:.\build\wakeup.exe ".\src\program.cs" ".\src\interop.cs" /debug+ /nologo /res:alarm-clock-1.wav /r:".\ref\CoreAudioApi\CoreAudioApi.dll" /r:".\ref\NAudio\NAudio.dll" /main:Program
@copy ".\ref\CoreAudioApi\2013-03-07\build\*" build
@copy ".\ref\NAudio\*" build
@call docs.bat
@call for /f "delims=" %%a in ('mydate') do @set date=%%a
@call 7z a .\build\wakeup-%date%.zip .\build\*.exe .\build\*.pdb .\build\*.wav .\build\*.html
@call cp -f .\build\wakeup-%date%.zip .\build\wakeup-latest.zip
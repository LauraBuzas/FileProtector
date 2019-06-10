@echo on
echo %1%
echo %2%
"c:\Program Files (x86)\Windows Kits\10\Debuggers\x64\symstore.exe" add /f %1\\%2.pdb /s "d:\symstore" /t %2

if exist %1\\%2.sys (
    "c:\Program Files (x86)\Windows Kits\10\Debuggers\x64\symstore.exe" add /f %1\\%2.sys /s "d:\symstore" /t %2
)

if exist %1\\%2.exe (
    "c:\Program Files (x86)\Windows Kits\10\Debuggers\x64\symstore.exe" add /f "%1\\%2.exe" /s "d:\symstore" /t %2
)

if exist %1\\%2.dll (
    "c:\Program Files (x86)\Windows Kits\10\Debuggers\x64\symstore.exe" add /f "%1\\%2.dll" /s "d:\symstore" /t %2
)

if exist %1\\%2.lib (
    "c:\Program Files (x86)\Windows Kits\10\Debuggers\x64\symstore.exe" add /f %1\\%2.lib /s "d:\symstore" /t %2
)

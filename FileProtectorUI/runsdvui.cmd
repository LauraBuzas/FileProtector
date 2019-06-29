cd /d "D:\FileProtector\FileProtectorUI" &msbuild "FileProtectorUI.csproj" /t:sdvViewer /p:configuration="Debug" /p:platform="Any CPU" /p:SolutionDir="D:\FileProtector" 
exit %errorlevel% 
@ECHO OFF

%Windir%\System32\WindowsPowerShell\v1.0\Powershell.exe write-host -foregroundcolor Green Cleaning up Debug build...
C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild Idecom.Bus.sln /v:q /p:VisualStudioVersion=12.0 /m  /target:Clean /p:Configuration=Debug /nologo
%Windir%\System32\WindowsPowerShell\v1.0\Powershell.exe write-host -foregroundcolor Green Cleaning up Release build...
C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild Idecom.Bus.sln /v:q /p:VisualStudioVersion=12.0 /m  /target:Clean /p:Configuration=Release /nologo
%Windir%\System32\WindowsPowerShell\v1.0\Powershell.exe write-host -foregroundcolor Green Deleting packages folder...
rd packages /S /Q

%Windir%\System32\WindowsPowerShell\v1.0\Powershell.exe write-host -foregroundcolor Green Done.
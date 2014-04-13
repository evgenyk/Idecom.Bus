call build.cmd

del _GeneratedNuGetPackages /F /S /Q
MD _GeneratedNuGetPackages
for /f %%X IN ('dir /s /b idecom*.nuspec') do .nuget\nuget pack "%%~fX" -OutputDirectory _GeneratedNuGetPackages -Verbosity detailed -Symbols
for /f %%X IN ('dir /s /b idecom*implementation*.nuspec') do .nuget\nuget pack "%%~fX" -OutputDirectory _GeneratedNuGetPackages -Verbosity detailed

CD _GeneratedNuGetPackages
for /f %%X IN ('dir /b *.nupkg') do ..\.nuget\NuGet.exe push "%%~fX"
CD ..
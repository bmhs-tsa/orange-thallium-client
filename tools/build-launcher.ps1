# Build the full-trust-launcher
# This file is automatically executed each time before the UWP app is built

# Get the MSBuild location
$msbuild = Get-ChildItem "C:\Program Files (x86)\Microsoft Visual Studio\*\*\MSBuild\Current\Bin\MSBuild.exe"

# Build the full-trust launcher
Start-Process -FilePath $msbuild -ArgumentList "$PSScriptRoot/../launcher/launcher.sln", "/property:Configuration=Release" -NoNewWindow
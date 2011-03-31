# MS Build location. 
# Update this for different framework versions or if you get any errors:
$build = "$env:windir\Microsoft.NET\Framework\v3.5\MSBuild.exe"

if (-not (Test-Path $build)) {exit -1}

# Location of this script, should be alongside the .sln file for FluentBDD
$base = Split-Path -parent $MyInvocation.MyCommand.Definition

# Find NUnit installations
$nunits = ls -Path "$env:ProgramFiles" -Filter "nunit.exe" -Recurse | %{Split-Path $_.FullName -Parent}
$nunits += ls -Path "$env:ProgramFiles (x86)" -Filter "nunit.exe" -Recurse | %{Split-Path $_.FullName -Parent}

# Loop through all nunit directories, copy from {$nunits}\lib\ {$base}\Dependencies\
#   for each of nunit.core.dll, nunit.core.interfaces.dll, nunit.framework.dll
#   then build the .sln, and copy from {$base}\FluentBddNUnitExtension\bin\Debug to {$nunits}\addins

foreach ($nunit in $nunits) {
	cp "$nunit\lib\nunit.core.dll" -Destination "$base\Dependencies\" -Force
	cp "$nunit\lib\nunit.core.interfaces.dll" -Destination "$base\Dependencies\" -Force
	cp "$nunit\lib\nunit.framework.dll" -Destination "$base\Dependencies\" -Force
	
	& $build "$base\FluentBDD.sln" /t:rebuild /p:OutputDir=bin\Debug
	#& $build "$base\FluentBDD.sln" /p:OutputDir=bin\Debug
	
	mkdir "$nunit\addins"
	
	cp "$base\FluentBddNUnitExtension\bin\Debug\FluentBDD.dll" -Destination "$nunit\addins" -Force
	cp "$base\FluentBddNUnitExtension\bin\Debug\FluentBddNUnitExtension.dll" -Destination "$nunit\addins" -Force
}

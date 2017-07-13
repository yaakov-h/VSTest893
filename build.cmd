@echo off
pushd .
cd %~dp0%

echo Restoring NuGet packages...
nuget.exe install Microsoft.TestPlatform -OutputDirectory bin\TestPlatform -Version 15.0.0

echo Building...
dotnet restore Repro893\Repro893.sln
dotnet build Repro893\Repro893.sln

echo Done.
popd
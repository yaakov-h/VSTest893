@echo off
pushd .
cd %~dp0%

echo Running TestRunner for test assembly targeting .NET Framework 4.6...
echo.
bin\net46\TestRunner.exe bin\net46\TestProject.exe
echo.

echo Running TestRunner for test assembly tageting .NET Core 1.1...
echo.
bin\net46\TestRunner.exe bin\netcoreapp1.1\TestProject.dll
echo.

popd
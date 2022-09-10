@echo off

pushd %~dp0

set /p apiKey=<nuget.apikey

dotnet nuget push "*.nupkg" --api-key %apiKey% --source https://api.nuget.org/v3/index.json

popd

echo.
pause
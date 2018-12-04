@echo off

echo.
dotnet pack --include-symbols --include-source --configuration Release src\DeepEqual\DeepEqual.csproj

echo.
pause
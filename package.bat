@echo off

echo.
dotnet pack --include-symbols --include-source --configuration Release --output ..\.. src\DeepEqual\DeepEqual.csproj

echo.
pause
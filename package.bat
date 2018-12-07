@echo off

echo.
echo === TEST ===
echo.
dotnet test  --configuration Release src/DeepEqual.Test/DeepEqual.Test.csproj

echo.
echo === PACKAGE ===
echo.
dotnet pack --include-symbols --include-source --configuration Release --output ..\.. src\DeepEqual\DeepEqual.csproj

echo.
pause
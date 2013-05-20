@echo off

call msbuild src\DeepEqual\DeepEqual.csproj /t:rebuild /p:Configuration=Release

echo.
NuGet pack src\DeepEqual\DeepEqual.csproj -Prop Configuration=Release

echo.
pause
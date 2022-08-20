@echo off

pushd %~dp0
pushd src

echo.
echo === TEST ===
echo.
dotnet test  --configuration Release

echo.
echo === PACKAGE ===
echo.
dotnet pack ^
  --include-symbols ^
	 -p:SymbolPackageFormat=snupkg ^
	--include-source ^
	--configuration Release ^
	--output .. ^
	DeepEqual\DeepEqual.csproj

dotnet pack ^
  --include-symbols ^
	 -p:SymbolPackageFormat=snupkg ^
	--include-source ^
	--configuration Release ^
	--output .. ^
	DeepEqual.System.Text.Json\DeepEqual.System.Text.Json.csproj

popd
popd

echo.
pause
@echo off

echo.
echo === TEST ===
echo.
dotnet test  --configuration Release src/DeepEqual.Test/DeepEqual.Test.csproj

echo.
echo === PACKAGE ===
echo.
dotnet pack ^
  --include-symbols ^
	 -p:SymbolPackageFormat=snupkg ^
	--include-source ^
	--configuration Release ^
	--output . ^
	src\DeepEqual\DeepEqual.csproj

dotnet pack ^
  --include-symbols ^
	 -p:SymbolPackageFormat=snupkg ^
	--include-source ^
	--configuration Release ^
	--output . ^
	src\DeepEqual.System.Text.Json\DeepEqual.System.Text.Json.csproj

echo.
pause
Push-Location

cd $PSScriptRoot
cd ../Configuration.SecretsManager
dotnet build /p:Configuration=Release
dotnet pack /p:Configuration=Release --output "$PSScriptRoot/../out"

Pop-Location
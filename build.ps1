# KKHungryAlertをビルドする時
# Usage: .\build.ps1 --project="KKHungryAlert"
dotnet run --project CakeBuild/CakeBuild.csproj -- $args
exit $LASTEXITCODE;
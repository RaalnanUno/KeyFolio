Remove-Item Env:KEYFOLIO_SECRET -ErrorAction SilentlyContinue
dotnet run --project .\KeyFolio.Console\KeyFolio.Console.csproj -- encrypt "hello world"

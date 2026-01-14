$env:KEYFOLIO_SECRET="my shared passphrase"
dotnet run --project .\KeyFolio.Console\KeyFolio.Console.csproj -- encrypt "hello world"

dotnet clean
dotnet restore
dotnet build -c Release

# comment A) Confirm the process sees the env vars



[Environment]::GetEnvironmentVariable("MAILFOLIO_PASSPHRASE","Process")
[Environment]::GetEnvironmentVariable("MAILFOLIO_PASSPHRASE","User")
[Environment]::GetEnvironmentVariable("MAILFOLIO_PASSPHRASE","Machine")

[Environment]::GetEnvironmentVariable("MAILFOLIO_KEYFILE","Process")
[Environment]::GetEnvironmentVariable("MAILFOLIO_KEYFILE","User")
[Environment]::GetEnvironmentVariable("MAILFOLIO_KEYFILE","Machine")


dotnet run -c Release -- `
  --to "Rahsaan.Pringle@Traxxis.net" `
  --subject "MailFolio Test" `
  --body "Hello from MailFolio."

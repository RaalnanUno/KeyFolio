PS C:\source\repos\RaalnanUno\KeyFolio\MailFolio.Console> dotnet build -c Release
Restore complete (0.4s)
  KeyFolio.Core net8.0 succeeded (0.1s) → C:\source\repos\RaalnanUno\KeyFolio\KeyFolio.Core\bin\Debug\net8.0\KeyFolio.Core.dll
  MailFolio.Console net8.0 failed with 3 error(s) (0.1s)
    C:\source\repos\RaalnanUno\KeyFolio\MailFolio.Console\Reporting\MailFolioException.cs(3,21): error CS0101: The namespace 'MailFolio.Console.Reporting' already contains a definition for 'MailFolioException'
    C:\source\repos\RaalnanUno\KeyFolio\MailFolio.Console\Reporting\MailFolioException.cs(7,12): error CS0111: Type 'MailFolioException' already defines a member called 'MailFolioException' with the same parameter types
    C:\source\repos\RaalnanUno\KeyFolio\MailFolio.Console\Reporting\MailFolioException.cs(13,12): error CS0111: Type 'MailFolioException' already defines a member called 'MailFolioException' with the same parameter types

Build failed with 3 error(s) in 0.8s
PS C:\source\repos\RaalnanUno\KeyFolio\MailFolio.Console> 


## Reporting/ErrorCodes.cs

![[Reporting/ErrorCodes.md]]

## Reporting/FailureReport.cs

![[Reporting/FailureReport.md]]

## Reporting/MailFolioException.cs

![[Reporting/MailFolioException.md]]

## Mail/MailSender.cs

![[Mail/MailSender.md]]

## Mail/TlsMode.cs

![[Mail/TlsMode.md]]

## Mail/Mail/TlsModeParser.cs
.
![[Mail/Mail/TlsModeParser.md]]
.
## Mail/Mail/Cli/MailFolioArgsParser.cs
.
![[Mail/Mail/Cli/MailFolioArgsParser.md]]
.
## Data/SentMailRepository.cs

![[Data/SentMailRepository.md]]

## Crypto/EnvSecretProvider.cs

![[Crypto/EnvSecretProvider.md]]

## Crypto/KeyFolioBridge.cs

![[Crypto/KeyFolioBridge.md]]

## Config/KeyFileLoader.cs

![[Config/KeyFileLoader.md]]

## Config/MailFolioKeyFile.cs

![[Config/MailFolioKeyFile.md]]

## Cli/MailFolioArgs.cs

![[Cli/MailFolioArgs.md]]


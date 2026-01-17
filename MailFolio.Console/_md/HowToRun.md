$env:MAILFOLIO_PASSPHRASE = "CorrectHorseBatteryStaple"
$env:MAILFOLIO_KEYFILE    = "C:\secrets\mailfolio.key.json"
# optional
$env:MAILFOLIO_DB         = "C:\MailFolio\Data\MailFolio.db"

.\MailFolio.Console.exe --to "someone@agency.mil" --subject "Test" --body "Hello"


```json
{
  "Server": "smtp.yourcompany.mil",
  "Port": 587,
  "TlsMode": "StartTls",
  "FromEmail": "noreply@yourcompany.mil",
  "FromName": "MailFolio",

  "UsernameEnc": "keyfolio:v1:<salt>.<nonce>.<ciphertextTag>",
  "PasswordEnc": "keyfolio:v1:<salt>.<nonce>.<ciphertextTag>"
}

```

```json
{
  "Server": "smtp.yourcompany.mil",
  "Port": 25,
  "TlsMode": "None",
  "FromEmail": "noreply@yourcompany.mil",
  "FromName": "MailFolio",

  "UsernameEnc": "keyfolio:v1:<encrypted envelope of noreply@yourcompany.mil>",
  "PasswordEnc": "keyfolio:v1:<encrypted envelope of your password>"
}

```



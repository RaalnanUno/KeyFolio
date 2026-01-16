# This is my project.

I want to create a Mail sending application (MailFolio) that will make use of KeyFolio.Core to read a file for credentials before sending an email. It would be a console application

Environment Variable
- Encryption passphrase
KeyFile (stored, JSON)
- Server ✅
- Port ✅ (recommended)
- FromEmail ✅
- FromName ✅
- Username ✅ (recommended; encrypted)
- Password ✅ (encrypted)
- TLS mode ✅ (recommended default: StartTLS true)
Runtime
- ToEmail ✅
- Subject ✅
- Body ✅ (or BodyFile)
- Attachments ✅ optional
- HTMLFlag ✅ optional
- Cc, Bcc, ReplyTo ✅ optional
- DryRun, Verbose ✅ operational

We also want to maintain a SQLite db of sent mail.
It should have one table.
- Sent Mail: Messages that have been sent. This will include the message information, and a BLOB file where we will store the success/error data as JSON. We will store the From, To, and the time that the message was sent, but we won't store the message body, or subject.

## KeyFolio.Core.md

![[KeyFolio.Core.md]]

## KeyFolio.md

![[KeyFolio.md]]

## Crypto/Base64Url.cs

![[Crypto/Base64Url.md]]

## Crypto/KeyFolioCrypto.cs

![[Crypto/KeyFolioCrypto.md]]

## Crypto/KeyFolioEnvelope.cs

![[Crypto/KeyFolioEnvelope.md]]

## Crypto/KeyFolioOptions.cs

![[Crypto/KeyFolioOptions.md]]

## Crypto/SecretProviders.cs

![[Crypto/SecretProviders.md]]


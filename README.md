# KeyFolio

## KeyFolio.Core

A library project that accepts a string, and returns it in an encrypted format. The level of encryption doesn't need to be super secure, but it needs to be decipherable even when KeyFolio.Core receives the same string on a different computer. String formatting should be considered, but we won't normalize by default. For example, one ley might be JSON, and another string might be plain text.

When I say “encryptable on one computer / decryptable on another”, I want to go with AES-GCM with a shared secret + Base64Url output
We can store the passphrase as a Environment variable: KEYFOLIO_SECRET, and Prompt user at runtime if the variable is not found.

### do we use that secret directly as the AES key, or do we derive a key from it?
- Derive an AES key from the passphrase (recommended)
- PBKDF2 for v1 (simple, built-in), leave room to swap to Argon2 later.

### If we derive keys from a passphrase, the same passphrase + same salt must produce the same key.
- We choose: Random per-message salt, stored in the output string.

### Output format (“envelope”) so it’s self-describing

- A json envelope for now.

### Authenticating context (AAD) — optional but nice

- include AAD = "KeyFolio:v1" at minimum.

### Secret prompting + UX options

- Env var first
- If missing: prompt with hidden input
- Cache in-memory for the run
- Avoid CLI arg for secret (it leaks to shell history / process list)

### API surface of KeyFolio.Core

- Service object + options (versioning, KDF iterations, output encoding).




## KeyFolio.Console

A console application that connects to `KeyFolio.Core` to implement encryption and decryption. This will serve as a basic usage example for other client projects

## KeyFolio.Client

A form application that connects to `KeyFolio.Core` to implement encryption and decryption. This will serve as a basic implementation example for users that need to make use of KeyFolio without going through the command line.

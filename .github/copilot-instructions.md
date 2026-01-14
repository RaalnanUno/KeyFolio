<!-- Copilot / agent instructions for the KeyFolio repo -->
# KeyFolio — Agent Instructions

Purpose: quickly orient an AI coding agent to what's essential in this repo so changes are safe and productive.

- **Big picture**: three main components:
  - `KeyFolio.Core` (library): encryption primitives, key derivation, and the JSON "envelope" format. See [KeyFolio.Core/Crypto/KeyFolioCrypto.cs](KeyFolio.Core/Crypto/KeyFolioCrypto.cs) and [KeyFolio.Core/KeyFolioEnvelope.cs](KeyFolio.Core/KeyFolioEnvelope.cs).
  - `KeyFolio.Console` (CLI): example usage and runnable examples under [KeyFolio.Console/RunExamples](KeyFolio.Console/RunExamples).
  - `KeyFolio.Client` (WinForms): GUI sample demonstrating integration with the Core library; see [KeyFolio.Client/Form1.cs](KeyFolio.Client/Form1.cs).

- **Security & secrets**:
  - The runtime secret is read by `Secrets/EnvOrPromptSecretProvider.cs` and optionally uses `SecretPromptForm.cs` to prompt on missing env vars.
  - The env var name is `KEYFOLIO_SECRET`. Prefer setting it in the environment for automated runs.
  - Key derivation is PBKDF2 in v1; look at [KeyFolio.Core/KeyFolioOptions.cs](KeyFolio.Core/KeyFolioOptions.cs) and [KeyFolio.Core/Crypto/KeyFolioCrypto.cs](KeyFolio.Core/Crypto/KeyFolioCrypto.cs) when changing KDF parameters.

- **Data formats & algorithms**:
  - Envelope: JSON envelope (self-describing). Inspect [KeyFolio.Core/KeyFolioEnvelope.cs](KeyFolio.Core/KeyFolioEnvelope.cs) for the shape and versioning rules.
  - AES-GCM is used for encryption. Do not change cipher mode without coordinating across Console/Client and envelope versioning.
  - AAD default: `KeyFolio:v1` is used in authenticated encryption — preserve or increment version when altering format.

- **Build / run / debug workflows** (Windows / .NET):
  - Build solution: `dotnet build` at the repo root (targets net8.0).
  - Run console example: `dotnet run -p KeyFolio.Console` or use the PowerShell examples in [KeyFolio.Console/RunExamples](KeyFolio.Console/RunExamples).
  - Run WinForms client: open the solution in Visual Studio or `dotnet run -p KeyFolio.Client` (Windows GUI).
  - To run with a secret in PowerShell: `setx KEYFOLIO_SECRET "your_secret"` (session: `$env:KEYFOLIO_SECRET = 'your_secret'`).

- **Examples & helper scripts**:
  - See the RunExamples folder for PS scripts and README fragments demonstrating piping input and env-var behaviors.

- **Code patterns & conventions** (project-specific):
  - Options objects exist (KeyFolioOptions) — use them rather than scattering constants.
  - Keep envelope/format changes backwards-compatible: bump envelope version and handle both versions when reading.
  - Secrets: prefer environment or the provided prompt provider; avoid adding CLI args for secrets (they leak).
  - Tests: repository does not contain automated unit tests — be cautious and run examples/manual checks after changes.

- **Where to change behavior safely**:
  - Crypto implementation & KDF parameters: [KeyFolio.Core/Crypto/KeyFolioCrypto.cs](KeyFolio.Core/Crypto/KeyFolioCrypto.cs)
  - Envelope format and parsing: [KeyFolio.Core/KeyFolioEnvelope.cs](KeyFolio.Core/KeyFolioEnvelope.cs)
  - Secret handling and UX: [Secrets/EnvOrPromptSecretProvider.cs](Secrets/EnvOrPromptSecretProvider.cs) and [Secrets/SecretPromptForm.cs](Secrets/SecretPromptForm.cs)

- **When to ask a human**:
  - Any change that modifies envelope shape, KDF or cipher parameters, or the AAD string.
  - Changes that affect cross-project public APIs (Core → Console/Client).

If anything above is unclear or you want the agent to expand a specific section (examples, commands, or code pointers), ask and I'll iterate.

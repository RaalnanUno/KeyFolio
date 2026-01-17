<!-- Readme/workflows.html -->
<!doctype html>
<html lang="en">

<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Workflows • KeyFolio.Client Docs</title>

  <!-- Same wiring as security-model.html -->
  <link rel="stylesheet" href="assets/site.css" />
</head>

<body>
  <!-- Header / Nav -->
  <header class="topbar">
    <div class="topbar__inner">
      <a class="brand" href="index.html">KeyFolio.Client Docs</a>

      <nav class="nav">
        <a href="index.html">Home</a>
        <a href="getting-started.html">Getting Started</a>
        <a href="security-model.html">Security Model</a>
        <a class="active" href="workflows.html">Workflows</a>
        <a href="troubleshooting.html">Troubleshooting</a>
      </nav>
    </div>
  </header>

  <main class="page">
    <div class="page__inner">

      <!-- Drop this block into workflows.html (replace the main content area, or place under the H1) -->

      <h1>Workflows</h1>
      <p class="lede">
        KeyFolio.Client supports two main workflows: creating an envelope string (encrypt) and validating an envelope
        string (decrypt). The UI is intentionally “human-paced” to reduce accidental disclosure.
      </p>

      <section class="card">
        <h2>Workflow Sequence Diagram</h2>

        <pre class="mermaid">
sequenceDiagram
  autonumber
  actor User
  participant UI as KeyFolio.Client (Form1)
  participant SP as EnvOrPromptSecretProvider
  participant KF as KeyFolio.Core (KeyFolio)
  participant CL as Clipboard

  rect rgb(245,245,245)
    note over User,UI: Encrypt workflow (Create envelope)
    User->>UI: Enter plaintext in Input
    User->>UI: Click Encrypt
    UI->>SP: GetSecret()
    alt KEYFOLIO_SECRET exists
      SP-->>UI: secret (env)
    else Missing env var
      SP-->>User: Prompt for passphrase (SecretPromptForm)
      User-->>SP: Enter passphrase
      SP-->>UI: secret (cached in-memory)
    end
    UI->>KF: Encrypt(plaintext, secretProvider)
    KF-->>UI: envelope string
    UI-->>User: Show envelope in Output
    User->>UI: Click Copy Output (optional)
    UI->>CL: SetText(envelope)
    UI-->>User: Status: "Output copied"
  end

  rect rgb(245,245,245)
    note over User,UI: Decrypt workflow (Validate envelope)
    User->>UI: Paste envelope in Input
    User->>UI: Click Decrypt
    UI->>SP: GetSecret()
    alt Secret cached/env available
      SP-->>UI: secret
    else Prompt required
      SP-->>User: Prompt for passphrase
      User-->>SP: Enter passphrase
      SP-->>UI: secret (cached)
    end
    UI->>KF: Decrypt(envelope, secretProvider)
    alt Passphrase matches & envelope valid
      KF-->>UI: plaintext
      UI-->>User: Show plaintext in Output
      User->>UI: Click Copy Output (optional)
      UI->>CL: SetText(plaintext)
    else Failure
      KF-->>UI: throws exception
      UI-->>User: Status: "Decrypt failed"
    end
  end
  </pre>
      </section>


      <section class="card">
        <h2>Create an envelope (Encrypt)</h2>
        <ol>
          <li>Paste or type plaintext into <b>Input</b> (JSON or plain text).</li>
          <li>Click <b>Encrypt</b>.</li>
          <li>If prompted, enter the passphrase (or set <code>KEYFOLIO_SECRET</code> before launch).</li>
          <li>Review the generated <b>envelope string</b> in <b>Output</b>.</li>
          <li>Click <b>Copy Output</b> to copy the envelope to clipboard.</li>
          <li>Paste the envelope into your destination (config, vault entry, tool input, etc.).</li>
        </ol>

        <div class="callout">
          <b>Goal:</b> You should be able to store/share the envelope safely. It is useless without the passphrase.
        </div>
      </section>

      <section class="card">
        <h2>Validate an envelope (Decrypt)</h2>
        <ol>
          <li>Paste the envelope string into <b>Input</b>.</li>
          <li>Click <b>Decrypt</b>.</li>
          <li>If prompted, enter the passphrase (must match the one used for encryption).</li>
          <li>Confirm plaintext appears in <b>Output</b>.</li>
          <li>Only click <b>Copy Output</b> if you intentionally need plaintext in clipboard.</li>
        </ol>

        <div class="callout">
          <b>Tip:</b> If your only question is “does this decrypt,” you usually don’t need to copy the plaintext
          anywhere.
        </div>
      </section>

      <section class="card">
        <h2>Fast checks when things fail</h2>
        <ul>
          <li><b>Wrong passphrase:</b> the most common cause of decrypt failures.</li>
          <li><b>Truncated/modified envelope:</b> re-copy the full string (no trimming, no line breaks).</li>
          <li><b>Environment secret not set:</b> the app will prompt each new session by design.</li>
        </ul>

        <p class="muted">
          For deeper help, see <a href="troubleshooting.html">Troubleshooting</a>.
        </p>
      </section>


      <footer class="footer">
        Last opened: <span id="buildStamp"></span>
      </footer>

    </div>
  </main>

  <script src="assets/nav.js"></script>
  <script src="assets/site.js"></script>

  <script type="module">
    import mermaid from "https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs";

    mermaid.initialize({
      startOnLoad: true,
      theme: "forest" // default | neutral | dark | forest | base
    });

  </script>

</body>

</html>
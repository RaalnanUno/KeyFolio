<!-- Readme/security-model.html -->
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Security Model • KeyFolio.Client Docs</title>

  <!-- Per your instruction -->
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
        <a class="active" href="security-model.html">Security Model</a>
        <a href="workflows.html">Workflows</a>
        <a href="troubleshooting.html">Troubleshooting</a>
      </nav>
    </div>
  </header>

  <main class="page">
    <div class="page__inner">

      <h1>Security Model</h1>
      <p class="lede">
        KeyFolio.Client is designed to help humans work with encrypted secrets safely.
        It intentionally adds friction and visibility around sensitive operations.
      </p>

      <section class="card">
        <h2>Core guarantees</h2>
        <ul>
          <li><b>No plaintext secrets are written to disk</b></li>
          <li><b>No passphrases are persisted</b></li>
          <li><b>No clipboard access without explicit user action</b></li>
          <li><b>No background logging of sensitive data</b></li>
        </ul>
      </section>

      <section class="card">
        <h2>Trust boundary</h2>
        <p>
          <b>KeyFolio.Client assumes the local machine is trusted during execution.</b>
          Secrets should still be handled with care.
        </p>

        <div class="callout">
          <b>Reminder:</b> If the machine is compromised (malware, remote access, keyloggers),
          no client app can fully protect secrets typed into it.
        </div>
      </section>

      <section class="card">
        <h2>Passphrases</h2>
        <p>
          The passphrase is never stored by KeyFolio.Client. It is supplied by the user at runtime
          and held only in memory.
        </p>

        <h3>Secret source order</h3>
        <ol>
          <li>Read from environment variable: <code>KEYFOLIO_SECRET</code></li>
          <li>If missing: prompt the user with a secure input dialog</li>
          <li>Cache in memory for the current app session only</li>
        </ol>

        <p class="muted">
          When you close the app, the cached passphrase is gone.
        </p>
      </section>

      <section class="card">
        <h2>Clipboard behavior</h2>
        <p>
          KeyFolio.Client does not automatically copy anything. Clipboard operations happen only when the user clicks
          <b>Copy Output</b>.
        </p>

        <div class="callout">
          <b>Operational note:</b> Clipboard contents are visible to other apps.
          Copy only what you intend to paste, and clear your clipboard when done.
        </div>
      </section>

      <section class="card">
        <h2>Envelope strings</h2>
        <p>
          An envelope string is an encrypted representation of a secret. It is safe to store and transport,
          but useless without the correct passphrase.
        </p>

        <h3>Common failure modes</h3>
        <ul>
          <li>Passphrase mismatch</li>
          <li>Envelope was generated with different options</li>
          <li>Envelope text was modified or truncated</li>
        </ul>
      </section>

      <section class="card">
        <h2>What KeyFolio.Client does not do</h2>
        <ul>
          <li>It does <b>not</b> recover passphrases if they’re lost</li>
          <li>It does <b>not</b> automatically sync or upload secrets anywhere</li>
          <li>It does <b>not</b> keep a history of inputs/outputs</li>
          <li>It does <b>not</b> auto-copy decrypted plaintext to clipboard</li>
        </ul>

        <div class="callout">
          <b>No recovery:</b> Envelopes cannot be decrypted without the original passphrase.
        </div>
      </section>

      <footer class="footer">
        Last opened: <span id="buildStamp"></span>
      </footer>

    </div>
  </main>

  <!-- Per your instruction -->
  <script src="assets/nav.js"></script>
  <script src="assets/site.js"></script>
</body>
</html>

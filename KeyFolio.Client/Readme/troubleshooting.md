<!-- Readme/troubleshooting.html -->
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Troubleshooting • KeyFolio.Client Docs</title>

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
        <a href="workflows.html">Workflows</a>
        <a class="active" href="troubleshooting.html">Troubleshooting</a>
      </nav>
    </div>
  </header>

  <main class="page">
    <div class="page__inner">

      <h1>Troubleshooting</h1>
      <p class="lede">
        Common issues you may hit while encrypting/decrypting envelope strings.
        Most failures come down to passphrases or envelope formatting.
      </p>

      <section class="card">
        <h2>I can’t decrypt an envelope</h2>

        <p>Check these in order:</p>
        <ol>
          <li>
            <b>Passphrase mismatch</b>
            <p class="muted">
              Make sure you entered the exact same shared passphrase used to create the envelope.
              If you’re using <code>KEYFOLIO_SECRET</code>, verify it’s set correctly in the current shell/session.
            </p>
          </li>

          <li>
            <b>Envelope was generated with different options</b>
            <p class="muted">
              If the generating tool/version used different settings, the output may not decrypt the way you expect.
              Ensure the envelope was produced by the same KeyFolio engine + compatible settings.
            </p>
          </li>

          <li>
            <b>Envelope text was modified or truncated</b>
            <p class="muted">
              This is extremely common: accidental whitespace trimming, lost characters, line breaks,
              or pasting into a system that shortens long strings.
            </p>
            <ul>
              <li>Copy/paste the entire envelope string again</li>
              <li>Avoid wrapping lines in emails or chat apps</li>
              <li>Confirm the destination system accepts long strings</li>
            </ul>
          </li>
        </ol>

        <div class="callout">
          <b>Tip:</b> If the envelope came from a config file, make sure the full value is still present
          (some editors or UI fields silently truncate long values).
        </div>
      </section>

      <section class="card">
        <h2>I lost my passphrase</h2>
        <p>
          <b>There is no recovery.</b> Envelopes cannot be decrypted without the original passphrase.
        </p>

        <div class="callout">
          <b>Recommendation:</b> Treat the passphrase as a critical shared key.
          Store it in your approved secure channel (policy-compliant) and rotate it intentionally.
        </div>
      </section>

      <section class="card">
        <h2>The app keeps prompting me for the secret</h2>
        <ul>
          <li>
            <b>Environment variable not set</b>
            <p class="muted">
              Set <code>KEYFOLIO_SECRET</code> before launching the app, or enter it when prompted.
              The client caches it only for the current session.
            </p>
          </li>
          <li>
            <b>New process session</b>
            <p class="muted">
              If you close and reopen the app, you’ll be prompted again (expected behavior).
            </p>
          </li>
        </ul>
      </section>

      <section class="card">
        <h2>Copy Output didn’t work</h2>
        <ul>
          <li>
            <b>Clipboard permissions / restrictions</b>
            <p class="muted">
              Some environments restrict clipboard access or remote sessions behave differently.
              Try running locally, or paste into a simple text editor first to confirm.
            </p>
          </li>
          <li>
            <b>Empty output</b>
            <p class="muted">
              Ensure <b>Output</b> actually contains text before copying.
            </p>
          </li>
        </ul>
      </section>

      <footer class="footer">
        Last opened: <span id="buildStamp"></span>
      </footer>

    </div>
  </main>

  <script src="assets/nav.js"></script>
  <script src="assets/site.js"></script>
</body>
</html>

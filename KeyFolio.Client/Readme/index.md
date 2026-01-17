<!doctype html>
<html>
<head>
  <meta charset="utf-8" />
  <title>KeyFolio.Client — Docs</title>
  <link rel="stylesheet" href="assets/site.css" />
</head>
<body>
<div class="wrap">
  <aside class="sidebar">
    <div class="brand">
      <div class="title">KeyFolio.Client <span class="badge">Docs</span></div>
      <div class="subtitle">
        Human-facing client for working with encrypted secrets using KeyFolio.Core.
      </div>
    </div>
    <nav id="nav" class="nav"></nav>
  </aside>

  <main class="main">
    <div class="header">
      <h1>KeyFolio.Client</h1>
      <p>User interface implementation of the KeyFolio.Core crypto engine.</p>
    </div>

    <div class="content">
      <div class="card">
        <h2>What this is</h2>
        <p>
          <b>KeyFolio.Client</b> is a human-facing application built on top of
          <b>KeyFolio.Core</b>. It allows users to create, inspect, and validate
          encrypted envelope strings without using the command line.
        </p>
      </div>

      <div class="callout">
        <b>Relationship:</b>
        <ul>
          <li>KeyFolio.Core → crypto engine</li>
          <li>KeyFolio.Console → developer CLI</li>
          <li>KeyFolio.Client → human UI</li>
          <li>MailFolio.Console → operational consumer</li>
        </ul>
      </div>
    </div>

    <div class="footer">
      Last opened: <span id="buildStamp"></span>
    </div>
  </main>
</div>

<script src="assets/nav.js"></script>
<script src="assets/site.js"></script>
</body>
</html>

// Single source of truth for navigation across pages.
window.MAILFOLIO_NAV = [
  { href: "index.html",              title: "Home",             desc: "What MailFolio is and how to use this guide" },
  { href: "getting-started.html",    title: "Getting Started",  desc: "Build + first run commands" },
  { href: "configuration.html",      title: "Configuration",    desc: "Env vars, key file JSON, DB path" },
  { href: "runbook.html",            title: "Runbook",          desc: "Run patterns: dry run, HTML, attachments" },
  { href: "troubleshooting.html",    title: "Troubleshooting",  desc: "Common failures + what to check" }
];

window.renderNav = function renderNav(activeHref) {
  const host = document.getElementById("nav");
  if (!host) return;

  const items = window.MAILFOLIO_NAV || [];
  const html = items.map(i => {
    const isActive = (i.href === activeHref);
    return `
      <a href="${i.href}" class="${isActive ? "active" : ""}">
        ${i.title}
        <small>${i.desc}</small>
      </a>
    `;
  }).join("");

  host.innerHTML = html;
};

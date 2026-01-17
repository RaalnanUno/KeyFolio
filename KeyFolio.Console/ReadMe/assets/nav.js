window.KEYFOLIO_CONSOLE_NAV = [
  { href: "index.html", title: "Home", desc: "What KeyFolio.Console is" },
  { href: "getting-started.html", title: "Getting Started", desc: "Build + first use" },
  { href: "commands.html", title: "Commands", desc: "CLI switches and usage" },
  { href: "examples.html", title: "Examples", desc: "Common workflows" },
  { href: "troubleshooting.html", title: "Troubleshooting", desc: "Failures and fixes" }
];

window.renderNav = function (active) {
  const nav = document.getElementById("nav");
  if (!nav) return;

  nav.innerHTML = KEYFOLIO_CONSOLE_NAV.map(i => `
    <a href="${i.href}" class="${i.href === active ? "active" : ""}">
      ${i.title}
      <small>${i.desc}</small>
    </a>
  `).join("");
};

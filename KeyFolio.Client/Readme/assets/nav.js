window.KEYFOLIO_CLIENT_NAV = [
  { href: "index.html", title: "Home", desc: "What KeyFolio.Client is" },
  { href: "getting-started.html", title: "Getting Started", desc: "Running the client" },
  { href: "concepts.html", title: "Concepts", desc: "User-facing crypto concepts" },
  { href: "workflows.html", title: "Workflows", desc: "Typical user flows" },
  { href: "security-model.html", title: "Security Model", desc: "How secrets are protected" },
  { href: "troubleshooting.html", title: "Troubleshooting", desc: "When things go wrong" }
];

window.renderNav = function (active) {
  const nav = document.getElementById("nav");
  if (!nav) return;

  nav.innerHTML = KEYFOLIO_CLIENT_NAV.map(i => `
    <a href="${i.href}" class="${i.href === active ? "active" : ""}">
      ${i.title}
      <small>${i.desc}</small>
    </a>
  `).join("");
};

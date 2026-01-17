// Readme/assets/nav.js
window.KEYFOLIO_CLIENT_NAV = [
  { route: "home",            title: "Home",            desc: "What KeyFolio.Client is" },
  { route: "getting-started", title: "Getting Started", desc: "Running the client" },
  { route: "concepts",        title: "Concepts",        desc: "User-facing crypto concepts" },
  { route: "workflows",       title: "Workflows",       desc: "Typical user flows" },
  { route: "security-model",  title: "Security Model",  desc: "How secrets are protected" },
  { route: "troubleshooting", title: "Troubleshooting", desc: "When things go wrong" }
];

window.renderNav = function renderNav(activeRoute) {
  const nav = document.getElementById("nav");
  if (!nav) return;

  nav.innerHTML = window.KEYFOLIO_CLIENT_NAV.map(i => {
    const isActive = i.route === activeRoute;
    return `
      <a alt="${i.desc}" title="${i.desc}" href="#/${i.route}" class="${isActive ? "active" : ""}">
        ${i.title}
        
      </a>
    `;
  }).join("");
};

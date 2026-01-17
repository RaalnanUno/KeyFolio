// Readme/assets/site.js
(function () {
  const DEFAULT_ROUTE = "home";
  const CONTENT_DIR = "content"; // relative to index.html

  function setStamp() {
    const stamp = document.getElementById("buildStamp");
    if (stamp) stamp.textContent = new Date().toLocaleString();
  }

  function getRoute() {
    // supports: #/getting-started
    const h = (location.hash || "").trim();
    const m = h.match(/^#\/([^/?#]+)/);
    return (m && m[1]) ? m[1] : DEFAULT_ROUTE;
  }

  async function ensureMermaidLoaded() {
    // Only load Mermaid if the page contains mermaid blocks.
    if (window.__mermaidLoaded) return;

    // Lazy-load Mermaid as ESM from CDN
    // If you ever want to vendor it locally, we can replace this URL.
    const mod = await import("https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs");
    window.mermaid = mod.default;

    window.mermaid.initialize({
      startOnLoad: false,
      theme: "neutral"
    });

    window.__mermaidLoaded = true;
  }

  async function renderMermaidIn(container) {
    const blocks = container.querySelectorAll("pre.mermaid");
    if (!blocks.length) return;

    await ensureMermaidLoaded();

    // Replace each <pre class="mermaid"> with rendered SVG
    for (let i = 0; i < blocks.length; i++) {
      const pre = blocks[i];
      const code = (pre.textContent || "").trim();
      const id = `mmd-${Date.now()}-${i}-${Math.random().toString(16).slice(2)}`;

      try {
        const { svg } = await window.mermaid.render(id, code);

        const wrap = document.createElement("div");
        wrap.className = "diagram";

        // Optional lightbox: if pre has data-lightbox="true"
        const wantsLightbox = pre.getAttribute("data-lightbox") === "true";

        if (wantsLightbox) {
          wrap.innerHTML = `
            <div class="diagram__preview" role="button" tabindex="0" aria-label="Open diagram">
              <div class="diagram__hint">Click to expand</div>
              ${svg}
            </div>
          `;
          const preview = wrap.querySelector(".diagram__preview");
          preview.dataset.svg = svg;
          preview.addEventListener("click", () => openDiagramModal(svg));
          preview.addEventListener("keydown", (e) => {
            if (e.key === "Enter" || e.key === " ") openDiagramModal(svg);
          });
        } else {
          wrap.innerHTML = svg;
        }

        pre.replaceWith(wrap);
      } catch (err) {
        // If Mermaid fails, keep the code visible for debugging
        pre.style.display = "block";
        pre.insertAdjacentHTML("beforebegin", `<div class="callout"><b>Diagram render failed:</b> ${String(err)}</div>`);
      }
    }
  }

  function openImageModal(src, alt) {
  ensureModal();
  const modal = document.getElementById("diagramModal");
  const content = document.getElementById("diagramModalContent");

  content.innerHTML = `
    <img
      src="${src}"
      alt="${alt || ""}"
      style="max-width: 100%; height: auto; display: block; border-radius: 12px;"
    />
  `;

  modal.classList.add("is-open");
  modal.setAttribute("aria-hidden", "false");
  document.body.style.overflow = "hidden";
}


  function ensureModal() {
    if (document.getElementById("diagramModal")) return;

    const modal = document.createElement("div");
    modal.id = "diagramModal";
    modal.className = "modal";
    modal.setAttribute("aria-hidden", "true");
    modal.innerHTML = `
      <div class="modal__backdrop" data-close-modal></div>
      <div class="modal__dialog" role="dialog" aria-modal="true" aria-label="Diagram">
        <div class="modal__header">
          <div class="modal__title">Diagram</div>
          <button class="btn btn-sm" type="button" data-close-modal>Close</button>
        </div>
        <div class="modal__body">
          <div id="diagramModalContent" class="modal__content"></div>
        </div>
      </div>
    `;
    document.body.appendChild(modal);

    modal.addEventListener("click", (e) => {
      const t = e.target;
      if (t && t.hasAttribute && t.hasAttribute("data-close-modal")) closeDiagramModal();
    });

    window.addEventListener("keydown", (e) => {
      if (e.key === "Escape" && modal.classList.contains("is-open")) closeDiagramModal();
    });
  }

  function openDiagramModal(svg) {
    ensureModal();
    const modal = document.getElementById("diagramModal");
    const content = document.getElementById("diagramModalContent");
    content.innerHTML = svg;

    modal.classList.add("is-open");
    modal.setAttribute("aria-hidden", "false");
    document.body.style.overflow = "hidden";
  }

  function closeDiagramModal() {
    const modal = document.getElementById("diagramModal");
    const content = document.getElementById("diagramModalContent");
    if (!modal || !content) return;

    modal.classList.remove("is-open");
    modal.setAttribute("aria-hidden", "true");
    content.innerHTML = "";
    document.body.style.overflow = "";
  }

  async function loadRoute(route) {
    if (window.renderNav) window.renderNav(route);

    const content = document.getElementById("content");
    if (!content) return;

    const url = `${CONTENT_DIR}/${route}.html`;

    try {
      const res = await fetch(url, { cache: "no-cache" });
      if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
      const html = await res.text();

      content.innerHTML = html;

      // After content swaps, render diagrams and run any page hooks
      await renderMermaidIn(content);

      // Wire image lightboxes in the newly loaded content
content.querySelectorAll("[data-lightbox-image]").forEach((btn) => {
  btn.addEventListener("click", () => {
    const src = btn.getAttribute("data-src");
    const alt = btn.getAttribute("data-alt") || "";
    if (src) openImageModal(src, alt);
  });
});


      // Update document title from content if provided
      const t = content.querySelector("[data-title]");
      if (t) document.title = t.getAttribute("data-title") || document.title;
    } catch (err) {
      content.innerHTML = `
        <div class="card">
          <h2>Page not found</h2>
          <p class="muted">Could not load <code>${url}</code>.</p>
          <pre><code>${String(err)}</code></pre>
        </div>
      `;
    }
  }

  async function onRouteChange() {
    setStamp();
    const route = getRoute();
    await loadRoute(route);
  }

  // Boot
  window.addEventListener("hashchange", onRouteChange);
  onRouteChange();
})();

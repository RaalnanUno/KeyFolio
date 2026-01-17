(function () {
  // Determine current page file name
  const path = (location.pathname || "");
  const file = path.split("/").pop() || "index.html";

  // Render nav
  if (typeof window.renderNav === "function") {
    window.renderNav(file);
  }

  // Footer timestamp (optional)
  const stamp = document.getElementById("buildStamp");
  if (stamp) stamp.textContent = new Date().toLocaleString();
})();

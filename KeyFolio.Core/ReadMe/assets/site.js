(function () {
  const path = (location.pathname || "");
  const file = path.split("/").pop() || "index.html";

  if (typeof window.renderNav === "function") {
    window.renderNav(file);
  }

  const stamp = document.getElementById("buildStamp");
  if (stamp) stamp.textContent = new Date().toLocaleString();
})();

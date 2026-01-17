(function () {
  const file = location.pathname.split("/").pop() || "index.html";
  if (window.renderNav) window.renderNav(file);

  const stamp = document.getElementById("buildStamp");
  if (stamp) stamp.textContent = new Date().toLocaleString();
})();

(function () {
    const html = document.documentElement;
    const themeToggle = document.getElementById("themeToggle");
    const themeToggleText = document.getElementById("themeToggleText");
    const mobileMenuToggle = document.getElementById("mobileMenuToggle");
    const categoryNav = document.getElementById("categoryNav");

    const STORAGE_THEME = "novashop.theme";

    function applyTheme(theme) {
        html.setAttribute("data-theme", theme);

        if (themeToggleText) {
            themeToggleText.textContent = theme === "dark" ? "🌙 Oscuro" : "🌞 Claro";
        }

        localStorage.setItem(STORAGE_THEME, theme);
    }

    function loadTheme() {
        const savedTheme = localStorage.getItem(STORAGE_THEME) || "light";
        applyTheme(savedTheme);
    }

    if (themeToggle) {
        themeToggle.addEventListener("click", function () {
            const currentTheme = html.getAttribute("data-theme") || "light";
            const nextTheme = currentTheme === "light" ? "dark" : "light";
            applyTheme(nextTheme);
        });
    }

    if (mobileMenuToggle && categoryNav) {
        mobileMenuToggle.addEventListener("click", function () {
            categoryNav.classList.toggle("open");
        });
    }

    loadTheme();
})();
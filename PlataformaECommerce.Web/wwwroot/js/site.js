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

        if (themeToggle) {
            themeToggle.setAttribute("aria-pressed", theme === "dark" ? "true" : "false");
            themeToggle.setAttribute("aria-label", theme === "dark" ? "Cambiar a tema claro" : "Cambiar a tema oscuro");
        }

        localStorage.setItem(STORAGE_THEME, theme);
    }

    function loadTheme() {
        const savedTheme = localStorage.getItem(STORAGE_THEME);
        const preferredTheme = window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
        const effectiveTheme = savedTheme || preferredTheme;
        applyTheme(effectiveTheme);
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
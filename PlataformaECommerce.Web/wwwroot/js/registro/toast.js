export function showToast({ type = "success", title, message, ms = 2800 }) {
    const toast = document.getElementById("toast");
    const icon = document.getElementById("toastIcon");
    const tTitle = document.getElementById("toastTitle");
    const tMsg = document.getElementById("toastMessage");

    if (!toast || !icon || !tTitle || !tMsg) return;

    // Reset estilos
    icon.classList.remove(
        "bg-emerald-500/15", "text-emerald-300", "border-emerald-500/20",
        "bg-rose-500/15", "text-rose-300", "border-rose-500/20",
        "bg-amber-500/15", "text-amber-300", "border-amber-500/20"
    );

    if (type === "success") {
        icon.textContent = "✓";
        icon.classList.add("bg-emerald-500/15", "text-emerald-300", "border-emerald-500/20");
    } else if (type === "error") {
        icon.textContent = "!";
        icon.classList.add("bg-rose-500/15", "text-rose-300", "border-rose-500/20");
    } else {
        icon.textContent = "i";
        icon.classList.add("bg-amber-500/15", "text-amber-300", "border-amber-500/20");
    }

    tTitle.textContent = title ?? (type === "success" ? "Listo" : "Atención");
    tMsg.textContent = message ?? "";

    toast.classList.remove("hidden");

    // auto-hide
    window.clearTimeout(toast.__timer);
    toast.__timer = window.setTimeout(() => {
        toast.classList.add("hidden");
    }, ms);
}
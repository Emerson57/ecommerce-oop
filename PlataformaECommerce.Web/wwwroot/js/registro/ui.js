function obtenerMsgId(campoId) {
    return `msg-${campoId}`;
}

export function setCampoEstado(campoEl, estado, mensaje = "") {
    // estado: "ok" | "error" | "neutral"
    const msgEl = document.getElementById(obtenerMsgId(campoEl.id));
    if (!msgEl) return;

    // Limpia clases anteriores
    campoEl.classList.remove("border-rose-400/70", "border-emerald-400/70");
    msgEl.classList.remove("text-rose-200", "text-emerald-200", "text-slate-400");

    if (estado === "error") {
        campoEl.classList.add("border-rose-400/70");
        msgEl.classList.add("text-rose-200");
        msgEl.textContent = mensaje;
        return;
    }

    if (estado === "ok") {
        campoEl.classList.add("border-emerald-400/70");
        msgEl.classList.add("text-emerald-200");
        msgEl.textContent = mensaje || "Correcto ✅";
        return;
    }

    // Neutral
    msgEl.classList.add("text-slate-400");
    msgEl.textContent = mensaje;
}

export function mostrarResumenAlertas(errores) {
    const box = document.getElementById("alertSummary");
    const list = document.getElementById("alertList");
    if (!box || !list) return;

    list.innerHTML = "";

    if (!errores || errores.length === 0) {
        box.classList.add("hidden");
        return;
    }

    for (const e of errores) {
        const li = document.createElement("li");
        li.textContent = e;
        list.appendChild(li);
    }

    box.classList.remove("hidden");
}

export function ocultarResumenAlertas() {
    const box = document.getElementById("alertSummary");
    if (box) box.classList.add("hidden");
}

export function setPasswordStrengthUI({ score, label }) {
    // score: 0..4
    const bar = document.getElementById("pwdBar");
    const hint = document.getElementById("pwdHint");
    if (!bar || !hint) return;

    // ancho proporcional (min 10%, max 100%)
    const porcentajes = [10, 25, 50, 75, 100];
    bar.style.width = `${porcentajes[Math.max(0, Math.min(4, score))]}%`;

    // color sin hardcode (pero aquí usaremos clases tailwind ya existentes)
    bar.classList.remove("bg-rose-500", "bg-amber-400", "bg-emerald-400", "bg-slate-500");

    if (score <= 1) bar.classList.add("bg-rose-500");
    else if (score === 2) bar.classList.add("bg-amber-400");
    else bar.classList.add("bg-emerald-400");

    hint.textContent = `Fortaleza: ${label}`;
}
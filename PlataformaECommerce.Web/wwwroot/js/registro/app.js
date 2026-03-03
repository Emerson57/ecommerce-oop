import { obtenerReglas } from "./reglas.js";
import { setCampoEstado, mostrarResumenAlertas, ocultarResumenAlertas, setPasswordStrengthUI } from "./ui.js";
import { evaluarFortalezaContrasena } from "./validadores.js";
import { showToast } from "./toast.js";

const form = document.getElementById("formRegistro");

const campos = {
    nombres: document.getElementById("nombres"),
    apellidos: document.getElementById("apellidos"),
    correo: document.getElementById("correo"),
    usuario: document.getElementById("usuario"),
    contrasena: document.getElementById("contrasena"),
    confirmarContrasena: document.getElementById("confirmarContrasena"),
    fechaNacimiento: document.getElementById("fechaNacimiento"),
    edad: document.getElementById("edad"),
    telefono: document.getElementById("telefono"),
    terminos: document.getElementById("terminos"),
};

const btnLimpiar = document.getElementById("btnLimpiar");
const btnCloseAlert = document.getElementById("btnCloseAlert");

function getState() {
    return {
        nombres: campos.nombres?.value ?? "",
        apellidos: campos.apellidos?.value ?? "",
        correo: campos.correo?.value ?? "",
        usuario: campos.usuario?.value ?? "",
        contrasena: campos.contrasena?.value ?? "",
        confirmarContrasena: campos.confirmarContrasena?.value ?? "",
        fechaNacimiento: campos.fechaNacimiento?.value ?? "",
        edad: campos.edad?.value ?? "",
        telefono: campos.telefono?.value ?? "",
        terminos: campos.terminos?.checked ?? false,
    };
}

async function validarCampo(campoId, modo = "input") {
    const state = getState();
    const reglas = obtenerReglas(state);
    const el = campos[campoId];
    if (!el) return { ok: true };

    let valor;
    if (el.type === "checkbox") valor = el.checked;
    else valor = el.value;

    // password strength UI en tiempo real (solo visual)
    if (campoId === "contrasena" && modo === "input") {
        const strength = evaluarFortalezaContrasena(valor);
        setPasswordStrengthUI(strength);
    }

    // Ejecutar reglas (soporta sync y async)
    for (const regla of reglas[campoId] ?? []) {
        const res = regla(valor);
        const mensaje = (res instanceof Promise) ? await res : res;
        if (mensaje) {
            setCampoEstado(el, "error", mensaje);
            return { ok: false, mensaje };
        }
    }

    // Mensajes “ok” más específicos para algunos campos
    const okMsg =
        campoId === "correo" ? "Correo válido ✅" :
            campoId === "usuario" ? "Usuario válido ✅" :
                campoId === "confirmarContrasena" ? "Coinciden ✅" :
                    campoId === "edad" ? "Edad calculada ✅" :
                    "Correcto ✅";

    setCampoEstado(el, "ok", okMsg);
    return { ok: true };
}

async function validarTodo() {
    const orden = [
        "nombres", "apellidos", "correo", "usuario",
        "contrasena", "confirmarContrasena",
        "fechaNacimiento", "edad", "telefono", "terminos"
    ];

    const errores = [];
    for (const id of orden) {
        const r = await validarCampo(id, "submit");
        if (!r.ok) {
            const etiqueta = etiquetaCampo(id);
            errores.push(`${etiqueta}: ${r.mensaje}`);
        }
    }

    mostrarResumenAlertas(errores);
    return errores.length === 0;
}

function etiquetaCampo(id) {
    const map = {
        nombres: "Nombres",
        apellidos: "Apellidos",
        correo: "Correo",
        usuario: "Usuario",
        contrasena: "Contraseña",
        confirmarContrasena: "Confirmación",
        fechaNacimiento: "Fecha de nacimiento",
        edad: "Edad",
        telefono: "Teléfono",
        terminos: "Términos",
    };
    return map[id] ?? id;
}

function wireEventos() {
    // input: validación en vivo
    ["nombres", "apellidos", "correo", "usuario", "contrasena", "confirmarContrasena", "edad", "telefono"].forEach(id => {
        const el = campos[id];
        if (!el) return;
        el.addEventListener("input", () => validarCampo(id, "input"));
    });

    // blur: refuerza mensajes al salir del campo
    ["nombres", "apellidos", "correo", "usuario", "contrasena", "confirmarContrasena", "fechaNacimiento", "edad", "telefono"].forEach(id => {
        const el = campos[id];
        if (!el) return;
        el.addEventListener("blur", () => validarCampo(id, "blur"));
    });

    // change para date y checkbox
    campos.fechaNacimiento?.addEventListener("change", async () => {
        await validarCampo("fechaNacimiento", "change");
        await validarCampo("edad", "change");
    });
    campos.terminos?.addEventListener("change", () => validarCampo("terminos", "change"));

    // submit: valida todo
    form?.addEventListener("submit", async (e) => {
        e.preventDefault();
        ocultarResumenAlertas();
        setLoading(true);

        const ok = await validarTodo();

        if (!ok) {
            setLoading(false);
            showToast({
                type: "error",
                title: "Revisa el formulario",
                message: "Hay campos con errores. Mira el resumen en la parte superior."
            });
            return;
        }

        // Demo de “envío”
        await new Promise(r => setTimeout(r, 400));

        setLoading(false);
        showToast({
            type: "success",
            title: "Registro validado",
            message: "Todos los campos están correctos. (Demo) Aquí se enviaría al backend."
        });
    });

    btnLimpiar?.addEventListener("click", () => {
        form?.reset();
        ocultarResumenAlertas();

        // Reset UI: neutralizar mensajes
        Object.values(campos).forEach(el => {
            if (!el) return;
            if (el.type === "checkbox") return;
            setCampoEstado(el, "neutral", "");
        });

        // Reset barra contraseña
        setPasswordStrengthUI({ score: 0, label: "Muy débil" });
    });

    btnCloseAlert?.addEventListener("click", () => ocultarResumenAlertas());
}

wireEventos();

function setLoading(isLoading) {
    const btn = document.getElementById("btnRegistrar");
    if (!btn) return;

    btn.disabled = isLoading;
    btn.classList.toggle("opacity-60", isLoading);
    btn.classList.toggle("cursor-not-allowed", isLoading);
    btn.textContent = isLoading ? "Validando..." : "Registrar";
}

console.log("✅ Validador Registro inicializado (ES6 Modules)");
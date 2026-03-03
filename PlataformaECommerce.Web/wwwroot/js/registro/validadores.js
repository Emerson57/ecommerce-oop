import { calcularEdad, esFechaFutura, esVacio, normalizarTexto } from "./utilidades.js";

export function validarRequired(valor) {
    if (esVacio(valor)) return "Este campo es obligatorio.";
    return null;
}

export function validarMinLength(valor, min) {
    const v = normalizarTexto(valor);
    if (v.length < min) return `Debe tener mínimo ${min} caracteres.`;
    return null;
}

export function validarRegex(valor, regex, mensaje) {
    const v = normalizarTexto(valor);
    if (esVacio(v)) return null; // required se encarga
    if (!regex.test(v)) return mensaje;
    return null;
}

export function validarRangoNumero(valor, min, max) {
    if (esVacio(valor)) return "Este campo es obligatorio.";
    const n = Number(valor);
    if (Number.isNaN(n)) return "Debe ser un número válido.";
    if (n < min || n > max) return `Debe estar entre ${min} y ${max}.`;
    return null;
}

export function validarFechaNacimiento(fechaISO, edadMinima = 18) {
    if (esVacio(fechaISO)) {
        setEdadUI("");
        return "La fecha de nacimiento es obligatoria.";
    }

    if (esFechaFutura(fechaISO)) {
        setEdadUI("");
        return "La fecha no puede ser futura.";
    }

    const edad = calcularEdad(fechaISO);
    setEdadUI(edad);

    if (edad < edadMinima) {
        return `Debes ser mayor o igual a ${edadMinima} años.`;
    }

    return null;
}

function setEdadUI(valor) {
    const edadInput = document.getElementById("edad");
    if (edadInput) edadInput.value = valor;
}

export function evaluarFortalezaContrasena(password) {
    const p = normalizarTexto(password);

    // score 0..4 (súper simple pero efectivo para la tarea)
    let score = 0;
    if (p.length >= 8) score++;
    if (/[A-Z]/.test(p)) score++;
    if (/[0-9]/.test(p)) score++;
    if (/[^A-Za-z0-9]/.test(p)) score++;

    const labelMap = {
        0: "Muy débil",
        1: "Débil",
        2: "Media",
        3: "Fuerte",
        4: "Muy fuerte",
    };

    return { score, label: labelMap[score] };
}

export function validarContrasena(password) {
    const p = normalizarTexto(password);
    if (p.length === 0) return "La contraseña es obligatoria.";
    if (p.length < 8) return "Debe tener mínimo 8 caracteres.";
    if (!/[A-Z]/.test(p)) return "Debe incluir al menos una letra mayúscula.";
    if (!/[0-9]/.test(p)) return "Debe incluir al menos un número.";
    if (!/[^A-Za-z0-9]/.test(p)) return "Debe incluir al menos un símbolo.";
    return null;
}

export function validarConfirmacion(password, confirmacion) {
    if (esVacio(confirmacion)) return "La confirmación es obligatoria.";
    if (normalizarTexto(password) !== normalizarTexto(confirmacion)) return "Las contraseñas no coinciden.";
    return null;
}

// Simulación async de “email ya existe”
export async function emailExisteSimulado(email) {
    const e = normalizarTexto(email).toLowerCase();
    if (esVacio(e)) return false;

    // Lista demo (puedes ampliarla)
    const existentes = new Set([
        "test@demo.com",
        "admin@demo.com",
        "eve.holt@reqres.in"
    ]);

    // Simula latencia real (API)
    await new Promise(r => setTimeout(r, 450));

    return existentes.has(e);
}
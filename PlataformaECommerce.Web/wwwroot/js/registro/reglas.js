import {
    validarRequired,
    validarMinLength,
    validarRegex,
    validarRangoNumero,
    validarFechaNacimiento,
    validarContrasena,
    validarConfirmacion,
    emailExisteSimulado,
} from "./validadores.js";

export const regexNombre = /^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$/;
export const regexUsuario = /^[a-zA-Z0-9_.]{4,16}$/; // sin espacios, 4..16, letras/números/_ .
export const regexTelefono = /^[0-9]{10}$/; // Colombia: 10 dígitos (opcional)
export const regexEmail = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export function obtenerReglas(formState) {
    // formState te permite validar campos cruzados (ej confirmar contraseña)
    return {
        nombres: [
            v => validarRequired(v),
            v => validarMinLength(v, 2),
            v => validarRegex(v, regexNombre, "Solo se permiten letras y espacios (incluye tildes y ñ)."),
        ],
        apellidos: [
            v => validarRequired(v),
            v => validarMinLength(v, 2),
            v => validarRegex(v, regexNombre, "Solo se permiten letras y espacios (incluye tildes y ñ)."),
        ],
        correo: [
            v => validarRequired(v),
            v => validarRegex(v, regexEmail, "Formato de correo inválido."),
            async (v) => {
                // si el formato no pasa, no gastamos “async”
                const formato = validarRegex(v, regexEmail, "Formato de correo inválido.");
                if (formato) return formato;

                const existe = await emailExisteSimulado(v);
                if (existe) return "Este correo ya está registrado (simulado).";
                return null;
            }
        ],
        usuario: [
            v => validarRequired(v),
            v => validarRegex(v, regexUsuario, "Usuario inválido. Usa 4–16 caracteres: letras, números, _ o . (sin espacios)."),
        ],
        contrasena: [
            v => validarContrasena(v),
        ],
        confirmarContrasena: [
            v => validarConfirmacion(formState.contrasena, v),
        ],
        fechaNacimiento: [
            v => validarFechaNacimiento(v, 18),
        ],
        edad: [
            v => {
                if (!v) return "La edad no pudo calcularse. Verifica tu fecha de nacimiento.";
                return null;
            }
        ],
        telefono: [
            v => {
                if (!v || v.trim() === "") return null; // opcional
                return validarRegex(v, regexTelefono, "Teléfono inválido. Debe tener 10 dígitos (ej: 3001234567).");
            }
        ],
        terminos: [
            v => (v === true ? null : "Debes aceptar los términos y condiciones."),
        ]
    };
}
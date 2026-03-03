export function normalizarTexto(valor) {
    return (valor ?? "").toString().trim();
}

export function esVacio(valor) {
    return normalizarTexto(valor).length === 0;
}

export function calcularEdad(fechaISO) {
    // fechaISO: "YYYY-MM-DD"
    const hoy = new Date();
    const fecha = new Date(fechaISO);

    let edad = hoy.getFullYear() - fecha.getFullYear();
    const m = hoy.getMonth() - fecha.getMonth();

    if (m < 0 || (m === 0 && hoy.getDate() < fecha.getDate())) {
        edad--;
    }
    return edad;
}

export function esFechaFutura(fechaISO) {
    const hoy = new Date();
    hoy.setHours(0, 0, 0, 0);

    const fecha = new Date(fechaISO);
    fecha.setHours(0, 0, 0, 0);

    return fecha > hoy;
}
document.addEventListener("DOMContentLoaded", function () {
    initFiltros();
    initAccionesTabla();
    initBotonesPrincipales();
});

function initFiltros() {
    const btnAplicar = document.getElementById("btnAplicarFiltros");
    if (btnAplicar) {
        btnAplicar.addEventListener("click", function () {
            const cliente = document.getElementById("filterCliente").value;
            const estado = document.getElementById("filterEstado").value;
            const fecha = document.getElementById("filterFecha").value;
            console.log("Filtrando en HeveLab:", { cliente, estado, fecha });
        });
    }
}

function initAccionesTabla() {
    const tabla = document.getElementById("tbodyRecepciones");
    if (tabla) {
        tabla.addEventListener("click", function (e) {
            const targetButton = e.target.closest(".view-details");
            if (targetButton) {
                const idRecepcion = targetButton.getAttribute("data-id");
                if (idRecepcion) {
                    window.location.href = `/RecepcionesView/Detalle?id=${encodeURIComponent(idRecepcion)}`;
                }
            }
        });
    }
}

// 🛠️ FIX: Evento para el botón + Nuevo
function initBotonesPrincipales() {
    const btnNuevo = document.getElementById("btnNuevo");
    if (btnNuevo) {
        btnNuevo.addEventListener("click", function () {
            console.log("➕ Redirigiendo al formulario de nueva recepción...");
            // Redirige a la vista de creación de una nueva recepción
            window.location.href = "/RecepcionesView/Crear";
        });
    }
}
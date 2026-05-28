const txtBuscarProducto = document.getElementById("txtBuscarProducto");
const productosContador = document.getElementById("productosContador");
const productosCards = document.querySelectorAll(".producto-card");

function filtrarProductos() {
    const textoBusqueda = txtBuscarProducto.value.trim().toLowerCase();

    let productosVisibles = 0;

    productosCards.forEach(card => {
        const sku = card.dataset.sku;
        const nombre = card.dataset.nombre;

        const coincide =
            sku.includes(textoBusqueda) ||
            nombre.includes(textoBusqueda);

        if (coincide) {
            card.style.display = "block";
            productosVisibles++;
        } else {
            card.style.display = "none";
        }
    });

    productosContador.textContent =
        `Mostrando ${productosVisibles} de ${productosCards.length} productos`;
}

txtBuscarProducto.addEventListener("input", filtrarProductos);
document.addEventListener('DOMContentLoaded', function () {
    const inventoryToggle = document.getElementById('hl-inventory-toggle');

    if (!inventoryToggle) return;

    const inventoryGroup = inventoryToggle.closest('.hl-nav-group');

    inventoryToggle.addEventListener('click', function () {
        const isOpen = inventoryGroup.classList.contains('open');

        inventoryGroup.classList.toggle('open');
        inventoryToggle.setAttribute('aria-expanded', String(!isOpen));
    });
});
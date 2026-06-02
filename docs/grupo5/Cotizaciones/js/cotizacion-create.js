(function () {
    'use strict';

    var IGV_RATE = 0.18;
    var itemCounter = 0;

    var root = document.querySelector('.cot-create');
    if (!root) return;

    var mode = root.dataset.cotMode || 'create';
    var readonly = mode === 'details';

    var catalogo = [
        { codigo: 'MAT-0092', descripcion: 'Vigueta de Acero Estructural', sub: 'Calibre 14, 6m galvanizado', stock: 450, unidad: 'UND', precio: 185.50 },
        { codigo: 'MAT-0041', descripcion: 'Cemento Portland Tipo I', sub: 'Saco 42.5 kg', stock: 1200, unidad: 'SAC', precio: 28.90 },
        { codigo: 'SRV-0015', descripcion: 'Instalación Eléctrica', sub: 'Mano de obra por punto', stock: 999, unidad: 'PZA', precio: 350.00 },
        { codigo: 'MAT-0110', descripcion: 'Varilla Corrugada #4', sub: '12m, grado 60', stock: 800, unidad: 'UND', precio: 42.75 },
        { codigo: 'MAT-0078', descripcion: 'Block de Concreto 15x20x40', sub: 'Alta resistencia', stock: 5000, unidad: 'UND', precio: 8.50 },
        { codigo: 'SRV-0022', descripcion: 'Transporte de Materiales', sub: 'Camión 10 ton, zona metropolitana', stock: 50, unidad: 'VJE', precio: 1200.00 }
    ];

    var tbody = document.getElementById('cotItemsBody');
    var form  = document.getElementById('cotForm');
    if (!tbody) return;

    function fmt(n) {
        return (n || 0).toLocaleString('es-MX', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function esc(s) {
        return String(s || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/"/g, '&quot;');
    }

    function nextItemNum() {
        return tbody.querySelectorAll('tr[data-tipo="producto"]').length + 1;
    }

    function reindexItems() {
        var num = 1;
        tbody.querySelectorAll('tr[data-tipo="producto"]').forEach(function (row) {
            var cell = row.querySelector('.col-num');
            if (cell) cell.textContent = num++;
        });
    }

    function setTotals(subtotal, igv, total) {
        var elSub = document.getElementById('cotSubtotal');
        var elIgv = document.getElementById('cotImpuestos');
        var elTot = document.getElementById('cotTotal');
        if (elSub) elSub.textContent = fmt(subtotal);
        if (elIgv) elIgv.textContent = fmt(igv);
        if (elTot) elTot.textContent = fmt(total);
        var hidSub = document.getElementById('hidSubtotal');
        var hidIgv = document.getElementById('hidImpuestos');
        var hidTot = document.getElementById('hidTotal');
        if (hidSub) hidSub.value = subtotal.toFixed(2);
        if (hidIgv) hidIgv.value = igv.toFixed(2);
        if (hidTot) hidTot.value = total.toFixed(2);
    }

    function calcRow(row) {
        if (row.dataset.tipo !== 'producto') return;

        if (readonly) return;

        var cantidad  = parseFloat(row.querySelector('[data-field="cantidad"]')?.value) || 0;
        var descuento = parseFloat(row.querySelector('[data-field="descuento"]')?.value) || 0;
        var precio    = parseFloat(row.querySelector('[data-field="precio"]')?.value) || 0;

        var base = cantidad * precio - descuento;
        if (base < 0) base = 0;

        var igv = base * IGV_RATE;
        var importe = base + igv;

        row.querySelector('[data-field="precioIgv"]').textContent = fmt(igv);
        row.querySelector('[data-field="importe"]').textContent = fmt(importe);

        row.dataset.base = base;
        row.dataset.igv = igv;
        row.dataset.importe = importe;
    }

    function calcTotals() {
        var subtotal = 0, igvTotal = 0, total = 0;

        tbody.querySelectorAll('tr[data-tipo="producto"]').forEach(function (row) {
            if (readonly) {
                subtotal += parseFloat(row.dataset.base) || 0;
                igvTotal += parseFloat(row.dataset.igv) || 0;
                total    += parseFloat(row.dataset.importe) || 0;
            } else {
                calcRow(row);
                subtotal += parseFloat(row.dataset.base) || 0;
                igvTotal += parseFloat(row.dataset.igv) || 0;
                total    += parseFloat(row.dataset.importe) || 0;
            }
        });

        if (subtotal === 0 && igvTotal === 0 && total === 0) {
            var hidSub = document.getElementById('hidSubtotal');
            if (hidSub && parseFloat(hidSub.value) > 0) {
                setTotals(parseFloat(hidSub.value) || 0, parseFloat(document.getElementById('hidImpuestos')?.value) || 0, parseFloat(document.getElementById('hidTotal')?.value) || 0);
                return;
            }
        }

        setTotals(subtotal, igvTotal, total);
    }

    function bindRowEvents(row) {
        if (readonly) return;

        row.querySelectorAll('[data-field="cantidad"], [data-field="descuento"], [data-field="precio"]').forEach(function (input) {
            input.addEventListener('input', calcTotals);
        });

        var btnRemove = row.querySelector('.cot-create__row-remove');
        if (btnRemove) {
            btnRemove.addEventListener('click', function () {
                row.remove();
                reindexItems();
                calcTotals();
            });
        }
    }

    function createProductRow(data) {
        data = data || {};

        if (readonly) {
            var trR = document.createElement('tr');
            trR.dataset.tipo = 'producto';
            var cantidad = parseFloat(data.cantidad) || 0;
            var descuento = parseFloat(data.descuento) || 0;
            var precio = parseFloat(data.precio !== undefined ? data.precio : data.precioUnitario) || 0;
            var base = cantidad * precio - descuento;
            if (base < 0) base = 0;
            var igv = data.precioIgv !== undefined ? parseFloat(data.precioIgv) : base * IGV_RATE;
            var importe = data.importe !== undefined ? parseFloat(data.importe) : base + igv;
            trR.dataset.base = base;
            trR.dataset.igv = igv;
            trR.dataset.importe = importe;

            var subHtml = data.sub || data.subDescripcion || '';
            trR.innerHTML =
                '<td class="col-num">' + nextItemNum() + '</td>' +
                '<td class="col-code"><span class="cot-create__code">' + esc(data.codigo) + '</span></td>' +
                '<td class="col-desc"><span class="cot-create__desc-title">' + esc(data.descripcion) + '</span>' +
                    (subHtml ? '<span class="cot-create__desc-sub">' + esc(subHtml) + '</span>' : '') + '</td>' +
                '<td class="col-sm">' + esc(data.stock) + '</td>' +
                '<td class="col-sm">' + fmt(cantidad) + '</td>' +
                '<td class="col-sm">' + fmt(descuento) + '</td>' +
                '<td class="col-sm">' + esc(data.unidad) + '</td>' +
                '<td class="col-price">' + fmt(precio) + '</td>' +
                '<td class="col-price">' + fmt(igv) + '</td>' +
                '<td class="col-price cot-create__importe">' + fmt(importe) + '</td>';
            tbody.appendChild(trR);
            return trR;
        }

        itemCounter++;
        var idx = itemCounter;
        var tr = document.createElement('tr');
        tr.dataset.tipo = 'producto';
        tr.dataset.idx = idx;

        tr.innerHTML =
            '<td class="col-num">' + nextItemNum() + '</td>' +
            '<td class="col-code"><input type="text" class="cot-create__cell-input cot-create__code" name="Items[' + idx + '].Codigo" data-field="codigo" value="' + esc(data.codigo || '') + '" placeholder="CÓDIGO" /></td>' +
            '<td class="col-desc">' +
                '<input type="text" class="cot-create__cell-input cot-create__desc-title" name="Items[' + idx + '].Descripcion" data-field="descripcion" value="' + esc(data.descripcion || '') + '" placeholder="Descripción" />' +
                '<input type="text" class="cot-create__cell-input cot-create__desc-sub" name="Items[' + idx + '].SubDescripcion" data-field="sub" value="' + esc(data.sub || data.subDescripcion || '') + '" placeholder="Detalle adicional" />' +
                '<input type="hidden" name="Items[' + idx + '].Tipo" value="producto" />' +
            '</td>' +
            '<td class="col-sm"><input type="number" class="cot-create__cell-input cot-create__cell-input--center" name="Items[' + idx + '].Stock" data-field="stock" value="' + (data.stock !== undefined ? data.stock : '') + '" min="0" placeholder="0" /></td>' +
            '<td class="col-sm"><input type="number" class="cot-create__cell-input cot-create__cell-input--center" name="Items[' + idx + '].Cantidad" data-field="cantidad" value="' + (data.cantidad !== undefined ? data.cantidad : '') + '" min="0" step="0.01" placeholder="0" /></td>' +
            '<td class="col-sm"><input type="number" class="cot-create__cell-input cot-create__cell-input--center" name="Items[' + idx + '].Descuento" data-field="descuento" value="' + (data.descuento !== undefined ? data.descuento : '') + '" min="0" step="0.01" placeholder="0" /></td>' +
            '<td class="col-sm"><input type="text" class="cot-create__cell-input cot-create__cell-input--center" name="Items[' + idx + '].Unidad" data-field="unidad" value="' + esc(data.unidad || '') + '" placeholder="UND" /></td>' +
            '<td class="col-price"><input type="number" class="cot-create__cell-input cot-create__cell-input--num" name="Items[' + idx + '].PrecioUnitario" data-field="precio" value="' + (data.precio !== undefined ? data.precio : (data.precioUnitario !== undefined ? data.precioUnitario : '')) + '" min="0" step="0.01" placeholder="0.00" /></td>' +
            '<td class="col-price"><span data-field="precioIgv">' + fmt(data.precioIgv || 0) + '</span><input type="hidden" name="Items[' + idx + '].PrecioIgv" value="' + (data.precioIgv || 0) + '" /></td>' +
            '<td class="col-price cot-create__importe"><span data-field="importe">' + fmt(data.importe || 0) + '</span><input type="hidden" name="Items[' + idx + '].Importe" value="' + (data.importe || 0) + '" /></td>' +
            '<td class="col-actions" style="width:32px;border:none;"><button type="button" class="cot-create__row-remove" title="Eliminar">&times;</button></td>';

        tbody.appendChild(tr);
        bindRowEvents(tr);
        calcRow(tr);
        calcTotals();
        return tr;
    }

    function createSectionRow(data) {
        data = data || {};

        if (readonly) {
            var trR = document.createElement('tr');
            trR.className = 'cot-create__row--section';
            trR.dataset.tipo = 'seccion';
            trR.innerHTML = '<td colspan="10">' + esc(data.descripcion) + '</td>';
            tbody.appendChild(trR);
            return;
        }

        itemCounter++;
        var idx = itemCounter;
        var tr = document.createElement('tr');
        tr.className = 'cot-create__row--section';
        tr.dataset.tipo = 'seccion';
        tr.innerHTML =
            '<td colspan="10">' +
                '<input type="text" class="cot-create__cell-input" name="Items[' + idx + '].Descripcion" value="' + esc(data.descripcion || '') + '" placeholder="Nombre de la sección..." style="font-weight:700;color:var(--cot-accent);text-transform:uppercase;" />' +
                '<input type="hidden" name="Items[' + idx + '].Tipo" value="seccion" />' +
            '</td>' +
            '<td class="col-actions" style="border:none;"><button type="button" class="cot-create__row-remove" title="Eliminar">&times;</button></td>';
        tbody.appendChild(tr);
        bindRowEvents(tr);
    }

    function createNoteRow(data) {
        data = data || {};

        if (readonly) {
            var trR = document.createElement('tr');
            trR.className = 'cot-create__row--note';
            trR.dataset.tipo = 'nota';
            trR.innerHTML = '<td colspan="10">' + esc(data.descripcion) + '</td>';
            tbody.appendChild(trR);
            return;
        }

        itemCounter++;
        var idx = itemCounter;
        var tr = document.createElement('tr');
        tr.className = 'cot-create__row--note';
        tr.dataset.tipo = 'nota';
        tr.innerHTML =
            '<td colspan="10">' +
                '<input type="text" class="cot-create__cell-input" name="Items[' + idx + '].Descripcion" value="' + esc(data.descripcion || '') + '" placeholder="Escriba una nota..." style="font-style:italic;color:var(--cot-label);" />' +
                '<input type="hidden" name="Items[' + idx + '].Tipo" value="nota" />' +
            '</td>' +
            '<td class="col-actions" style="border:none;"><button type="button" class="cot-create__row-remove" title="Eliminar">&times;</button></td>';
        tbody.appendChild(tr);
        bindRowEvents(tr);
    }

    function loadInitialItems() {
        var el = document.getElementById('cotInitialItems');
        if (!el || !el.textContent.trim()) {
            calcTotals();
            return;
        }

        try {
            var items = JSON.parse(el.textContent);
            if (!items || !items.length) {
                calcTotals();
                return;
            }

            items.forEach(function (item) {
                var tipo = (item.tipo || item.Tipo || 'producto').toLowerCase();
                var mapped = {
                    codigo: item.codigo || item.Codigo || '',
                    descripcion: item.descripcion || item.Descripcion || '',
                    sub: item.subDescripcion || item.SubDescripcion || '',
                    stock: item.stock !== undefined ? item.stock : item.Stock,
                    cantidad: item.cantidad !== undefined ? item.cantidad : item.Cantidad,
                    descuento: item.descuento !== undefined ? item.descuento : item.Descuento,
                    unidad: item.unidad || item.Unidad || '',
                    precio: item.precioUnitario !== undefined ? item.precioUnitario : item.PrecioUnitario,
                    precioIgv: item.precioIgv !== undefined ? item.precioIgv : item.PrecioIgv,
                    importe: item.importe !== undefined ? item.importe : item.Importe
                };

                if (tipo === 'seccion') createSectionRow(mapped);
                else if (tipo === 'nota') createNoteRow(mapped);
                else createProductRow(mapped);
            });

            calcTotals();
        } catch (e) {
            calcTotals();
        }
    }

    function renderCatalog() {
        var body = document.getElementById('cotCatalogBody');
        if (!body) return;
        body.innerHTML = '';
        catalogo.forEach(function (p) {
            var tr = document.createElement('tr');
            tr.innerHTML =
                '<td><span class="cot-create__code">' + p.codigo + '</span></td>' +
                '<td><strong>' + p.descripcion + '</strong><br><small style="color:#94a3b8">' + p.sub + '</small></td>' +
                '<td style="text-align:center">' + p.stock + '</td>' +
                '<td style="text-align:right">$ ' + fmt(p.precio) + '</td>' +
                '<td><button type="button" class="cot-create__catalog-add" data-codigo="' + p.codigo + '">Agregar</button></td>';
            body.appendChild(tr);
        });

        body.querySelectorAll('.cot-create__catalog-add').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var cod = btn.dataset.codigo;
                var p = catalogo.find(function (x) { return x.codigo === cod; });
                if (p) {
                    createProductRow({ codigo: p.codigo, descripcion: p.descripcion, sub: p.sub, stock: p.stock, unidad: p.unidad, precio: p.precio, cantidad: 1 });
                    closeCatalog();
                }
            });
        });
    }

    function openCatalog() {
        renderCatalog();
        document.getElementById('cotCatalogModal').classList.add('is-open');
    }

    function closeCatalog() {
        document.getElementById('cotCatalogModal').classList.remove('is-open');
    }

    if (!readonly) {
        var btnAddProduct = document.getElementById('btnAddProduct');
        var btnAddSection = document.getElementById('btnAddSection');
        var btnAddNote    = document.getElementById('btnAddNote');
        var btnCatalog    = document.getElementById('btnCatalog');
        var btnCloseCatalog = document.getElementById('btnCloseCatalog');
        var modal = document.getElementById('cotCatalogModal');

        if (btnAddProduct) btnAddProduct.addEventListener('click', function () { createProductRow(); });
        if (btnAddSection) btnAddSection.addEventListener('click', function () { createSectionRow(); });
        if (btnAddNote)    btnAddNote.addEventListener('click', function () { createNoteRow(); });
        if (btnCatalog)    btnCatalog.addEventListener('click', openCatalog);
        if (btnCloseCatalog) btnCloseCatalog.addEventListener('click', closeCatalog);
        if (modal) modal.addEventListener('click', function (e) { if (e.target === modal) closeCatalog(); });

        if (form && form.tagName === 'FORM') {
            form.addEventListener('submit', function (e) {
                calcTotals();

                var clienteEl = form.querySelector('[name="Cliente"]');
                if (clienteEl && !clienteEl.value.trim()) {
                    e.preventDefault();
                    clienteEl.focus();
                    alert('Ingrese el nombre del cliente.');
                    return;
                }

                tbody.querySelectorAll('tr[data-tipo="producto"]').forEach(function (row) {
                    var hidIgv = row.querySelector('[name$=".PrecioIgv"]');
                    var hidImp = row.querySelector('[name$=".Importe"]');
                    if (hidIgv) hidIgv.value = (parseFloat(row.dataset.igv) || 0).toFixed(2);
                    if (hidImp) hidImp.value = (parseFloat(row.dataset.importe) || 0).toFixed(2);
                });
            });
        }
    }

    loadInitialItems();
})();

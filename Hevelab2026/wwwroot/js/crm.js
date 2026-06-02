document.addEventListener('DOMContentLoaded', () => {

    // ─── 1. LÓGICA DE PESTAÑAS ───────────────────────────────────────────────
    const tabs = document.querySelectorAll('.frm-tab');
    const contents = document.querySelectorAll('.tab-content');

    tabs.forEach(tab => {
        tab.addEventListener('click', () => {
            tabs.forEach(t => t.classList.remove('active'));
            contents.forEach(c => c.style.display = 'none');
            tab.classList.add('active');
            const target = document.querySelector(tab.dataset.target);
            if (target) target.style.display = 'block';
        });
    });

    // ─── 2. INICIALIZACIÓN ───────────────────────────────────────────────────
    inicializar();

    // ─── 3. FORMULARIO NUEVO LEAD ────────────────────────────────────────────
    const frmNuevoLead = document.getElementById('frmNuevoLead');
    if (frmNuevoLead) {
        frmNuevoLead.addEventListener('submit', async (e) => {
            e.preventDefault();

            const nuevoLead = {
                nombreNegocio:    document.getElementById('leadNegocio').value.trim(),
                contactoNombre:   document.getElementById('leadContacto').value.trim(),
                contactoCorreo:   document.getElementById('leadCorreo').value.trim(),
                contactoTelefono: document.getElementById('leadTelefono').value.trim(),
                origen:           document.getElementById('leadOrigen').value,
                prioridad:        parseInt(document.getElementById('leadPrioridad').value),
                vendedorId:       parseInt(document.getElementById('leadVendedor').value),
                vendedorNombre:   document.getElementById('leadVendedor').options[document.getElementById('leadVendedor').selectedIndex].text,
                etapaId:          parseInt(document.getElementById('leadEtapa').value),
                activo:           true
            };

            try {
                const response = await fetch('/api/Crm/leads', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(nuevoLead)
                });

                if (response.ok) {
                    frmNuevoLead.reset();
                    // Cerrar modal Bootstrap
                    const modalEl = document.getElementById('modalNuevoLead');
                    const modal = bootstrap.Modal.getInstance(modalEl);
                    if (modal) modal.hide();

                    // Recargar todo el tablero
                    await cargarLeads();
                    alert('Lead registrado exitosamente.');
                } else {
                    const errorText = await response.text();
                    alert(`Error: ${errorText}`);
                }
            } catch (err) {
                console.error('Error al crear lead:', err);
                alert('Ocurrió un error al guardar el lead.');
            }
        });
    }

    // ─── FORMULARIO NUEVA ETAPA ──────────────────────────────────────────────
    const frmNuevaEtapa = document.getElementById('frmNuevaEtapa');
    if (frmNuevaEtapa) {
        frmNuevaEtapa.addEventListener('submit', async (e) => {
            e.preventDefault();
            const nuevaEtapa = {
                nombre: document.getElementById('etapaNombre').value.trim(),
                secuencia: parseInt(document.getElementById('etapaSecuencia').value),
                probabilidad: parseInt(document.getElementById('etapaProbabilidad').value),
                esGanado: document.getElementById('etapaGanado').value === 'true',
                esPerdido: document.getElementById('etapaPerdido').value === 'true'
            };

            try {
                const response = await fetch('/api/Crm/etapas', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(nuevaEtapa)
                });

                if (response.ok) {
                    frmNuevaEtapa.reset();
                    const modal = bootstrap.Modal.getInstance(document.getElementById('modalNuevaEtapa'));
                    if (modal) modal.hide();
                    await cargarEtapas();
                    alert('Etapa guardada exitosamente.');
                } else {
                    alert(`Error: ${await response.text()}`);
                }
            } catch (err) {
                console.error('Error al guardar etapa:', err);
            }
        });
    }

    // ─── FORMULARIO NUEVA ETIQUETA ───────────────────────────────────────────
    const frmNuevaEtiqueta = document.getElementById('frmNuevaEtiqueta');
    if (frmNuevaEtiqueta) {
        frmNuevaEtiqueta.addEventListener('submit', async (e) => {
            e.preventDefault();
            const nuevaEtiqueta = {
                nombre: document.getElementById('etiquetaNombre').value.trim(),
                color: document.getElementById('etiquetaColor').value
            };

            try {
                const response = await fetch('/api/Crm/etiquetas', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(nuevaEtiqueta)
                });

                if (response.ok) {
                    frmNuevaEtiqueta.reset();
                    const modal = bootstrap.Modal.getInstance(document.getElementById('modalNuevaEtiqueta'));
                    if (modal) modal.hide();
                    await cargarEtiquetas();
                    alert('Etiqueta guardada exitosamente.');
                } else {
                    alert(`Error: ${await response.text()}`);
                }
            } catch (err) {
                console.error('Error al guardar etiqueta:', err);
            }
        });
    }

    // ─── FORMULARIO NUEVA ACTIVIDAD ──────────────────────────────────────────
    const frmNuevaActividad = document.getElementById('frmNuevaActividad');
    if (frmNuevaActividad) {
        frmNuevaActividad.addEventListener('submit', async (e) => {
            e.preventDefault();
            const nuevaActividad = {
                leadId: parseInt(document.getElementById('actividadLead').value),
                usuarioId: 1, // Usuario actual placeholder
                usuarioNombre: "Administrador (Tú)", // Placeholder
                tipo: document.getElementById('actividadTipo').value,
                titulo: document.getElementById('actividadTitulo').value.trim(),
                descripcion: document.getElementById('actividadDescripcion').value.trim(),
                fechaProgramada: document.getElementById('actividadFecha').value
            };

            try {
                const response = await fetch('/api/Crm/actividades', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(nuevaActividad)
                });

                if (response.ok) {
                    frmNuevaActividad.reset();
                    const modal = bootstrap.Modal.getInstance(document.getElementById('modalNuevaActividad'));
                    if (modal) modal.hide();
                    alert('Actividad guardada exitosamente.');
                } else {
                    alert(`Error: ${await response.text()}`);
                }
            } catch (err) {
                console.error('Error al guardar actividad:', err);
            }
        });
    }

});

// ─── INICIALIZACIÓN PRINCIPAL ─────────────────────────────────────────────────
async function inicializar() {
    await cargarEtapas();   // Arma columnas Kanban + llena select del modal
    await cargarEtiquetas();
    await cargarLeads();    // Llena tarjetas Kanban + tabla Lista
}

// ─── CARGAR ETAPAS Y CONSTRUIR KANBAN ────────────────────────────────────────
let etapasGlobales = [];

async function cargarEtapas() {
    try {
        const res = await fetch('/api/Crm/etapas');
        if (!res.ok) return;
        etapasGlobales = await res.json();

        // 1. Construir columnas del Kanban
        const board = document.getElementById('kanbanBoard');
        if (board) {
            board.innerHTML = '';
            etapasGlobales.forEach(etapa => {
                const borderColor = etapa.esGanado
                    ? '#10b981'
                    : etapa.esPerdido
                        ? '#ef4444'
                        : 'var(--sys-primary, #4338ca)';

                board.innerHTML += `
                    <div class="crm-kanban-col" id="col-etapa-${etapa.id}">
                        <div class="crm-kanban-col-header" style="border-top: 3px solid ${borderColor};">
                            <span class="col-title">${etapa.nombre}</span>
                            <span class="col-count" id="count-etapa-${etapa.id}">0</span>
                        </div>
                        <div class="crm-kanban-cards" id="cards-etapa-${etapa.id}">
                            <!-- Tarjetas de leads -->
                        </div>
                    </div>`;
            });
        }

        // 2. Llenar select del modal con las etapas
        const selectEtapa = document.getElementById('leadEtapa');
        if (selectEtapa) {
            selectEtapa.innerHTML = '<option value="">Seleccione una etapa...</option>';
            etapasGlobales
                .filter(e => !e.esGanado && !e.esPerdido)
                .forEach(e => {
                    selectEtapa.innerHTML += `<option value="${e.id}">${e.nombre}</option>`;
                });
        }

        // 3. Llenar tabla de etapas
        const tbEtapas = document.getElementById('tbEtapas');
        if (tbEtapas) {
            tbEtapas.innerHTML = '';
            etapasGlobales.forEach(e => {
                const estadoStr = e.esGanado ? "Ganado" : e.esPerdido ? "Perdido" : "En Proceso";
                tbEtapas.innerHTML += `
                    <tr>
                        <td>${e.id}</td>
                        <td><strong>${e.nombre}</strong></td>
                        <td>${e.secuencia}</td>
                        <td>${e.probabilidad}%</td>
                        <td>${estadoStr}</td>
                    </tr>
                `;
            });
        }

    } catch (err) {
        console.error('Error al cargar etapas:', err);
    }
}

// ─── CARGAR ETIQUETAS ────────────────────────────────────────────────────────
async function cargarEtiquetas() {
    try {
        const res = await fetch('/api/Crm/etiquetas');
        if (!res.ok) return;
        const etiquetas = await res.json();

        const tbEtiquetas = document.getElementById('tbEtiquetas');
        if (tbEtiquetas) {
            tbEtiquetas.innerHTML = '';
            etiquetas.forEach(e => {
                tbEtiquetas.innerHTML += `
                    <tr>
                        <td>${e.id}</td>
                        <td><strong>${e.nombre}</strong></td>
                        <td><span style="display:inline-block;width:15px;height:15px;background-color:${e.color};border-radius:50%;vertical-align:middle;margin-right:5px;"></span>${e.color}</td>
                    </tr>
                `;
            });
        }
    } catch (err) {
        console.error('Error al cargar etiquetas:', err);
    }
}

// ─── CARGAR LEADS Y LLENAR KANBAN + LISTA ────────────────────────────────────
async function cargarLeads() {
    try {
        const res = await fetch('/api/Crm/leads');
        if (!res.ok) return;
        const leads = await res.json();

        // Resetear contadores y tarjetas de cada columna
        etapasGlobales.forEach(etapa => {
            const cards = document.getElementById(`cards-etapa-${etapa.id}`);
            const count = document.getElementById(`count-etapa-${etapa.id}`);
            if (cards) cards.innerHTML = '';
            if (count) count.textContent = '0';
        });

        // KPIs
        document.getElementById('kpiLeads').textContent = leads.length;
        const totalIngresos = leads.reduce((sum, l) => sum + (l.ingresoEsperado || 0), 0);
        document.getElementById('kpiIngresos').textContent =
            '$' + totalIngresos.toLocaleString('es-PE', { minimumFractionDigits: 2 });

        // Llenar Kanban y tabla lista
        const tbLista = document.getElementById('tbLeadsList');
        if (tbLista) tbLista.innerHTML = '';

        const selectActividadLead = document.getElementById('actividadLead');
        if (selectActividadLead) {
            selectActividadLead.innerHTML = '<option value="">Seleccione un lead...</option>';
        }

        if (leads.length === 0) {
            if (tbLista) tbLista.innerHTML = `<tr><td colspan="6" class="text-center py-4">No hay leads registrados.</td></tr>`;
            return;
        }

        // Mapa de contadores por etapa
        const conteoPorEtapa = {};

        leads.forEach(lead => {
            // ── KANBAN CARD ──────────────────────────────────
            const prioridadMap = {
                0: { cls: 'low',    label: 'Baja'  },
                1: { cls: 'medium', label: 'Media' },
                2: { cls: 'high',   label: 'Alta'  }
            };
            const prio = prioridadMap[lead.prioridad] || prioridadMap[0];
            const ingreso = lead.ingresoEsperado
                ? '$' + Number(lead.ingresoEsperado).toLocaleString('es-PE', { minimumFractionDigits: 2 })
                : '—';

            const cardsContainer = document.getElementById(`cards-etapa-${lead.etapaId}`);
            if (cardsContainer) {
                const card = document.createElement('div');
                card.className = 'crm-card';
                card.dataset.leadId = lead.id;
                card.innerHTML = `
                    <div class="crm-card-title">${lead.nombreNegocio}</div>
                    <div class="crm-card-contact">${lead.contactoNombre || 'Sin contacto'}</div>
                    <div class="crm-card-footer">
                        <span class="crm-priority ${prio.cls}">${prio.label}</span>
                        <span class="crm-amount">${ingreso}</span>
                    </div>`;
                cardsContainer.appendChild(card);

                // Actualizar contador de la columna
                conteoPorEtapa[lead.etapaId] = (conteoPorEtapa[lead.etapaId] || 0) + 1;
                const countEl = document.getElementById(`count-etapa-${lead.etapaId}`);
                if (countEl) countEl.textContent = conteoPorEtapa[lead.etapaId];
            }

            // ── FILA EN TABLA LISTA ──────────────────────────
            if (tbLista) {
                const badge = lead.esOportunidad
                    ? `<span class="frm-badge frm-badge-accepted">${lead.etapaNombre}</span>`
                    : `<span class="frm-badge frm-badge-draft">${lead.etapaNombre}</span>`;

                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td><strong>${lead.nombreNegocio}</strong></td>
                    <td>${lead.contactoNombre || '—'}<br>
                        <small style="color: var(--text-secondary)">${lead.contactoCorreo || ''}</small>
                    </td>
                    <td>${badge}</td>
                    <td>${lead.vendedorNombre || '—'}</td>
                    <td style="font-weight:700; color:#10b981;">${ingreso}</td>
                    <td>
                        <button class="action-btn" title="Ver Detalles" onclick="verLead(${lead.id})">
                            <svg viewBox="0 0 24 24"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle></svg>
                        </button>
                    </td>`;
                tbLista.appendChild(tr);
            }

            // Llenar select de leads para el modal de actividades
            if (selectActividadLead) {
                selectActividadLead.innerHTML += `<option value="${lead.id}">${lead.nombreNegocio}</option>`;
            }
        });

    } catch (err) {
        console.error('Error al cargar leads:', err);
    }
}

// ─── VER DETALLE DE UN LEAD (placeholder) ────────────────────────────────────
function verLead(id) {
    alert(`Detalle del Lead #${id}\nPróximamente: modal de detalle con actividades.`);
}

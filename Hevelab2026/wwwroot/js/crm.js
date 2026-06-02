document.addEventListener('DOMContentLoaded', () => {
    // Tabs
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

    // Inicialización
    cargarEtapas();
    cargarLeads();
    cargarActividadesPendientes();

    // Nuevo Lead
    const frmNuevoLead = document.getElementById('frmNuevoLead');
    if (frmNuevoLead) {
        frmNuevoLead.addEventListener('submit', async (e) => {
            e.preventDefault();

            const dto = {
                nombreNegocio: document.getElementById('leadNegocio')?.value.trim(),
                contactoNombre: document.getElementById('leadContacto')?.value.trim() || '',
                contactoCorreo: document.getElementById('leadCorreo')?.value.trim() || '',
                contactoTelefono: document.getElementById('leadTelefono')?.value.trim() || '',
                origen: document.getElementById('leadOrigen')?.value || 'Directo',
                prioridad: parseInt(document.getElementById('leadPrioridad')?.value ?? '0', 10),
                vendedorId: parseInt(document.getElementById('leadVendedor')?.value ?? '0', 10),
                etapaId: parseInt(document.getElementById('leadEtapa')?.value ?? '0', 10),
                // según CrmController.CrearLead
                etiquetaIds: []
            };

            try {
                const res = await fetch('/api/Crm/leads', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(dto)
                });

                if (!res.ok) {
                    const err = await res.text();
                    alert(`Error al guardar lead: ${err}`);
                    return;
                }

                frmNuevoLead.reset();
                await cargarEtapas();
                await cargarLeads();
                await cargarActividadesPendientes();

                // Cerrar modal
                const modalEl = document.getElementById('modalNuevoLead');
                if (modalEl && window.bootstrap?.Modal) {
                    const modal = window.bootstrap.Modal.getInstance(modalEl);
                    if (modal) modal.hide();
                }
            } catch (err) {
                console.error('Error POST /api/Crm/leads', err);
                alert('Ocurrió un error al guardar el lead.');
            }
        });
    }
});

async function cargarEtapas() {
    try {
        const res = await fetch('/api/Crm/etapas');
        if (!res.ok) return;
        const etapas = await res.json();

        const selectEtapa = document.getElementById('leadEtapa');
        if (selectEtapa) {
            selectEtapa.innerHTML = '';
            selectEtapa.innerHTML = '<option value="">Seleccione...</option>';
            etapas.forEach(etapa => {
                const opt = document.createElement('option');
                opt.value = etapa.id;
                opt.textContent = etapa.nombre;
                selectEtapa.appendChild(opt);
            });
        }

        // Si ya tenemos leads, reconstruimos el kanban
        renderKanban(etapas, null);
    } catch (err) {
        console.error('Error cargarEtapas', err);
    }
}

async function cargarLeads() {
    try {
        const [resEtapas, resLeads] = await Promise.all([
            fetch('/api/Crm/etapas'),
            fetch('/api/Crm/leads')
        ]);

        if (!resEtapas.ok || !resLeads.ok) return;

        const etapas = await resEtapas.json();
        const leads = await resLeads.json();

        // KPI
        const kpi = document.getElementById('kpiLeads');
        if (kpi) kpi.innerText = leads.length;

        renderKanban(etapas, leads);
        renderLeadsList(leads);

    } catch (err) {
        console.error('Error cargarLeads', err);
    }
}

function renderKanban(etapas, leads) {
    const board = document.getElementById('kanbanBoard');
    if (!board) return;

    board.innerHTML = '';

    const leadMapByEtapa = new Map();
    (leads || []).forEach(l => {
        const arr = leadMapByEtapa.get(l.etapaId) || [];
        arr.push(l);
        leadMapByEtapa.set(l.etapaId, arr);
    });

    etapas.forEach(etapa => {
        const col = document.createElement('div');
        col.className = 'crm-kanban-column';

        const header = document.createElement('div');
        header.className = 'crm-kanban-column-header';
        header.innerHTML = `
            <div class="crm-kanban-title">${etapa.nombre}</div>
            <div class="crm-kanban-sub">Prob: ${etapa.probabilidad}%</div>
        `;

        const body = document.createElement('div');
        body.className = 'crm-kanban-column-body';

        const leadsInCol = leadMapByEtapa.get(etapa.id) || [];
        if (leadsInCol.length === 0) {
            body.innerHTML = `<div class="crm-kanban-empty">Sin leads</div>`;
        } else {
            leadsInCol.forEach(lead => {
                const card = document.createElement('div');
                card.className = 'crm-lead-card';
                card.dataset.leadId = lead.id;
                card.innerHTML = `
                    <div class="crm-lead-card-top">
                        <div class="crm-lead-business">${lead.nombreNegocio}</div>
                        <div class="crm-lead-priority">Prioridad: ${lead.prioridad}</div>
                    </div>
                    <div class="crm-lead-card-mid">
                        <div class="crm-lead-contact">${lead.contactoNombre || ''}</div>
                        <div class="crm-lead-ingreso">Ingreso est.: ${lead.ingresoEsperado ?? 0}</div>
                    </div>
                    <div class="crm-lead-card-actions">
                        <button type="button" class="frm-btn-small" data-action="ver">Ver</button>
                    </div>
                `;

                body.appendChild(card);
            });
        }

        col.appendChild(header);
        col.appendChild(body);
        board.appendChild(col);
    });
}

function renderLeadsList(leads) {
    const tbody = document.getElementById('tbLeadsList');
    if (!tbody) return;

    tbody.innerHTML = '';

    if (!leads || leads.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center py-4">No hay leads.</td></tr>';
        return;
    }

    leads.forEach(lead => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${lead.nombreNegocio}</td>
            <td>${lead.contactoNombre || ''}</td>
            <td>${lead.etapaNombre || ''}</td>
            <td>${lead.vendedorNombre || ''}</td>
            <td>${lead.ingresoEsperado ?? 0}</td>
            <td>
                <button class="frm-btn-small" type="button" data-action="convertir" data-lead-id="${lead.id}">Convertir</button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

async function cargarActividadesPendientes() {
    // Como en el controlador no existe un endpoint "actividades pendientes" global,
    // la KPI se deja en 0 si no existe estructura adicional.
    // Si en el futuro agregas un endpoint, conectamos aquí.
    const kpi = document.getElementById('kpiActividades');
    if (kpi) kpi.innerText = '0';

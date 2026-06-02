document.addEventListener('DOMContentLoaded', () => {
    // 1. Lógica de Pestañas
    const tabs = document.querySelectorAll('.frm-tab');
    const contents = document.querySelectorAll('.tab-content');

    tabs.forEach(tab => {
        tab.addEventListener('click', () => {
            tabs.forEach(t => t.classList.remove('active'));
            contents.forEach(c => c.style.display = 'none');
            
            tab.classList.add('active');
            const target = document.querySelector(tab.dataset.target);
            if (target) {
                target.style.display = 'block';
            }
        });
    });

    // 2. Inicialización de datos
    cargarDepartamentos();
    cargarCargos();
    cargarEmpleados();

    // 3. Manejo del formulario de Nuevo Empleado
    const frmNuevoEmpleado = document.getElementById('frmNuevoEmpleado');
    if (frmNuevoEmpleado) {
        // Establecer la fecha actual por defecto en el input de fecha de ingreso
        document.getElementById('empFechaIngreso').valueAsDate = new Date();

        frmNuevoEmpleado.addEventListener('submit', async (e) => {
            e.preventDefault();

            // Construimos el DTO (HrEmployee) a enviar
            const nuevoEmpleado = {
                nombres: document.getElementById('empNombres').value.trim(),
                apellidos: document.getElementById('empApellidos').value.trim(),
                tipoDocumento: document.getElementById('empTipoDoc').value,
                numeroDocumento: document.getElementById('empNumDoc').value.trim(),
                departamentoId: parseInt(document.getElementById('empDepto').value),
                cargoId: parseInt(document.getElementById('empCargo').value),
                fechaIngreso: document.getElementById('empFechaIngreso').value,
                tipoContrato: "FIJO", // Por defecto
                regimenLaboral: "General" // Por defecto
            };

            try {
                const response = await fetch('/api/Rrhh/empleados', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(nuevoEmpleado)
                });

                if (response.ok) {
                    frmNuevoEmpleado.reset();
                    document.getElementById('empFechaIngreso').valueAsDate = new Date();
                    
                    const modalEl = document.getElementById('modalNuevoEmpleado');
                    const modal = bootstrap.Modal.getInstance(modalEl);
                    if (modal) modal.hide();

                    cargarEmpleados();
                    alert('Empleado registrado exitosamente.');
                } else {
                    const errorText = await response.text();
                    alert(`Error al registrar: ${errorText}`);
                }
            } catch (err) {
                console.error("Error en la petición POST:", err);
                alert("Ocurrió un error al intentar guardar el empleado.");
            }
        });
    }

    // 4. Manejo del formulario de Nuevo Departamento
    const frmNuevoDepartamento = document.getElementById('frmNuevoDepartamento');
    if (frmNuevoDepartamento) {
        frmNuevoDepartamento.addEventListener('submit', async (e) => {
            e.preventDefault();

            const nuevoDepartamento = {
                nombre: document.getElementById('deptNombre').value.trim(),
                responsableNombre: document.getElementById('deptResponsable').value.trim()
            };

            try {
                const response = await fetch('/api/Rrhh/departamentos', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(nuevoDepartamento)
                });

                if (response.ok) {
                    frmNuevoDepartamento.reset();

                    const modalEl = document.getElementById('modalNuevoDepartamento');
                    const modal = bootstrap.Modal.getInstance(modalEl);
                    if (modal) modal.hide();

                    cargarDepartamentos();
                    alert('Departamento creado exitosamente.');
                } else {
                    const errorText = await response.text();
                    alert(`Error al crear departamento: ${errorText}`);
                }
            } catch (err) {
                console.error("Error al crear departamento:", err);
                alert("Ocurrió un error al intentar guardar el departamento.");
            }
        });
    }

    // 5. Manejo del formulario de Nuevo Cargo
    const frmNuevoCargo = document.getElementById('frmNuevoCargo');
    if (frmNuevoCargo) {
        frmNuevoCargo.addEventListener('submit', async (e) => {
            e.preventDefault();

            const nuevoCargo = {
                nombre: document.getElementById('cargoNombre').value.trim()
            };

            try {
                const response = await fetch('/api/Rrhh/cargos', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(nuevoCargo)
                });

                if (response.ok) {
                    frmNuevoCargo.reset();

                    const modalEl = document.getElementById('modalNuevoCargo');
                    const modal = bootstrap.Modal.getInstance(modalEl);
                    if (modal) modal.hide();

                    cargarCargos();
                    alert('Cargo creado exitosamente.');
                } else {
                    const errorText = await response.text();
                    alert(`Error al crear cargo: ${errorText}`);
                }
            } catch (err) {
                console.error("Error al crear cargo:", err);
                alert("Ocurrió un error al intentar guardar el cargo.");
            }
        });
    }
});

// --- Funciones de Fetch a la API ---

async function cargarEmpleados() {
    try {
        const res = await fetch('/api/Rrhh/empleados');
        if (!res.ok) return;
        const empleados = await res.json();
        
        // Actualizar KPI
        document.getElementById('kpiEmpleados').innerText = empleados.length;

        // Renderizar tabla
        const tbody = document.getElementById('tbEmpleados');
        tbody.innerHTML = '';

        if (empleados.length === 0) {
            tbody.innerHTML = `<tr><td colspan="6" class="text-center py-4">No hay empleados registrados.</td></tr>`;
            return;
        }

        empleados.forEach(emp => {
            const tr = document.createElement('tr');
            
            const iniciales = (emp.nombres.charAt(0) + emp.apellidos.charAt(0)).toUpperCase();
            const fechaIngreso = new Date(emp.fechaIngreso).toLocaleDateString('es-ES');
            
            const badge = emp.activo 
                ? `<span class="frm-badge frm-badge-accepted">Activo</span>`
                : `<span class="frm-badge frm-badge-inactivo">Inactivo</span>`;

            tr.innerHTML = `
                <td>
                    <div class="empleado-info">
                        <div class="empleado-avatar">${iniciales}</div>
                        <div class="empleado-details">
                            <span class="empleado-name">${emp.nombreCompleto}</span>
                            <span class="empleado-date">Ingreso: ${fechaIngreso}</span>
                        </div>
                    </div>
                </td>
                <td>${emp.departamentoNombre}</td>
                <td>${emp.cargoNombre}</td>
                <td>${emp.tipoDocumento} - ${emp.numeroDocumento}</td>
                <td>${badge}</td>
                <td>
                    <button class="action-btn" title="Ver Detalles">
                        <svg viewBox="0 0 24 24"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle></svg>
                    </button>
                </td>
            `;
            tbody.appendChild(tr);
        });

    } catch (err) {
        console.error("Error al cargar empleados", err);
    }
}

async function cargarDepartamentos() {
    try {
        const res = await fetch('/api/Rrhh/departamentos');
        if (!res.ok) return;
        const deptos = await res.json();

        // 1. Llenar tabla de departamentos
        const tbody = document.getElementById('tbDepartamentos');
        tbody.innerHTML = '';
        if (deptos.length === 0) {
            tbody.innerHTML = `<tr><td colspan="4" class="text-center py-4">No hay departamentos.</td></tr>`;
        } else {
            deptos.forEach(d => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td>${d.id}</td>
                    <td>${d.nombre}</td>
                    <td>${d.responsableNombre || 'Sin asignar'}</td>
                    <td>
                        <button class="action-btn" title="Editar">
                            <svg viewBox="0 0 24 24"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path></svg>
                        </button>
                    </td>
                `;
                tbody.appendChild(tr);
            });
        }

        // 2. Llenar select del formulario de empleados
        const selectDepto = document.getElementById('empDepto');
        if (selectDepto) {
            selectDepto.innerHTML = '<option value="">Seleccione...</option>';
            deptos.forEach(d => {
                selectDepto.innerHTML += `<option value="${d.id}">${d.nombre}</option>`;
            });
        }

    } catch (err) {
        console.error("Error al cargar departamentos", err);
    }
}

async function cargarCargos() {
    try {
        const res = await fetch('/api/Rrhh/cargos');
        if (!res.ok) return;
        const cargos = await res.json();

        // 1. Llenar tabla de cargos
        const tbody = document.getElementById('tbCargos');
        if (tbody) {
            tbody.innerHTML = '';
            if (cargos.length === 0) {
                tbody.innerHTML = `<tr><td colspan="3" class="text-center py-4">No hay cargos registrados.</td></tr>`;
            } else {
                cargos.forEach(c => {
                    const tr = document.createElement('tr');
                    tr.innerHTML = `
                        <td>${c.id}</td>
                        <td>${c.nombre}</td>
                        <td>
                            <button class="action-btn" title="Editar">
                                <svg viewBox="0 0 24 24"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path></svg>
                            </button>
                        </td>
                    `;
                    tbody.appendChild(tr);
                });
            }
        }

        // 2. Llenar select del formulario de empleados
        const selectCargo = document.getElementById('empCargo');
        if (selectCargo) {
            selectCargo.innerHTML = '<option value="">Seleccione...</option>';
            cargos.forEach(c => {
                selectCargo.innerHTML += `<option value="${c.id}">${c.nombre}</option>`;
            });
        }
    } catch (err) {
        console.error("Error al cargar cargos", err);
    }
}

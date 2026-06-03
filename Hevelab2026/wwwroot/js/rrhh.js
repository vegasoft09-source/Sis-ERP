document.addEventListener('DOMContentLoaded', () => {
    // === 1. INICIALIZACIÓN Y NAVEGACIÓN DE PESTAÑAS ===
    const tabs = document.querySelectorAll('.frm-tab');
    const contents = document.querySelectorAll('.tab-content');

    tabs.forEach(tab => {
        tab.addEventListener('click', () => {
            tabs.forEach(t => t.classList.remove('active'));
            contents.forEach(c => {
                c.style.display = 'none';
                c.classList.remove('active');
            });
            
            tab.classList.add('active');
            const target = document.querySelector(tab.dataset.target);
            if (target) {
                target.style.display = 'block';
                target.classList.add('active');
            }
            
            // Recargar datos específicos según la pestaña seleccionada
            const targetId = tab.dataset.target;
            if (targetId === '#tab-empleados') {
                cargarEmpleados();
            } else if (targetId === '#tab-departamentos') {
                cargarDepartamentos();
            } else if (targetId === '#tab-cargos') {
                cargarCargos();
            } else if (targetId === '#tab-contratos') {
                cargarContratosTab();
            } else if (targetId === '#tab-asistencia') {
                cargarAsistenciaTab();
            } else if (targetId === '#tab-ausencias') {
                cargarAusenciasTab();
            }
        });
    });

    // Iniciar pestaña inicial
    cargarEmpleados();
    iniciarReloj();

    // === 2. EVENTOS DE FORMULARIOS Y MODALES ===

    // Formulario: Nuevo Empleado
    const frmNuevoEmpleado = document.getElementById('frmNuevoEmpleado');
    if (frmNuevoEmpleado) {
        document.getElementById('empFechaIngreso').valueAsDate = new Date();
        frmNuevoEmpleado.addEventListener('submit', async (e) => {
            e.preventDefault();
            const usuarioVal = document.getElementById('empUsuario').value;
            const nuevoEmpleado = {
                nombres: document.getElementById('empNombres').value.trim(),
                apellidos: document.getElementById('empApellidos').value.trim(),
                tipoDocumento: document.getElementById('empTipoDoc').value,
                numeroDocumento: document.getElementById('empNumDoc').value.trim(),
                departamentoId: parseInt(document.getElementById('empDepto').value),
                cargoId: parseInt(document.getElementById('empCargo').value),
                usuarioId: usuarioVal ? parseInt(usuarioVal) : null,
                fechaIngreso: document.getElementById('empFechaIngreso').value,
                tipoContrato: document.getElementById('empTipoContrato').value,
                regimenLaboral: document.getElementById('empRegimenLaboral').value.trim()
            };

            try {
                const response = await fetch('/api/rrhh/empleados', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(nuevoEmpleado)
                });

                if (response.ok) {
                    frmNuevoEmpleado.reset();
                    document.getElementById('empFechaIngreso').valueAsDate = new Date();
                    cerrarModal('modalNuevoEmpleado');
                    cargarEmpleados();
                    mostrarAlerta('Empleado registrado exitosamente.', 'success');
                } else {
                    const errorText = await response.text();
                    mostrarAlerta(`Error al registrar empleado: ${errorText}`, 'danger');
                }
            } catch (err) {
                console.error("Error en POST Empleado:", err);
                mostrarAlerta("Error de conexión al guardar el empleado.", 'danger');
            }
        });

        // Al abrir modal de empleado, cargar selectores auxiliares
        const modalEmpEl = document.getElementById('modalNuevoEmpleado');
        if (modalEmpEl) {
            modalEmpEl.addEventListener('show.bs.modal', () => {
                poblarDropdown('/api/rrhh/select/departamentos', 'empDepto', 'id', 'nombre', 'Seleccione departamento...');
                poblarDropdown('/api/rrhh/select/cargos', 'empCargo', 'id', 'nombre', 'Seleccione cargo...');
                poblarDropdown('/api/rrhh/select/usuarios', 'empUsuario', 'id', 'nombreCompleto', 'Ninguno');
            });
        }
    }

    // Formulario: Nuevo Departamento
    const frmNuevoDepartamento = document.getElementById('frmNuevoDepartamento');
    if (frmNuevoDepartamento) {
        frmNuevoDepartamento.addEventListener('submit', async (e) => {
            e.preventDefault();
            const respVal = document.getElementById('deptResponsable').value;
            const respInt = respVal ? parseInt(respVal, 10) : null;
            const nuevoDepto = {
                nombre: document.getElementById('deptNombre').value.trim(),
                responsableId: (respInt && !isNaN(respInt)) ? respInt : null
            };

            try {
                const response = await fetch('/api/rrhh/departamentos', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(nuevoDepto)
                });

                if (response.ok) {
                    frmNuevoDepartamento.reset();
                    cerrarModal('modalNuevoDepartamento');
                    cargarDepartamentos();
                    mostrarAlerta('Departamento creado exitosamente.', 'success');
                } else {
                    const errorText = await response.text();
                    mostrarAlerta(`Error al crear departamento: ${errorText}`, 'danger');
                }
            } catch (err) {
                console.error("Error en POST Departamento:", err);
                mostrarAlerta("Error de conexión al guardar departamento.", 'danger');
            }
        });

        const modalDeptoEl = document.getElementById('modalNuevoDepartamento');
        if (modalDeptoEl) {
            modalDeptoEl.addEventListener('show.bs.modal', () => {
                poblarDropdown('/api/rrhh/select/usuarios-todos', 'deptResponsable', 'id', 'nombreCompleto', 'Sin responsable');
            });
        }
    }

    // Formulario: Nuevo Cargo
    const frmNuevoCargo = document.getElementById('frmNuevoCargo');
    if (frmNuevoCargo) {
        frmNuevoCargo.addEventListener('submit', async (e) => {
            e.preventDefault();
            const nuevoCargo = {
                nombre: document.getElementById('cargoNombre').value.trim(),
                descripcion: document.getElementById('cargoDescripcion').value.trim()
            };

            try {
                const response = await fetch('/api/rrhh/cargos', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(nuevoCargo)
                });

                if (response.ok) {
                    frmNuevoCargo.reset();
                    cerrarModal('modalNuevoCargo');
                    cargarCargos();
                    mostrarAlerta('Cargo creado exitosamente.', 'success');
                } else {
                    const errorText = await response.text();
                    mostrarAlerta(`Error al crear cargo: ${errorText}`, 'danger');
                }
            } catch (err) {
                console.error("Error en POST Cargo:", err);
                mostrarAlerta("Error de conexión al guardar cargo.", 'danger');
            }
        });
    }

    // Formulario: Nuevo Contrato
    const frmNuevoContrato = document.getElementById('frmNuevoContrato');
    if (frmNuevoContrato) {
        document.getElementById('contratoFechaInicio').valueAsDate = new Date();
        frmNuevoContrato.addEventListener('submit', async (e) => {
            e.preventDefault();
            const fechaFinVal = document.getElementById('contratoFechaFin').value;
            const nuevoContrato = {
                empleadoId: parseInt(document.getElementById('contratoEmpleado').value),
                nombre: document.getElementById('contratoNombre').value.trim(),
                fechaInicio: document.getElementById('contratoFechaInicio').value,
                fechaFin: fechaFinVal ? fechaFinVal : null,
                sueldo: parseFloat(document.getElementById('contratoSueldo').value),
                monedaId: parseInt(document.getElementById('contratoMoneda').value),
                tipoContrato: document.getElementById('contratoTipo').value
            };

            try {
                const response = await fetch('/api/rrhh/contratos', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(nuevoContrato)
                });

                if (response.ok) {
                    frmNuevoContrato.reset();
                    document.getElementById('contratoFechaInicio').valueAsDate = new Date();
                    cerrarModal('modalNuevoContrato');
                    
                    // Si el empleado registrado coincide con el del filtro, recargar la tabla
                    const empFiltro = document.getElementById('selectEmpleadoContratos').value;
                    if (empFiltro && parseInt(empFiltro) === nuevoContrato.empleadoId) {
                        cargarContratosPorEmpleado(nuevoContrato.empleadoId);
                    }
                    actualizarKPIs();
                    mostrarAlerta('Contrato registrado exitosamente. Contratos activos anteriores cerrados.', 'success');
                } else {
                    const errorText = await response.text();
                    mostrarAlerta(`Error al registrar contrato: ${errorText}`, 'danger');
                }
            } catch (err) {
                console.error("Error en POST Contrato:", err);
                mostrarAlerta("Error de conexión al guardar contrato.", 'danger');
            }
        });

        const modalContratoEl = document.getElementById('modalNuevoContrato');
        if (modalContratoEl) {
            modalContratoEl.addEventListener('show.bs.modal', () => {
                poblarDropdown('/api/rrhh/select/empleados', 'contratoEmpleado', 'id', 'nombreCompleto', 'Seleccione empleado...');
                poblarDropdown('/api/rrhh/select/monedas', 'contratoMoneda', 'id', 'nombre', 'Seleccione moneda...');
                
                // Pre-seleccionar el empleado del filtro si existe
                const empFiltro = document.getElementById('selectEmpleadoContratos').value;
                if (empFiltro) {
                    setTimeout(() => {
                        const selectElement = document.getElementById('contratoEmpleado');
                        if (selectElement) selectElement.value = empFiltro;
                    }, 500); // Pequeña espera para asegurar que el dropdown cargó
                }
            });
        }
    }

    // Formulario: Solicitar Ausencia
    const frmSolicitarAusencia = document.getElementById('frmSolicitarAusencia');
    if (frmSolicitarAusencia) {
        document.getElementById('ausenciaFechaInicio').valueAsDate = new Date();
        document.getElementById('ausenciaFechaFin').valueAsDate = new Date();
        
        frmSolicitarAusencia.addEventListener('submit', async (e) => {
            e.preventDefault();
            const nuevaAusencia = {
                empleadoId: parseInt(document.getElementById('ausenciaEmpleado').value),
                tipoAusenciaId: parseInt(document.getElementById('ausenciaTipo').value),
                aprobadorId: parseInt(document.getElementById('ausenciaAprobador').value),
                fechaInicio: document.getElementById('ausenciaFechaInicio').value,
                fechaFin: document.getElementById('ausenciaFechaFin').value,
                motivo: document.getElementById('ausenciaMotivo').value.trim()
            };

            try {
                const response = await fetch('/api/rrhh/ausencias', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(nuevaAusencia)
                });

                if (response.ok) {
                    frmSolicitarAusencia.reset();
                    document.getElementById('ausenciaFechaInicio').valueAsDate = new Date();
                    document.getElementById('ausenciaFechaFin').valueAsDate = new Date();
                    cerrarModal('modalSolicitarAusencia');
                    
                    const empFiltro = document.getElementById('selectEmpleadoAusencias').value;
                    if (empFiltro && parseInt(empFiltro) === nuevaAusencia.empleadoId) {
                        cargarAusenciasPorEmpleado(nuevaAusencia.empleadoId);
                    }
                    mostrarAlerta('Solicitud de ausencia enviada exitosamente.', 'success');
                } else {
                    const errorText = await response.text();
                    mostrarAlerta(`Error al solicitar ausencia: ${errorText}`, 'danger');
                }
            } catch (err) {
                console.error("Error en POST Ausencia:", err);
                mostrarAlerta("Error de conexión al enviar solicitud de ausencia.", 'danger');
            }
        });

        const modalAusenciaEl = document.getElementById('modalSolicitarAusencia');
        if (modalAusenciaEl) {
            modalAusenciaEl.addEventListener('show.bs.modal', () => {
                poblarDropdown('/api/rrhh/select/empleados', 'ausenciaEmpleado', 'id', 'nombreCompleto', 'Seleccione empleado...');
                poblarDropdown('/api/rrhh/tipos-ausencia', 'ausenciaTipo', 'id', 'nombre', 'Seleccione tipo...');
                poblarDropdown('/api/rrhh/select/usuarios-todos', 'ausenciaAprobador', 'id', 'nombreCompleto', 'Seleccione aprobador...');
                
                // Pre-seleccionar el empleado del filtro si existe
                const empFiltro = document.getElementById('selectEmpleadoAusencias').value;
                if (empFiltro) {
                    setTimeout(() => {
                        const selectElement = document.getElementById('ausenciaEmpleado');
                        if (selectElement) selectElement.value = empFiltro;
                    }, 500);
                }
            });
        }
    }

    // Formulario: Crear Tipo de Ausencia
    const frmNuevoTipoAusencia = document.getElementById('frmNuevoTipoAusencia');
    if (frmNuevoTipoAusencia) {
        frmNuevoTipoAusencia.addEventListener('submit', async (e) => {
            e.preventDefault();
            const nuevoTipo = {
                nombre: document.getElementById('tipoAusNombre').value.trim(),
                requiereAprobacion: document.getElementById('tipoAusAprobacion').checked,
                diasMaximos: parseInt(document.getElementById('tipoAusDiasMax').value)
            };

            try {
                const response = await fetch('/api/rrhh/tipos-ausencia', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(nuevoTipo)
                });

                if (response.ok) {
                    frmNuevoTipoAusencia.reset();
                    cerrarModal('modalNuevoTipoAusencia');
                    mostrarAlerta('Tipo de ausencia configurado correctamente.', 'success');
                } else {
                    const errorText = await response.text();
                    mostrarAlerta(`Error al configurar tipo: ${errorText}`, 'danger');
                }
            } catch (err) {
                console.error("Error en POST Tipo Ausencia:", err);
                mostrarAlerta("Error de conexión al guardar tipo de ausencia.", 'danger');
            }
        });
    }

    // Formulario: Resolver Ausencia (Confirmación de Aprobación/Rechazo)
    const frmResolverAusencia = document.getElementById('frmResolverAusencia');
    if (frmResolverAusencia) {
        frmResolverAusencia.addEventListener('submit', async (e) => {
            e.preventDefault();
            const id = document.getElementById('resolverAusenciaId').value;
            const body = {
                aprobadorId: parseInt(document.getElementById('resolverAprobador').value),
                decision: document.getElementById('resolverDecision').value
            };

            try {
                const response = await fetch(`/api/rrhh/ausencias/${id}/resolver`, {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(body)
                });

                if (response.ok) {
                    frmResolverAusencia.reset();
                    cerrarModal('modalResolverAusencia');
                    
                    const empFiltro = document.getElementById('selectEmpleadoAusencias').value;
                    if (empFiltro) {
                        cargarAusenciasPorEmpleado(parseInt(empFiltro));
                    }
                    mostrarAlerta(`Solicitud de ausencia resuelta con éxito.`, 'success');
                } else {
                    const errorText = await response.text();
                    mostrarAlerta(`Error al resolver ausencia: ${errorText}`, 'danger');
                }
            } catch (err) {
                console.error("Error en PUT Resolver Ausencia:", err);
                mostrarAlerta("Error de conexión al procesar la solicitud.", 'danger');
            }
        });
    }

    // === 3. LOGICA DE FILTROS EN PESTAÑAS (CHANGE EVENTS) ===

    // Selector: Empleado para ver Contratos
    const selectEmpContratos = document.getElementById('selectEmpleadoContratos');
    if (selectEmpContratos) {
        selectEmpContratos.addEventListener('change', () => {
            const empleadoId = selectEmpContratos.value;
            if (empleadoId) {
                cargarContratosPorEmpleado(parseInt(empleadoId));
            } else {
                document.getElementById('tbContratos').innerHTML = `<tr><td colspan="8" class="text-center py-4 text-muted">Seleccione un empleado para ver sus contratos.</td></tr>`;
            }
        });
    }

    // Selector: Empleado para ver Asistencias y habilitar Marcador
    const selectEmpAsistencias = document.getElementById('selectEmpleadoAsistencias');
    if (selectEmpAsistencias) {
        selectEmpAsistencias.addEventListener('change', () => {
            const empleadoId = selectEmpAsistencias.value;
            const pnlMarcacion = document.getElementById('pnlMarcacion');
            const placeholder = document.getElementById('pnlMarcacionPlaceholder');
            
            if (empleadoId) {
                pnlMarcacion.style.display = 'flex';
                placeholder.style.display = 'none';
                cargarAsistenciasPorEmpleado(parseInt(empleadoId));
            } else {
                pnlMarcacion.style.display = 'none';
                placeholder.style.display = 'block';
            }
        });
    }

    // Selector: Empleado para ver Ausencias
    const selectEmpAusencias = document.getElementById('selectEmpleadoAusencias');
    if (selectEmpAusencias) {
        selectEmpAusencias.addEventListener('change', () => {
            const empleadoId = selectEmpAusencias.value;
            if (empleadoId) {
                cargarAusenciasPorEmpleado(parseInt(empleadoId));
            } else {
                document.getElementById('tbAusencias').innerHTML = `<tr><td colspan="9" class="text-center py-4 text-muted">Seleccione un empleado para ver sus ausencias.</td></tr>`;
            }
        });
    }

    // Botones de Marcación Rápida
    const btnEntrada = document.getElementById('btnMarcarEntrada');
    if (btnEntrada) {
        btnEntrada.addEventListener('click', () => registrarAsistenciaRapida('entrada'));
    }
    const btnSalida = document.getElementById('btnMarcarSalida');
    if (btnSalida) {
        btnSalida.addEventListener('click', () => registrarAsistenciaRapida('salida'));
    }
});

// === 4. FUNCIONES DE CARGA Y FETCH DEL API ===

// Cargar Directorio de Empleados
async function cargarEmpleados() {
    try {
        const res = await fetch('/api/rrhh/empleados?soloActivos=false');
        if (!res.ok) {
            const err = await res.ok ? '' : await res.text();
            console.error('Error al cargar empleados:', err);
            return;
        }
        const empleados = await res.json();
        
        // Renderizar tabla
        const tbody = document.getElementById('tbEmpleados');
        if (tbody) {
            tbody.innerHTML = '';
            if (empleados.length === 0) {
                tbody.innerHTML = `<tr><td colspan="6" class="text-center py-4">No hay empleados registrados.</td></tr>`;
                return;
            }

            empleados.forEach(emp => {
                const tr = document.createElement('tr');
                const iniciales = (emp.nombres.charAt(0) + emp.apellidos.charAt(0)).toUpperCase();
                const fechaIngreso = formatFecha(emp.fechaIngreso);
                const badge = emp.activo 
                    ? `<span class="frm-badge frm-badge-activo">Activo</span>`
                    : `<span class="frm-badge frm-badge-inactivo">Inactivo</span>`;

                let btnBaja = '';
                if (emp.activo) {
                    btnBaja = `
                        <button class="action-btn text-danger mx-1" onclick="desactivarEmpleado(${emp.id})" title="Dar de Baja">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 6h18M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                        </button>
                    `;
                }

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
                    <td>${emp.departamentoNombre || 'Sin asignar'}</td>
                    <td>${emp.cargoNombre || 'Sin asignar'}</td>
                    <td>${emp.tipoDocumento} - ${emp.numeroDocumento}</td>
                    <td>${badge}</td>
                    <td>
                        <button class="action-btn text-primary mx-1" onclick="verDetallesEmpleado(${emp.id})" title="Ver Detalles">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle></svg>
                        </button>
                        ${btnBaja}
                    </td>
                `;
                tbody.appendChild(tr);
            });
        }
        
        actualizarKPIs(empleados);

    } catch (err) {
        console.error("Error al cargar empleados:", err);
    }
}

// Cargar Departamentos
async function cargarDepartamentos() {
    try {
        const res = await fetch('/api/rrhh/departamentos');
        if (!res.ok) return;
        const deptos = await res.json();

        const tbody = document.getElementById('tbDepartamentos');
        if (tbody) {
            tbody.innerHTML = '';
            if (deptos.length === 0) {
                tbody.innerHTML = `<tr><td colspan="4" class="text-center py-4">No hay departamentos configurados.</td></tr>`;
                return;
            }
            deptos.forEach(d => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td>${d.id}</td>
                    <td class="fw-bold">${d.nombre}</td>
                    <td>${d.responsableNombre || '<span class="text-muted small">Sin asignar</span>'}</td>
                    <td>
                        <span class="text-muted small">Mantenimiento vía backend</span>
                    </td>
                `;
                tbody.appendChild(tr);
            });
        }
    } catch (err) {
        console.error("Error al cargar departamentos:", err);
    }
}

// Cargar Cargos
async function cargarCargos() {
    try {
        const res = await fetch('/api/rrhh/cargos');
        if (!res.ok) return;
        const cargos = await res.json();

        const tbody = document.getElementById('tbCargos');
        if (tbody) {
            tbody.innerHTML = '';
            if (cargos.length === 0) {
                tbody.innerHTML = `<tr><td colspan="4" class="text-center py-4">No hay cargos configurados.</td></tr>`;
                return;
            }
            cargos.forEach(c => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td>${c.id}</td>
                    <td class="fw-bold">${c.nombre}</td>
                    <td>${c.descripcion || '<span class="text-muted small">Sin descripción</span>'}</td>
                    <td>
                        <span class="text-muted small">Mantenimiento vía backend</span>
                    </td>
                `;
                tbody.appendChild(tr);
            });
        }
    } catch (err) {
        console.error("Error al cargar cargos:", err);
    }
}

// Pestaña Contratos: Poblar selectores de empleados
function cargarContratosTab() {
    poblarDropdown('/api/rrhh/select/empleados', 'selectEmpleadoContratos', 'id', 'nombreCompleto', 'Seleccione un empleado...');
}

// Cargar Contratos de un Empleado
async function cargarContratosPorEmpleado(empleadoId) {
    try {
        const res = await fetch(`/api/rrhh/empleados/${empleadoId}/contratos`);
        const tbody = document.getElementById('tbContratos');
        if (!tbody) return;

        tbody.innerHTML = '';
        if (!res.ok) {
            tbody.innerHTML = `<tr><td colspan="8" class="text-center py-4 text-danger">Error al obtener contratos.</td></tr>`;
            return;
        }

        const contratos = await res.json();
        if (contratos.length === 0) {
            tbody.innerHTML = `<tr><td colspan="8" class="text-center py-4 text-muted">No hay contratos registrados para este empleado.</td></tr>`;
            return;
        }

        contratos.forEach(c => {
            const tr = document.createElement('tr');
            const fInicio = formatFecha(c.fechaInicio);
            const fFin = c.fechaFin ? formatFecha(c.fechaFin) : '<span class="text-muted">Indefinido</span>';
            const badgeClass = c.estado === 'ACTIVO' ? 'frm-badge-activo' : 'frm-badge-cerrado';
            
            tr.innerHTML = `
                <td>${c.id}</td>
                <td class="fw-bold">${c.nombre}</td>
                <td>${fInicio}</td>
                <td>${fFin}</td>
                <td class="font-monospace">${c.sueldo.toFixed(2)}</td>
                <td>${c.moneda}</td>
                <td>${c.tipoContrato}</td>
                <td><span class="frm-badge ${badgeClass}">${c.estado}</span></td>
            `;
            tbody.appendChild(tr);
        });
    } catch (err) {
        console.error("Error al cargar contratos:", err);
    }
}

// Pestaña Asistencia: Poblar selector
function cargarAsistenciaTab() {
    poblarDropdown('/api/rrhh/select/empleados', 'selectEmpleadoAsistencias', 'id', 'nombreCompleto', 'Seleccione un empleado...');
}

// Cargar Asistencias de un Empleado
async function cargarAsistenciasPorEmpleado(empleadoId) {
    try {
        const res = await fetch(`/api/rrhh/empleados/${empleadoId}/asistencias`);
        const tbody = document.getElementById('tbAsistencias');
        if (!tbody) return;

        tbody.innerHTML = '';
        if (!res.ok) {
            tbody.innerHTML = `<tr><td colspan="5" class="text-center py-4 text-danger">Error al cargar asistencias.</td></tr>`;
            return;
        }

        const asistencias = await res.json();
        
        // Determinar estado de asistencia de hoy para los botones
        const hoy = new Date();
        const strHoy = hoy.toLocaleDateString('en-CA'); // YYYY-MM-DD local
        let tieneEntradaHoy = false;
        let tieneSalidaHoy = false;

        if (asistencias.length === 0) {
            tbody.innerHTML = `<tr><td colspan="5" class="text-center py-4 text-muted">Sin asistencias registradas recientemente.</td></tr>`;
        } else {
            asistencias.forEach(a => {
                const tr = document.createElement('tr');
                const fecha = formatFecha(a.fecha);
                
                // Extraer fecha para comparar con hoy
                const fechaIso = a.fecha ? a.fecha.split('T')[0] : '';
                if (fechaIso === strHoy) {
                    tieneEntradaHoy = true;
                    if (a.horaSalida) tieneSalidaHoy = true;
                }
                
                // Las horas pueden venir completas en ISO, extraemos solo la hora
                const hEntrada = a.horaEntrada ? new Date(a.horaEntrada).toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' }) : '-';
                const hSalida = a.horaSalida ? new Date(a.horaSalida).toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' }) : '<span class="text-warning fw-bold">Pendiente</span>';
                const hrsTrabajadas = a.horasTrabajadas !== null ? a.horasTrabajadas.toFixed(2) : '-';

                tr.innerHTML = `
                    <td>${fecha}</td>
                    <td class="font-monospace">${hEntrada}</td>
                    <td class="font-monospace">${hSalida}</td>
                    <td class="font-monospace fw-bold">${hrsTrabajadas}</td>
                    <td><small>${a.observaciones || ''}</small></td>
                `;
                tbody.appendChild(tr);
            });
        }
        
        // Actualizar estado de los botones
        const btnEntrada = document.getElementById('btnMarcarEntrada');
        const btnSalida = document.getElementById('btnMarcarSalida');
        const msgAsis = document.getElementById('asistenciaMsg');
        
        if (btnEntrada && btnSalida && msgAsis) {
            if (!tieneEntradaHoy) {
                btnEntrada.disabled = false;
                btnSalida.disabled = true;
                msgAsis.innerText = "No se ha registrado entrada hoy.";
                btnEntrada.style.opacity = '1';
                btnSalida.style.opacity = '0.5';
            } else if (tieneEntradaHoy && !tieneSalidaHoy) {
                btnEntrada.disabled = true;
                btnSalida.disabled = false;
                msgAsis.innerText = "Entrada registrada. No olvide marcar salida.";
                btnEntrada.style.opacity = '0.5';
                btnSalida.style.opacity = '1';
            } else {
                btnEntrada.disabled = true;
                btnSalida.disabled = true;
                msgAsis.innerText = "Asistencia de hoy completada.";
                btnEntrada.style.opacity = '0.5';
                btnSalida.style.opacity = '0.5';
            }
        }
    } catch (err) {
        console.error("Error al cargar asistencias:", err);
    }
}

// Registrar Entrada/Salida rápida
async function registrarAsistenciaRapida(tipo) {
    const empleadoId = document.getElementById('selectEmpleadoAsistencias').value;
    if (!empleadoId) {
        mostrarAlerta("Debe seleccionar un empleado.", "warning");
        return;
    }

    const endpoint = tipo === 'entrada' ? '/api/rrhh/asistencia/entrada' : '/api/rrhh/asistencia/salida';
    const method = tipo === 'entrada' ? 'POST' : 'PUT';

    try {
        const res = await fetch(endpoint, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ empleadoId: parseInt(empleadoId) })
        });

        const data = await res.json();
        if (res.ok) {
            mostrarAlerta(data.mensaje, "success");
            cargarAsistenciasPorEmpleado(parseInt(empleadoId));
            actualizarKPIs();
        } else {
            mostrarAlerta(data.message || data.mensaje || "Error al procesar registro de asistencia.", "danger");
        }
    } catch (err) {
        console.error("Error registrando asistencia:", err);
        mostrarAlerta("Error de conexión al marcar asistencia.", "danger");
    }
}

// Pestaña Ausencias: Poblar selector
function cargarAusenciasTab() {
    poblarDropdown('/api/rrhh/select/empleados', 'selectEmpleadoAusencias', 'id', 'nombreCompleto', 'Seleccione un empleado...');
}

// Cargar Ausencias de un Empleado
async function cargarAusenciasPorEmpleado(empleadoId) {
    try {
        const res = await fetch(`/api/rrhh/empleados/${empleadoId}/ausencias`);
        const tbody = document.getElementById('tbAusencias');
        if (!tbody) return;

        tbody.innerHTML = '';
        if (!res.ok) {
            tbody.innerHTML = `<tr><td colspan="9" class="text-center py-4 text-danger">Error al cargar ausencias.</td></tr>`;
            return;
        }

        const ausencias = await res.json();
        if (ausencias.length === 0) {
            tbody.innerHTML = `<tr><td colspan="9" class="text-center py-4 text-muted">No se registran solicitudes de ausencia.</td></tr>`;
            return;
        }

        ausencias.forEach(a => {
            const tr = document.createElement('tr');
            const fInicio = formatFecha(a.fechaInicio);
            const fFin = formatFecha(a.fechaFin);
            
            let badgeClass = '';
            if (a.estado === 'PENDIENTE') badgeClass = 'frm-badge-pendiente';
            else if (a.estado === 'APROBADA') badgeClass = 'frm-badge-aprobada';
            else if (a.estado === 'RECHAZADA') badgeClass = 'frm-badge-rechazada';

            let actionBtn = '';
            if (a.estado === 'PENDIENTE') {
                actionBtn = `
                    <div class="d-flex gap-1">
                        <button class="frm-btn-save frm-btn-sm" onclick="abrirModalResolucion(${a.id}, 'APROBADA')" style="padding: 4px 8px !important; font-size: 11px !important;">
                            Aprobar
                        </button>
                        <button class="frm-btn-cancel frm-btn-sm text-danger" onclick="abrirModalResolucion(${a.id}, 'RECHAZADA')" style="padding: 4px 8px !important; font-size: 11px !important; border-color: rgba(220,38,38,0.2);">
                            Rechazar
                        </button>
                    </div>
                `;
            } else {
                actionBtn = `<span class="text-muted small">Resuelta</span>`;
            }

            tr.innerHTML = `
                <td>${a.id}</td>
                <td class="fw-bold">${a.tipoAusenciaNombre}</td>
                <td>${fInicio}</td>
                <td>${fFin}</td>
                <td class="font-monospace text-center fw-bold">${a.diasSolicitados}</td>
                <td><small>${a.motivo}</small></td>
                <td><small>${a.aprobadorNombre || 'No asignado'}</small></td>
                <td><span class="frm-badge ${badgeClass}">${a.estado}</span></td>
                <td>${actionBtn}</td>
            `;
            tbody.appendChild(tr);
        });
    } catch (err) {
        console.error("Error al cargar ausencias:", err);
    }
}

// Abrir modal para resolver solicitud
function abrirModalResolucion(ausenciaId, decision) {
    document.getElementById('resolverAusenciaId').value = ausenciaId;
    document.getElementById('resolverDecision').value = decision;
    
    // Cambiar texto del botón confirmador de acuerdo a la decisión
    const btn = document.getElementById('btnConfirmarResolucion');
    if (decision === 'APROBADA') {
        btn.className = 'frm-btn-save';
        btn.innerText = 'Confirmar Aprobación';
    } else {
        btn.className = 'frm-btn-save bg-danger';
        btn.innerText = 'Confirmar Rechazo';
    }

    // Poblar aprobadores
    poblarDropdown('/api/rrhh/select/usuarios-todos', 'resolverAprobador', 'id', 'nombreCompleto', 'Seleccione un aprobador...');
    
    // Mostrar modal
    const modalEl = document.getElementById('modalResolverAusencia');
    const modal = new bootstrap.Modal(modalEl);
    modal.show();
}

// Ver ficha detallada de un empleado
async function verDetallesEmpleado(id) {
    try {
        const res = await fetch(`/api/rrhh/empleados/${id}`);
        if (!res.ok) {
            mostrarAlerta("No se pudo obtener la información detallada del empleado.", "danger");
            return;
        }

        const emp = await res.json();
        
        // Asignar datos al modal
        document.getElementById('detNombreCompleto').innerText = emp.nombreCompleto;
        document.getElementById('detCargo').innerText = emp.cargoNombre || 'Sin asignar';
        document.getElementById('detDocumento').innerText = `${emp.tipoDocumento} - ${emp.numeroDocumento}`;
        document.getElementById('detDepartamento').innerText = emp.departamentoNombre || 'Sin asignar';
        document.getElementById('detFechaIngreso').innerText = formatFecha(emp.fechaIngreso);
        document.getElementById('detRegimenLaboral').innerText = emp.regimenLaboral || 'No asignado';
        document.getElementById('detUsuarioId').innerText = emp.usuarioId || 'Ninguno';
        
        const badge = emp.activo 
            ? `<span class="frm-badge frm-badge-activo">Activo</span>`
            : `<span class="frm-badge frm-badge-inactivo">Inactivo</span>`;
        document.getElementById('detEstado').innerHTML = badge;

        const iniciales = (emp.nombres.charAt(0) + emp.apellidos.charAt(0)).toUpperCase();
        document.getElementById('detAvatar').innerText = iniciales;

        // Mostrar modal
        const modalEl = document.getElementById('modalDetalleEmpleado');
        const modal = new bootstrap.Modal(modalEl);
        modal.show();

    } catch (err) {
        console.error("Error al ver detalles del empleado:", err);
    }
}

// Desactivación lógica de un empleado (Baja)
async function desactivarEmpleado(id) {
    if (!confirm("¿Está seguro de que desea dar de baja (desactivar) a este empleado? Esta acción es irreversible.")) {
        return;
    }

    try {
        const res = await fetch(`/api/rrhh/empleados/${id}/desactivar`, {
            method: 'PUT'
        });

        if (res.ok) {
            mostrarAlerta("Empleado dado de baja exitosamente.", "success");
            cargarEmpleados();
        } else {
            const err = await res.text();
            mostrarAlerta(`Error al desactivar empleado: ${err}`, "danger");
        }
    } catch (err) {
        console.error("Error al dar de baja:", err);
    }
}

// === 5. FUNCIONES UTILS / HELPERS ===

// Poblar dropdown genérico — búsqueda de propiedad insensible a mayúsculas/minúsculas
async function poblarDropdown(url, selectId, valueField = 'id', textField = 'nombre', defaultText = 'Seleccione...') {
    const select = document.getElementById(selectId);
    if (!select) return;
    select.innerHTML = `<option value="">${defaultText}</option>`;
    try {
        const res = await fetch(url);
        if (!res.ok) {
            console.warn(`poblarDropdown [${selectId}]: API devolvió ${res.status} para ${url}`);
            return;
        }
        const data = await res.json();
        if (!Array.isArray(data) || data.length === 0) {
            console.warn(`poblarDropdown [${selectId}]: sin datos en ${url}`);
            return;
        }
        data.forEach(item => {
            // Búsqueda insensible a mayúsculas/minúsculas para el campo valor
            const itemKeys = Object.keys(item);
            const vKey = itemKeys.find(k => k.toLowerCase() === valueField.toLowerCase()) ?? valueField;
            const tKey = itemKeys.find(k => k.toLowerCase() === textField.toLowerCase()) ?? textField;

            const opt = document.createElement('option');
            opt.value = item[vKey];
            opt.textContent = item[tKey];
            select.appendChild(opt);
        });
    } catch (err) {
        console.error(`poblarDropdown [${selectId}] error en ${url}:`, err);
    }
}


// Cerrar modal programáticamente
function cerrarModal(modalId) {
    const modalEl = document.getElementById(modalId);
    const modal = bootstrap.Modal.getInstance(modalEl);
    if (modal) {
        modal.hide();
    } else {
        // Fallback en caso de que no esté instanciado en el pool de Bootstrap
        const m = new bootstrap.Modal(modalEl);
        m.hide();
    }
}

// Reloj dinámico de asistencia
function iniciarReloj() {
    const reloj = document.getElementById('asistenciaReloj');
    const fecha = document.getElementById('asistenciaFecha');
    if (!reloj || !fecha) return;
    
    const actualizar = () => {
        const ahora = new Date();
        reloj.innerText = ahora.toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
        fecha.innerText = ahora.toLocaleDateString('es-ES', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
    };
    actualizar();
    setInterval(actualizar, 1000);
}

// Formateadores de fechas
function formatFecha(isoStr) {
    if (!isoStr) return '-';
    // Cortamos la hora si viene en formato ISO estándar para evitar desfases de zona horaria al instanciar Date
    const limpio = isoStr.includes('T') ? isoStr.split('T')[0] : isoStr;
    const parts = limpio.split('-');
    if (parts.length === 3) {
        // Retorna DD/MM/AAAA
        return `${parts[2]}/${parts[1]}/${parts[0]}`;
    }
    const date = new Date(isoStr);
    if (isNaN(date.getTime())) return isoStr;
    return date.toLocaleDateString('es-ES');
}

// Alerta flotante premium para informar resultados al usuario
function mostrarAlerta(mensaje, tipo = 'success') {
    // Buscar si existe un contenedor de notificaciones, si no crearlo
    let alertContainer = document.getElementById('alert-toast-container');
    if (!alertContainer) {
        alertContainer = document.createElement('div');
        alertContainer.id = 'alert-toast-container';
        alertContainer.style.position = 'fixed';
        alertContainer.style.top = '20px';
        alertContainer.style.right = '20px';
        alertContainer.style.zIndex = '9999';
        alertContainer.style.display = 'flex';
        alertContainer.style.flexDirection = 'column';
        alertContainer.style.gap = '10px';
        document.body.appendChild(alertContainer);
    }

    const alertDiv = document.createElement('div');
    alertDiv.className = `frm-alert-toast toast-${tipo}`;
    alertDiv.style.background = tipo === 'success' ? '#dcfce7' : (tipo === 'warning' ? '#fef3c7' : '#ffe4e6');
    alertDiv.style.color = tipo === 'success' ? '#15803d' : (tipo === 'warning' ? '#b45309' : '#be123c');
    alertDiv.style.border = `1px solid ${tipo === 'success' ? '#bbf7d0' : (tipo === 'warning' ? '#fde68a' : '#fecdd3')}`;
    alertDiv.style.padding = '12px 20px';
    alertDiv.style.borderRadius = 'var(--radius-md, 8px)';
    alertDiv.style.boxShadow = '0 10px 15px -3px rgba(0, 0, 0, 0.1)';
    alertDiv.style.fontSize = '13.5px';
    alertDiv.style.fontWeight = '600';
    alertDiv.style.minWidth = '280px';
    alertDiv.style.maxWidth = '400px';
    alertDiv.style.display = 'flex';
    alertDiv.style.alignItems = 'center';
    alertDiv.style.justifyContent = 'space-between';
    alertDiv.style.animation = 'slideInToast 0.3s ease-out forwards';
    
    // Icono representativo
    let icon = '';
    if (tipo === 'success') {
        icon = `<svg viewBox="0 0 24 24" width="18" height="18" stroke="currentColor" fill="none" stroke-width="2" class="me-2"><polyline points="20 6 9 17 4 12"></polyline></svg>`;
    } else if (tipo === 'warning') {
        icon = `<svg viewBox="0 0 24 24" width="18" height="18" stroke="currentColor" fill="none" stroke-width="2" class="me-2"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="8" x2="12" y2="12"></line><line x1="12" y1="16" x2="12.01" y2="16"></line></svg>`;
    } else {
        icon = `<svg viewBox="0 0 24 24" width="18" height="18" stroke="currentColor" fill="none" stroke-width="2" class="me-2"><circle cx="12" cy="12" r="10"></circle><line x1="15" y1="9" x2="9" y2="15"></line><line x1="9" y1="9" x2="15" y2="15"></line></svg>`;
    }

    alertDiv.innerHTML = `
        <div class="d-flex align-items-center">
            ${icon}
            <span>${mensaje}</span>
        </div>
        <button type="button" style="background:none; border:none; color:inherit; font-size:16px; cursor:pointer;" onclick="this.parentElement.remove()">×</button>
    `;

    alertContainer.appendChild(alertDiv);

    // Desvanecer después de 5 segundos
    setTimeout(() => {
        alertDiv.style.animation = 'slideOutToast 0.3s ease-in forwards';
        setTimeout(() => alertDiv.remove(), 300);
    }, 5000);
}

// Actualizar KPIs de la parte superior del Dashboard de forma centralizada
async function actualizarKPIs(empleadosList = null) {
    try {
        let empleados = empleadosList;
        if (!empleados) {
            const res = await fetch('/api/rrhh/empleados?soloActivos=false');
            if (res.ok) empleados = await res.json();
            else return;
        }

        const activos = empleados.filter(e => e.activo);
        document.getElementById('kpiEmpleados').innerText = activos.length;

        // Contratos Activos: Iterar sobre los empleados activos y comprobar cuántos tienen contrato con estado 'ACTIVO'
        let contratosActivos = 0;
        const contratoPromises = activos.map(emp => 
            fetch(`/api/rrhh/empleados/${emp.id}/contratos`)
                .then(r => r.ok ? r.json() : [])
                .then(contracts => {
                    if (contracts.some(c => c.estado === 'ACTIVO')) {
                        contratosActivos++;
                    }
                })
                .catch(() => {})
        );
        await Promise.all(contratoPromises);
        const kpiContratos = document.getElementById('kpiContratos');
        if (kpiContratos) kpiContratos.innerText = contratosActivos;

        // Asistencias Hoy: Cantidad de empleados activos que registraron entrada/salida hoy
        const hoy = new Date().toISOString().split('T')[0];
        let asistenciasHoy = 0;
        const asistenciaPromises = activos.map(emp => 
            fetch(`/api/rrhh/empleados/${emp.id}/asistencias?desde=${hoy}&hasta=${hoy}`)
                .then(r => r.ok ? r.json() : [])
                .then(asistencias => {
                    if (asistencias.length > 0) {
                        asistenciasHoy++;
                    }
                })
                .catch(() => {})
        );
        await Promise.all(asistenciaPromises);
        const kpiAsistencias = document.getElementById('kpiAsistencias');
        if (kpiAsistencias) kpiAsistencias.innerText = asistenciasHoy;

    } catch (err) {
        console.error("Error al actualizar KPIs generales:", err);
    }
}

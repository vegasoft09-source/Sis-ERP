// Estado global para filtro y búsqueda del directorio de empleados
let _empleadosCache = [];
let _empFiltroActual = 'todos'; // 'todos' | 'activos' | 'inactivos'
let _empBusquedaActual = '';

// Estado global para el panel general de asistencia
let _asistenciaGeneralCache = [];
let _asisFiltroActual = 'todos'; // 'todos' | 'completa' | 'en_curso' | 'sin_registro'
let _asisBusquedaActual = '';

// Estado global para Contratos General
let _contratosGeneralCache = [];
let _contratosFiltroActual = 'todos'; // 'todos' | 'activo' | 'finalizado' | 'anulado'
let _contratosBusquedaActual = '';

// Estado global para Ausencias General
let _ausenciasGeneralCache = [];
let _ausenciasFiltroActual = 'todas'; // 'todas' | 'pendiente' | 'aprobada' | 'rechazada'
let _ausenciasBusquedaActual = '';

// Estado global para Departamentos
let _departamentosCache = [];
let _deptBusquedaActual = '';

// Estado global para Cargos
let _cargosCache = [];
let _cargoBusquedaActual = '';

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

    // === 1.1 BÚSQUEDA Y FILTROS DEL DIRECTORIO DE EMPLEADOS ===

    // Evento: Input de búsqueda con debounce
    const empSearchInput = document.getElementById('empSearchInput');
    const empSearchClear = document.getElementById('empSearchClear');
    let _empSearchTimer = null;

    if (empSearchInput) {
        empSearchInput.addEventListener('input', () => {
            clearTimeout(_empSearchTimer);
            _empSearchTimer = setTimeout(() => {
                _empBusquedaActual = empSearchInput.value.trim().toLowerCase();
                empSearchClear.style.display = _empBusquedaActual ? 'flex' : 'none';
                filtrarYRenderizarEmpleados();
            }, 250);
        });

        // Limpiar búsqueda con Escape
        empSearchInput.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                empSearchInput.value = '';
                _empBusquedaActual = '';
                empSearchClear.style.display = 'none';
                filtrarYRenderizarEmpleados();
            }
        });
    }

    if (empSearchClear) {
        empSearchClear.addEventListener('click', () => {
            empSearchInput.value = '';
            _empBusquedaActual = '';
            empSearchClear.style.display = 'none';
            empSearchInput.focus();
            filtrarYRenderizarEmpleados();
        });
    }

    // Evento: Click en filtro tag (Empleados)
    const empFilterTags = document.getElementById('empFilterTags');
    if (empFilterTags) {
        empFilterTags.addEventListener('click', (e) => {
            const tagBtn = e.target.closest('.emp-filter-tag');
            if (!tagBtn) return;

            empFilterTags.querySelectorAll('.emp-filter-tag').forEach(t => t.classList.remove('active'));
            tagBtn.classList.add('active');

            _empFiltroActual = tagBtn.dataset.filter;
            filtrarYRenderizarEmpleados();
        });
    }

    // === 1.2 NAVEGACIÓN Y FILTROS - PANELES GENERALES (ASISTENCIA, CONTRATOS, AUSENCIAS) ===
    
    // Navegación entre sub-vistas (Pills) con scope por contenedor
    const asisNavContainers = document.querySelectorAll('.asis-nav-pills');
    asisNavContainers.forEach(container => {
        const pills = container.querySelectorAll('.asis-pill');
        pills.forEach(pill => {
            pill.addEventListener('click', () => {
                // Quitar active solo a las pills de este contenedor
                pills.forEach(p => p.classList.remove('active'));
                pill.classList.add('active');
                
                const viewId = pill.dataset.view; // ej: 'general', 'individual', 'general-contratos'
                
                // Buscar el tab-content padre (ej: tab-asistencia, tab-contratos)
                const parentTab = container.closest('.tab-content');
                if (parentTab) {
                    const sections = parentTab.querySelectorAll('.asis-view-section');
                    sections.forEach(sec => {
                        if (sec.id.includes(viewId) || viewId.includes(sec.id.replace('-view-', ''))) {
                             // Lógica simple: el ID de la sección suele ser [prefijo]-view-[viewId] o viceversa
                             // En el HTML pusimos id="contratos-view-general" y data-view="general-contratos"
                             // Para Asistencia: id="asis-view-general" y data-view="general"
                             const isActive = sec.id.endsWith(viewId) || viewId.endsWith(sec.id.split('-').pop());
                             sec.classList.toggle('active', isActive);
                             sec.style.display = isActive ? 'block' : 'none';
                        }
                    });
                }
            });
        });
    });

    // DatePicker Panel General Asistencia
    const asisGeneralFecha = document.getElementById('asisGeneralFecha');
    if (asisGeneralFecha) {
        const hoy = new Date().toLocaleDateString('en-CA');
        asisGeneralFecha.value = hoy;
        asisGeneralFecha.max = hoy;
        
        asisGeneralFecha.addEventListener('change', () => {
            cargarAsistenciaGeneral(asisGeneralFecha.value);
        });
    }

    // Búsqueda Panel General
    const asisSearchInput = document.getElementById('asisSearchInput');
    const asisSearchClear = document.getElementById('asisSearchClear');
    let _asisSearchTimer = null;

    if (asisSearchInput) {
        asisSearchInput.addEventListener('input', () => {
            clearTimeout(_asisSearchTimer);
            _asisSearchTimer = setTimeout(() => {
                _asisBusquedaActual = asisSearchInput.value.trim().toLowerCase();
                if (asisSearchClear) asisSearchClear.style.display = _asisBusquedaActual ? 'flex' : 'none';
                filtrarYRenderizarAsistenciaGeneral();
            }, 250);
        });

        asisSearchInput.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                asisSearchInput.value = '';
                _asisBusquedaActual = '';
                if (asisSearchClear) asisSearchClear.style.display = 'none';
                filtrarYRenderizarAsistenciaGeneral();
            }
        });
    }

    if (asisSearchClear) {
        asisSearchClear.addEventListener('click', () => {
            asisSearchInput.value = '';
            _asisBusquedaActual = '';
            asisSearchClear.style.display = 'none';
            asisSearchInput.focus();
            filtrarYRenderizarAsistenciaGeneral();
        });
    }

    // Filtros por Estado Panel General (Asistencia)
    const asisFilterTags = document.getElementById('asisFilterTags');
    if (asisFilterTags) {
        asisFilterTags.addEventListener('click', (e) => {
            const tagBtn = e.target.closest('.emp-filter-tag');
            if (!tagBtn) return;

            asisFilterTags.querySelectorAll('.emp-filter-tag').forEach(t => t.classList.remove('active'));
            tagBtn.classList.add('active');

            _asisFiltroActual = tagBtn.dataset.filter;
            filtrarYRenderizarAsistenciaGeneral();
        });
    }

    // === Búsqueda y Filtros: Panel General de Contratos ===
    const contratosSearchInput = document.getElementById('contratosSearchInput');
    const contratosSearchClear = document.getElementById('contratosSearchClear');
    let _contratosSearchTimer = null;

    if (contratosSearchInput) {
        contratosSearchInput.addEventListener('input', () => {
            clearTimeout(_contratosSearchTimer);
            _contratosSearchTimer = setTimeout(() => {
                _contratosBusquedaActual = contratosSearchInput.value.trim().toLowerCase();
                if (contratosSearchClear) contratosSearchClear.style.display = _contratosBusquedaActual ? 'flex' : 'none';
                filtrarYRenderizarContratosGeneral();
            }, 250);
        });

        contratosSearchClear.addEventListener('click', () => {
            contratosSearchInput.value = '';
            _contratosBusquedaActual = '';
            contratosSearchClear.style.display = 'none';
            contratosSearchInput.focus();
            filtrarYRenderizarContratosGeneral();
        });
    }

    const contratosFilterTags = document.getElementById('contratosFilterTags');
    if (contratosFilterTags) {
        contratosFilterTags.addEventListener('click', (e) => {
            const tagBtn = e.target.closest('.emp-filter-tag');
            if (!tagBtn) return;
            contratosFilterTags.querySelectorAll('.emp-filter-tag').forEach(t => t.classList.remove('active'));
            tagBtn.classList.add('active');
            _contratosFiltroActual = tagBtn.dataset.filter;
            filtrarYRenderizarContratosGeneral();
        });
    }

    // === Búsqueda y Filtros: Panel General de Ausencias ===
    const ausenciasSearchInput = document.getElementById('ausenciasSearchInput');
    const ausenciasSearchClear = document.getElementById('ausenciasSearchClear');
    let _ausenciasSearchTimer = null;

    if (ausenciasSearchInput) {
        ausenciasSearchInput.addEventListener('input', () => {
            clearTimeout(_ausenciasSearchTimer);
            _ausenciasSearchTimer = setTimeout(() => {
                _ausenciasBusquedaActual = ausenciasSearchInput.value.trim().toLowerCase();
                if (ausenciasSearchClear) ausenciasSearchClear.style.display = _ausenciasBusquedaActual ? 'flex' : 'none';
                filtrarYRenderizarAusenciasGeneral();
            }, 250);
        });

        ausenciasSearchClear.addEventListener('click', () => {
            ausenciasSearchInput.value = '';
            _ausenciasBusquedaActual = '';
            ausenciasSearchClear.style.display = 'none';
            ausenciasSearchInput.focus();
            filtrarYRenderizarAusenciasGeneral();
        });
    }

    const ausenciasFilterTags = document.getElementById('ausenciasFilterTags');
    if (ausenciasFilterTags) {
        ausenciasFilterTags.addEventListener('click', (e) => {
            const tagBtn = e.target.closest('.emp-filter-tag');
            if (!tagBtn) return;
            ausenciasFilterTags.querySelectorAll('.emp-filter-tag').forEach(t => t.classList.remove('active'));
            tagBtn.classList.add('active');
            _ausenciasFiltroActual = tagBtn.dataset.filter;
            filtrarYRenderizarAusenciasGeneral();
        });
    }

    // === Búsqueda: Departamentos ===
    const deptSearchInput = document.getElementById('deptSearchInput');
    const deptSearchClear = document.getElementById('deptSearchClear');
    let _deptSearchTimer = null;

    if (deptSearchInput) {
        deptSearchInput.addEventListener('input', () => {
            clearTimeout(_deptSearchTimer);
            _deptSearchTimer = setTimeout(() => {
                _deptBusquedaActual = deptSearchInput.value.trim().toLowerCase();
                if (deptSearchClear) deptSearchClear.style.display = _deptBusquedaActual ? 'flex' : 'none';
                filtrarYRenderizarDepartamentos();
            }, 250);
        });

        if (deptSearchClear) {
            deptSearchClear.addEventListener('click', () => {
                deptSearchInput.value = '';
                _deptBusquedaActual = '';
                deptSearchClear.style.display = 'none';
                deptSearchInput.focus();
                filtrarYRenderizarDepartamentos();
            });
        }
    }

    // === Búsqueda: Cargos ===
    const cargoSearchInput = document.getElementById('cargoSearchInput');
    const cargoSearchClear = document.getElementById('cargoSearchClear');
    let _cargoSearchTimer = null;

    if (cargoSearchInput) {
        cargoSearchInput.addEventListener('input', () => {
            clearTimeout(_cargoSearchTimer);
            _cargoSearchTimer = setTimeout(() => {
                _cargoBusquedaActual = cargoSearchInput.value.trim().toLowerCase();
                if (cargoSearchClear) cargoSearchClear.style.display = _cargoBusquedaActual ? 'flex' : 'none';
                filtrarYRenderizarCargos();
            }, 250);
        });

        if (cargoSearchClear) {
            cargoSearchClear.addEventListener('click', () => {
                cargoSearchInput.value = '';
                _cargoBusquedaActual = '';
                cargoSearchClear.style.display = 'none';
                cargoSearchInput.focus();
                filtrarYRenderizarCargos();
            });
        }
    }

    iniciarReloj();

    // === 2. EVENTOS DE FORMULARIOS Y MODALES ===

    // Formulario: Nuevo Empleado
    const frmNuevoEmpleado = document.getElementById('frmNuevoEmpleado');
    if (frmNuevoEmpleado) {
        document.getElementById('empFechaIngreso').valueAsDate = new Date();
        frmNuevoEmpleado.addEventListener('submit', async (e) => {
            e.preventDefault();
            const idVal = document.getElementById('empId').value;
            const usuarioVal = document.getElementById('empUsuario').value;
            const responsableVal = document.getElementById('empResponsable').value;
            const datosEmpleado = {
                nombres: document.getElementById('empNombres').value.trim(),
                apellidos: document.getElementById('empApellidos').value.trim(),
                tipoDocumento: document.getElementById('empTipoDoc').value,
                numeroDocumento: document.getElementById('empNumDoc').value.trim(),
                fechaNacimiento: document.getElementById('empFechaNacimiento').value || null,
                genero: document.getElementById('empGenero').value || null,
                estadoCivil: document.getElementById('empEstadoCivil').value || null,
                telefono: document.getElementById('empTelefono').value.trim() || null,
                celular: document.getElementById('empCelular').value.trim() || null,
                correoPersonal: document.getElementById('empCorreoPersonal').value.trim() || null,
                correoEmpresa: document.getElementById('empCorreoEmpresa').value.trim() || null,
                direccion: document.getElementById('empDireccion').value.trim() || null,
                departamentoId: parseInt(document.getElementById('empDepto').value),
                cargoId: parseInt(document.getElementById('empCargo').value),
                responsableId: responsableVal ? parseInt(responsableVal) : null,
                usuarioId: usuarioVal ? parseInt(usuarioVal) : null,
                fechaIngreso: document.getElementById('empFechaIngreso').value || null,
                fechaCese: document.getElementById('empFechaCese').value || null,
                tipoContrato: document.getElementById('empTipoContrato').value,
                regimenLaboral: document.getElementById('empRegimenLaboral').value.trim(),
                activo: document.getElementById('empActivo').checked
            };

            const isEdit = !!idVal;
            const method = isEdit ? 'PUT' : 'POST';
            const endpoint = isEdit ? `/api/rrhh/empleados/${idVal}` : '/api/rrhh/empleados';

            try {
                const response = await fetch(endpoint, {
                    method: method,
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(datosEmpleado)
                });

                if (response.ok) {
                    frmNuevoEmpleado.reset();
                    document.getElementById('empId').value = '';
                    document.getElementById('empFechaIngreso').valueAsDate = new Date();
                    cerrarModal('modalNuevoEmpleado');
                    cargarEmpleados();
                    mostrarAlerta(isEdit ? 'Empleado actualizado exitosamente.' : 'Empleado registrado exitosamente.', 'success');
                } else {
                    const errorText = await response.text();
                    mostrarAlerta(`Error al guardar empleado: ${errorText}`, 'danger');
                }
            } catch (err) {
                console.error("Error al guardar Empleado:", err);
                mostrarAlerta("Error de conexión al guardar el empleado.", 'danger');
            }
        });

        // Limpiar el modal al cerrarlo para evitar datos cruzados
        const modalEmpEl = document.getElementById('modalNuevoEmpleado');
        if (modalEmpEl) {
            modalEmpEl.addEventListener('hidden.bs.modal', () => {
                frmNuevoEmpleado.reset();
                document.getElementById('empId').value = '';
                document.getElementById('modalNuevoEmpleadoLabel').innerText = 'Nuevo Empleado';
            });
            // Cargar selectores si están vacíos
            modalEmpEl.addEventListener('show.bs.modal', () => {
                if (document.getElementById('empDepto').options.length <= 1) {
                    poblarDropdown('/api/rrhh/select/departamentos', 'empDepto', 'id', 'nombre', 'Seleccione departamento...');
                    poblarDropdown('/api/rrhh/select/cargos', 'empCargo', 'id', 'nombre', 'Seleccione cargo...');
                    poblarDropdown('/api/rrhh/select/usuarios', 'empUsuario', 'id', 'nombreCompleto', 'Ninguno');
                    poblarDropdown('/api/rrhh/select/empleados', 'empResponsable', 'id', 'nombreCompleto', 'Ninguno');
                }
            });
        }
    }

    // Formulario: Nuevo Departamento
    const frmNuevoDepartamento = document.getElementById('frmNuevoDepartamento');
    if (frmNuevoDepartamento) {
        frmNuevoDepartamento.addEventListener('submit', async (e) => {
            e.preventDefault();
            const idVal = document.getElementById('deptId').value;
            const respVal = document.getElementById('deptResponsable').value;
            const respInt = respVal ? parseInt(respVal, 10) : null;
            const datos = {
                nombre: document.getElementById('deptNombre').value.trim(),
                responsableId: (respInt && !isNaN(respInt)) ? respInt : null
            };

            const isEdit = !!idVal;
            const method = isEdit ? 'PUT' : 'POST';
            const endpoint = isEdit ? `/api/rrhh/departamentos/${idVal}` : '/api/rrhh/departamentos';

            try {
                const response = await fetch(endpoint, {
                    method,
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(datos)
                });

                if (response.ok) {
                    frmNuevoDepartamento.reset();
                    document.getElementById('deptId').value = '';
                    cerrarModal('modalNuevoDepartamento');
                    cargarDepartamentos();
                    actualizarKPIs();
                    mostrarAlerta(isEdit ? 'Departamento actualizado exitosamente.' : 'Departamento creado exitosamente.', 'success');
                } else {
                    const errorText = await response.text();
                    mostrarAlerta(`Error al guardar departamento: ${errorText}`, 'danger');
                }
            } catch (err) {
                console.error("Error en Departamento:", err);
                mostrarAlerta("Error de conexión al guardar departamento.", 'danger');
            }
        });

        const modalDeptoEl = document.getElementById('modalNuevoDepartamento');
        if (modalDeptoEl) {
            modalDeptoEl.addEventListener('hidden.bs.modal', () => {
                frmNuevoDepartamento.reset();
                document.getElementById('deptId').value = '';
                document.getElementById('modalNuevoDepartamentoLabel').innerText = 'Nuevo Departamento';
            });
        }
        // Botón Nuevo: poblar dropdown y abrir limpio
        const btnNuevoDepto = document.getElementById('btnNuevoDepartamento');
        if (btnNuevoDepto) {
            btnNuevoDepto.addEventListener('click', async () => {
                await poblarDropdown('/api/rrhh/select/usuarios-todos', 'deptResponsable', 'id', 'nombreCompleto', 'Sin responsable');
                document.getElementById('deptId').value = '';
                document.getElementById('deptNombre').value = '';
                document.getElementById('modalNuevoDepartamentoLabel').innerText = 'Nuevo Departamento';
                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalNuevoDepartamento')).show();
            });
        }
    }

    // Formulario: Nuevo/Editar Cargo
    const frmNuevoCargo = document.getElementById('frmNuevoCargo');
    if (frmNuevoCargo) {
        frmNuevoCargo.addEventListener('submit', async (e) => {
            e.preventDefault();
            const idVal = document.getElementById('cargoId').value;
            const datos = {
                nombre: document.getElementById('cargoNombre').value.trim(),
                descripcion: document.getElementById('cargoDescripcion').value.trim()
            };

            const isEdit = !!idVal;
            const method = isEdit ? 'PUT' : 'POST';
            const endpoint = isEdit ? `/api/rrhh/cargos/${idVal}` : '/api/rrhh/cargos';

            try {
                const response = await fetch(endpoint, {
                    method,
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(datos)
                });

                if (response.ok) {
                    frmNuevoCargo.reset();
                    document.getElementById('cargoId').value = '';
                    cerrarModal('modalNuevoCargo');
                    cargarCargos();
                    actualizarKPIs();
                    mostrarAlerta(isEdit ? 'Cargo actualizado exitosamente.' : 'Cargo creado exitosamente.', 'success');
                } else {
                    const errorText = await response.text();
                    mostrarAlerta(`Error al guardar cargo: ${errorText}`, 'danger');
                }
            } catch (err) {
                console.error("Error en Cargo:", err);
                mostrarAlerta("Error de conexión al guardar cargo.", 'danger');
            }
        });

        const modalCargoEl = document.getElementById('modalNuevoCargo');
        if (modalCargoEl) {
            modalCargoEl.addEventListener('hidden.bs.modal', () => {
                frmNuevoCargo.reset();
                document.getElementById('cargoId').value = '';
                document.getElementById('modalNuevoCargoLabel').innerText = 'Nuevo Cargo';
            });
        }

        // Botón Nuevo: abrir limpio
        const btnNuevoCargo = document.getElementById('btnNuevoCargo');
        if (btnNuevoCargo) {
            btnNuevoCargo.addEventListener('click', () => {
                document.getElementById('cargoId').value = '';
                document.getElementById('cargoNombre').value = '';
                document.getElementById('cargoDescripcion').value = '';
                document.getElementById('modalNuevoCargoLabel').innerText = 'Nuevo Cargo';
                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalNuevoCargo')).show();
            });
        }
    }

    // Formulario: Nuevo/Editar Contrato
    const frmNuevoContrato = document.getElementById('frmNuevoContrato');
    if (frmNuevoContrato) {
        document.getElementById('contratoFechaInicio').valueAsDate = new Date();

        frmNuevoContrato.addEventListener('submit', async (e) => {
            e.preventDefault();
            const idVal = document.getElementById('contratoId').value;
            const fechaFinVal = document.getElementById('contratoFechaFin').value;
            const datos = {
                empleadoId: parseInt(document.getElementById('contratoEmpleado').value),
                nombre: document.getElementById('contratoNombre').value.trim(),
                fechaInicio: document.getElementById('contratoFechaInicio').value,
                fechaFin: fechaFinVal ? fechaFinVal : null,
                sueldo: parseFloat(document.getElementById('contratoSueldo').value),
                monedaId: parseInt(document.getElementById('contratoMoneda').value),
                tipoContrato: document.getElementById('contratoTipo').value,
                estado: document.getElementById('contratoEstado').value
            };

            const isEdit = !!idVal;
            const method = isEdit ? 'PUT' : 'POST';
            const endpoint = isEdit ? `/api/rrhh/contratos/${idVal}` : '/api/rrhh/contratos';

            try {
                const response = await fetch(endpoint, {
                    method,
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(datos)
                });

                if (response.ok) {
                    frmNuevoContrato.reset();
                    document.getElementById('contratoId').value = '';
                    document.getElementById('contratoFechaInicio').valueAsDate = new Date();
                    document.getElementById('contratoEstadoWrap').style.display = 'none';
                    cerrarModal('modalNuevoContrato');

                    cargarContratosGeneral();
                    actualizarKPIs();
                    mostrarAlerta(isEdit ? 'Contrato actualizado exitosamente.' : 'Contrato registrado exitosamente. Contratos activos anteriores cerrados.', 'success');
                } else {
                    const errorText = await response.text();
                    mostrarAlerta(`Error al guardar contrato: ${errorText}`, 'danger');
                }
            } catch (err) {
                console.error("Error en Contrato:", err);
                mostrarAlerta("Error de conexión al guardar contrato.", 'danger');
            }
        });

        const modalContratoEl = document.getElementById('modalNuevoContrato');
        if (modalContratoEl) {
            modalContratoEl.addEventListener('hidden.bs.modal', () => {
                frmNuevoContrato.reset();
                document.getElementById('contratoId').value = '';
                document.getElementById('contratoEstadoWrap').style.display = 'none';
                document.getElementById('modalNuevoContratoLabel').innerText = 'Nuevo Contrato';
            });
        }

        // Botón Nuevo: abrir limpio
        const btnNuevoContrato = document.getElementById('btnNuevoContrato');
        if (btnNuevoContrato) {
            btnNuevoContrato.addEventListener('click', async () => {
                document.getElementById('contratoId').value = '';
                document.getElementById('modalNuevoContratoLabel').innerText = 'Nuevo Contrato';
                document.getElementById('contratoEstadoWrap').style.display = 'none';
                await poblarDropdown('/api/rrhh/select/empleados', 'contratoEmpleado', 'id', 'nombreCompleto', 'Seleccione empleado...');
                await poblarDropdown('/api/rrhh/select/monedas', 'contratoMoneda', 'id', 'nombre', 'Seleccione moneda...');

                document.getElementById('contratoFechaInicio').valueAsDate = new Date();
                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalNuevoContrato')).show();
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

                    cargarAusenciasGeneral();
                    actualizarKPIs();
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
                    cargarAusenciasGeneral();
                    actualizarKPIs();
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

    const frmNuevaAsistencia = document.getElementById('frmNuevaAsistencia');
    if (frmNuevaAsistencia) {
        frmNuevaAsistencia.addEventListener('submit', async (e) => {
            e.preventDefault();
            const btn = frmNuevaAsistencia.querySelector('button[type="submit"]');
            btn.disabled = true;

            let horaSalida = null;
            const valSalida = document.getElementById('asistenciaManualSalida').value;
            if (valSalida) {
                horaSalida = document.getElementById('asistenciaManualFecha').value + 'T' + valSalida + ':00';
            }

            const payload = {
                EmpleadoId: parseInt(document.getElementById('asistenciaSelectEmpleado').value),
                Fecha: document.getElementById('asistenciaManualFecha').value,
                HoraEntrada: document.getElementById('asistenciaManualFecha').value + 'T' + document.getElementById('asistenciaManualEntrada').value + ':00',
                HoraSalida: horaSalida,
                Observaciones: document.getElementById('asistenciaManualObservaciones').value
            };

            try {
                const res = await fetch('/api/rrhh/asistencia/manual', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });
                
                let data;
                const contentType = res.headers.get('content-type');
                if (contentType && contentType.includes('application/json')) {
                    data = await res.json();
                } else {
                    data = await res.text();
                }
                
                if (res.ok) {
                    mostrarAlerta((data && data.mensaje) || 'Asistencia manual registrada.', 'success');
                    bootstrap.Modal.getInstance(document.getElementById('modalNuevaAsistencia')).hide();
                    const asisGeneralFecha = document.getElementById('asisGeneralFecha');
                    if (asisGeneralFecha) cargarAsistenciaGeneral(asisGeneralFecha.value);
                } else {
                    const errorMsg = (data && typeof data === 'object') ? (data.mensaje || data.message) : data;
                    mostrarAlerta(errorMsg || 'Error al registrar asistencia manual', 'danger');
                }
            } catch (err) {
                console.error(err);
                mostrarAlerta('Error de red al enviar formulario', 'danger');
            } finally {
                btn.disabled = false;
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
            const id = selectEmpAsistencias.value;
            const pnlMarcacion = document.getElementById('pnlMarcacion');
            const placeholder = document.getElementById('pnlMarcacionPlaceholder');
        
            if (id) {
                if (pnlMarcacion) pnlMarcacion.style.display = 'flex';
                if (placeholder) placeholder.style.display = 'none';
                cargarAsistenciasPorEmpleado(parseInt(id));
            } else {
                if (pnlMarcacion) pnlMarcacion.style.display = 'none';
                if (placeholder) placeholder.style.display = 'block';
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
    const btnGuardarObs = document.getElementById('btnGuardarObservacion');
    if (btnGuardarObs) {
        btnGuardarObs.addEventListener('click', () => actualizarObservacionAsistencia());
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

        // Almacenar en caché global para filtros client-side
        _empleadosCache = empleados;

        // Actualizar contadores de los filter tags
        actualizarContadoresFiltro(empleados);

        // Renderizar con filtro actual
        filtrarYRenderizarEmpleados();

        actualizarKPIs(empleados);

    } catch (err) {
        console.error("Error al cargar empleados:", err);
    }
}

// Actualizar contadores en los filter tags
function actualizarContadoresFiltro(empleados) {
    const total = empleados.length;
    const activos = empleados.filter(e => e.activo).length;
    const inactivos = total - activos;

    const elTodos = document.getElementById('countTodos');
    const elActivos = document.getElementById('countActivos');
    const elInactivos = document.getElementById('countInactivos');

    if (elTodos) elTodos.textContent = total;
    if (elActivos) elActivos.textContent = activos;
    if (elInactivos) elInactivos.textContent = inactivos;
}

// Filtrar y renderizar empleados según estado y búsqueda
function filtrarYRenderizarEmpleados() {
    let filtrados = [..._empleadosCache];

    // 1. Filtrar por estado
    if (_empFiltroActual === 'activos') {
        filtrados = filtrados.filter(e => e.activo);
    } else if (_empFiltroActual === 'inactivos') {
        filtrados = filtrados.filter(e => !e.activo);
    }

    // 2. Filtrar por texto de búsqueda
    if (_empBusquedaActual) {
        const termino = _empBusquedaActual;
        filtrados = filtrados.filter(e => {
            const campos = [
                e.nombreCompleto,
                e.departamentoNombre,
                e.cargoNombre,
                e.numeroDocumento,
                e.tipoDocumento
            ].filter(Boolean).map(c => c.toLowerCase());
            return campos.some(c => c.includes(termino));
        });
    }

    // Renderizar
    renderizarTablaEmpleados(filtrados);

    // Actualizar barra de resultados
    const resultsBar = document.getElementById('empResultsBar');
    const resultsText = document.getElementById('empResultsText');
    const isFiltered = _empFiltroActual !== 'todos' || _empBusquedaActual;

    if (isFiltered && resultsBar && resultsText) {
        resultsBar.classList.add('visible');
        const total = _empleadosCache.length;
        const mostrados = filtrados.length;
        let texto = `Mostrando ${mostrados} de ${total} empleados`;

        if (_empFiltroActual !== 'todos') {
            texto += ` · Filtro: <strong>${_empFiltroActual === 'activos' ? 'Activos' : 'Inactivos'}</strong>`;
        }
        if (_empBusquedaActual) {
            texto += ` · Búsqueda: "<strong>${_empBusquedaActual}</strong>"`;
        }
        resultsText.innerHTML = texto;
    } else if (resultsBar) {
        resultsBar.classList.remove('visible');
    }
}

// Renderizar tabla de empleados (acepta lista ya filtrada)
function renderizarTablaEmpleados(empleados) {
    const tbody = document.getElementById('tbEmpleados');
    if (!tbody) return;

    tbody.innerHTML = '';
    if (empleados.length === 0) {
        const msg = _empBusquedaActual
            ? `No se encontraron empleados que coincidan con "<strong>${_empBusquedaActual}</strong>".`
            : (_empFiltroActual === 'activos'
                ? 'No hay empleados activos registrados.'
                : (_empFiltroActual === 'inactivos'
                    ? 'No hay empleados inactivos.'
                    : 'No hay empleados registrados.'));

        tbody.innerHTML = `<tr class="emp-no-results"><td colspan="6">${msg}</td></tr>`;
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
                <button class="action-btn text-warning mx-1" onclick="editarEmpleado(${emp.id})" title="Editar">
                    <svg viewBox="0 0 24 24" stroke="currentColor" fill="none" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 20h9"/><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><path d="M12 11l5-5 2 2-5 5-2-2z"/></svg>
                </button>
                ${btnBaja}
            </td>
        `;
        tbody.appendChild(tr);
    });
}

// Cargar Departamentos
async function cargarDepartamentos() {
    try {
        const res = await fetch('/api/rrhh/departamentos');
        if (!res.ok) return;
        _departamentosCache = await res.json();
        filtrarYRenderizarDepartamentos();
    } catch (err) {
        console.error("Error al cargar departamentos:", err);
    }
}

function filtrarYRenderizarDepartamentos() {
    let filtrados = [..._departamentosCache];

    if (_deptBusquedaActual) {
        filtrados = filtrados.filter(d => 
            (d.nombre && d.nombre.toLowerCase().includes(_deptBusquedaActual)) ||
            (d.responsableNombre && d.responsableNombre.toLowerCase().includes(_deptBusquedaActual))
        );
    }

    const tbody = document.getElementById('tbDepartamentos');
    if (!tbody) return;
    
    tbody.innerHTML = '';
    if (filtrados.length === 0) {
        tbody.innerHTML = `<tr><td colspan="4" class="text-center py-4">No hay departamentos configurados o no coinciden con la búsqueda.</td></tr>`;
        return;
    }
    
    filtrados.forEach(d => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${d.id}</td>
            <td class="fw-bold">${d.nombre}</td>
            <td>${d.responsableNombre || '<span class="text-muted small">Sin asignar</span>'}</td>
            <td>
                <button class="action-btn text-warning mx-1" onclick="editarDepartamento(${d.id}, '${d.nombre.replace(/'/g, "\\'")}', ${d.responsableId || 0})" title="Editar">
                    <svg viewBox="0 0 24 24" stroke="currentColor" fill="none" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="16" height="16"><path d="M12 20h9"/><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><path d="M12 11l5-5 2 2-5 5-2-2z"/></svg>
                </button>
                <button class="action-btn text-danger mx-1" onclick="eliminarDepartamento(${d.id}, '${d.nombre.replace(/'/g, "\\'")}')">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16"><path d="M3 6h18M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                </button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

// Cargar Cargos
async function cargarCargos() {
    try {
        const res = await fetch('/api/rrhh/cargos');
        if (!res.ok) return;
        _cargosCache = await res.json();
        filtrarYRenderizarCargos();
    } catch (err) {
        console.error("Error al cargar cargos:", err);
    }
}

function filtrarYRenderizarCargos() {
    let filtrados = [..._cargosCache];

    if (_cargoBusquedaActual) {
        filtrados = filtrados.filter(c => 
            (c.nombre && c.nombre.toLowerCase().includes(_cargoBusquedaActual)) ||
            (c.descripcion && c.descripcion.toLowerCase().includes(_cargoBusquedaActual))
        );
    }

    const tbody = document.getElementById('tbCargos');
    if (!tbody) return;
    
    tbody.innerHTML = '';
    if (filtrados.length === 0) {
        tbody.innerHTML = `<tr><td colspan="4" class="text-center py-4">No hay cargos configurados o no coinciden con la búsqueda.</td></tr>`;
        return;
    }
    
    filtrados.forEach(c => {
        const tr = document.createElement('tr');
        const nombreEscapado = c.nombre.replace(/'/g, "\\'");
        const descEscapada = (c.descripcion || '').replace(/'/g, "\\'");
        tr.innerHTML = `
            <td>${c.id}</td>
            <td class="fw-bold">${c.nombre}</td>
            <td>${c.descripcion || '<span class="text-muted small">Sin descripción</span>'}</td>
            <td>
                <button class="action-btn text-warning mx-1" onclick="editarCargo(${c.id}, '${nombreEscapado}', '${descEscapada}')" title="Editar">
                    <svg viewBox="0 0 24 24" stroke="currentColor" fill="none" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="16" height="16"><path d="M12 20h9"/><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><path d="M12 11l5-5 2 2-5 5-2-2z"/></svg>
                </button>
                <button class="action-btn text-danger mx-1" onclick="eliminarCargo(${c.id}, '${nombreEscapado}')" title="Eliminar">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16"><path d="M3 6h18M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                </button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

// Pestaña Contratos
function cargarContratosTab() {
    poblarDropdown('/api/rrhh/select/empleados', 'selectEmpleadoContratos', 'id', 'nombreCompleto', 'Seleccione un empleado...');
    cargarContratosGeneral();
}

async function cargarContratosGeneral() {
    try {
        const tbody = document.getElementById('tbContratosGeneral');
        if (tbody) tbody.innerHTML = `<tr><td colspan="6" class="text-center py-4 text-muted">Cargando datos de contratos...</td></tr>`;

        const res = await fetch('/api/rrhh/contratos/general');
        if (!res.ok) throw new Error("Error al cargar contratos generales");
        
        _contratosGeneralCache = await res.json();
        filtrarYRenderizarContratosGeneral();
    } catch (err) {
        console.error("Error al cargar contratos generales:", err);
        const tbody = document.getElementById('tbContratosGeneral');
        if (tbody) tbody.innerHTML = `<tr><td colspan="6" class="text-center py-4 text-danger">Error al cargar datos. Intente nuevamente.</td></tr>`;
    }
}

function filtrarYRenderizarContratosGeneral() {
    let filtrados = [..._contratosGeneralCache];

    if (_contratosFiltroActual !== 'todos') {
        filtrados = filtrados.filter(c => c.estado && c.estado.toLowerCase() === _contratosFiltroActual);
    }

    if (_contratosBusquedaActual) {
        const termino = _contratosBusquedaActual;
        filtrados = filtrados.filter(c => {
            const campos = [c.empleadoNombre, c.departamentoNombre, c.nombre].filter(Boolean).map(x => x.toLowerCase());
            return campos.some(x => x.includes(termino));
        });
    }

    const tbody = document.getElementById('tbContratosGeneral');
    if (!tbody) return;

    tbody.innerHTML = '';
    if (filtrados.length === 0) {
        tbody.innerHTML = `<tr><td colspan="6" class="text-center py-4 text-muted">No se encontraron contratos con estos filtros.</td></tr>`;
        return;
    }

    filtrados.forEach(c => {
        const tr = document.createElement('tr');
        const iniciales = c.empleadoNombre ? (c.empleadoNombre.split(' ').map(n => n.charAt(0)).join('')).substring(0, 2).toUpperCase() : 'EMP';
        const fInicio = formatFecha(c.fechaInicio);
        const fFin = c.fechaFin ? formatFecha(c.fechaFin) : '-';
        
        let badgeHtml = '';
        if (c.estado === 'ACTIVO') badgeHtml = `<span class="badge-marcacion badge-completa">Activo</span>`;
        else if (c.estado === 'FINALIZADO') badgeHtml = `<span class="badge-marcacion badge-sinregistro">Finalizado</span>`;
        else badgeHtml = `<span class="badge-marcacion badge-encurso" style="color:#ef4444; border-color:rgba(239,68,68,0.2); background:rgba(239,68,68,0.1)">${c.estado}</span>`;

        tr.innerHTML = `
            <td>
                <div class="empleado-info">
                    <div class="empleado-avatar" style="width: 32px; height: 32px; font-size: 11px;">${iniciales}</div>
                    <div class="d-flex flex-column ms-2">
                        <span class="empleado-name fw-bold">${c.empleadoNombre || '-'}</span>
                        <span class="text-muted" style="font-size: 11px;">${c.departamentoNombre || '-'}</span>
                    </div>
                </div>
            </td>
            <td class="fw-bold text-secondary">${c.nombre}</td>
            <td class="font-monospace">${fInicio}</td>
            <td class="font-monospace">${fFin}</td>
            <td class="font-monospace fw-bold">${c.moneda} ${c.sueldo.toFixed(2)}</td>
            <td>${badgeHtml}</td>
            <td>
                <div class="d-flex gap-2">
                    <button class="btn btn-sm btn-outline-primary border-0" onclick="editarContrato(${c.id})" title="Editar Contrato">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="16" height="16"><path d="M12 20h9"></path><path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"></path></svg>
                    </button>
                    <button class="btn btn-sm btn-outline-danger border-0" onclick="eliminarContrato(${c.id})" title="Eliminar Contrato">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="16" height="16"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path><line x1="10" y1="11" x2="10" y2="17"></line><line x1="14" y1="11" x2="14" y2="17"></line></svg>
                    </button>
                </div>
            </td>
        `;
        tbody.appendChild(tr);
    });
}


// Pestaña Asistencia: Poblar selector y cargar panel general
function cargarAsistenciaTab() {
    poblarDropdown('/api/rrhh/select/empleados', 'selectEmpleadoAsistencias', 'id', 'nombreCompleto', 'Seleccione un empleado para marcar...');
    
    // Cargar el panel general con la fecha seleccionada (por defecto hoy)
    const fechaInput = document.getElementById('asisGeneralFecha');
    if (fechaInput && fechaInput.value) {
        cargarAsistenciaGeneral(fechaInput.value);
    }
}

// Cargar Asistencia General (Todos los empleados)
async function cargarAsistenciaGeneral(fecha) {
    try {
        const tbody = document.getElementById('tbAsistenciaGeneral');
        if (tbody) tbody.innerHTML = `<tr><td colspan="7" class="text-center py-4 text-muted">Cargando datos de asistencia...</td></tr>`;

        const res = await fetch(`/api/rrhh/asistencias/general?fecha=${fecha}`);
        if (!res.ok) throw new Error("Error al cargar asistencia general");
        
        _asistenciaGeneralCache = await res.json();
        filtrarYRenderizarAsistenciaGeneral();
        
    } catch (err) {
        console.error("Error al cargar asistencia general:", err);
        const tbody = document.getElementById('tbAsistenciaGeneral');
        if (tbody) tbody.innerHTML = `<tr><td colspan="7" class="text-center py-4 text-danger">Error al cargar datos. Intente nuevamente.</td></tr>`;
    }
}

// Filtrar y renderizar la tabla del Panel General y sus KPIs
function filtrarYRenderizarAsistenciaGeneral() {
    let filtrados = [..._asistenciaGeneralCache];

    // 1. Filtrar por estado de marcación
    if (_asisFiltroActual !== 'todos') {
        filtrados = filtrados.filter(a => {
            const estado = a.estadoMarcacion || a.EstadoMarcacion;
            return estado && estado.toLowerCase() === _asisFiltroActual;
        });
    }

    // 2. Filtrar por texto de búsqueda
    if (_asisBusquedaActual) {
        const termino = _asisBusquedaActual;
        filtrados = filtrados.filter(a => {
            const campos = [a.empleadoNombre, a.departamentoNombre].filter(Boolean).map(c => c.toLowerCase());
            return campos.some(c => c.includes(termino));
        });
    }

    // Renderizar tabla
    const tbody = document.getElementById('tbAsistenciaGeneral');
    if (tbody) {
        tbody.innerHTML = '';
        if (filtrados.length === 0) {
            tbody.innerHTML = `<tr><td colspan="8" class="text-center py-4 text-muted">No se encontraron registros que coincidan con los filtros.</td></tr>`;
        } else {
            filtrados.forEach(a => {
                const tr = document.createElement('tr');
                const iniciales = (a.empleadoNombre.split(' ').map(n => n.charAt(0)).join('')).substring(0, 2).toUpperCase();
                
                // Formatear horas
                const hEntrada = a.horaEntrada ? new Date(a.horaEntrada).toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' }) : '-';
                const hSalida = a.horaSalida ? new Date(a.horaSalida).toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' }) : '-';
                const hrsTrabajadas = a.horasTrabajadas !== null ? a.horasTrabajadas.toFixed(2) : '-';

                // Badge Estado
                let badgeClass = '';
                let badgeIcon = '';
                let badgeText = '';

                const estado = a.estadoMarcacion || a.EstadoMarcacion;

                if (estado === 'COMPLETA') {
                    badgeClass = 'badge-completa';
                    badgeIcon = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="12" height="12"><polyline points="20 6 9 17 4 12"></polyline></svg>';
                    badgeText = 'Completa';
                } else if (estado === 'EN_CURSO') {
                    badgeClass = 'badge-encurso';
                    badgeIcon = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="12" height="12"><circle cx="12" cy="12" r="10"></circle><polyline points="12 6 12 12 16 14"></polyline></svg>';
                    badgeText = 'En Curso';
                } else if (estado === 'AUSENTE') {
                    badgeClass = 'badge-ausente bg-danger bg-opacity-10 text-danger border border-danger border-opacity-25';
                    badgeIcon = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="12" height="12"><circle cx="12" cy="12" r="10"></circle><line x1="15" y1="9" x2="9" y2="15"></line><line x1="9" y1="9" x2="15" y2="15"></line></svg>';
                    badgeText = 'Falta';
                } else {
                    badgeClass = 'badge-sinregistro';
                    badgeIcon = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="12" height="12"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>';
                    badgeText = 'Sin Registro';
                }

                const badgeHtml = `<span class="badge-marcacion ${badgeClass}" style="${estado === 'AUSENTE' ? 'padding: 4px 8px; border-radius: 12px;' : ''}">${badgeIcon} ${badgeText}</span>`;

                tr.innerHTML = `
                    <td>
                        <div class="empleado-info">
                            <div class="empleado-avatar" style="width: 32px; height: 32px; font-size: 11px;">${iniciales}</div>
                            <span class="empleado-name fw-bold ms-2">${a.empleadoNombre}</span>
                        </div>
                    </td>
                    <td>${a.departamentoNombre || 'Sin asignar'}</td>
                    <td class="font-monospace">${hEntrada}</td>
                    <td class="font-monospace">${hSalida}</td>
                    <td class="font-monospace fw-bold">${hrsTrabajadas}</td>
                    <td>${badgeHtml}</td>
                    <td><small class="text-muted" style="max-width: 200px; display: inline-block; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;" title="${a.observaciones || ''}">${a.observaciones || '-'}</small></td>
                    <td>
                        ${estado === 'EN_CURSO' 
                            ? `<button class="frm-btn-cancel text-danger frm-btn-sm" style="border-color: rgba(220,38,38,0.2);" onclick="registrarSalidaRapida(${a.empleadoId || a.EmpleadoId})">Registrar Salida</button>` 
                            : (estado === 'SIN_REGISTRO' 
                                ? `<button class="frm-btn-save frm-btn-sm" onclick="abrirModalNuevaAsistencia(${a.empleadoId || a.EmpleadoId})">Registrar Entrada</button>`
                                : `-`)}
                    </td>
                `;
                tbody.appendChild(tr);
            });
        }
    }

    // Actualizar Mini KPIs
    const total = _asistenciaGeneralCache.length;
    const completas = _asistenciaGeneralCache.filter(a => a.estadoMarcacion === 'COMPLETA').length;
    const enCurso = _asistenciaGeneralCache.filter(a => a.estadoMarcacion === 'EN_CURSO').length;
    const presentes = completas + enCurso;
    const sinRegistro = total - presentes;

    // Calcular promedio horas solo de los que tienen salida
    const conSalida = _asistenciaGeneralCache.filter(a => a.horasTrabajadas !== null);
    const sumHoras = conSalida.reduce((acc, a) => acc + a.horasTrabajadas, 0);
    const promedio = conSalida.length > 0 ? (sumHoras / conSalida.length).toFixed(1) : 0;

    const elTotal = document.getElementById('kpiAsisTotal');
    const elPresentes = document.getElementById('kpiAsisPresentes');
    const elAusentes = document.getElementById('kpiAsisAusentes');
    const elPromedio = document.getElementById('kpiAsisPromedio');

    if (elTotal) elTotal.textContent = total;
    if (elPresentes) elPresentes.textContent = presentes;
    if (elAusentes) elAusentes.textContent = sinRegistro;
    if (elPromedio) elPromedio.textContent = `${promedio}h`;
}

// Función para registrar rápida la salida
window.registrarSalidaRapida = async function(empleadoId) {
    if (!confirm('¿Desea registrar la hora de salida actual para este empleado?')) return;
    
    try {
        const res = await fetch('/api/rrhh/asistencia/salida', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ EmpleadoId: empleadoId, Observaciones: '' })
        });
        
        let data;
        const contentType = res.headers.get('content-type');
        if (contentType && contentType.includes('application/json')) {
            data = await res.json();
        } else {
            data = await res.text();
        }
        
        if (res.ok) {
            mostrarAlerta((data && data.mensaje) || 'Salida registrada correctamente.', 'success');
            const asisGeneralFecha = document.getElementById('asisGeneralFecha');
            if (asisGeneralFecha) cargarAsistenciaGeneral(asisGeneralFecha.value);
        } else {
            const errorMsg = (data && typeof data === 'object') ? (data.mensaje || data.message) : data;
            mostrarAlerta(errorMsg || 'Error al registrar salida', 'danger');
        }
    } catch (err) {
        console.error(err);
        mostrarAlerta('Error de conexión al registrar salida', 'danger');
    }
};

// Función para abrir el modal de Nueva Asistencia
window.abrirModalNuevaAsistencia = async function(empleadoId = null) {
    document.getElementById('frmNuevaAsistencia').reset();
    
    // Asignar fecha y hora actual por defecto
    const now = new Date();
    document.getElementById('asistenciaManualFecha').value = now.toISOString().split('T')[0];
    document.getElementById('asistenciaManualEntrada').value = now.toTimeString().substring(0, 5);
    
    // Cargar empleados en el select
    try {
        const res = await fetch('/api/rrhh/select/empleados');
        if (res.ok) {
            const empleados = await res.json();
            const select = document.getElementById('asistenciaSelectEmpleado');
            select.innerHTML = '<option value="">Seleccione un empleado...</option>';
            empleados.forEach(e => {
                const empId = e.id || e.Id;
                const empName = e.nombreCompleto || e.NombreCompleto || e.nombre || e.Nombre || "Desconocido";
                select.innerHTML += `<option value="${empId}">${empName}</option>`;
            });
            if (empleadoId) {
                select.value = empleadoId;
            }
        }
    } catch (e) {
        console.error("Error cargando empleados", e);
    }
    
    const modal = new bootstrap.Modal(document.getElementById('modalNuevaAsistencia'));
    modal.show();
};

// Cargar Asistencias Individuales de un Empleado
async function cargarAsistenciasPorEmpleado(empleadoId) {
    try {
        const res = await fetch(`/api/rrhh/empleados/${empleadoId}/asistencias`);
        const tbody = document.getElementById('tbAsistencias');
        if (!tbody) return;

        tbody.innerHTML = '';
        if (!res.ok) {
            tbody.innerHTML = `<tr><td colspan="5" class="text-center py-4 text-danger">Error al cargar historial.</td></tr>`;
            return;
        }

        const asistencias = await res.json();

        const hoy = new Date();
        const strHoy = hoy.toLocaleDateString('en-CA');
        let tieneEntradaHoy = false;
        let tieneSalidaHoy = false;

        if (asistencias.length === 0) {
            tbody.innerHTML = `<tr><td colspan="5" class="text-center py-4 text-muted">Sin asistencias registradas recientemente.</td></tr>`;
        } else {
            asistencias.forEach(a => {
                const tr = document.createElement('tr');
                const fecha = formatFecha(a.fecha);
                const fechaIso = a.fecha ? a.fecha.split('T')[0] : '';
                
                if (fechaIso === strHoy) {
                    tieneEntradaHoy = true;
                    if (a.horaSalida) tieneSalidaHoy = true;
                    document.getElementById('asistenciaObservaciones').value = a.observaciones || '';
                }

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

        // Actualizar estado de los botones de marcación
        const btnEntrada = document.getElementById('btnMarcarEntrada');
        const btnSalida = document.getElementById('btnMarcarSalida');
        const btnGuardarObs = document.getElementById('btnGuardarObservacion');
        const msgAsis = document.getElementById('asistenciaMsg');

        if (btnEntrada && btnSalida && msgAsis) {
            if (!tieneEntradaHoy) {
                btnEntrada.disabled = false;
                btnSalida.disabled = true;
                msgAsis.innerText = "No se ha registrado entrada hoy.";
                btnEntrada.style.opacity = '1';
                btnSalida.style.opacity = '0.5';
                if (btnGuardarObs) btnGuardarObs.style.display = 'none';
                document.getElementById('asistenciaObservaciones').value = '';
            } else if (tieneEntradaHoy && !tieneSalidaHoy) {
                btnEntrada.disabled = true;
                btnSalida.disabled = false;
                msgAsis.innerText = "Entrada registrada. Puede marcar salida o actualizar la observación.";
                btnEntrada.style.opacity = '0.5';
                btnSalida.style.opacity = '1';
                if (btnGuardarObs) btnGuardarObs.style.display = 'inline-block';
            } else {
                btnEntrada.disabled = true;
                btnSalida.disabled = true;
                msgAsis.innerText = "Asistencia completada. Puede actualizar la observación si lo requiere.";
                btnEntrada.style.opacity = '0.5';
                btnSalida.style.opacity = '0.5';
                if (btnGuardarObs) btnGuardarObs.style.display = 'inline-block';
            }
        }
    } catch (err) {
        console.error("Error al cargar asistencias individuales:", err);
    }
}

// Registrar Entrada/Salida rápida
async function registrarAsistenciaRapida(tipo) {
    const empleadoId = document.getElementById('selectEmpleadoAsistencias').value;
    const observaciones = document.getElementById('asistenciaObservaciones').value.trim();
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
            body: JSON.stringify({
                empleadoId: parseInt(empleadoId),
                observaciones: observaciones || null
            })
        });

        let data;
        const contentType = res.headers.get('content-type');
        if (contentType && contentType.includes('application/json')) {
            data = await res.json();
        } else {
            data = await res.text();
        }

        if (res.ok) {
            document.getElementById('asistenciaObservaciones').value = '';
            mostrarAlerta((data && data.mensaje) || 'Registro de asistencia exitoso.', "success");
            
            // Recargar vista individual
            cargarAsistenciasPorEmpleado(parseInt(empleadoId));
            
            // Recargar también panel general si la fecha seleccionada es hoy
            const asisGeneralFecha = document.getElementById('asisGeneralFecha');
            if (asisGeneralFecha && asisGeneralFecha.value === new Date().toLocaleDateString('en-CA')) {
                cargarAsistenciaGeneral(asisGeneralFecha.value);
            }
            
            actualizarKPIs();
        } else {
            const errorMsg = (data && typeof data === 'object') ? (data.message || data.mensaje) : data;
            mostrarAlerta(errorMsg || "Error al procesar registro de asistencia.", "danger");
        }
    } catch (err) {
        console.error("Error registrando asistencia:", err);
        mostrarAlerta("Error de conexión al marcar asistencia.", "danger");
    }
}

// Actualizar observación de forma independiente
async function actualizarObservacionAsistencia() {
    const empleadoId = document.getElementById('selectEmpleadoAsistencias').value;
    const observaciones = document.getElementById('asistenciaObservaciones').value.trim();
    if (!empleadoId) {
        mostrarAlerta("Debe seleccionar un empleado.", "warning");
        return;
    }

    try {
        const res = await fetch('/api/rrhh/asistencia/observacion', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                empleadoId: parseInt(empleadoId),
                observaciones: observaciones || null
            })
        });

        const data = await res.json();
        if (res.ok) {
            mostrarAlerta(data.mensaje, "success");
            cargarAsistenciasPorEmpleado(parseInt(empleadoId));
        } else {
            mostrarAlerta(data.message || data.mensaje || "Error al actualizar la observación.", "danger");
        }
    } catch (err) {
        console.error("Error actualizando observación:", err);
        mostrarAlerta("Error de conexión al guardar observación.", "danger");
    }
}

// Pestaña Ausencias
function cargarAusenciasTab() {
    cargarAusenciasGeneral();
}

async function cargarAusenciasGeneral() {
    try {
        const tbody = document.getElementById('tbAusenciasGeneral');
        if (tbody) tbody.innerHTML = `<tr><td colspan="7" class="text-center py-4 text-muted">Cargando datos de ausencias...</td></tr>`;

        const res = await fetch('/api/rrhh/ausencias/general');
        if (!res.ok) throw new Error("Error al cargar ausencias generales");
        
        _ausenciasGeneralCache = await res.json();
        filtrarYRenderizarAusenciasGeneral();
    } catch (err) {
        console.error("Error al cargar ausencias generales:", err);
        const tbody = document.getElementById('tbAusenciasGeneral');
        if (tbody) tbody.innerHTML = `<tr><td colspan="7" class="text-center py-4 text-danger">Error al cargar datos. Intente nuevamente.</td></tr>`;
    }
}

function filtrarYRenderizarAusenciasGeneral() {
    let filtrados = [..._ausenciasGeneralCache];

    if (_ausenciasFiltroActual !== 'todas') {
        filtrados = filtrados.filter(a => a.estado && a.estado.toLowerCase() === _ausenciasFiltroActual);
    }

    if (_ausenciasBusquedaActual) {
        const termino = _ausenciasBusquedaActual;
        filtrados = filtrados.filter(a => {
            const campos = [a.empleadoNombre, a.departamentoNombre, a.tipoAusenciaNombre].filter(Boolean).map(x => x.toLowerCase());
            return campos.some(x => x.includes(termino));
        });
    }

    const tbody = document.getElementById('tbAusenciasGeneral');
    if (!tbody) return;

    tbody.innerHTML = '';
    if (filtrados.length === 0) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center py-4 text-muted">No se encontraron ausencias con estos filtros.</td></tr>`;
        return;
    }

    filtrados.forEach(a => {
        const tr = document.createElement('tr');
        const iniciales = a.empleadoNombre ? (a.empleadoNombre.split(' ').map(n => n.charAt(0)).join('')).substring(0, 2).toUpperCase() : 'EMP';
        const fInicio = formatFecha(a.fechaInicio);
        const fFin = formatFecha(a.fechaFin);
        
        let badgeHtml = '';
        if (a.estado === 'APROBADA') badgeHtml = `<span class="badge-marcacion badge-completa">Aprobada</span>`;
        else if (a.estado === 'PENDIENTE') badgeHtml = `<span class="badge-marcacion badge-encurso">Pendiente</span>`;
        else badgeHtml = `<span class="badge-marcacion" style="color:#ef4444; border: 1px solid rgba(239,68,68,0.2); background:rgba(239,68,68,0.1)">Rechazada</span>`;

        tr.innerHTML = `
            <td>
                <div class="empleado-info">
                    <div class="empleado-avatar" style="width: 32px; height: 32px; font-size: 11px;">${iniciales}</div>
                    <div class="d-flex flex-column ms-2">
                        <span class="empleado-name fw-bold">${a.empleadoNombre}</span>
                        <span class="text-muted" style="font-size: 11px;">${a.departamentoNombre || 'Sin departamento'}</span>
                    </div>
                </div>
            </td>
            <td class="fw-bold text-secondary">${a.tipoAusenciaNombre}</td>
            <td class="font-monospace">${fInicio}</td>
            <td class="font-monospace">${fFin}</td>
            <td class="font-monospace fw-bold">${a.diasSolicitados}</td>
            <td>${badgeHtml}</td>
            <td><small class="text-muted" style="max-width: 150px; display: inline-block; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;" title="${a.motivo || ''}">${a.motivo || '-'}</small></td>
            <td>
                <div class="d-flex gap-2">
                    ${a.estado === 'PENDIENTE' ? `
                    <button class="btn btn-sm btn-outline-primary border-0" onclick="abrirModalResolverAusencia(${a.id})" title="Resolver/Editar Ausencia">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="16" height="16"><path d="M12 20h9"></path><path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"></path></svg>
                    </button>
                    ` : ''}
                    <button class="btn btn-sm btn-outline-danger border-0" onclick="eliminarAusencia(${a.id})" title="Eliminar Ausencia">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="16" height="16"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path><line x1="10" y1="11" x2="10" y2="17"></line><line x1="14" y1="11" x2="14" y2="17"></line></svg>
                    </button>
                </div>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

// (Removido: cargarAusenciasPorEmpleado ya no es necesario)


// Abrir modal para resolver solicitud
function abrirModalResolverAusencia(ausenciaId) {
    document.getElementById('resolverAusenciaId').value = ausenciaId;
    document.getElementById('resolverDecision').value = '';

    const btn = document.getElementById('btnConfirmarResolucion');
    btn.className = 'frm-btn-save';
    btn.innerText = 'Confirmar Resolución';

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
        document.getElementById('detNombreCompleto').innerText = emp.nombreCompleto || '-';
        document.getElementById('detCargo').innerText = emp.cargoNombre || 'Sin asignar';
        document.getElementById('detDocumento').innerText = `${emp.tipoDocumento || '-'} - ${emp.numeroDocumento || '-'}`;

        document.getElementById('detFechaNacimiento').innerText = emp.fechaNacimiento ? formatFecha(emp.fechaNacimiento) : '-';
        document.getElementById('detGenero').innerText = emp.genero || '-';
        document.getElementById('detEstadoCivil').innerText = emp.estadoCivil || '-';
        document.getElementById('detTelefono').innerText = emp.telefono || '-';
        document.getElementById('detCelular').innerText = emp.celular || '-';
        document.getElementById('detCorreoPersonal').innerText = emp.correoPersonal || '-';
        document.getElementById('detCorreoEmpresa').innerText = emp.correoEmpresa || '-';
        document.getElementById('detDireccion').innerText = emp.direccion || '-';

        document.getElementById('detDepartamento').innerText = emp.departamentoNombre || 'Sin asignar';
        document.getElementById('detCargoDetalle').innerText = emp.cargoNombre || 'Sin asignar';
        document.getElementById('detResponsable').innerText = emp.responsableNombre || 'Ninguno';
        document.getElementById('detUsuarioId').innerText = emp.usuarioNombre || (emp.usuarioId ? `ID: ${emp.usuarioId}` : 'Ninguno');

        document.getElementById('detFechaIngreso').innerText = emp.fechaIngreso ? formatFecha(emp.fechaIngreso) : '-';
        document.getElementById('detFechaCese').innerText = emp.fechaCese ? formatFecha(emp.fechaCese) : '-';
        document.getElementById('detTipoContrato').innerText = emp.tipoContrato || '-';
        document.getElementById('detRegimenLaboral').innerText = emp.regimenLaboral || 'No asignado';

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

// Editar empleado: Cargar datos en el modal
async function editarEmpleado(id) {
    try {
        const res = await fetch(`/api/rrhh/empleados/${id}`);
        if (!res.ok) {
            mostrarAlerta("No se pudo obtener la información del empleado.", "danger");
            return;
        }

        const emp = await res.json();

        // Asegurarse de que los combos están poblados
        if (document.getElementById('empDepto').options.length <= 1) {
            await poblarDropdown('/api/rrhh/select/departamentos', 'empDepto', 'id', 'nombre', 'Seleccione departamento...');
            await poblarDropdown('/api/rrhh/select/cargos', 'empCargo', 'id', 'nombre', 'Seleccione cargo...');
            await poblarDropdown('/api/rrhh/select/usuarios', 'empUsuario', 'id', 'nombreCompleto', 'Ninguno');
            await poblarDropdown('/api/rrhh/select/empleados', 'empResponsable', 'id', 'nombreCompleto', 'Ninguno');
        }

        // Poblar formulario
        document.getElementById('empId').value = emp.id;
        document.getElementById('empNombres').value = emp.nombres || '';
        document.getElementById('empApellidos').value = emp.apellidos || '';
        document.getElementById('empTipoDoc').value = emp.tipoDocumento || 'DNI';
        document.getElementById('empNumDoc').value = emp.numeroDocumento || '';

        document.getElementById('empFechaNacimiento').value = emp.fechaNacimiento ? emp.fechaNacimiento.split('T')[0] : '';
        document.getElementById('empGenero').value = emp.genero || '';
        document.getElementById('empEstadoCivil').value = emp.estadoCivil || '';
        document.getElementById('empTelefono').value = emp.telefono || '';
        document.getElementById('empCelular').value = emp.celular || '';
        document.getElementById('empCorreoPersonal').value = emp.correoPersonal || '';
        document.getElementById('empCorreoEmpresa').value = emp.correoEmpresa || '';
        document.getElementById('empDireccion').value = emp.direccion || '';

        document.getElementById('empDepto').value = emp.departamentoId || '';
        document.getElementById('empCargo').value = emp.cargoId || '';
        document.getElementById('empResponsable').value = emp.responsableId || '';
        document.getElementById('empUsuario').value = emp.usuarioId || '';

        document.getElementById('empFechaIngreso').value = emp.fechaIngreso ? emp.fechaIngreso.split('T')[0] : '';
        document.getElementById('empFechaCese').value = emp.fechaCese ? emp.fechaCese.split('T')[0] : '';
        document.getElementById('empTipoContrato').value = emp.tipoContrato || 'FIJO';
        document.getElementById('empRegimenLaboral').value = emp.regimenLaboral || 'General';

        document.getElementById('empActivo').checked = emp.activo;

        document.getElementById('modalNuevoEmpleadoLabel').innerText = 'Editar Empleado';

        const modalEl = document.getElementById('modalNuevoEmpleado');
        const modal = new bootstrap.Modal(modalEl);
        modal.show();

    } catch (err) {
        console.error("Error al cargar empleado para editar:", err);
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
        const res = await fetch('/api/rrhh/dashboard-kpis');
        if (!res.ok) return;
        const kpis = await res.json();

        const setVal = (id, val) => {
            const el = document.getElementById(id);
            if (el) el.innerText = val || 0;
        };

        setVal('kpiEmpleados', kpis.empleadosActivos);
        setVal('kpiContratos', kpis.contratosActivos);
        setVal('kpiAsistencias', kpis.asistenciasHoy);
        setVal('kpiAusencias', kpis.ausenciasPendientes);
        setVal('kpiDepartamentos', kpis.departamentos);
        setVal('kpiCargos', kpis.cargos);

    } catch (err) {
        console.error("Error al actualizar KPIs generales:", err);
    }
}

// Abrir modal de departamento en modo edición
async function editarDepartamento(id, nombre, responsableId) {
    // Primero poblar el dropdown, luego asignar valores
    await poblarDropdown('/api/rrhh/select/usuarios-todos', 'deptResponsable', 'id', 'nombreCompleto', 'Sin responsable');

    document.getElementById('deptId').value = id;
    document.getElementById('deptNombre').value = nombre;
    document.getElementById('modalNuevoDepartamentoLabel').innerText = 'Editar Departamento';

    if (responsableId) {
        document.getElementById('deptResponsable').value = responsableId;
    }

    // Usar getOrCreateInstance para no disparar show.bs.modal dos veces
    const modalEl = document.getElementById('modalNuevoDepartamento');
    bootstrap.Modal.getOrCreateInstance(modalEl).show();
}

// Eliminar departamento (baja lógica)
async function eliminarDepartamento(id, nombre) {
    if (!confirm(`¿Está seguro de eliminar el departamento "${nombre}"?\nNo podrá eliminarlo si tiene empleados activos.`)) return;

    try {
        const res = await fetch(`/api/rrhh/departamentos/${id}`, { method: 'DELETE' });
        if (res.ok) {
            mostrarAlerta('Departamento eliminado correctamente.', 'success');
            cargarDepartamentos();
            actualizarKPIs();
        } else {
            const err = await res.text();
            mostrarAlerta(`Error: ${err}`, 'danger');
        }
    } catch (err) {
        console.error('Error al eliminar departamento:', err);
        mostrarAlerta('Error de conexión al eliminar.', 'danger');
    }
}


// Abrir modal de cargo en modo edición
function editarCargo(id, nombre, descripcion) {
    document.getElementById('cargoId').value = id;
    document.getElementById('cargoNombre').value = nombre;
    document.getElementById('cargoDescripcion').value = descripcion;
    document.getElementById('modalNuevoCargoLabel').innerText = 'Editar Cargo';
    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalNuevoCargo')).show();
}

// Eliminar cargo (baja lógica)
async function eliminarCargo(id, nombre) {
    if (!confirm(`¿Está seguro de eliminar el cargo "${nombre}"?\nNo podrá eliminarlo si tiene empleados activos asignados.`)) return;

    try {
        const res = await fetch(`/api/rrhh/cargos/${id}`, { method: 'DELETE' });
        if (res.ok) {
            mostrarAlerta('Cargo eliminado correctamente.', 'success');
            cargarCargos();
            actualizarKPIs();
        } else {
            const err = await res.text();
            mostrarAlerta(`Error: ${err}`, 'danger');
        }
    } catch (err) {
        console.error('Error al eliminar cargo:', err);
        mostrarAlerta('Error de conexión al eliminar.', 'danger');
    }
}


// Abrir modal de contrato en modo edición
async function editarContrato(id) {
    try {
        // Fetch directo por ID, sin depender del select
        const res = await fetch(`/api/rrhh/contratos/${id}`);
        if (!res.ok) {
            mostrarAlerta('No se encontró el contrato.', 'danger');
            return;
        }
        const c = await res.json();

        await poblarDropdown('/api/rrhh/select/empleados', 'contratoEmpleado', 'id', 'nombreCompleto', 'Seleccione empleado...');
        await poblarDropdown('/api/rrhh/select/monedas', 'contratoMoneda', 'id', 'nombre', 'Seleccione moneda...');

        document.getElementById('contratoId').value = c.id;
        document.getElementById('contratoEmpleado').value = c.empleadoId;
        document.getElementById('contratoNombre').value = c.nombre;
        document.getElementById('contratoFechaInicio').value = c.fechaInicio.split('T')[0];
        document.getElementById('contratoFechaFin').value = c.fechaFin ? c.fechaFin.split('T')[0] : '';
        document.getElementById('contratoSueldo').value = c.sueldo;
        document.getElementById('contratoMoneda').value = c.monedaId;
        document.getElementById('contratoTipo').value = c.tipoContrato;
        document.getElementById('contratoEstado').value = c.estado;

        document.getElementById('contratoEstadoWrap').style.display = 'block';
        document.getElementById('modalNuevoContratoLabel').innerText = 'Editar Contrato';

        bootstrap.Modal.getOrCreateInstance(document.getElementById('modalNuevoContrato')).show();
    } catch (err) {
        console.error('Error al cargar contrato para editar:', err);
        mostrarAlerta('Error al cargar el contrato.', 'danger');
    }
}

// Eliminar contrato (solo CERRADOS)
async function eliminarContrato(id) {
    if (!confirm('¿Está seguro de eliminar este contrato? Esta acción es permanente.')) return;

    try {
        const res = await fetch(`/api/rrhh/contratos/${id}`, { method: 'DELETE' });
        if (res.ok) {
            mostrarAlerta('Contrato eliminado correctamente.', 'success');
            cargarContratosGeneral(); // Reload the general view
            actualizarKPIs();
        } else {
            const err = await res.text();
            mostrarAlerta(`Error: ${err}`, 'danger');
        }
    } catch (err) {
        console.error('Error al eliminar contrato:', err);
        mostrarAlerta('Error de conexión al eliminar.', 'danger');
    }
}

async function eliminarAusencia(id) {
    if (!confirm('¿Está seguro de eliminar esta ausencia? Esta acción es permanente.')) return;

    try {
        const res = await fetch(`/api/rrhh/ausencias/${id}`, { method: 'DELETE' });
        if (res.ok) {
            mostrarAlerta('Ausencia eliminada correctamente.', 'success');
            cargarAusenciasGeneral(); // Reload the general view
            actualizarKPIs();
        } else {
            const err = await res.text();
            mostrarAlerta(`Error: ${err}`, 'danger');
        }
    } catch (err) {
        console.error('Error al eliminar ausencia:', err);
        mostrarAlerta('Error de conexión al eliminar.', 'danger');
    }
}
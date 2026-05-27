# Grupo 05 — Módulo de Ventas y Reportes
## Sistema ERP Académico

---

## Índice
1. [Contexto General](#contexto-general)
2. [Tecnologías y Restricciones](#tecnologías-y-restricciones)
3. [Responsabilidades del Grupo](#responsabilidades-del-grupo)
4. [Estructura de Archivos](#estructura-de-archivos)
5. [Convenciones de Desarrollo](#convenciones-de-desarrollo)
6. [Cronograma](#cronograma)
7. [Documentación por Módulo](#documentación-por-módulo)

---

## Contexto General

Sistema ERP distribuido entre grupos de trabajo. Cada grupo es responsable de un conjunto
de módulos. Esta etapa corresponde a mockups funcionales con datos simulados, sin base de datos.

---

## Tecnologías y Restricciones

| Elemento          | Detalle                                      |
|-------------------|----------------------------------------------|
| Backend           | C#                                           |
| Frontend          | HTML, CSS, Bootstrap                         |
| Sin frameworks JS | No React, no Vue, no Angular                 |
| Sin CSS externo   | Solo Bootstrap como framework CSS            |
| Sin base de datos | Datos simulados en Controller/Model          |
| Repositorio base  | Layout general provisto, cada grupo extiende |

---

## Responsabilidades del Grupo

| Módulo    | Sub-módulo        | Estado     | Prioridad |
|-----------|-------------------|------------|-----------|
| Ventas    | Clientes          | En curso   | 1         |
| Ventas    | Cotizaciones      | Pendiente  | 2         |
| Ventas    | Órdenes           | Pendiente  | 3         |
| Reportes  | Dashboard         | Pendiente  | 4         |
| ~~Ventas~~| ~~Facturas~~      | Excluido   | —         |

---

## Estructura de Archivos
/grupo5/
│
├── README.md                         ← Este documento
├── docs/
│   ├── MODULO_VENTAS.md              ← Planificación módulo Ventas
│   ├── SUBMODULO_CLIENTES.md         ← Planificación detallada Clientes
│   ├── SUBMODULO_COTIZACIONES.md     ← Planificación detallada Cotizaciones
│   ├── SUBMODULO_ORDENES.md          ← Planificación detallada Órdenes
│   └── MODULO_REPORTES.md            ← Planificación módulo Reportes
│
├── ventas/
│   ├── clientes/
│   │   ├── ClienteModel.cs
│   │   ├── ClientesController.cs
│   │   ├── Index.html
│   │   ├── Detalle.html
│   │   └── Formulario.html
│   ├── cotizaciones/
│   │   ├── CotizacionModel.cs
│   │   ├── CotizacionesController.cs
│   │   ├── Index.html
│   │   ├── Detalle.html
│   │   └── Formulario.html
│   └── ordenes/
│       ├── OrdenModel.cs
│       ├── OrdenesController.cs
│       ├── Index.html
│       ├── Detalle.html
│       └── Formulario.html
│
└── reportes/
├── ReportesController.cs
└── Dashboard.html

---

## Convenciones de Desarrollo

### Nomenclatura de archivos
- Clases C#: `PascalCase` → `ClienteModel.cs`, `ClientesController.cs`
- Vistas HTML: `PascalCase` → `Index.html`, `Detalle.html`
- Documentación: `MAYUSCULAS_GUION_BAJO.md`

### Nomenclatura de datos simulados
- Arrays estáticos definidos en el Controller.
- IDs numéricos autoincrementales desde 1.
- Códigos con prefijo por entidad: `CLI-001`, `COT-001`, `ORD-001`.

### Reglas generales
- Respetar clases CSS globales definidas en `modulos.md`.
- No duplicar estilos ya definidos en el layout base.
- Toda navegación simulada mediante `href` entre vistas.
- Formularios con validación HTML5 básica (`required`, `type`, `pattern`).

---

## Cronograma

| Semana | Actividad                                         |
|--------|---------------------------------------------------|
| 1      | Planificación y documentación (docs/)             |
| 2      | Mockup: Sub-módulo Clientes (completo)            |
| 3      | Mockup: Sub-módulo Cotizaciones (completo)        |
| 4      | Mockup: Sub-módulo Órdenes + flujo entre módulos  |
| 5      | Mockup: Módulo Reportes                           |
| 6      | Revisión general, integración y ajustes finales   |

---

## Documentación por Módulo

- [Módulo Ventas](./docs/MODULO_VENTAS.md)
- [Sub-módulo Clientes](./docs/SUBMODULO_CLIENTES.md)
- [Sub-módulo Cotizaciones](./docs/SUBMODULO_COTIZACIONES.md)
- [Sub-módulo Órdenes](./docs/SUBMODULO_ORDENES.md)
- [Módulo Reportes](./docs/MODULO_REPORTES.md)
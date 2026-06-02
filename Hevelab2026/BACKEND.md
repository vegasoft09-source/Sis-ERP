# HeveLab ERP — Backend

Arquitectura **ASP.NET Core 8** con capas: Controllers → Services → Repositories → EF Core.

## Configuración

| Clave | Descripción |
|-------|-------------|
| `DatabaseProvider` | `MySql` (producción/local) o `InMemory` (sin BD) |
| `ConnectionStrings:DefaultConnection` | `localhost:3306` → `u340197236_SIS_ERP` |
| `Jwt:*` | Autenticación API |

**Usuario demo:** `admin` / `Admin123!`

## API REST (`/api/v1`)

| Módulo | Rutas |
|--------|--------|
| Auth | `POST /api/v1/Auth/login`, `refresh`, `logout` |
| Clientes | `GET/POST/PUT/DELETE /api/v1/Clientes` |
| Productos | `GET/POST/PUT /api/v1/Productos` |
| Cotizaciones | `GET/POST /api/v1/Cotizaciones`, `POST .../convertir-orden` |
| Compras | `GET /api/v1/Compras/solicitudes`, `ordenes`, `proveedores` |
| Inventario | `GET/POST /api/v1/Inventario/recepciones` |

Swagger: `https://localhost:7087/swagger` (desarrollo).

## MVC (frontend existente)

Los controladores MVC usan los mismos **Services** (persistencia real, no listas estáticas):

- Clientes, Producto, Cotizaciones, Ordenes
- SolicitudDeCotizacion, OrdenesDeCompra, Proveedores
- RecepcionesView

## Modelo de datos

Entidades alineadas a `documentacion.html` (66 tablas). Implementadas en esta fase:

- Config: Empresa, Moneda, Rol, Permiso, Usuario
- Socio (clientes/proveedores)
- Productos, Almacén, Stock
- Ventas: PedidoVenta, DetallePedidoVenta (cotizaciones)
- Compras: SolicitudCotizacion, PedidoCompra
- Inventario: Transferencia (recepciones), MovimientoStock

## Próximas fases (contabilidad, CRM, HR, POS)

Misma plantilla: Entity → Repository → Service → `Controllers/Api` + MVC.

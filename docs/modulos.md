# Cómo agregar un módulo nuevo a HeveLab

> **Stack:** ASP.NET Core 8 MVC · C# · HTML · CSS vanilla · JS vanilla  
> Sin React, sin Tailwind. **Bootstrap nativo** está disponible para layouts, rejillas (grids) y componentes preconstruidos.

---

## Estructura del proyecto

```
Hevelab2026/
├── Controllers/          ← Lógica C# de cada módulo
├── Models/               ← Clases de datos C#
├── Views/
│   ├── Shared/
│   │   └── _Layout.cshtml   ← Layout global (sidebar + topbar)
│   └── {Modulo}/         ← Vistas del módulo
│       ├── Index.cshtml
│       ├── Create.cshtml
│       └── ...
└── wwwroot/
    ├── css/site.css      ← Estilos globales (NO tocar sin coordinar)
    ├── js/site.js        ← JS global del shell (NO tocar sin coordinar)
    └── img/              ← Imágenes estáticas
```

---

## Pasos para crear un módulo

### 1 · Crear el Model (C#)

Archivo: `Models/Venta.cs`

```csharp
namespace Hevelab2026.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime Fecha { get; set; }
    }
}
```

---

### 2 · Crear el Controller (C#)

Archivo: `Controllers/VentasController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Hevelab2026.Models;

namespace Hevelab2026.Controllers
{
    public class VentasController : Controller
    {
        public IActionResult Index()
        {
            // Aquí irá la consulta a BD en el futuro
            ViewData["Title"] = "Ventas";
            return View();
        }
    }
}
```

> **Regla de nombre:** si el controller se llama `VentasController`,
> la URL será `/Ventas` y las vistas deben estar en `Views/Ventas/`.

---

### 3 · Crear la carpeta y vista (HTML + Razor)

Archivo: `Views/Ventas/Index.cshtml`

```html
@{
    ViewData["Title"] = "Ventas";
    // Opcional: breadcrumbs
    ViewData["Breadcrumbs"] = new List<(string Label, string Url)>
    {
        ("Dashboard", "/"),
        ("Ventas", "/Ventas")
    };
}

<h2>Ventas</h2>
<p>Contenido del módulo de ventas.</p>
```

> El `_Layout.cshtml` ya se aplica automáticamente.  
> No hay que incluir `<html>`, `<head>`, sidebar ni topbar — solo el contenido.

---

### 4 · Agregar el enlace al sidebar

Abre `Views/Shared/_Layout.cshtml` y busca el bloque `<!-- MAIN section label -->`.  
Agrega el nuevo enlace **antes** del bloque de AJUSTES:

```html
<!-- Ventas -->
<a href="/Ventas" class="hl-nav-link" aria-label="Ventas">
    <svg class="hl-icon" viewBox="0 0 24 24">
        <circle cx="9" cy="21" r="1"/>
        <circle cx="20" cy="21" r="1"/>
        <path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6"/>
    </svg>
    <span class="hl-nav-label">Ventas</span>
 </a>
```

> Los SVG son iconos inline — sin dependencias externas.  
> Busca iconos en [lucide.dev](https://lucide.dev) (copiar como SVG).

---

## Clases CSS disponibles

Las clases del design system están en `wwwroot/css/site.css`.

| Clase | Uso |
|---|---|
| `hl-nav-link` | Enlace del sidebar (aplica activo automáticamente via JS) |
| `hl-nav-label` | Texto del enlace (se oculta cuando el sidebar está colapsado) |
| `hl-icon` | Icono SVG 16×16 |
| `hl-icon-lg` | Icono SVG 20×20 |
| `truncate` | Corta texto con `…` |
| `sr-only` | Oculto visualmente, visible para lectores de pantalla |

### Variables de color (usar en CSS propio)

```css
/* En tu .cshtml.css o en un <style> dentro del .cshtml */
.mi-elemento {
    color: var(--sys-primary);        /* color primario de la paleta */
    background: var(--sys-soft);      /* fondo suave de la paleta */
    border-color: var(--sys-border);
    background: var(--bg-app);        /* fondo general de la app */
    color: var(--text-primary);       /* texto principal */
    color: var(--text-secondary);     /* texto secundario */
    color: var(--text-muted);         /* texto tenue */
    border: 1px solid var(--border-color);
}
```

> Estas variables cambian automáticamente con el modo oscuro y la paleta de colores.

---

## Estilos CSS por vista (opcionales)

ASP.NET permite CSS scoped por vista. Crea un archivo con el mismo nombre:

```
Views/Ventas/Index.cshtml       ← tu vista
Views/Ventas/Index.cshtml.css   ← CSS solo para esa vista (opcional)
```

El CSS scoped se incluye automáticamente y no afecta otras vistas.

---

## Breadcrumbs (opcional)

Para mostrar la ruta en el topbar, pon esto en cualquier view:

```csharp
@{
    ViewData["Breadcrumbs"] = new List<(string Label, string Url)>
    {
        ("Dashboard", "/"),
        ("Ventas", "/Ventas"),
        ("Nueva venta", "/Ventas/Create")
    };
}
```

Si no se define `Breadcrumbs`, el topbar muestra solo el `ViewData["Title"]`.

---

## Reglas generales

- ✅ Usar las variables CSS `--sys-*`, `--bg-*`, `--text-*` en lugar de colores hardcodeados
- ✅ Los íconos SVG van inline (sin librería externa)
- ✅ JS propio del módulo va en `wwwroot/js/{modulo}.js` y se incluye con `@section Scripts`
- ❌ No modificar `site.css` ni `site.js` para lógica específica de un módulo
- ✅ Sí puedes usar las clases de utilidad y componentes nativos de **Bootstrap** para acelerar tu desarrollo y optimizar el rendimiento.
- ❌ No usar React ni otros frameworks de JS del lado del cliente.

```html
@* Incluir JS propio del módulo al final de la vista *@
@section Scripts {
    <script src="~/js/ventas.js" asp-append-version="true"></script>
}
```

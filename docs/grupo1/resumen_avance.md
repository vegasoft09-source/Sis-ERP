# Resumen de Avance del Proyecto - HeveLab 2026

Este documento detalla de manera cronológica el avance del desarrollo, las implementaciones realizadas y la reestructuración arquitectónica llevada a cabo hasta el momento en el sistema ERP **HeveLab 2026**.

---

## 1. Migración de React a ASP.NET Core 8 MVC (Limpieza del Stack)
* **Objetivo**: Reemplazar un frontend pesado del lado del cliente en React por una arquitectura de servidor ultra-rápida, limpia y fácil de expandir usando C# y Razor.
* **Acción**:
  * Eliminación de scripts de Vite/PostCSS, archivos innecesarios de configuración y dependencias de frontend en React.
  * Establecimiento del estándar limpio **ASP.NET Core 8 MVC**.
  * Preparación de los directorios de base (`Controllers`, `Models`, `Views`, `wwwroot`) para el desarrollo desacoplado y modular de la aplicación.

---

## 2. Maquetación del Shell e Interfaz de Usuario (Base Global)
* **Objetivo**: Crear una base visual responsiva y elegante para toda la aplicación ERP.
* **Acción**:
  * **Estructura base (`_Layout.cshtml`)**: Sidebar izquierdo para navegación principal, Topbar superior con reloj en tiempo real, notificaciones y menú flotante de usuario.
  * **Sistema de Temas (`site.css`)**: Implementación de *Design Tokens* para cambiar de paleta de colores dinámicamente y soporte nativo para **Modo Oscuro**.
  * **Dinámica visual (`site.js`)**: Lógica pura (Vanilla JS) para abrir/cerrar menús responsivos en móviles, colapso de la barra lateral en escritorio y persistencia de estados de usuario con `localStorage`.

---

## 3. Sistema de Documentación y Trabajo en Grupo
* **Objetivo**: Facilitar la integración de equipos de desarrollo y la subida de sus respectivas tareas.
* **Acción**:
  * **Guía de Desarrollo (`docs/modulos.md`)**: Tutorial completo sobre cómo añadir un módulo nuevo al ERP (modelos, controladores, vistas responsivas, SVG inline, CSS específico por vista, etc.).
  * **Carpetas de Grupo (`docs/grupo1/` a `docs/grupo6/`)**: Estructura de directorios lista y registrada bajo control de versiones con marcadores `.md` autodescriptivos para cada equipo.
  * **Índice general (`docs/README.md`)**: Página de bienvenida e índice que enlaza ordenadamente todas las guías y las carpetas de los grupos.

---

## 4. Control de Versiones y Publicación en GitHub
* **Objetivo**: Publicar los avances de forma segura y consistente en la plataforma de control de versiones.
* **Acción**:
  * Creación de un archivo `.gitignore` optimizado para entornos .NET Core y desarrollo web moderno.
  * Inicialización de Git local en la rama `main`.
  * Enlace al repositorio remoto oficial: `https://github.com/vegasoft09-source/Sis-ERP.git`.
  * Ejecución exitosa del primer push publicando el proyecto.

---

## 5. Solución de Bugs de Disposición (Layout)
* **Objetivo**: Pulir detalles de diseño para alcanzar un acabado profesional e impecable.
* **Acción**:
  * **Navbar desplazado**: Eliminación de espaciado duplicado (`margin-left`) en escritorio al convertir el sidebar en in-flow (`sticky`), logrando una perfecta conjunción con el contenido principal vía flexbox.
  * **Botón recortado**: Corrección del desbordamiento del sidebar a `overflow: visible`, permitiendo que el botón circular de colapsado flote libre de recortes en el borde del menú lateral.

---

## 6. Integración de Bootstrap Nativo (Rendimiento)
* **Objetivo**: Facilitar maquetaciones adaptativas veloces y de alto rendimiento de renderizado.
* **Acción**:
  * Enlace directo a los recursos locales de **Bootstrap 5.3** a nivel shell.
  * Actualización de guías técnicas recomendando su uso para grids (`row`, `col-md`), alineaciones y espaciados responsivos.

---

## 7. Implementación de Dashboard Interactivo (MVC Completo)
* **Objetivo**: Crear una página de bienvenida rica en información y totalmente funcional utilizando el patrón de diseño MVC.
* **Acción**:
  * **Model (`Models/DashboardViewModel.cs`)**: Creado de forma fuertemente tipada para organizar la data del dashboard.
  * **Controller (`HomeController.cs`)**: Poblado con datos realistas (mock) para representar el ecosistema del ERP (métricas de ventas, clientes, existencias, alertas, transacciones recientes y auditorías de actividad).
  * **View (`Views/Home/Index.cshtml`)**: Diseño interactivo premium estilo SaaS que despliega tarjetas de rendimiento, accesos rápidos con micro-interacciones a los 6 módulos principales, tabla detallada de transacciones y timeline histórico.

---

## 🛠️ Tecnologías y Librerías Utilizadas

* **Backend**: ASP.NET Core 8.0 MVC (C# / Razor Pages).
* **Maquetación**: Bootstrap 5.3 (Nativo y Local).
* **Estilos**: CSS Vanilla y HSL CSS Variables (Soporte nativo Dark Mode).
* **Lógica Cliente**: Vanilla JS (0 frameworks de cliente para máxima velocidad).
* **Vectores**: SVG Inline (Máxima velocidad de renderizado, sin librerías pesadas de fuentes).
* **Base de Código**: Versionado y alojado en GitHub.

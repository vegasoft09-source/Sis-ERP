using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers;

public class OrdenesController : Controller
{
    public IActionResult Index(string? fecha, string? busqueda, string? estado) =>
        RedirectToAction("Index", "Cotizaciones", new { fecha, cliente = busqueda, estado });
}

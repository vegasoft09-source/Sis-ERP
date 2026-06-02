using Hevelab2026.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers;

public class LibroMayorController : Controller
{
    public IActionResult Index() =>
        View(new LibroMayorViewModel { Cuentas = new List<CuentaMayor>(), Movimientos = new List<MovimientoMayor>() });
}

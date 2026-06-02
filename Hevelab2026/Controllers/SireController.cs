using Hevelab2026.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers;

public class SireController : Controller
{
    public IActionResult Index() =>
        View(new SireViewModel { VentasRV = new List<VentaSire>(), ComprasRC = new List<CompraSire>() });
}

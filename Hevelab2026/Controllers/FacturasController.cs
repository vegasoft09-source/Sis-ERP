using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Hevelab2026.Models;

namespace Hevelab2026.Controllers
{
    public class FacturasController : Controller
    {
        public IActionResult Index() => View(new List<Factura>());
    }
}

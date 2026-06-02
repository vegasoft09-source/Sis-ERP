using Hevelab2026.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers;

public class DocumentosController : Controller
{
    public IActionResult Index() => View(new List<DocumentoContable>());
}

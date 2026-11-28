using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GYMELITEGiTHub.Controllers
{
    [Authorize(Roles = RolesSistema.Todos)]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Página controlada para excepciones no previstas. El detalle técnico queda en el registro.
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Página para códigos de estado sin contenido (404, 403, 405...).
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("Home/Estado/{codigo:int}")]
        public IActionResult Estado(int codigo)
        {
            ViewData["Codigo"] = codigo;
            return View();
        }

        // Prueba controlada del manejo de excepciones (solo Administrador y con
        // Diagnostico:HabilitarPruebaError = true). Sirve para la demostración y el manual técnico.
        [Authorize(Roles = RolesSistema.Administrador)]
        public IActionResult PruebaError([FromServices] IConfiguration configuracion)
        {
            if (!configuracion.GetValue<bool>("Diagnostico:HabilitarPruebaError"))
                return NotFound();

            _logger.LogWarning("Se solicitó la prueba controlada de error desde MVC.");
            throw new InvalidOperationException("Excepción de prueba generada desde /Home/PruebaError.");
        }
    }
}

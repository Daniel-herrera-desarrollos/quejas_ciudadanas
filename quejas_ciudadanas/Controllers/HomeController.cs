using Microsoft.AspNetCore.Mvc;
using quejas_ciudadanas.Data;
using quejas_ciudadanas.Models;
using System.Diagnostics;
using quejas_ciudadanas.Filtros;

namespace quejas_ciudadanas.Controllers
{
	//[VerificaSesion]
	public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly QuejasCiudadanasContext _context;
		public HomeController(ILogger<HomeController> logger, QuejasCiudadanasContext context)
        {
            _logger = logger;
			_context = context;
		}

        public IActionResult Index()
        {
			ViewBag.OcultarMenu = true;
			ViewBag.TiposIdentificacion = _context.Tipo_identificacion.ToList();
			ViewBag.Sexos = _context.Sexo.ToList();
			return View();
        }
		[HttpPost]
		public async Task<IActionResult> inicio_secion(inicia_secion model)
		{
			Usuario_Logueado usuario = await _context.ValidarLoginAsync( model);

			if (usuario != null)
			{
				// Aquí creas sesión o cookies si deseas
				HttpContext.Session.SetInt32("Id_user", usuario.Id_user);
				HttpContext.Session.SetInt32("Id_tipo_user", usuario.Id_tipo_user);
				HttpContext.Session.SetString("TipoUsuario", usuario.TipoUsuario ?? "");
				HttpContext.Session.SetString("TipoIdentificacion", usuario.TipoIdentificacion ?? "");
				HttpContext.Session.SetString("Identificacion", usuario.Identificacion ?? "");
				HttpContext.Session.SetString("Nombre", usuario.Nombre ?? "");
				HttpContext.Session.SetString("Apellido", usuario.Apellido ?? "");
				HttpContext.Session.SetString("Correo", usuario.Correo ?? "");
				HttpContext.Session.SetString("Genero", usuario.Genero ?? "");
				// La fecha debes convertirla a string para guardarla
				HttpContext.Session.SetString("FechaNacimiento", usuario.FechaNacimiento.ToString("yyyy-MM-dd"));

				return RedirectToAction("Index", "Reportes"); // O donde quieras redirigir
			}

			ViewBag.Error = "Usuario o contraseña incorrectos";
			return new RedirectToActionResult("Index", "Home", null);
			//return View("Index");
		}
		public async Task<IActionResult> nuevo_usuario(Registro_usuario model)
		{
			inicia_secion_respuesta_registro usuario_nuevo = await _context.ValidarregistroAsync(model);
			if(usuario_nuevo.Resultado=="1")
			{
				var user_creado = usuario_nuevo;
				inicia_secion respuesta = new inicia_secion
				{
					Resultado = user_creado.Resultado,
					Contrasena_login = user_creado.Contrasena_login,
					NumeroIdentificacion_login = user_creado.NumeroIdentificacion_login,
					// Agrega otras propiedades que necesites copiar...
				};
				Usuario_Logueado usuario = await _context.ValidarLoginAsync(respuesta);
				if (usuario != null)
				{
					// Aquí creas sesión o cookies si deseas
					HttpContext.Session.SetInt32("Id_user", usuario.Id_user);
					HttpContext.Session.SetInt32("Id_tipo_user", usuario.Id_tipo_user);
					HttpContext.Session.SetString("TipoUsuario", usuario.TipoUsuario ?? "");
					HttpContext.Session.SetString("TipoIdentificacion", usuario.TipoIdentificacion ?? "");
					HttpContext.Session.SetString("Identificacion", usuario.Identificacion ?? "");
					HttpContext.Session.SetString("Nombre", usuario.Nombre ?? "");
					HttpContext.Session.SetString("Apellido", usuario.Apellido ?? "");
					HttpContext.Session.SetString("Correo", usuario.Correo ?? "");
					HttpContext.Session.SetString("Genero", usuario.Genero ?? "");
					// La fecha debes convertirla a string para guardarla
					HttpContext.Session.SetString("FechaNacimiento", usuario.FechaNacimiento.ToString("yyyy-MM-dd"));

					return RedirectToAction("Index", "Reportes"); // O donde quieras redirigir
				}
			}
			

			ViewBag.Error = "Usuario o contraseña incorrectos";
			return new RedirectToActionResult("Index", "Home", null);
			//return View("Index");
		}

		public IActionResult CerrarSesion()
		{
			HttpContext.Session.Clear(); // Elimina todos los datos de la sesión
			return new RedirectToActionResult("Index", "Home", null);
		}

		[VerificaSesion]
		public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

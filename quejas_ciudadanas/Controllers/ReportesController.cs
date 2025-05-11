using Microsoft.AspNetCore.Mvc;
using quejas_ciudadanas.Data;
using quejas_ciudadanas.Models;
using System.Diagnostics;
using quejas_ciudadanas.Filtros;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;


namespace quejas_ciudadanas.Controllers
{
	[VerificaSesion]
	public class ReportesController : Controller
    {
        private readonly ILogger<ReportesController> _logger;
		private readonly QuejasCiudadanasContext _context;
		public ReportesController(ILogger<ReportesController> logger, QuejasCiudadanasContext context)
        {
            _logger = logger;
			_context = context;
		}

        public IActionResult Index()
        {
			ViewBag.OcultarMenu = false;
            ViewBag.OcultarMenu = false;
            ViewBag.Problemas = _context.Problema.ToList();
            ViewBag.deptos = _context.CAT001_DEPTOS.ToList();
            ViewBag.estado_caso = _context.Estado_caso.ToList();
            return View();
        }
        //[VerificaSesion]
        public IActionResult crear_reporte()
        {
            ViewBag.OcultarMenu = false;
            ViewBag.Problemas = _context.Problema.ToList();
            ViewBag.deptos = _context.CAT001_DEPTOS.ToList();
            return View();
        }

        [HttpGet]
        public JsonResult GetClasesProblema(int id)
        {
            var clases = _context.Clase_Problema
                .Where(c => c.Id_Problema == id)
                .Select(c => new { id_Clase_Problema = c.Id_Clase_Problema, nombre = c.Nombre })
                .ToList();
            return Json(clases);
        }

        [HttpGet]
        public JsonResult GetTiposProblema(int id)
        {
            var tipos = _context.Tipo_Problema
                .Where(t => t.Id_Clase_Problema == id)
                .Select(t => new { id_Tipo_Problema = t.Id_Tipo_Problema, nombre = t.Nombre })
                .ToList();
            return Json(tipos);
        }

        [HttpGet]
        public JsonResult ObtenerCiudades(string codDepto)
        {
            var ciudades = _context.CAT002_CIUDADES
                .Where(c => c.A002CODDEPTO == codDepto)
                .Select(c => new { c.A002CODUBICACION, c.A002DESCRIPCION })
                .ToList();
            //A002ID
            return Json(ciudades);
        }

        [HttpPost]
		public async Task<IActionResult> GuardarReporte([FromForm]Reporte_nuevo model)
		{
            var session = HttpContext.Session;
            var idUsuario = session.GetInt32("Id_user");
            model.Id_user = (int)idUsuario;
            model.id_estado = 1;

            respuesta_repote_registrado nuevo_reporte = await _context.GuardarReporteCiudadanoAsync(model);
            if (nuevo_reporte != null)
            {
                // Validar que se recibieron los 6 archivos
                var archivos = new List<IFormFile>();
                for (int i = 0; i < 6; i++)
                {
                    var archivo = Request.Form.Files["Imagen" + i];
                    if (archivo == null || archivo.Length == 0)
                        return BadRequest($"Imagen {i + 1} no fue cargada.");

                    archivos.Add(archivo);
                }

                var idReporte = nuevo_reporte.Id_caso+"_"+ nuevo_reporte.Id_reporte+"_"+ nuevo_reporte.Id_user_ciudadano+"_"+ nuevo_reporte.llave;

                // Crear carpeta para imágenes
                string carpeta = Path.Combine("wwwroot/evidencias", idReporte.ToString());
                Directory.CreateDirectory(carpeta);

                for (int i = 0; i < archivos.Count; i++)
                {
                    var nombreArchivo = $"evidencia_{i + 1}{Path.GetExtension(archivos[i].FileName)}";
                    var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await archivos[i].CopyToAsync(stream);
                    }

                    // Guardar en base de datos con SP la ruta
                    var rutaRelativa = $"/evidencias/{idReporte}/{nombreArchivo}";

                    Reporte_nuevo_fotos respuesta = new Reporte_nuevo_fotos
                    {
                        Id_reporte = nuevo_reporte.Id_reporte,
                        Nombre_imagen = nombreArchivo,
                        Ruta_foto = rutaRelativa,
                        // Agrega otras propiedades que necesites copiar...
                    };
                    respuesta_repote_fotos foto_guardada = await _context.GuardarReporteFotosAsync(respuesta);
                }
                return Ok(new
                {
                    mensaje = "Reporte enviado correctamente",
                    //idReporte = idReporte
                });

            }
            else
            {
                return BadRequest("Error al guardar el reporte.");
            }

        }

        public async Task<IActionResult> consultar_reportes_2([FromForm] Filtros_buscar_reportes model)
        {
            CasoFiltrado consulta_repor = await _context.ObtenerCasosFiltradosAsync_2(model);
            // Simulando datos que obtendrías de la base de datos
            var data = Enumerable.Range(1, 6).Select(i => new
            {
                id = i,
                lat = 4.6 + new Random().NextDouble() * 0.1,
                lon = -74.1 + new Random().NextDouble() * 0.1,
                titulo = $"Queja {i}",
                problema = $"Problema {i}",
                ciudad = $"Ciudad {i}",
                fecha = $"2024-05-{(i % 30 + 1).ToString().PadLeft(2, '0')}",
                cantidad = new Random().Next(1, 11),
                estado = (i % 3) + 1
            }).ToList();

            return Json(data);
        }

        public async Task<IActionResult> consultar_reportes([FromForm] Filtros_buscar_reportes model)
        {
            if (string.IsNullOrEmpty(model.selectDepto))
                model.selectDepto = "0000";

            if (string.IsNullOrEmpty(model.selectCiudad))
                model.selectCiudad = "0000";

            List<CasoFiltrado> consulta_repor = await _context.ObtenerCasosFiltradosAsync(model);

            var data = consulta_repor.Select(caso => new
            {
                id=caso.Id_caso,
                lat=caso.latitud,
                lon=caso.longitud,
                problema= caso.Problema,
                clase=caso.Clase_problema,
                tipo=caso.Tipo_problema,
                departamento=caso.Departamento,
                ciudad=caso.Ciudad,
                fecha =caso.Fecha_registro,
                estado = caso.Estado,
                cantidad =caso.Num_quejas
            }).ToList();

            return Json(data);
        }

        public async Task<IActionResult> consultar_caso_x_report([FromForm] Consultar_reportes_caso model)
        {

            List<ReporteInfraestructura> consulta_repor = await _context.ObtenerCasosreportesAsync(model);

            var data_info = consulta_repor.Select(caso => new
            {
                id_reporte = caso.Id_reporte,
                id_caso = caso.Id_caso,
                titulo = caso.Titulo,
                fecha = caso.Fecha_registro,
                problema = caso.Problema,
                clase = caso.Clase_problema,
                tipo = caso.Tipo_problema,
                departamento = caso.Departamento,
                ciudad = caso.Ciudad,
                direccion = caso.Direccion,
                estado = caso.Estado,
                cantidadReportes = caso.Num_quejas,
                tipoDocumento = caso.Tipo_doc,
                numeroIdentificacion = caso.Num_identificacion,
                nombreReportador = caso.Nombre_Completo,
                descripcion = caso.Descripcion,
            }).ToList();

            // Si solo se desea consultar fotos del primer reporte
            var primerReporte = consulta_repor[0];

            Consultar_reportes_caso_fotos consulta_foto_model = new Consultar_reportes_caso_fotos
            {
                IdReporte = primerReporte.Id_reporte
            };

            List<ReporteInfraestructura_fotos> consulta_repor_foto = await _context.ObtenerCasosreportes_fotos_Async(consulta_foto_model);

            var foto = consulta_repor_foto.Select(caso => new
            {
                id_reporte = caso.Ruta_foto

            }).ToList();

            var data = new
            {
                info = data_info,
                fotos = foto
            };

            //
            return Json(data);
        }

        public async Task<IActionResult> lista_reportes_similares([FromForm] Consultar_reportes_caso model)
        {
            model.IdReporte = -1;
            List<ReporteInfraestructura> consulta_repor = await _context.ObtenerCasosreportesAsync(model);

            var data_info = consulta_repor.Select(caso => new
            {
                id_reporte = caso.Id_reporte,
                id_caso = caso.Id_caso,
                nombre = caso.Titulo,
                fecha = caso.Fecha_registro,
                problema = caso.Problema,
                clase = caso.Clase_problema,
                tipo = caso.Tipo_problema,
                departamento = caso.Departamento,
                ciudad = caso.Ciudad,
                direccion = caso.Direccion,
                estado = caso.Estado,
                cantidadReportes = caso.Num_quejas,
                tipoDocumento = caso.Tipo_doc,
                numeroIdentificacion = caso.Num_identificacion,
                nombreReportador = caso.Nombre_Completo,
                descripcion = caso.Descripcion,
            }).ToList();

            var resultado = new
            {
                id = 1,
                nombre = "Informes relacionados con el caso",
                proyectos = consulta_repor
            };
            //
            return Json(resultado);
        }

        [HttpPost]
        public async Task<IActionResult> Enviar_Respuest_caso([FromForm] FormularioModel_respuesta_caso modelo)
        {
            try
            {
                // Guardar la respuesta en la base de datos
                respuesta_repote_registrado_agente nuevo_reporte = await _context.AnsycRegistrar_respuesta_agente(modelo);

                // Validar que nuevo_reporte no sea null
                if (nuevo_reporte == null)
                {
                    return BadRequest(new { mensaje = "No se pudo guardar la respuesta, intente nuevamente." });
                }

                // Usar nuevo_reporte.id_respuesta_agente dentro de la ruta
                var rutasArchivos = new List<string>();
                var idRespuestaAgente = nuevo_reporte.id_respuesta_agente; // Obtén el id_respuesta_agente

                // Base path de almacenamiento, incluyendo id_respuesta_agente en la ruta
                var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "evidencias_respuesta_caso", modelo.id_caso.ToString(), idRespuestaAgente.ToString());

                // Crear directorios si es necesario
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                // Crear carpeta para fotos si vienen fotos
                if (modelo.fotos.Any())
                {
                    var fotosPath = Path.Combine(basePath, "adjunto_foto");
                    if (!Directory.Exists(fotosPath))
                    {
                        Directory.CreateDirectory(fotosPath);
                    }

                    int index = 1;
                    foreach (var foto in modelo.fotos)
                    {
                        if (foto.Length > 0)
                        {
                            var extension = Path.GetExtension(foto.FileName).ToLower();
                            var fileName = $"foto_{index}{extension}";
                            var fullPath = Path.Combine(fotosPath, fileName);

                            // Validación de tipo de imagen
                            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                            {
                                return BadRequest(new { mensaje = "Solo se permiten imágenes .jpg, .jpeg y .png." });
                            }

                            using var stream = new FileStream(fullPath, FileMode.Create);
                            await foto.CopyToAsync(stream);

                            rutasArchivos.Add($"/evidencias_respuesta_caso/{modelo.id_caso}/{idRespuestaAgente}/adjunto_foto/{fileName}");

                            // Crear el objeto para fotos y guardar en la base de datos
                            var fotoModel = new Reporte_nuevo_fotos_adjente
                            {
                                Id_resp_agente = idRespuestaAgente,
                                Nombre_imagen = fileName,
                                Ruta_foto = rutasArchivos.Last()
                            };

                            // Llamar al método GuardarReporteFotos_adjenteAsync
                            await _context.GuardarReporteFotos_adjenteAsync(fotoModel);

                            index++;
                        }
                    }
                }

                // Verificar y guardar PDF si es proporcionado
                if (modelo.archivoPdf != null && modelo.archivoPdf.Length > 0)
                {
                    var pdfExtension = Path.GetExtension(modelo.archivoPdf.FileName).ToLower();
                    if (pdfExtension != ".pdf")
                    {
                        return BadRequest(new { mensaje = "El archivo debe ser un PDF." });
                    }

                    var pdfPath = Path.Combine(basePath, "adjunto_pdf");
                    if (!Directory.Exists(pdfPath))
                    {
                        Directory.CreateDirectory(pdfPath);
                    }

                    var fileName = "pdf_1.pdf";
                    var fullPath = Path.Combine(pdfPath, fileName);

                    using var stream = new FileStream(fullPath, FileMode.Create);
                    await modelo.archivoPdf.CopyToAsync(stream);

                    rutasArchivos.Add($"/evidencias_respuesta_caso/{modelo.id_caso}/{idRespuestaAgente}/adjunto_pdf/{fileName}");

                    // Crear el objeto para el adjunto y guardar en la base de datos
                    var adjuntoModel = new Reporte_nuevo_adjunto_adjente
                    {
                        Id_resp_agente = idRespuestaAgente,
                        Nombre_imagen = fileName,
                        Ruta_foto = rutasArchivos.Last()
                    };

                    // Llamar al método GuardarReporteadjunto_adjenteAsync
                    await _context.GuardarReporteadjunto_adjenteAsync(adjuntoModel);
                }

                // Si todo es exitoso, retorna las rutas de los archivos
                //return Ok(new
                //{
                //    mensaje = "Respuesta enviada exitosamente.",
                //    archivos = rutasArchivos
                //});
                return Json(new { mensaje = "Formulario enviado correctamente." });

            }
            catch (Exception ex)
            {
                // Manejo de excepciones global
                return StatusCode(500, new { mensaje = "Hubo un error al procesar la solicitud.", error = ex.Message });
            }
        }

        //
        public async Task<IActionResult> consultar_respuesta_agentes_2([FromForm] Respuestas_agente model)
        {

            List<respuesta_agente_recibe> consulta_repor = await _context.Obtenerrespuesta_agenteAsync(model);

            // Si solo se desea consultar fotos del primer reporte
            var primerReporte = consulta_repor[0];

            Respuestas_agente_fotos consulta_foto_model = new Respuestas_agente_fotos
            {
                IdRespAgente = primerReporte.Id_resp_agente
            };

            Respuestas_agente_adjuntos consulta_adjunto_model = new Respuestas_agente_adjuntos
            {
                IdRespAgente = primerReporte.Id_resp_agente
            };

            List<ReporteInfraestructura_fotos_agente> consulta_repor_foto_agente = await _context.ObtenerRespuestas_agente_fotos_Async(consulta_foto_model);
            List<ReporteInfraestructura_fotos_adjuntos> consulta_repor_adjunto_agente = await _context.ObtenerRespuestas_agente_adjunto_Async(consulta_adjunto_model);



            //
            return Json(primerReporte);
        }

        public async Task<IActionResult> consultar_respuesta_agentes([FromForm] Respuestas_agente model)
        {
            var consulta_repor = await _context.Obtenerrespuesta_agenteAsync(model);

            if (consulta_repor == null || !consulta_repor.Any())
            {
                return Json(new List<object>());
            }

            var documentos = new List<object>();

            foreach (var reporte in consulta_repor)
            {
                var consulta_foto_model = new Respuestas_agente_fotos
                {
                    IdRespAgente = reporte.Id_resp_agente
                };

                var consulta_adjunto_model = new Respuestas_agente_adjuntos
                {
                    IdRespAgente = reporte.Id_resp_agente
                };

                var consulta_repor_foto_agente = await _context.ObtenerRespuestas_agente_fotos_Async(consulta_foto_model);
                var consulta_repor_adjunto_agente = await _context.ObtenerRespuestas_agente_adjunto_Async(consulta_adjunto_model);

                var imagenes = (consulta_repor_foto_agente != null)
                    ? consulta_repor_foto_agente.Select(f => (object)new { url = f.Ruta_foto }).ToList()
                    : new List<object>();


                string pdfUrl = consulta_repor_adjunto_agente?.FirstOrDefault()?.Ruta_adjunto ?? "";

                documentos.Add(new
                {
                    id = reporte.Id_resp_agente,
                    titulo = reporte.Titulo ?? "Sin título",
                    fecha = reporte.Fecha_registro.ToString("dd MMM yyyy"),
                descripcion = reporte.Descripcion ?? "Sin descripción",
                    imagenes = imagenes,
                    pdfUrl = pdfUrl
                });
            }

            return Json(documentos);
        }

        //

        public IActionResult CerrarSesion()
		{
			HttpContext.Session.Clear(); // Elimina todos los datos de la sesión
			return new RedirectToActionResult("Index", "Home", null);
		}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

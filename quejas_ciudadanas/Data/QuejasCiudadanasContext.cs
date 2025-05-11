using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using quejas_ciudadanas.Models;

namespace quejas_ciudadanas.Data
{
	public class QuejasCiudadanasContext : DbContext
	{
		public QuejasCiudadanasContext(DbContextOptions<QuejasCiudadanasContext> options)
			: base(options)
		{
		}

		public DbSet<TipoIdentificacion> Tipo_identificacion { get; set; }
		public DbSet<Sexo> Sexo { get; set; }
        public DbSet<Problema> Problema { get; set; }
        public DbSet<Clase_Problema> Clase_Problema { get; set; }
        public DbSet<Tipo_Problema> Tipo_Problema { get; set; }
        public DbSet<CAT001_DEPTOS> CAT001_DEPTOS { get; set; }
        public DbSet<CAT002_CIUDADES> CAT002_CIUDADES { get; set; }
        public DbSet<Estado_caso> Estado_caso { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relaciones
            modelBuilder.Entity<Clase_Problema>()
                .HasOne(c => c.Problema)
                .WithMany(p => p.Clases_Problema)
                .HasForeignKey(c => c.Id_Problema);

            modelBuilder.Entity<Tipo_Problema>()
                .HasOne(t => t.Clase_Problema)
                .WithMany(c => c.Tipos_Problema)
                .HasForeignKey(t => t.Id_Clase_Problema);

			modelBuilder.Entity<CAT002_CIUDADES>()
				.HasOne(c => c.CAT001_DEPTOS)
				.WithMany(d => d.Ciudades)
				.HasForeignKey(c => c.A002CODDEPTO)
				.HasPrincipalKey(d => d.A001CODIGO); // Relación por A001CODIGO, no ID
        }
        public DbSet<Usuario_Logueado> LoginTemp { get; set; } // Para evitar conflictos con tabla real
		public async Task<Usuario_Logueado?> ValidarLoginAsync(inicia_secion model)
		{
			var identificacionParam = new SqlParameter("@Identificacion", model.NumeroIdentificacion_login);
			var claveParam = new SqlParameter("@Clave", model.Contrasena_login);

			var result = await LoginTemp
				.FromSqlRaw("EXEC sp_validar_login @Identificacion, @Clave", identificacionParam, claveParam)
				.ToListAsync();

			return result.FirstOrDefault();
		}

		public DbSet<inicia_secion_respuesta_registro> RegisterTemp { get; set; } // Para evitar conflictos con tabla real
		public async Task<inicia_secion_respuesta_registro?> ValidarregistroAsync(Registro_usuario model)
		{
			var parametros = new[]
			{
				new SqlParameter("@Id_tipo_user", model.tipo_usuario),
				new SqlParameter("@Id_identificacion", model.tipo_doc_regist), // ID del tipo de documento
				new SqlParameter("@Identificacion_reg", model.numero_doc_regist),
				new SqlParameter("@Nombre", model.nombre_regist),
				new SqlParameter("@Apellido", model.apellido_regist),
				new SqlParameter("@Correo", model.email_regist),
				new SqlParameter("@FechaNacimiento", model.fech_nacimien_regist),
				new SqlParameter("@Id_genero", model.sexo_regist),
				new SqlParameter("@CLAVE", model.clave_regist)
			};

			var result = await RegisterTemp
				.FromSqlRaw("EXEC sp_registrar_usuario @Id_tipo_user, @Id_identificacion, @Identificacion_reg, @Nombre, @Apellido, @Correo, @FechaNacimiento, @Id_genero, @CLAVE", parametros)
				.ToListAsync();

			return result.FirstOrDefault();
		}

        public DbSet<respuesta_repote_registrado> ReporteTemp { get; set; } // Para evitar conflictos con tabla real
        public async Task<respuesta_repote_registrado?> GuardarReporteCiudadanoAsync(Reporte_nuevo model)
        {
            var parametros = new[]
            {
				new SqlParameter("@Id_user", model.Id_user),
				new SqlParameter("@id_estado", model.id_estado),
				new SqlParameter("@Latitud", model.Latitud),
				new SqlParameter("@Longitud", model.Longitud),
				new SqlParameter("@Geohash", model.Geohash),
				new SqlParameter("@TipoProblema", model.TipoProblema),
				new SqlParameter("@Ciudad", model.Ciudad),
				new SqlParameter("@Direccion", model.Direccion),
				new SqlParameter("@Descripcion", model.Descripcion)
			};
            string sqlConsulta = $@"
            EXEC sp_guardar_report_ciudadanos 
                @Id_user = '{model.Id_user}', 
                @id_estado = '{model.id_estado}', 
                @Latitud = '{model.Latitud}', 
                @Longitud = '{model.Longitud}', 
                @Geohash = '{model.Geohash}', 
                @TipoProblema = '{model.TipoProblema}', 
                @Ciudad = '{model.Ciudad}', 
                @Direccion = '{model.Direccion}', 
                @Descripcion = '{model.Descripcion}'";

            var result = await ReporteTemp
                .FromSqlRaw("EXEC sp_guardar_report_ciudadanos @Id_user, @id_estado, @Latitud, @Longitud, @Geohash, @TipoProblema, @Ciudad, @Direccion, @Descripcion", parametros)
                .ToListAsync();

            return result.FirstOrDefault();
        }

        public DbSet<respuesta_repote_fotos> Reportefotos { get; set; } // Para evitar conflictos con tabla real
        public async Task<respuesta_repote_fotos?> GuardarReporteFotosAsync(Reporte_nuevo_fotos model)
        {
            var parametros = new[]
            {
                new SqlParameter("@Id_reporte", model.Id_reporte),
                new SqlParameter("@Nombre_imagen", model.Nombre_imagen),
                new SqlParameter("@Ruta_foto", model.Ruta_foto),

            };

            var result = await Reportefotos
                .FromSqlRaw("EXEC sp_guardar_foto_reporte @Id_reporte, @Nombre_imagen, @Ruta_foto ", parametros)
                .ToListAsync();

            return result.FirstOrDefault();
        }

        public DbSet<CasoFiltrado> Consulta_report { get; set; } // Para evitar conflictos con tabla real

        public async Task<CasoFiltrado?> ObtenerCasosFiltradosAsync_2(Filtros_buscar_reportes model)
        {
            var parametros = new[]
            {
                new SqlParameter("@problemaSelect", model.problemaSelect),
                new SqlParameter("@claseProblemaSelect", model.claseProblemaSelect),
                new SqlParameter("@tipoProblemaSelect", model.tipoProblemaSelect),
                new SqlParameter("@selectDepto", (object?)model.selectDepto ?? DBNull.Value),
                new SqlParameter("@selectCiudad", (object?)model.selectCiudad ?? DBNull.Value),
                new SqlParameter("@Estado_del_caso", model.Estado_del_caso)
            };

            var result = await Consulta_report
                .FromSqlRaw("EXEC sp_ConsultaCasosFiltrados @problemaSelect, @claseProblemaSelect, @tipoProblemaSelect, @selectDepto, @selectCiudad, @Estado_del_caso", parametros)
                .ToListAsync();

            return result.FirstOrDefault();
        }
        public async Task<List<CasoFiltrado>> ObtenerCasosFiltradosAsync(Filtros_buscar_reportes model)
        {

           var parametros = new[]
            {
                new SqlParameter("@problemaSelect", model.problemaSelect),
                new SqlParameter("@claseProblemaSelect", model.claseProblemaSelect),
                new SqlParameter("@tipoProblemaSelect", model.tipoProblemaSelect),
                new SqlParameter("@selectDepto", model.selectDepto) ,
                new SqlParameter("@selectCiudad", model.selectCiudad ),
                new SqlParameter("@Estado_del_caso", model.Estado_del_caso)
            };

            string sqlConsulta = $@"
                EXEC sp_ConsultaCasosFiltrados 
                    @problemaSelect = '{model.problemaSelect}', 
                    @claseProblemaSelect = '{model.claseProblemaSelect}', 
                    @tipoProblemaSelect = '{model.tipoProblemaSelect}', 
                    @selectDepto = '{model.selectDepto}', 
                    @selectCiudad = '{model.selectCiudad}', 
                    @Estado_del_caso = '{model.Estado_del_caso}'";

            try
            {
                var result = await Consulta_report
                    .FromSqlRaw("EXEC sp_ConsultaCasosFiltrados @problemaSelect, @claseProblemaSelect, @tipoProblemaSelect, @selectDepto, @selectCiudad, @Estado_del_caso", parametros)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                // Revisa esto en tiempo de depuración o en logs
                throw new Exception("Error al ejecutar el procedimiento almacenado", ex);
            }

            //return result;
        }

        public DbSet<ReporteInfraestructura> ReporteInfraestructura { get; set; } // Para evitar conflictos con tabla real

        public async Task<List<ReporteInfraestructura>> ObtenerCasosreportesAsync(Consultar_reportes_caso model)
        {

            var parametros = new[]
             {
                new SqlParameter("@IdCaso", model.IdCaso),
                new SqlParameter("@IdReporte", model.IdReporte),
            };

            string sqlConsulta = $@"
                EXEC Sp_consulta_reportes_unicos 
                    @IdCaso = '{model.IdCaso}', 
                    @IdReporte = '{model.IdReporte}'";

            try
            {
                var result = await ReporteInfraestructura
                    .FromSqlRaw("EXEC Sp_consulta_reportes_unicos  @IdCaso, @IdReporte", parametros)
                    .AsNoTracking() // <--- clave
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                // Revisa esto en tiempo de depuración o en logs
                throw new Exception("Error al ejecutar el procedimiento almacenado", ex);
            }

            //return result;
        }

        public DbSet<ReporteInfraestructura_fotos> ReporteInfraestructura_fotos { get; set; } // Para evitar conflictos con tabla real

        public async Task<List<ReporteInfraestructura_fotos>> ObtenerCasosreportes_fotos_Async(Consultar_reportes_caso_fotos model)
        {

            var parametros = new[]
             {
                new SqlParameter("@IdReporte", model.IdReporte),
            };

            string sqlConsulta = $@"
                EXEC Sp_obtener_fotos_reporte 
                    @IdReporte = '{model.IdReporte}'";

            try
            {
                var result = await ReporteInfraestructura_fotos
                    .FromSqlRaw("EXEC Sp_obtener_fotos_reporte  @IdReporte", parametros)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                // Revisa esto en tiempo de depuración o en logs
                throw new Exception("Error al ejecutar el procedimiento almacenado", ex);
            }

            //return result;
        }


        public DbSet<respuesta_repote_registrado_agente> Registrar_respuesta_agente { get; set; } // Para evitar conflictos con tabla real
        public async Task<respuesta_repote_registrado_agente?> AnsycRegistrar_respuesta_agente(FormularioModel_respuesta_caso model)
        {
            var parametros = new[]
            {
                new SqlParameter("@Id_caso", model.id_caso),
                new SqlParameter("@id_opcion_caso", model.opcion), // ID del tipo de documento
				new SqlParameter("@Descripcion", model.comentario)
            };

            var result = await Registrar_respuesta_agente
                .FromSqlRaw("EXEC sp_Guardar_RespuestaAgente @Id_caso,@id_opcion_caso,@Descripcion", parametros)
                .ToListAsync();

            return result.FirstOrDefault();
        }

        public DbSet<respuesta_repote_fotos_agente> Reportefotos_agentes { get; set; } // Para evitar conflictos con tabla real
        public async Task<respuesta_repote_fotos_agente?> GuardarReporteFotos_adjenteAsync(Reporte_nuevo_fotos_adjente model)
        {
            var parametros = new[]
            {
                new SqlParameter("@Id_resp_agente", model.Id_resp_agente),
                new SqlParameter("@Nombre_imagen", model.Nombre_imagen),
                new SqlParameter("@Ruta_foto", model.Ruta_foto),

            };

            var result = await Reportefotos_agentes
                .FromSqlRaw("EXEC sp_guardar_foto_respuesta_agente @Id_resp_agente, @Nombre_imagen, @Ruta_foto ", parametros)
                .ToListAsync();

            return result.FirstOrDefault();
        }

        public DbSet<respuesta_repote_adjunto_agente> Reporteadjunto_agentes { get; set; } // Para evitar conflictos con tabla real
        public async Task<respuesta_repote_adjunto_agente?> GuardarReporteadjunto_adjenteAsync(Reporte_nuevo_adjunto_adjente model)
        {
            var parametros = new[]
            {
                new SqlParameter("@Id_resp_agente", model.Id_resp_agente),
                new SqlParameter("@Nombre_imagen", model.Nombre_imagen),
                new SqlParameter("@Ruta_foto", model.Ruta_foto),

            };

            var result = await Reporteadjunto_agentes
                .FromSqlRaw("EXEC sp_guardar_adjunto_respuesta_agente @Id_resp_agente, @Nombre_imagen, @Ruta_foto ", parametros)
                .ToListAsync();

            return result.FirstOrDefault();
        }

        public DbSet<respuesta_agente_recibe> respuesta_agente { get; set; } // Para evitar conflictos con tabla real
        public async Task<List<respuesta_agente_recibe>> Obtenerrespuesta_agenteAsync(Respuestas_agente model)
        {

            var parametros = new[]
             {
                new SqlParameter("@IdCaso", model.IdCaso)
            };

            string sqlConsulta = $@"
                EXEC Sp_obtener_respuestas_agente 
                    @IdCaso = '{model.IdCaso}'";

            try
            {
                var result = await respuesta_agente
                    .FromSqlRaw("EXEC Sp_obtener_respuestas_agente @IdCaso", parametros)
                    .AsNoTracking() // <--- clave
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                // Revisa esto en tiempo de depuración o en logs
                throw new Exception("Error al ejecutar el procedimiento almacenado", ex);
            }

            //return result;
        }

        public DbSet<ReporteInfraestructura_fotos_agente> respuesta_agente_fotos { get; set; } // Para evitar conflictos con tabla real
        public async Task<List<ReporteInfraestructura_fotos_agente>> ObtenerRespuestas_agente_fotos_Async(Respuestas_agente_fotos model)
        {

            var parametros = new[]
             {
                new SqlParameter("@IdRespAgente", model.IdRespAgente),
            };

            string sqlConsulta = $@"
                EXEC Sp_obtener_fotos_agente 
                    @IdRespAgente = '{model.IdRespAgente}'";

            try
            {
                var result = await respuesta_agente_fotos
                    .FromSqlRaw("EXEC Sp_obtener_fotos_agente  @IdRespAgente", parametros)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                // Revisa esto en tiempo de depuración o en logs
                throw new Exception("Error al ejecutar el procedimiento almacenado", ex);
            }

            //return result;
        }

        public DbSet<ReporteInfraestructura_fotos_adjuntos> respuesta_agente_adjunto { get; set; } // Para evitar conflictos con tabla real
        public async Task<List<ReporteInfraestructura_fotos_adjuntos>> ObtenerRespuestas_agente_adjunto_Async(Respuestas_agente_adjuntos model)
        {

            var parametros = new[]
             {
                new SqlParameter("@IdRespAgente", model.IdRespAgente),
            };

            string sqlConsulta = $@"
                EXEC Sp_obtener_adjuntos_agente 
                    @IdRespAgente = '{model.IdRespAgente}'";

            try
            {
                var result = await respuesta_agente_adjunto
                    .FromSqlRaw("EXEC Sp_obtener_adjuntos_agente @IdRespAgente", parametros)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                // Revisa esto en tiempo de depuración o en logs
                throw new Exception("Error al ejecutar el procedimiento almacenado", ex);
            }

            //return result;
        }


    }
}

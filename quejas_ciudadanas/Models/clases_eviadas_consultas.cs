using System.ComponentModel.DataAnnotations;

namespace quejas_ciudadanas.Models
{
	public class inicia_secion
	{
		public string NumeroIdentificacion_login { get; set; }
		public string Contrasena_login { get; set; }
		public string Resultado {  get; set; }
	}

	public class Registro_usuario
	{
		public string nombre_regist { get; set; } // ID del tipo de documento (del select)

		public int numero_doc_regist { get; set; }

		public string tipo_doc_regist { get; set; } // Campo separado para evitar conflicto de nombre

		public string apellido_regist { get; set; }

		public string email_regist { get; set; }

		public string clave_regist { get; set; }

		public DateTime fech_nacimien_regist { get; set; }

		public int sexo_regist { get; set; }

		public int tipo_usuario { get; set; }
	}

    public class Reporte_nuevo
    {
        public int Id_user { get; set; }
        public int id_estado { get; set; }
        public string Latitud { get; set; }
        public string Longitud { get; set; }
        public string Geohash { get; set; }
        public string TipoProblema { get; set; }
        public string Ciudad { get; set; }
        public string Direccion { get; set; }
        public string Descripcion { get; set; }
    }

    public class Reporte_nuevo_fotos
    {
        public int Id_reporte { get; set; }
        public string Nombre_imagen { get; set; }
        public string Ruta_foto { get; set; }

    }

    public class Filtros_buscar_reportes
    {
        public int problemaSelect { get; set; }
        public int claseProblemaSelect { get; set; }
        public int tipoProblemaSelect { get; set; }
        public string selectDepto { get; set; } 
        public string selectCiudad { get; set; }
        public int Estado_del_caso { get; set; }

    }

    public class Consultar_reportes_caso
    {
        public int IdCaso {  get; set; }
        public int IdReporte { get; set; }

    }

    public class Consultar_reportes_caso_fotos
    {
        public int IdReporte { get; set; }

    }

    public class FormularioModel_respuesta_caso
    {
        public int id_caso { get; set; }

        public int opcion { get; set; }

        public string comentario { get; set; }

        public List<IFormFile> fotos { get; set; } = new();

        public IFormFile archivoPdf { get; set; }
    }

    public class Reporte_nuevo_fotos_adjente
    {
        public int Id_resp_agente { get; set; }
        public string Nombre_imagen { get; set; }
        public string Ruta_foto { get; set; }

    }

    public class Reporte_nuevo_adjunto_adjente
    {
        public int Id_resp_agente { get; set; }
        public string Nombre_imagen { get; set; }
        public string Ruta_foto { get; set; }

    }


    public class Respuestas_agente
    {
        public int IdCaso { get; set; }

    }
    public class Respuestas_agente_fotos
    {
        public int IdRespAgente { get; set; }

    }
    public class Respuestas_agente_adjuntos
    {
        public int IdRespAgente { get; set; }

    }


}

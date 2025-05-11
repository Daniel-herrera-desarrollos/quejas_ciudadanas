using System.ComponentModel.DataAnnotations;

namespace quejas_ciudadanas.Models
{


	public class Usuario_Logueado
	{
		[Key]
		public int Id_user { get; set; }
		public int Id_tipo_user { get; set; }
		public string TipoUsuario { get; set; }
		public string TipoIdentificacion { get; set; }
		public string Identificacion { get; set; }
		public string Nombre { get; set; }
		public string Apellido { get; set; }
		public string Correo { get; set; }
		public DateTime FechaNacimiento { get; set; }
		public string Genero { get; set; }
	}
	public class inicia_secion_respuesta_registro
	{
		[Key]
		public string NumeroIdentificacion_login { get; set; }
		public string Contrasena_login { get; set; }
		public string Resultado { get; set; }
	}

    public class respuesta_repote_registrado
    {
        [Key]
        public int Id_reporte { get; set; }
        public string llave { get; set; }
        public int Id_caso { get; set; }
        public int Id_user_ciudadano { get; set; }
    }

    public class respuesta_repote_fotos
    {
        [Key]
        public int ErrorNumber { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class CasoFiltrado
    {
        [Key]
        public int Id_caso { get; set; }
        public string latitud { get; set; }
        public string longitud { get; set; }
        public string Problema { get; set; }
        public string Clase_problema { get; set; }
        public string Tipo_problema { get; set; }
        public string Departamento { get; set; }
        public string Ciudad { get; set; }
        public DateTime Fecha_registro { get; set; }
        public int Estado { get; set; }
        public int Num_quejas { get; set; }
    }

    public class ReporteInfraestructura
    {
        [Key]
        public int Id_caso { get; set; }
        public int Id_reporte { get; set; }
        public string Titulo { get; set; }
        public string Problema { get; set; }
        public string Clase_problema { get; set; }
        public string Tipo_problema { get; set; }
        public string Departamento { get; set; }
        public string Ciudad { get; set; }
        public string Direccion { get; set; }
        public DateTime Fecha_registro { get; set; }
        public int Estado { get; set; }
        public int Num_quejas { get; set; }
        public string Tipo_doc { get; set; }
        public string Num_identificacion { get; set; }
        public string Nombre_Completo { get; set; }
        public string Descripcion { get; set; }
    }

    public class ReporteInfraestructura_fotos
    {
        [Key]
        public string Ruta_foto { get; set; }

    }
    public class respuesta_repote_registrado_agente
    {
        [Key]
        public int id_respuesta_agente { get; set; }
    }

    public class respuesta_repote_fotos_agente
    {
        [Key]
        public int ErrorNumber { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class respuesta_repote_adjunto_agente
    {
        [Key]
        public int ErrorNumber { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class respuesta_agente_recibe
    {
        [Key]
        public int Id_resp_agente { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha_registro { get; set; }
    }

    public class ReporteInfraestructura_fotos_agente
    {
        [Key]
        public string Ruta_foto { get; set; }

    }

    public class ReporteInfraestructura_fotos_adjuntos
    {
        [Key]
        public string Ruta_adjunto { get; set; }

    }
}

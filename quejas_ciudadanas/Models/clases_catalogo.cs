using System.ComponentModel.DataAnnotations;

namespace quejas_ciudadanas.Models
{
	public class TipoIdentificacion
	{
		[Key]
		public int Id_identificacion { get; set; }
		public string Descripcion { get; set; }
	}

	public class Sexo
	{
		[Key]
		public int Id_sexo { get; set; }
		public string Descripcion { get; set; }
	}

    public class Problema
    {
        [Key]
        public int Id_Problema { get; set; }
        public string Nombre { get; set; }

        public ICollection<Clase_Problema> Clases_Problema { get; set; }
    }
    public class Clase_Problema
    {
        [Key]
        public int Id_Clase_Problema { get; set; }
        public string Nombre { get; set; }

        public int Id_Problema { get; set; }
        public Problema Problema { get; set; }

        public ICollection<Tipo_Problema> Tipos_Problema { get; set; }
    }
    public class Tipo_Problema
    {
        [Key]
        public int Id_Tipo_Problema { get; set; }
        public string Nombre { get; set; }

        public int Id_Clase_Problema { get; set; }
        public Clase_Problema Clase_Problema { get; set; }
    }
    public class CAT001_DEPTOS
    {
        [Key]
        public short A001ID { get; set; }
        public string A001CODIGO { get; set; }
        public string A001DESCRIPCION { get; set; }

        public ICollection<CAT002_CIUDADES> Ciudades { get; set; }
    }
    public class CAT002_CIUDADES
    {
        [Key]
        public short A002ID { get; set; }
        public string A002CODDEPTO { get; set; } // Clave foránea al departamento
        public string A002CODCIUDAD { get; set; }
        public string A002DESCRIPCION { get; set; }
        public string A002CODUBICACION {  get; set; }

        public CAT001_DEPTOS CAT001_DEPTOS { get; set; }
    }

    public class Estado_caso
    {
        [Key]
        public int Id_estado_caso { get; set; }
        public string Descripcion { get; set; }

    }
}

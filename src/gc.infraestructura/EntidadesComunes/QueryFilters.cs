using gc.infraestructura.Dtos.Gen;
using System;

namespace gc.infraestructura.Core.EntidadesComunes
{
    public class QueryFilters : BaseFilters
    {
        public QueryFilters() {
            Buscar = "";
        }

        public bool Todo { 
            get { return string.IsNullOrWhiteSpace(Id)
                    && string.IsNullOrWhiteSpace(Id2)
                    && !FechaD.HasValue 
                    && string.IsNullOrWhiteSpace(Buscar) 
                    && Registros == default 
                    && Pagina == default; } }
        public string? Id { get; set; }
        public string? Id2 { get; set; }
        /// <summary>
        /// Campo especificamente para buscar una serie de registros de una relacion primaria
        /// </summary>
        public List<string>? Rel01 { get; set; }
        /// <summary>
        /// Campo especificamente para buscar una serie de registros de una relacion secundaria
        /// </summary>
        public List<string>? Rel02 { get; set; }
        /// <summary>
        /// Campo especificamente para buscar una serie de registros de una relacion terciaria
        /// </summary>
        public List<ComboGenDto>? Rel03 { get; set; }
        public DateTime? FechaD { get; set; }
        public DateTime? FechaH { get; set; }
        /// <summary>
        /// Campo orientado a realizar cualquier busqueda de dato textual
        /// </summary>
        public string? Buscar { get; set; }
        /// <summary>
        /// Este campo indica la cantidad máxima por lote que se devolverá a la vista, constituyendo, de esta manera, el tamaño de 1 página en el paginado.
        /// </summary>
        public int? Registros { get; set; }
        /// <summary>
        /// Indica la pagina dentro del paginado que se desea devolver. Esta permitira, en conjunto con registros determinar la cantidad de registros que se dejaran pasar para recien tomar la cantidad de instancias de datos especificados en el campo "registros".
        /// </summary>
        public int? Pagina { get; set; }

		/*
        
         */
		public bool Opt1 { get; set; }
		public bool Opt2 { get; set; }
		public bool Opt3 { get; set; }
		public bool Opt4 { get; set; }
		public bool Opt5 { get; set; }
	}
    public class BaseFilters
    {
        /// <summary>
        /// Campo destinado a definir por que campo se va a ordenar la consulta.
        /// </summary>
        public string? Sort { get; set; }
        /// <summary>
        /// Campo para definir ordenamiento de datos.
        /// </summary>
        public string? SortDir { get; set; } = "ASC";

        public int Paginas { get; set; }
        public int TotalRegistros { get; set; }

        //orientado para poder seleccionar en el filtro los campos que son deseables.
        public string Modelo { get; set; } = string.Empty;
    }
}

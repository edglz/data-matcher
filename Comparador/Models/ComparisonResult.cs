using System.Collections.Generic;

namespace Comparador.Models
{
    /// <summary>
    /// Representa el resultado de una comparación entre dos archivos
    /// </summary>
    public class ComparisonResult
    {
        /// <summary>
        /// Total de registros procesados del archivo A
        /// </summary>
        public int TotalRecordsA { get; set; }

        /// <summary>
        /// Total de registros procesados del archivo B
        /// </summary>
        public int TotalRecordsB { get; set; }

        /// <summary>
        /// Número de registros que hicieron match
        /// </summary>
        public int MatchCount { get; set; }

        /// <summary>
        /// Registros del archivo A que no se encontraron en el archivo B
        /// </summary>
        public List<Row> NotFoundInB { get; set; } = new List<Row>();

        /// <summary>
        /// Registros del archivo B que no se encontraron en el archivo A
        /// </summary>
        public List<Row> NotFoundInA { get; set; } = new List<Row>();

        /// <summary>
        /// Registros modificados (después de aplicar las reglas de transferencia)
        /// </summary>
        public List<Row> ModifiedRecords { get; set; } = new List<Row>();
    }
}

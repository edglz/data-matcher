namespace Comparador.Models
{
    /// <summary>
    /// Representa una regla para hacer match entre dos archivos
    /// </summary>
    public class MatchRule
    {
        /// <summary>
        /// Nombre de la columna en el archivo A
        /// </summary>
        public string ColumnA { get; set; }

        /// <summary>
        /// Nombre de la columna en el archivo B
        /// </summary>
        public string ColumnB { get; set; }

        /// <summary>
        /// Indica si la comparación debe ser case-sensitive
        /// </summary>
        public bool CaseSensitive { get; set; } = false;

        public override string ToString()
        {
            return $"{ColumnA} = {ColumnB}";
        }
    }
}

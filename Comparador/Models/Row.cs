using System.Collections.Generic;

namespace Comparador.Models
{
    /// <summary>
    /// Representa una fila de datos de un archivo de texto
    /// </summary>
    public class Row
    {
        /// <summary>
        /// Índice de la fila en el archivo original
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Valores de cada columna
        /// </summary>
        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Contenido original de la fila
        /// </summary>
        public string OriginalContent { get; set; }
    }
}

namespace Comparador.Models
{
    /// <summary>
    /// Representa una regla para transferir datos entre dos archivos
    /// </summary>
    public class TransferRule
    {
        /// <summary>
        /// Indica si el origen es el archivo A (true) o el archivo B (false)
        /// </summary>
        public bool IsSourceA { get; set; }

        /// <summary>
        /// Nombre de la columna de origen
        /// </summary>
        public string SourceColumn { get; set; }

        /// <summary>
        /// Nombre de la columna de destino
        /// </summary>
        public string DestinationColumn { get; set; }

        public override string ToString()
        {
            string source = IsSourceA ? "A" : "B";
            string destination = IsSourceA ? "B" : "A";
            return $"{source}.{SourceColumn} → {destination}.{DestinationColumn}";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Comparador.Models;

namespace Comparador.Services
{
    /// <summary>
    /// Interfaz para el servicio de comparación de registros
    /// </summary>
    public interface IRecordComparerService
    {
        /// <summary>
        /// Carga el archivo A
        /// </summary>
        /// <param name="path">Ruta del archivo</param>
        /// <param name="separator">Separador de columnas</param>
        /// <returns>Lista de encabezados detectados</returns>
        Task<List<string>> LoadFileA(string path, char separator);

        /// <summary>
        /// Carga el archivo B
        /// </summary>
        /// <param name="path">Ruta del archivo</param>
        /// <param name="separator">Separador de columnas</param>
        /// <returns>Lista de encabezados detectados</returns>
        Task<List<string>> LoadFileB(string path, char separator);

        /// <summary>
        /// Establece las reglas para hacer match entre archivos
        /// </summary>
        /// <param name="rules">Lista de reglas</param>
        void SetMatchRules(List<MatchRule> rules);

        /// <summary>
        /// Establece las reglas para transferir datos entre archivos
        /// </summary>
        /// <param name="rules">Lista de reglas</param>
        void SetTransferRules(List<TransferRule> rules);

        /// <summary>
        /// Realiza la comparación y transferencia de datos
        /// </summary>
        /// <param name="startIndex">Índice desde donde comenzar</param>
        /// <param name="cancellationToken">Token para cancelar la operación</param>
        /// <param name="progress">Callback para reportar progreso</param>
        /// <returns>Resultado de la comparación</returns>
        Task<ComparisonResult> CompareAsync(int startIndex, CancellationToken cancellationToken, IProgress<(int current, int total)> progress);

        /// <summary>
        /// Exporta los resultados al archivo especificado
        /// </summary>
        /// <param name="path">Ruta del archivo</param>
        /// <param name="isFileA">Indica si se exporta el archivo A (true) o B (false)</param>
        /// <returns>Tarea asíncrona</returns>
        Task ExportResults(string path, bool isFileA);

        /// <summary>
        /// Exporta los registros no encontrados
        /// </summary>
        /// <param name="path">Ruta del archivo</param>
        /// <param name="isFileA">Indica si se exportan los no encontrados en A (true) o en B (false)</param>
        /// <returns>Tarea asíncrona</returns>
        Task ExportNotFound(string path, bool isFileA);

        /// <summary>
        /// Obtiene una vista previa de los datos del archivo A
        /// </summary>
        /// <param name="count">Número de filas a obtener</param>
        /// <returns>Lista de filas</returns>
        List<Row> GetFileAPreview(int count);

        /// <summary>
        /// Obtiene una vista previa de los datos del archivo B
        /// </summary>
        /// <param name="count">Número de filas a obtener</param>
        /// <returns>Lista de filas</returns>
        List<Row> GetFileBPreview(int count);

        /// <summary>
        /// Obtiene los encabezados del archivo A
        /// </summary>
        List<string> GetFileAHeaders();

        /// <summary>
        /// Obtiene los encabezados del archivo B
        /// </summary>
        List<string> GetFileBHeaders();
    }
}

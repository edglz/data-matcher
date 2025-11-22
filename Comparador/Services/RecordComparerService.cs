using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Comparador.Models;

namespace Comparador.Services
{
    /// <summary>
    /// Implementación del servicio de comparación de registros
    /// </summary>
    public class RecordComparerService : IRecordComparerService
    {
        private List<Row> _fileARows = new List<Row>();
        private List<Row> _fileBRows = new List<Row>();
        private List<string> _fileAHeaders = new List<string>();
        private List<string> _fileBHeaders = new List<string>();
        private List<MatchRule> _matchRules = new List<MatchRule>();
        private List<TransferRule> _transferRules = new List<TransferRule>();
        private ComparisonResult _lastResult = new ComparisonResult();
        private char _separatorA;
        private char _separatorB;
        private string _fileAPath;
        private string _fileBPath;

        /// <summary>
        /// Carga el archivo A
        /// </summary>
        public async Task<List<string>> LoadFileA(string path, char separator)
        {
            _fileAPath = path;
            _separatorA = separator;
            _fileARows.Clear();
            _fileAHeaders.Clear();

            using (var reader = new StreamReader(path))
            {
                // Leer encabezados
                var headerLine = await reader.ReadLineAsync();
                if (headerLine != null)
                {
                    _fileAHeaders = headerLine.Split(separator).ToList();
                }

                // Leer primeras filas para vista previa
                int index = 0;
                while (!reader.EndOfStream && index < 100)
                {
                    var line = await reader.ReadLineAsync();
                    if (line != null)
                    {
                        var values = line.Split(separator);
                        var row = new Row
                        {
                            Index = index,
                            OriginalContent = line
                        };

                        for (int i = 0; i < Math.Min(values.Length, _fileAHeaders.Count); i++)
                        {
                            row.Values[_fileAHeaders[i]] = values[i];
                        }

                        _fileARows.Add(row);
                        index++;
                    }
                }
            }

            return _fileAHeaders;
        }

        /// <summary>
        /// Carga el archivo B
        /// </summary>
        public async Task<List<string>> LoadFileB(string path, char separator)
        {
            _fileBPath = path;
            _separatorB = separator;
            _fileBRows.Clear();
            _fileBHeaders.Clear();

            using (var reader = new StreamReader(path))
            {
                // Leer encabezados
                var headerLine = await reader.ReadLineAsync();
                if (headerLine != null)
                {
                    _fileBHeaders = headerLine.Split(separator).ToList();
                }

                // Leer primeras filas para vista previa
                int index = 0;
                while (!reader.EndOfStream && index < 100)
                {
                    var line = await reader.ReadLineAsync();
                    if (line != null)
                    {
                        var values = line.Split(separator);
                        var row = new Row
                        {
                            Index = index,
                            OriginalContent = line
                        };

                        for (int i = 0; i < Math.Min(values.Length, _fileBHeaders.Count); i++)
                        {
                            row.Values[_fileBHeaders[i]] = values[i];
                        }

                        _fileBRows.Add(row);
                        index++;
                    }
                }
            }

            return _fileBHeaders;
        }

        /// <summary>
        /// Establece las reglas para hacer match entre archivos
        /// </summary>
        public void SetMatchRules(List<MatchRule> rules)
        {
            _matchRules = rules ?? new List<MatchRule>();
        }

        /// <summary>
        /// Establece las reglas para transferir datos entre archivos
        /// </summary>
        public void SetTransferRules(List<TransferRule> rules)
        {
            _transferRules = rules ?? new List<TransferRule>();
        }

        /// <summary>
        /// Realiza la comparación y transferencia de datos
        /// </summary>
        public async Task<ComparisonResult> CompareAsync(int startIndex, CancellationToken cancellationToken, IProgress<(int current, int total)> progress)
        {
            var result = new ComparisonResult();

            // Diccionario para acceso rápido a los registros de B
            var fileBDictionary = new Dictionary<string, List<Row>>();

            // Leer archivo B completo y construir diccionario
            using (var reader = new StreamReader(_fileBPath))
            {
                // Saltar encabezados
                await reader.ReadLineAsync();

                int index = 0;
                while (!reader.EndOfStream)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return result;
                    }

                    var line = await reader.ReadLineAsync();
                    if (line != null)
                    {
                        var values = line.Split(_separatorB);
                        var row = new Row
                        {
                            Index = index,
                            OriginalContent = line
                        };

                        for (int i = 0; i < Math.Min(values.Length, _fileBHeaders.Count); i++)
                        {
                            row.Values[_fileBHeaders[i]] = values[i];
                        }

                        // Construir clave compuesta para búsqueda
                        string key = BuildMatchKey(row, false);

                        if (!fileBDictionary.ContainsKey(key))
                        {
                            fileBDictionary[key] = new List<Row>();
                        }

                        fileBDictionary[key].Add(row);
                        index++;
                    }
                }

                result.TotalRecordsB = index;
            }

            // Procesar archivo A y aplicar reglas de transferencia
            using (var reader = new StreamReader(_fileAPath))
            {
                // Saltar encabezados
                await reader.ReadLineAsync();

                // Saltar hasta el índice de inicio
                for (int i = 0; i < startIndex; i++)
                {
                    await reader.ReadLineAsync();
                }

                int index = startIndex;
                int totalLines = File.ReadLines(_fileAPath).Count() - 1; // Restar encabezados

                while (!reader.EndOfStream)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return result;
                    }

                    var line = await reader.ReadLineAsync();
                    if (line != null)
                    {
                        var values = line.Split(_separatorA);
                        var rowA = new Row
                        {
                            Index = index,
                            OriginalContent = line
                        };

                        for (int i = 0; i < Math.Min(values.Length, _fileAHeaders.Count); i++)
                        {
                            rowA.Values[_fileAHeaders[i]] = values[i];
                        }

                        // Construir clave compuesta para búsqueda
                        string key = BuildMatchKey(rowA, true);

                        // Buscar en el diccionario de B
                        if (fileBDictionary.TryGetValue(key, out var matchingRows))
                        {
                            result.MatchCount++;

                            // Aplicar reglas de transferencia
                            foreach (var rowB in matchingRows)
                            {
                                ApplyTransferRules(rowA, rowB);
                                result.ModifiedRecords.Add(rowB);
                            }
                        }
                        else
                        {
                            result.NotFoundInB.Add(rowA);
                        }

                        index++;
                        progress?.Report((index - startIndex, totalLines - startIndex));
                    }
                }

                result.TotalRecordsA = index;
            }

            // Encontrar registros en B que no están en A
            var allKeysA = new HashSet<string>();
            using (var reader = new StreamReader(_fileAPath))
            {
                // Saltar encabezados
                await reader.ReadLineAsync();

                while (!reader.EndOfStream)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return result;
                    }

                    var line = await reader.ReadLineAsync();
                    if (line != null)
                    {
                        var values = line.Split(_separatorA);
                        var row = new Row
                        {
                            OriginalContent = line
                        };

                        for (int i = 0; i < Math.Min(values.Length, _fileAHeaders.Count); i++)
                        {
                            row.Values[_fileAHeaders[i]] = values[i];
                        }

                        string key = BuildMatchKey(row, true);
                        allKeysA.Add(key);
                    }
                }
            }

            foreach (var entry in fileBDictionary)
            {
                if (!allKeysA.Contains(entry.Key))
                {
                    result.NotFoundInA.AddRange(entry.Value);
                }
            }

            _lastResult = result;
            return result;
        }

        /// <summary>
        /// Exporta los resultados al archivo especificado
        /// </summary>
        public async Task ExportResults(string path, bool isFileA)
        {
            using (var writer = new StreamWriter(path))
            {
                // Escribir encabezados
                var headers = isFileA ? _fileAHeaders : _fileBHeaders;
                await writer.WriteLineAsync(string.Join(_separatorA.ToString(), headers));

                // Escribir filas modificadas
                foreach (var row in _lastResult.ModifiedRecords)
                {
                    var values = new List<string>();
                    foreach (var header in headers)
                    {
                        values.Add(row.Values.ContainsKey(header) ? row.Values[header] : string.Empty);
                    }

                    await writer.WriteLineAsync(string.Join(_separatorA.ToString(), values));
                }
            }
        }

        /// <summary>
        /// Exporta los registros no encontrados
        /// </summary>
        public async Task ExportNotFound(string path, bool isFileA)
        {
            using (var writer = new StreamWriter(path))
            {
                // Escribir encabezados
                var headers = isFileA ? _fileAHeaders : _fileBHeaders;
                await writer.WriteLineAsync(string.Join(_separatorA.ToString(), headers));

                // Escribir filas no encontradas
                var notFoundRows = isFileA ? _lastResult.NotFoundInB : _lastResult.NotFoundInA;
                foreach (var row in notFoundRows)
                {
                    var values = new List<string>();
                    foreach (var header in headers)
                    {
                        values.Add(row.Values.ContainsKey(header) ? row.Values[header] : string.Empty);
                    }

                    await writer.WriteLineAsync(string.Join(_separatorA.ToString(), values));
                }
            }
        }

        /// <summary>
        /// Obtiene una vista previa de los datos del archivo A
        /// </summary>
        public List<Row> GetFileAPreview(int count)
        {
            return _fileARows.Take(count).ToList();
        }

        /// <summary>
        /// Obtiene una vista previa de los datos del archivo B
        /// </summary>
        public List<Row> GetFileBPreview(int count)
        {
            return _fileBRows.Take(count).ToList();
        }

        /// <summary>
        /// Obtiene los encabezados del archivo A
        /// </summary>
        public List<string> GetFileAHeaders()
        {
            return _fileAHeaders;
        }

        /// <summary>
        /// Obtiene los encabezados del archivo B
        /// </summary>
        public List<string> GetFileBHeaders()
        {
            return _fileBHeaders;
        }

        /// <summary>
        /// Construye una clave compuesta para hacer match entre registros
        /// </summary>
        private string BuildMatchKey(Row row, bool isFileA)
        {
            var keyBuilder = new StringBuilder();

            foreach (var rule in _matchRules)
            {
                string columnName = isFileA ? rule.ColumnA : rule.ColumnB;
                if (row.Values.TryGetValue(columnName, out string value))
                {
                    string valueToAdd = rule.CaseSensitive ? value : value.ToLower();
                    keyBuilder.Append(valueToAdd).Append('|');
                }
                else
                {
                    keyBuilder.Append("|");
                }
            }

            return keyBuilder.ToString();
        }

        /// <summary>
        /// Aplica las reglas de transferencia entre dos registros
        /// </summary>
        private void ApplyTransferRules(Row rowA, Row rowB)
        {
            foreach (var rule in _transferRules)
            {
                Row sourceRow = rule.IsSourceA ? rowA : rowB;
                Row destRow = rule.IsSourceA ? rowB : rowA;

                if (sourceRow.Values.TryGetValue(rule.SourceColumn, out string value))
                {
                    destRow.Values[rule.DestinationColumn] = value;
                }
            }
        }
    }
}

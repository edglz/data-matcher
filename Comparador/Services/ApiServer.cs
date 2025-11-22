using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Comparador.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace Comparador.Services
{
    /// <summary>
    /// Servidor API local basado en Kestrel
    /// </summary>
    public class ApiServer
    {
        private readonly IRecordComparerService _comparerService;
        private IHost _host;
        private readonly int _port;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiServer(IRecordComparerService comparerService, int port = 5000)
        {
            _comparerService = comparerService;
            _port = port;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Inicia el servidor API
        /// </summary>
        public async Task StartAsync()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel()
                        .UseUrls($"http://localhost:{_port}")
                        .Configure(app =>
                        {
                            app.UseRouting();

                            app.UseEndpoints(endpoints =>
                            {
                                // Endpoint raíz para validar que el servidor está funcionando
                                endpoints.MapGet("/", HandleRootAsync);

                                // Endpoint para subir archivos
                                endpoints.MapPost("/upload", HandleUploadAsync);

                                // Endpoint para comparar archivos
                                endpoints.MapPost("/compare", HandleCompareAsync);

                                // Endpoint para obtener estadísticas
                                endpoints.MapGet("/stats", HandleStatsAsync);
                            });
                        });
                })
                .Build();

            await _host.StartAsync();
        }

        /// <summary>
        /// Detiene el servidor API
        /// </summary>
        public async Task StopAsync()
        {
            if (_host != null)
            {
                await _host.StopAsync();
                await _host.WaitForShutdownAsync();
            }
        }

        /// <summary>
        /// Maneja la solicitud para subir archivos
        /// </summary>
        private async Task HandleUploadAsync(HttpContext context)
        {
            if (!context.Request.HasFormContentType)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Se esperaba un formulario multipart");
                return;
            }

            var form = await context.Request.ReadFormAsync();
            var files = form.Files;

            if (files.Count == 0)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("No se encontraron archivos");
                return;
            }

            var fileType = context.Request.Query["type"].ToString();
            var separator = context.Request.Query["separator"].ToString();

            if (string.IsNullOrEmpty(fileType) || string.IsNullOrEmpty(separator))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Se requieren los parámetros 'type' y 'separator'");
                return;
            }

            if (fileType != "A" && fileType != "B")
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("El parámetro 'type' debe ser 'A' o 'B'");
                return;
            }

            if (separator.Length != 1)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("El separador debe ser un único carácter");
                return;
            }

            var file = files[0];
            var tempPath = Path.GetTempFileName();

            try
            {
                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                List<string> headers;
                if (fileType == "A")
                {
                    headers = await _comparerService.LoadFileA(tempPath, separator[0]);
                }
                else
                {
                    headers = await _comparerService.LoadFileB(tempPath, separator[0]);
                }

                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    Success = true,
                    Message = $"Archivo {fileType} cargado correctamente",
                    Headers = headers
                }, _jsonOptions));
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"Error al procesar el archivo: {ex.Message}");
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        /// <summary>
        /// Maneja la solicitud para comparar archivos
        /// </summary>
        private async Task HandleCompareAsync(HttpContext context)
        {
            try
            {
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
                var requestBody = await reader.ReadToEndAsync();
                var options = JsonSerializer.Deserialize<CompareOptions>(requestBody, _jsonOptions);

                if (options == null)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Datos de solicitud inválidos");
                    return;
                }

                // Configurar reglas
                _comparerService.SetMatchRules(options.MatchRules);
                _comparerService.SetTransferRules(options.TransferRules);

                // Realizar comparación
                var progress = new Progress<(int current, int total)>();
                var cancellationTokenSource = new CancellationTokenSource();
                var result = await _comparerService.CompareAsync(options.StartIndex, cancellationTokenSource.Token, progress);

                // Devolver resultado
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    Success = true,
                    TotalRecordsA = result.TotalRecordsA,
                    TotalRecordsB = result.TotalRecordsB,
                    MatchCount = result.MatchCount,
                    NotFoundInACount = result.NotFoundInA.Count,
                    NotFoundInBCount = result.NotFoundInB.Count
                }, _jsonOptions));
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"Error durante la comparación: {ex.Message}");
            }
        }

        /// <summary>
        /// Maneja la solicitud al endpoint raíz
        /// </summary>
        private async Task HandleRootAsync(HttpContext context)
        {
            try
            {
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    Status = "OK",
                    Message = "API del Comparador de Archivos funcionando correctamente",
                    Version = "1.0",
                    Endpoints = new[]
                    {
                        new { Path = "/", Method = "GET", Description = "Información sobre la API" },
                        new { Path = "/upload", Method = "POST", Description = "Subir archivos para comparación" },
                        new { Path = "/compare", Method = "POST", Description = "Realizar comparación entre archivos" },
                        new { Path = "/stats", Method = "GET", Description = "Obtener estadísticas de los archivos cargados" }
                    },
                    Timestamp = DateTime.Now
                }, _jsonOptions));
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"Error en el endpoint raíz: {ex.Message}");
            }
        }

        /// <summary>
        /// Maneja la solicitud para obtener estadísticas
        /// </summary>
        private async Task HandleStatsAsync(HttpContext context)
        {
            try
            {
                var fileAHeaders = _comparerService.GetFileAHeaders();
                var fileBHeaders = _comparerService.GetFileBHeaders();

                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    FileALoaded = fileAHeaders.Count > 0,
                    FileBLoaded = fileBHeaders.Count > 0,
                    FileAHeaders = fileAHeaders,
                    FileBHeaders = fileBHeaders
                }, _jsonOptions));
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"Error al obtener estadísticas: {ex.Message}");
            }
        }

        /// <summary>
        /// Opciones para la comparación de archivos
        /// </summary>
        private class CompareOptions
        {
            public List<MatchRule> MatchRules { get; set; }
            public List<TransferRule> TransferRules { get; set; }
            public int StartIndex { get; set; }
        }
    }
}

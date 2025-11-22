# Comparador de Archivos

Aplicación de escritorio en WPF + C# (.NET 10) que permite comparar dos archivos TXT con un separador configurable, seleccionar campos para hacer match, y copiar información de un archivo a otro según las reglas definidas por el usuario.

## Características principales

- **Importación de archivos**: Carga de archivos TXT con separadores configurables (coma, punto y coma, tabulación, pipe).
- **Detección automática de encabezados**: Identifica las columnas de los archivos.
- **Vista previa de datos**: Muestra los datos de ambos archivos en DataGrids.
- **Configuración de reglas de match**: Permite seleccionar qué columnas usar para hacer match entre ambos archivos.
- **Configuración de reglas de transferencia**: Permite seleccionar qué columnas copiar de un archivo a otro.
- **Proceso de comparación optimizado**: Utiliza estructuras de datos eficientes para comparar grandes volúmenes de información.
- **Control de progreso**: Barra de progreso, botones para pausar/continuar y opción para empezar desde un registro específico.
- **Exportación de resultados**: Permite exportar el archivo final modificado y los registros no encontrados.
- **API REST integrada**: Servidor Kestrel opcional que expone endpoints para realizar operaciones de comparación desde otros sistemas.

## Estructura del proyecto

El proyecto sigue el patrón de arquitectura MVVM (Model-View-ViewModel):

- **Models**: Contiene las clases de datos como `Row`, `MatchRule`, `TransferRule` y `ComparisonResult`.
- **ViewModels**: Contiene la lógica de presentación, principalmente `MainViewModel`.
- **Views**: Contiene las vistas XAML, principalmente `MainWindow.xaml`.
- **Services**: Contiene los servicios de la aplicación, principalmente `RecordComparerService` y `ApiServer`.
- **Converters**: Contiene convertidores de valores para el binding en XAML.

## Uso de la aplicación

1. **Carga de archivos**:
   - Haz clic en "Cargar Archivo A" y selecciona un archivo TXT.
   - Haz clic en "Cargar Archivo B" y selecciona otro archivo TXT.
   - Configura el separador adecuado para cada archivo.

2. **Configuración de reglas de match**:
   - Selecciona una columna del Archivo A y una columna del Archivo B.
   - Haz clic en "Agregar Match" para crear una regla de match.
   - Puedes agregar múltiples reglas para hacer match por múltiples campos.

3. **Configuración de reglas de transferencia**:
   - Selecciona si quieres transferir datos de A a B o de B a A.
   - Selecciona la columna de origen y la columna de destino.
   - Haz clic en "Agregar Transferencia" para crear una regla de transferencia.

4. **Ejecución de la comparación**:
   - Opcionalmente, configura desde qué registro empezar.
   - Haz clic en "Iniciar Comparación".
   - Puedes pausar y continuar la comparación si es necesario.

5. **Exportación de resultados**:
   - Una vez completada la comparación, haz clic en "Exportar Resultado Final" para guardar el archivo modificado.
   - Haz clic en "Exportar No Encontrados" para guardar los registros que no hicieron match.

6. **API REST (opcional)**:
   - Configura el puerto para el servidor API.
   - Haz clic en "Iniciar API" para iniciar el servidor.
   - Utiliza los endpoints disponibles:
     - `POST /upload`: Para subir archivos.
     - `POST /compare`: Para realizar comparaciones.
     - `GET /stats`: Para obtener estadísticas.

## Requisitos del sistema

- Windows 10 o superior
- .NET 10 Runtime

## Desarrollo

Para desarrollar o modificar la aplicación:

1. Clona el repositorio.
2. Abre la solución en Visual Studio 2022 o superior.
3. Restaura los paquetes NuGet.
4. Compila y ejecuta la aplicación.

## Paquetes NuGet utilizados

- CommunityToolkit.Mvvm
- Microsoft.AspNetCore.OpenApi
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.DependencyInjection

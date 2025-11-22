using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Comparador.Models;
using Comparador.Services;
using Microsoft.Win32;

namespace Comparador.ViewModels
{
    /// <summary>
    /// ViewModel principal de la aplicación
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IRecordComparerService _comparerService;
        private CancellationTokenSource _cancellationTokenSource;

        #region Propiedades

        private string _fileAPath;
        public string FileAPath
        {
            get => _fileAPath;
            set => SetProperty(ref _fileAPath, value);
        }

        private string _fileBPath;
        public string FileBPath
        {
            get => _fileBPath;
            set => SetProperty(ref _fileBPath, value);
        }

        private char _selectedSeparatorA = ',';
        public char SelectedSeparatorA
        {
            get => _selectedSeparatorA;
            set => SetProperty(ref _selectedSeparatorA, value);
        }

        private char _selectedSeparatorB = ',';
        public char SelectedSeparatorB
        {
            get => _selectedSeparatorB;
            set => SetProperty(ref _selectedSeparatorB, value);
        }

        private ObservableCollection<string> _fileAHeaders = new ObservableCollection<string>();
        public ObservableCollection<string> FileAHeaders
        {
            get => _fileAHeaders;
            set => SetProperty(ref _fileAHeaders, value);
        }

        private ObservableCollection<string> _fileBHeaders = new ObservableCollection<string>();
        public ObservableCollection<string> FileBHeaders
        {
            get => _fileBHeaders;
            set => SetProperty(ref _fileBHeaders, value);
        }

        private ObservableCollection<Row> _fileAPreview = new ObservableCollection<Row>();
        public ObservableCollection<Row> FileAPreview
        {
            get => _fileAPreview;
            set => SetProperty(ref _fileAPreview, value);
        }

        private ObservableCollection<Row> _fileBPreview = new ObservableCollection<Row>();
        public ObservableCollection<Row> FileBPreview
        {
            get => _fileBPreview;
            set => SetProperty(ref _fileBPreview, value);
        }

        private ObservableCollection<MatchRule> _matchRules = new ObservableCollection<MatchRule>();
        public ObservableCollection<MatchRule> MatchRules
        {
            get => _matchRules;
            set => SetProperty(ref _matchRules, value);
        }

        private ObservableCollection<TransferRule> _transferRules = new ObservableCollection<TransferRule>();
        public ObservableCollection<TransferRule> TransferRules
        {
            get => _transferRules;
            set => SetProperty(ref _transferRules, value);
        }

        private string _selectedFileAColumn;
        public string SelectedFileAColumn
        {
            get => _selectedFileAColumn;
            set => SetProperty(ref _selectedFileAColumn, value);
        }

        private string _selectedFileBColumn;
        public string SelectedFileBColumn
        {
            get => _selectedFileBColumn;
            set => SetProperty(ref _selectedFileBColumn, value);
        }

        private string _selectedSourceColumn;
        public string SelectedSourceColumn
        {
            get => _selectedSourceColumn;
            set => SetProperty(ref _selectedSourceColumn, value);
        }

        private string _selectedDestinationColumn;
        public string SelectedDestinationColumn
        {
            get => _selectedDestinationColumn;
            set => SetProperty(ref _selectedDestinationColumn, value);
        }

        private bool _isSourceA = true;
        public bool IsSourceA
        {
            get => _isSourceA;
            set
            {
                if (SetProperty(ref _isSourceA, value))
                {
                    UpdateSourceColumns();
                    UpdateDestinationColumns();
                }
            }
        }

        private ObservableCollection<string> _sourceColumns = new ObservableCollection<string>();
        public ObservableCollection<string> SourceColumns
        {
            get => _sourceColumns;
            set => SetProperty(ref _sourceColumns, value);
        }

        private ObservableCollection<string> _destinationColumns = new ObservableCollection<string>();
        public ObservableCollection<string> DestinationColumns
        {
            get => _destinationColumns;
            set => SetProperty(ref _destinationColumns, value);
        }

        private int _startIndex;
        public int StartIndex
        {
            get => _startIndex;
            set => SetProperty(ref _startIndex, value);
        }

        private int _progress;
        public int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        private int _totalRecords;
        public int TotalRecords
        {
            get => _totalRecords;
            set => SetProperty(ref _totalRecords, value);
        }

        private int _processedRecords;
        public int ProcessedRecords
        {
            get => _processedRecords;
            set => SetProperty(ref _processedRecords, value);
        }

        private int _matchCount;
        public int MatchCount
        {
            get => _matchCount;
            set => SetProperty(ref _matchCount, value);
        }

        private int _notFoundCount;
        public int NotFoundCount
        {
            get => _notFoundCount;
            set => SetProperty(ref _notFoundCount, value);
        }

        private bool _isComparing;
        public bool IsComparing
        {
            get => _isComparing;
            set => SetProperty(ref _isComparing, value);
        }

        private bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            set => SetProperty(ref _isPaused, value);
        }

        private bool _isFileALoaded;
        public bool IsFileALoaded
        {
            get => _isFileALoaded;
            set => SetProperty(ref _isFileALoaded, value);
        }

        private bool _isFileBLoaded;
        public bool IsFileBLoaded
        {
            get => _isFileBLoaded;
            set => SetProperty(ref _isFileBLoaded, value);
        }

        private ComparisonResult _lastResult;
        public ComparisonResult LastResult
        {
            get => _lastResult;
            set => SetProperty(ref _lastResult, value);
        }

        private bool _isApiServerRunning;
        public bool IsApiServerRunning
        {
            get => _isApiServerRunning;
            set => SetProperty(ref _isApiServerRunning, value);
        }

        private int _apiPort = 5000;
        public int ApiPort
        {
            get => _apiPort;
            set => SetProperty(ref _apiPort, value);
        }

        private ApiServer _apiServer;

        #endregion

        #region Comandos

        public ICommand LoadFileACommand { get; }
        public ICommand LoadFileBCommand { get; }
        public ICommand AddMatchRuleCommand { get; }
        public ICommand RemoveMatchRuleCommand { get; }
        public ICommand AddTransferRuleCommand { get; }
        public ICommand RemoveTransferRuleCommand { get; }
        public ICommand StartComparisonCommand { get; }
        public ICommand PauseComparisonCommand { get; }
        public ICommand ResumeComparisonCommand { get; }
        public ICommand ExportResultsCommand { get; }
        public ICommand ExportNotFoundCommand { get; }
        public ICommand StartApiServerCommand { get; }
        public ICommand StopApiServerCommand { get; }

        #endregion

        public MainViewModel(IRecordComparerService comparerService)
        {
            _comparerService = comparerService;

            // Inicializar comandos
            LoadFileACommand = new RelayCommand(LoadFileA);
            LoadFileBCommand = new RelayCommand(LoadFileB);
            AddMatchRuleCommand = new RelayCommand(AddMatchRule, CanAddMatchRule);
            RemoveMatchRuleCommand = new RelayCommand<MatchRule>(RemoveMatchRule);
            AddTransferRuleCommand = new RelayCommand(AddTransferRule, CanAddTransferRule);
            RemoveTransferRuleCommand = new RelayCommand<TransferRule>(RemoveTransferRule);
            StartComparisonCommand = new RelayCommand(StartComparison, CanStartComparison);
            PauseComparisonCommand = new RelayCommand(PauseComparison, () => IsComparing && !IsPaused);
            ResumeComparisonCommand = new RelayCommand(ResumeComparison, () => IsComparing && IsPaused);
            ExportResultsCommand = new RelayCommand(ExportResults, () => LastResult != null);
            ExportNotFoundCommand = new RelayCommand(ExportNotFound, () => LastResult != null);
            StartApiServerCommand = new RelayCommand(StartApiServer, () => !IsApiServerRunning);
            StopApiServerCommand = new RelayCommand(StopApiServer, () => IsApiServerRunning);
        }

        #region Métodos

        /// <summary>
        /// Carga el archivo A
        /// </summary>
        private async void LoadFileA()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*",
                Title = "Seleccionar archivo A"
            };

            if (dialog.ShowDialog() == true)
            {
                FileAPath = dialog.FileName;

                try
                {
                    var headers = await _comparerService.LoadFileA(FileAPath, SelectedSeparatorA);
                    FileAHeaders = new ObservableCollection<string>(headers);
                    FileAPreview = new ObservableCollection<Row>(_comparerService.GetFileAPreview(10));
                    IsFileALoaded = true;

                    // Actualizar columnas disponibles
                    UpdateSourceColumns();
                    UpdateDestinationColumns();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar el archivo A: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Carga el archivo B
        /// </summary>
        private async void LoadFileB()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*",
                Title = "Seleccionar archivo B"
            };

            if (dialog.ShowDialog() == true)
            {
                FileBPath = dialog.FileName;

                try
                {
                    var headers = await _comparerService.LoadFileB(FileBPath, SelectedSeparatorB);
                    FileBHeaders = new ObservableCollection<string>(headers);
                    FileBPreview = new ObservableCollection<Row>(_comparerService.GetFileBPreview(10));
                    IsFileBLoaded = true;

                    // Actualizar columnas disponibles
                    UpdateSourceColumns();
                    UpdateDestinationColumns();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar el archivo B: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Agrega una regla de match
        /// </summary>
        private void AddMatchRule()
        {
            var rule = new MatchRule
            {
                ColumnA = SelectedFileAColumn,
                ColumnB = SelectedFileBColumn
            };

            MatchRules.Add(rule);
        }

        /// <summary>
        /// Verifica si se puede agregar una regla de match
        /// </summary>
        private bool CanAddMatchRule()
        {
            return IsFileALoaded && IsFileBLoaded &&
                   !string.IsNullOrEmpty(SelectedFileAColumn) &&
                   !string.IsNullOrEmpty(SelectedFileBColumn);
        }

        /// <summary>
        /// Elimina una regla de match
        /// </summary>
        private void RemoveMatchRule(MatchRule rule)
        {
            if (rule != null)
            {
                MatchRules.Remove(rule);
            }
        }

        /// <summary>
        /// Agrega una regla de transferencia
        /// </summary>
        private void AddTransferRule()
        {
            var rule = new TransferRule
            {
                IsSourceA = IsSourceA,
                SourceColumn = SelectedSourceColumn,
                DestinationColumn = SelectedDestinationColumn
            };

            TransferRules.Add(rule);
        }

        /// <summary>
        /// Verifica si se puede agregar una regla de transferencia
        /// </summary>
        private bool CanAddTransferRule()
        {
            return IsFileALoaded && IsFileBLoaded &&
                   !string.IsNullOrEmpty(SelectedSourceColumn) &&
                   !string.IsNullOrEmpty(SelectedDestinationColumn);
        }

        /// <summary>
        /// Elimina una regla de transferencia
        /// </summary>
        private void RemoveTransferRule(TransferRule rule)
        {
            if (rule != null)
            {
                TransferRules.Remove(rule);
            }
        }

        /// <summary>
        /// Inicia la comparación
        /// </summary>
        private async void StartComparison()
        {
            IsComparing = true;
            IsPaused = false;
            Progress = 0;
            ProcessedRecords = 0;
            MatchCount = 0;
            NotFoundCount = 0;

            _comparerService.SetMatchRules(MatchRules.ToList());
            _comparerService.SetTransferRules(TransferRules.ToList());

            _cancellationTokenSource = new CancellationTokenSource();
            var progress = new Progress<(int current, int total)>(ReportProgress);

            try
            {
                var result = await _comparerService.CompareAsync(StartIndex, _cancellationTokenSource.Token, progress);
                LastResult = result;

                MatchCount = result.MatchCount;
                NotFoundCount = result.NotFoundInA.Count + result.NotFoundInB.Count;

                MessageBox.Show("Comparación completada", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (OperationCanceledException)
            {
                // Operación cancelada, no hacer nada
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error durante la comparación: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsComparing = false;
                IsPaused = false;
            }
        }

        /// <summary>
        /// Verifica si se puede iniciar la comparación
        /// </summary>
        private bool CanStartComparison()
        {
            return IsFileALoaded && IsFileBLoaded &&
                   MatchRules.Count > 0 &&
                   !IsComparing;
        }

        /// <summary>
        /// Pausa la comparación
        /// </summary>
        private void PauseComparison()
        {
            IsPaused = true;
            _cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Reanuda la comparación
        /// </summary>
        private void ResumeComparison()
        {
            IsPaused = false;
            StartComparison();
        }

        /// <summary>
        /// Exporta los resultados
        /// </summary>
        private async void ExportResults()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*",
                Title = "Guardar resultados"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await _comparerService.ExportResults(dialog.FileName, false); // Exportar archivo B modificado
                    MessageBox.Show("Resultados exportados correctamente", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar resultados: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Exporta los registros no encontrados
        /// </summary>
        private async void ExportNotFound()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*",
                Title = "Guardar registros no encontrados"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await _comparerService.ExportNotFound(dialog.FileName, true); // Exportar no encontrados en B
                    MessageBox.Show("Registros no encontrados exportados correctamente", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar registros no encontrados: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Actualiza las columnas disponibles para el origen
        /// </summary>
        private void UpdateSourceColumns()
        {
            if (IsSourceA)
            {
                SourceColumns = new ObservableCollection<string>(FileAHeaders);
            }
            else
            {
                SourceColumns = new ObservableCollection<string>(FileBHeaders);
            }

            SelectedSourceColumn = SourceColumns.FirstOrDefault();
        }

        /// <summary>
        /// Actualiza las columnas disponibles para el destino
        /// </summary>
        private void UpdateDestinationColumns()
        {
            if (IsSourceA)
            {
                DestinationColumns = new ObservableCollection<string>(FileBHeaders);
            }
            else
            {
                DestinationColumns = new ObservableCollection<string>(FileAHeaders);
            }

            SelectedDestinationColumn = DestinationColumns.FirstOrDefault();
        }

        /// <summary>
        /// Reporta el progreso de la comparación
        /// </summary>
        private void ReportProgress((int current, int total) progress)
        {
            ProcessedRecords = progress.current;
            TotalRecords = progress.total;
            Progress = progress.total > 0 ? (int)((double)progress.current / progress.total * 100) : 0;
        }

        /// <summary>
        /// Inicia el servidor API
        /// </summary>
        private async void StartApiServer()
        {
            try
            {
                _apiServer = new ApiServer(_comparerService, ApiPort);
                await _apiServer.StartAsync();
                IsApiServerRunning = true;

                MessageBox.Show($"Servidor API iniciado en http://localhost:{ApiPort}", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar el servidor API: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Detiene el servidor API
        /// </summary>
        private async void StopApiServer()
        {
            try
            {
                if (_apiServer != null)
                {
                    await _apiServer.StopAsync();
                    IsApiServerRunning = false;

                    MessageBox.Show("Servidor API detenido", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al detener el servidor API: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }

    /// <summary>
    /// Implementación simple de ICommand
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    /// <summary>
    /// Implementación simple de ICommand con parámetro
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}

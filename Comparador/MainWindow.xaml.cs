using System.Windows;
using Comparador.Services;
using Comparador.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Comparador
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Configurar servicios
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            // Configurar ViewModel
            DataContext = serviceProvider.GetRequiredService<MainViewModel>();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Registrar servicios
            services.AddSingleton<IRecordComparerService, RecordComparerService>();

            // Registrar ViewModels
            services.AddSingleton<MainViewModel>();
        }
    }
}

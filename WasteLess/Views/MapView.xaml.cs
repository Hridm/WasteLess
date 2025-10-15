// File: Views/MapView.xaml.cs
using System.Windows.Controls;
using WasteManagementApp.ViewModels;
using WasteManagementApp.Data;

namespace WasteManagementApp.Views
{
    /// <summary>
    /// Interaction logic for MapView.xaml
    /// </summary>
    public partial class MapView : UserControl
    {
        public MapView()
        {
            InitializeComponent();

            // Initialize with mock data repository
            // In production, this would be injected via dependency injection
            var repository = new MockDisposalLocationRepository();
            DataContext = new MapViewModel(repository);
        }

        // Constructor for testing with custom repository
        public MapView(IDisposalLocationRepository repository)
        {
            InitializeComponent();
            DataContext = new MapViewModel(repository);
        }
    }
}

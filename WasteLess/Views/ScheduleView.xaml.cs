// File: Views/ScheduleView.xaml.cs
using System.Windows.Controls;
using WasteManagementApp.ViewModels;
using WasteManagementApp.Data;

namespace WasteManagementApp.Views
{
    /// <summary>
    /// Interaction logic for ScheduleView.xaml
    /// </summary>
    public partial class ScheduleView : UserControl
    {
        public ScheduleView()
        {
            InitializeComponent();

            // Initialize with mock data repository
            // In production, this would be injected via dependency injection
            var repository = new MockScheduleRepository();
            DataContext = new ScheduleViewModel(repository);
        }

        // Constructor for testing with custom repository
        public ScheduleView(IScheduleRepository repository)
        {
            InitializeComponent();
            DataContext = new ScheduleViewModel(repository);
        }
    }
}
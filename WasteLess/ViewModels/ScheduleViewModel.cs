// File: ViewModels/ScheduleViewModel.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using WasteManagementApp.Data;
using WasteManagementApp.Models;
using WasteManagementApp.Services;
using System.Text;
using System.Threading.Tasks;



namespace WasteManagementApp.ViewModels
{
    /// <summary>
    /// ViewModel for the Schedule View
    /// Implements MVVM pattern with INotifyPropertyChanged
    /// </summary>
    public class ScheduleViewModel : INotifyPropertyChanged
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly ScheduleCalculatorService _calculatorService;

        // Address input
        private string _address;
        private string _suburb;

        // Schedule data
        private ObservableCollection<BinCollectionSchedule> _binSchedules;
        private ObservableCollection<UpcomingCollection> _upcomingCollections;
        private ObservableCollection<BulkyWasteSchedule> _bulkyWasteSchedules;
        private Dictionary<DateTime, List<UpcomingCollection>> _collectionCalendar;

        // Next collection info
        private string _nextRedBinDate;
        private string _nextYellowBinDate;
        private string _nextGreenBinDate;
        private int _daysUntilNextCollection;
        private string _nextCollectionBins;

        // Reminder
        private bool _showReminder;
        private string _reminderMessage;

        // View state
        private string _statusMessage;
        private bool _hasValidSchedule;
        private string _validationMessage;
        private ObservableCollection<string> _availableSuburbs;

        // Selected date for calendar
        private DateTime _selectedDate;

        public ScheduleViewModel(IScheduleRepository scheduleRepository)
        {
            _scheduleRepository = scheduleRepository;
            _calculatorService = new ScheduleCalculatorService(_scheduleRepository);

            // Initialize collections
            _binSchedules = new ObservableCollection<BinCollectionSchedule>();
            _upcomingCollections = new ObservableCollection<UpcomingCollection>();
            _bulkyWasteSchedules = new ObservableCollection<BulkyWasteSchedule>();
            _availableSuburbs = new ObservableCollection<string>();

            // Initialize commands
            LookupScheduleCommand = new RelayCommand(ExecuteLookupSchedule, CanExecuteLookupSchedule);
            ClearAddressCommand = new RelayCommand(ExecuteClearAddress);
            RefreshCommand = new RelayCommand(ExecuteRefresh);

            // Initialize with current date
            _selectedDate = DateTime.Now.Date;

            // Load available suburbs
            LoadAvailableSuburbs();

            StatusMessage = "Enter an address to view collection schedule";
        }

        #region Properties

        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Suburb
        {
            get => _suburb;
            set
            {
                _suburb = value;
                OnPropertyChanged(nameof(Suburb));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ObservableCollection<string> AvailableSuburbs
        {
            get => _availableSuburbs;
            set
            {
                _availableSuburbs = value;
                OnPropertyChanged(nameof(AvailableSuburbs));
            }
        }

        public ObservableCollection<BinCollectionSchedule> BinSchedules
        {
            get => _binSchedules;
            set
            {
                _binSchedules = value;
                OnPropertyChanged(nameof(BinSchedules));
            }
        }

        public ObservableCollection<UpcomingCollection> UpcomingCollections
        {
            get => _upcomingCollections;
            set
            {
                _upcomingCollections = value;
                OnPropertyChanged(nameof(UpcomingCollections));
            }
        }

        public ObservableCollection<BulkyWasteSchedule> BulkyWasteSchedules
        {
            get => _bulkyWasteSchedules;
            set
            {
                _bulkyWasteSchedules = value;
                OnPropertyChanged(nameof(BulkyWasteSchedules));
            }
        }

        public string NextRedBinDate
        {
            get => _nextRedBinDate;
            set
            {
                _nextRedBinDate = value;
                OnPropertyChanged(nameof(NextRedBinDate));
            }
        }

        public string NextYellowBinDate
        {
            get => _nextYellowBinDate;
            set
            {
                _nextYellowBinDate = value;
                OnPropertyChanged(nameof(NextYellowBinDate));
            }
        }

        public string NextGreenBinDate
        {
            get => _nextGreenBinDate;
            set
            {
                _nextGreenBinDate = value;
                OnPropertyChanged(nameof(NextGreenBinDate));
            }
        }

        public int DaysUntilNextCollection
        {
            get => _daysUntilNextCollection;
            set
            {
                _daysUntilNextCollection = value;
                OnPropertyChanged(nameof(DaysUntilNextCollection));
                OnPropertyChanged(nameof(DaysUntilNextCollectionText));
            }
        }

        public string DaysUntilNextCollectionText
        {
            get
            {
                if (DaysUntilNextCollection < 0)
                    return "No schedule found";
                if (DaysUntilNextCollection == 0)
                    return "Collection is TODAY!";
                if (DaysUntilNextCollection == 1)
                    return "Collection is TOMORROW!";
                return $"Collection in {DaysUntilNextCollection} days";
            }
        }

        public string NextCollectionBins
        {
            get => _nextCollectionBins;
            set
            {
                _nextCollectionBins = value;
                OnPropertyChanged(nameof(NextCollectionBins));
            }
        }

        public bool ShowReminder
        {
            get => _showReminder;
            set
            {
                _showReminder = value;
                OnPropertyChanged(nameof(ShowReminder));
            }
        }

        public string ReminderMessage
        {
            get => _reminderMessage;
            set
            {
                _reminderMessage = value;
                OnPropertyChanged(nameof(ReminderMessage));
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public bool HasValidSchedule
        {
            get => _hasValidSchedule;
            set
            {
                _hasValidSchedule = value;
                OnPropertyChanged(nameof(HasValidSchedule));
                OnPropertyChanged(nameof(HasNoSchedule));
            }
        }

        public bool HasNoSchedule => !HasValidSchedule;

        public string ValidationMessage
        {
            get => _validationMessage;
            set
            {
                _validationMessage = value;
                OnPropertyChanged(nameof(ValidationMessage));
            }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
                UpdateCollectionsForSelectedDate();
            }
        }

        #endregion

        #region Commands

        public ICommand LookupScheduleCommand { get; }
        public ICommand ClearAddressCommand { get; }
        public ICommand RefreshCommand { get; }

        private bool CanExecuteLookupSchedule()
        {
            return !string.IsNullOrWhiteSpace(Address) || !string.IsNullOrWhiteSpace(Suburb);
        }

        private void ExecuteLookupSchedule()
        {
            try
            {
                // Lookup by address or suburb
                List<BinCollectionSchedule> schedules;

                if (!string.IsNullOrWhiteSpace(Address))
                {
                    schedules = _calculatorService.GetSchedulesByAddress(Address);
                    StatusMessage = $"Schedule for: {Address}";
                }
                else
                {
                    schedules = _calculatorService.GetSchedulesBySuburb(Suburb);
                    StatusMessage = $"Schedule for suburb: {Suburb}";
                }

                if (schedules.Count == 0)
                {
                    HasValidSchedule = false;
                    ValidationMessage = "No schedule found for this address/suburb. Please try another.";
                    StatusMessage = "No schedule found";
                    ClearScheduleData();
                    return;
                }

                // Validate schedule
                var validation = _calculatorService.ValidateScheduleForAddress(
                    !string.IsNullOrWhiteSpace(Address) ? Address : schedules.First().Address);

                HasValidSchedule = validation.IsValid;
                ValidationMessage = validation.GetValidationMessage();

                // Load schedules
                LoadSchedules(schedules);

                // Calculate next collections
                CalculateNextCollections();

                // Load upcoming collections
                LoadUpcomingCollections();

                // Check for reminders
                CheckReminders();

                // Load bulky waste
                LoadBulkyWasteSchedules();

                StatusMessage = "Schedule loaded successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading schedule: {ex.Message}";
                HasValidSchedule = false;
            }
        }

        private void ExecuteClearAddress()
        {
            Address = string.Empty;
            Suburb = null;
            ClearScheduleData();
            StatusMessage = "Enter an address to view collection schedule";
        }

        private void ExecuteRefresh()
        {
            if (HasValidSchedule)
            {
                ExecuteLookupSchedule();
                StatusMessage = "Schedule refreshed";
            }
        }

        #endregion

        #region Data Loading Methods

        private void LoadAvailableSuburbs()
        {
            // Get all schedules and extract unique suburbs
            var allSchedules = _scheduleRepository.GetAll();
            var suburbs = allSchedules
                .OfType<BinCollectionSchedule>()
                .Select(s => s.Suburb)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            AvailableSuburbs.Clear();
            foreach (var suburb in suburbs)
            {
                AvailableSuburbs.Add(suburb);
            }
        }

        private void LoadSchedules(List<BinCollectionSchedule> schedules)
        {
            BinSchedules.Clear();
            foreach (var schedule in schedules.OrderBy(s => s.BinType))
            {
                BinSchedules.Add(schedule);
            }
        }

        private void CalculateNextCollections()
        {
            if (BinSchedules.Count == 0)
                return;

            var address = BinSchedules.First().Address;
            var now = DateTime.Now;

            // Get all next collections
            var allNext = _calculatorService.GetAllNextCollections(address, now);

            // Update individual bin dates
            if (allNext.ContainsKey("Red"))
                NextRedBinDate = allNext["Red"].ToString("ddd, MMM d");
            else
                NextRedBinDate = "Not scheduled";

            if (allNext.ContainsKey("Yellow"))
                NextYellowBinDate = allNext["Yellow"].ToString("ddd, MMM d");
            else
                NextYellowBinDate = "Not scheduled";

            if (allNext.ContainsKey("Green"))
                NextGreenBinDate = allNext["Green"].ToString("ddd, MMM d");
            else
                NextGreenBinDate = "Not scheduled";

            // Calculate days until next collection (any bin)
            DaysUntilNextCollection = _calculatorService.GetDaysUntilNextAnyCollection(address, now);

            // Get which bins are next
            var nextBins = _calculatorService.GetNextCollectionBinTypes(address, now);
            NextCollectionBins = string.Join(", ", nextBins);
        }

        private void LoadUpcomingCollections()
        {
            if (BinSchedules.Count == 0)
                return;

            var address = BinSchedules.First().Address;
            var upcoming = _calculatorService.GetUpcomingCollectionsForAddress(address, DateTime.Now, 8);

            UpcomingCollections.Clear();
            foreach (var collection in upcoming)
            {
                UpcomingCollections.Add(collection);
            }

            // Generate calendar
            _collectionCalendar = _calculatorService.GenerateCollectionCalendar(address, DateTime.Now, 8);
        }

        private void LoadBulkyWasteSchedules()
        {
            var bulkySchedules = _calculatorService.GetBulkyWasteSchedules();

            BulkyWasteSchedules.Clear();
            foreach (var schedule in bulkySchedules)
            {
                BulkyWasteSchedules.Add(schedule);
            }
        }

        private void CheckReminders()
        {
            if (BinSchedules.Count == 0)
            {
                ShowReminder = false;
                return;
            }

            var address = BinSchedules.First().Address;
            var now = DateTime.Now;

            ShowReminder = _calculatorService.ShouldShowReminder(address, now);

            if (ShowReminder)
            {
                ReminderMessage = _calculatorService.GetReminderMessage(address, now);
            }
        }

        private void UpdateCollectionsForSelectedDate()
        {
            if (BinSchedules.Count == 0 || _collectionCalendar == null)
                return;

            // This method can be used to highlight collections on selected date
            // For now, just updating the property for potential UI bindings
        }

        private void ClearScheduleData()
        {
            BinSchedules.Clear();
            UpcomingCollections.Clear();
            BulkyWasteSchedules.Clear();
            NextRedBinDate = string.Empty;
            NextYellowBinDate = string.Empty;
            NextGreenBinDate = string.Empty;
            DaysUntilNextCollection = -1;
            NextCollectionBins = string.Empty;
            ShowReminder = false;
            ReminderMessage = string.Empty;
            HasValidSchedule = false;
            ValidationMessage = string.Empty;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
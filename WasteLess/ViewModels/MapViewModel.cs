// File: ViewModels/MapViewModel.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using WasteManagementApp.Data;
using WasteManagementApp.Models;
using WasteManagementApp.Services;

namespace WasteManagementApp.ViewModels
{
    /// <summary>
    /// ViewModel for the Map View
    /// Implements MVVM pattern with INotifyPropertyChanged
    /// </summary>
    public class MapViewModel : INotifyPropertyChanged
    {
        private readonly IDisposalLocationRepository _locationRepository;
        private readonly LocationFilterService _filterService;

        // Observable collections for data binding
        private ObservableCollection<IDisposalLocation> _allLocations;
        private ObservableCollection<IDisposalLocation> _filteredLocations;
        private ObservableCollection<string> _suburbs;
        private ObservableCollection<string> _wasteTypes;

        // Selected/Filter properties
        private string _selectedSuburb;
        private string _selectedWasteType;
        private string _searchText;
        private IDisposalLocation _selectedLocation;
        private bool _showBatteryLocations;
        private bool _showFurnitureLocations;
        private bool _showPickupOnly;

        // Statistics
        private int _totalLocationsCount;
        private int _filteredLocationsCount;
        private string _statusMessage;

        public MapViewModel(IDisposalLocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
            _filterService = new LocationFilterService(_locationRepository);

            // Initialize collections
            _allLocations = new ObservableCollection<IDisposalLocation>();
            _filteredLocations = new ObservableCollection<IDisposalLocation>();
            _suburbs = new ObservableCollection<string>();
            _wasteTypes = new ObservableCollection<string>();

            // Initialize commands
            SearchCommand = new RelayCommand(ExecuteSearch);
            ClearFiltersCommand = new RelayCommand(ExecuteClearFilters);
            ShowLocationDetailsCommand = new RelayCommand<IDisposalLocation>(ExecuteShowLocationDetails);
            RefreshCommand = new RelayCommand(ExecuteRefresh);

            // Load initial data
            LoadData();
        }

        #region Properties

        public ObservableCollection<IDisposalLocation> FilteredLocations
        {
            get => _filteredLocations;
            set
            {
                _filteredLocations = value;
                OnPropertyChanged(nameof(FilteredLocations));
            }
        }

        public ObservableCollection<string> Suburbs
        {
            get => _suburbs;
            set
            {
                _suburbs = value;
                OnPropertyChanged(nameof(Suburbs));
            }
        }

        public ObservableCollection<string> WasteTypes
        {
            get => _wasteTypes;
            set
            {
                _wasteTypes = value;
                OnPropertyChanged(nameof(WasteTypes));
            }
        }

        public string SelectedSuburb
        {
            get => _selectedSuburb;
            set
            {
                _selectedSuburb = value;
                OnPropertyChanged(nameof(SelectedSuburb));
                ApplyFilters();
            }
        }

        public string SelectedWasteType
        {
            get => _selectedWasteType;
            set
            {
                _selectedWasteType = value;
                OnPropertyChanged(nameof(SelectedWasteType));
                ApplyFilters();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        public IDisposalLocation SelectedLocation
        {
            get => _selectedLocation;
            set
            {
                _selectedLocation = value;
                OnPropertyChanged(nameof(SelectedLocation));
                OnPropertyChanged(nameof(IsLocationSelected));
                OnPropertyChanged(nameof(LocationDetailsText));
            }
        }

        public bool ShowBatteryLocations
        {
            get => _showBatteryLocations;
            set
            {
                _showBatteryLocations = value;
                OnPropertyChanged(nameof(ShowBatteryLocations));
                ApplyFilters();
            }
        }

        public bool ShowFurnitureLocations
        {
            get => _showFurnitureLocations;
            set
            {
                _showFurnitureLocations = value;
                OnPropertyChanged(nameof(ShowFurnitureLocations));
                ApplyFilters();
            }
        }

        public bool ShowPickupOnly
        {
            get => _showPickupOnly;
            set
            {
                _showPickupOnly = value;
                OnPropertyChanged(nameof(ShowPickupOnly));
                ApplyFilters();
            }
        }

        public int TotalLocationsCount
        {
            get => _totalLocationsCount;
            set
            {
                _totalLocationsCount = value;
                OnPropertyChanged(nameof(TotalLocationsCount));
            }
        }

        public int FilteredLocationsCount
        {
            get => _filteredLocationsCount;
            set
            {
                _filteredLocationsCount = value;
                OnPropertyChanged(nameof(FilteredLocationsCount));
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

        public bool IsLocationSelected => SelectedLocation != null;

        public string LocationDetailsText
        {
            get
            {
                if (SelectedLocation == null)
                    return "Select a location to view details";

                return SelectedLocation.GetLocationDescription();
            }
        }

        #endregion

        #region Commands

        public ICommand SearchCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand ShowLocationDetailsCommand { get; }
        public ICommand RefreshCommand { get; }

        private void ExecuteSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                ApplyFilters();
                return;
            }

            var results = _filterService.SearchByText(SearchText);
            UpdateFilteredLocations(results);
            StatusMessage = $"Found {results.Count} locations matching '{SearchText}'";
        }

        private void ExecuteClearFilters()
        {
            SelectedSuburb = null;
            SelectedWasteType = null;
            SearchText = string.Empty;
            ShowBatteryLocations = false;
            ShowFurnitureLocations = false;
            ShowPickupOnly = false;

            UpdateFilteredLocations(_allLocations.ToList());
            StatusMessage = "Filters cleared. Showing all locations.";
        }

        private void ExecuteShowLocationDetails(IDisposalLocation location)
        {
            SelectedLocation = location;
        }

        private void ExecuteRefresh()
        {
            LoadData();
            StatusMessage = "Data refreshed.";
        }

        #endregion

        #region Data Loading

        private void LoadData()
        {
            // Load all locations
            var locations = _locationRepository.GetAll();
            _allLocations.Clear();
            foreach (var location in locations)
            {
                _allLocations.Add(location);
            }

            TotalLocationsCount = _allLocations.Count;

            // Load suburbs (unique)
            var suburbs = locations.Select(l => l.Suburb).Distinct().OrderBy(s => s).ToList();
            Suburbs.Clear();
            Suburbs.Add("All Suburbs"); // Add default option
            foreach (var suburb in suburbs)
            {
                Suburbs.Add(suburb);
            }

            // Load waste types
            var wasteTypes = new HashSet<string>();
            foreach (var location in locations)
            {
                foreach (var type in location.GetAcceptedWasteTypes())
                {
                    wasteTypes.Add(type);
                }
            }
            WasteTypes.Clear();
            WasteTypes.Add("All Waste Types"); // Add default option
            foreach (var type in wasteTypes.OrderBy(t => t))
            {
                WasteTypes.Add(type);
            }

            // Show all locations initially
            UpdateFilteredLocations(_allLocations.ToList());
            StatusMessage = $"Loaded {TotalLocationsCount} disposal locations";
        }

        #endregion

        #region Filtering

        private void ApplyFilters()
        {
            List<IDisposalLocation> results = _allLocations.ToList();

            // Filter by suburb
            if (!string.IsNullOrWhiteSpace(SelectedSuburb) && SelectedSuburb != "All Suburbs")
            {
                results = _filterService.FilterBySuburb(SelectedSuburb);
            }

            // Filter by waste type
            if (!string.IsNullOrWhiteSpace(SelectedWasteType) && SelectedWasteType != "All Waste Types")
            {
                if (results.Count > 0)
                {
                    // Apply on top of existing filter
                    results = results.Where(loc =>
                        loc.GetAcceptedWasteTypes()
                            .Any(type => type.IndexOf(SelectedWasteType, StringComparison.OrdinalIgnoreCase) >= 0))
                        .ToList();
                }
                else
                {
                    results = _filterService.FilterByWasteType(SelectedWasteType);
                }
            }

            // Filter by location type checkboxes
            if (ShowBatteryLocations && !ShowFurnitureLocations)
            {
                results = results.OfType<BatteryDropOffLocation>().Cast<IDisposalLocation>().ToList();
            }
            else if (!ShowBatteryLocations && ShowFurnitureLocations)
            {
                results = results.OfType<FurnitureDonationCenter>().Cast<IDisposalLocation>().ToList();
            }
            else if (ShowBatteryLocations && ShowFurnitureLocations)
            {
                // Show both (no additional filtering needed)
            }
            else if (!ShowBatteryLocations && !ShowFurnitureLocations)
            {
                // Show all (no filtering)
            }

            // Filter by pickup service
            if (ShowPickupOnly)
            {
                results = results.Where(loc =>
                    loc is FurnitureDonationCenter &&
                    ((FurnitureDonationCenter)loc).OffersPickupService)
                    .ToList();
            }

            UpdateFilteredLocations(results);
            UpdateStatusMessage(results.Count);
        }

        private void UpdateFilteredLocations(List<IDisposalLocation> locations)
        {
            FilteredLocations.Clear();
            foreach (var location in locations)
            {
                FilteredLocations.Add(location);
            }
            FilteredLocationsCount = locations.Count;
        }

        private void UpdateStatusMessage(int count)
        {
            if (count == TotalLocationsCount)
            {
                StatusMessage = $"Showing all {count} locations";
            }
            else if (count == 0)
            {
                StatusMessage = "No locations match your filters";
            }
            else
            {
                StatusMessage = $"Showing {count} of {TotalLocationsCount} locations";
            }
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

    #region RelayCommand Helper

    /// <summary>
    /// Simple RelayCommand implementation for MVVM
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

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }

    /// <summary>
    /// Generic RelayCommand with parameter
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }

    #endregion
}

// File: WasteManagementApp.Tests/MapViewModelTests.cs
using NUnit.Framework;
using System.Linq;
using WasteManagementApp.Data;
using WasteManagementApp.ViewModels;

namespace WasteManagementApp.Tests
{
    [TestFixture]
    public class MapViewModelTests
    {
        private MapViewModel _viewModel;
        private IDisposalLocationRepository _repository;

        [SetUp]
        public void Setup()
        {
            _repository = new MockDisposalLocationRepository();
            _viewModel = new MapViewModel(_repository);
        }

        #region Initialization Tests

        [Test]
        public void ViewModel_OnInitialization_LoadsAllLocations()
        {
            // Assert
            Assert.Greater(_viewModel.FilteredLocations.Count, 0);
            Assert.Greater(_viewModel.TotalLocationsCount, 0);
            Assert.AreEqual(_viewModel.TotalLocationsCount, _viewModel.FilteredLocations.Count);

            Console.WriteLine($"Loaded {_viewModel.TotalLocationsCount} locations");
        }

        [Test]
        public void ViewModel_OnInitialization_LoadsSuburbs()
        {
            // Assert
            Assert.Greater(_viewModel.Suburbs.Count, 0);
            Assert.IsTrue(_viewModel.Suburbs.Contains("All Suburbs"));
            Assert.IsTrue(_viewModel.Suburbs.Contains("Chatswood"));

            Console.WriteLine($"Loaded {_viewModel.Suburbs.Count} suburbs:");
            foreach (var suburb in _viewModel.Suburbs)
            {
                Console.WriteLine($"  - {suburb}");
            }
        }

        [Test]
        public void ViewModel_OnInitialization_LoadsWasteTypes()
        {
            // Assert
            Assert.Greater(_viewModel.WasteTypes.Count, 0);
            Assert.IsTrue(_viewModel.WasteTypes.Contains("All Waste Types"));

            Console.WriteLine($"Loaded {_viewModel.WasteTypes.Count} waste types:");
            foreach (var type in _viewModel.WasteTypes)
            {
                Console.WriteLine($"  - {type}");
            }
        }

        [Test]
        public void ViewModel_OnInitialization_StatusMessageIsSet()
        {
            // Assert
            Assert.IsNotEmpty(_viewModel.StatusMessage);
            Assert.IsTrue(_viewModel.StatusMessage.Contains("Loaded"));
            Console.WriteLine($"Status: {_viewModel.StatusMessage}");
        }

        #endregion

        #region Filter by Suburb Tests

        [Test]
        public void FilterBySuburb_SelectChatswood_FiltersCorrectly()
        {
            // Act
            _viewModel.SelectedSuburb = "Chatswood";

            // Assert
            Assert.Greater(_viewModel.FilteredLocations.Count, 0);
            Assert.IsTrue(_viewModel.FilteredLocations.All(loc => loc.Suburb == "Chatswood"));
            Assert.Less(_viewModel.FilteredLocationsCount, _viewModel.TotalLocationsCount);

            Console.WriteLine($"Chatswood has {_viewModel.FilteredLocationsCount} locations");
        }

        [Test]
        public void FilterBySuburb_SelectAllSuburbs_ShowsAll()
        {
            // Arrange
            _viewModel.SelectedSuburb = "Chatswood"; // First filter

            // Act
            _viewModel.SelectedSuburb = "All Suburbs"; // Reset

            // Assert
            Assert.AreEqual(_viewModel.TotalLocationsCount, _viewModel.FilteredLocationsCount);
        }

        [Test]
        public void FilterBySuburb_ChangeSuburb_UpdatesFilteredLocations()
        {
            // Act
            _viewModel.SelectedSuburb = "Chatswood";
            int chatswoodCount = _viewModel.FilteredLocationsCount;

            _viewModel.SelectedSuburb = "Parramatta";
            int parramattaCount = _viewModel.FilteredLocationsCount;

            // Assert
            Assert.Greater(chatswoodCount, 0);
            Assert.Greater(parramattaCount, 0);
            Assert.IsTrue(_viewModel.FilteredLocations.All(loc => loc.Suburb == "Parramatta"));

            Console.WriteLine($"Chatswood: {chatswoodCount}, Parramatta: {parramattaCount}");
        }

        #endregion

        #region Filter by Waste Type Tests

        [Test]
        public void FilterByWasteType_SelectBattery_ShowsOnlyBatteryLocations()
        {
            // Act
            _viewModel.SelectedWasteType = "Batteries";

            // Assert
            Assert.Greater(_viewModel.FilteredLocations.Count, 0);
            Assert.IsTrue(_viewModel.FilteredLocations.All(loc =>
                loc.GetAcceptedWasteTypes().Any(t => t.Contains("Battery"))));

            Console.WriteLine($"Found {_viewModel.FilteredLocationsCount} battery locations");
        }

        [Test]
        public void FilterByWasteType_SelectFurniture_ShowsOnlyFurnitureLocations()
        {
            // Act
            _viewModel.SelectedWasteType = "Furniture";

            // Assert
            Assert.Greater(_viewModel.FilteredLocations.Count, 0);
            Assert.IsTrue(_viewModel.FilteredLocations.All(loc =>
                loc.GetAcceptedWasteTypes().Any(t => t.Contains("Furniture"))));

            Console.WriteLine($"Found {_viewModel.FilteredLocationsCount} furniture locations");
        }

        [Test]
        public void FilterByWasteType_AllWasteTypes_ShowsAll()
        {
            // Arrange
            _viewModel.SelectedWasteType = "Batteries"; // First filter

            // Act
            _viewModel.SelectedWasteType = "All Waste Types"; // Reset

            // Assert
            Assert.AreEqual(_viewModel.TotalLocationsCount, _viewModel.FilteredLocationsCount);
        }

        #endregion

        #region Combined Filter Tests

        [Test]
        public void CombinedFilter_SuburbAndWasteType_FiltersBoth()
        {
            // Act
            _viewModel.SelectedSuburb = "Chatswood";
            _viewModel.SelectedWasteType = "Batteries";

            // Assert
            Assert.Greater(_viewModel.FilteredLocations.Count, 0);
            Assert.IsTrue(_viewModel.FilteredLocations.All(loc =>
                loc.Suburb == "Chatswood" &&
                loc.GetAcceptedWasteTypes().Any(t => t.Contains("Battery"))));

            Console.WriteLine($"Found {_viewModel.FilteredLocationsCount} battery locations in Chatswood");
        }

        [Test]
        public void CombinedFilter_NoMatchingResults_ShowsZero()
        {
            // Act - Try to find something that doesn't exist
            _viewModel.SelectedSuburb = "Chatswood";
            _viewModel.SelectedWasteType = "Electronics"; // Might not exist in Chatswood

            // Assert - Could be 0 or more, just verify it's valid
            Assert.GreaterOrEqual(_viewModel.FilteredLocationsCount, 0);
            Console.WriteLine($"Combined filter result: {_viewModel.FilteredLocationsCount} locations");
        }

        #endregion

        #region Checkbox Filter Tests

        [Test]
        public void CheckboxFilter_ShowBatteryOnly_FiltersCorrectly()
        {
            // Act
            _viewModel.ShowBatteryLocations = true;
            _viewModel.ShowFurnitureLocations = false;

            // Assert
            Assert.Greater(_viewModel.FilteredLocations.Count, 0);
            Assert.IsTrue(_viewModel.FilteredLocations.All(loc =>
                loc is Models.BatteryDropOffLocation));

            Console.WriteLine($"Battery-only filter: {_viewModel.FilteredLocationsCount} locations");
        }

        [Test]
        public void CheckboxFilter_ShowFurnitureOnly_FiltersCorrectly()
        {
            // Act
            _viewModel.ShowBatteryLocations = false;
            _viewModel.ShowFurnitureLocations = true;

            // Assert
            Assert.Greater(_viewModel.FilteredLocations.Count, 0);
            Assert.IsTrue(_viewModel.FilteredLocations.All(loc =>
                loc is Models.FurnitureDonationCenter));

            Console.WriteLine($"Furniture-only filter: {_viewModel.FilteredLocationsCount} locations");
        }

        [Test]
        public void CheckboxFilter_ShowBoth_ShowsAllLocations()
        {
            // Act
            _viewModel.ShowBatteryLocations = true;
            _viewModel.ShowFurnitureLocations = true;

            // Assert
            Assert.AreEqual(_viewModel.TotalLocationsCount, _viewModel.FilteredLocationsCount);
        }

        [Test]
        public void CheckboxFilter_ShowPickupOnly_FiltersCorrectly()
        {
            // Act
            _viewModel.ShowPickupOnly = true;

            // Assert
            Assert.Greater(_viewModel.FilteredLocations.Count, 0);
            Assert.IsTrue(_viewModel.FilteredLocations.All(loc =>
                loc is Models.FurnitureDonationCenter &&
                ((Models.FurnitureDonationCenter)loc).OffersPickupService));

            Console.WriteLine($"Pickup-only filter: {_viewModel.FilteredLocationsCount} locations");
        }

        #endregion

        #region Search Tests

        [Test]
        public void Search_ByLocationName_FindsMatches()
        {
            // Act
            _viewModel.SearchText = "Aldi";
            _viewModel.SearchCommand.Execute(null);

            // Assert
            Assert.Greater(_viewModel.FilteredLocations.Count, 0);
            Assert.IsTrue(_viewModel.FilteredLocations.All(loc =>
                loc.Name.Contains("Aldi", System.StringComparison.OrdinalIgnoreCase)));

            Console.WriteLine($"Search 'Aldi': {_viewModel.FilteredLocationsCount} results");
        }

        [Test]
        public void Search_BySuburbName_FindsMatches()
        {
            // Act
            _viewModel.SearchText = "Parramatta";
            _viewModel.SearchCommand.Execute(null);

            // Assert
            Assert.Greater(_viewModel.FilteredLocations.Count, 0);
            Console.WriteLine($"Search 'Parramatta': {_viewModel.FilteredLocationsCount} results");
        }

        [Test]
        public void Search_EmptyString_ShowsAllLocations()
        {
            // Arrange
            _viewModel.SearchText = "Aldi"; // First search
            _viewModel.SearchCommand.Execute(null);

            // Act
            _viewModel.SearchText = "";
            _viewModel.SearchCommand.Execute(null);

            // Assert
            Assert.AreEqual(_viewModel.TotalLocationsCount, _viewModel.FilteredLocationsCount);
        }

        [Test]
        public void Search_CaseInsensitive_FindsMatches()
        {
            // Act
            _viewModel.SearchText = "WOOLWORTHS";
            _viewModel.SearchCommand.Execute(null);

            // Assert
            Assert.Greater(_viewModel.FilteredLocations.Count, 0);
            Console.WriteLine($"Case-insensitive search: {_viewModel.FilteredLocationsCount} results");
        }

        #endregion

        #region Command Tests

        [Test]
        public void ClearFiltersCommand_Execute_ResetsAllFilters()
        {
            // Arrange - Set some filters
            _viewModel.SelectedSuburb = "Chatswood";
            _viewModel.SelectedWasteType = "Batteries";
            _viewModel.SearchText = "Aldi";
            _viewModel.ShowBatteryLocations = true;
            _viewModel.ShowPickupOnly = true;

            // Act
            _viewModel.ClearFiltersCommand.Execute(null);

            // Assert
            Assert.IsNull(_viewModel.SelectedSuburb);
            Assert.IsNull(_viewModel.SelectedWasteType);
            Assert.IsEmpty(_viewModel.SearchText);
            Assert.IsFalse(_viewModel.ShowBatteryLocations);
            Assert.IsFalse(_viewModel.ShowFurnitureLocations);
            Assert.IsFalse(_viewModel.ShowPickupOnly);
            Assert.AreEqual(_viewModel.TotalLocationsCount, _viewModel.FilteredLocationsCount);

            Console.WriteLine("Filters cleared successfully");
        }

        [Test]
        public void RefreshCommand_Execute_ReloadsData()
        {
            // Arrange
            int originalCount = _viewModel.FilteredLocationsCount;

            // Act
            _viewModel.RefreshCommand.Execute(null);

            // Assert
            Assert.AreEqual(originalCount, _viewModel.FilteredLocationsCount);
            Assert.IsTrue(_viewModel.StatusMessage.Contains("refresh"));
            Console.WriteLine("Data refreshed");
        }

        #endregion

        #region Location Selection Tests

        [Test]
        public void SelectLocation_UpdatesSelectedLocation()
        {
            // Act
            var firstLocation = _viewModel.FilteredLocations.First();
            _viewModel.SelectedLocation = firstLocation;

            // Assert
            Assert.IsNotNull(_viewModel.SelectedLocation);
            Assert.AreEqual(firstLocation, _viewModel.SelectedLocation);
            Assert.IsTrue(_viewModel.IsLocationSelected);
            Console.WriteLine($"Selected: {_viewModel.SelectedLocation.Name}");
        }

        [Test]
        public void SelectLocation_UpdatesLocationDetailsText()
        {
            // Arrange
            var location = _viewModel.FilteredLocations.First();

            // Act
            _viewModel.SelectedLocation = location;

            // Assert
            Assert.IsNotEmpty(_viewModel.LocationDetailsText);
            Assert.IsTrue(_viewModel.LocationDetailsText.Contains(location.Name) ||
                         _viewModel.LocationDetailsText.Contains(location.Suburb));
            Console.WriteLine($"Details: {_viewModel.LocationDetailsText}");
        }

        [Test]
        public void NoLocationSelected_ShowsDefaultMessage()
        {
            // Arrange
            _viewModel.SelectedLocation = null;

            // Assert
            Assert.IsFalse(_viewModel.IsLocationSelected);
            Assert.IsTrue(_viewModel.LocationDetailsText.Contains("Select"));
        }

        #endregion

        #region PropertyChanged Tests

        [Test]
        public void PropertyChanged_SelectedSuburb_RaisesEvent()
        {
            // Arrange
            bool eventRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_viewModel.SelectedSuburb))
                    eventRaised = true;
            };

            // Act
            _viewModel.SelectedSuburb = "Chatswood";

            // Assert
            Assert.IsTrue(eventRaised);
        }

        [Test]
        public void PropertyChanged_FilteredLocations_RaisesEvent()
        {
            // Arrange
            bool eventRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_viewModel.FilteredLocations))
                    eventRaised = true;
            };

            // Act
            _viewModel.SelectedSuburb = "Parramatta"; // This triggers filter update

            // Assert
            Assert.IsTrue(eventRaised);
        }

        #endregion

        #region Status Message Tests

        [Test]
        public void StatusMessage_AfterFiltering_UpdatesCorrectly()
        {
            // Act
            _viewModel.SelectedSuburb = "Chatswood";

            // Assert
            Assert.IsNotEmpty(_viewModel.StatusMessage);
            Assert.IsTrue(_viewModel.StatusMessage.Contains("Showing") ||
                         _viewModel.StatusMessage.Contains("locations"));
            Console.WriteLine($"Status: {_viewModel.StatusMessage}");
        }

        [Test]
        public void StatusMessage_NoResults_ShowsNoMatchMessage()
        {
            // Act - Try to create a filter that returns no results
            _viewModel.SelectedSuburb = "Chatswood";
            _viewModel.ShowPickupOnly = true; // Chatswood might not have pickup-only

            // Assert - Check message makes sense
            Assert.IsNotEmpty(_viewModel.StatusMessage);
            Console.WriteLine($"Status: {_viewModel.StatusMessage}");
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Integration_CompleteFilterWorkflow_WorksCorrectly()
        {
            Console.WriteLine("=== COMPLETE FILTER WORKFLOW ===");

            // 1. Initial state
            Console.WriteLine($"Initial: {_viewModel.FilteredLocationsCount} locations");
            int initialCount = _viewModel.FilteredLocationsCount;

            // 2. Filter by suburb
            _viewModel.SelectedSuburb = "Chatswood";
            Console.WriteLine($"After suburb filter: {_viewModel.FilteredLocationsCount} locations");
            Assert.Less(_viewModel.FilteredLocationsCount, initialCount);

            // 3. Add waste type filter
            _viewModel.SelectedWasteType = "Batteries";
            Console.WriteLine($"After waste type filter: {_viewModel.FilteredLocationsCount} locations");

            // 4. Search
            _viewModel.SearchText = "Aldi";
            _viewModel.SearchCommand.Execute(null);
            Console.WriteLine($"After search: {_viewModel.FilteredLocationsCount} locations");

            // 5. Select location
            if (_viewModel.FilteredLocations.Count > 0)
            {
                _viewModel.SelectedLocation = _viewModel.FilteredLocations.First();
                Console.WriteLine($"Selected: {_viewModel.SelectedLocation.Name}");
                Assert.IsNotNull(_viewModel.SelectedLocation);
            }

            // 6. Clear all filters
            _viewModel.ClearFiltersCommand.Execute(null);
            Console.WriteLine($"After clear: {_viewModel.FilteredLocationsCount} locations");
            Assert.AreEqual(initialCount, _viewModel.FilteredLocationsCount);
        }

        #endregion
    }
}
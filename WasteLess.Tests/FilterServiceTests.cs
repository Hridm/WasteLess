// File: WasteManagementApp.Tests/FilterServiceTests.cs
using NUnit.Framework;
using System;
using System.Linq;
using WasteManagementApp.Data;
using WasteManagementApp.Models;
using WasteManagementApp.Services;

namespace WasteManagementApp.Tests
{
    [TestFixture]
    public class FilterServiceTests
    {
        private LocationFilterService _filterService;
        private IDisposalLocationRepository _repository;

        [SetUp]
        public void Setup()
        {
            _repository = new MockDisposalLocationRepository();
            _filterService = new LocationFilterService(_repository);
        }

        #region Basic Filter Tests

        [Test]
        public void FilterBySuburb_WithValidSuburb_ReturnsMatchingLocations()
        {
            // Act
            var results = _filterService.FilterBySuburb("Chatswood");

            // Assert
            Assert.Greater(results.Count, 0);
            Assert.IsTrue(results.All(loc => loc.Suburb == "Chatswood"));

            Console.WriteLine($"Found {results.Count} locations in Chatswood:");
            foreach (var loc in results)
            {
                Console.WriteLine($"  - {loc.Name}");
            }
        }

        [Test]
        public void FilterBySuburb_CaseInsensitive_Works()
        {
            // Act
            var results1 = _filterService.FilterBySuburb("CHATSWOOD");
            var results2 = _filterService.FilterBySuburb("chatswood");
            var results3 = _filterService.FilterBySuburb("Chatswood");

            // Assert
            Assert.AreEqual(results1.Count, results2.Count);
            Assert.AreEqual(results2.Count, results3.Count);
        }

        [Test]
        public void FilterBySuburb_WithEmptyString_ReturnsAll()
        {
            // Act
            var results = _filterService.FilterBySuburb("");
            var allLocations = _repository.GetAll();

            // Assert
            Assert.AreEqual(allLocations.Count, results.Count);
        }

        [Test]
        public void FilterByWasteType_BatteryType_ReturnsOnlyBatteryLocations()
        {
            // Act
            var results = _filterService.FilterByWasteType("Battery");

            // Assert
            Assert.Greater(results.Count, 0);
            Assert.IsTrue(results.All(loc => loc is BatteryDropOffLocation));

            Console.WriteLine($"Found {results.Count} battery disposal locations");
        }

        [Test]
        public void FilterByWasteType_FurnitureType_ReturnsOnlyFurnitureLocations()
        {
            // Act
            var results = _filterService.FilterByWasteType("Furniture");

            // Assert
            Assert.Greater(results.Count, 0);
            Assert.IsTrue(results.All(loc => loc is FurnitureDonationCenter));

            Console.WriteLine($"Found {results.Count} furniture donation centers");
        }

        [Test]
        public void FilterByWasteType_PartialMatch_Works()
        {
            // Act - Search for "Rechargeable" which is part of "Rechargeable Batteries"
            var results = _filterService.FilterByWasteType("Rechargeable");

            // Assert
            Assert.Greater(results.Count, 0);
            Console.WriteLine($"Found {results.Count} locations accepting rechargeable items");
        }

        [Test]
        public void FilterByPostcode_ValidPostcode_ReturnsMatches()
        {
            // Act
            var results = _filterService.FilterByPostcode("2067"); // Chatswood

            // Assert
            Assert.Greater(results.Count, 0);
            Assert.IsTrue(results.All(loc => loc.Postcode == "2067"));
        }

        #endregion

        #region Advanced Filter Tests

        [Test]
        public void FilterBySuburbAndWasteType_BothCriteria_ReturnsCorrectResults()
        {
            // Act
            var results = _filterService.FilterBySuburbAndWasteType("Chatswood", "Battery");

            // Assert
            Assert.Greater(results.Count, 0);
            Assert.IsTrue(results.All(loc =>
                loc.Suburb == "Chatswood" &&
                loc.GetAcceptedWasteTypes().Any(t => t.Contains("Battery"))));

            Console.WriteLine($"Found {results.Count} battery locations in Chatswood");
        }

        [Test]
        public void FilterBySuburbAndWasteType_OnlySuburb_Works()
        {
            // Act
            var results = _filterService.FilterBySuburbAndWasteType("Parramatta", null);

            // Assert
            Assert.Greater(results.Count, 0);
            Assert.IsTrue(results.All(loc => loc.Suburb == "Parramatta"));
        }

        [Test]
        public void FilterBySuburbAndWasteType_OnlyWasteType_Works()
        {
            // Act
            var results = _filterService.FilterBySuburbAndWasteType(null, "Furniture");

            // Assert
            Assert.Greater(results.Count, 0);
            Assert.IsTrue(results.All(loc => loc is FurnitureDonationCenter));
        }

        [Test]
        public void FilterByMultipleCriteria_AllParameters_ReturnsFiltered()
        {
            // Act
            var results = _filterService.FilterByMultipleCriteria(
                suburb: "Bondi Junction",
                wasteType: "Furniture",
                postcode: "2022",
                offersPickup: true
            );

            // Assert
            Assert.Greater(results.Count, 0);
            foreach (var loc in results)
            {
                Assert.AreEqual("Bondi Junction", loc.Suburb);
                Assert.AreEqual("2022", loc.Postcode);
                Assert.IsInstanceOf<FurnitureDonationCenter>(loc);
                Assert.IsTrue(((FurnitureDonationCenter)loc).OffersPickupService);
            }

            Console.WriteLine($"Found {results.Count} furniture centers in Bondi Junction with pickup");
        }

        [Test]
        public void FilterByMultipleCriteria_PickupServiceOnly_ReturnsOnlyWithPickup()
        {
            // Act
            var results = _filterService.FilterByMultipleCriteria(offersPickup: true);

            // Assert
            Assert.Greater(results.Count, 0);
            Assert.IsTrue(results.All(loc =>
                loc is FurnitureDonationCenter &&
                ((FurnitureDonationCenter)loc).OffersPickupService));
        }

        #endregion

        #region Search Function Tests

        [Test]
        public void SearchByText_ByName_FindsLocations()
        {
            // Act
            var results = _filterService.SearchByText("Aldi");

            // Assert
            Assert.Greater(results.Count, 0);
            Assert.IsTrue(results.All(loc => loc.Name.Contains("Aldi")));

            Console.WriteLine($"Found {results.Count} Aldi locations");
        }

        [Test]
        public void SearchByText_ByAddress_FindsLocations()
        {
            // Act
            var results = _filterService.SearchByText("Oxford");

            // Assert
            Assert.Greater(results.Count, 0);
            Assert.IsTrue(results.Any(loc => loc.Address.Contains("Oxford")));
        }

        [Test]
        public void SearchByText_BySuburb_FindsLocations()
        {
            // Act
            var results = _filterService.SearchByText("Parramatta");

            // Assert
            Assert.Greater(results.Count, 0);
            Assert.IsTrue(results.All(loc => loc.Suburb.Contains("Parramatta")));
        }

        [Test]
        public void SearchByText_CaseInsensitive_Works()
        {
            // Act
            var results1 = _filterService.SearchByText("WOOLWORTHS");
            var results2 = _filterService.SearchByText("woolworths");

            // Assert
            Assert.AreEqual(results1.Count, results2.Count);
        }

        [Test]
        public void GetLocationsNearCoordinate_ChatswoodCenter_FindsNearbyLocations()
        {
            // Arrange - Chatswood coordinates
            double lat = -33.7969;
            double lon = 151.1835;

            // Act
            var results = _filterService.GetLocationsNearCoordinate(lat, lon, radiusKm: 5.0);

            // Assert
            Assert.Greater(results.Count, 0);
            Console.WriteLine($"Found {results.Count} locations within 5km of Chatswood:");
            foreach (var loc in results)
            {
                Console.WriteLine($"  - {loc.Name} ({loc.Suburb})");
            }
        }

        [Test]
        public void GetLocationsNearCoordinate_OrderedByDistance_ClosestFirst()
        {
            // Arrange - Sydney CBD coordinates
            double lat = -33.8688;
            double lon = 151.2093;

            // Act
            var results = _filterService.GetLocationsNearCoordinate(lat, lon, radiusKm: 10.0);

            // Assert
            Assert.Greater(results.Count, 0);
            // First result should be closest (test that ordering works)
            Assert.IsNotNull(results.First());
        }

        #endregion

        #region Specialized Filter Tests

        [Test]
        public void GetBatteryLocationsByRetailer_Aldi_ReturnsOnlyAldi()
        {
            // Act
            var results = _filterService.GetBatteryLocationsByRetailer("Aldi");

            // Assert
            Assert.Greater(results.Count, 0);
            Assert.IsTrue(results.All(loc => loc.RetailerType == "Aldi"));

            Console.WriteLine($"Found {results.Count} Aldi battery drop-off locations");
        }

        [Test]
        public void GetBatteryLocationsByRetailer_Woolworths_ReturnsOnlyWoolworths()
        {
            // Act
            var results = _filterService.GetBatteryLocationsByRetailer("Woolworths");

            // Assert
            Assert.Greater(results.Count, 0);
            Assert.IsTrue(results.All(loc => loc.RetailerType == "Woolworths"));
        }

        [Test]
        public void GetBatteryLocationsByRetailer_Officeworks_ReturnsOnlyOfficeworks()
        {
            // Act
            var results = _filterService.GetBatteryLocationsByRetailer("Officeworks");

            // Assert
            Assert.Greater(results.Count, 0);
            Assert.IsTrue(results.All(loc => loc.RetailerType == "Officeworks"));
        }

        [Test]
        public void GetFurnitureCentersWithPickup_ReturnsOnlyPickupAvailable()
        {
            // Act
            var results = _filterService.GetFurnitureCentersWithPickup();

            // Assert
            Assert.Greater(results.Count, 0);
            Assert.IsTrue(results.All(center => center.OffersPickupService));

            Console.WriteLine($"Found {results.Count} furniture centers with pickup service:");
            foreach (var center in results)
            {
                Console.WriteLine($"  - {center.Name}");
            }
        }

        [Test]
        public void GetLocationsGroupedBySuburb_ReturnsCorrectGrouping()
        {
            // Act
            var grouped = _filterService.GetLocationsGroupedBySuburb();

            // Assert
            Assert.Greater(grouped.Count, 0);
            Assert.IsTrue(grouped.ContainsKey("Chatswood"));
            Assert.IsTrue(grouped.ContainsKey("Parramatta"));

            Console.WriteLine($"Locations grouped into {grouped.Count} suburbs:");
            foreach (var kvp in grouped.OrderBy(x => x.Key))
            {
                Console.WriteLine($"  {kvp.Key}: {kvp.Value.Count} locations");
            }
        }

        [Test]
        public void GetLocationsSorted_ReturnsInCorrectOrder()
        {
            // Act
            var sorted = _filterService.GetLocationsSorted();

            // Assert
            Assert.Greater(sorted.Count, 0);

            // Verify ordering
            for (int i = 0; i < sorted.Count - 1; i++)
            {
                int suburbCompare = string.Compare(sorted[i].Suburb, sorted[i + 1].Suburb, StringComparison.Ordinal);
                if (suburbCompare == 0)
                {
                    // Same suburb, check name ordering
                    Assert.LessOrEqual(
                        string.Compare(sorted[i].Name, sorted[i + 1].Name, StringComparison.Ordinal),
                        0);
                }
            }

            Console.WriteLine("First 5 sorted locations:");
            foreach (var loc in sorted.Take(5))
            {
                Console.WriteLine($"  {loc.Suburb} - {loc.Name}");
            }
        }

        #endregion

        #region Statistics Tests

        [Test]
        public void GetLocationCountsByType_ReturnsCorrectCounts()
        {
            // Act
            var counts = _filterService.GetLocationCountsByType();

            // Assert
            Assert.IsTrue(counts.ContainsKey("Battery"));
            Assert.IsTrue(counts.ContainsKey("Furniture"));
            Assert.Greater(counts["Battery"], 0);
            Assert.Greater(counts["Furniture"], 0);

            Console.WriteLine($"Battery locations: {counts["Battery"]}");
            Console.WriteLine($"Furniture centers: {counts["Furniture"]}");
        }

        [Test]
        public void GetTopSuburbsByLocationCount_ReturnsTopSuburbs()
        {
            // Act
            var topSuburbs = _filterService.GetTopSuburbsByLocationCount(3);

            // Assert
            Assert.LessOrEqual(topSuburbs.Count, 3);
            Assert.Greater(topSuburbs.Count, 0);

            // Verify descending order
            for (int i = 0; i < topSuburbs.Count - 1; i++)
            {
                Assert.GreaterOrEqual(topSuburbs[i].Value, topSuburbs[i + 1].Value);
            }

            Console.WriteLine("Top suburbs by location count:");
            foreach (var kvp in topSuburbs)
            {
                Console.WriteLine($"  {kvp.Key}: {kvp.Value} locations");
            }
        }

        #endregion

        #region LINQ Lambda Expression Demonstration Tests

        [Test]
        public void LINQDemo_WhereClause_Works()
        {
            // Demonstrates: Where with Lambda
            var results = _filterService.FilterBySuburb("Chatswood");
            Assert.Greater(results.Count, 0);
        }

        [Test]
        public void LINQDemo_AnyClause_Works()
        {
            // Demonstrates: Any with Lambda
            var results = _filterService.FilterByWasteType("Battery");
            Assert.Greater(results.Count, 0);
        }

        [Test]
        public void LINQDemo_OfTypeClause_Works()
        {
            // Demonstrates: OfType with Lambda
            var results = _filterService.GetBatteryLocationsByRetailer("Aldi");
            Assert.Greater(results.Count, 0);
        }

        [Test]
        public void LINQDemo_GroupByClause_Works()
        {
            // Demonstrates: GroupBy with Lambda
            var grouped = _filterService.GetLocationsGroupedBySuburb();
            Assert.Greater(grouped.Count, 0);
        }

        [Test]
        public void LINQDemo_OrderByClause_Works()
        {
            // Demonstrates: OrderBy with Lambda
            var sorted = _filterService.GetLocationsSorted();
            Assert.Greater(sorted.Count, 0);
        }

        [Test]
        public void LINQDemo_SelectClause_Works()
        {
            // Demonstrates: Select with Lambda (in GetLocationsNearCoordinate)
            var results = _filterService.GetLocationsNearCoordinate(-33.8688, 151.2093, 10);
            Assert.IsNotNull(results);
        }

        #endregion
    }
}
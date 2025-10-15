// File: WasteLess.Tests/RepositoryTests.cs
using NUnit.Framework;
using System;
using System.Linq;
using WasteManagementApp.Data;
using WasteManagementApp.Models;

namespace WasteLess.Tests
{
    [TestFixture]
    public class RepositoryTests
    {
        private IDisposalLocationRepository _locationRepo;
        private IScheduleRepository _scheduleRepo;

        [SetUp]
        public void Setup()
        {
            _locationRepo = new MockDisposalLocationRepository();
            _scheduleRepo = new MockScheduleRepository();
        }

        #region Disposal Location Repository Tests

        [Test]
        public void LocationRepo_GetAll_ReturnsAllLocations()
        {
            // Act
            var locations = _locationRepo.GetAll();

            // Assert
            Assert.IsNotNull(locations);
            Assert.Greater(locations.Count, 0);
            Console.WriteLine($"Total locations loaded: {locations.Count}");
        }

        [Test]
        public void LocationRepo_GetAllBatteryLocations_ReturnsOnlyBatteryLocations()
        {
            // Act
            var batteryLocations = _locationRepo.GetAllBatteryLocations();

            // Assert
            Assert.IsNotNull(batteryLocations);
            Assert.Greater(batteryLocations.Count, 0);
            Assert.IsTrue(batteryLocations.All(l => l is BatteryDropOffLocation));

            Console.WriteLine($"Battery locations found: {batteryLocations.Count}");
            foreach (var location in batteryLocations)
            {
                Console.WriteLine($"  - {location.Name} ({location.Suburb})");
            }
        }

        [Test]
        public void LocationRepo_GetAllFurnitureLocations_ReturnsOnlyFurnitureLocations()
        {
            // Act
            var furnitureLocations = _locationRepo.GetAllFurnitureLocations();

            // Assert
            Assert.IsNotNull(furnitureLocations);
            Assert.Greater(furnitureLocations.Count, 0);
            Assert.IsTrue(furnitureLocations.All(l => l is FurnitureDonationCenter));

            Console.WriteLine($"Furniture donation centers found: {furnitureLocations.Count}");
            foreach (var location in furnitureLocations)
            {
                Console.WriteLine($"  - {location.Name} ({location.Suburb})");
            }
        }

        [Test]
        public void LocationRepo_GetById_ReturnsCorrectLocation()
        {
            // Arrange
            var allLocations = _locationRepo.GetAll();
            var firstLocation = allLocations.First();

            // Act
            var foundLocation = _locationRepo.GetById(firstLocation.Id);

            // Assert
            Assert.IsNotNull(foundLocation);
            Assert.AreEqual(firstLocation.Id, foundLocation.Id);
            Assert.AreEqual(firstLocation.Name, foundLocation.Name);
        }

        [Test]
        public void LocationRepo_Add_IncreasesCount()
        {
            // Arrange
            int originalCount = _locationRepo.Count();
            var newLocation = new BatteryDropOffLocation
            {
                Name = "Test Aldi",
                RetailerType = "Aldi",
                Address = "123 Test St",
                Suburb = "TestSuburb",
                Postcode = "2000"
            };

            // Act
            _locationRepo.Add(newLocation);
            int newCount = _locationRepo.Count();

            // Assert
            Assert.AreEqual(originalCount + 1, newCount);
            Assert.Greater(newLocation.Id, 0); // ID should be assigned
        }

        [Test]
        public void LocationRepo_Delete_DecreasesCount()
        {
            // Arrange
            var locations = _locationRepo.GetAll();
            int originalCount = locations.Count;
            var locationToDelete = locations.First();

            // Act
            _locationRepo.Delete(locationToDelete.Id);
            int newCount = _locationRepo.Count();

            // Assert
            Assert.AreEqual(originalCount - 1, newCount);
            Assert.IsNull(_locationRepo.GetById(locationToDelete.Id));
        }

        [Test]
        public void LocationRepo_Update_ModifiesLocation()
        {
            // Arrange
            var location = _locationRepo.GetAll().First();
            string originalName = location.Name;
            location.Name = "Updated Name";

            // Act
            _locationRepo.Update(location);
            var updatedLocation = _locationRepo.GetById(location.Id);

            // Assert
            Assert.AreEqual("Updated Name", updatedLocation.Name);
            Assert.AreNotEqual(originalName, updatedLocation.Name);
        }

        [Test]
        public void LocationRepo_MockData_ContainsSydneySuburbs()
        {
            // Act
            var locations = _locationRepo.GetAll();
            var suburbs = locations.Select(l => l.Suburb).Distinct().ToList();

            // Assert
            Assert.Contains("Chatswood", suburbs);
            Assert.Contains("Bondi Junction", suburbs);
            Assert.Contains("Parramatta", suburbs);

            Console.WriteLine("Suburbs covered:");
            foreach (var suburb in suburbs.OrderBy(s => s))
            {
                Console.WriteLine($"  - {suburb}");
            }
        }

        [Test]
        public void LocationRepo_MockData_HasValidCoordinates()
        {
            // Act
            var locations = _locationRepo.GetAll();

            // Assert
            foreach (var location in locations)
            {
                Assert.IsTrue(location.Latitude != 0, $"{location.Name} missing latitude");
                Assert.IsTrue(location.Longitude != 0, $"{location.Name} missing longitude");

                // Sydney coordinates roughly: Lat -33.5 to -34.5, Long 150.5 to 151.5
                Assert.IsTrue(location.Latitude < -33 && location.Latitude > -34.5,
                    $"{location.Name} latitude outside Sydney range");
                Assert.IsTrue(location.Longitude > 150.5 && location.Longitude < 151.5,
                    $"{location.Name} longitude outside Sydney range");
            }
        }

        #endregion

        #region Schedule Repository Tests

        [Test]
        public void ScheduleRepo_GetAll_ReturnsAllSchedules()
        {
            // Act
            var schedules = _scheduleRepo.GetAll();

            // Assert
            Assert.IsNotNull(schedules);
            Assert.Greater(schedules.Count, 0);
            Console.WriteLine($"Total schedules loaded: {schedules.Count}");
        }

        [Test]
        public void ScheduleRepo_GetSchedulesBySuburb_ReturnsCorrectSchedules()
        {
            // Act
            var chatswoodSchedules = _scheduleRepo.GetSchedulesBySuburb("Chatswood");

            // Assert
            Assert.IsNotNull(chatswoodSchedules);
            Assert.AreEqual(3, chatswoodSchedules.Count); // Red, Yellow, Green bins
            Assert.IsTrue(chatswoodSchedules.All(s => s.Suburb == "Chatswood"));

            Console.WriteLine("Chatswood bin schedules:");
            foreach (var schedule in chatswoodSchedules)
            {
                Console.WriteLine($"  - {schedule.BinType} bin: {schedule.GetFrequencyDescription()} on {schedule.CollectionDay}");
            }
        }

        [Test]
        public void ScheduleRepo_GetSchedulesBySuburb_HasAllBinTypes()
        {
            // Act
            var schedules = _scheduleRepo.GetSchedulesBySuburb("Parramatta");
            var binTypes = schedules.Select(s => s.BinType).ToList();

            // Assert
            Assert.Contains("Red", binTypes);
            Assert.Contains("Yellow", binTypes);
            Assert.Contains("Green", binTypes);
        }

        [Test]
        public void ScheduleRepo_GetBulkyWasteSchedules_ReturnsOnlyBulkyWaste()
        {
            // Act
            var bulkySchedules = _scheduleRepo.GetBulkyWasteSchedules();

            // Assert
            Assert.IsNotNull(bulkySchedules);
            Assert.Greater(bulkySchedules.Count, 0);
            Assert.IsTrue(bulkySchedules.All(s => s is BulkyWasteSchedule));

            Console.WriteLine($"Bulky waste schedules found: {bulkySchedules.Count}");
            foreach (var schedule in bulkySchedules)
            {
                Console.WriteLine($"  - {schedule.Council}: {schedule.GetFrequencyDescription()}");
            }
        }

        [Test]
        public void ScheduleRepo_GetById_ReturnsCorrectSchedule()
        {
            // Arrange
            var allSchedules = _scheduleRepo.GetAll();
            var firstSchedule = allSchedules.First();

            // Act
            var foundSchedule = _scheduleRepo.GetById(firstSchedule.Id);

            // Assert
            Assert.IsNotNull(foundSchedule);
            Assert.AreEqual(firstSchedule.Id, foundSchedule.Id);
            Assert.AreEqual(firstSchedule.ScheduleName, foundSchedule.ScheduleName);
        }

        [Test]
        public void ScheduleRepo_Add_IncreasesCount()
        {
            // Arrange
            int originalCount = _scheduleRepo.GetAll().Count;
            var newSchedule = new BinCollectionSchedule
            {
                ScheduleName = "Test Schedule",
                BinType = "Red",
                Frequency = CollectionFrequency.Weekly,
                CollectionDay = DayOfWeek.Friday,
                Suburb = "TestSuburb"
            };

            // Act
            _scheduleRepo.Add(newSchedule);
            int newCount = _scheduleRepo.GetAll().Count;

            // Assert
            Assert.AreEqual(originalCount + 1, newCount);
            Assert.Greater(newSchedule.Id, 0);
        }

        [Test]
        public void ScheduleRepo_Delete_DecreasesCount()
        {
            // Arrange
            var schedules = _scheduleRepo.GetAll();
            int originalCount = schedules.Count;
            var scheduleToDelete = schedules.First();

            // Act
            _scheduleRepo.Delete(scheduleToDelete.Id);
            int newCount = _scheduleRepo.GetAll().Count;

            // Assert
            Assert.AreEqual(originalCount - 1, newCount);
            Assert.IsNull(_scheduleRepo.GetById(scheduleToDelete.Id));
        }

        [Test]
        public void ScheduleRepo_MockData_HasValidCollectionDays()
        {
            // Act
            var binSchedules = _scheduleRepo.GetAll().OfType<BinCollectionSchedule>().ToList();

            // Assert
            foreach (var schedule in binSchedules)
            {
                Assert.IsTrue(schedule.CollectionDay >= DayOfWeek.Sunday &&
                             schedule.CollectionDay <= DayOfWeek.Saturday,
                             $"{schedule.ScheduleName} has invalid collection day");

                Assert.IsTrue(schedule.NextCollectionDate >= DateTime.Now.Date,
                             $"{schedule.ScheduleName} next collection is in the past");
            }
        }

        [Test]
        public void ScheduleRepo_MockData_WeeklyBinsHaveWeeklyFrequency()
        {
            // Act
            var schedules = _scheduleRepo.GetAll().OfType<BinCollectionSchedule>().ToList();
            var redBins = schedules.Where(s => s.BinType == "Red").ToList();
            var yellowBins = schedules.Where(s => s.BinType == "Yellow").ToList();

            // Assert
            Assert.IsTrue(redBins.All(b => b.Frequency == CollectionFrequency.Weekly),
                "Red bins should be collected weekly");
            Assert.IsTrue(yellowBins.All(b => b.Frequency == CollectionFrequency.Weekly),
                "Yellow bins should be collected weekly");
        }

        [Test]
        public void ScheduleRepo_MockData_GreenBinsHaveFortnightlyFrequency()
        {
            // Act
            var schedules = _scheduleRepo.GetAll().OfType<BinCollectionSchedule>().ToList();
            var greenBins = schedules.Where(s => s.BinType == "Green").ToList();

            // Assert
            Assert.IsTrue(greenBins.All(b => b.Frequency == CollectionFrequency.Fortnightly),
                "Green bins should be collected fortnightly");
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Integration_LocationAndSchedule_MatchBySuburb()
        {
            // Arrange
            var locations = _locationRepo.GetAll();

            // Act
            foreach (var location in locations)
            {
                var schedulesForSuburb = _scheduleRepo.GetSchedulesBySuburb(location.Suburb);

                // Assert
                if (location.Suburb == "Chatswood" ||
                    location.Suburb == "Bondi Junction" ||
                    location.Suburb == "Parramatta")
                {
                    Assert.Greater(schedulesForSuburb.Count, 0,
                        $"Expected schedules for {location.Suburb} but found none");
                }
            }
        }

        [Test]
        public void Integration_AllRepositoryData_IsAccessible()
        {
            // Act & Assert - Verify all mock data loads without errors
            var locations = _locationRepo.GetAll();
            var batteryLocations = _locationRepo.GetAllBatteryLocations();
            var furnitureLocations = _locationRepo.GetAllFurnitureLocations();
            var schedules = _scheduleRepo.GetAll();
            var bulkySchedules = _scheduleRepo.GetBulkyWasteSchedules();

            Assert.Greater(locations.Count, 0);
            Assert.Greater(batteryLocations.Count, 0);
            Assert.Greater(furnitureLocations.Count, 0);
            Assert.Greater(schedules.Count, 0);
            Assert.Greater(bulkySchedules.Count, 0);

            Console.WriteLine("=== MOCK DATA SUMMARY ===");
            Console.WriteLine($"Total Locations: {locations.Count}");
            Console.WriteLine($"  - Battery Drop-offs: {batteryLocations.Count}");
            Console.WriteLine($"  - Furniture Centers: {furnitureLocations.Count}");
            Console.WriteLine($"Total Schedules: {schedules.Count}");
            Console.WriteLine($"  - Bulky Waste: {bulkySchedules.Count}");
        }

        #endregion
    }
}
// File: WasteManagementApp.Tests/FoundationTest.cs
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using WasteManagementApp.Models;

namespace WasteManagementApp.Tests
{
    [TestFixture]
    public class FoundationTest
    {
        #region Battery Drop-Off Tests

        [Test]
        public void BatteryDropOff_ImplementsInterface_Successfully()
        {
            // Arrange & Act
            IDisposalLocation location = new BatteryDropOffLocation();

            // Assert
            Assert.IsNotNull(location);
            Assert.IsInstanceOf<IDisposalLocation>(location);
        }

        [Test]
        public void BatteryDropOff_GetAcceptedWasteTypes_ReturnsCorrectTypes()
        {
            // Arrange
            var location = new BatteryDropOffLocation
            {
                Name = "Woolworths Chatswood",
                RetailerType = "Woolworths",
                AcceptsRechargeableBatteries = true,
                AcceptsSingleUseBatteries = true
            };

            // Act
            var wasteTypes = location.GetAcceptedWasteTypes();

            // Assert
            Assert.AreEqual(2, wasteTypes.Count);
            Assert.Contains("Rechargeable Batteries", wasteTypes);
            Assert.Contains("Single-Use Batteries", wasteTypes);
        }

        [Test]
        public void BatteryDropOff_GetLocationDescription_FormatsCorrectly()
        {
            // Arrange
            var location = new BatteryDropOffLocation
            {
                Name = "Aldi Bondi",
                RetailerType = "Aldi",
                Address = "123 Oxford St",
                Suburb = "Bondi Junction"
            };

            // Act
            string description = location.GetLocationDescription();

            // Assert
            Assert.That(description, Does.Contain("Aldi"));
            Assert.That(description, Does.Contain("Battery Recycling"));
            Assert.That(description, Does.Contain("Bondi Junction"));
        }

        #endregion

        #region Furniture Donation Tests

        [Test]
        public void FurnitureDonation_Polymorphism_WorksCorrectly()
        {
            // Arrange & Act
            BaseDisposalLocation location = new FurnitureDonationCenter
            {
                Name = "Vinnies Parramatta",
                OrganizationType = "Vinnies",
                AcceptsFurniture = true,
                AcceptsElectronics = true
            };

            // Assert - Polymorphism allows base class reference
            Assert.IsNotNull(location);
            Assert.IsInstanceOf<BaseDisposalLocation>(location);
            Assert.IsInstanceOf<IDisposalLocation>(location);
        }

        [Test]
        public void FurnitureDonation_GetAcceptedWasteTypes_ReturnsMultipleTypes()
        {
            // Arrange
            var center = new FurnitureDonationCenter
            {
                OrganizationType = "Salvos",
                AcceptsFurniture = true,
                AcceptsWhiteGoods = true,
                AcceptsElectronics = true
            };

            // Act
            var types = center.GetAcceptedWasteTypes();

            // Assert
            Assert.AreEqual(3, types.Count);
            Assert.Contains("Furniture", types);
            Assert.Contains("White Goods", types);
            Assert.Contains("Electronics", types);
        }

        [Test]
        public void FurnitureDonation_WithPickupService_ShowsInDescription()
        {
            // Arrange
            var center = new FurnitureDonationCenter
            {
                Name = "Salvos",
                OrganizationType = "Salvos",
                Address = "45 King St",
                Suburb = "Newtown",
                OffersPickupService = true
            };

            // Act
            string description = center.GetLocationDescription();

            // Assert
            Assert.That(description, Does.Contain("Pickup Available"));
        }

        #endregion

        #region Generic Collections Test

        [Test]
        public void GenericCollections_ListOfLocations_WorksCorrectly()
        {
            // Arrange - Demonstrates Generic Collections requirement
            var locations = new List<IDisposalLocation>
            {
                new BatteryDropOffLocation { Name = "Aldi", Suburb = "Chatswood" },
                new FurnitureDonationCenter { Name = "Vinnies", Suburb = "Parramatta" },
                new BatteryDropOffLocation { Name = "Woolworths", Suburb = "Bondi" }
            };

            // Act
            var chatswoodLocations = locations.Where(l => l.Suburb == "Chatswood").ToList();

            // Assert
            Assert.AreEqual(3, locations.Count);
            Assert.AreEqual(1, chatswoodLocations.Count);
            Assert.AreEqual("Aldi", chatswoodLocations[0].Name);
        }

        [Test]
        public void GenericCollections_Dictionary_WorksWithModels()
        {
            // Arrange - Demonstrates Generic Dictionary
            var locationsBySuburb = new Dictionary<string, List<IDisposalLocation>>
            {
                { "Chatswood", new List<IDisposalLocation>() },
                { "Parramatta", new List<IDisposalLocation>() }
            };

            // Act
            locationsBySuburb["Chatswood"].Add(new BatteryDropOffLocation { Name = "Aldi Chatswood" });
            locationsBySuburb["Parramatta"].Add(new FurnitureDonationCenter { Name = "Vinnies Parramatta" });

            // Assert
            Assert.AreEqual(2, locationsBySuburb.Keys.Count);
            Assert.AreEqual(1, locationsBySuburb["Chatswood"].Count);
            Assert.IsInstanceOf<BatteryDropOffLocation>(locationsBySuburb["Chatswood"][0]);
        }

        #endregion

        #region Bin Collection Schedule Tests

        [Test]
        public void BinSchedule_ImplementsIScheduleable_Successfully()
        {
            // Arrange & Act
            IScheduleable schedule = new BinCollectionSchedule();

            // Assert
            Assert.IsNotNull(schedule);
            Assert.IsInstanceOf<IScheduleable>(schedule);
        }

        [Test]
        public void BinSchedule_Weekly_CalculatesNextCollection()
        {
            // Arrange
            var schedule = new BinCollectionSchedule
            {
                BinType = "Red",
                Frequency = CollectionFrequency.Weekly,
                CollectionDay = DayOfWeek.Monday,
                NextCollectionDate = new DateTime(2025, 10, 13) // A Monday
            };

            // Act - Test from a Wednesday
            DateTime testDate = new DateTime(2025, 10, 15); // Wednesday
            DateTime nextCollection = schedule.CalculateNextCollection(testDate);

            // Assert - Should be next Monday
            Assert.AreEqual(DayOfWeek.Monday, nextCollection.DayOfWeek);
            Assert.AreEqual(new DateTime(2025, 10, 20), nextCollection);
        }

        [Test]
        public void BinSchedule_Fortnightly_CalculatesCorrectWeek()
        {
            // Arrange
            var schedule = new BinCollectionSchedule
            {
                BinType = "Green",
                Frequency = CollectionFrequency.Fortnightly,
                CollectionDay = DayOfWeek.Tuesday,
                NextCollectionDate = new DateTime(2025, 10, 14) // Tuesday
            };

            // Act
            DateTime testDate = new DateTime(2025, 10, 15);
            var upcomingCollections = schedule.GetUpcomingCollections(testDate, 4);

            // Assert
            Assert.AreEqual(2, upcomingCollections.Count); // 2 collections in 4 weeks (fortnightly)
            Assert.AreEqual(14, (upcomingCollections[1] - upcomingCollections[0]).Days);
        }

        [Test]
        public void BinSchedule_GetFrequencyDescription_ReturnsCorrectText()
        {
            // Arrange
            var weeklySchedule = new BinCollectionSchedule { Frequency = CollectionFrequency.Weekly };
            var fortnightlySchedule = new BinCollectionSchedule { Frequency = CollectionFrequency.Fortnightly };

            // Act & Assert
            Assert.AreEqual("Weekly", weeklySchedule.GetFrequencyDescription());
            Assert.AreEqual("Fortnightly", fortnightlySchedule.GetFrequencyDescription());
        }

        #endregion

        #region Bulky Waste Schedule Tests

        [Test]
        public void BulkyWaste_ImplementsIScheduleable_Successfully()
        {
            // Arrange & Act
            IScheduleable schedule = new BulkyWasteSchedule();

            // Assert
            Assert.IsNotNull(schedule);
            Assert.IsInstanceOf<IScheduleable>(schedule);
        }

        [Test]
        public void BulkyWaste_RequiresBooking_ShowsInDescription()
        {
            // Arrange
            var schedule = new BulkyWasteSchedule
            {
                Council = "City of Sydney",
                RequiresBooking = true
            };

            // Act
            string frequency = schedule.GetFrequencyDescription();

            // Assert
            Assert.That(frequency, Does.Contain("Booking Required"));
        }

        [Test]
        public void BulkyWaste_GetUpcomingCollections_ReturnsQuarterlyDates()
        {
            // Arrange
            var schedule = new BulkyWasteSchedule
            {
                NextCollectionDate = new DateTime(2025, 10, 15),
                RequiresBooking = false
            };

            // Act
            var collections = schedule.GetUpcomingCollections(new DateTime(2025, 10, 1), 52);

            // Assert
            Assert.LessOrEqual(collections.Count, 4); // Maximum 4 per year
            Assert.GreaterOrEqual(collections.Count, 1);
        }

        #endregion
    }
}
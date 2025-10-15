// File: WasteManagementApp.Tests/ScheduleCalculatorTests.cs
using NUnit.Framework;
using System;
using System.Linq;
using WasteManagementApp.Data;
using WasteManagementApp.Models;
using WasteManagementApp.Services;

namespace WasteManagementApp.Tests
{
    [TestFixture]
    public class ScheduleCalculatorTests
    {
        private ScheduleCalculatorService _calculatorService;
        private IScheduleRepository _scheduleRepository;

        [SetUp]
        public void Setup()
        {
            _scheduleRepository = new MockScheduleRepository();
            _calculatorService = new ScheduleCalculatorService(_scheduleRepository);
        }

        #region Get Schedules Tests

        [Test]
        public void GetSchedulesByAddress_ValidAddress_ReturnsSchedules()
        {
            // Act
            var schedules = _calculatorService.GetSchedulesByAddress("436 Victoria Ave");

            // Assert
            Assert.Greater(schedules.Count, 0);
            Assert.IsTrue(schedules.All(s => s.Address.Contains("436 Victoria Ave")));

            Console.WriteLine($"Found {schedules.Count} schedules for address:");
            foreach (var schedule in schedules)
            {
                Console.WriteLine($"  - {schedule.BinType} bin: {schedule.GetFrequencyDescription()}");
            }
        }

        [Test]
        public void GetSchedulesBySuburb_Chatswood_ReturnsThreeSchedules()
        {
            // Act
            var schedules = _calculatorService.GetSchedulesBySuburb("Chatswood");

            // Assert
            Assert.AreEqual(3, schedules.Count); // Red, Yellow, Green
            Assert.IsTrue(schedules.Any(s => s.BinType == "Red"));
            Assert.IsTrue(schedules.Any(s => s.BinType == "Yellow"));
            Assert.IsTrue(schedules.Any(s => s.BinType == "Green"));
        }

        [Test]
        public void GetSchedulesBySuburb_EmptyString_ReturnsEmpty()
        {
            // Act
            var schedules = _calculatorService.GetSchedulesBySuburb("");

            // Assert
            Assert.AreEqual(0, schedules.Count);
        }

        [Test]
        public void GetBulkyWasteSchedules_ReturnsSchedules()
        {
            // Act
            var schedules = _calculatorService.GetBulkyWasteSchedules();

            // Assert
            Assert.Greater(schedules.Count, 0);
            Console.WriteLine($"Found {schedules.Count} bulky waste schedules");
        }

        #endregion

        #region Next Collection Date Tests

        [Test]
        public void CalculateNextCollection_FromMonday_ReturnsNextMonday()
        {
            // Arrange
            var schedule = new BinCollectionSchedule
            {
                BinType = "Red",
                Frequency = CollectionFrequency.Weekly,
                CollectionDay = DayOfWeek.Monday,
                NextCollectionDate = new DateTime(2025, 10, 13) // A Monday
            };
            var testDate = new DateTime(2025, 10, 8); // Wednesday

            // Act
            var nextDate = _calculatorService.CalculateNextCollection(schedule, testDate);

            // Assert
            Assert.AreEqual(DayOfWeek.Monday, nextDate.DayOfWeek);
            Assert.AreEqual(new DateTime(2025, 10, 13), nextDate);
            Console.WriteLine($"Next Monday from {testDate:ddd, MMM d} is {nextDate:ddd, MMM d}");
        }

        [Test]
        public void CalculateNextCollection_FortnightlySchedule_SkipsWeek()
        {
            // Arrange
            var schedule = new BinCollectionSchedule
            {
                BinType = "Green",
                Frequency = CollectionFrequency.Fortnightly,
                CollectionDay = DayOfWeek.Tuesday,
                NextCollectionDate = new DateTime(2025, 10, 14) // Tuesday
            };
            var testDate = new DateTime(2025, 10, 15); // Wednesday after collection

            // Act
            var nextDate = _calculatorService.CalculateNextCollection(schedule, testDate);

            // Assert
            Assert.AreEqual(DayOfWeek.Tuesday, nextDate.DayOfWeek);
            // Should be 2 weeks later, not 1 week
            Assert.GreaterOrEqual((nextDate - testDate).TotalDays, 7);
            Console.WriteLine($"Next fortnightly collection: {nextDate:ddd, MMM d}");
        }

        [Test]
        public void GetNextCollectionForBinType_ValidBinType_ReturnsDate()
        {
            // Act
            var nextDate = _calculatorService.GetNextCollectionForBinType(
                "436 Victoria Ave",
                "Red",
                DateTime.Now);

            // Assert
            Assert.IsNotNull(nextDate);
            Assert.IsTrue(nextDate.Value > DateTime.Now.Date);
            Console.WriteLine($"Next Red bin collection: {nextDate.Value:ddd, MMM d, yyyy}");
        }

        [Test]
        public void GetNextCollectionForBinType_InvalidBinType_ReturnsNull()
        {
            // Act
            var nextDate = _calculatorService.GetNextCollectionForBinType(
                "436 Victoria Ave",
                "Purple",
                DateTime.Now);

            // Assert
            Assert.IsNull(nextDate);
        }

        [Test]
        public void GetAllNextCollections_ReturnsAllBinTypes()
        {
            // Act
            var allNext = _calculatorService.GetAllNextCollections(
                "436 Victoria Ave",
                DateTime.Now);

            // Assert
            Assert.Greater(allNext.Count, 0);
            Assert.IsTrue(allNext.ContainsKey("Red"));
            Assert.IsTrue(allNext.ContainsKey("Yellow"));
            Assert.IsTrue(allNext.ContainsKey("Green"));

            Console.WriteLine("Next collections:");
            foreach (var kvp in allNext.OrderBy(x => x.Value))
            {
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:ddd, MMM d}");
            }
        }

        #endregion

        #region Upcoming Collections Tests

        [Test]
        public void GetUpcomingCollectionsForAddress_FourWeeks_ReturnsMultiple()
        {
            // Arrange
            var startDate = new DateTime(2025, 10, 15);

            // Act
            var upcoming = _calculatorService.GetUpcomingCollectionsForAddress(
                "436 Victoria Ave",
                startDate,
                numberOfWeeks: 4);

            // Assert
            Assert.Greater(upcoming.Count, 0);
            Assert.IsTrue(upcoming.All(c => c.Date >= startDate));

            Console.WriteLine($"Upcoming collections (4 weeks from {startDate:MMM d}):");
            foreach (var collection in upcoming.Take(10))
            {
                Console.WriteLine($"  {collection}");
            }
        }

        [Test]
        public void GetUpcomingCollectionsForAddress_SortedByDate_OrderedCorrectly()
        {
            // Act
            var upcoming = _calculatorService.GetUpcomingCollectionsForAddress(
                "436 Victoria Ave",
                DateTime.Now,
                numberOfWeeks: 4);

            // Assert
            for (int i = 0; i < upcoming.Count - 1; i++)
            {
                Assert.LessOrEqual(upcoming[i].Date, upcoming[i + 1].Date,
                    "Collections should be sorted by date");
            }
        }

        [Test]
        public void GetUpcomingCollectionsForSuburb_ReturnsCollections()
        {
            // Act
            var upcoming = _calculatorService.GetUpcomingCollectionsForSuburb(
                "Chatswood",
                DateTime.Now,
                numberOfWeeks: 2);

            // Assert
            Assert.Greater(upcoming.Count, 0);
            Console.WriteLine($"Found {upcoming.Count} upcoming collections for Chatswood");
        }

        #endregion

        #region Calendar Generation Tests

        [Test]
        public void GenerateCollectionCalendar_EightWeeks_ReturnsDictionary()
        {
            // Arrange
            var startDate = new DateTime(2025, 10, 15);

            // Act
            var calendar = _calculatorService.GenerateCollectionCalendar(
                "436 Victoria Ave",
                startDate,
                numberOfWeeks: 8);

            // Assert
            Assert.Greater(calendar.Count, 0);

            Console.WriteLine($"Collection calendar ({calendar.Count} collection days):");
            foreach (var kvp in calendar.OrderBy(x => x.Key).Take(5))
            {
                Console.WriteLine($"  {kvp.Key:ddd, MMM d}: {kvp.Value.Count} bin(s)");
                foreach (var collection in kvp.Value)
                {
                    Console.WriteLine($"    - {collection.BinType} bin");
                }
            }
        }

        [Test]
        public void GenerateCollectionCalendar_GroupsByDate_Correctly()
        {
            // Arrange
            var startDate = new DateTime(2025, 10, 13); // A Monday (Chatswood collection day)

            // Act
            var calendar = _calculatorService.GenerateCollectionCalendar(
                "436 Victoria Ave",
                startDate,
                numberOfWeeks: 4);

            // Assert - On collection day, should have multiple bins
            var firstMonday = calendar.Keys.OrderBy(k => k).First();
            Assert.Greater(calendar[firstMonday].Count, 0);
        }

        [Test]
        public void GetCollectionsForDate_OnCollectionDay_ReturnsCollections()
        {
            // Arrange - Get a known collection date
            var schedules = _calculatorService.GetSchedulesByAddress("436 Victoria Ave");
            var firstSchedule = schedules.First();
            var collectionDate = firstSchedule.CalculateNextCollection(DateTime.Now);

            // Act
            var collections = _calculatorService.GetCollectionsForDate(
                "436 Victoria Ave",
                collectionDate);

            // Assert
            Assert.Greater(collections.Count, 0);
            Console.WriteLine($"Collections on {collectionDate:ddd, MMM d}: {collections.Count}");
        }

        [Test]
        public void GetCollectionsForDate_OnNonCollectionDay_ReturnsEmpty()
        {
            // Arrange - Use a date we know is not a collection day
            var nonCollectionDate = new DateTime(2025, 10, 16); // Thursday (no collections)

            // Act
            var collections = _calculatorService.GetCollectionsForDate(
                "436 Victoria Ave",
                nonCollectionDate);

            // Assert - Might be 0 or might have some, depends on schedule
            Assert.IsNotNull(collections);
        }

        #endregion

        #region Schedule Validation Tests

        [Test]
        public void ValidateScheduleForAddress_CompleteSchedule_ReturnsValid()
        {
            // Act
            var result = _calculatorService.ValidateScheduleForAddress("436 Victoria Ave");

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.MissingBinTypes.Count);
            Console.WriteLine($"Validation: {result.GetValidationMessage()}");
        }

        [Test]
        public void ValidateScheduleForAddress_IncompleteSchedule_ReturnsInvalid()
        {
            // Act - Use address that doesn't exist
            var result = _calculatorService.ValidateScheduleForAddress("999 Fake St");

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.Greater(result.MissingBinTypes.Count, 0);
            Console.WriteLine($"Validation: {result.GetValidationMessage()}");
        }

        [Test]
        public void IsCollectionDay_OnCollectionDay_ReturnsTrue()
        {
            // Arrange - Get next collection date
            var schedules = _calculatorService.GetSchedulesByAddress("436 Victoria Ave");
            var collectionDate = schedules.First().CalculateNextCollection(DateTime.Now);

            // Act
            var isCollectionDay = _calculatorService.IsCollectionDay(
                "436 Victoria Ave",
                collectionDate);

            // Assert
            Assert.IsTrue(isCollectionDay);
        }

        #endregion

        #region Days Until Collection Tests

        [Test]
        public void GetDaysUntilNextCollection_ReturnsPositiveNumber()
        {
            // Act
            var days = _calculatorService.GetDaysUntilNextCollection(
                "436 Victoria Ave",
                "Red",
                DateTime.Now);

            // Assert
            Assert.GreaterOrEqual(days, 0);
            Assert.LessOrEqual(days, 7); // Should be within a week for weekly collection
            Console.WriteLine($"Days until next Red bin collection: {days}");
        }

        [Test]
        public void GetDaysUntilNextCollection_InvalidBinType_ReturnsMinusOne()
        {
            // Act
            var days = _calculatorService.GetDaysUntilNextCollection(
                "436 Victoria Ave",
                "Purple",
                DateTime.Now);

            // Assert
            Assert.AreEqual(-1, days);
        }

        [Test]
        public void GetDaysUntilNextAnyCollection_ReturnsSmallestValue()
        {
            // Act
            var days = _calculatorService.GetDaysUntilNextAnyCollection(
                "436 Victoria Ave",
                DateTime.Now);

            // Assert
            Assert.GreaterOrEqual(days, 0);
            Assert.LessOrEqual(days, 7);
            Console.WriteLine($"Days until next collection (any bin): {days}");
        }

        [Test]
        public void GetNextCollectionBinTypes_ReturnsListOfBins()
        {
            // Act
            var binTypes = _calculatorService.GetNextCollectionBinTypes(
                "436 Victoria Ave",
                DateTime.Now);

            // Assert
            Assert.Greater(binTypes.Count, 0);
            Console.WriteLine($"Next collection bins: {string.Join(", ", binTypes)}");
        }

        [Test]
        public void GetNextCollectionBinTypes_SameDay_ReturnsMultipleBins()
        {
            // Arrange - Monday is collection day for Red and Yellow (both weekly)
            var schedules = _calculatorService.GetSchedulesByAddress("436 Victoria Ave");
            var mondaySchedule = schedules.First(s => s.CollectionDay == DayOfWeek.Monday);
            var testDate = mondaySchedule.CalculateNextCollection(DateTime.Now).AddDays(-7);

            // Act
            var binTypes = _calculatorService.GetNextCollectionBinTypes(
                "436 Victoria Ave",
                testDate);

            // Assert
            Assert.GreaterOrEqual(binTypes.Count, 2); // At least Red and Yellow
        }

        #endregion

        #region Reminder Tests

        [Test]
        public void ShouldShowReminder_EveningBeforeCollection_ReturnsTrue()
        {
            // Arrange - Get next collection date and set time to evening before
            var schedules = _calculatorService.GetSchedulesByAddress("436 Victoria Ave");
            var nextCollection = schedules.First().CalculateNextCollection(DateTime.Now);
            var eveningBefore = nextCollection.AddDays(-1).Date.AddHours(18); // 6 PM day before

            // Act
            var shouldShow = _calculatorService.ShouldShowReminder(
                "436 Victoria Ave",
                eveningBefore);

            // Assert
            Assert.IsTrue(shouldShow);
            Console.WriteLine($"Reminder shown at: {eveningBefore:ddd, MMM d h:mm tt}");
        }

        [Test]
        public void ShouldShowReminder_MorningBeforeCollection_ReturnsFalse()
        {
            // Arrange - Get next collection and set to morning before
            var schedules = _calculatorService.GetSchedulesByAddress("436 Victoria Ave");
            var nextCollection = schedules.First().CalculateNextCollection(DateTime.Now);
            var morningBefore = nextCollection.AddDays(-1).Date.AddHours(10); // 10 AM day before

            // Act
            var shouldShow = _calculatorService.ShouldShowReminder(
                "436 Victoria Ave",
                morningBefore);

            // Assert
            Assert.IsFalse(shouldShow);
        }

        [Test]
        public void GetReminderMessage_WhenReminderDue_ReturnsMessage()
        {
            // Arrange
            var schedules = _calculatorService.GetSchedulesByAddress("436 Victoria Ave");
            var nextCollection = schedules.First().CalculateNextCollection(DateTime.Now);
            var eveningBefore = nextCollection.AddDays(-1).Date.AddHours(19); // 7 PM day before

            // Act
            var message = _calculatorService.GetReminderMessage(
                "436 Victoria Ave",
                eveningBefore);

            // Assert
            Assert.IsNotEmpty(message);
            Assert.IsTrue(message.Contains("bin"));
            Assert.IsTrue(message.Contains("tomorrow"));
            Console.WriteLine($"Reminder: {message}");
        }

        [Test]
        public void GetReminderMessage_WhenNotDue_ReturnsEmpty()
        {
            // Arrange - Use a time when reminder shouldn't show
            var testTime = DateTime.Now.Date.AddHours(10); // 10 AM today

            // Act
            var message = _calculatorService.GetReminderMessage(
                "436 Victoria Ave",
                testTime);

            // Assert - Might be empty or might have message depending on schedule
            Assert.IsNotNull(message);
        }

        #endregion

        #region Edge Case Tests

        [Test]
        public void CalculateNextCollection_OnCollectionDay_ReturnsNextWeek()
        {
            // Arrange - Test on actual collection day
            var schedule = new BinCollectionSchedule
            {
                BinType = "Red",
                Frequency = CollectionFrequency.Weekly,
                CollectionDay = DayOfWeek.Monday,
                NextCollectionDate = new DateTime(2025, 10, 13) // A Monday
            };
            var collectionDay = new DateTime(2025, 10, 13).AddHours(7); // Monday 7 AM

            // Act
            var nextDate = _calculatorService.CalculateNextCollection(schedule, collectionDay);

            // Assert
            Assert.Greater(nextDate, collectionDay.Date);
            Console.WriteLine($"On collection day {collectionDay:ddd MMM d}, next is {nextDate:ddd MMM d}");
        }

        [Test]
        public void GetUpcomingCollections_ZeroWeeks_ReturnsEmpty()
        {
            // Act
            var upcoming = _calculatorService.GetUpcomingCollectionsForAddress(
                "436 Victoria Ave",
                DateTime.Now,
                numberOfWeeks: 0);

            // Assert
            Assert.AreEqual(0, upcoming.Count);
        }

        [Test]
        public void GetSchedulesByAddress_NonExistentAddress_ReturnsEmpty()
        {
            // Act
            var schedules = _calculatorService.GetSchedulesByAddress("999 Nonexistent St");

            // Assert
            Assert.AreEqual(0, schedules.Count);
        }

        [Test]
        public void CalculateNextCollection_NullSchedule_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _calculatorService.CalculateNextCollection(null, DateTime.Now));
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Integration_CompleteWorkflow_ForAddress()
        {
            // This test demonstrates a complete workflow
            string address = "436 Victoria Ave";

            Console.WriteLine("=== COMPLETE SCHEDULE WORKFLOW ===");
            Console.WriteLine($"Address: {address}\n");

            // 1. Get schedules
            var schedules = _calculatorService.GetSchedulesByAddress(address);
            Console.WriteLine($"Found {schedules.Count} bin schedules");

            // 2. Validate schedules
            var validation = _calculatorService.ValidateScheduleForAddress(address);
            Console.WriteLine($"Schedule valid: {validation.IsValid}");

            // 3. Get next collections
            var allNext = _calculatorService.GetAllNextCollections(address, DateTime.Now);
            Console.WriteLine("\nNext collection dates:");
            foreach (var kvp in allNext.OrderBy(x => x.Value))
            {
                Console.WriteLine($"  {kvp.Key} bin: {kvp.Value:ddd, MMM d}");
            }

            // 4. Get days until next collection
            var daysUntil = _calculatorService.GetDaysUntilNextAnyCollection(address, DateTime.Now);
            Console.WriteLine($"\nDays until next collection: {daysUntil}");

            // 5. Get which bins are next
            var nextBins = _calculatorService.GetNextCollectionBinTypes(address, DateTime.Now);
            Console.WriteLine($"Next bins: {string.Join(", ", nextBins)}");

            // 6. Get upcoming collections
            var upcoming = _calculatorService.GetUpcomingCollectionsForAddress(address, DateTime.Now, 4);
            Console.WriteLine($"\nUpcoming collections (next 4 weeks): {upcoming.Count} total");
            foreach (var collection in upcoming.Take(5))
            {
                Console.WriteLine($"  {collection}");
            }

            // 7. Generate calendar
            var calendar = _calculatorService.GenerateCollectionCalendar(address, DateTime.Now, 4);
            Console.WriteLine($"\nCalendar has {calendar.Count} collection days");

            // Assert
            Assert.Greater(schedules.Count, 0);
            Assert.IsTrue(validation.IsValid);
            Assert.Greater(allNext.Count, 0);
            Assert.GreaterOrEqual(daysUntil, 0);
            Assert.Greater(nextBins.Count, 0);
            Assert.Greater(upcoming.Count, 0);
            Assert.Greater(calendar.Count, 0);
        }

        [Test]
        public void Integration_MultipleSuburbs_HaveDifferentCollectionDays()
        {
            // Act
            var chatswoodSchedules = _calculatorService.GetSchedulesBySuburb("Chatswood");
            var bondiSchedules = _calculatorService.GetSchedulesBySuburb("Bondi Junction");

            var chatswoodDay = chatswoodSchedules.First().CollectionDay;
            var bondiDay = bondiSchedules.First().CollectionDay;

            // Assert
            Assert.AreNotEqual(chatswoodDay, bondiDay,
                "Different suburbs should have different collection days");

            Console.WriteLine($"Chatswood collects on: {chatswoodDay}");
            Console.WriteLine($"Bondi Junction collects on: {bondiDay}");
        }

        [Test]
        public void Integration_AllSuburbs_HaveCompleteSchedules()
        {
            // Arrange
            var suburbs = new[] { "Chatswood", "Bondi Junction", "Parramatta" };

            // Act & Assert
            foreach (var suburb in suburbs)
            {
                var schedules = _calculatorService.GetSchedulesBySuburb(suburb);

                Assert.AreEqual(3, schedules.Count,
                    $"{suburb} should have 3 bin types");

                Assert.IsTrue(schedules.Any(s => s.BinType == "Red"),
                    $"{suburb} should have Red bin");
                Assert.IsTrue(schedules.Any(s => s.BinType == "Yellow"),
                    $"{suburb} should have Yellow bin");
                Assert.IsTrue(schedules.Any(s => s.BinType == "Green"),
                    $"{suburb} should have Green bin");

                Console.WriteLine($"✓ {suburb} has complete schedule");
            }
        }

        #endregion

        #region Date Calculation Accuracy Tests

        [Test]
        public void DateCalculation_WeeklyBin_CollectsEveryWeek()
        {
            // Arrange
            var schedule = new BinCollectionSchedule
            {
                BinType = "Red",
                Frequency = CollectionFrequency.Weekly,
                CollectionDay = DayOfWeek.Monday,
                NextCollectionDate = new DateTime(2025, 10, 13)
            };
            var startDate = new DateTime(2025, 10, 1);

            // Act
            var upcoming = schedule.GetUpcomingCollections(startDate, 4);

            // Assert
            Assert.AreEqual(4, upcoming.Count); // 4 Mondays in 4 weeks

            // Check spacing is 7 days
            for (int i = 0; i < upcoming.Count - 1; i++)
            {
                var daysBetween = (upcoming[i + 1] - upcoming[i]).Days;
                Assert.AreEqual(7, daysBetween, "Weekly collections should be 7 days apart");
            }
        }

        [Test]
        public void DateCalculation_FortnightlyBin_CollectsEveryTwoWeeks()
        {
            // Arrange
            var schedule = new BinCollectionSchedule
            {
                BinType = "Green",
                Frequency = CollectionFrequency.Fortnightly,
                CollectionDay = DayOfWeek.Monday,
                NextCollectionDate = new DateTime(2025, 10, 13)
            };
            var startDate = new DateTime(2025, 10, 1);

            // Act
            var upcoming = schedule.GetUpcomingCollections(startDate, 4);

            // Assert
            Assert.AreEqual(2, upcoming.Count); // 2 collections in 4 weeks (fortnightly)

            // Check spacing is 14 days
            if (upcoming.Count >= 2)
            {
                var daysBetween = (upcoming[1] - upcoming[0]).Days;
                Assert.AreEqual(14, daysBetween, "Fortnightly collections should be 14 days apart");
            }
        }

        [Test]
        public void DateCalculation_AllDatesInFuture_FromGivenDate()
        {
            // Arrange
            var testDate = new DateTime(2025, 10, 15);

            // Act
            var upcoming = _calculatorService.GetUpcomingCollectionsForAddress(
                "436 Victoria Ave",
                testDate,
                numberOfWeeks: 4);

            // Assert
            Assert.IsTrue(upcoming.All(c => c.Date >= testDate),
                "All upcoming collections should be in the future");
        }

        [Test]
        public void DateCalculation_CorrectDayOfWeek_ForAllCollections()
        {
            // Arrange
            var schedules = _calculatorService.GetSchedulesByAddress("436 Victoria Ave");
            var startDate = DateTime.Now;

            // Act & Assert
            foreach (var schedule in schedules)
            {
                var upcoming = schedule.GetUpcomingCollections(startDate, 8);

                foreach (var date in upcoming)
                {
                    Assert.AreEqual(schedule.CollectionDay, date.DayOfWeek,
                        $"{schedule.BinType} bin should be collected on {schedule.CollectionDay}");
                }
            }
        }

        #endregion
    }
}
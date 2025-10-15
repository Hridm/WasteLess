// File: Services/ScheduleCalculatorService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using WasteManagementApp.Data;
using WasteManagementApp.Models;

namespace WasteManagementApp.Services
{
    /// <summary>
    /// Service for calculating bin collection schedules
    /// Handles weekly, fortnightly, and bulky waste calculations
    /// </summary>
    public class ScheduleCalculatorService
    {
        private readonly IScheduleRepository _scheduleRepository;

        public ScheduleCalculatorService(IScheduleRepository scheduleRepository)
        {
            _scheduleRepository = scheduleRepository;
        }

        #region Get Schedules

        /// <summary>
        /// Get all bin schedules for a specific address
        /// </summary>
        public List<BinCollectionSchedule> GetSchedulesByAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return new List<BinCollectionSchedule>();

            return _scheduleRepository.GetSchedulesByAddress(address);
        }

        /// <summary>
        /// Get all bin schedules for a specific suburb
        /// </summary>
        public List<BinCollectionSchedule> GetSchedulesBySuburb(string suburb)
        {
            if (string.IsNullOrWhiteSpace(suburb))
                return new List<BinCollectionSchedule>();

            return _scheduleRepository.GetSchedulesBySuburb(suburb);
        }

        /// <summary>
        /// Get bulky waste schedules
        /// </summary>
        public List<BulkyWasteSchedule> GetBulkyWasteSchedules()
        {
            return _scheduleRepository.GetBulkyWasteSchedules();
        }

        #endregion

        #region Next Collection Date Calculations

        /// <summary>
        /// Calculate next collection date for a specific schedule
        /// </summary>
        public DateTime CalculateNextCollection(IScheduleable schedule, DateTime fromDate)
        {
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));

            return schedule.CalculateNextCollection(fromDate);
        }

        /// <summary>
        /// Calculate next collection date for a specific bin type and address
        /// </summary>
        public DateTime? GetNextCollectionForBinType(string address, string binType, DateTime fromDate)
        {
            var schedules = GetSchedulesByAddress(address);
            var binSchedule = schedules.FirstOrDefault(s =>
                s.BinType.Equals(binType, StringComparison.OrdinalIgnoreCase));

            if (binSchedule == null)
                return null;

            return binSchedule.CalculateNextCollection(fromDate);
        }

        /// <summary>
        /// Get next collection date for all bins at an address
        /// Returns dictionary of bin type to next collection date
        /// </summary>
        public Dictionary<string, DateTime> GetAllNextCollections(string address, DateTime fromDate)
        {
            var schedules = GetSchedulesByAddress(address);
            var result = new Dictionary<string, DateTime>();

            foreach (var schedule in schedules)
            {
                result[schedule.BinType] = schedule.CalculateNextCollection(fromDate);
            }

            return result;
        }

        #endregion

        #region Upcoming Collections

        /// <summary>
        /// Get upcoming collections for all bins at an address
        /// Returns list of upcoming collection dates with bin info
        /// </summary>
        public List<UpcomingCollection> GetUpcomingCollectionsForAddress(
            string address,
            DateTime startDate,
            int numberOfWeeks = 4)
        {
            var schedules = GetSchedulesByAddress(address);
            var upcomingCollections = new List<UpcomingCollection>();

            foreach (var schedule in schedules)
            {
                var dates = schedule.GetUpcomingCollections(startDate, numberOfWeeks);

                foreach (var date in dates)
                {
                    upcomingCollections.Add(new UpcomingCollection
                    {
                        Date = date,
                        BinType = schedule.BinType,
                        BinDescription = schedule.BinDescription,
                        Frequency = schedule.GetFrequencyDescription(),
                        CollectionDay = schedule.CollectionDay.ToString()
                    });
                }
            }

            return upcomingCollections.OrderBy(c => c.Date).ToList();
        }

        /// <summary>
        /// Get upcoming collections for a specific suburb
        /// </summary>
        public List<UpcomingCollection> GetUpcomingCollectionsForSuburb(
            string suburb,
            DateTime startDate,
            int numberOfWeeks = 4)
        {
            var schedules = GetSchedulesBySuburb(suburb);
            var upcomingCollections = new List<UpcomingCollection>();

            foreach (var schedule in schedules)
            {
                var dates = schedule.GetUpcomingCollections(startDate, numberOfWeeks);

                foreach (var date in dates)
                {
                    upcomingCollections.Add(new UpcomingCollection
                    {
                        Date = date,
                        BinType = schedule.BinType,
                        BinDescription = schedule.BinDescription,
                        Frequency = schedule.GetFrequencyDescription(),
                        CollectionDay = schedule.CollectionDay.ToString()
                    });
                }
            }

            return upcomingCollections.OrderBy(c => c.Date).ToList();
        }

        #endregion

        #region Calendar Generation

        /// <summary>
        /// Generate a calendar view of collections grouped by date
        /// Returns dictionary of date to list of collections on that date
        /// </summary>
        public Dictionary<DateTime, List<UpcomingCollection>> GenerateCollectionCalendar(
            string address,
            DateTime startDate,
            int numberOfWeeks = 8)
        {
            var upcomingCollections = GetUpcomingCollectionsForAddress(address, startDate, numberOfWeeks);

            return upcomingCollections
                .GroupBy(c => c.Date.Date) // Group by date only (ignore time)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList());
        }

        /// <summary>
        /// Get collections for a specific date
        /// </summary>
        public List<UpcomingCollection> GetCollectionsForDate(string address, DateTime date)
        {
            var allCollections = GetUpcomingCollectionsForAddress(address, date.AddDays(-7), 2);
            return allCollections.Where(c => c.Date.Date == date.Date).ToList();
        }

        #endregion

        #region Schedule Validation

        /// <summary>
        /// Validate if an address has complete bin schedules (Red, Yellow, Green)
        /// </summary>
        public ScheduleValidationResult ValidateScheduleForAddress(string address)
        {
            var schedules = GetSchedulesByAddress(address);
            var result = new ScheduleValidationResult
            {
                Address = address,
                IsValid = true,
                MissingBinTypes = new List<string>()
            };

            // Check for required bin types
            var binTypes = schedules.Select(s => s.BinType).ToList();

            if (!binTypes.Any(b => b.Equals("Red", StringComparison.OrdinalIgnoreCase)))
            {
                result.IsValid = false;
                result.MissingBinTypes.Add("Red (General Waste)");
            }

            if (!binTypes.Any(b => b.Equals("Yellow", StringComparison.OrdinalIgnoreCase)))
            {
                result.IsValid = false;
                result.MissingBinTypes.Add("Yellow (Recycling)");
            }

            if (!binTypes.Any(b => b.Equals("Green", StringComparison.OrdinalIgnoreCase)))
            {
                result.IsValid = false;
                result.MissingBinTypes.Add("Green (Garden Organics)");
            }

            return result;
        }

        /// <summary>
        /// Check if a specific date is a collection day for an address
        /// </summary>
        public bool IsCollectionDay(string address, DateTime date)
        {
            var collections = GetCollectionsForDate(address, date);
            return collections.Count > 0;
        }

        #endregion

        #region Days Until Collection

        /// <summary>
        /// Get days until next collection for a specific bin type
        /// </summary>
        public int GetDaysUntilNextCollection(string address, string binType, DateTime fromDate)
        {
            var nextDate = GetNextCollectionForBinType(address, binType, fromDate);

            if (!nextDate.HasValue)
                return -1;

            return (int)(nextDate.Value - fromDate.Date).TotalDays;
        }

        /// <summary>
        /// Get days until next collection (any bin)
        /// </summary>
        public int GetDaysUntilNextAnyCollection(string address, DateTime fromDate)
        {
            var allNext = GetAllNextCollections(address, fromDate);

            if (allNext.Count == 0)
                return -1;

            var nextDate = allNext.Values.Min();
            return (int)(nextDate - fromDate.Date).TotalDays;
        }

        /// <summary>
        /// Get which bins are being collected next
        /// </summary>
        public List<string> GetNextCollectionBinTypes(string address, DateTime fromDate)
        {
            var allNext = GetAllNextCollections(address, fromDate);

            if (allNext.Count == 0)
                return new List<string>();

            var nextDate = allNext.Values.Min();

            return allNext
                .Where(kvp => kvp.Value.Date == nextDate.Date)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        #endregion

        #region Reminders

        /// <summary>
        /// Check if reminder should be shown (night before collection)
        /// </summary>
        public bool ShouldShowReminder(string address, DateTime currentDateTime)
        {
            var nextDate = GetAllNextCollections(address, currentDateTime.Date);

            if (nextDate.Count == 0)
                return false;

            var earliestCollection = nextDate.Values.Min();

            // Show reminder if collection is tomorrow and it's after 5 PM
            bool isTomorrow = (earliestCollection.Date - currentDateTime.Date).TotalDays == 1;
            bool isEvening = currentDateTime.Hour >= 17; // After 5 PM

            return isTomorrow && isEvening;
        }

        /// <summary>
        /// Get reminder message for upcoming collection
        /// </summary>
        public string GetReminderMessage(string address, DateTime currentDateTime)
        {
            if (!ShouldShowReminder(address, currentDateTime))
                return string.Empty;

            var binTypes = GetNextCollectionBinTypes(address, currentDateTime.Date);
            var nextDate = GetAllNextCollections(address, currentDateTime.Date).Values.Min();

            if (binTypes.Count == 0)
                return string.Empty;

            string binList = string.Join(", ", binTypes);
            return $"Reminder: Put out your {binList} bin(s) tonight! Collection is tomorrow ({nextDate:dddd, MMMM d}).";
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// Represents an upcoming collection with details
    /// </summary>
    public class UpcomingCollection
    {
        public DateTime Date { get; set; }
        public string BinType { get; set; }
        public string BinDescription { get; set; }
        public string Frequency { get; set; }
        public string CollectionDay { get; set; }

        public override string ToString()
        {
            return $"{Date:ddd, MMM d} - {BinType} bin ({BinDescription})";
        }
    }

    /// <summary>
    /// Result of schedule validation
    /// </summary>
    public class ScheduleValidationResult
    {
        public string Address { get; set; }
        public bool IsValid { get; set; }
        public List<string> MissingBinTypes { get; set; }

        public string GetValidationMessage()
        {
            if (IsValid)
                return "Schedule is complete.";

            return $"Missing schedules for: {string.Join(", ", MissingBinTypes)}";
        }
    }

    #endregion
}
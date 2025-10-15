using System;
using System.Collections.Generic;

namespace WasteManagementApp.Models
{
    //Bin collection schedule class
    //Demonstrates: Interface implementation and date calculations
    public class BinCollectionSchedule : IScheduleable
    {
        public int Id { get; set; }
        public string ScheduleName { get; set; }
        public string BinType { get; set; }
        public string BinDescription { get; set; }
        public string Address { get; set; }
        public string Suburb { get; set; }
        public CollectionFrequency Frequency { get; set; }
        public DayOfWeek CollectionDay { get; set; }
        public DateTime NextCollectionDate { get; set; }

        public DateTime CalculateNextCollection(DateTime fromDate)
        {
            // Find the next occurrence of CollectionDay
            int daysUntilCollection = ((int)CollectionDay - (int)fromDate.DayOfWeek + 7) % 7;
            if (daysUntilCollection == 0 && fromDate.TimeOfDay > TimeSpan.Zero)
                daysUntilCollection = 7; // If it's the same day but later in the day, get next week
                
            return fromDate.AddDays(daysUntilCollection);
        }

        public List<DateTime> GetUpcomingCollections(DateTime fromDate, int weeksAhead)
        {
            var collections = new List<DateTime>();
            var currentDate = CalculateNextCollection(fromDate);
            var endDate = fromDate.AddDays(weeksAhead * 7);

            while (currentDate <= endDate)
            {
                collections.Add(currentDate);
                
                // Add appropriate interval based on frequency
                switch (Frequency)
                {
                    case CollectionFrequency.Weekly:
                        currentDate = currentDate.AddDays(7);
                        break;
                    case CollectionFrequency.Fortnightly:
                        currentDate = currentDate.AddDays(14);
                        break;
                    case CollectionFrequency.Monthly:
                        currentDate = currentDate.AddMonths(1);
                        break;
                    default:
                        currentDate = currentDate.AddDays(7);
                        break;
                }
            }

            return collections;
        }

        public string GetFrequencyDescription()
        {
            switch (Frequency)
            {
                case CollectionFrequency.Weekly:
                    return "Weekly";
                case CollectionFrequency.Fortnightly:
                    return "Fortnightly";
                case CollectionFrequency.Monthly:
                    return "Monthly";
                case CollectionFrequency.Quarterly:
                    return "Quarterly";
                default:
                    return "Unknown";
            }
        }
    }
}
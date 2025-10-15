using System;
using System.Collections.Generic;

namespace WasteManagementApp.Models
{
    //Bulky waste schedule class
    //Demonstrates: Interface implementation for quarterly collections
    public class BulkyWasteSchedule : IScheduleable
    {
        public int Id { get; set; }
        public string ScheduleName { get; set; }
        public string Council { get; set; }
        public bool RequiresBooking { get; set; }
        public DateTime NextCollectionDate { get; set; }

        public DateTime CalculateNextCollection(DateTime fromDate)
        {
            // For bulky waste, typically quarterly
            var next = NextCollectionDate;
            while (next <= fromDate)
            {
                next = next.AddMonths(3); // Quarterly
            }
            return next;
        }

        public List<DateTime> GetUpcomingCollections(DateTime fromDate, int weeksAhead)
        {
            var collections = new List<DateTime>();
            var currentDate = CalculateNextCollection(fromDate);
            var endDate = fromDate.AddDays(weeksAhead * 7);

            while (currentDate <= endDate && collections.Count < 4) // Max 4 per year
            {
                collections.Add(currentDate);
                currentDate = currentDate.AddMonths(3); // Quarterly
            }

            return collections;
        }

        public string GetFrequencyDescription()
        {
            if (RequiresBooking)
                return "Quarterly - Booking Required";
            else
                return "Quarterly";
        }
    }
}
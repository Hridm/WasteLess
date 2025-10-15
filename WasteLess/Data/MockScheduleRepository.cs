using System;
using System.Collections.Generic;
using System.Linq;
using WasteManagementApp.Models;

namespace WasteManagementApp.Data
{
    /// <summary>
    /// Mock repository for schedules with sample data
    /// Demonstrates: Repository pattern implementation with in-memory schedule data
    /// </summary>
    public class MockScheduleRepository : IScheduleRepository
    {
        private List<IScheduleable> _schedules;
        private int _nextId = 1;

        public MockScheduleRepository()
        {
            InitializeData();
        }

        private void InitializeData()
        {
            _schedules = new List<IScheduleable>();

            // Create bin collection schedules for each suburb with specific addresses
            var suburbData = new[]
            {
                new { Suburb = "Chatswood", Address = "436 Victoria Ave", Day = DayOfWeek.Monday },
                new { Suburb = "Bondi Junction", Address = "123 Oxford St", Day = DayOfWeek.Wednesday },
                new { Suburb = "Parramatta", Address = "159 Church St", Day = DayOfWeek.Friday }
            };
            
            foreach (var data in suburbData)
            {
                // Red bin - Weekly collection
                _schedules.Add(new BinCollectionSchedule
                {
                    Id = _nextId++,
                    ScheduleName = $"{data.Suburb} Red Bin Collection",
                    BinType = "Red",
                    BinDescription = "General Waste",
                    Address = data.Address,
                    Suburb = data.Suburb,
                    Frequency = CollectionFrequency.Weekly,
                    CollectionDay = data.Day,
                    NextCollectionDate = GetNextWeekday(DateTime.Now, data.Day)
                });

                // Yellow bin - Weekly collection
                _schedules.Add(new BinCollectionSchedule
                {
                    Id = _nextId++,
                    ScheduleName = $"{data.Suburb} Yellow Bin Collection",
                    BinType = "Yellow",
                    BinDescription = "Recycling",
                    Address = data.Address,
                    Suburb = data.Suburb,
                    Frequency = CollectionFrequency.Weekly,
                    CollectionDay = data.Day,
                    NextCollectionDate = GetNextWeekday(DateTime.Now, data.Day)
                });

                // Green bin - Fortnightly collection
                _schedules.Add(new BinCollectionSchedule
                {
                    Id = _nextId++,
                    ScheduleName = $"{data.Suburb} Green Bin Collection",
                    BinType = "Green",
                    BinDescription = "Garden Organics",
                    Address = data.Address,
                    Suburb = data.Suburb,
                    Frequency = CollectionFrequency.Fortnightly,
                    CollectionDay = data.Day,
                    NextCollectionDate = GetNextWeekday(DateTime.Now, data.Day)
                });
            }

            // Add bulky waste schedules
            var councils = new[]
            {
                new { Name = "City of Sydney", RequiresBooking = true },
                new { Name = "Willoughby City Council", RequiresBooking = false },
                new { Name = "Parramatta City Council", RequiresBooking = true }
            };

            foreach (var council in councils)
            {
                _schedules.Add(new BulkyWasteSchedule
                {
                    Id = _nextId++,
                    ScheduleName = $"{council.Name} Bulky Waste Collection",
                    Council = council.Name,
                    RequiresBooking = council.RequiresBooking,
                    NextCollectionDate = DateTime.Now.AddDays(30) // Next month
                });
            }
        }

        private DateTime GetNextWeekday(DateTime fromDate, DayOfWeek dayOfWeek)
        {
            int daysUntilTarget = ((int)dayOfWeek - (int)fromDate.DayOfWeek + 7) % 7;
            if (daysUntilTarget == 0)
                daysUntilTarget = 7; // Next week if it's the same day
            return fromDate.AddDays(daysUntilTarget);
        }

        public List<IScheduleable> GetAll()
        {
            return _schedules.ToList();
        }

        public IScheduleable GetById(int id)
        {
            return _schedules.FirstOrDefault(s => s.Id == id);
        }

        public List<BinCollectionSchedule> GetSchedulesBySuburb(string suburb)
        {
            return _schedules.OfType<BinCollectionSchedule>()
                           .Where(s => s.Suburb == suburb)
                           .ToList();
        }

        public List<BinCollectionSchedule> GetSchedulesByAddress(string address)
        {
            return _schedules.OfType<BinCollectionSchedule>()
                           .Where(s => s.Address != null && s.Address.IndexOf(address, StringComparison.OrdinalIgnoreCase) >= 0)
                           .ToList();
        }

        public List<BulkyWasteSchedule> GetBulkyWasteSchedules()
        {
            return _schedules.OfType<BulkyWasteSchedule>().ToList();
        }

        public void Add(IScheduleable schedule)
        {
            schedule.Id = _nextId++;
            _schedules.Add(schedule);
        }

        public void Update(IScheduleable schedule)
        {
            var existing = _schedules.FirstOrDefault(s => s.Id == schedule.Id);
            if (existing != null)
            {
                var index = _schedules.IndexOf(existing);
                _schedules[index] = schedule;
            }
        }

        public void Delete(int id)
        {
            var schedule = _schedules.FirstOrDefault(s => s.Id == id);
            if (schedule != null)
            {
                _schedules.Remove(schedule);
            }
        }

        public int Count()
        {
            return _schedules.Count;
        }
    }
}
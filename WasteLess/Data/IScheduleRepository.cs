using System.Collections.Generic;
using WasteManagementApp.Models;

namespace WasteManagementApp.Data
{
    /// <summary>
    /// Repository interface for schedules
    /// Demonstrates: Repository pattern for data access
    /// </summary>
    public interface IScheduleRepository
    {
        /// <summary>
        /// Get all schedules
        /// </summary>
        List<IScheduleable> GetAll();

        /// <summary>
        /// Get schedule by ID
        /// </summary>
        IScheduleable GetById(int id);

        /// <summary>
        /// Get schedules by suburb
        /// </summary>
        List<BinCollectionSchedule> GetSchedulesBySuburb(string suburb);

        /// <summary>
        /// Get schedules by address
        /// </summary>
        List<BinCollectionSchedule> GetSchedulesByAddress(string address);

        /// <summary>
        /// Get all bulky waste schedules
        /// </summary>
        List<BulkyWasteSchedule> GetBulkyWasteSchedules();

        /// <summary>
        /// Add new schedule
        /// </summary>
        void Add(IScheduleable schedule);

        /// <summary>
        /// Update existing schedule
        /// </summary>
        void Update(IScheduleable schedule);

        /// <summary>
        /// Delete schedule
        /// </summary>
        void Delete(int id);

        /// <summary>
        /// Get count of all schedules
        /// </summary>
        int Count();
    }
}
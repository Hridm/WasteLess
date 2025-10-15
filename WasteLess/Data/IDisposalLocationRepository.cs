// File: Data/IDisposalLocationRepository.cs
using System.Collections.Generic;
using WasteManagementApp.Models;

namespace WasteManagementApp.Data
{
    /// <summary>
    /// Repository interface for disposal locations
    /// Demonstrates: Repository pattern for data access
    /// </summary>
    public interface IDisposalLocationRepository
    {
        /// <summary>
        /// Get all disposal locations
        /// </summary>
        List<IDisposalLocation> GetAll();

        /// <summary>
        /// Get location by ID
        /// </summary>
        IDisposalLocation GetById(int id);

        /// <summary>
        /// Get all battery drop-off locations
        /// </summary>
        List<BatteryDropOffLocation> GetAllBatteryLocations();

        /// <summary>
        /// Get all furniture donation centers
        /// </summary>
        List<FurnitureDonationCenter> GetAllFurnitureLocations();

        /// <summary>
        /// Add new location
        /// </summary>
        void Add(IDisposalLocation location);

        /// <summary>
        /// Update existing location
        /// </summary>
        void Update(IDisposalLocation location);

        /// <summary>
        /// Delete location
        /// </summary>
        void Delete(int id);

        /// <summary>
        /// Get count of all locations
        /// </summary>
        int Count();
    }
}

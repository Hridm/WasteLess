// File: Services/LocationFilterService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using WasteManagementApp.Data;
using WasteManagementApp.Models;

namespace WasteManagementApp.Services
{
    /// <summary>
    /// Service for filtering disposal locations using LINQ with Lambda expressions
    /// Demonstrates: LINQ queries with Lambda expressions (Kgobi's requirement)
    /// </summary>
    public class LocationFilterService
    {
        private readonly IDisposalLocationRepository _repository;

        public LocationFilterService(IDisposalLocationRepository repository)
        {
            _repository = repository;
        }

        #region Basic Filters

        /// <summary>
        /// Filter locations by suburb using LINQ Lambda
        /// </summary>
        public List<IDisposalLocation> FilterBySuburb(string suburb)
        {
            if (string.IsNullOrWhiteSpace(suburb))
                return _repository.GetAll();

            // LINQ with Lambda expression
            return _repository.GetAll()
                .Where(location => location.Suburb.Equals(suburb, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Filter locations by waste type using LINQ Lambda
        /// </summary>
        public List<IDisposalLocation> FilterByWasteType(string wasteType)
        {
            if (string.IsNullOrWhiteSpace(wasteType))
                return _repository.GetAll();

            // LINQ with Lambda expression - checks if location accepts the waste type
            return _repository.GetAll()
                .Where(location => location.GetAcceptedWasteTypes()
                    .Any(type => type.IndexOf(wasteType, StringComparison.OrdinalIgnoreCase) >= 0))
                .ToList();
        }

        /// <summary>
        /// Filter locations by postcode using LINQ Lambda
        /// </summary>
        public List<IDisposalLocation> FilterByPostcode(string postcode)
        {
            if (string.IsNullOrWhiteSpace(postcode))
                return _repository.GetAll();

            // LINQ with Lambda expression
            return _repository.GetAll()
                .Where(location => location.Postcode == postcode)
                .ToList();
        }

        #endregion

        #region Advanced Filters (Multiple Criteria)

        /// <summary>
        /// Filter by suburb AND waste type using LINQ Lambda
        /// Demonstrates: Combining multiple Lambda expressions
        /// </summary>
        public List<IDisposalLocation> FilterBySuburbAndWasteType(string suburb, string wasteType)
        {
            var allLocations = _repository.GetAll();

            // LINQ with multiple Lambda expressions combined
            return allLocations
                .Where(location =>
                    (string.IsNullOrWhiteSpace(suburb) ||
                     location.Suburb.Equals(suburb, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrWhiteSpace(wasteType) ||
                     location.GetAcceptedWasteTypes()
                        .Any(type => type.IndexOf(wasteType, StringComparison.OrdinalIgnoreCase) >= 0)))
                .ToList();
        }

        /// <summary>
        /// Complex filter with multiple optional criteria
        /// Demonstrates: Advanced LINQ with Lambda expressions
        /// </summary>
        public List<IDisposalLocation> FilterByMultipleCriteria(
            string suburb = null,
            string wasteType = null,
            string postcode = null,
            bool? offersPickup = null)
        {
            var query = _repository.GetAll().AsQueryable();

            // LINQ Lambda: Filter by suburb
            if (!string.IsNullOrWhiteSpace(suburb))
            {
                query = query.Where(loc =>
                    loc.Suburb.Equals(suburb, StringComparison.OrdinalIgnoreCase));
            }

            // LINQ Lambda: Filter by waste type
            if (!string.IsNullOrWhiteSpace(wasteType))
            {
                query = query.Where(loc =>
                    loc.GetAcceptedWasteTypes()
                        .Any(type => type.IndexOf(wasteType, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            // LINQ Lambda: Filter by postcode
            if (!string.IsNullOrWhiteSpace(postcode))
            {
                query = query.Where(loc => loc.Postcode == postcode);
            }

            // LINQ Lambda: Filter by pickup service (only for furniture centers)
            if (offersPickup.HasValue)
            {
                query = query.Where(loc =>
                    loc is FurnitureDonationCenter &&
                    ((FurnitureDonationCenter)loc).OffersPickupService == offersPickup.Value);
            }

            return query.ToList();
        }

        #endregion

        #region Search Functions

        /// <summary>
        /// Search locations by name or address using LINQ Lambda
        /// </summary>
        public List<IDisposalLocation> SearchByText(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return _repository.GetAll();

            searchText = searchText.ToLower();

            // LINQ Lambda: Search across multiple fields
            return _repository.GetAll()
                .Where(location =>
                    location.Name.ToLower().Contains(searchText) ||
                    location.Address.ToLower().Contains(searchText) ||
                    location.Suburb.ToLower().Contains(searchText))
                .ToList();
        }

        /// <summary>
        /// Get locations near a coordinate (simple distance calculation)
        /// Demonstrates: LINQ Lambda with mathematical operations
        /// </summary>
        public List<IDisposalLocation> GetLocationsNearCoordinate(
            double latitude,
            double longitude,
            double radiusKm = 5.0)
        {
            // LINQ Lambda with distance calculation
            return _repository.GetAll()
                .Select(location => new
                {
                    Location = location,
                    Distance = CalculateDistance(latitude, longitude,
                                               location.Latitude, location.Longitude)
                })
                .Where(x => x.Distance <= radiusKm)
                .OrderBy(x => x.Distance)
                .Select(x => x.Location)
                .ToList();
        }

        #endregion

        #region Specialized Filters

        /// <summary>
        /// Get all battery drop-off locations by retailer type
        /// Demonstrates: LINQ Lambda with type casting
        /// </summary>
        public List<BatteryDropOffLocation> GetBatteryLocationsByRetailer(string retailerType)
        {
            // LINQ Lambda: Type filtering and casting
            return _repository.GetAll()
                .OfType<BatteryDropOffLocation>()
                .Where(loc => loc.RetailerType.Equals(retailerType, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Get furniture centers that offer pickup service
        /// Demonstrates: LINQ Lambda with specific property filtering
        /// </summary>
        public List<FurnitureDonationCenter> GetFurnitureCentersWithPickup()
        {
            // LINQ Lambda: Type filtering and property check
            return _repository.GetAll()
                .OfType<FurnitureDonationCenter>()
                .Where(center => center.OffersPickupService)
                .ToList();
        }

        /// <summary>
        /// Get locations grouped by suburb
        /// Demonstrates: LINQ Lambda with GroupBy
        /// </summary>
        public Dictionary<string, List<IDisposalLocation>> GetLocationsGroupedBySuburb()
        {
            // LINQ Lambda: GroupBy operation
            return _repository.GetAll()
                .GroupBy(loc => loc.Suburb)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList());
        }

        /// <summary>
        /// Get locations ordered by suburb then name
        /// Demonstrates: LINQ Lambda with multiple ordering
        /// </summary>
        public List<IDisposalLocation> GetLocationsSorted()
        {
            // LINQ Lambda: Multiple OrderBy
            return _repository.GetAll()
                .OrderBy(loc => loc.Suburb)
                .ThenBy(loc => loc.Name)
                .ToList();
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Get count of locations by type
        /// Demonstrates: LINQ Lambda for aggregation
        /// </summary>
        public Dictionary<string, int> GetLocationCountsByType()
        {
            var all = _repository.GetAll();

            // LINQ Lambda: Count aggregation
            return new Dictionary<string, int>
            {
                { "Battery", all.OfType<BatteryDropOffLocation>().Count() },
                { "Furniture", all.OfType<FurnitureDonationCenter>().Count() }
            };
        }

        /// <summary>
        /// Get suburbs with most locations
        /// Demonstrates: LINQ Lambda with complex aggregation
        /// </summary>
        public List<KeyValuePair<string, int>> GetTopSuburbsByLocationCount(int topN = 5)
        {
            // LINQ Lambda: GroupBy, Count, OrderByDescending, Take
            return _repository.GetAll()
                .GroupBy(loc => loc.Suburb)
                .Select(group => new KeyValuePair<string, int>(group.Key, group.Count()))
                .OrderByDescending(kvp => kvp.Value)
                .Take(topN)
                .ToList();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Calculate distance between two coordinates (Haversine formula - simplified)
        /// </summary>
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371.0;

            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);

            lat1 = DegreesToRadians(lat1);
            lat2 = DegreesToRadians(lat2);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                      Math.Sin(dLon / 2) * Math.Sin(dLon / 2) *
                      Math.Cos(lat1) * Math.Cos(lat2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        #endregion
    }
}
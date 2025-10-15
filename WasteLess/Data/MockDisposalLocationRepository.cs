using System.Collections.Generic;
using System.Linq;
using WasteManagementApp.Models;

namespace WasteManagementApp.Data
{
    /// <summary>
    /// Mock repository for disposal locations with sample data
    /// Demonstrates: Repository pattern implementation with in-memory data
    /// </summary>
    public class MockDisposalLocationRepository : IDisposalLocationRepository
    {
        private List<IDisposalLocation> _locations;
        private int _nextId = 1;

        public MockDisposalLocationRepository()
        {
            InitializeData();
        }

        private void InitializeData()
        {
            _locations = new List<IDisposalLocation>
            {
                // Battery Drop-off Locations
                new BatteryDropOffLocation
                {
                    Id = _nextId++,
                    Name = "Woolworths Chatswood",
                    RetailerType = "Woolworths",
                    Address = "436 Victoria Ave",
                    Suburb = "Chatswood",
                    Postcode = "2067",
                    Latitude = -33.7969,
                    Longitude = 151.1832,
                    PhoneNumber = "02 9411 2300",
                    OpeningHours = "6:00 AM - 12:00 AM",
                    AcceptsRechargeableBatteries = true,
                    AcceptsSingleUseBatteries = true
                },
                new BatteryDropOffLocation
                {
                    Id = _nextId++,
                    Name = "Aldi Bondi Junction",
                    RetailerType = "Aldi",
                    Address = "123 Oxford St",
                    Suburb = "Bondi Junction",
                    Postcode = "2022",
                    Latitude = -33.8915,
                    Longitude = 151.2477,
                    PhoneNumber = "02 8021 4567",
                    OpeningHours = "8:30 AM - 8:00 PM",
                    AcceptsRechargeableBatteries = true,
                    AcceptsSingleUseBatteries = false
                },
                new BatteryDropOffLocation
                {
                    Id = _nextId++,
                    Name = "Coles Parramatta",
                    RetailerType = "Coles",
                    Address = "159-175 Church St",
                    Suburb = "Parramatta",
                    Postcode = "2150",
                    Latitude = -33.8151,
                    Longitude = 151.0052,
                    PhoneNumber = "02 9891 2345",
                    OpeningHours = "6:00 AM - 10:00 PM",
                    AcceptsRechargeableBatteries = true,
                    AcceptsSingleUseBatteries = true
                },

                // Furniture Donation Centers
                new FurnitureDonationCenter
                {
                    Id = _nextId++,
                    Name = "Vinnies Chatswood",
                    OrganizationType = "Vinnies",
                    Address = "2 Help St",
                    Suburb = "Chatswood",
                    Postcode = "2067",
                    Latitude = -33.7970,
                    Longitude = 151.1835,
                    PhoneNumber = "02 9413 7777",
                    OpeningHours = "9:00 AM - 4:00 PM",
                    AcceptsFurniture = true,
                    AcceptsWhiteGoods = true,
                    AcceptsElectronics = false,
                    OffersPickupService = true
                },
                new FurnitureDonationCenter
                {
                    Id = _nextId++,
                    Name = "Salvos Bondi Junction",
                    OrganizationType = "Salvos",
                    Address = "45 King St",
                    Suburb = "Bondi Junction",
                    Postcode = "2022",
                    Latitude = -33.8920,
                    Longitude = 151.2480,
                    PhoneNumber = "02 9387 5555",
                    OpeningHours = "9:00 AM - 5:00 PM",
                    AcceptsFurniture = true,
                    AcceptsWhiteGoods = false,
                    AcceptsElectronics = true,
                    OffersPickupService = false
                },
                new FurnitureDonationCenter
                {
                    Id = _nextId++,
                    Name = "Vinnies Parramatta",
                    OrganizationType = "Vinnies",
                    Address = "301 Church St",
                    Suburb = "Parramatta",
                    Postcode = "2150",
                    Latitude = -33.8155,
                    Longitude = 151.0055,
                    PhoneNumber = "02 9635 4444",
                    OpeningHours = "10:00 AM - 4:00 PM",
                    AcceptsFurniture = true,
                    AcceptsWhiteGoods = true,
                    AcceptsElectronics = true,
                    OffersPickupService = true
                }
            };
        }

        public List<IDisposalLocation> GetAll()
        {
            return _locations.ToList();
        }

        public IDisposalLocation GetById(int id)
        {
            return _locations.FirstOrDefault(l => l.Id == id);
        }

        public List<BatteryDropOffLocation> GetAllBatteryLocations()
        {
            return _locations.OfType<BatteryDropOffLocation>().ToList();
        }

        public List<FurnitureDonationCenter> GetAllFurnitureLocations()
        {
            return _locations.OfType<FurnitureDonationCenter>().ToList();
        }

        public void Add(IDisposalLocation location)
        {
            location.Id = _nextId++;
            _locations.Add(location);
        }

        public void Update(IDisposalLocation location)
        {
            var existing = _locations.FirstOrDefault(l => l.Id == location.Id);
            if (existing != null)
            {
                var index = _locations.IndexOf(existing);
                _locations[index] = location;
            }
        }

        public void Delete(int id)
        {
            var location = _locations.FirstOrDefault(l => l.Id == id);
            if (location != null)
            {
                _locations.Remove(location);
            }
        }

        public int Count()
        {
            return _locations.Count;
        }
    }
}
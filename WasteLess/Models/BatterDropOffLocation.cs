using System.Collections.Generic;

namespace WasteManagementApp.Models
{
    //Battery drop-off location class
    //Demonstrates: Interface implementation
    public class BatteryDropOffLocation : IDisposalLocation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Suburb { get; set; }
        public string Postcode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string PhoneNumber { get; set; }
        public string OpeningHours { get; set; }

        // Battery-specific properties
        public string RetailerType { get; set; }
        public bool AcceptsRechargeableBatteries { get; set; }
        public bool AcceptsSingleUseBatteries { get; set; }

        public List<string> GetAcceptedWasteTypes()
        {
            var wasteTypes = new List<string>();
            
            if (AcceptsRechargeableBatteries)
                wasteTypes.Add("Rechargeable Batteries");
                
            if (AcceptsSingleUseBatteries)
                wasteTypes.Add("Single-Use Batteries");
                
            return wasteTypes;
        }

        public string GetLocationDescription()
        {
            return $"{RetailerType} - Battery Recycling - {Suburb}";
        }
    }
}
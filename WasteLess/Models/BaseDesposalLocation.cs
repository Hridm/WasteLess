using System.Collections.Generic;

namespace WasteManagementApp.Models
{
    //Base class for all disposal locations
    //Demonstrates: Inheritance and abstract classes
    public abstract class BaseDisposalLocation : IDisposalLocation
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

        //Abstract methods that must be implemented by derived classes
        public abstract List<string> GetAcceptedWasteTypes();
        public abstract string GetLocationDescription();
    }
}
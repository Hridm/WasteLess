using System.Collections.Generic;

namespace WasteManagementApp.Models
{
    //Interface for all disposal location types 
    //Demonstrates: Interface implementation requirement

    public interface IDisposalLocation
    {
        int Id { get; set; }
        string Name { get; set; }
        string Address { get; set; }
        string Suburb { get; set; }
        string Postcode { get; set; }
        double Latitude { get; set; }
        double Longitude { get; set; }
        string PhoneNumber { get; set; }
        string OpeningHours { get; set; }

        //Returns the type of waste this location accepts
        List<string> GetAcceptedWasteTypes();

        //Returns a display-friendly description
        string GetLocationDescription();
    }

}
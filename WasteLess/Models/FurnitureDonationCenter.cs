using System.Collections.Generic;

namespace WasteManagementApp.Models
{
    //Furniture donation center class
    //Demonstrates: Inheritance and polymorphism
    public class FurnitureDonationCenter : BaseDisposalLocation
    {
        // Furniture-specific properties
        public string OrganizationType { get; set; }
        public bool AcceptsFurniture { get; set; }
        public bool AcceptsWhiteGoods { get; set; }
        public bool AcceptsElectronics { get; set; }
        public bool OffersPickupService { get; set; }

        public override List<string> GetAcceptedWasteTypes()
        {
            var wasteTypes = new List<string>();
            
            if (AcceptsFurniture)
                wasteTypes.Add("Furniture");
                
            if (AcceptsWhiteGoods)
                wasteTypes.Add("White Goods");
                
            if (AcceptsElectronics)
                wasteTypes.Add("Electronics");
                
            return wasteTypes;
        }

        public override string GetLocationDescription()
        {
            var description = $"{OrganizationType} - Donation Center - {Suburb}";
            
            if (OffersPickupService)
                description += " - Pickup Available";
                
            return description;
        }
    }
}
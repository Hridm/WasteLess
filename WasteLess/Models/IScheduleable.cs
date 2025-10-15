using System;
using System.Collections.Generic;

namespace WasteManagementApp.Models
{
    //Interface for all scheduleable items
    //Demonstrates: Interface implementation requirement
    public interface IScheduleable
    {
        int Id { get; set; }
        DateTime NextCollectionDate { get; set; }
        
        //Calculate the next collection date from a given date
        DateTime CalculateNextCollection(DateTime fromDate);
        
        //Get upcoming collection dates
        List<DateTime> GetUpcomingCollections(DateTime fromDate, int weeksAhead);
        
        //Get a description of the frequency
        string GetFrequencyDescription();
    }
    
    public enum CollectionFrequency
    {
        Weekly,
        Fortnightly,
        Monthly,
        Quarterly
    }
}
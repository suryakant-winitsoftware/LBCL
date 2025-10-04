using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace WINITSharedObjects.Models
{
   
    public class HolidayDetails
    {

        public int Id { get; set; }
        public string UID { get; set; }
        public string CompanyUID { get; set; }
        public string OrgUID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LocationUID { get; set; }
        public bool IsActive { get; set; }
        public int Year { get; set; }
        public int SS { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }

        public List<Holiday> Holidays ;
        public List<HolidayListRole> HolidayListRoleList;

        public HolidayDetails()
        {
            Holidays = new List<Holiday>();
            HolidayListRoleList = new List<HolidayListRole>();
        }
    }


    public class Holiday
    {

        public int Id { get; set; }
        public string UID { get; set; }
        public string HolidayListUID { get; set; }
        public DateTime HolidayDate { get; set; }
        public string Type { get; set; }
        
        public string Name { get; set; }
        public bool IsOptional { get; set; }
        public int Year { get; set; }
        public int SS { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }
        
    }
    public class HolidayListRole
    {

          public int Id { get; set; }
        public string UID { get; set; }
        public string HolidayListUID { get; set; }
        public string UserRoleUID { get; set; }
        public int SS { get; set; }

        public DateTime CreatedTime { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }

    }

    public class HolidayList
    {
        public int Id { get; set; }
        public string UID { get; set; }
        public string CompanyUID { get; set; }
        public string OrgUID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LocationUID { get; set; }
        public bool IsActive { get; set; }
        public int Year { get; set; }
        public int SS { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }

        
    }



}

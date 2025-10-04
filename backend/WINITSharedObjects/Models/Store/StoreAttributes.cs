using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models.Store
{
    public class StoreAttributes
    {
        //public int Id { get; set; }
        //public string UID { get; set; }
        //public string OrgUID { get; set; }
        //public string DistributionChannelUID { get; set; }
        //public string StoreUID { get; set; }
        //public string Name { get; set; }
        //public string Code { get; set; }
        //public string Value { get; set; }
        //public string ParentName { get; set; }
        //public int? SS { get; set; }
        //public DateTime? CreatedTime { get; set; }
        //public DateTime? ModifiedTime { get; set; }
        //public DateTime? ServerAddTime { get; set; }
        //public DateTime? ServerModifiedTime { get; set; }


        public int Id { get; set; }
       public string UID { get; set; }
        public string CompanyUID { get; set; }
        public string OrgUID { get; set; }
        public string DistributionChannelUID { get; set; }
        public string StoreUID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
        public string ParentName { get; set; }
        public string CreatedBy { get; set; }
        
        public DateTime? CreatedTime { get; set; }
        public string ModifiedBy { get; set; }

        public DateTime? ModifiedTime { get; set; }
        public DateTime? ServerAddTime { get; set; }
        public DateTime? ServerModifiedTime { get; set; }
    }

}

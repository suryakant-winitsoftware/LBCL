using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models
{
   


    public class Org
    {
        public Int64 org_id { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string org_code { get; set; }
        public string org_name { get; set; }
        public bool is_active { get; set; }
        
        public DateTime CreatedTime { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }


    }




}

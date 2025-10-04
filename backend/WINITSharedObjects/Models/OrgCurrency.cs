using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models
{
   


    public class OrgCurrency
    {

        public int Id { get; set; }
        public string OrgUID { get; set; }
        public string CurrencyUID { get; set; }
        public bool IsPrimary { get; set; }
        public int SS { get; set; }
        
        public DateTime CreatedTime { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }


    }




}

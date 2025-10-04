using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models
{
    public class Bank
    {

        public int Id { get; set; }
        public string UID { get; set; }
        public string CompanyUID { get; set; }
        public string BankName { get; set; }
        public string CountryUID { get; set; }
        public decimal ChequeFee { get; set; }
        public int SS { get; set;}
        public DateTime CreatedTime { get ; set;}
        public DateTime ModifiedTime { get ; set;}
        public DateTime ServerAddTime { get ; set;}
        public DateTime ServerModifiedTime { get ; set;}


    }

   
    
   

}

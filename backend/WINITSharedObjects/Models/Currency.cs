using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models
{
   


    public class Currency
    {

        public int Id { get; set; }
        public string UID { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public int Digits { get; set; }
        public int NumberCode { get; set; }
        public string FractionName { get; set; }
        public int SS { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }


    }




}

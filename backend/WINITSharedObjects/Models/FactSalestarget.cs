using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models
{
    public class FactSalestarget
    {
       
        public string user_code { get; set; }
        public string customer_code { get; set; }
        public string product_code { get; set; }
        public DateTime tgt_month { get; set; }
        public decimal amount { get; set; }
        public decimal quantity { get; set; }

    }





}

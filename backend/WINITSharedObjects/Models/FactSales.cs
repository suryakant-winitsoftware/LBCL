using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models
{
    public class FactSales
    {
        public string user_code { get; set; }
        public DateTime trx_date { get; set; }
        public string customer_code { get; set; }
        public string product_code { get; set; }
        public decimal gross_amt { get; set; }
        public decimal tax_amt { get; set; }
        public int trx_type { get; set; }

    }





}

using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models
{
    public class CommissionKPI
    {

        public Int64 commission_id { get; set; }
        public string org_code { get; set; }
        public string commission_name { get; set; }
        public Int64 kpi_type_id { get; set; }
        public string kpi_name { get; set; }
        public string kpi_structure_type { get; set; }
        public decimal kpi_weight { get; set; }


    }
    public class CommissionKPICustomerMapping
    {

        public Int64 commission_kpi_id { get; set; }
        public string linked_customer_code { get; set; }
        public string linked_customer_type { get; set; }

    }
    public class CommissionKPIProductMapping
    {

        public Int64 commission_kpi_id { get; set; }
        public string linked_product_code { get; set; }
        public string linked_product_type { get; set; }

    }
    public class CommissionKPISlab
    {

        public Int64 commission_kpi_slab_id { get; set; }
        public Int64 commission_kpi_id { get; set; }
        public decimal commission_slab_from { get; set; }
        public decimal commission_slab_to { get; set; }
        public decimal commission_kpi_slab_payout { get; set; }

    }

    public class CommissionUserMapping
    {
        public Int64 commission_id { get; set; }
        public string linked_user_type { get; set; }
        public string linked_user_code { get; set; }

    }

    public class CommissionUserKPIPerformance {
        public Int64 commission_id { get; set; }
        public Int64 commission_kpi_id { get; set; }
        public string user_code { get; set; }
        public decimal commission_payout { get; set; }
    }


    public class CommissionUserPayout
    {
        public Int64 commission_id { get; set; }
        public string user_code { get; set; }
        public decimal commission_payout { get; set; }
    }




}

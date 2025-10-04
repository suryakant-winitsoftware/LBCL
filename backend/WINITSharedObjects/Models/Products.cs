using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models
{
    public class Product
    {

        public Int64 product_id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }
        public string org_code { get; set; }
        public string product_code { get; set; }
        public string product_name { get; set; }
        public bool is_active { get; set; }
        public string BaseUOM { get; set; }




    }
    public class ProductConfig
    {

        public Int64 SKUConfigId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }
        public string ProductCode { get; set; }
        public string DistributionChannelOrgCode { get; set; }
        public bool CanBuy { get; set; }
        public bool CanSell { get; set; }
        public string BuyingUOM { get; set; }
        public string SellingUOM { get; set; }
        public bool IsActive { get; set; }




    }

    public class ProductUOM
    {

        public Int64 ProductUOMId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }
        public string ProductCode { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string BarCode { get; set; }
        public bool IsBaseUOM { get; set; }
        public bool IsOuterUOM { get; set; }
        public decimal Multiplier { get; set; }




    }
    public class ProductMaster
    {

        public List<Product> Products { get; set; }
        public List<ProductConfig> ProductConfigs { get; set; }
        public List<ProductUOM> ProductUOMs { get; set; }

    }
    public class ProductAttributes
    {

        public Int64 product_attributes_id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }
        public string product_code { get; set; }
        public string hierachy_type { get; set; }
        public string hierachy_code { get; set; }
        public string hierachy_value { get; set; }
      

    }

    public class ProductDimensionBridge
    {

        public Int64 product_dimension_bridge_id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }
        public string product_code { get; set; }
        public int product_dimension_id { get; set; }
      


    }


    public class ProductTypeBridge
    {

        public Int64 product_type_bridge_id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }
        public string product_code { get; set; }
        public int product_type_id { get; set; }



    }

    public class ProductType
    {

        public Int64 product_type_id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }
        public string product_group_type { get; set; }
        public string product_type_code { get; set; }
        public string product_type_description { get; set; }
        public Int64 parent_product_type_id { get; set; }



    }

    public class ProductDimension
    {

        public Int64 product_dimension_id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }
        public string product_dimension_code { get; set; }
        public string product_dimension_description { get; set; }
        public Int64 parent_product_dimension_id { get; set; }


    }

}

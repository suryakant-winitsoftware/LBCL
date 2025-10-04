using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models
{
    public interface ICustomer
    {
        public int Id { get; set; }
        public int TerritoryId { get; set; }
        public string Name { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
    public class Customer
    {

        public int Id { get; set; }
        public int TerritoryId { get; set; }
        public string Name { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }


    }
    public class CustomerAttributes
    {
        public string customer_code { get; set; }
        public string hierachy_type { get; set; }
        public string hierachy_code { get; set; }
        public string hierachy_value { get; set; }

    }




}

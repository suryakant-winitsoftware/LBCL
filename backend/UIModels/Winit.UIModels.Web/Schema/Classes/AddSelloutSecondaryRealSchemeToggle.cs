using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.UIModels.Web.Schema.Interfaces;

namespace Winit.UIModels.Web.Schema.Classes
{
    public class AddSelloutSecondaryRealSchemeToggle : IAddSelloutSecondaryRealSchemeToggle
    {
        bool details { get; set; }=true;
        bool schemeProducts { get; set; }
        bool slab { get; set; }
        bool customer { get; set; }
        public bool Details
        {
            get
            {
                return details;
            }
            set
            {
                details = value;
                if (value)
                {
                    ResetAllProperties(nameof(Details));
                }
            }
        } 
        public bool SchemeProducts
        {
            get
            {
                return schemeProducts;
            }
            set
            {
                schemeProducts = value;
                if (value)
                {
                    ResetAllProperties(nameof(SchemeProducts));
                }
            }
        }
        public bool Slab
        {
            get
            {
                return slab;
            }
            set
            {
                slab = value;
                if (value)
                {
                    ResetAllProperties(nameof(Slab));
                }
            }
        }
        public bool Customer
        {
            get
            {
                return customer;
            }
            set
            {
                customer = value;
                if (value)
                {
                    ResetAllProperties(nameof(Customer));
                }
            }
        }
        private void ResetAllProperties(string propertyName)
        {
            if (nameof(Details) != propertyName) { details = false; }
            if (nameof(SchemeProducts) != propertyName) { schemeProducts = false; }
            if (nameof(Slab) != propertyName) { slab = false; }
            if (nameof(Customer) != propertyName) { customer = false; }

        }
    }
}

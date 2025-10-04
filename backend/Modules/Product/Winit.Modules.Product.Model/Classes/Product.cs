using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;
using Winit.Modules.Product.Model.Interfaces;

namespace Winit.Modules.Product.Model.Classes
{
    public class Product : BaseModel, IProduct
    {
        public string OrgUID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductAliasName { get; set; }
        public string LongName { get; set; }
        public string DisplayName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsActive { get; set; }
        public string BaseUOM { get; set; }
    }

}

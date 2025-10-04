using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.Model.Interfaces;

namespace Winit.Modules.SKU.Model.Classes
{
    public class SKU : BaseModel,ISKU
    {
        public string CompanyUID { get; set; }
        public string OrgUID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ArabicName { get; set; }
        public string AliasName { get; set; }
        public string LongName { get; set; }
        public string BaseUOM { get; set; }
        public string OuterUOM { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsStockable { get; set; }
        public string ParentUID { get; set; }
        public bool IsActive { get; set; }
        public bool IsThirdParty { get; set; }
        public string SupplierOrgUID { get; set; }
        public string SKUImage { get; set; }
        public string CatalogueURL { get; set; }
        public bool IsSelected { get; set; }
        public decimal Qty { get; set; }
        public bool IsFocusSKU { get; set; }
        public Shared.Models.Enums.ActionType ActionType { get; set; }
    }
}

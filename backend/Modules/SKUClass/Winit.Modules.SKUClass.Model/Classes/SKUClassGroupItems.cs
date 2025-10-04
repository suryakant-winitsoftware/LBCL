using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKUClass.Model.Classes
{
    public class SKUClassGroupItems:BaseModel,ISKUClassGroupItems
    {
        public string SKUClassGroupUID { get; set; }
        public string SKUCode { get; set; }
        public string SKUUID { get; set; }
        public int SerialNumber { get; set; }
        public decimal ModelQty { get; set; }
        public string ModelUoM { get; set; }
        public string SupplierOrgUID { get; set; }
        public int LeadTimeInDays { get; set; }
        public string DailyCutOffTime { get; set; }
        public bool IsExclusive { get; set; }
        public decimal MaxQTY { get; set; }
        public decimal MinQTY { get; set; }
        public bool IsExcluded { get; set; }
        public ActionType ActionType { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;


namespace Winit.Modules.Scheme.Model.Classes
{
    public class SellOutSchemeLine : BaseModel, ISellOutSchemeLine
    {
        public string SellOutSchemeHeaderUID { get; set; }
        public int LineNumber { get; set; }
        public string SkuUID { get; set; }
        public string SkuName { get; set; }
        public string SkuCode { get; set; }
        public bool IsDeleted { get; set; }
        public int Qty { get; set; }
        public int QtyScanned { get; set; }
        public string Reason { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitCreditNoteAmount { get; set; }
        public decimal TotalCreditNoteAmount { get; set; }
        public List<ISelectionItem> ResonsDDL { get; set; }
        public string SerialNos { get; set; }
        public List<ISerialNumbers> SerialNosUILevel { get; set; }
    }
}

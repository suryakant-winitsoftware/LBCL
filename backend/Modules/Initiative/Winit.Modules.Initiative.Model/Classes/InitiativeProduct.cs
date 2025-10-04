using System;
using Winit.Modules.Base.Model;
using Winit.Modules.Initiative.Model.Interfaces;

namespace Winit.Modules.Initiative.Model.Classes
{
    public class InitiativeProduct : BaseModel, IInitiativeProduct
    {
        public int InitiativeProductId { get; set; }
        public int InitiativeId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Barcode { get; set; }
        public decimal? PttPrice { get; set; }
    }
}
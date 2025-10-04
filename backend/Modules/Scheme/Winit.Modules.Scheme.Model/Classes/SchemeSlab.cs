using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class SchemeSlab : ISchemeSlab
    {
        public int Id { get; set; }
        public string PromoOrderUID { get; set; }
        public string PromoOfferUID { get; set; }
        public decimal Maximum { get; set; }
        public decimal Minimum { get; set; }
        public string OfferType { get; set; }
        public string OfferItem { get; set; }
        public string OfferItemUID { get; set; }
        public decimal OfferValue { get; set; }
        public bool IsFOCType { get; set; }
        public bool IsNewSlab { get; set; }
        public ActionType ActionType { get; set; }
    }
}

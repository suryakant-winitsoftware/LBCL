using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.Model.Interfaces
{
    public interface ISchemeSlab
    {
        int Id { get; set; }
        string PromoOrderUID { get; set; }
        string PromoOfferUID { get; set; }
        decimal Maximum { get; set; }
        decimal Minimum { get; set; }
        string OfferType { get; set; }
        decimal OfferValue { get; set; }
        bool IsFOCType { get; set; }
        string OfferItem { get; set; }
        string OfferItemUID { get; set; }
        ActionType ActionType { get; set; }
        bool IsNewSlab {  get; set; }
    }
}

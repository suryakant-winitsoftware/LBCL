using System;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Initiative.Model.Interfaces
{
    public interface IInitiativeProduct : IBaseModel
    {
        int InitiativeProductId { get; set; }
        int InitiativeId { get; set; }
        string ItemCode { get; set; }
        string ItemName { get; set; }
        string Barcode { get; set; }
        decimal? PttPrice { get; set; }
    }
}
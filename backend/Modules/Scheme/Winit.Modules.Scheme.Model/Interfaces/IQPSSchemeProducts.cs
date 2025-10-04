using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.Model.Interfaces
{
    public interface IQPSSchemeProducts
    {
        int  Id{ get; set; }
        string PromoOrderItemUID { get; set; }
        string OrderType { get; set; }
        string OrderTypeCode { get; set; }
        string SKUGroupType { get; set; }
        string SKUGroupTypeCode { get; set; }
        string SelectedValue { get; set; }
        string SelectedValueCode { get; set; }
        bool IsNewProduct { get; set; }
        bool IsSKU { get; set; }
        ActionType ActionType { get; set; }
    }
}

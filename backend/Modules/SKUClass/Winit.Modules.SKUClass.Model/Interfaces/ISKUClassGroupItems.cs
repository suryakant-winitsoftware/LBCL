using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKUClass.Model.Interfaces;

public interface ISKUClassGroupItems : IBaseModel
{
    string SKUClassGroupUID { get; set; }
    string SKUCode { get; set; }
    string SKUUID { get; set; }
    int SerialNumber { get; set; }
    decimal ModelQty { get; set; }
    string ModelUoM { get; set; }
    string SupplierOrgUID { get; set; }
    int LeadTimeInDays { get; set; }
    string DailyCutOffTime { get; set; }
    bool IsExclusive { get; set; }
    decimal MaxQTY { get; set; }
    decimal MinQTY { get; set; }
    bool IsExcluded { get; set; }
    ActionType ActionType { get; set; }
}

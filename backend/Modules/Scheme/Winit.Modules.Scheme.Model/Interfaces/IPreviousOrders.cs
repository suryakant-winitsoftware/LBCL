using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Scheme.Model.Interfaces
{
    public interface IPreviousOrders
    {
        string OrderUID { get; set; }
        string OrgUID { get; set; }
        string SKUUID { get; set; }
        string SKUCode { get; set; }
        string SKUName  { get; set; }
        decimal UnitPrice { get; set; }
        DateTime ModifiedTime { get; set; }
        decimal LastUnitPrice { get; set; }
    }
}

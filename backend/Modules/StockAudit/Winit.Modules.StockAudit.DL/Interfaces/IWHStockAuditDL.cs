using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.StockAudit.DL.Interfaces
{
    public interface IWHStockAuditDL
    {
        Task<int> CUDWHStock(Winit.Modules.StockAudit.Model.Classes.WHStockAuditRequestTemplateModel wHRequestTempleteModel);
    }
}

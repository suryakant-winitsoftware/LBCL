using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.StockAudit.BL.Interfaces
{
    public interface IWHStockAuditBL
    {
        public Task<int> CUDWHStock(Winit.Modules.StockAudit.Model.Classes.WHStockAuditRequestTemplateModel wHStockAuditRequestTemplateModel);
    }
}

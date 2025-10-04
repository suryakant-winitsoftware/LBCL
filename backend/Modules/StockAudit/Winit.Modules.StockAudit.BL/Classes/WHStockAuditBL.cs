using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StockAudit.BL.Interfaces;

namespace Winit.Modules.StockAudit.BL.Classes
{
    public class WHStockAuditBL: IWHStockAuditBL
    {
        protected readonly Winit.Modules.StockAudit.DL.Interfaces.IWHStockAuditDL _whStockAuditDL = null;
        IServiceProvider _serviceProvider = null;
        public WHStockAuditBL(DL.Interfaces.IWHStockAuditDL whStockAuditBL, IServiceProvider serviceProvider)
        {
            _whStockAuditDL = whStockAuditBL;
            _serviceProvider = serviceProvider;
        }
        public async Task<int> CUDWHStock(Winit.Modules.StockAudit.Model.Classes.WHStockAuditRequestTemplateModel wHStockAuditRequestTemplateModel)
        {
            return await _whStockAuditDL.CUDWHStock(wHStockAuditRequestTemplateModel);
        }
    }
}

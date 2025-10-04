using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.StockAudit.Model.Classes
{
    public class WHStockAuditRequestTemplateModel
    {
        public WHStockAuditItemView WHStockAuditItemView { get; set; }
        public List<WHStockAuditDetailsItemView> WHStockAuditDetailsItemView { get; set; }
    }
}

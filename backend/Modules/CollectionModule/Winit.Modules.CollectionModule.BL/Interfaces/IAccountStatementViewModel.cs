using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.CollectionModule.BL.Interfaces
{
    public interface IAccountStatementViewModel
    {
        public Winit.Modules.Store.Model.Classes.Store[] _UsersList { get; set; }
        public List<Model.Classes.AccStoreLedger> oStatementDisplay { get; set; }
        public List<Model.Classes.AccPayable> oStatementDisplayPayable { get; set; }
        Task StatementReportCustomers(string CustomerCode);
        Task ViewAccountStatement(string FromDate, string ToDate, string Customer);
    }
}

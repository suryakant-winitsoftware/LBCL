using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CollectionModule.BL.Classes.Statement
{
    public abstract class AccountStatementBaseViewModel : IAccountStatementViewModel
    {
        protected IServiceProvider _serviceProvider;
        protected IAppConfig _appConfig;
        protected IAppUser _appUser;
        public Winit.Modules.Store.Model.Classes.Store[] _UsersList { get; set; }
        public List<Model.Classes.AccStoreLedger> oStatementDisplay { get; set; } 
        public List<Model.Classes.AccPayable> oStatementDisplayPayable { get; set; } 

        public AccountStatementBaseViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser)
        {
            _serviceProvider = serviceProvider;
            _appConfig = appConfig;
            _appUser = appUser;
            _UsersList = new Winit.Modules.Store.Model.Classes.Store[0];
            oStatementDisplay = new List<Model.Classes.AccStoreLedger>();
            oStatementDisplayPayable = new List<Model.Classes.AccPayable>();
        }


        public abstract Task StatementReportCustomers(string CustomerCode);
        public abstract Task ViewAccountStatement(string FromDate, string ToDate, string Customer);
    }
}

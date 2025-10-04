using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Contact.Model.Interfaces;
using Winit.Modules.Survey.Model.Classes;
using Winit.Modules.Survey.Model.Interfaces;

namespace Winit.Modules.Survey.BL.Interfaces
{
    public interface IStoreAnalyticsExcelViewModel : Winit.Modules.Base.BL.Interfaces.ITableGridViewModel
    {
         List<Winit.Modules.Survey.Model.Classes.StoreRollingStatsModel> ExcelStoreRollingStats { get; set; }
        public List<Winit.Modules.Survey.Model.Interfaces.IStoreRollingStatsModel> storeRollingStatsList { get; set; }

        Task InsertStoreRollingStats();
    }
}

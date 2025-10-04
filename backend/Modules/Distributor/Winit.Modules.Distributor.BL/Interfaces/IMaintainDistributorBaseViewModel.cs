using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Distributor.BL.Interfaces
{
    public interface IMaintainDistributorBaseViewModel:ITableGridViewModel
    {
        bool IsLoad { get; set; }
        
        List<Model.Classes.Distributor> DispayDistributorsList { get; set; }
        Task PopulateViewModel();
        Task OnFilterApply(Dictionary<string, string> filterCriteria);
        Task GetDataFromAPIAsync();
    }
}

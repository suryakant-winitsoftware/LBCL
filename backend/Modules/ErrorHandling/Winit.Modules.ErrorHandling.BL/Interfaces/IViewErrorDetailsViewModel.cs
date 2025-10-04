using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ErrorHandling.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ErrorHandling.BL.Interfaces
{
    public interface IViewErrorDetailsViewModel
    {
        public List<IErrorDetail> ErrorDetailsList { get; set; }
        Task ApplySort(SortCriteria sortCriteria);
        Task PopulateViewModel();
        Task ApplyFilter(List<FilterCriteria> filterCriterias);
    }
}

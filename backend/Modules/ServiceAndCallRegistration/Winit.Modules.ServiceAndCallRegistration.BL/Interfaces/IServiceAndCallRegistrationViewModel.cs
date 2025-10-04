using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ServiceAndCallRegistration.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ServiceAndCallRegistration.BL.Interfaces
{
    public interface IServiceAndCallRegistrationViewModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }
        public List<ISelectionItem> CustTypeSelectionList { get; set; }
        public List<ISelectionItem> ProductCategorySelectionList { get; set; }
        public List<ISelectionItem> BrandCodeSelectionList { get; set; }
        public List<ISelectionItem> ServiceTypeCodeSelectionList { get; set; }
        public List<ISelectionItem> WarrentyStatusSelectionList { get; set; }
        public ICallRegistration CallRegistrationDetails { get; set; }
        public ICallRegistrationResponce CallRegistrationResponce {  get; set; }
        public List<Winit.Modules.FileSys.Model.Interfaces.IFileSys>? FileSys { get; set; }
        public IServiceRequestStatus ServiceStatus { get; set; }
        public IServiceRequestStatusResponce serviceRequestStatusResponce { get; set; }
        Task<IServiceRequestStatusResponce> GetServiceStatusBasedOnNumber(IServiceRequestStatus serviceNumber);
        Task PopulateDropDowns();
        Task<ICallRegistrationResponce> SaveCallRegistrationDetails();
        Task PopulateCallRegistrations();
        Task PopulateCallRegistrationItemDetailsByUID(string serviceCallNumber);
        Task ApplyFilterForDealer(List<FilterCriteria> filterCriterias);
        Task ApplySort(SortCriteria sortCriteria);
        Task PageIndexChanged(int pageNumber);

        public List<ICallRegistration> CallRegisteredDataList { get; set; }
        public ICallRegistration CallRegisteredItemDetails { get; set; }

    }
}

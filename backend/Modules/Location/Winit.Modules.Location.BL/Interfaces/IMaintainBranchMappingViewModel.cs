using Winit.Modules.Base.BL.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.BL.Interfaces
{
    public interface IMaintainBranchMappingViewModel : ITableGridViewModel
    {
        public List<IBranch> BranchDetailsList { get; set; }
        public IBranch ViewBranchDetails { get; set; }
        Task PopulateViewModel();
        Task PopulateBranchDetailsByUID(string UID);
        List<ILocation> CitiesForSelection { get; set; }
        List<ILocation> StatesForSelection { get; set; }
        List<ILocation> LocalitiesForSelection { get; set; }
        Task GetStatesForSelection();
        Task GetCitiesForSelection(List<ILocation> selectedStates);
        Task GetLocalitiesForSelection(List<ILocation> selectedCities);
        Task<bool> SaveOrUpdateBranchDetails(IBranch viewBranchDetails, bool @operator);
        Task<bool> SaveStoreDetailsDetails(ISalesOffice salesOffice);
        Task<bool> DeleteSalesOfficeDetails(ISalesOffice salesOffice);
        Task GetBranchMappingSalesOffices(string uID);
        public ISalesOffice SalesOffice { get; set; }
        public List<ISalesOffice> SalesOfficeDetails { get; set; }
        public List<ISelectionItem> SalesOfficeOrgType { get; set; }

        new Task OnFilterApply(IDictionary<string, string> filterCriteria);
        public List<ISalesOffice> CompleteSalesOfficeDetailsList { get; set; }
        //  Task GetStoreDetails(List<ISalesOffice> salesOffices);

    }
}

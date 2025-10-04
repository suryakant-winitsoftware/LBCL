using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.JobPosition.Model.Interfaces;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Modules.User.Model.Classes;
using Winit.Modules.User.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.User.BL.Interfaces
{
    public interface IAddEditEmployeeViewModel
    {
        public Winit.Modules.ApprovalEngine.Model.Interfaces.IAllApprovalRequest AllApprovalRequestData { get; set; }
        public Dictionary<string, List<EmployeeDetail>> ApprovalUserCodes { get; set; }
        public List<IListItem> AuthTypeDD { get; set; }
        public List<IListItem> DepartmentDD { get; set; }
        public List<IOrg> orgDD { get; set; }
        public IOrg org { get; set; }
        public string Uid { get; set; }
        public List<IRole> roleDD { get; set; }
        public IRole role { get; set; }
        public List<IEmp> reportsToDD { get; set; }
        bool IsBranchApplicable { get; set; }
        public List<IEmpOrgMappingDDL> EmpOrgMappingDD { get; set; }
        public IEmp reportsTo { get; set; }
        public EmpDTOModel EmpDTOModelmaintainUser { get; set; }
        public List<EmpOrgMapping> EmpOrgMapping { get; set; }
        public string? OrgCode { get; set; }
        public string? OrgName { get; set; }
        public IEmpDTO EmpDTOmaintainUser { get; set; }
        public IJobPosition jobPosition { get; set; }
        //  public ILocationTypeAndValue locationTypeAndValue { get; set; }
        public List<ISelectionItem> AuthTypeSelectionItems { get; set; }
        // public List<ISelectionItem> OrgSelectionItems { get; set; }
        public List<ISelectionItem> RoleSelectionItems { get; set; }
        public List<ISelectionItem> BranchSelectionItems { get; set; }
        public List<ISelectionItem> SalesOfficeSelectionItems { get; set; }
        public List<ISelectionItem> ReportsToSelectionItems { get; set; }
        public List<ISelectionItem> ApplicableOrganizationSelectionItems { get; set; }
        public List<ISelectionItem> DepartmentSelectionItems { get; set; }
        public UIComponents.Common.CustomControls.DropDown Roleddl { get; set; }
        public UIComponents.Common.CustomControls.DropDown Reportsddl { get; set; }


        string LocationMappingLable { get; set; }
        bool IsLocationMappingVisible { get; set; }
        List<ISelectionItem> LocationMappingList { get; set; }

        string SKUMappingLable { get; set; }
        bool IsSKUMappingVisible { get; set; }
        List<ISelectionItem> SKUMappingList { get; set; }


        Task PopulateViewModel(string UID, bool IsNew);
        Task PopulateViewModelForOrg_RoleMapping(string UID, bool IsNew);
        Task PopulateViewModelForApplicableOrg(string UID, bool IsNew);
        //Task OnOrgDD();
        Task OnRoleDD(string UID);
        Task OnReportsDD(string UID);
        Task OnDepartmentDD();
        Task GetEmpOrgMappingDD(string empuid);
        Task<(string, bool)> SaveUpdateOrg_RoleMapping(IJobPosition jobPosition, bool Iscreate);
        Task<bool> SaveUpdateEmployee(IEmpDTO user, bool Iscreate);
        // Task SaveUpdateEmpOrgMapping(List< EmpOrgMapping> empOrgMapping, bool Iscreate);
        Task<bool> SaveUpdateEmpOrgMapping(List<EmpOrgMapping> empOrgMappings);
        Task<string> DeleteEmpOrgMapping(string UID);
        Task SetEditForAuthTypeDD(IEmpDTO empDTO);
        Task<(List<EmpOrgMapping>, List<EmpOrgMappingDDL>)> GetGridviewData(bool isEdit = false);
        void OnAuthTypeSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent);
        //void OnOrgSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent);
        Task OnRoleSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent);
        Task OnBranchSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent);
        void OnSalesOfficeSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent);
        void OnReportsToSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent);
        void OnDepartmentSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent);
        void OnApplicableOrganizationSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent);
        //ramana
        Task<string> CreateUpdateFileSysData(Winit.Modules.FileSys.Model.Interfaces.IFileSys fileSys);
        void OnSKUMappingSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent);
        void OnLocationMappingSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent);

        //Added By Ravichandra 
        // List<ISelectionItem> LocationTypesForDD { get; set; }
        //List<ISelectionItem> Locations { get; set; }
        // Task OnLocationTypeSelected(DropDownEvent dropDownEvent);
        // Task OnLocationSelected(DropDownEvent dropDownEvent);

        //niranjan
        Task GetTheBranchesList();
        Task GetTheSalesOfficesList(string BranchUID);

        //Selva New
        public string UserCreationStoreUID { get; set; }
        // Task CreateUpdateUserLocationDetails();
        // Task GetLocationType();
        // Task SetEditForLocation();

        //userLocation Mapping -Ravichandra
        Task GetUserLocationMappingType();
        List<ISelectionItem> UserLocationTypes { get; set; }
        List<ISelectionItem> UserLocationValues { get; set; }
        public ILocationTypeAndValue LocationTypeAndValueForLocationMapping { get; set; }
        Task<bool> CreateUpdateUserLocationTypeAndValueDetails(ILocationTypeAndValue locationTypeAndValueForLocationMapping);
        Task GetUserLocationMappingValue(string? code);
        Task PopulateUserLocationTypeAndValue(string loginID);

        Task PopulateUsersDetailsforEdit(string Uid);
        Task EmployeeViewModel();

    }
}

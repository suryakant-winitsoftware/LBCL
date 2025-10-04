using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.JobPosition.Model.Interfaces;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.User.BL.Interfaces;
using Winit.Modules.User.Model.Classes;
using Winit.Modules.User.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.User.BL.Classes
{
    public abstract class AddEditEmployeeBaseViewModel : IAddEditEmployeeViewModel
    {
        public Winit.Modules.ApprovalEngine.Model.Interfaces.IAllApprovalRequest AllApprovalRequestData { get; set; }
        public Dictionary<string, List<EmployeeDetail>> ApprovalUserCodes { get; set; }
        protected CommonFunctions _commonFunctions;
        public List<IListItem> AuthTypeDD { get; set; }
        public List<IListItem> DepartmentDD { get; set; }
        public List<IOrg> orgDD { get; set; }
        public IOrg org { get; set; }
        public string? OrgCode { get; set; }
        public string? OrgName { get; set; }
        //  public List<ISelectionItem> Locations { get; set; } = new();
        protected List<Winit.Modules.Location.Model.Classes.Location> LocationsByType { get; set; }
        protected List<ILocationType> LocationTypes { get; set; }
        public List<IRole> roleDD { get; set; }
        public IRole role { get; set; }
        public List<IEmp> reportsToDD { get; set; }
        public string Uid { get; set; }
        //   public List<ISelectionItem> LocationTypesForDD { get; set; } 
        public bool IsBranchApplicable { get; set; }
        public List<EmpOrgMapping> EmpOrgMapping { get; set; }
        private string msg = "";

        public IEmp reportsTo { get; set; }
        public IJobPosition jobPosition { get; set; } = new Winit.Modules.JobPosition.Model.Classes.JobPosition();
        public IMaintainUser maintainUser { get; set; }
        public EmpDTOModel EmpDTOModelmaintainUser { get; set; }
        public IEmpDTO EmpDTOmaintainUser { get; set; } = new EmpDTO()
        {
            Emp = new Winit.Modules.Emp.Model.Classes.Emp(),
            EmpInfo = new EmpInfo(),
            JobPosition = new Winit.Modules.JobPosition.Model.Classes.JobPosition(),
        };
        public List<ISelectionItem> AuthTypeSelectionItems { get; set; }
        // public List<ISelectionItem> OrgSelectionItems { get; set; }
        public List<ISelectionItem> RoleSelectionItems { get; set; }
        public List<ISelectionItem> BranchSelectionItems { get; set; }
        public List<ISelectionItem> SalesOfficeSelectionItems { get; set; }
        public List<ISelectionItem> ReportsToSelectionItems { get; set; }
        public List<ISelectionItem> DepartmentSelectionItems { get; set; }
        public List<ISelectionItem> ApplicableOrganizationSelectionItems { get; set; }// Dropdown
        public List<IEmpOrgMappingDDL> EmpOrgMappingDD { get; set; }// List

        //Location && SKU Mapping 
        public string LocationMappingLable { get; set; }
        public bool IsLocationMappingVisible { get; set; }
        public List<ISelectionItem> LocationMappingList { get; set; } = new List<ISelectionItem>();
        protected List<LocationTemplate> LocationTemplatesCopy { get; set; }
        public string SKUMappingLable { get; set; }
        public bool IsSKUMappingVisible { get; set; }
        public List<ISelectionItem> SKUMappingList { get; set; } = new List<ISelectionItem>();
        protected List<SKUTemplate> SKUTemplatesCopy { get; set; }

        public SelectionManager AuthTypeSM { get; set; }
        public SelectionManager OrgSM { get; set; }
        public SelectionManager RoleSM { get; set; }
        public SelectionManager ReportsToSM { get; set; }
        public SelectionManager EmpOrgMappingToSM { get; set; }
        public UIComponents.Common.CustomControls.DropDown Roleddl { get; set; }
        public UIComponents.Common.CustomControls.DropDown Reportsddl { get; set; }


        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appuser;
        private Winit.Modules.Base.BL.ApiService _apiService;
        protected ISelectionItem SelectedLocationMapping { get; set; }
        protected ISelectionItem SelectedSKUMapping { get; set; }

        //  protected ISelectionItem SelectedLocationType { get; set; }
        //  protected ISelectionItem SelectedLocationValue { get; set; }
        //  ISelectionItem? SelectedlocationType { get; set; }
        //  List<ISelectionItem>? Selectedlocations { get; set; }

        //New
        public string UserCreationStoreUID { get; set; }

        //USerLocation Mapping
        public List<ISelectionItem> UserLocationTypes { get; set; }
        public List<ISelectionItem> UserLocationValues { get; set; }
        public ILocationTypeAndValue LocationTypeAndValueForLocationMapping { get; set; }

        public AddEditEmployeeBaseViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IAppUser appUser,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appuser = appUser;
            _commonFunctions = new CommonFunctions();
            AuthTypeSelectionItems = new List<ISelectionItem>();
            //OrgSelectionItems = new List<ISelectionItem>();
            RoleSelectionItems = new List<ISelectionItem>();
            BranchSelectionItems = new List<ISelectionItem>();
            SalesOfficeSelectionItems = new List<ISelectionItem>();
            ReportsToSelectionItems = new List<ISelectionItem>();
            ApplicableOrganizationSelectionItems = new List<ISelectionItem>();
            EmpDTOmaintainUser = new EmpDTO();
            EmpDTOmaintainUser.EmpInfo = new EmpInfo();
            EmpDTOmaintainUser.JobPosition = new Winit.Modules.JobPosition.Model.Classes.JobPosition();
            reportsTo = new Winit.Modules.Emp.Model.Classes.Emp();
            role = new Winit.Modules.Role.Model.Classes.Role();
            org = new Winit.Modules.Org.Model.Classes.Org();
            orgDD = new List<IOrg>();
            roleDD = new List<IRole>();
            reportsToDD = new List<IEmp>();
            jobPosition = new Winit.Modules.JobPosition.Model.Classes.JobPosition();
            AuthTypeDD = new List<IListItem>();
            DepartmentDD = new List<IListItem>();
            EmpOrgMappingDD = new List<IEmpOrgMappingDDL>();
            EmpOrgMapping = new List<EmpOrgMapping>();
            DepartmentSelectionItems = new List<ISelectionItem>();
            // locationTypeAndValue = new LocationTypeAndValue();
            // LocationTypesForDD = [];
            //  LocationTypes = new List<ILocationType>();
            UserLocationTypes = new List<ISelectionItem>();
            UserLocationValues = new List<ISelectionItem>();
            LocationTypeAndValueForLocationMapping = new LocationTypeAndValue();
            AllApprovalRequestData = new AllApprovalRequest();
        }
        public virtual async Task EmployeeViewModel()
        {
            AuthTypeDD.Clear();
            AuthTypeDD.AddRange(await GetAuthTypeDD());
            if (AuthTypeDD != null && AuthTypeDD.Any())
            {
                AuthTypeSelectionItems.Clear();
                AuthTypeSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<IListItem>
                (AuthTypeDD, new List<string> { "UID", "Name", "Name" }));
                AuthTypeSM = new SelectionManager(AuthTypeSelectionItems, SelectionMode.Single);
            }

        }
        public virtual async Task PopulateViewModel(string UID, bool IsNew)
        {
            OrgCode = _appuser.SelectedJobPosition?.OrgUID;
            OrgName = _appuser.CurrentOrg?.Name;
            if (OrgCode != null)
            {
                IOrg org = await GetOrgDetailsByUID(OrgCode);
                if (org != null)
                {
                    OrgName = org.Name;
                }
            }
            //AuthTypeDD.Clear();
            //AuthTypeDD.AddRange(await GetAuthTypeDD());
            //if (AuthTypeDD != null && AuthTypeDD.Any())
            //{
            //    AuthTypeSelectionItems.Clear();
            //    AuthTypeSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<IListItem>
            //    (AuthTypeDD, new List<string> { "UID", "Name", "Name" }));
            //    AuthTypeSM = new SelectionManager(AuthTypeSelectionItems, SelectionMode.Single);
            //}
            if (IsNew)
            {
                EmpDTOmaintainUser = _serviceProvider.CreateInstance<IEmpDTO>();
                EmpDTOmaintainUser.EmpInfo = _serviceProvider.CreateInstance<IEmpInfo>();
                EmpDTOmaintainUser.Emp = _serviceProvider.CreateInstance<IEmp>();
                EmpDTOmaintainUser.JobPosition = _serviceProvider.CreateInstance<IJobPosition>();
            }
            else
            {
                // await PopulateUsersDetailsforEdit(UID);
                //SetEditForAuthTypeDD(EmpDTOmaintainUser);
            }
            // await GetLocationTypeFromAPI();
            // await SetEditForLocation();
        }
        public async Task SetEditForAuthTypeDD(IEmpDTO empDTO)
        {
            var selectedAuthType = AuthTypeSelectionItems.Find(e => e.Label == empDTO.Emp.AuthType);
            if (selectedAuthType != null)
            {
                selectedAuthType.IsSelected = true;
            }
        }
        public abstract Task<IOrg> GetOrgDetailsByUID(string orgUID);

        public virtual async Task PopulateViewModelForOrg_RoleMapping(string UID, bool IsNew)
        {
            await PopulateUsersDetailsforEdit(UID);
            // await SetEditForOrgDD(jobPosition);
            await SetEditForRolesDD(jobPosition);
            await SetEditForReportsToDD(jobPosition);
            await SetEditForDepartmentDD(jobPosition);
        }
        public virtual async Task PopulateViewModelForApplicableOrg(string UID, bool IsNew)
        {
            await PopulateUsersDetailsforEdit(UID);
            await SetEditForEmpOrgMappingDD(EmpDTOmaintainUser);
        }
        //public virtual async Task OnOrgDD()
        //{
        //    orgDD.Clear();
        //    orgDD.AddRange(await GetOrgDD());
        //    if (orgDD != null && orgDD.Any())
        //    {
        //        OrgSelectionItems.Clear();
        //        OrgSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<IOrg>
        //        (orgDD, new List<string> { "UID", "Name", "Name" }));
        //        OrgSM = new SelectionManager(OrgSelectionItems, SelectionMode.Single);
        //    }

        //}
        //public async Task SetEditForOrgDD(IJobPosition org)
        //{
        //    var selectedOrgType = OrgSelectionItems.Find(e => e.UID == org.OrgUID);
        //    if (selectedOrgType != null)
        //    {
        //        selectedOrgType.IsSelected = true;
        //    }
        //    //EmpDTOmaintainUser = listItem;
        //}
        public virtual async Task OnRoleDD(string? UID)
        {

            if (!string.IsNullOrEmpty(UID))
            {
                roleDD.Clear();
                roleDD.AddRange(await GetRoleDD(UID));
                RoleSelectionItems.Clear();
                RoleSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<IRole>
                (roleDD, new List<string> { "UID", "RoleNameEn", "RoleNameEn" }));
                RoleSM = new SelectionManager(RoleSelectionItems, SelectionMode.Single);
            }
            else
            {
                RoleSM = new SelectionManager(new(), SelectionMode.Single);
            }

        }

        public async Task GetTheBranchesList()
        {
            await GetAllTheBranches();
        }
        public async Task GetTheSalesOfficesList(string BranchUID)
        {
            await GetSalesOfficesByBranchUID(BranchUID);
        }
        public abstract Task GetAllTheBranches();
        public abstract Task GetSalesOfficesByBranchUID(string BranchUID);

        public async Task SetEditForRolesDD(IJobPosition role)
        {
            await OnRoleDD(role.OrgUID);
            var selectedRoleType = RoleSelectionItems.Find(e => e.UID == role.UserRoleUID);
            if (selectedRoleType != null)
            {
                selectedRoleType.IsSelected = true;
                var selectedRole = roleDD.Find(e => selectedRoleType.UID == e.UID);
                if (selectedRole != null)
                {
                    IsBranchApplicable = selectedRole.IsBranchApplicable;
                    await SetEditForBranchDD();
                }
            }
        }
        public async Task SetEditForBranchDD()
        {
            var selectedBranch = BranchSelectionItems.Find(e => e.UID == jobPosition.BranchUID);
            if ((selectedBranch != null))
            {
                selectedBranch.IsSelected = true;
                await GetTheSalesOfficesList(jobPosition.BranchUID);
                SetEditForSalesOfficeDD();
            }
        }
        public void SetEditForSalesOfficeDD()
        {
            var selectedSalesOffice = SalesOfficeSelectionItems.Find(e => e.UID == jobPosition.SalesOfficeUID);
            if ((selectedSalesOffice != null)) selectedSalesOffice.IsSelected = true;

        }
        public virtual async Task OnReportsDD(string UID)
        {
            reportsToDD.Clear();
            reportsToDD.AddRange(await GetReportsToDD(UID));
            if (reportsToDD != null)
            {
                ReportsToSelectionItems.Clear();
                ReportsToSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<IEmp>
                (reportsToDD, new List<string> { "JobPositionUid", "Code", "Name" }));
                ReportsToSM = new SelectionManager(ReportsToSelectionItems, SelectionMode.Single);
            }
            else
            {
                ReportsToSM = new SelectionManager(new(), SelectionMode.Single);

            }
        }
        protected void ConvertLocationMappingDataintoSelectionItem()
        {
            for (int i = 0; i < LocationTemplatesCopy?.Count; i++)
            {
                ISelectionItem selectionItem = new SelectionItem()
                {
                    Code = LocationTemplatesCopy[i].TemplateCode,
                    Label = LocationTemplatesCopy[i].TemplateName,
                    UID = LocationTemplatesCopy[i].UID,
                };

                LocationMappingList.Add(selectionItem);
            }
        }
        protected void ConvertSKUMappingDataintoSelectionItem()
        {
            for (int i = 0; i < SKUTemplatesCopy?.Count; i++)
            {
                ISelectionItem selectionItem = new SelectionItem()
                {
                    Code = SKUTemplatesCopy[i].TemplateCode,
                    Label = SKUTemplatesCopy[i].TemplateName,
                    UID = SKUTemplatesCopy[i].UID,
                };

                SKUMappingList.Add(selectionItem);
            }
        }
        public async Task SetEditForReportsToDD(IJobPosition emp)
        {
            await OnReportsDD(emp.UserRoleUID);
            var selectedReportsToType = ReportsToSelectionItems.Find(e => e.UID == emp.ReportsToUID);
            if (selectedReportsToType != null)
            {
                selectedReportsToType.IsSelected = true;
            }
        }
        public virtual async Task OnDepartmentDD()
        {
            DepartmentDD.Clear();
            DepartmentDD.AddRange(await GetDepartmentDD());

            if (DepartmentDD != null && DepartmentDD.Any())
            {
                DepartmentSelectionItems.Clear();
                DepartmentSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<IListItem>
                (DepartmentDD, new List<string> { "UID", "Name", "Name" }));
                AuthTypeSM = new SelectionManager(AuthTypeSelectionItems, SelectionMode.Single);
            }
            else
            {
                AuthTypeSM = new SelectionManager(new(), SelectionMode.Single);

            }
        }
        public async Task SetEditForDepartmentDD(IJobPosition listItem)
        {
            var selectedDepartmentType = DepartmentSelectionItems.Find(e => e.Label == listItem.Department);
            if (selectedDepartmentType != null)
            {
                selectedDepartmentType.IsSelected = true;
            }
        }
        public async Task GetEmpOrgMappingDD(string empuid)
        {
            var empOrgMappingDD = await GetEmpOrgMappingToDD(empuid);
            if (empOrgMappingDD != null)
            {
                ApplicableOrganizationSelectionItems.Clear();
                var selectionItems = empOrgMappingDD.Select(org => new SelectionItem
                {
                    UID = org.UID, // Assuming UID is used as value
                    Code = org.Code,  // Concatenating Code and Name
                    Label = $"[{org.Code}] {org.Name}"  // Concatenating Code and Name
                }).ToList();

                ApplicableOrganizationSelectionItems.AddRange(selectionItems);
                EmpOrgMappingToSM = new SelectionManager(ApplicableOrganizationSelectionItems, SelectionMode.Multiple);
            }
        }
        public async Task SetEditForEmpOrgMappingDD(IEmpDTO listItemuid)
        {

            List<string> selectedOrgs = EmpDTOmaintainUser.EmpOrgMapping.Select(e => e.OrgUID).ToList();
            if (selectedOrgs != null && selectedOrgs.Any())
            {
                foreach (var item in ApplicableOrganizationSelectionItems.Where(e => selectedOrgs.Contains(e.UID)))
                {
                    item.IsSelected = true;
                }
            }
        }
        public void OnAuthTypeSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            EmpDTOmaintainUser.Emp.AuthType = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                // sku.Code = selecetedValue?.Code;
                EmpDTOmaintainUser.Emp.AuthType = selecetedValue?.Label;
            }
        }
        public void OnLocationMappingSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
                {
                    SelectedLocationMapping = dropDownEvent.SelectionItems.FirstOrDefault();
                    LocationMappingLable = SelectedLocationMapping?.Label;
                }
            }
            IsLocationMappingVisible = false;

        }
        public void OnSKUMappingSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
                {
                    SelectedSKUMapping = dropDownEvent.SelectionItems.FirstOrDefault();
                    SKUMappingLable = SelectedSKUMapping?.Label;
                }
            }
            IsSKUMappingVisible = false;
        }
        //public async void OnOrgSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        //{
        //    jobPosition.OrgUID = null;
        //    jobPosition.UserRoleUID = null;
        //    jobPosition.ReportsToUID = null;

        //    if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        //    {
        //        var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
        //        string UID = selecetedValue.UID;
        //        //string UID = _appuser.SelectedJobPosition.OrgUID;
        //        RoleSelectionItems.Clear();
        //        await OnRoleDD(UID);
        //        jobPosition.OrgUID = selecetedValue.UID;
        //        Roleddl.GetLoad();
        //        ReportsToSelectionItems.Clear();
        //        Reportsddl.GetLoad();
        //    }
        //}
        //public async Task OnRoleSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        //{
        //    //jobPosition.UserRoleUID = null;
        //    //jobPosition.ReportsToUID = null;

        //    if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        //    {
        //        var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
        //        string UID = selecetedValue?.UID;
        //        IRole? SelecetedRole = roleDD.Find(p => p.UID == UID);
        //        if (SelecetedRole != null && SelecetedRole.IsBranchApplicable)
        //        {
        //            IsBranchApplicable = true;
        //        }
        //        else { IsBranchApplicable = false; }
        //        await OnReportsDD(UID);
        //        jobPosition.UserRoleUID = selecetedValue.UID;
        //        //ReportsToSelectionItems.Clear();
        //        Reportsddl.GetLoad();
        //    }
        //    else if (dropDownEvent.SelectionItems.ite)
        //    {
        //        jobPosition.UserRoleUID = null;
        //    }
        //}

        public async Task OnRoleSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            // jobPosition.UserRoleUID = null;
            // jobPosition.ReportsToUID = null;

            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                string UID = selecetedValue?.UID;
                IRole? SelecetedRole = roleDD.Find(p => p.UID == UID);

                if (SelecetedRole != null && SelecetedRole.IsBranchApplicable)
                {
                    IsBranchApplicable = true;
                }
                else
                {
                    IsBranchApplicable = false;
                }

                await OnReportsDD(UID);
                jobPosition.UserRoleUID = selecetedValue.UID;

                // ReportsToSelectionItems.Clear();
                Reportsddl.GetLoad();
            }
            else if (dropDownEvent?.SelectionItems?.Count == 0)
            {
                jobPosition.UserRoleUID = null;
                // await OnReportsDD(UID);
            }
        }

        public async Task OnBranchSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            //jobPosition.BranchUID = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                string UID = selecetedValue?.UID;
                jobPosition.BranchUID = UID;

                await GetTheSalesOfficesList(UID);
            }
            else if (dropDownEvent?.SelectionItems?.Count == 0)
            {
                jobPosition.BranchUID = null;
            }

        }
        public void OnSalesOfficeSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            // jobPosition.SalesOfficeUID = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                string UID = selecetedValue?.UID;
                jobPosition.SalesOfficeUID = UID;
            }
            else if (dropDownEvent.SelectionItems == null)
            {
                jobPosition.SalesOfficeUID = null;
            }
        }
        public void OnReportsToSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            // jobPosition.ReportsToUID = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                //string UID = selecetedValue?.UID;
                jobPosition.ReportsToUID = selecetedValue.UID;
            }
            else if (dropDownEvent?.SelectionItems?.Count == 0)
            {
                jobPosition.ReportsToUID = null;
            }
        }
        public void OnDepartmentSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            //jobPosition.Department = null;

            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                jobPosition.Department = selecetedValue?.Label;
            }
            else if (dropDownEvent?.SelectionItems?.Count == 0)
            {
                jobPosition.Department = null;
            }
        }
        public void OnApplicableOrganizationSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            jobPosition.OrgUID = null;

            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                jobPosition.OrgUID = selecetedValue.UID;
            }
        }


        //public async Task OnLocationTypeSelected(DropDownEvent dropDownEvent)
        //{
        //    locationTypeAndValue.LocationType = null;
        //    if (dropDownEvent != null && dropDownEvent.SelectionItems != null)
        //    {
        //        SelectedlocationType = dropDownEvent.SelectionItems.FirstOrDefault();
        //        if (SelectedlocationType != null)
        //        {
        //            await GetLocationFromAPI(SelectedlocationType.Code);
        //            locationTypeAndValue.LocationType = SelectedlocationType.Code;
        //            /// SelectedLocationType = SelectedlocationType;

        //        }
        //    }
        //}
        //public async Task OnLocationSelected(DropDownEvent dropDownEvent)
        //{
        //    locationTypeAndValue.LocationValue = null;
        //    if (dropDownEvent != null)
        //    {
        //        Selectedlocations = dropDownEvent.SelectionItems;
        //        locationTypeAndValue.LocationValue = Selectedlocations.First().Code;
        //        // SelectedLocationValue = Selectedlocations;
        //    }
        //}






        public async Task<(List<EmpOrgMapping>, List<EmpOrgMappingDDL>)> GetGridviewData(bool isEdit = false)
        {
            List<EmpOrgMapping> empOrgMappings = new List<EmpOrgMapping>();
            List<EmpOrgMappingDDL> empOrgMappingDDLs = new List<EmpOrgMappingDDL>();
            foreach (var item in ApplicableOrganizationSelectionItems)
            {
                if (item.IsSelected && !EmpOrgMappingDD.Any(i => i.OrgCode == item.Code))
                {
                    EmpOrgMapping empOrgMapping = new EmpOrgMapping();
                    if (isEdit)
                    {
                        empOrgMapping.UID = EmpDTOmaintainUser.EmpOrgMapping.ToList().Find(e => e.OrgUID == item.UID)!.UID;
                    }
                    else
                    {
                        empOrgMapping.UID = Guid.NewGuid().ToString();
                    }
                    empOrgMapping.EmpUID = EmpDTOmaintainUser.Emp.UID;
                    empOrgMapping.OrgUID = item.UID;
                    empOrgMapping.CreatedBy = _appuser.Emp.UID;
                    empOrgMapping.ModifiedBy = _appuser.Emp.UID;
                    empOrgMapping.CreatedTime = DateTime.Now;
                    empOrgMapping.ModifiedTime = DateTime.Now;
                    empOrgMappings.Add(empOrgMapping);

                    EmpOrgMappingDDL empOrgMappingDDL = new EmpOrgMappingDDL();
                    empOrgMappingDDL.EmpOrgMappingUID = empOrgMapping.UID;
                    empOrgMappingDDL.EmpUID = empOrgMapping.EmpUID;
                    empOrgMappingDDL.OrgUID = empOrgMapping.OrgUID;
                    empOrgMappingDDL.OrgCode = item.Code;
                    empOrgMappingDDL.OrgName = item.Label;
                    empOrgMappingDDLs.Add(empOrgMappingDDL);
                }
            }
            return await Task.FromResult((empOrgMappings, empOrgMappingDDLs));
        }
        public async Task<bool> SaveUpdateEmpOrgMapping(List<EmpOrgMapping> empOrgMappings)
        {
            return await CreateUpdateEmpOrgMapping(empOrgMappings);
        }
        public async Task<(string, bool)> SaveUpdateOrg_RoleMapping(IJobPosition jobPositions, bool Iscreate)
        {
            // aziz
            //if (Iscreate)
            //{
            //   // jobPosition.UID = Guid.NewGuid().ToString();
            //    jobPosition.CreatedBy = _appuser.Emp.UID;
            //    jobPosition.ModifiedBy = _appuser.Emp.UID;
            //    jobPosition.EmpUID = EmpDTOmaintainUser.Emp.UID;
            //    jobPosition.OrgUID = _appuser.SelectedJobPosition.OrgUID;
            //    jobPosition.CreatedTime = DateTime.Now;
            //    jobPosition.ModifiedTime = DateTime.Now;
            //    await CreateUpdateOrg_RoleMapping(jobPosition, true);
            //}
            //else
            //{
            if (EmpDTOmaintainUser.JobPosition?.UID != null)
            {
                jobPositions.UID = EmpDTOmaintainUser.JobPosition.UID;
                jobPositions.EmpUID = EmpDTOmaintainUser.Emp.UID;
                jobPositions.OrgUID = _appuser.SelectedJobPosition.OrgUID;
                jobPositions.ModifiedBy = _appuser.Emp.UID;
                jobPositions.ModifiedTime = DateTime.Now;
                jobPositions.BranchUID = jobPosition.BranchUID;
                jobPositions.CreatedBy = _appuser.Emp.CreatedBy;

                await CreateUpdateOrg_RoleMapping(jobPosition, false);
            }
            //else
            //{
            //   // jobPosition.UID = Guid.NewGuid().ToString();
            //    jobPosition.CreatedBy = _appuser.Emp.UID;
            //    jobPosition.ModifiedBy = _appuser.Emp.UID;
            //    jobPosition.EmpUID = EmpDTOmaintainUser.Emp.UID;
            //    jobPosition.OrgUID = _appuser.SelectedJobPosition.OrgUID;
            //    jobPosition.CreatedTime = DateTime.Now;
            //    jobPosition.ModifiedTime = DateTime.Now;
            //    await CreateUpdateOrg_RoleMapping(jobPosition, true);
            //}

            //}
            return ("", false);
        }
        public async Task<bool> SaveUpdateEmployee(IEmpDTO user, bool iscreate)
        {
            if (iscreate)
            {
                user.Emp.UID = Guid.NewGuid().ToString();
                Uid = user.Emp.UID;
                user.Emp.Name = EmpDTOmaintainUser.Emp.Name;
                user.Emp.ApprovalStatus = "Pending";
                user.Emp.Code = EmpDTOmaintainUser.Emp.Code;
                user.Emp.AuthType = EmpDTOmaintainUser.Emp.AuthType;
                user.Emp.LoginId = EmpDTOmaintainUser.Emp.LoginId;
                user.Emp.EmpNo = EmpDTOmaintainUser.Emp.EmpNo;
                user.Emp.AliasName = EmpDTOmaintainUser.Emp.AliasName;
                user.Emp.Status = EmpDTOmaintainUser.Emp.Status;
                user.Emp.EncryptedPassword = "password";
                user.Emp.CreatedBy = _appuser.Emp.UID;
                user.Emp.CreatedTime = DateTime.Now;
                user.Emp.ModifiedTime = DateTime.Now;
                user.Emp.ModifiedBy = _appuser.Emp.UID;
                user.EmpInfo.UID = Guid.NewGuid().ToString();
                user.EmpInfo.CreatedBy = _appuser.Emp.UID;
                user.EmpInfo.CreatedTime = DateTime.Now;
                user.EmpInfo.ModifiedTime = DateTime.Now;
                user.EmpInfo.ModifiedBy = _appuser.Emp.UID;
                user.EmpInfo.EmpUID = EmpDTOmaintainUser.Emp.UID;
                user.EmpInfo.Phone = EmpDTOmaintainUser.EmpInfo.Phone;
                user.EmpInfo.Email = EmpDTOmaintainUser.EmpInfo.Email;
                user.EmpInfo.ADGroup = EmpDTOmaintainUser.EmpInfo.ADGroup;
                user.EmpInfo.ADUsername = EmpDTOmaintainUser.EmpInfo.ADUsername;
                user.EmpInfo.CanHandleStock = EmpDTOmaintainUser.EmpInfo.CanHandleStock;
                user.JobPosition.UID = Guid.NewGuid().ToString();
                user.JobPosition.OrgUID = _appuser.SelectedJobPosition.OrgUID;
                user.JobPosition.CreatedTime = DateTime.Now;
                user.JobPosition.CreatedBy = _appuser.Emp.UID;
                user.JobPosition.ModifiedBy = _appuser.Emp.UID;
                user.JobPosition.ModifiedTime = DateTime.Now;
                user.JobPosition.ServerAddTime = DateTime.Now;
                user.JobPosition.ServerModifiedTime = DateTime.Now;
                user.JobPosition.EmpUID = EmpDTOmaintainUser.Emp.UID;
                return await CreateUpdateUser(user, true);
            }
            else
            {

                user.Emp.Name = EmpDTOmaintainUser.Emp.Name;
                Uid = user.Emp.UID;
                user.Emp.Code = EmpDTOmaintainUser.Emp.Code;
                user.Emp.AuthType = EmpDTOmaintainUser.Emp.AuthType;
                user.Emp.LoginId = EmpDTOmaintainUser.Emp.LoginId;
                user.Emp.EmpNo = EmpDTOmaintainUser.Emp.EmpNo;
                user.Emp.Status = EmpDTOmaintainUser.Emp.Status;
                user.Emp.ModifiedTime = DateTime.Now;
                user.Emp.ModifiedBy = _appuser.Emp.UID;
                user.Emp.AliasName = EmpDTOmaintainUser.Emp.AliasName;
                user.EmpInfo.Phone = EmpDTOmaintainUser.EmpInfo.Phone;
                user.EmpInfo.Email = EmpDTOmaintainUser.EmpInfo.Email;
                user.EmpInfo.ADGroup = EmpDTOmaintainUser.EmpInfo.ADGroup;
                user.EmpInfo.ADUsername = EmpDTOmaintainUser.EmpInfo.ADUsername;
                user.EmpInfo.CanHandleStock = EmpDTOmaintainUser.EmpInfo.CanHandleStock;
                user.EmpInfo.ModifiedTime = DateTime.Now;
                user.EmpInfo.ModifiedBy = _appuser.Emp.UID;

                return await CreateUpdateUser(user, false);
            }
        }
        public async Task PopulateUsersDetailsforEdit(string Uid)
        {
            (EmpDTOmaintainUser, jobPosition) = await GetUsersDetailsforEdit(Uid);
        }
        public async Task<string> DeleteEmpOrgMapping(string UID)
        {
            return await DeleteEmpOrgMappingFromGrid(UID);
        }

        public async Task<string> CreateUpdateFileSysData(Winit.Modules.FileSys.Model.Interfaces.IFileSys fileSys)
        {
            fileSys.LinkedItemUID = Uid;
            return await CreateUpdateFileSysDetails(fileSys);
        }
        //public async Task CreateUpdateUserLocationDetails()
        //{
        //    await CreateUpdateUserLocationData();
        //}
        //public async Task GetLocationType()
        //{
        //    await GetLocationTypeFromAPI();
        //}

        //UserLocation Mapping
        public async Task GetUserLocationMappingType()
        {
            UserLocationTypes.Clear();
            UserLocationValues.Clear();
            var userlocationtypes = await GetUserLocationTypes();
            if (userlocationtypes != null && userlocationtypes.Any())
            {
                UserLocationTypes.AddRange(CommonFunctions.ConvertToSelectionItems(userlocationtypes, e => e.UID, e => e.Code, e => e.Name, e => $"[{e.Code}] {e.Name}"));
            }
        }
        public async Task GetUserLocationMappingValue(string? code)
        {
            UserLocationValues.Clear();
            var userlocationvalues = await GetUserLocationValues(code);
            if (userlocationvalues != null && userlocationvalues.Any())
            {
                UserLocationValues.AddRange(CommonFunctions.ConvertToSelectionItems(userlocationvalues, e => e.UID, e => e.Code, e => e.Name, e => $"[{e.Code}] {e.Name}"));
            }
        }
        public async Task PopulateUserLocationTypeAndValue(string loginID)
        {
            LocationTypeAndValueForLocationMapping = await GetUserLocationTypeAndValue(loginID);
            string locationType = LocationTypeAndValueForLocationMapping.LocationType;
            string locationValue = LocationTypeAndValueForLocationMapping.LocationValue;
            if (LocationTypeAndValueForLocationMapping != null)
            {
                await GetUserLocationMappingType();
                await GetUserLocationMappingValue(LocationTypeAndValueForLocationMapping.LocationType);

                foreach (var item in UserLocationTypes)
                {
                    //  item.IsSelected = item.Code.Equals(locationType, StringComparison.OrdinalIgnoreCase);
                    if (item.Code == locationType)
                    {
                        item.IsSelected = true;
                        item.IsSelected_InDropDownLevel = true;
                    }
                }
                UserLocationTypes = UserLocationTypes
                    .OrderByDescending(item => item.IsSelected)
                    .ToList();
                foreach (var item in UserLocationValues)
                {
                    //item.IsSelected = item.Code.Equals(locationValue, StringComparison.OrdinalIgnoreCase);
                    if (item.Code == locationValue)
                    {
                        item.IsSelected = true;
                        item.IsSelected_InDropDownLevel = true;
                    }
                }
                UserLocationValues = UserLocationValues
                    .OrderByDescending(item => item.IsSelected)
                    .ToList();
            }
            else
            {
                LocationTypeAndValueForLocationMapping = new LocationTypeAndValue();
                await GetUserLocationMappingType();
            }

        }

        public async Task<bool> CreateUpdateUserLocationTypeAndValueDetails(ILocationTypeAndValue locationTypeAndValueForLocationMapping)
        {
            return await CreateUpdateUserLocationTypeAndValue(locationTypeAndValueForLocationMapping);
        }

        public abstract Task<List<IListItem>> GetAuthTypeDD();
        public abstract Task<List<IListItem>> GetDepartmentDD();
        // public abstract Task<List<IOrg>> GetOrgDD();
        public abstract Task<List<IRole>> GetRoleDD(string? UID);
        public abstract Task<List<IEmp>> GetReportsToDD(string? uid);
        public abstract Task<List<IOrg>> GetEmpOrgMappingToDD(string empuid);
        public abstract Task<bool> CreateUpdateOrg_RoleMapping(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition jobPosition, bool IsCreate);
        public abstract Task<bool> CreateUpdateUser(IEmpDTO user, bool iscreate);
        public abstract Task<bool> CreateUpdateEmpOrgMapping(List<EmpOrgMapping> empOrgMapping);
        public abstract Task<string> DeleteEmpOrgMappingFromGrid(string uid);
        public abstract Task<(IEmpDTO empDTO, IJobPosition jobPosition)> GetUsersDetailsforEdit(string uid);
        public abstract Task<string> CreateUpdateFileSysDetails(Winit.Modules.FileSys.Model.Interfaces.IFileSys fileSys);
        public abstract Task<List<ILocationType>> GetUserLocationTypes();
        public abstract Task<List<ILocation>> GetUserLocationValues(string? code);
        public abstract Task<bool> CreateUpdateUserLocationTypeAndValue(ILocationTypeAndValue locationTypeAndValueForLocationMapping);
        public abstract Task<ILocationTypeAndValue> GetUserLocationTypeAndValue(string UID);

    }
}

using Microsoft.AspNetCore.Components;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Modules.Contact.Model.Classes;
using Winit.Shared.Models.Constants;
using Winit.Modules.Store.Model.Constants;
using Winit.Modules.Store.BL.Classes;
using Winit.Shared.CommonUtilities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace WinIt.Pages.Customer_Details
{
    public partial class EmployeeDetails
    {
        private int TotalEmp => (StoreAdditionalInfoCMI?.NoOfManager ?? 0)
                         + (StoreAdditionalInfoCMI?.NoOfSalesTeam ?? 0)
                         + (StoreAdditionalInfoCMI?.NoOfCommercial ?? 0)
                         + (StoreAdditionalInfoCMI?.NoOfService ?? 0)
                         + (StoreAdditionalInfoCMI?.NoOfOthers ?? 0);
        public string ValidationMessage;
        private bool IsSaveAttempted { get; set; } = false;
        [Parameter]public IStoreAdditionalInfoCMI StoreAdditionalInfoCMI { get; set; } = new StoreAdditionalInfoCMI();
        public IStoreAdditionalInfoCMI OriginalStoreAdditionalInfoCmi { get; set; } = new StoreAdditionalInfoCMI();

        [Parameter] public EventCallback<IStoreAdditionalInfoCMI> SaveOrUpdateeEmployeeDetails { get; set; }
        [Parameter] public string StoreAdditionalInfoCMIUid { get; set; }
        public string ButtonName { get; set; } = "Save";
        [Parameter]public bool IsEditOnBoardDetails { get; set; }
        public bool IsSuccess { get; set; } = false;
        [Parameter] public string TabName { get; set; }
        [Parameter] public EventCallback<string> InsertDataInChangeRequest { get; set; }
        [Parameter] public bool CustomerEditApprovalRequired { get; set; }
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            ButtonName = IsEditOnBoardDetails  ? "Update" : "Save";
            //if(StoreAdditionalInfoCMI!=null && StoreAdditionalInfoCMI.NoOfManager !=null)
            // {
            //     ButtonName = "Update";
            // }
            // else
            // {
            //     ButtonName = "Add";
            // }
            //StoreAdditionalInfoCMI = _serviceProvider.CreateInstance<IStoreAdditionalInfoCMI>();
            if (TabName==StoreConstants.Confirmed)
            {

                var storeAdditionalInfoCmi = StoreAdditionalInfoCMI as StoreAdditionalInfoCMI;
                OriginalStoreAdditionalInfoCmi = storeAdditionalInfoCmi.DeepCopy()!;
            }
            _loadingService.HideLoading();
            StateHasChanged();
        }
        private void CalculateTotalEmployees()
        {
            // Explicitly parse and calculate the total employees
            StoreAdditionalInfoCMI.TotalEmp = (StoreAdditionalInfoCMI.NoOfManager ?? 0)
                                             + (StoreAdditionalInfoCMI.NoOfSalesTeam ?? 0)
                                             + (StoreAdditionalInfoCMI.NoOfCommercial ?? 0)
                                             + (StoreAdditionalInfoCMI.NoOfService ?? 0)
                                             + (StoreAdditionalInfoCMI.NoOfOthers ?? 0);

            StateHasChanged(); // Ensure UI updates
        }

        protected async Task OnClean()
        {

            StoreAdditionalInfoCMI = new StoreAdditionalInfoCMI
            {
                NoOfCommercial = null,
                NoOfManager = null,
                NoOfSalesTeam = null,
                NoOfOthers = null,
                NoOfService = null,
                TotalEmp = null
            };


            StateHasChanged();
        }

        public async Task SaveOrUpdate()
        {
            //IsSaveAttempted = true;
            try
            {
                ValidationMessage = null;
                if (
                    string.IsNullOrWhiteSpace(StoreAdditionalInfoCMI?.NoOfManager.ToString()) ||
                    string.IsNullOrWhiteSpace(StoreAdditionalInfoCMI.NoOfSalesTeam.ToString()) ||
                    string.IsNullOrWhiteSpace(StoreAdditionalInfoCMI.NoOfCommercial.ToString()) ||
                    string.IsNullOrWhiteSpace(StoreAdditionalInfoCMI.NoOfCommercial.ToString()) ||
                    string.IsNullOrWhiteSpace(StoreAdditionalInfoCMI.NoOfOthers.ToString()))
                {
                    ValidationMessage = "The Following fields has invalid field(s)" + ": ";

                    if (string.IsNullOrWhiteSpace(StoreAdditionalInfoCMI?.NoOfManager.ToString()))
                    {
                        ValidationMessage += "Manager, ";
                    }

                    if (string.IsNullOrWhiteSpace(StoreAdditionalInfoCMI.NoOfSalesTeam.ToString()))
                    {
                        ValidationMessage += "sales, ";
                    }
                    if (string.IsNullOrWhiteSpace(StoreAdditionalInfoCMI.NoOfCommercial.ToString()))
                    {
                        ValidationMessage += "Commercial, ";
                    }
                    if (string.IsNullOrWhiteSpace(StoreAdditionalInfoCMI.NoOfService.ToString()))
                    {
                        ValidationMessage += "Service, ";
                    }
                    if (string.IsNullOrWhiteSpace(StoreAdditionalInfoCMI.NoOfOthers.ToString()))
                    {
                        ValidationMessage += "Others, ";
                    }
                    
                    ValidationMessage = ValidationMessage.TrimEnd(' ', ',');
                }
                else
                {
                    IStoreAdditionalInfoCMI storeAdditionalInfoCMI = new StoreAdditionalInfoCMI();
                    CalculateTotalEmployees();
                    StoreAdditionalInfoCMI.SectionName = OnboardingScreenConstant.EmployeeDetails;
                    //ButtonName = "Update";
                    ShowLoader();
                    if (TabName==StoreConstants.Confirmed && CustomerEditApprovalRequired)
                    {
                       
                        OriginalStoreAdditionalInfoCmi.SectionName = OnboardingScreenConstant.EmployeeDetails;
                        await RequestChange();
                    }
                    else if(TabName==StoreConstants.Confirmed)
                    {
                        OriginalStoreAdditionalInfoCmi.SectionName = OnboardingScreenConstant.EmployeeDetails;
                        await RequestChange();
                        await SaveOrUpdateeEmployeeDetails.InvokeAsync(StoreAdditionalInfoCMI);

                    }
                    else
                    {
                        await SaveOrUpdateeEmployeeDetails.InvokeAsync(StoreAdditionalInfoCMI);

                    }
                    HideLoader();
                }
                IsSuccess = true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        #region Change RequestLogic
        
        public async Task RequestChange()
        {
            List<IChangeRecordDTO> ChangeRecordDTOs = new List<IChangeRecordDTO>
            {
                new ChangeRecordDTO
                {
                     Action= OnboardingScreenConstant.Update,
                    ScreenModelName = OnboardingScreenConstant.EmployeeDetails,
                    UID = StoreAdditionalInfoCMIUid,
                    ChangeRecords = CommonFunctions.GetChangedData(CommonFunctions.CompareObjects(OriginalStoreAdditionalInfoCmi!, StoreAdditionalInfoCMI)!)
                }
            }
            .Where(c => c.ChangeRecords.Count > 0)
            .ToList();

            if (ChangeRecordDTOs.Count>0)
            {
                var ChangeRecordDTOInJson = CommonFunctions.ConvertToJson(ChangeRecordDTOs);
                await InsertDataInChangeRequest.InvokeAsync(ChangeRecordDTOInJson);
            }
            ChangeRecordDTOs.Clear();
        }
        public IStoreAdditionalInfoCMI GetModifiedObject(IStoreAdditionalInfoCMI storeAdditionalinfoCMI)
        {
            var modifiedObject = new StoreAdditionalInfoCMI
            {
                NoOfManager = storeAdditionalinfoCMI.NoOfManager,
                NoOfSalesTeam = storeAdditionalinfoCMI.NoOfSalesTeam,
                NoOfCommercial = storeAdditionalinfoCMI.NoOfCommercial,
                NoOfService = storeAdditionalinfoCMI.NoOfService,
                TotalEmp = storeAdditionalinfoCMI.TotalEmp,
                NoOfOthers = storeAdditionalinfoCMI.NoOfOthers
            };

            return modifiedObject;
        }

        #endregion
    }
    
}

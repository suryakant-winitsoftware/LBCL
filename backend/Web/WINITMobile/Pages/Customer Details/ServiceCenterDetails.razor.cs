using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.UIComponents.Common.FileUploader;
using Winit.Modules.Store.BL.Classes;
using Winit.Shared.Models.Constants;
using Winit.Shared.CommonUtilities.Common;
using Winit.Modules.Store.Model.Constants;
using Microsoft.AspNetCore.Components.Forms;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using WINITMobile.Pages.Base;

namespace WINITMobile.Pages.Customer_Details
{
    public partial class ServiceCenterDetails : BaseComponentBase
    {
       
        [Parameter] public IOnBoardCustomerDTO? _onBoardCustomerDTO { get; set; }
        [Parameter] public string LinkedItemUID { get; set; }

        [Parameter]
        public IStoreAdditionalInfoCMI StoreAdditionalInfoCmi { get; set; } = new StoreAdditionalInfoCMI();
        public IStoreAdditionalInfoCMI OriginalStoreAdditionalInfoCmi { get; set; } = new StoreAdditionalInfoCMI();
        [Parameter] public EventCallback<string> InsertDataInChangeRequest { get; set; }

        [Parameter] public EventCallback<IStoreAdditionalInfoCMI> SaveOrUpdateServiceCenterDetail{ get; set; }
        [Parameter] public string StoreAdditionalInfoCMIUid { get; set; }

        private string? FilePath { get; set; }
        [Parameter] public string TabName { get; set; }
        private Winit.UIComponents.Common.FileUploader.FileUploader? fileUploader { get; set; }

        public bool IsSuccess { get; set; } = false;

        private bool isYesChecked = true;
        private bool isNoChecked = false;
        [Parameter] public List<TechniciansInfo> TechnicianInfo { get; set; } = new List<TechniciansInfo> { new TechniciansInfo { Sn = 1 } };

        [Parameter] public List<SupervisorInfo> SupervisorInfos { get; set; } = new List<SupervisorInfo> { new SupervisorInfo { Sn = 1 } };
        [Parameter] public List<IFileSys> filesysList { get; set; } = new List<IFileSys>();
        [Parameter] public bool IsEditOnBoardDetails { get; set; } = false;
        public string ButtonName { get; set; } = "Save";

        protected override async Task OnInitializedAsync()
        {
            FilePath = FileSysTemplateControles.GetOnBoardImageCheckFolderPath(LinkedItemUID);
            if(IsEditOnBoardDetails)
            {
                await MapFiles();
                ButtonName = "Update";
            }
            if(TabName==StoreConstants.Confirmed)
            {
               
                var storeAdditionalInfoCmi = StoreAdditionalInfoCmi as StoreAdditionalInfoCMI;
                OriginalStoreAdditionalInfoCmi = storeAdditionalInfoCmi.DeepCopy()!;
                OriginalStoreAdditionalInfoCmi!.ScTechnicianData=CommonFunctions.ConvertToJson(TechnicianInfo);
                OriginalStoreAdditionalInfoCmi.ScSupervisorData=CommonFunctions.ConvertToJson(SupervisorInfos);

            }
        }
        public async Task MapFiles()
        {
            try
            {
                Memo_AssofileSysList = filesysList.Where(f => f.FileSysType == FileSysTypeConstants.ScRentalAgreement).ToList();
                Partnership_DealerfileSysList = filesysList.Where(f => f.FileSysType == FileSysTypeConstants.ScLicence).ToList();
                GSTfileSysList = filesysList.Where(f => f.FileSysType == FileSysTypeConstants.ScFrontOffice).ToList();
                PANfileSysList = filesysList.Where(f => f.FileSysType == FileSysTypeConstants.ScStore).ToList();
                Shop_EstfileSysList = filesysList.Where(f => f.FileSysType == FileSysTypeConstants.ScTrainingArea).ToList();
                ESICfileSysList = filesysList.Where(f => f.FileSysType == FileSysTypeConstants.ScWorkshop).ToList();
                PFfileSysList = filesysList.Where(f => f.FileSysType == FileSysTypeConstants.ScGroupPhoto).ToList();
                StateHasChanged();
                
            }
            catch(Exception ex)
            {

            }
        }

        private void AddTechnicianRow()
        {
            var currentSN = TechnicianInfo.Count;
            TechnicianInfo.Add(new TechniciansInfo { Sn = currentSN + 1 });
        }

        private void RemoveTechnicianRow(TechniciansInfo techinfo)
        {
            TechnicianInfo.Remove(techinfo);
            for (int i = 0; i < TechnicianInfo.Count; i++)
            {
                TechnicianInfo[i].Sn = i + 1;
            }
        }
        private void AddSupervisorRow()
        {
            var currentSN = SupervisorInfos.Count;
            SupervisorInfos.Add(new SupervisorInfo { Sn = currentSN + 1 });
        }

        private void RemoveSupervisorRow(SupervisorInfo superinfo)
        {
            SupervisorInfos.Remove(superinfo);
            for (int i = 0; i < TechnicianInfo.Count; i++)
            {
                SupervisorInfos[i].Sn = i + 1;
            }
        }
        
        private void OnLocationChanged(bool value)
        {
            StoreAdditionalInfoCmi.ScIsServiceCenterDifferenceFromPrinciplePlace = value;
        }
        public async Task CreateUpdateServiceCenterDetails()
        {
            ValidateAllFields();
            if (string.IsNullOrWhiteSpace(ValidationMessage))
            {
                try
                {
                    var TechnicianInfoJson = JsonConvert.SerializeObject(TechnicianInfo);
                    var SupervisorInfosJson = JsonConvert.SerializeObject(SupervisorInfos);
                    StoreAdditionalInfoCmi.ScTechnicianData = TechnicianInfoJson;
                    StoreAdditionalInfoCmi.ScSupervisorData = SupervisorInfosJson;
                    StoreAdditionalInfoCmi.SectionName = OnboardingScreenConstant.ServiceCenterDetail;
                    if(TabName==StoreConstants.Confirmed)
                    {
                        ShowLoader();
                        OriginalStoreAdditionalInfoCmi.SectionName = OnboardingScreenConstant.ServiceCenterDetail;
                        await RequestChange();
                        HideLoader();
                    }
                    else
                    {
                        await SaveFileSys();
                        await SaveOrUpdateServiceCenterDetail.InvokeAsync(StoreAdditionalInfoCmi);
                    }
                  
                    IsSuccess = true;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        
        private void OnCheckboxChanged(ChangeEventArgs e, string type)
        {
                StoreAdditionalInfoCmi.ScAddressType = type;
        }
        private void AddToDictionaryIfNotNull(Dictionary<Winit.UIComponents.Common.FileUploader.FileUploader?, List<IFileSys>?> fileUploaders,
                                               Winit.UIComponents.Common.FileUploader.FileUploader? uploader, List<IFileSys>? fileSysList)
        {
            if (uploader != null && fileSysList != null && fileSysList.Any())
            {
                fileUploaders.Add(uploader, fileSysList);
            }
        }

        public async Task<bool> ValidateFiles(Dictionary<Winit.UIComponents.Common.FileUploader.FileUploader?, List<IFileSys>> fileUploaders)
        {
            try
            {
                var requiredUploaders = new List<string>();

                

                // Check if any required file uploader is empty
                var emptyFileUploaderNames = fileUploaders
                                                .Where(u => requiredUploaders.Contains(u.Key?.FileSysType) && (u.Value == null || !u.Value.Any()))
                                                .Select(u => u.Key?.FileSysType ?? "Unknown")
                                                .ToList();

                if (emptyFileUploaderNames.Any())
                {
                    // Format the message with newline characters
                    var uploaderNames = string.Join("\n", emptyFileUploaderNames);
                    var formattedMessage = $"Please upload files for the following:\n{uploaderNames}";
                    await _alertService.ShowErrorAlert("Error", formattedMessage);
                    return false;
                }

                return true; // Return true if all required files are uploaded
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<List<IFileSys>>? DocumentAppendixfileSysList { get; set; } = new List<List<IFileSys>>();
        public async Task SaveFileSys()
        {
            try
            {
                //        // Dictionary to pair file uploaders with their corresponding file lists
                //       
                var fileUploaders = new Dictionary<Winit.UIComponents.Common.FileUploader.FileUploader?, List<IFileSys>?>();

                // Add file uploaders to the dictionary only if their corresponding lists are not null or empty
                AddToDictionaryIfNotNull(fileUploaders, Memo_AssofileUploader, Memo_AssofileSysList);
                AddToDictionaryIfNotNull(fileUploaders, Partnership_DealerPfileUploader, Partnership_DealerfileSysList);
                AddToDictionaryIfNotNull(fileUploaders, GSTfileUploader, GSTfileSysList);
                AddToDictionaryIfNotNull(fileUploaders, PANfileUploader, PANfileSysList);
                AddToDictionaryIfNotNull(fileUploaders, Shop_EstfileUploader, Shop_EstfileSysList);
                AddToDictionaryIfNotNull(fileUploaders, ESICfileUploader, ESICfileSysList);
                AddToDictionaryIfNotNull(fileUploaders, PFfileUploader, PFfileSysList);
                if (await ValidateFiles(fileUploaders))
                {
                    DocumentAppendixfileSysList.Clear();
                    foreach (var uploader in fileUploaders)
                    {
                        //if (uploader.Value != null)
                        var apiResponse = await uploader.Key.MoveFiles();
                        if (apiResponse.IsSuccess)
                        {
                            DocumentAppendixfileSysList.Add(uploader.Value);
                        }
                        else
                        {
                            await _alertService.ShowErrorAlert("Error", "Moving files failed");
                        }
                    }
                    ValidateAllFields();
                    if (string.IsNullOrWhiteSpace(ValidationMessage))
                    {
                        await SaveOrUpdateDocument.InvokeAsync(DocumentAppendixfileSysList);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public string ValidationMessage;
        private bool ValidateAllFields()
        {
            ValidationMessage = null;

            if (
                string.IsNullOrWhiteSpace(StoreAdditionalInfoCmi.ScAddress) ||
                string.IsNullOrWhiteSpace(StoreAdditionalInfoCmi.ScCurrentBrandHandled))
            {
                ValidationMessage = "The following fields have invalid field(s)" + ": ";

                if (string.IsNullOrWhiteSpace(StoreAdditionalInfoCmi.ScAddress))
                {
                    ValidationMessage += "Address, ";
                }

                if (string.IsNullOrWhiteSpace(StoreAdditionalInfoCmi.ScCurrentBrandHandled))
                {
                    ValidationMessage += "Current Brand handled, ";
                }

                ValidationMessage = ValidationMessage.TrimEnd(' ', ',');
                return false;
            }
            else
            {
                return true;
            }
        }
        [Parameter] public EventCallback<List<List<IFileSys>>> SaveOrUpdateDocument { get; set; }
        private Winit.UIComponents.Common.FileUploader.FileUploader? Memo_AssofileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? Partnership_DealerPfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? GSTfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? PANfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? Shop_EstfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? ESICfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? PFfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        public List<IFileSys>? Memo_AssofileSysList { get; set; } = new List<IFileSys>();
        public List<IFileSys>? Partnership_DealerfileSysList { get; set; } = new List<IFileSys>();
        public List<IFileSys>? GSTfileSysList { get; set; } = new List<IFileSys>();
        public List<IFileSys>? PANfileSysList { get; set; } = new List<IFileSys>();
        public List<IFileSys>? Shop_EstfileSysList { get; set; } = new List<IFileSys>();
        public List<IFileSys>? ESICfileSysList { get; set; } = new List<IFileSys>();
        public List<IFileSys>? PFfileSysList { get; set; } = new List<IFileSys>();
        private void GetMoASavedImagePath(List<IFileSys> ImagePath)
        {
            Memo_AssofileSysList = ImagePath;
        }

        private void GetPDPSavedImagePath(List<IFileSys> ImagePath)
        {
            Partnership_DealerfileSysList = ImagePath;
        }

        private void GetGSTSavedImagePath(List<IFileSys> ImagePath)
        {
            GSTfileSysList = ImagePath;
        }

        private void GetPANSavedImagePath(List<IFileSys> ImagePath)
        {
            PANfileSysList = ImagePath;
        }

        private void GetShopEstSavedImagePath(List<IFileSys> ImagePath)
        {
            Shop_EstfileSysList = ImagePath;
        }

        private void GetESICSavedImagePath(List<IFileSys> ImagePath)
        {
            ESICfileSysList = ImagePath;
        }
        public void OnClear()
        {
            try
            {
                Memo_AssofileSysList?.Clear();
                Partnership_DealerfileSysList?.Clear();
                GSTfileSysList?.Clear();
                PANfileSysList?.Clear();
                Shop_EstfileSysList?.Clear();
                ESICfileSysList?.Clear();
                PFfileSysList?.Clear();
                TechnicianInfo = new List<TechniciansInfo> { new TechniciansInfo { Sn = 1 } };
                SupervisorInfos = new List<SupervisorInfo> { new SupervisorInfo { Sn = 1 } };
                StoreAdditionalInfoCmi = new StoreAdditionalInfoCMI();
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        private void GetPFSavedImagePath(List<IFileSys> ImagePath)
        {
            PFfileSysList = ImagePath;
        }
        private void AfterDeleteImage()
        {

        }
        private bool IsChecked(string type)
        {
            return StoreAdditionalInfoCmi.ScAddressType == type;
        }
        #region Change RequestLogic
       
        
        public async Task RequestChange()
        {
            List<IChangeRecordDTO> ChangeRecordDTOs = new List<IChangeRecordDTO>
            {
                new ChangeRecordDTO
                {
                    Action= OnboardingScreenConstant.Update,
                    ScreenModelName = OnboardingScreenConstant.ServiceCenterDetail,
                    UID = StoreAdditionalInfoCMIUid,
                    ChangeRecords = CommonFunctions.GetChangedData(CommonFunctions.CompareObjects(OriginalStoreAdditionalInfoCmi, StoreAdditionalInfoCmi)!)
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
            public object GetModifiedObject(IStoreAdditionalInfoCMI storeAdditionalinfoCMI)
        {
            var modifiedObject = new
            {
                storeAdditionalinfoCMI.ScAddress,
                storeAdditionalinfoCMI.ScAddressType,
                storeAdditionalinfoCMI.ScIsServiceCenterDifferenceFromPrinciplePlace,
                storeAdditionalinfoCMI.ScArea,
                storeAdditionalinfoCMI.ScCurrentBrandHandled,
                storeAdditionalinfoCMI.ScExpInYear,
                storeAdditionalinfoCMI.ScNoOfTechnician,
                storeAdditionalinfoCMI.ScTechnicianData,
                storeAdditionalinfoCMI.ScNoOfSupervisor,
                storeAdditionalinfoCMI.ScSupervisorData,
                storeAdditionalinfoCMI.ScLicenseNo

            };

            return modifiedObject;
        }
        #endregion
    }
}

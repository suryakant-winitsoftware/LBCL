using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using Winit.Modules.Contact.Model.Classes;
using Winit.Modules.Store.BL.Classes;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Constants;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;

namespace WinIt.Pages.Customer_Details
{
    public partial class BusinessDetails
    {
        [Parameter] public IStoreAdditionalInfoCMI storeAdditionalInfoCMI { get; set; } = new StoreAdditionalInfoCMI();
        [Parameter] public IStoreAdditionalInfoCMI OriginalStoreAdditionalInfoCMI { get; set; } = new StoreAdditionalInfoCMI();
        [Parameter] public List<StoreBrandDealingIn> brandInfos { get; set; } = new List<StoreBrandDealingIn> { new StoreBrandDealingIn { Sn = 1 } };

        [Parameter] public EventCallback<IStoreAdditionalInfoCMI> SaveOrUpdateBusinessDetails { get; set; }
        [Parameter] public EventCallback<IStoreShowroom> OnAddBusinessDetails { get; set; }
        [Parameter] public EventCallback<IStoreShowroom> OnEditBusinessDetails { get; set; }
        [Parameter] public EventCallback<string> OnDelete { get; set; }
        [Parameter] public Func<Task<List<IStoreShowroom>>> OnShowAllShowroomClick { get; set; }
        [Parameter] public bool IsEditOnBoardDetails { get; set; }
        public string ButtonName { get; set; } = "Save";
        public string ValidationMessage;
        public string BrandInfoJson { get; set; }

        private bool IsEditPage = false;
        public bool IsSuccess { get; set; } = false;
        [Parameter] public EventCallback<string> InsertDataInChangeRequest { get; set; }
        [Parameter] public bool CustomerEditApprovalRequired { get; set; }
        [Parameter] public string StoreAdditionalInfoCMIUid { get; set; }
        [Parameter] public string TabName { get; set; }
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            ButtonName = IsEditOnBoardDetails ? "Update" : "Save";

            //if (storeAdditionalInfoCMI != null && storeAdditionalInfoCMI.ProductDealingIn != null)
            //{
            //    ButtonName = "Update";
            //}
            //else
            //{
            //    ButtonName = "Save";
            //}
            if(TabName==StoreConstants.Confirmed)
            {
                var concreteAddress = storeAdditionalInfoCMI as StoreAdditionalInfoCMI;
                OriginalStoreAdditionalInfoCMI=concreteAddress.DeepCopy()!;
                OriginalStoreAdditionalInfoCMI.BrandDealingInDetails=CommonFunctions.ConvertToJson(brandInfos);

            }
            _loadingService.HideLoading();

            StateHasChanged();
        }
        private void AddRow()
        {
            var currentSN = brandInfos.Count;
            brandInfos.Add(new StoreBrandDealingIn { Sn = currentSN + 1 });
        }

        private void RemoveRow(StoreBrandDealingIn brandInfo)
        {
            brandInfos.Remove(brandInfo);
            for (int i = 0; i < brandInfos.Count; i++)
            {
                brandInfos[i].Sn = i + 1;
            }
        }
        protected async Task OnClean()
        {

            storeAdditionalInfoCMI = new StoreAdditionalInfoCMI
            {
                ProductDealingIn = string.Empty,
                AreaOfOperation = string.Empty,
                DistProducts = string.Empty,
                DistAreaOfOperation = string.Empty,
            };
            ButtonName = "Save";
            IsEditPage = false;

            StateHasChanged();
        }
        protected void GetBrandInfoJson()
        {
            //TotalStoreCount = ShowroomDetails.Sum(srm => srm.NoOfStores);
            //ShowroomDetails.ForEach(srm => srm.NoOfStores += TotalStoreCount);
            BrandInfoJson = JsonConvert.SerializeObject(brandInfos);
        }
        private bool ValidateAllFields()
        {
            ValidationMessage = null;

            if (
                string.IsNullOrWhiteSpace(storeAdditionalInfoCMI.ProductDealingIn) ||
                string.IsNullOrWhiteSpace(storeAdditionalInfoCMI.AreaOfOperation) ||
                string.IsNullOrWhiteSpace(storeAdditionalInfoCMI.DistProducts) ||
                string.IsNullOrWhiteSpace(storeAdditionalInfoCMI.DistAreaOfOperation))
              
            {
                ValidationMessage = "The following fields have invalid field(s)" + ": ";

                if (string.IsNullOrWhiteSpace(storeAdditionalInfoCMI.ProductDealingIn))
                {
                    ValidationMessage += "ProductDealingIn, ";
                }

                if (string.IsNullOrWhiteSpace(storeAdditionalInfoCMI.AreaOfOperation))
                {
                    ValidationMessage += "AreaOfOperation, ";
                }

                if (string.IsNullOrWhiteSpace(storeAdditionalInfoCMI.DistProducts))
                {
                    ValidationMessage += "DistProducts, ";
                }
                if (string.IsNullOrWhiteSpace(storeAdditionalInfoCMI.DistAreaOfOperation))
                {
                    ValidationMessage += "DistAreaOfOperation, ";
                }
              

                ValidationMessage = ValidationMessage.TrimEnd(' ', ',');
                return false;
            }
            else
            {
                return true;
            }
        }
        public async Task SaveBusinessDetails()
        {
            ValidateAllFields();
            if (string.IsNullOrWhiteSpace(ValidationMessage))
            {
                try
                {
                    GetBrandInfoJson();
                    if (!IsEditPage)
                    {
                        //await OnAddShowroom.InvokeAsync();
                        storeAdditionalInfoCMI.BrandDealingInDetails = BrandInfoJson;
                        storeAdditionalInfoCMI.SectionName = OnboardingScreenConstant.BusinessDetails;
                        if (TabName==StoreConstants.Confirmed && !CustomerEditApprovalRequired)
                        {
                            await RequestChange();
                            await SaveOrUpdateBusinessDetails.InvokeAsync(storeAdditionalInfoCMI);
                        }
                        else if (TabName==StoreConstants.Confirmed && CustomerEditApprovalRequired)
                        {
                            await RequestChange();
                        }
                        else
                        {
                            await SaveOrUpdateBusinessDetails.InvokeAsync(storeAdditionalInfoCMI);
                        }
                        await GenerateGridColumns();
                    }
                    else
                    {
                        //await OnEditBusinessDetails.InvokeAsync();
                        await SaveOrUpdateBusinessDetails.InvokeAsync(storeAdditionalInfoCMI);
                        await GenerateGridColumns();
                    }
                    IsSuccess = true;

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            // }

        }

        #region Change RequestLogic
        
        public async Task RequestChange()
        {
            List<IChangeRecordDTO> ChangeRecordDTOs = new List<IChangeRecordDTO>
            {
                new ChangeRecordDTO
                {
                     Action= OnboardingScreenConstant.Update,
                    ScreenModelName = OnboardingScreenConstant.BusinessDetails,
                    UID = StoreAdditionalInfoCMIUid,
                    ChangeRecords = CommonFunctions.GetChangedData(CommonFunctions.CompareObjects(OriginalStoreAdditionalInfoCMI!, storeAdditionalInfoCMI)!)
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
                storeAdditionalinfoCMI.BrandDealingInDetails,
                storeAdditionalinfoCMI.DistMonthlySales,
                storeAdditionalinfoCMI.DistBrands,
                storeAdditionalinfoCMI.DistAreaOfOperation,
                storeAdditionalinfoCMI.DistProducts,
                storeAdditionalinfoCMI.AreaOfOperation,
                storeAdditionalinfoCMI.ProductDealingIn,

            };

            return modifiedObject;
        }
        #endregion

        private async Task GenerateGridColumns()
        {
            //DataGridColumns = new List<DataGridColumn>
            //{

            //    new DataGridColumn {Header = "Address1"},
            //    new DataGridColumn {Header = "Address2"},
            //    new DataGridColumn {Header = "Address3"},
            //    new DataGridColumn {Header = "Landmark"},
            //    new DataGridColumn {Header = "Pin Code"},
            //    new DataGridColumn {Header = "Mobile Number1"},
            //    new DataGridColumn {Header = "Mobile Number2"},
            //    new DataGridColumn {Header = "Email"},
            //    new DataGridColumn
            //    {
            //    Header = "Actions",
            //    IsButtonColumn = true,
            //    ButtonActions = new List<ButtonAction>
            //    {
            //        new ButtonAction
            //        {
            //            ButtonType = ButtonTypes.Image,
            //            URL = "https://qa-fonterra.winitsoftware.com/assets/Images/edit.png",

            //        },
            //        new ButtonAction
            //        {
            //            ButtonType = ButtonTypes.Image,
            //            URL = "https://qa-fonterra.winitsoftware.com/assets/Images/delete.png",

            //        }
            //    }
            //}
            // };
        }
    }
}

using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Winit.Modules.Store.BL.Classes;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Constants;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.UIComponents.Common.Services;
using WINITMobile.Pages.Base;

namespace WINITMobile.Pages.Customer_Details
{
    public partial class DistributorBusinessDetails : BaseComponentBase
    {
        public string ValidationMessage;
        public string ButtonName { get; set; } = "Save";

        private bool IsSaveAttempted { get; set; } = false;
        [Parameter] public IStoreAdditionalInfoCMIRetailingCityMonthlySales RetailingCityMonthlySales { get; set; } = new StoreAdditionalInfoCMIRetailingCityMonthlySales();
        [Parameter] public IStoreAdditionalInfoCMIRACSalesByYear RACSalesByYear { get; set; } = new StoreAdditionalInfoCMIRACSalesByYear();
        [Parameter] public IStoreAdditionalInfoCMI storeAdditionalInfoCMI { get; set; } = new StoreAdditionalInfoCMI();
        public IStoreAdditionalInfoCMI OriginalStoreAdditionalInfoCMI { get; set; } = new StoreAdditionalInfoCMI();
        [Parameter] public string StoreAdditionalInfoCMIUid { get; set; }
        public int RACSalesByYearSn = 0;
        public int RetailingCityMonthlySalesSn = 0;
        public bool IsSuccess { get; set; } = false;
        [Parameter] public bool IsEditOnBoardDetails { get; set; }

        [Parameter] public EventCallback<IStoreAdditionalInfoCMI> SaveOrUpdateDistBusiness { get; set; }
        //private List<IStoreAdditionalInfoCMIRetailingCityMonthlySales> RetailingCityMonthlySalesDetails = new List<IStoreAdditionalInfoCMIRetailingCityMonthlySales>();
        // private List<IStoreAdditionalInfoCMIRACSalesByYear> RACSalesByYearDetails = new List<IStoreAdditionalInfoCMIRACSalesByYear>();
       
        List<StoreAdditionalInfoCMIRetailingCityMonthlySales1> RetailingCityMonthlySalesDetails = new List<StoreAdditionalInfoCMIRetailingCityMonthlySales1>();
        
        List<StoreAdditionalInfoCMIRACSalesByYear1> RACSalesByYearDetails = new List<StoreAdditionalInfoCMIRACSalesByYear1>();
        [Parameter] public EventCallback<string> InsertDataInChangeRequest { get; set; }
        [Parameter] public bool CustomerEditApprovalRequired { get; set; }
        [Parameter] public string TabName { get; set; }
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            //RetailingCityMonthlySales = _serviceProvider.CreateInstance<IStoreAdditionalInfoCMIRetailingCityMonthlySales>();
            //RACSalesByYear = _serviceProvider.CreateInstance<IStoreAdditionalInfoCMIRACSalesByYear>();
            //storeAdditionalInfoCMI = _serviceProvider.CreateInstance<IStoreAdditionalInfoCMI>();
            ButtonName = IsEditOnBoardDetails ? "Update" : "Save";
            //if (RetailingCityMonthlySales != null && RetailingCityMonthlySales.CitySD1 != null)
            //{
            //    ButtonName = "Update";
            //}
            //else
            //{
            //    ButtonName = "Save";
            //}
            if (TabName==StoreConstants.Confirmed)
            {
                AddDistBusinessDetails();
                GetShowRoomJson();
                var concreteAddress = storeAdditionalInfoCMI as StoreAdditionalInfoCMI;
                OriginalStoreAdditionalInfoCMI=concreteAddress.DeepCopy()!;
                OriginalStoreAdditionalInfoCMI.DistRetailingCityMonthlySales=RetailingCityMonthlySalesJson;
                OriginalStoreAdditionalInfoCMI.DistRacSalesByYear=RACSalesByYearJson;

            }
            _loadingService.HideLoading();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }
        protected async Task OnClean()
        {

            RetailingCityMonthlySales = new StoreAdditionalInfoCMIRetailingCityMonthlySales
            {
                Sn = null,
                CityName = string.Empty,
                AvgMonthlySales = string.Empty,


            };
            RACSalesByYear = new StoreAdditionalInfoCMIRACSalesByYear
            {
                Sn = null,
                Qty = null,
                Year = null,
            };


            StateHasChanged();
        }
        public bool CheckYear1(string year)
        {
            if (string.IsNullOrWhiteSpace(year))
            {
                return false;
            }

            // Example: Account number validation for numbers between 8 and 10 digits
            var yearPattern = @"^\d{8,10}$";
            return Regex.IsMatch(year, yearPattern);
        }
        private string Year1validationMessage = string.Empty;
        private string Year2validationMessage = string.Empty;
        private string Year3validationMessage = string.Empty;
        public void ValidateYear1(ChangeEventArgs e, int name)
        {
            string input = e.Value?.ToString();

            if (string.IsNullOrEmpty(input) || input.Length != 4)
            {
                if (name == 1)
                    Year1validationMessage = "Year should be 4 digits range";
                if (name == 2)
                    Year2validationMessage = "Year should be 4 digits range";
                if (name == 3)
                    Year3validationMessage = "Year should be 4 digits range";
            }
            else
            {
                if (name == 1)
                    Year1validationMessage = string.Empty;
                if (name == 2)
                    Year2validationMessage = string.Empty;
                if (name == 3)
                    Year3validationMessage = string.Empty;

            }
        }
        public string RetailingCityMonthlySalesJson { get; set; }
        public string RACSalesByYearJson { get; set; }
        //protected void GetShowRoomJson()
        //{

        //    RetailingCityMonthlySalesJson = JsonConvert.SerializeObject(RetailingCityMonthlySalesDetails);
        //    RACSalesByYearJson = JsonConvert.SerializeObject(RACSalesByYearDetails);
        //}
        protected void GetShowRoomJson()
        {
            RetailingCityMonthlySalesJson = JsonConvert.SerializeObject(RetailingCityMonthlySalesDetails);
            Console.WriteLine(RetailingCityMonthlySalesJson);
            RACSalesByYearJson = JsonConvert.SerializeObject(RACSalesByYearDetails);
        }
        public int Sn = 0;
        private void AddDistBusinessDetails()
        {
            // Ensure the RetailingCityMonthlySales is not null
            if (ValidateAllFields())
            {
                if (RetailingCityMonthlySales != null)
                {
                    RetailingCityMonthlySalesDetails=new List<StoreAdditionalInfoCMIRetailingCityMonthlySales1>();

                    RetailingCityMonthlySalesDetails.Add(new StoreAdditionalInfoCMIRetailingCityMonthlySales1
                    {
                        Sn = Sn + 1,
                        CityName = RetailingCityMonthlySales.CitySD1,
                        AvgMonthlySales = RetailingCityMonthlySales.CityAMS1
                    });
                    Sn = Sn + 1;
                    RetailingCityMonthlySalesDetails.Add(new StoreAdditionalInfoCMIRetailingCityMonthlySales1
                    {
                        Sn = Sn + 1,
                        CityName = RetailingCityMonthlySales.CitySD2,
                        AvgMonthlySales = RetailingCityMonthlySales.CityAMS2
                    });
                    Sn = Sn + 1;
                    RetailingCityMonthlySalesDetails.Add(new StoreAdditionalInfoCMIRetailingCityMonthlySales1
                    {
                        Sn = Sn + 1,
                        CityName = RetailingCityMonthlySales.CitySD3,
                        AvgMonthlySales = RetailingCityMonthlySales.CityAMS3
                    });
                    Sn = Sn + 1;
                    RetailingCityMonthlySalesDetails.Add(new StoreAdditionalInfoCMIRetailingCityMonthlySales1
                    {
                        Sn = Sn + 1,
                        CityName = RetailingCityMonthlySales.CitySD4,
                        AvgMonthlySales = RetailingCityMonthlySales.CityAMS4
                    });
                    Sn = Sn + 1;
                    RetailingCityMonthlySalesDetails.Add(new StoreAdditionalInfoCMIRetailingCityMonthlySales1
                    {
                        Sn = Sn + 1,
                        CityName = RetailingCityMonthlySales.CitySD5,
                        AvgMonthlySales = RetailingCityMonthlySales.CityAMS5
                    });

                    // Optionally, clear the RetailingCityMonthlySales
                    RetailingCityMonthlySales = new StoreAdditionalInfoCMIRetailingCityMonthlySales();
                }
                if (RACSalesByYear != null)
                {
                    RACSalesByYearDetails= new List<StoreAdditionalInfoCMIRACSalesByYear1>();
                    RACSalesByYearDetails.Add(new StoreAdditionalInfoCMIRACSalesByYear1
                    {
                        Sn = 1,
                        Year = RACSalesByYear.Year1,
                        Qty = RACSalesByYear.Qty1
                    });

                    RACSalesByYearDetails.Add(new StoreAdditionalInfoCMIRACSalesByYear1
                    {
                        Sn = 2,
                        Year = RACSalesByYear.Year2,
                        Qty = RACSalesByYear.Qty2
                    });

                    RACSalesByYearDetails.Add(new StoreAdditionalInfoCMIRACSalesByYear1
                    {
                        Sn = 3,
                        Year = RACSalesByYear.Year3,
                        Qty = RACSalesByYear.Qty3
                    });

                    RACSalesByYear = new StoreAdditionalInfoCMIRACSalesByYear();
                }
            }
        }

        public async Task SaveUpdateDistBusiness()
        {
            IsSaveAttempted = true;
            //ValidateAllFields();
            //if (string.IsNullOrWhiteSpace(ValidationMessage))
            //{
            try
            {
                AddDistBusinessDetails();
                GetShowRoomJson();
                //if (!IsEditPage)
                //{
                //    await OnAddBanking.InvokeAsync();
                if (RetailingCityMonthlySalesDetails != null && RetailingCityMonthlySalesDetails.Count > 0 && RACSalesByYearDetails != null && RACSalesByYearDetails.Count > 0)
                {
                    storeAdditionalInfoCMI.SectionName = OnboardingScreenConstant.DistBusinessDetails;
                    storeAdditionalInfoCMI.DistRetailingCityMonthlySales = RetailingCityMonthlySalesJson;
                    storeAdditionalInfoCMI.DistRacSalesByYear = RACSalesByYearJson;
                    ShowLoader();
                    if (TabName==StoreConstants.Confirmed && !CustomerEditApprovalRequired)
                    {
;                       await RequestChange();
                        await SaveOrUpdateDistBusiness.InvokeAsync(storeAdditionalInfoCMI);
                    }
                    else if (TabName==StoreConstants.Confirmed && CustomerEditApprovalRequired)
                    {
                        await RequestChange();
                    }
                    else
                    {
                        await SaveOrUpdateDistBusiness.InvokeAsync(storeAdditionalInfoCMI);
                    }
                    HideLoader();
                }
                IsSuccess = true;

                //}
                //else
                //{
                //    await OnEditBanking.InvokeAsync();
                //    await SaveOrUpdateDistBusiness.InvokeAsync(storeAdditionalInfoCMI);
                //    await GenerateGridColumns();
                //}
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //}

        }

        private bool ValidateAllFields()
        {
            ValidationMessage = null;

            //if (string.IsNullOrWhiteSpace(RetailingCityMonthlySales.CitySD1) ||
            //    string.IsNullOrWhiteSpace(RetailingCityMonthlySales.CitySD2) ||
            //    string.IsNullOrWhiteSpace(RetailingCityMonthlySales.CitySD3) ||
            //    string.IsNullOrWhiteSpace(RetailingCityMonthlySales.CitySD4) ||
            //    string.IsNullOrWhiteSpace(RetailingCityMonthlySales.CityAMS1) ||
            //    string.IsNullOrWhiteSpace(RetailingCityMonthlySales.CityAMS2) ||
            //    string.IsNullOrWhiteSpace(RetailingCityMonthlySales.CityAMS3) ||
            //    string.IsNullOrWhiteSpace(RetailingCityMonthlySales.CityAMS4) ||
            //    string.IsNullOrWhiteSpace(RACSalesByYear.Year1.ToString()) ||
            //    string.IsNullOrWhiteSpace(RACSalesByYear.Year2.ToString()) ||
            //    string.IsNullOrWhiteSpace(RACSalesByYear.Year3.ToString()) ||
            //    string.IsNullOrWhiteSpace(RACSalesByYear.Qty1.ToString()) ||
            //    string.IsNullOrWhiteSpace(RACSalesByYear.Qty2.ToString()) ||
            //    string.IsNullOrWhiteSpace(RACSalesByYear.Qty3.ToString()))
            //{
            //    ValidationMessage = "The following fields field(s) should not be empty" + ": ";

            //    if (string.IsNullOrWhiteSpace(RetailingCityMonthlySales.CitySD1))
            //    {
            //        ValidationMessage += "City1(Sub Dealer), ";
            //    }

            //    if (string.IsNullOrWhiteSpace(RetailingCityMonthlySales.CitySD2))
            //    {
            //        ValidationMessage += "City2(Sub Dealer), ";
            //    }

            //    if (string.IsNullOrWhiteSpace(RetailingCityMonthlySales.CitySD3))
            //    {
            //        ValidationMessage += "City3(Sub Dealer), ";
            //    }
            //    if (string.IsNullOrWhiteSpace(RetailingCityMonthlySales.CitySD4))
            //    {
            //        ValidationMessage += "City4(Sub Dealer), ";
            //    }

            //    if (string.IsNullOrWhiteSpace(RetailingCityMonthlySales.CityAMS1))
            //    {
            //        ValidationMessage += "City1(Avg Monthly Sales), ";
            //    }

            //    if (string.IsNullOrWhiteSpace(RetailingCityMonthlySales.CityAMS2))
            //    {
            //        ValidationMessage += "City2(Avg Monthly Sales), ";
            //    }
            //    if (string.IsNullOrWhiteSpace(RetailingCityMonthlySales.CityAMS3))
            //    {
            //        ValidationMessage += "City3(Avg Monthly Sales), ";
            //    }
            //    if (string.IsNullOrWhiteSpace(RetailingCityMonthlySales.CityAMS4))
            //    {
            //        ValidationMessage += "City4(Avg Monthly Sales), ";
            //    }
            //    if (string.IsNullOrWhiteSpace(RACSalesByYear.Year1.ToString()))
            //    {
            //        ValidationMessage += "Year, ";
            //    }
            //    if (string.IsNullOrWhiteSpace(RACSalesByYear.Qty1.ToString()))
            //    {
            //        ValidationMessage += "Qty, ";
            //    }
            //    if (string.IsNullOrWhiteSpace(RACSalesByYear.Year2.ToString()))
            //    {
            //        ValidationMessage += "Year, ";
            //    }
            //    if (string.IsNullOrWhiteSpace(RACSalesByYear.Qty2.ToString()))
            //    {
            //        ValidationMessage += "Qty, ";
            //    }
            //    if (string.IsNullOrWhiteSpace(RACSalesByYear.Year3.ToString()))
            //    {
            //        ValidationMessage += "Year, ";
            //    }
            //    if (string.IsNullOrWhiteSpace(RACSalesByYear.Qty3.ToString()))
            //    {
            //        ValidationMessage += "Qty, ";
            //    }
            if (string.IsNullOrWhiteSpace(storeAdditionalInfoCMI.DistNoOfSubDealers.ToString()))
            {
                ValidationMessage = "The following fields field(s) should not be empty" + ": ";

                if (string.IsNullOrWhiteSpace(storeAdditionalInfoCMI.DistNoOfSubDealers.ToString()))
                {
                    ValidationMessage += "No of Sub Dealers, ";
                }
                ValidationMessage = ValidationMessage.TrimEnd(' ', ',');
                return false;
            }
            else
            {
                return true;
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
                    ScreenModelName = OnboardingScreenConstant.DistBusinessDetails,
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
                storeAdditionalinfoCMI.DistRacSalesByYear,
                storeAdditionalinfoCMI.DistRetailingCityMonthlySales,
                storeAdditionalinfoCMI.DistNoOfSubDealers
            };

            return modifiedObject;
        }
        #endregion
    }
}

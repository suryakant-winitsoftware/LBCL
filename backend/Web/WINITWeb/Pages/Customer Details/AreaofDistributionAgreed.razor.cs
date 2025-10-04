using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
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
    public partial class AreaofDistributionAgreed
    {
        public string ValidationMessage;
        private bool IsSaveAttempted { get; set; } = false;
        public List<ISelectionItem> selectionItems = new();
        private bool isChecked = false;
        private bool isDisabled = true;
        private bool IsAddPopUp = false;
        private bool showErrorPopup = false; // To control error message visibility
        public bool IsSuccess { get; set; } = false;
        public string ButtonName { get; set; } = "Save";
        public IStoreAdditionalInfoCMI StoreAdditionalInfoCMI { get; set; }
        public IStoreAdditionalInfoCMI OriginalStoreAdditionalInfoCMI { get; set; }
        [Parameter] public string StoreAdditionalInfoCMIUid { get; set; }
        [Parameter] public EventCallback<IStoreAdditionalInfoCMI> SaveandUpdate { get; set; }

        [Parameter] public IOnBoardCustomerDTO? _onBoardCustomerDTO { get; set; }

        [Parameter] public bool IsEditOnBoardDetails { get; set; }

        [Parameter] public EventCallback<string> InsertDataInChangeRequest { get; set; }
        [Parameter] public bool CustomerEditApprovalRequired { get; set; }
        [Parameter] public string TabName { get; set; }
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            StoreAdditionalInfoCMI = _serviceProvider.CreateInstance<IStoreAdditionalInfoCMI>();
            await SaveInitialValuesAsync();
            if (TabName==StoreConstants.Confirmed)
            {
                var concreteAddress = _onBoardCustomerDTO?.StoreAdditionalInfoCMI as StoreAdditionalInfoCMI;
                OriginalStoreAdditionalInfoCMI=concreteAddress.DeepCopy()!;

            }
            await Task.CompletedTask;
            _loadingService.HideLoading();
        }

        private List<RadioCategory> radioCategories = new()
                    {
                    new RadioCategory { Key = "office", Label = "Office", Name = "officeYesNo", IdYes = "officeYes", IdNo = "officeNo", IsYesChecked = true, IsNoChecked = false },
                    new RadioCategory { Key = "goDown", Label = "Godown", Name = "goDownYesNo", IdYes = "goDownYes", IdNo = "goDownNo", IsYesChecked = true, IsNoChecked = false },
                    new RadioCategory { Key = "manPower", Label = "ManPower", Name = "manPowerYesNo", IdYes = "manPowerYes", IdNo = "manPowerNo", IsYesChecked = true, IsNoChecked = false },
                    new RadioCategory { Key = "serviceCenters", Label = "Service Centers", Name = "serviceCentersYesNo", IdYes = "serviceCentersYes", IdNo = "serviceCentersNo", IsYesChecked = true, IsNoChecked = false },
                    new RadioCategory { Key = "deliveryVan", Label = "Delivery Van", Name = "deliveryVanYesNo", IdYes = "deliveryVanYes", IdNo = "deliveryVanNo", IsYesChecked = true, IsNoChecked = false },
                    new RadioCategory { Key = "salesService", Label = "Sales / Service Man Power", Name = "salesServiceYesNo", IdYes = "salesServiceYes", IdNo = "salesServiceNo", IsYesChecked = true, IsNoChecked = false },
                    new RadioCategory { Key = "computer", Label = "Computer (In Details)", Name = "computerYesNo", IdYes = "computerYes", IdNo = "computerNo", IsYesChecked = true, IsNoChecked = false },
                    new RadioCategory { Key = "others", Label = "Others", Name = "othersYesNo", IdYes = "othersYes", IdNo = "othersNo", IsYesChecked = true, IsNoChecked = false },
                    };
        protected override void OnInitialized()
        {
            if (IsEditOnBoardDetails)
            {
                ButtonName = "Update";
                isDisabled = false;
                InitializedRadioCategoriesVariable();
                InitializedCriteriaListVariable();
            }
            base.OnInitialized();
        }
        public void InitializedRadioCategoriesVariable()
        {
            foreach (var item in radioCategories)
            {
                if (item.Key == "office")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AodaHasOffice ?? true;
                }

                else if (item.Key == "goDown")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AodaHasGodown ?? true;
                }
                else if (item.Key == "manPower")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AodaHasManpower ?? true;
                }
                else if (item.Key == "serviceCenters")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AodaHasServiceCenter ?? true;
                }
                else if (item.Key == "deliveryVan")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AodaHasDeliveryVan ?? true;
                }
                else if (item.Key == "salesService")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AodaHasSalesman ?? true;
                }
                else if (item.Key == "computer")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AodaHasComputer ?? true;
                }
                else if (item.Key == "others")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AodaHasOthers ?? true;
                }
            }

        }

        public void InitializedCriteriaListVariable()
        {
            if (_onBoardCustomerDTO != null)
            {
                //foreach(var criteria in criteriaList)
                //{
                //    if (criteria.Name== "Make Reputation" && criteria.category=="BM")
                //    {

                //        StoreAdditionalInfoCMI.ApMarketReputationLevel1 = value;
                //    }
                //    else if (criteria== "Make Reputation" && category == "FSM")
                //    {
                //        StoreAdditionalInfoCMI.ApMarketReputationLevel2 = value;
                //    }
                //    else if (criteria == "Make Reputation" && category == "Average")
                //    {
                //        StoreAdditionalInfoCMI.ApMarketReputationLevel3 = value;
                //    }
                //    else if (criteria == "Display Quality" && category == "BM")
                //    {
                //        StoreAdditionalInfoCMI.ApDisplayQuantityLevel1 = value;
                //    }
                //    else if (criteria == "Display Quality" && category == "FSM")
                //    {
                //        StoreAdditionalInfoCMI.ApDisplayQuantityLevel2 = value;
                //    }
                //    else if (criteria == "Display Quality" && category == "Average")
                //    {
                //        StoreAdditionalInfoCMI.ApDisplayQuantityLevel3 = value;
                //    }
                //    else if (criteria == "Distribution of Retail Strength" && category == "BM")
                //    {
                //        StoreAdditionalInfoCMI.ApDistRetStrengthLevel1 = value;
                //    }
                //    else if (criteria == "Distribution of Retail Strength" && category == "FSM")
                //    {
                //        StoreAdditionalInfoCMI.ApDistRetStrengthLevel2 = value;
                //    }
                //    else if (criteria == "Distribution of Retail Strength" && category == "Average")
                //    {
                //        StoreAdditionalInfoCMI.ApDistRetStrengthLevel3 = value;
                //    }
                //    else if (criteria == "Financial Strength" && category == "BM")
                //    {
                //        StoreAdditionalInfoCMI.ApFinancialStrengthLevel1 = value;
                //    }
                //    else if (criteria == "Financial Strength" && category == "FSM")
                //    {
                //        StoreAdditionalInfoCMI.ApFinancialStrengthLevel2 = value;
                //    }
                //    else if (criteria == "Financial Strength" && category == "Average")
                //    {
                //        StoreAdditionalInfoCMI.ApFinancialStrengthLevel3 = value;
                //    }
                //    if (criteria== "Make Reputation")
                //    {
                //        StoreAdditionalInfoCMI.ApMarketReputationLevel3 = Convert.ToDecimal(criterion?.AverageScore);
                //    }
                //    else if (criteria == "Distribution of Retail Strength")
                //    {
                //        StoreAdditionalInfoCMI.ApDistRetStrengthLevel3 = Convert.ToDecimal(criterion?.AverageScore);
                //    }
                //    else if (criteria == "Financial Strength")
                //    {
                //        StoreAdditionalInfoCMI.ApFinancialStrengthLevel3 = Convert.ToDecimal(criterion?.AverageScore);
                //    }
                //}

            }
        }
        protected async Task OnClean()
        {
            StoreAdditionalInfoCMI = new StoreAdditionalInfoCMI
            {
                ExpectedTO1 = null,
                ExpectedTO2 = null,
                ExpectedTO3 = null,
            };

            StateHasChanged();
        }
        public async Task SaveOrUpdate()
        {
            IsSaveAttempted = true;
            ValidateAllFields();

            if (string.IsNullOrWhiteSpace(ValidationMessage))
            {
                if (!_onBoardCustomerDTO.StoreAdditionalInfoCMI.IsAgreedWithTNC)
                {
                    // Show error popup if the checkbox is not checked
                    showErrorPopup = true;
                    StateHasChanged(); // Refresh the UI to reflect changes
                }
                else
                {

                    try
                    {
                        _onBoardCustomerDTO.StoreAdditionalInfoCMI.SectionName = OnboardingScreenConstant.AreaOfDistAgreed;
                        if (TabName==StoreConstants.Confirmed && !CustomerEditApprovalRequired)
                        {
                            await RequestChange();
                            await SaveandUpdate.InvokeAsync(_onBoardCustomerDTO.StoreAdditionalInfoCMI);
                        }
                        else if (TabName==StoreConstants.Confirmed && CustomerEditApprovalRequired)
                        {
                            await RequestChange();
                        }
                        else
                        {
                            await SaveandUpdate.InvokeAsync(_onBoardCustomerDTO.StoreAdditionalInfoCMI);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    IsSuccess = true;

                }
            }

        }
        private void ValidateAllFields()
        {
            ValidationMessage = null;

            if (string.IsNullOrWhiteSpace(_onBoardCustomerDTO.StoreAdditionalInfoCMI.AodaExpectedTo1.ToString()) ||
                string.IsNullOrWhiteSpace(_onBoardCustomerDTO.StoreAdditionalInfoCMI.AodaExpectedTo2.ToString()) ||
                string.IsNullOrWhiteSpace(_onBoardCustomerDTO.StoreAdditionalInfoCMI.AodaExpectedTo3.ToString()))
            {
                ValidationMessage = "The Following fields have invalid field(s)" + ": ";

                if (string.IsNullOrWhiteSpace(_onBoardCustomerDTO.StoreAdditionalInfoCMI.AodaExpectedTo1.ToString()))
                {
                    ValidationMessage += "Expected TO 3 Months , ";
                }

                if (string.IsNullOrWhiteSpace(_onBoardCustomerDTO.StoreAdditionalInfoCMI.AodaExpectedTo2.ToString()))
                {
                    ValidationMessage += "Expected TO 6 Months, ";
                }
                if (string.IsNullOrWhiteSpace(_onBoardCustomerDTO.StoreAdditionalInfoCMI.AodaExpectedTo3.ToString()))
                {
                    ValidationMessage += "Expected TO 1 year, ";
                }

                ValidationMessage = ValidationMessage.TrimEnd(' ', ',');
            }
        }
       
        private class RadioCategory
        {
            public string Key { get; set; }
            public string Label { get; set; }
            public string Name { get; set; }
            public string IdYes { get; set; }
            public string IdNo { get; set; }
            public bool IsYesChecked { get; set; }
            public bool IsNoChecked { get; set; }
        }
        private async Task SaveInitialValuesAsync()
        {
            foreach (var category in radioCategories)
            {
                await OnRadioChanged1(category.Key, category.IsYesChecked);
            }
        }
        private async Task OnRadioChanged1(string key, bool selected)
        {
            // Update the state of the selected category
            var selectedCategory = radioCategories.FirstOrDefault(c => c.Key == key);
            if (selectedCategory != null)
            {
                selectedCategory.IsYesChecked = selected;
                selectedCategory.IsNoChecked = !selected;
            }
            if (key == "office")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AodaHasOffice = selected;
            }
            else if (key == "goDown")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AodaHasGodown = selected;
            }
            else if (key == "manPower")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AodaHasManpower = selected;
            }
            else if (key == "serviceCenters")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AodaHasServiceCenter = selected;
            }
            else if (key == "deliveryVan")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AodaHasDeliveryVan = selected;
            }
            else if (key == "salesService")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AodaHasSalesman = selected;
            }
            else if (key == "computer")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AodaHasComputer = selected;
            }
            else if (key == "others")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AodaHasOthers = selected;
            }
        }

        public async Task OnUploadAreaofDistribution()
        {
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.IsAgreedWithTNC = true;
            isDisabled = false;
            IsAddPopUp = false;
            StateHasChanged();
        }

        private void OnCheckboxChanged(ChangeEventArgs e)
        {
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.IsAgreedWithTNC = (bool)e.Value;
            if (!_onBoardCustomerDTO.StoreAdditionalInfoCMI.IsAgreedWithTNC)
            {
                isDisabled = true;
            }
        }
        //private void OnCheckboxChanged(ChangeEventArgs e)
        //{
        //    isChecked = (bool)e.Value;
        //    // Enable or disable the Save button based on the checkbox state
        //    isDisabled = !isChecked;
        //}
        private async Task OpenPopup()
        {
            IsAddPopUp = true;
            await Task.Delay(0);
        }
        public async Task OnCancelAreaofDistribution()
        {
            IsAddPopUp = false;
            StateHasChanged();
        }
        private async Task OnSaveClick()
        {
            if (!isChecked)
            {
                // Show error popup if the checkbox is not checked
                showErrorPopup = true;
                StateHasChanged(); // Refresh the UI to reflect changes
            }
            else
            {
                // Handle the save logic here
                // For example, you could call an API or save data
            }
        }
        private void OnOKErrorPopup()
        {
            showErrorPopup = false;
            StateHasChanged(); // Refresh the UI to reflect changes
        }
        private void HandleRadioChange(ChangeEventArgs e, string category)
        {
            // Extract the value from ChangeEventArgs
            var selectedValue = e.Value?.ToString();

            // Update the points based on the category
            if (selectedValue != null)
            {
                points[category] = CalculatePoints(selectedValue);
                Mapping(points, category);
                RecalculateAverage();
                StateHasChanged();
            }
        }
        public int? Mapping(Dictionary<string, decimal> mappingPoints, string category)
        {
            try
            {
                switch (category)
                {
                    case "dist_bm1":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.ApMarketReputationLevel1 = Convert.ToInt32(mappingPoints[category]);
                    case "dist_fsm1":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.ApMarketReputationLevel2 = Convert.ToInt32(mappingPoints[category]);
                    case "dist_bm2":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.ApDisplayQuantityLevel1 = Convert.ToInt32(mappingPoints[category]);
                    case "dist_fsm2":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.ApDisplayQuantityLevel2 = Convert.ToInt32(mappingPoints[category]);
                    case "dist_bm3":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.ApDistRetStrengthLevel1 = Convert.ToInt32(mappingPoints[category]);
                    case "dist_fsm3":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.ApDistRetStrengthLevel2 = Convert.ToInt32(mappingPoints[category]);
                    case "dist_bm4":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.ApFinancialStrengthLevel1 = Convert.ToInt32(mappingPoints[category]);
                    case "dist_fsm4":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.ApFinancialStrengthLevel2 = Convert.ToInt32(mappingPoints[category]);
                    default:
                        return 0;
                }
                StateHasChanged();
            }
            catch (Exception ex)
            {
                return 0;
            }
        } 
        private async void RecalculateAverage()
        {
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.ApMarketReputationLevel3 = ((decimal)_onBoardCustomerDTO.StoreAdditionalInfoCMI.ApMarketReputationLevel1 + _onBoardCustomerDTO.StoreAdditionalInfoCMI.ApMarketReputationLevel2) / 2;
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.ApDisplayQuantityLevel3 = ((decimal)_onBoardCustomerDTO.StoreAdditionalInfoCMI.ApDisplayQuantityLevel1 + _onBoardCustomerDTO.StoreAdditionalInfoCMI.ApDisplayQuantityLevel2) / 2;
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.ApDistRetStrengthLevel3 = ((decimal)_onBoardCustomerDTO.StoreAdditionalInfoCMI.ApDistRetStrengthLevel1 + _onBoardCustomerDTO.StoreAdditionalInfoCMI.ApDistRetStrengthLevel2) / 2;
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.ApFinancialStrengthLevel3 = ((decimal)_onBoardCustomerDTO.StoreAdditionalInfoCMI.ApFinancialStrengthLevel1 + _onBoardCustomerDTO.StoreAdditionalInfoCMI.ApFinancialStrengthLevel2) / 2;
            TotalAverageScore();
            StateHasChanged();
        }
        public decimal? TotalAverageScore()
        {
            return (_onBoardCustomerDTO.StoreAdditionalInfoCMI.ApMarketReputationLevel3 +
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.ApDisplayQuantityLevel3 +
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.ApDistRetStrengthLevel3 +
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.ApFinancialStrengthLevel3) / 4;
        }

        private decimal CalculateAverage(List<decimal> points)
        {
            if (points.Count == 0)
                return 0;

            decimal sum = points.Sum();
            return sum / points.Count;
        }

        private decimal CalculatePoints(string selectedValue)
        {
            return selectedValue switch
            {
                "Good" => 3,
                "Average" => 2,
                "Poor" => 1,
                _ => 0
            };
        }

        // Initialize the dictionary to store points for each category
        private Dictionary<string, decimal> points = new Dictionary<string, decimal>();
        private decimal averageScore;
        #region Change RequestLogic
       
        public async Task RequestChange()
        {
            List<IChangeRecordDTO> ChangeRecordDTOs = new List<IChangeRecordDTO>
            {
                new ChangeRecordDTO
                {
                     Action= OnboardingScreenConstant.Update,
                    ScreenModelName = OnboardingScreenConstant.AreaOfDistAgreed,
                    UID = StoreAdditionalInfoCMIUid,
                    ChangeRecords = CommonFunctions.GetChangedData(CommonFunctions.CompareObjects(OriginalStoreAdditionalInfoCMI!, _onBoardCustomerDTO?.StoreAdditionalInfoCMI!)!)
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
                storeAdditionalinfoCMI.AodaExpectedTo1,
                storeAdditionalinfoCMI.AodaExpectedTo2,
                storeAdditionalinfoCMI.AodaExpectedTo3,
                storeAdditionalinfoCMI.AodaHasOffice,
                storeAdditionalinfoCMI.AodaHasGodown,
                storeAdditionalinfoCMI.AodaHasManpower,
                storeAdditionalinfoCMI.AodaHasDeliveryVan,
                storeAdditionalinfoCMI.AodaHasSalesman,
                storeAdditionalinfoCMI.AodaHasComputer,
                storeAdditionalinfoCMI.AodaHasOthers,
                storeAdditionalinfoCMI.ApMarketReputationLevel1,
                storeAdditionalinfoCMI.ApMarketReputationLevel2,
                storeAdditionalinfoCMI.ApMarketReputationLevel3,
                storeAdditionalinfoCMI.ApDisplayQuantityLevel1,
                storeAdditionalinfoCMI.ApDisplayQuantityLevel2,
                storeAdditionalinfoCMI.ApDisplayQuantityLevel3,
                storeAdditionalinfoCMI.ApDistRetStrengthLevel1,
                storeAdditionalinfoCMI.ApDistRetStrengthLevel2,
                storeAdditionalinfoCMI.ApDistRetStrengthLevel3,
                storeAdditionalinfoCMI.ApFinancialStrengthLevel1,
                storeAdditionalinfoCMI.ApFinancialStrengthLevel2,
                storeAdditionalinfoCMI.ApFinancialStrengthLevel3,
                storeAdditionalinfoCMI.IsAgreedWithTNC
            };

            return modifiedObject;
        }
        #endregion
    }
}

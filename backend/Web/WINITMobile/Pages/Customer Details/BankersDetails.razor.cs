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
using WINITMobile.Pages.Base;

namespace WINITMobile.Pages.Customer_Details
{
    public partial class BankersDetails : BaseComponentBase
    {
        public List<DataGridColumn> DataGridColumns { get; set; }
        public string ValidationMessage;
        private ElementReference BankingInput;
        private bool IsSaveAttempted { get; set; } = false;
        [Parameter] public IStoreBanking SelectedBanking { get; set; } = new StoreBanking();
        public IStoreSignatory StoreSignatory { get; set; } = new StoreSignatory();
        private IStoreAdditionalInfoCMI storeAdditionalInfoCMI = new StoreAdditionalInfoCMI();
        private IStoreAdditionalInfoCMI OriginalstoreAdditionalInfoCMI = new StoreAdditionalInfoCMI();
        [Parameter] public string TabName { get; set; }
        [Parameter] public EventCallback<string> InsertDataInChangeRequest { get; set; }
        [Parameter] public bool CustomerEditApprovalRequired { get; set; }
        public int StoreSignatorySn = 0;
        private bool IsEditSignatoryDetails = false;
        public bool IsDelete = false;
        [Parameter] public string StoreAdditionalInfoCMIUid { get; set; }

        [Parameter] public string FirmName { get; set; }
        [Parameter] public EventCallback<IStoreAdditionalInfoCMI> SaveOrUpdateBanking { get; set; }
        [Parameter] public EventCallback<IStoreBanking> OnAddBanking { get; set; }
        [Parameter] public EventCallback<IStoreBanking> OnEditBanking { get; set; }
        [Parameter] public EventCallback<string> OnDelete { get; set; }
        [Parameter] public Func<Task<List<IStoreBanking>>> OnShowAllBankingClick { get; set; }
        [Parameter]public List<IStoreBankingJson> BankingDetails { get; set; } = new List<IStoreBankingJson>();
        [Parameter] public List<IStoreSignatory> SignatoryDetails { get; set; } = new List<IStoreSignatory>();
        private bool IsEditPage = false;
        public bool IsShowAllBanking { get; set; } = false;
        public bool IsAddPopUp { get; set; }
        public string ButtonName { get; set; } = "Save";
        [Parameter] public bool IsEditOnBoardDetails { get; set; }
        public bool IsSuccess { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {

            if (TabName==Winit.Modules.Store.Model.Constants.StoreConstants.Confirmed)
            {
                storeAdditionalInfoCMI.UID=StoreAdditionalInfoCMIUid;
                OriginalstoreAdditionalInfoCMI.UID=StoreAdditionalInfoCMIUid;
                OriginalstoreAdditionalInfoCMI.SignatoryDetails=CommonFunctions.ConvertToJson(SignatoryDetails);
                OriginalstoreAdditionalInfoCMI.BankDetails=CommonFunctions.ConvertToJson(BankingDetails);

            }
            await GenerateGridColumns();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }
        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {

                new DataGridColumn {Header = "Name", GetValue = s => ((IStoreSignatory)s)?.Name ?? "N/A"},
                new DataGridColumn {Header = "Address", GetValue = s => ((IStoreSignatory)s)?.Address ?? "N/A"},
                new DataGridColumn {Header = "PAN Number ", GetValue = s =>((IStoreSignatory) s) ?.PanNo ?? "N/A"},
                new DataGridColumn {Header = "Present O/D Limit(Rs in Lacs)", GetValue = s => ((IStoreSignatory)s)?.ODLimit ?? "N/A"},

                new DataGridColumn
                {
                Header = "Actions",
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "Images/edit.png",
                        Action = item => OnEditClick((IStoreSignatory)item)

                    },
                    new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "Images/delete.png",
                        Action = item => OnDeleteClick((IStoreSignatory)item)

                    }
                }
            }
             };
        }
        private string AccountNumbervalidationMessage = string.Empty;
        private string AccountNumbervalidationMessage2 = string.Empty;
        private void ValidateAccountNumber(ChangeEventArgs e)
        {
            string input = e.Value?.ToString();

            if (string.IsNullOrEmpty(input) || (input.Length <= 7 || input.Length >= 16))
            {
                AccountNumbervalidationMessage = "Account number should be in range of 8-15 digits.";
            }
            else
            {
                AccountNumbervalidationMessage = string.Empty;
            }
        }
        private void ValidateAccountNumber2(ChangeEventArgs e)
        {
            string input = e.Value?.ToString();

            if (string.IsNullOrEmpty(input) || (input.Length <= 7 || input.Length >= 16))
            {
                AccountNumbervalidationMessage2 = "Account number should be in range of 8-15 digits.";
            }
            else
            {
                AccountNumbervalidationMessage2 = string.Empty;
            }
        }
        private string PanValidationMessage = string.Empty;
        private void ValidatePanNumber(ChangeEventArgs e)
        {
            string input = e.Value?.ToString();

            // Regex to match the PAN format: 5 uppercase letters, 4 digits, 1 uppercase letter
            var panRegex = new System.Text.RegularExpressions.Regex(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$");

            if (string.IsNullOrEmpty(input) || !panRegex.IsMatch(input))
            {
                PanValidationMessage = "Invalid PAN format. The format should be 5 alphabets, 4 digits, and 1 alphabet (e.g., ERTYU5656R).";
            }
            else
            {
                PanValidationMessage = string.Empty;
            }
        }
        public void OnEditClick(IStoreSignatory storeSignatory)
        {
            IStoreSignatory duplicateStoreSignatory = (storeSignatory as StoreSignatory).DeepCopy()!;
            StoreSignatory = duplicateStoreSignatory;
            IsEditSignatoryDetails = true;
            ButtonName = "Update";
            FocusOnContactName();
            StateHasChanged();
        }
        public async Task OnDeleteClick(IStoreSignatory StoreSignatory)
        {
            if (await _AlertMessgae.ShowConfirmationReturnType("Delete", "Are you sure want to Delete this?"))
            {
                SignatoryDetails.Remove(StoreSignatory);
                for (int i = 0; i < SignatoryDetails.Count; i++)
                {
                    SignatoryDetails[i].Sn = i + 1;
                }
                SignatoryJson = JsonConvert.SerializeObject(SignatoryDetails);
                BankingJson = JsonConvert.SerializeObject(SelectedBanking);
                storeAdditionalInfoCMI.SectionName = OnboardingScreenConstant.BankersDetails;
                storeAdditionalInfoCMI.SignatoryDetails = SignatoryJson;
                storeAdditionalInfoCMI.BankDetails = BankingJson;
                storeAdditionalInfoCMI.Action = "Delete";
                await SaveOrUpdateBanking.InvokeAsync(storeAdditionalInfoCMI);
            }

        }

        private async void FocusOnContactName()
        {
            await BankingInput.FocusAsync();
        }
        public async Task GetAllBankingDetails()
        {
            IsShowAllBanking = !IsShowAllBanking;
        }
        protected async Task OnClean()
        {

            //SelectedBanking = new StoreBanking
            //{
            //    Sn = null,
            //    BankAccountNo1 = null,
            //    BankName1 = string.Empty,
            //    BankAddressFirst1 = string.Empty,
            //    BankAddressFirst2 = string.Empty,
            //    IFSCCode1 = string.Empty,

            //};
            StoreSignatory = new StoreSignatory
            {
                Name = string.Empty,
                Address = string.Empty,
                PanNo = string.Empty,
                ODLimit = string.Empty
            };

            IsEditPage = false;
            ButtonName = "Save";
            StateHasChanged();
        }
        public string BankingJson { get; set; }
        public string SignatoryJson { get; set; }
        protected void GetShowRoomJson()
        {
            if (IsEditOnBoardDetails)
            {
                //    List<IStoreBanking> Copy = new List<IStoreBanking>();
                //    Copy.Add(SelectedBanking);
                //    BankingJson = JsonConvert.SerializeObject(Copy);
                BankingJson = JsonConvert.SerializeObject(BankingDetails);

                SignatoryJson = JsonConvert.SerializeObject(SignatoryDetails);
            }
            else
            {
                BankingJson = JsonConvert.SerializeObject(BankingDetails);
                SignatoryJson = JsonConvert.SerializeObject(SignatoryDetails);
            }
        }
        public void MapBankingDetails()
        {
            var firstBankDetails = new StoreBankingJson
            {
                Sn = 1, // Increment Sn based on the count
                BankAccountNo = SelectedBanking.BankAccountNo1,
                BankAddress1 = SelectedBanking.BankAddressFirst1,
                BankAddress2 = SelectedBanking.BankAddressFirst2,
                BankName = SelectedBanking.BankName1,
                IFSCCode = SelectedBanking.IFSCCode1
            };

            // Create the second bank details object, excluding the first bank details
            var secondBankDetails = new StoreBankingJson
            {
                Sn = 2, // Use the same Sn as the first object
                BankAccountNo = SelectedBanking.BankAccountNo2,
                BankAddress1 = SelectedBanking.BankAddressSecond1,
                BankAddress2 = SelectedBanking.BankAddressSecond2,
                BankName = SelectedBanking.BankName2,
                IFSCCode = SelectedBanking.IFSCCode2
            };
            BankingDetails.Clear();
            // Add the first and second bank details objects to the list separately
            BankingDetails.Add(firstBankDetails);
            BankingDetails.Add(secondBankDetails);
        }
        public void AddBankingDetails()
        {
            if (ValidateAllFields())
            {
                if (!IsEditSignatoryDetails)
                {
                    //if (SelectedBanking != null && !BankingDetails.Any())
                    //{
                    //    // Create the first bank details object, excluding the second bank details
                    //    MapBankingDetails();

                    //    // Clear SelectedBanking if necessary
                    //SelectedBanking = new StoreBanking();

                    //}

                    if (StoreSignatory != null)
                    {
                        SignatoryDetails.Add(new StoreSignatory
                        {
                            Sn = SignatoryDetails.Count + 1,
                            Name = StoreSignatory.Name,
                            Address = StoreSignatory.Address,
                            PanNo = StoreSignatory.PanNo,
                            ODLimit = StoreSignatory.ODLimit
                        });
                    }
                }
                else
                {
                    int index = SignatoryDetails.FindIndex(s => s.Sn == StoreSignatory.Sn);
                    

                    if (index != -1 )
                    {
                        SignatoryDetails[index] = StoreSignatory;
                    }
                    IsEditSignatoryDetails = false;
                }

                StoreSignatory = new StoreSignatory();
            }
        }


        protected async Task SaveOrUpdate()
        {
            IsDelete = false;
            MapBankingDetails();
            await SaveUpdateDeleteBankers();
        }
        public async Task SaveUpdateDeleteBankers()
        {
            IsSaveAttempted = true;
            //ValidateAllFields();
            //if (string.IsNullOrWhiteSpace(ValidationMessage))
            //{
            try
            {

                GetShowRoomJson();
                //if (!IsEditPage)
                //{
                //    await OnAddBanking.InvokeAsync();
                if (BankingDetails.Any() && SignatoryDetails.Any())
                {
                    storeAdditionalInfoCMI.SectionName = OnboardingScreenConstant.BankersDetails;
                    storeAdditionalInfoCMI.BankDetails = BankingJson;
                    storeAdditionalInfoCMI.SignatoryDetails = SignatoryJson;
                    storeAdditionalInfoCMI.Action = IsDelete ? "Delete" : "Save";
                    if(TabName==StoreConstants.Confirmed && !CustomerEditApprovalRequired)
                    {
                        await RequestChange();
                        await SaveOrUpdateBanking.InvokeAsync(storeAdditionalInfoCMI);
                    }
                    else if(TabName==StoreConstants.Confirmed && CustomerEditApprovalRequired)
                    {
                        await RequestChange();
                    }
                    else
                    {
                        await SaveOrUpdateBanking.InvokeAsync(storeAdditionalInfoCMI);
                    }
                    await GenerateGridColumns();
                }
                else
                {
                    ValidateAllFields();
                }
                IsSuccess = true;

                //}
                //else
                //{
                //    await OnEditBanking.InvokeAsync();
                //    await SaveOrUpdateBanking.InvokeAsync(storeAdditionalInfoCMI);
                //    await GenerateGridColumns();
                //}
            }


            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //}

        }
        public bool IsValidPAN(string pan)
        {
            // PAN regex pattern
            if (!string.IsNullOrWhiteSpace(pan))
            {
                string pattern = @"^[A-Z]{5}[0-9]{4}[A-Z]{1}$";

                // Match the PAN with the regex pattern
                return Regex.IsMatch(pan, pattern);
            }
            return false;
        }
        public bool IsValidAccountNumber(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return false;
            }

            // Example: Account number validation for numbers between 8 and 15 digits
            var accountPattern = @"^\d{8,15}$";
            return Regex.IsMatch(accountNumber, accountPattern);
        }
        private bool ValidateAllFields()
        {
            ValidationMessage = null;

            if ((string.IsNullOrWhiteSpace(SelectedBanking.BankAccountNo1.ToString()) ||
                !IsValidAccountNumber(SelectedBanking.BankAccountNo1.ToString()) ||
                string.IsNullOrWhiteSpace(SelectedBanking.BankName1) ||
                string.IsNullOrWhiteSpace(SelectedBanking.BankAddressFirst1) ||
                string.IsNullOrWhiteSpace(SelectedBanking.BankAddressFirst2) ||
                string.IsNullOrWhiteSpace(SelectedBanking.IFSCCode1) ||
                string.IsNullOrWhiteSpace(StoreSignatory.Name) ||
                 string.IsNullOrWhiteSpace(StoreSignatory.ODLimit) ||
                !IsValidPAN(StoreSignatory.PanNo)))
            {
                ValidationMessage = "The following fields have invalid field(s)" + ": ";

                if (string.IsNullOrWhiteSpace(SelectedBanking.BankAccountNo1.ToString()) || !IsValidAccountNumber(SelectedBanking.BankAccountNo1.ToString()))
                {
                    ValidationMessage += "BankAccountNo, ";
                }

                if (string.IsNullOrWhiteSpace(SelectedBanking.BankName1))
                {
                    ValidationMessage += "BankName, ";
                }
                if (string.IsNullOrWhiteSpace(SelectedBanking.BankAddressFirst1))
                {
                    ValidationMessage += "Bank Address1, ";
                }
                if (string.IsNullOrWhiteSpace(SelectedBanking.BankAddressFirst2))
                {
                    ValidationMessage += "Bank Address2, ";
                }
                if (string.IsNullOrWhiteSpace(SelectedBanking.IFSCCode1))
                {
                    ValidationMessage += "IFSCCode, ";
                }
                if (string.IsNullOrWhiteSpace(StoreSignatory.Name))
                {
                    ValidationMessage += "Name, ";
                }
                if (!IsValidPAN(StoreSignatory.PanNo))
                {
                    ValidationMessage += "PanNo, ";
                }
                if (string.IsNullOrWhiteSpace(StoreSignatory.ODLimit))
                {
                    ValidationMessage += "ODLimit, ";
                }

                ValidationMessage = ValidationMessage.TrimEnd(' ', ',');
                return false;
            }
            else
            {
                return true;
            }
        }


        public async Task GetAllBankersDetails()
        {
            IsShowAllBanking = !IsShowAllBanking;
        }
        #region Change RequestLogic
      
        public async Task RequestChange()
        {
            List<IChangeRecordDTO> ChangeRecordDTOs = new List<IChangeRecordDTO>
            {
                new ChangeRecordDTO
                {
                    Action = OnboardingScreenConstant.Update,
                    ScreenModelName = OnboardingScreenConstant.BankersDetails,
                    UID = StoreAdditionalInfoCMIUid,
                    ChangeRecords = CommonFunctions.GetChangedData(CommonFunctions.CompareObjects(OriginalstoreAdditionalInfoCMI!, storeAdditionalInfoCMI)!)
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
            return new
            {
                BankDetails = new { Value = storeAdditionalinfoCMI.BankDetails, ColumnName = "bank_details" },
                SignatoryDetails = new { Value = storeAdditionalinfoCMI.SignatoryDetails, ColumnName = "signatory_details" }
            };
        }

        #endregion
    }
}

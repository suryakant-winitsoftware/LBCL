using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Constants;
using WINITMobile.Pages.Base;

namespace WINITMobile.Pages.Customer_Details
{
    public partial class NameAndAddressOfProprietor : BaseComponentBase
    {
        [Parameter] public List<NameAndAddressOfProprietorModel> KartaDetails { get; set; } = new List<NameAndAddressOfProprietorModel> { new NameAndAddressOfProprietorModel { Sn = 1 } };
        [Parameter] public IStoreAdditionalInfoCMI storeAdditionalInfoCMI { get; set; } = new StoreAdditionalInfoCMI();
        [Parameter] public bool IsEditOnBoardDetails { get; set; }
        [Parameter] public EventCallback<IStoreAdditionalInfoCMI> SaveOrUpdateKarta { get; set; }
        public string ButtonName { get; set; } = "Save";
        public string AllKartaDetails { get; set; }
        private string Mobile2validationMessage = string.Empty;
        private string MobilevalidationMessage = string.Empty;
        private string AadharNumbervalidationMessage = string.Empty;
        private string PanValidationMessage = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            if (IsEditOnBoardDetails)
            {
                ButtonName = "Update";
                if (KartaDetails.Count == 0)
                {
                    KartaDetails = new List<NameAndAddressOfProprietorModel> { new NameAndAddressOfProprietorModel { Sn = 1 } };
                }
            }
            StateHasChanged();
        }
        private void ValidateMobileNumber(ChangeEventArgs e, NameAndAddressOfProprietorModel karta)
        {
            string input = e.Value?.ToString();

            if (!string.IsNullOrEmpty(input) && input.Length != 10)
            {
                karta.MobilevalidationMessage = "Mobile number must be exactly 10 digits.";
            }
            else
            {
                karta.MobilevalidationMessage = string.Empty;
            }
        }
        private void ValidatePanNumber(ChangeEventArgs e, NameAndAddressOfProprietorModel karta)
        {
            string input = e.Value?.ToString();

            // Regex to match the PAN format: 5 uppercase letters, 4 digits, 1 uppercase letter
            var panRegex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z]{5}[0-9]{4}[a-zA-Z]{1}$");
            if (!string.IsNullOrEmpty(input) && !panRegex.IsMatch(input))
            {
                karta.PanValidationMessage = "Invalid PAN format. The format should be 5 alphabets, 4 digits, and 1 alphabet (e.g., ERTYU5656R).";
            }
            else
            {
                karta.PanValidationMessage = string.Empty;
            }
        }
        private void ValidateaadharNumber(ChangeEventArgs e, NameAndAddressOfProprietorModel karta)
        {
            string input = e.Value?.ToString();

            if (!string.IsNullOrEmpty(input) && input.Length != 12)
            {
                karta.AadharNumbervalidationMessage = "Aadhar number should be 12 digits.";
            }
            else
            {
                karta.AadharNumbervalidationMessage = string.Empty;
            }
        }
        private void AddRow()
        {
            var currentSN = KartaDetails.Count;
            KartaDetails.Add(new NameAndAddressOfProprietorModel { Sn = currentSN + 1 });
        }

        private void RemoveRow(NameAndAddressOfProprietorModel brandInfo)
        {
            KartaDetails.Remove(brandInfo);
            for (int i = 0; i < KartaDetails.Count; i++)
            {
                KartaDetails[i].Sn = i + 1;
            }
        }
        protected void GetKartaDetailsJson()
        {
            //TotalStoreCount = ShowroomDetails.Sum(srm => srm.NoOfStores);
            //ShowroomDetails.ForEach(srm => srm.NoOfStores += TotalStoreCount);
            AllKartaDetails = JsonConvert.SerializeObject(KartaDetails);
        }

        public async Task SaveKartaDetails()
        {
            try
            {
                if (await ValidateFields())
                {
                    GetKartaDetailsJson();
                    storeAdditionalInfoCMI.PartnerDetails = AllKartaDetails;
                    storeAdditionalInfoCMI.SectionName = OnboardingScreenConstant.Karta;
                    await SaveOrUpdateKarta.InvokeAsync(storeAdditionalInfoCMI);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<bool> ValidateFields()
        {
            try
            {
                var invalidItems = KartaDetails.Where(item =>
                (!string.IsNullOrEmpty(item.PhoneNumber) && item.PhoneNumber.Length != 10) ||
                (!string.IsNullOrEmpty(item.AadharNumber) && item.AadharNumber.Length != 12) ||
                (!string.IsNullOrEmpty(item.PanNumber) && !new System.Text.RegularExpressions.Regex(@"^[a-zA-Z]{5}[0-9]{4}[a-zA-Z]{1}$").IsMatch(item.PanNumber))).ToList();

                if (invalidItems.Any())
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Shared.Models.Common;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.Modules.Base.BL.Helper.Classes;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using WinIt.Pages.Base;
using Winit.Modules.CollectionModule.Model.Interfaces;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;

namespace WinIt.Pages.Collection.CreatePayment
{
    public partial class EarlyPaymentConfiguration 
    {
        private List<DataGridColumn> Columns { get; set; } = new List<DataGridColumn>();
        public bool IsInitialised { get; set; } = false;
        public List<IEarlyPaymentDiscountConfiguration> earlyPaymentDiscountConfigurations { get; set; } = new List<IEarlyPaymentDiscountConfiguration>();
        private static Dictionary<string, string> storeData { get; set; } = new Dictionary<string, string>();

        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            LoadResources(null, _languageService.SelectedCulture);
            await _viewPaymentsViewModel.GetCustomerCodeName();
            storeData = _viewPaymentsViewModel.storeData;
            earlyPaymentDiscountConfigurations = await _earlyPaymentConfigurationViewModel.GetConfigurationDetails();
            GridColumns();
            StateHasChanged();
            IsInitialised = true;
            _loadingService.HideLoading();
        }

        public void GridColumns()
        {
            Columns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = @Localizer["customer_name"], GetValue = s => BidirectionalLookup(((EarlyPaymentDiscountConfiguration)s).Applicable_Code ?? ""),  IsSortable = true  },
                new DataGridColumn { Header = @Localizer["type"], GetValue = s => ((EarlyPaymentDiscountConfiguration)s).Applicable_Type ?? "",  IsSortable = true  },
                new DataGridColumn { Header = @Localizer["org"], GetValue = s => ((EarlyPaymentDiscountConfiguration)s).Sales_Org ?? "",  IsSortable = true },
                new DataGridColumn { Header = @Localizer["payment_mode"], GetValue = s => ((EarlyPaymentDiscountConfiguration)s).Payment_Mode ?? "",  IsSortable = true },
                new DataGridColumn { Header = @Localizer["advance_days"], GetValue = s => ((EarlyPaymentDiscountConfiguration)s).Advance_Paid_Days   },
                new DataGridColumn { Header = @Localizer["discount_type"], GetValue = s => ((EarlyPaymentDiscountConfiguration)s).Discount_Type ?? "" },
                new DataGridColumn { Header = @Localizer["discount_value"], GetValue = s => ((EarlyPaymentDiscountConfiguration)s).Discount_Value},
                new DataGridColumn { Header = @Localizer["is_active"], GetValue = s => ((EarlyPaymentDiscountConfiguration)s).IsActive  },
                new DataGridColumn { Header = @Localizer["validfrom"], GetValue = s => CommonFunctions.GetDateTimeInFormat(((EarlyPaymentDiscountConfiguration)s).Valid_From) },
                new DataGridColumn { Header = @Localizer["validto"], GetValue = s => CommonFunctions.GetDateTimeInFormat(((EarlyPaymentDiscountConfiguration)s).Valid_To) },
                new DataGridColumn { Header = @Localizer["is_partial_payments"], GetValue = s => ((EarlyPaymentDiscountConfiguration)s).Applicable_OnPartial_Payments },
                new DataGridColumn { Header = @Localizer["is_on_overdue_customers"], GetValue = s => ((EarlyPaymentDiscountConfiguration)s).Applicable_OnOverDue_Customers },

            };

        }
        static string BidirectionalLookup(string input)
        {
            // Check if the input is a key in the dictionary
            if (storeData.TryGetValue(input, out string value))
            {
                return value; // Return the value (customer name or GUID) if found
            }

            // Check if the input is a value in the dictionary
            foreach (var pair in storeData)
            {
                if (pair.Value == input)
                {
                    return pair.Key; // Return the key (customer name) if found
                }
            }
            return "";
        }
        public async Task NavigateToAddNew()
        {
            try
            {
                _navigationManager.NavigateTo("addearlypayment");
            }
            catch(Exception ex)
            {

            }
        }
    }
}

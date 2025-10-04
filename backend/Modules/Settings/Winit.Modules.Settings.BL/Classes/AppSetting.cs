using Winit.Modules.Setting.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.Setting.BL.Classes
{
    public class AppSettings : Interfaces.IAppSetting
    {
        private readonly Dictionary<string, Dictionary<string, ISetting>> _settings = new Dictionary<string, Dictionary<string, ISetting>>();

        public void PopulateSettings(IEnumerable<ISetting> settings)
        {
            if (settings != null)
            {
                foreach (var setting in settings)
                {
                    string type = (setting.Type ?? "global");
                    string name = (setting.Name ?? "");

                    if (!_settings.ContainsKey(type))
                    {
                        _settings[type] = new Dictionary<string, ISetting>();
                    }

                    _settings[type][name] = setting;
                }
            }
        }

        public string GetValue(string name, string type = AppSettingType.Global)
        {
            if (_settings.TryGetValue(type, out var typeSettings) &&
                typeSettings.TryGetValue(name, out var setting))
            {
                return setting.Value;
            }

            return null;// Or return a default value.
        }

        public int DefaultDeliveryDay
        {
            get
            {
                return CommonFunctions.GetIntValue(GetValue(AppSettingNames.DefaultDeliveryDay));
            }
        }
        public int RoundOffDecimal
        {
            get
            {
                return CommonFunctions.GetIntValue(GetValue(AppSettingNames.RoundOffDecimal));
            }
        }
        public int DefaultMaxQty
        {
            get
            {
                return CommonFunctions.GetIntValue(GetValue(AppSettingNames.DefaultMaxQty));
            }
        }
        public bool LOAD_IS_CPE_APPROVAL_REQUIRED
        {
            get
            {
                return CommonFunctions.GetBooleanValue(GetValue(AppSettingNames.LOAD_IS_CPE_APPROVAL_REQUIRED));
            }
        }
        public bool LOAD_IS_ERP_APPROVAL_REQUIRED
        {
            get
            {
                return CommonFunctions.GetBooleanValue(GetValue(AppSettingNames.LOAD_IS_ERP_APPROVAL_REQUIRED));
            }
        }
        public bool Customer_Edit_Approval_Required
        {
            get
            {
                return CommonFunctions.GetBooleanValue(GetValue(AppSettingNames.Customer_Edit_Approval_Required));
            }
        }
        public string LocationLevel
        {
            get
            {
                return CommonFunctions.GetStringValue(GetValue(AppSettingNames.LocationLevel));
            }
        }
        public string SKUGroupLevel
        {
            get
            {
                return CommonFunctions.GetStringValue(GetValue(AppSettingNames.SKUGroupLevel));
            }
        }
        public string StoreGroupLevel
        {
            get
            {
                return CommonFunctions.GetStringValue(GetValue(AppSettingNames.StoreGroupLevel));
            }
        }
        public bool Payment_Multicurrency_Allowed
        {
            get
            {
                return CommonFunctions.GetBooleanValue(GetValue(AppSettingNames.Payment_Multicurrency_Allowed));
            }
        }
        public bool IsRejectNeededInOnBoarding
        {
            get
            {
                return CommonFunctions.GetBooleanValue(GetValue(AppSettingNames.IsRejectNeededInOnBoarding));
            }
        }
        public bool IsReassignNeededInOnBoarding
        {
            get
            {
                return CommonFunctions.GetBooleanValue(GetValue(AppSettingNames.IsReassignNeededInOnBoarding));
            }
        }
        public bool IsRejectNeededInScheme
        {
            get
            {
                return CommonFunctions.GetBooleanValue(GetValue(AppSettingNames.IsRejectNeededInScheme));
            }
        }
        public bool IsRejectNeededInPurchaseOrder
        {
            get
            {
                return CommonFunctions.GetBooleanValue(GetValue(AppSettingNames.IsRejectNeededInPurchaseOrder));
            }
        }
        public bool IsReassignNeededInPurchaseOrder
        {
            get
            {
                return CommonFunctions.GetBooleanValue(GetValue(AppSettingNames.IsReassignNeededInPurchaseOrder));
            }
        }
        public bool IsReassignNeededInScheme
        {
            get
            {
                return CommonFunctions.GetBooleanValue(GetValue(AppSettingNames.IsReassignNeededInScheme));
            }
        }
        public string PriceApplicationModel => CommonFunctions.GetStringValue(GetValue(AppSettingNames.PriceApplicationModel, AppSettingType.FR));
        public int SchemeCalendarPeriod => CommonFunctions.GetIntValue(GetValue(AppSettingNames.SchemeCalendarPeriod, AppSettingType.Global));
        public bool IsPriceInclusiveVat => CommonFunctions.GetBooleanValue(GetValue(AppSettingNames.IsPriceInclusiveVat));
        public string SELL_IN_END_DATE_ROLES => CommonFunctions.GetStringValue(GetValue(AppSettingNames.SELL_IN_END_DATE_ROLES));
        public bool IsAPMasterValidationRequired => CommonFunctions.GetBooleanValue(GetValue(AppSettingNames.IsAPMasterValidationRequired));
        public int TempCreditLimitMaxDays => CommonFunctions.GetIntValue(GetValue(AppSettingNames.TempCreditLimitMaxDays));
        public int TempAgingDaysMaxDays => CommonFunctions.GetIntValue(GetValue(AppSettingNames.TempAgingDaysMaxDays));
        public decimal CashDiscountPercentage => CommonFunctions.GetDecimalValue(GetValue(AppSettingNames.CashDiscountPercentage));
        public string EndDateUpdatePermittedRoles => CommonFunctions.GetStringValue(GetValue(AppSettingNames.EndDateUpdatePermittedRoles));
    }
}

using Winit.Modules.Setting.Model.Interfaces;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.Setting.BL.Interfaces
{
    public interface IAppSetting
    {
        public void PopulateSettings(IEnumerable<ISetting> settings);
        public string GetValue(string name, string type = AppSettingType.Global);
        public int DefaultDeliveryDay { get; }
        public int RoundOffDecimal { get; }
        public int DefaultMaxQty { get; }
        public bool LOAD_IS_CPE_APPROVAL_REQUIRED { get; }
        public bool LOAD_IS_ERP_APPROVAL_REQUIRED { get; }
        public bool Customer_Edit_Approval_Required { get; }
        public string LocationLevel { get; }
        public string SKUGroupLevel { get; }
        public string StoreGroupLevel { get; }
        public bool Payment_Multicurrency_Allowed { get; }
        public string PriceApplicationModel { get; }
        public bool IsReassignNeededInPurchaseOrder { get; }
        public bool IsReassignNeededInScheme { get; }
        public bool IsRejectNeededInOnBoarding { get; }
        public bool IsReassignNeededInOnBoarding { get; }
        public bool IsRejectNeededInScheme { get; }
        public bool IsRejectNeededInPurchaseOrder { get; }
        public bool IsPriceInclusiveVat { get; }
        public int SchemeCalendarPeriod { get; }
        public string SELL_IN_END_DATE_ROLES { get; }
        public bool IsAPMasterValidationRequired { get; }
        public int TempCreditLimitMaxDays { get; }
        public int TempAgingDaysMaxDays { get; }
        public decimal CashDiscountPercentage { get; }
        public string EndDateUpdatePermittedRoles { get; }
    }
}

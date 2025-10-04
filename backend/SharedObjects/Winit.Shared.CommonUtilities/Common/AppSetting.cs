using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.CommonUtilities.Common
{
    public class AppSetting
    {
        public static Dictionary<string, Dictionary<string, Setting>> Instance { get; set; } = new Dictionary<string, Dictionary<string, Setting>>();
        static void ConvertSettingsDictionary()
        {
            foreach (var v in Shared.sFAModel.Context.AppContext.Settings)
            {
                string type = (v.Type ?? "global").ToLower();
                if (!Shared.sFAModel.Context.AppContext.AppSetting.Instance.ContainsKey(type))
                {
                    Shared.sFAModel.Context.AppContext.AppSetting.Instance.Add(type, new Dictionary<string, Shared.sFAModel.Setting>());
                }
                var name = (v.Name ?? "").ToLower();
                if (!Shared.sFAModel.Context.AppContext.AppSetting.Instance[type].ContainsKey(name))
                {
                    Shared.sFAModel.Context.AppContext.AppSetting.Instance[type].Add(name, new Shared.sFAModel.Setting());
                }
                Shared.sFAModel.Context.AppContext.AppSetting.Instance[type][name] = v;
            }
        }
        public static string GetValueFromGlobal(string kkey)
        {
            if (Instance != null && Instance.ContainsKey("global"))
            {
                return !Instance["global"].ContainsKey(kkey) ? "" : Instance["global"][kkey].Value;
            }
            else
            {
                return "";
            }
        }
        public static string AppSalesOrderViewDisplayMode
        {
            get
            {
                var kkey = "APP_SALES_ORDER_VIEW_DISPLAY_MODE".ToLower();
                return GetValueFromGlobal(kkey);
            }
        }
        public static string AppSecondaryUOM
        {
            get
            {
                var kkey = "APP_SECONDARY_UOM".ToLower();
                return GetValueFromGlobal(kkey);
            }
        }
        public static string TotalSKUAvailable
        {
            get
            {
                var kkey = "TOTAL_SKU_AVAILABLE".ToLower();
                return GetValueFromGlobal(kkey);
            }
        }


        public static string Company
        {
            get
            {
                var kkey = "COMPANY".ToLower();
                return GetValueFromGlobal(kkey);
            }
        }


        public static string SalesOrderFinalizedStatusCode
        {
            get
            {
                var kkey = "APP_SALES_ORDER_FINALIZED_STATUS_CODE".ToLower();
                return GetValueFromGlobal(kkey);
            }
        }
        public static bool IsAppSalesOrderItemVisibleWithZeroStock
        {
            get
            {
                var kkey = "APP_SALES_ORDER_ITEM_VISIBLE_WITH_ZERO_STOCK".ToLower();
                return CommonFunctions.GetBooleanValue(GetValueFromGlobal(kkey));
            }
        }
        public static bool IsAppSalesOrderFOCItemVisibleSeparately
        {
            get
            {
                var kkey = "APP_SALES_ORDER_FOC_ITEM_VISIBLE_SEPARATELY".ToLower();
                return CommonFunctions.GetBooleanValue(GetValueFromGlobal(kkey));
            }
        }
        public static bool IsAppFOCUnitPriceZero
        {
            get
            {
                var kkey = "APP_FOC_UNIT_PRICE_ZERO".ToLower();
                return CommonFunctions.GetBooleanValue(GetValueFromGlobal(kkey));
            }
        }
        public static bool IsAppDeliveryDateValidationRequired
        {
            get
            {
                var kkey = "APP_DELIVERY_DATE_VALIDATION_REQUIRED".ToLower();
                return CommonFunctions.GetBooleanValue(GetValueFromGlobal(kkey));
            }
        }
        public static bool IsPriceInclusiveVat
        {
            get
            {
                var kkey = "PRICE_INCLUSIVE_VAT".ToLower();
                return CommonFunctions.GetBooleanValue(GetValueFromGlobal(kkey));
            }
        }
        public static bool IsAppMinOrderAmountValidationRequired
        {
            get
            {
                var kkey = "APP_MIN_ORDER_AMOUNT_VALIDATION_REQUIRED".ToLower();
                return CommonFunctions.GetBooleanValue(GetValueFromGlobal(kkey));
            }
        }
        public static bool IsAppSalesOrderItemVisibleByRange
        {
            get
            {
                var kkey = "APP_SALES_ORDER_ITEM_VISIBLE_BY_RANGE".ToLower();
                return CommonFunctions.GetBooleanValue(GetValueFromGlobal(kkey));
            }
        }
        public static bool IsMinOrderQtyEnabled
        {
            get
            {
                var kkey = "IS_MIN_ORDER_QTY_ENABLED".ToLower();
                return CommonFunctions.GetBooleanValue(GetValueFromGlobal(kkey));
            }
        }
        public static int DefaultDeliveryDay
        {
            get
            {
                var kkey = "SALES_ORDER_DEFAULT_DELIVERY_DAY".ToLower();
                return CommonFunctions.GetIntValue(GetValueFromGlobal(kkey));
            }
        }

        public static float MaxTemp
        {
            get
            {
                var kkey = "MaxTemp".ToLower();
                return CommonFunctions.GetFloatValue(GetValueFromGlobal(kkey));
            }
        }

        public static float MinTemp
        {
            get
            {
                var kkey = "MinTemp".ToLower();
                return CommonFunctions.GetFloatValue(GetValueFromGlobal(kkey));
            }
        }


        public static int MinDefaultDeliveryDay
        {
            get
            {
                var kkey = "SALES_ORDER_MIN_DEFAULT_DELIVERY_DAY".ToLower();
                return CommonFunctions.GetIntValue(GetValueFromGlobal(kkey));
            }
        }
        public static int AppSalesOrderItemAvailabiltyNoOfOrders
        {
            get
            {
                var kkey = "APP_SALES_ORDER_ITEM_AVAILABILITY_NO_OF_ORDER".ToLower();
                return CommonFunctions.GetIntValue(GetValueFromGlobal(kkey));
            }
        }
        public static bool IsAppSalesOrderGiftAssetItemEnabled
        {
            get
            {
                var kkey = "APP_SALES_ORDER_GIFT_ASSET_ITEM_ENABLE".ToLower();
                return CommonFunctions.GetBooleanValue(GetValueFromGlobal(kkey));
            }
        }
        public static bool IsCategoryFilterVisibleInSales { get; set; }
        public static bool IsLoginPageImageProfileUpdated
        {
            get
            {
                var kkey = "LOGIN_IMAGE_PROFILE_UPDATED".ToLower();
                return CommonFunctions.GetBooleanValue(GetValueFromGlobal(kkey));
            }
        }
        public static bool NewInvoiceNumberRequired
        {
            get
            {
                var kkey = "NewInvoiceNumberRequired".ToLower();
                return CommonFunctions.GetBooleanValue(GetValueFromGlobal(kkey));
            }
        }
        public static int DataUploadBatchSize
        {
            get
            {
                var kkey = "DATA_UPLOAD_BATCH_SIZE".ToLower();
                return CommonFunctions.GetIntValue(GetValueFromGlobal(kkey));
                //return CommonFunctions.GetIntValue(!Instance["global"].ContainsKey(kkey) ? "0" : Instance["global"][kkey].Value);
            }
        }
        public static bool IsAppSKUSalesInPrimaryCodeOnly
        {
            get
            {
                var kkey = "APP_SKU_SALES_IN_PRIMARY_CODE_ONLY".ToLower();
                return CommonFunctions.GetBooleanValue(GetValueFromGlobal(kkey));
                //return CommonFunctions.GetBooleanValue(!Instance["global"].ContainsKey(kkey) ? "" : Instance["global"][kkey].Value);
            }
        }
        public static string AppreturnOrderPriceSelectionType
        {
            get
            {
                var kkey = "APP_RETURN_ORDER_PRICE_SELECTION_TYPE".ToLower();
                return GetValueFromGlobal(kkey);
                //return !Instance["global"].ContainsKey(kkey) ? "" : Instance["global"][kkey].Value;
            }
        }
        public static string AppreturnOrderPriceSelectionLastN
        {
            get
            {
                var kkey = "APP_RETURN_ORDER_PRICE_SELECTION_LAST_N".ToLower();
                return GetValueFromGlobal(kkey);
                //return !Instance["global"].ContainsKey(kkey) ? "" : Instance["global"][kkey].Value;
            }
        }
        public static string AppReturnOrderValidationValueKPIEnabled
        {
            get
            {
                var kkey = "APP_RETURN_ORDER_VALIDATION_VALUE_KPI_ENABLED".ToLower();
                return GetValueFromGlobal(kkey);
                //return !Instance["global"].ContainsKey(kkey) ? "" : Instance["global"][kkey].Value;
            }
        }
        public static string AppReturnOrderValidationValueKPIPercentage
        {
            get
            {
                var kkey = "APP_RETURN_ORDER_VALIDATION_VALUE_KPI_PERCENTAGE".ToLower();
                return GetValueFromGlobal(kkey);
                //return !Instance["global"].ContainsKey(kkey) ? "" : Instance["global"][kkey].Value;
            }
        }
        public static string AppStoreCheckType
        {
            get
            {
                var kkey = "APP_STORE_CHECK_TYPE".ToLower();
                return GetValueFromGlobal(kkey);
                //return !Instance["global"].ContainsKey(kkey) ? "" : Instance["global"][kkey].Value;
            }
        }
        public static string SalesOrderMinDefaultDeliveryDay
        {
            get
            {
                var kkey = "SALES_ORDER_MIN_DEFAULT_DELIVERY_DAY".ToLower();
                return GetValueFromGlobal(kkey);
                //return !Instance["global"].ContainsKey(kkey) ? "" : Instance["global"][kkey].Value;

            }
        }
        public static string DefaultCreditType
        {
            get
            {
                var kkey = "APP_DEFAULT_CREDITTYPE".ToLower();
                return GetValueFromGlobal(kkey);
                //return !Instance["global"].ContainsKey(kkey) ? "" : Instance["global"][kkey].Value;
            }
        }
        public static bool EnableAppLog
        {
            get
            {
                var kkey = "EnableAppLog".ToLower();
                return CommonFunctions.GetBooleanValue(GetValueFromGlobal(kkey));
                //return CommonFunctions.GetBooleanValue(!Instance["global"].ContainsKey(kkey) ? "" : Instance["global"][kkey].Value);
            }
        }
        public static bool EnableAppLogSqlite
        {
            get
            {
                var kkey = "EnableAppLog_Sqlite".ToLower();
                return CommonFunctions.GetBooleanValue(GetValueFromGlobal(kkey));
                //return CommonFunctions.GetBooleanValue(!Instance["global"].ContainsKey(kkey) ? "" : Instance["global"][kkey].Value);
            }
        }
        public static bool EnableAppLogWeb
        {
            get
            {
                var kkey = "EnableAppLog_Web".ToLower();
                return CommonFunctions.GetBooleanValue(GetValueFromGlobal(kkey));
                //return CommonFunctions.GetBooleanValue(!Instance["global"].ContainsKey(kkey) ? "" : Instance["global"][kkey].Value);
            }
        }
        public static bool EnableAppLogAppcenter
        {
            get
            {
                var kkey = "EnableAppLog_Appcenter".ToLower();
                return CommonFunctions.GetBooleanValue(GetValueFromGlobal(kkey));
                //return CommonFunctions.GetBooleanValue(!Instance["global"].ContainsKey(kkey) ? "" : Instance["global"][kkey].Value);
            }
        }
        public static bool EnableAppLogDiagnostics
        {
            get
            {
                var kkey = "EnableAppLog_Diagnostic".ToLower();
                return CommonFunctions.GetBooleanValue(GetValueFromGlobal(kkey));
                //return CommonFunctions.GetBooleanValue(!Instance["global"].ContainsKey(kkey) ? "" : Instance["global"][kkey].Value);
            }
        }
        public static bool EnableAppLogTrace
        {
            get
            {
                var kkey = "EnableAppLog_Trace".ToLower();
                return CommonFunctions.GetBooleanValue(GetValueFromGlobal(kkey));
                //return CommonFunctions.GetBooleanValue(!Instance["global"].ContainsKey(kkey) ? "" : Instance["global"][kkey].Value);
            }
        }
        public static bool Collection_Branch_Name_Enable
        {
            get
            {
                var kkey = "Collection_Branch_Name_Enable".ToLower();
                return CommonFunctions.GetBooleanValue(GetValueFromGlobal(kkey));
                //return CommonFunctions.GetBooleanValue(!Instance["global"].ContainsKey(kkey) ? "" : Instance["global"][kkey].Value);
            }
        }
        public static int CameraWidthInPixel { get; set; }
        public static int CameraHeightInPixel { get; set; }
    }
}

using Newtonsoft.Json;
using System.Data;
using System.Text;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Classes;

public class SqliteSKUDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, ISKUDL
{
    private readonly JsonSerializerSettings _jsonSerializerSettings;
    public SqliteSKUDL(IServiceProvider serviceProvider, JsonSerializerSettings jsonSerializerSettings) :
        base(serviceProvider)
    {
        _jsonSerializerSettings = jsonSerializerSettings;
    }
    public async Task<int> CRUDWinitCache(string key, string value, IDbConnection? connection = null,
        IDbTransaction? transaction = null)
    {
        int retValue;
        try
        {
            int exists = await IsExistsWinitCache(key);
            retValue = exists > 0 ? await UpdateWinitCache(key, value) : await CreateWinitCache(key, value);
        }
        catch
        {
            throw;
        }
        return retValue;
    }
    public async Task<int> CreateWinitCache(string key, string value, IDbConnection? connection = null,
        IDbTransaction? transaction = null)
    {
        int retValue;
        try
        {
            string query = @"
                                INSERT INTO winit_cache (key, value)
                                VALUES
                                (@Key, @Value);";


            Dictionary<string, object?> salesOrderParameters = new()
            {
                { "Key", key },
                { "Value", value }
            };
            retValue = await ExecuteNonQueryAsync(query, salesOrderParameters, connection, transaction);
        }
        catch (Exception)
        {
            throw;
        }
        return retValue;
    }
    public async Task<int> UpdateWinitCache(string key, string value,
    IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        int retValue;
        try
        {
            string query = @"UPDATE winit_cache SET value = @Value WHERE key = @Key;";

            Dictionary<string, object?> salesOrderParameters = new()
            {
                { "Key", key },
                { "Value", value }
            };
            retValue = await ExecuteNonQueryAsync(query, salesOrderParameters, connection, transaction);
        }
        catch (Exception)
        {
            throw;
        }
        return retValue;
    }
    public async Task<int> IsExistsWinitCache(string key,
    IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        int retValue;
        try
        {
            string query = @"SELECT count(*) FROM winit_cache WHERE key = @Key;";

            var salesOrderParameters = new
            {
                Key = key
            };
            retValue = await ExecuteScalarAsync<int>(query, salesOrderParameters);
        }
        catch (Exception)
        {
            throw;
        }
        return retValue;
    }
    public async Task<List<ISKUMaster>> GetWinitCache(string key,
    IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        List<ISKUMaster> retValue;
        try
        {
            string query = @"SELECT value FROM winit_cache WHERE key = @Key;";

            var salesOrderParameters = new
            {
                Key = key 
            };
            string data  = await ExecuteSingleAsync<string>(query, salesOrderParameters, null, connection);
            retValue = JsonConvert.DeserializeObject<List<ISKUMaster>>(data,_jsonSerializerSettings);
        }
        catch (Exception)
        {
            throw;
        }
        return retValue;
    }
    public async Task<(List<Model.Interfaces.ISKU>, List<Model.Interfaces.ISKUConfig>, List<Model.Interfaces.ISKUUOM>,
        List<Model.Interfaces.ISKUAttributes>, List<Model.Interfaces.ITaxSkuMap>)>
        PrepareSKUMaster(List<string> orgUIDs, List<string> DistributionChannelUIDs, List<string> skuUIDs, List<string> attributeTypes)
    {
        try
        {
            Dictionary<string, object?> skuParameters = [];
            StringBuilder skuSql = new(@"SELECT S.id AS Id, S.uid AS UID, S.created_by AS CreatedBy, S.created_time AS CreatedTime, S.modified_by AS ModifiedBy,
                                                S.modified_time AS ModifiedTime, S.server_add_time AS ServerAddTime, S.server_modified_time AS ServerModifiedTime,
                                                S.company_uid AS CompanyUID, S.org_uid AS OrgUID, S.code AS Code, S.name AS Name, S.arabic_name AS ArabicName,
                                                S.alias_name AS AliasName, S.long_name AS LongName, S.base_uom AS BaseUOM, S.outer_uom AS OuterUOM, S.from_date AS FromDate,
                                                S.to_date AS ToDate, S.is_stockable AS IsStockable, S.parent_uid AS ParentUID, S.is_active AS IsActive, 
                                                S.is_third_party As IsThirdParty, S.supplier_org_uid AS SupplierOrgUID,
                                                FSC.relative_path || '/' || FSC.file_name AS CatalogueURL,
                                                FSC.relative_path || '/' || FSC.file_name AS SKUImage,
                                                CASE WHEN FO.sku_uid IS NOT NULL THEN 1 ELSE 0 END AS IsFocusSKU
                                                FROM sku S
                                                LEFT JOIN file_sys FS ON FS.linked_item_type = 'SKU' AND FS.linked_item_uid = S.uid
                                                AND FS.file_sys_type = 'Image' AND FS.is_default = 1
                                                LEFT JOIN file_sys FSC ON FSC.linked_item_type = 'SKU' AND FSC.linked_item_uid = S.uid
                                                AND FSC.file_sys_type = 'SKU'
                                                LEFT JOIN (
                                                    SELECT DISTINCT SCGI.sku_uid 
                                                    FROM sku_class_group SCG
                                                    INNER JOIN sku_class_group_items SCGI ON SCGI.sku_class_group_uid = SCG.uid
                                                    AND SCG.sku_class_uid = 'FOCUS'
                                                ) FO ON FO.sku_uid = S.uid
                                                WHERE is_active = 1");
            if (orgUIDs != null && orgUIDs.Any())
            {
                _ = skuSql.Append($" AND org_uid IN @ORGUIDs");
                skuParameters.Add("ORGUIDs", orgUIDs);
            }
            if (skuUIDs != null && skuUIDs.Any())
            {
                _ = skuSql.Append($" AND S.UID IN @SKUUIDs");
                skuParameters.Add("SKUUIDs", skuUIDs);
            }
            //List<Model.Interfaces.ISKU> skuList = await ExecuteQueryAsync<Model.Interfaces.ISKU>(skuSql.ToString(), skuParameters);

            Dictionary<string, object?> skuConfigParameters = new();
            StringBuilder skuConfig = new(@"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime,
                                                    server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, org_uid AS OrgUID, distribution_channel_org_uid AS DistributionChannelOrgUID, 
                                                    sku_uid AS SKUUID, can_buy AS CanBuy, can_sell AS CanSell, buying_uom AS BuyingUOM, selling_uom AS SellingUOM, is_active AS IsActive 
                                                    FROM sku_config WHERE is_active = true");

            if (orgUIDs != null && orgUIDs.Any())
            {
                _ = skuConfig.Append($" AND org_uid IN @ORGUIDs");
                skuConfigParameters.Add("ORGUIDs", orgUIDs);
            }
            if (DistributionChannelUIDs != null && DistributionChannelUIDs.Any())
            {
                _ = skuConfig.Append($" AND distribution_channel_org_uid IN @DistributionChannelUIDs");
                skuConfigParameters.Add("DistributionChannelUIDs", DistributionChannelUIDs);
            }
            if (skuUIDs != null && skuUIDs.Any())
            {
                _ = skuConfig.Append($" AND sku_uid IN @SKUUIDs");
                skuConfigParameters.Add("SKUUIDs", skuUIDs);
            }
            //List<Model.Interfaces.ISKUConfig> skuConfigList = await ExecuteQueryAsync<Model.Interfaces.ISKUConfig>(skuConfig.ToString(), skuConfigParameters);

            Dictionary<string, object?> skuUOMParameters = new();
            StringBuilder skuUomSql = new(@"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                                                    server_modified_time AS ServerModifiedTime, sku_uid AS SKUUID, code AS Code, name AS Name, label AS Label, barcodes AS Barcodes, is_base_uom AS IsBaseUOM, 
                                                    is_outer_uom AS IsOuterUOM, multiplier AS Multiplier, length AS Length, depth AS Depth, width AS Width, height AS Height, volume AS Volume, weight AS Weight, 
                                                    gross_weight AS GrossWeight, dimension_unit AS DimensionUnit, volume_unit AS VolumeUnit, weight_unit AS WeightUnit, gross_weight_unit AS GrossWeightUnit, 
                                                    liter AS Liter, kgm AS KGM FROM sku_uom WHERE 1=1");
            if (skuUIDs != null && skuUIDs.Any())
            {
                _ = skuUomSql.Append($" AND sku_uid IN @SKUUIDs;");
                skuUOMParameters.Add("SKUUIDs", skuUIDs);
            }

            //List<Model.Interfaces.ISKUUOM> skuUomList = await ExecuteQueryAsync<Model.Interfaces.ISKUUOM>(skuUomSql.ToString(), skuUOMParameters);


            Dictionary<string, object?> skuAttributesparameters = new();

            StringBuilder skuAtrributesSql = new(@"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, 
                                                            modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, 
                                                            sku_uid AS SKUUID, type AS Type, code AS Code, value AS Value, parent_type AS ParentType 
                                                            FROM sku_attributes WHERE 1=1 ");

            if (skuUIDs != null && skuUIDs.Any())
            {
                _ = skuAtrributesSql.Append($" AND sku_uid IN @SKUUIDs");
                skuAttributesparameters.Add("SKUUIDs", skuUIDs);
            }

            if (attributeTypes != null && attributeTypes.Any())
            {
                _ = skuAtrributesSql.Append($" AND type IN @attributeTypes;");
                skuAttributesparameters.Add("attributeTypes", attributeTypes);
            }
            //List<Model.Interfaces.ISKUAttributes> sKUAttributesList = await ExecuteQueryAsync<Model.Interfaces.ISKUAttributes>(skuAtrributesSql.ToString(), skuAttributesparameters);

            Dictionary<string, object?> taxSkuMapparameters = new();

            StringBuilder taxSkuMapSql = new(@"SELECT DISTINCT TSM.sku_uid AS SKUUID, TSM.tax_uid AS TaxUID 
                                    FROM org O 
                                    INNER JOIN tax_group_taxes TGT ON TGT.tax_group_uid = O.tax_group_uid 
                                    INNER JOIN tax T ON T.[uid] = TGT.tax_uid AND T.applicable_at = 'Item'
                                    INNER JOIN tax_sku_map TSM ON TSM.tax_uid = TGT.tax_uid 
                                    WHERE 1=1 ");
            if (orgUIDs != null && orgUIDs.Any())
            {
                _ = taxSkuMapSql.Append($" AND O.[uid] IN @ORGUIDs");
                taxSkuMapparameters.Add("ORGUIDs", orgUIDs);
            }
            if (skuUIDs != null && skuUIDs.Any())
            {
                _ = taxSkuMapSql.Append($" AND sku_uid IN @SKUUIDs");
                taxSkuMapparameters.Add("SKUUIDs", skuUIDs);
            }
            //List<Model.Interfaces.ITaxSkuMap> taxSkuMapList = await ExecuteQueryAsync<Model.Interfaces.ITaxSkuMap>(taxSkuMapSql.ToString(), taxSkuMapparameters);

            Task<List<Model.Interfaces.ISKU>> skuListTask;
            Task<List<Model.Interfaces.ISKUConfig>> skuConfigListTask;
            Task<List<Model.Interfaces.ISKUUOM>> skuUomListTask;
            Task<List<Model.Interfaces.ISKUAttributes>> sKUAttributesListTask;
            Task<List<Model.Interfaces.ITaxSkuMap>> taxSkuMapListTask;

            skuListTask = Task.Run(() => ExecuteQueryAsync<Model.Interfaces.ISKU>(skuSql.ToString(), skuParameters));
            skuConfigListTask = Task.Run(() => ExecuteQueryAsync<Model.Interfaces.ISKUConfig>(skuConfig.ToString(), skuConfigParameters));
            skuUomListTask = Task.Run(() => ExecuteQueryAsync<Model.Interfaces.ISKUUOM>(skuUomSql.ToString(), skuUOMParameters));
            sKUAttributesListTask = Task.Run(() => ExecuteQueryAsync<Model.Interfaces.ISKUAttributes>(skuAtrributesSql.ToString(), skuAttributesparameters));
            taxSkuMapListTask = Task.Run(() => ExecuteQueryAsync<Model.Interfaces.ITaxSkuMap>(taxSkuMapSql.ToString(), taxSkuMapparameters));

            // Wait for all tasks to complete
            await Task.WhenAll(skuListTask, skuConfigListTask, skuUomListTask, sKUAttributesListTask, taxSkuMapListTask);

            return (await skuListTask, await skuConfigListTask, await skuUomListTask,
                await sKUAttributesListTask, await taxSkuMapListTask);
        }
        catch (Exception)
        {
            throw;
        }
    }
    /*
    public async Task<(List<Winit.Modules.Store.Model.Interfaces.IStore>, List<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo>,
        List<Winit.Modules.Store.Model.Interfaces.IStoreCredit>, List<Winit.Modules.Store.Model.Interfaces.IStoreAttributes>, List<Winit.Modules.Address.Model.Interfaces.IAddress>,
        List<Winit.Modules.Contact.Model.Interfaces.IContact>)> PrepareStoreMaster(List<string> storeUIDs)
    {
        try
        {
            string commaSeparatedstoreUIDs = null; 
            if (storeUIDs != null)
            {
                commaSeparatedstoreUIDs = string.Join(",", storeUIDs);
            }
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                { "UIDs", commaSeparatedstoreUIDs }
            };
            var storeSql = new StringBuilder(@"SELECT Id, UID, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime, ServerAddTime,
                                               ServerModifiedTime, CompanyUID, Code, Number, Name, AliasName, LegalName, Type,
                                               BillToStoreUID, ShipToStoreUID, SoldToStoreUID, Status, IsActive, StoreClass,
                                               StoreRating, IsBlocked, BlockedReasonCode, BlockedReasonDescription, CreatedByEmpUID,
                                               CreatedByJobPositionUID, CountryUID, RegionUID, CityUID, Source, OutletName, BlockedByEmpUID,
                                               ArabicName, IsTaxApplicable, TaxDocNumber, SchoolWarehouse, DayType, SpecialDay, IsTaxDocVerified,
                                               StoreSize, ProspectEmpUID, TaxKeyField, StoreImage, IsVATQRCaptureMandatory, TaxType FROM Store;");
            if (!string.IsNullOrEmpty(commaSeparatedstoreUIDs))
            {

                storeSql.Append($" WHERE UID IN ('{commaSeparatedstoreUIDs}');");
            }

            var storeParameters = new Dictionary<string, object?>();
            Type storeType = _serviceProvider.GetRequiredService<Winit.Modules.Store.Model.Interfaces.IStore>().GetType();
            List<Winit.Modules.Store.Model.Interfaces.IStore> storeList = await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStore>(storeSql.ToString(), parameters, storeType);

            var storeAdditionalInfoSql = new StringBuilder(@"SELECT Id, UID, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime, ServerAddTime, ServerModifiedTime, StoreUID, OrderType, IsPromotionsBlock, 
                                                        CustomerStartDate, CustomerEndDate, PurchaseOrderNumber, DeliveryDocketIsPurchaseOrderRequired, IsWithPrintedInvoices, IsCaptureSignatureRequired, 
                                                        IsAlwaysPrinted, BuildingDeliveryCode, DeliveryInformation, IsStopDelivery, IsForeCastTopUpQty, IsTemperatureCheck, InvoiceStartDate, InvoiceEndDate,
                                                        InvoiceFormat, InvoiceDeliveryMethod, DisplayDeliveryDocket, DisplayPrice, ShowCustPO, InvoiceText, InvoiceFrequency, StockCreditIsPurchaseOrderRequired,
                                                        AdminFeePerBillingCycle, AdminFeePerDelivery, LatePaymentFee, Drawer, BankUID, BankAccount, MandatoryPONumber, IsStoreCreditCaptureSignatureRequired, 
                                                        StoreCreditAlwaysPrinted, IsDummyCustomer, DefaultRun, IsFOCCustomer, RSSShowPrice, RSSShowPayment, RSSShowCredit, RSSShowInvoice, RSSIsActive,
                                                        RSSDeliveryInstructionStatus, RSSTimeSpentOnRSSPortal, RSSOrderPlacedInRSS, RSSAvgOrdersPerWeek, RSSTotalOrderValue, AllowForceCheckIn, IsManaualEditAllowed, 
                                                        CanUpdateLatLong, AllowGoodReturn, AllowBadReturn, EnableAsset, EnableSurvey, AllowReplacement, IsInvoiceCancellationAllowed, IsDeliveryNoteRequired, 
                                                        EInvoicingEnabled, ImageRecognizationEnabled, MaxOutstandingInvoices, NegativeInvoiceAllowed, DeliveryMode, VisitFrequency, ShippingContactSameAsStore, 
                                                        BillingAddressSameAsShipping, PaymentMode, PriceType, AverageMonthlyIncome, DefaultBankUID, AccountNumber, NoOfCashCounters, CustomField1, CustomField2,
                                                        CustomField3, CustomField4, CustomField5, CustomField6, CustomField7, CustomField8, CustomField9, CustomField10, IsAssetEnabled, IsSurveyEnabled,
                                                        AllowGoodReturns, AllowBadReturns, AllowReturnAgainstInvoice, AllowReturnWithSalesOrder, WeekOffSun, WeekOffMon, WeekOffTue, WeekOffWed, WeekOffThu, WeekOffFri, WeekOffSat
                                                        FROM StoreAdditionalInfo;");
            if (!string.IsNullOrEmpty(commaSeparatedstoreUIDs))
            {
                storeAdditionalInfoSql.Append($" WHERE StoreUID IN ('{commaSeparatedstoreUIDs}');");


            }
            var storeAdditionalInfoParameters = new Dictionary<string, object?>();
            Type storeAdditionalInfoType = _serviceProvider.GetRequiredService<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo>().GetType();
            List<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo> storeAdditionalInfoList = await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo>(storeAdditionalInfoSql.ToString(), parameters, storeAdditionalInfoType);

            var storeCreditSql = new StringBuilder(@"SELECT Id, UID, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime, ServerAddTime, ServerModifiedTime, StoreUID, 
                                                   PaymentTermUID, CreditType, CreditLimit, TemporaryCredit, OrgUID, DistributionChannelUID, PreferredPaymentMode, IsActive, 
                                                   IsBlocked, BlockingReasonCode, BlockingReasonDescription FROM StoreCredit;");
            if (!string.IsNullOrEmpty(commaSeparatedstoreUIDs))
            {
                storeCreditSql.Append(@" WHERE StoreUID IN (SELECT value FROM STRING_SPLIT(@UIDs, ','));");

                storeCreditSql.Append($" WHERE StoreUID IN ('{commaSeparatedstoreUIDs}');");

            }
            var storeCreditParameters = new Dictionary<string, object?>();
            Type storeCreditType = _serviceProvider.GetRequiredService<Winit.Modules.Store.Model.Interfaces.IStoreCredit>().GetType();
            List<Winit.Modules.Store.Model.Interfaces.IStoreCredit> storeCreditList = await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStoreCredit>(storeCreditSql.ToString(), parameters, storeCreditType);


            var storeAttributesSql = new StringBuilder(@"SELECT Id, UID, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime, ServerAddTime, ServerModifiedTime,
                                                        CompanyUID, OrgUID, DistributionChannelUID, StoreUID, Name, Code, Value, ParentName FROM StoreAttributes;");

            if (!string.IsNullOrEmpty(commaSeparatedstoreUIDs))
            {

                storeAttributesSql.Append($" WHERE StoreUID IN ('{commaSeparatedstoreUIDs}');");
             //   storeAttributesSql.Append(@" WHERE StoreUID IN (SELECT value FROM STRING_SPLIT(@UIDs, ','));");
            }
            var storeAttributesParameters = new Dictionary<string, object?>();
            Type storeAttributesType = _serviceProvider.GetRequiredService<Winit.Modules.Store.Model.Interfaces.IStoreAttributes>().GetType();
            List<Winit.Modules.Store.Model.Interfaces.IStoreAttributes> storeAttributesList = await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStoreAttributes>(storeAttributesSql.ToString(), parameters, storeAttributesType);

            var addressSql = new StringBuilder(@"SELECT Id, UID, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime, ServerAddTime, ServerModifiedTime, Type, Name, Line1, Line2, Line3,
                            Landmark, Area, SubArea, ZipCode, City, CountryCode, RegionCode, Phone, PhoneExtension, Mobile1, Mobile2, Email, Fax, Latitude, Longitude, Altitude, LinkedItemUID, LinkedItemType, Status,
                            StateCode, TerritoryCode, PAN, AADHAR, SSN, IsEditable, IsDefault, Line4, Info, Depot FROM Address;");
            if (!string.IsNullOrEmpty(commaSeparatedstoreUIDs))
            {
                addressSql.Append($" WHERE StoreUID IN ('{commaSeparatedstoreUIDs}');");
            }

            var addressParameters = new Dictionary<string, object?>();
            Type addressType = _serviceProvider.GetRequiredService<Winit.Modules.Address.Model.Interfaces.IAddress>().GetType();
            List<Winit.Modules.Address.Model.Interfaces.IAddress> addressList = await ExecuteQueryAsync<Winit.Modules.Address.Model.Interfaces.IAddress>(addressSql.ToString(), parameters, addressType);

            var contactSql = new StringBuilder(@"SELECT Id, UID, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime, ServerAddTime, ServerModifiedTime, Title, Name, Phone, 
                                     PhoneExtension, Description, Designation, Mobile, Email, Email2, Email3, InvoiceForEmail1, InvoiceForEmail2, InvoiceForEmail3, Fax, LinkedItemUID, LinkedItemType, IsDefault, 
                                     IsEditable, EnabledForInvoiceEmail, EnabledForDocketEmail, EnabledForPromoEmail, IsEmailCC, Mobile2 FROM Contact;");

            if (!string.IsNullOrEmpty(commaSeparatedstoreUIDs))
            {
                contactSql.Append($" WHERE StoreUID IN ('{commaSeparatedstoreUIDs}');");
            }
            var contactParameters = new Dictionary<string, object?>();
            Type contactType = _serviceProvider.GetRequiredService<Winit.Modules.Contact.Model.Interfaces.IContact>().GetType();
            List<Winit.Modules.Contact.Model.Interfaces.IContact> contactList = await ExecuteQueryAsync<Winit.Modules.Contact.Model.Interfaces.IContact>(contactSql.ToString(), parameters, contactType);

            return (storeList, storeAdditionalInfoList, storeCreditList, storeAttributesList, addressList, contactList);
        }
        catch (Exception)
        {
            throw;
        }
    }


    public async Task<List<Winit.Modules.OrgCurrency.Model.Interfaces.IOrgCurrency>> PrepareOrgCurrencyMaster()
    {
        try
        {
            var orgCurrencySql = new StringBuilder(@"SELECT Id, OrgUID, CurrencyUID, IsPrimary, SS, CreatedTime, ModifiedTime, ServerAddTime, ServerModifiedTime FROM OrgCurrency;");
            var orgCurrencyParameters = new Dictionary<string, object?>();
            Type orgCurrencyType = _serviceProvider.GetRequiredService<Winit.Modules.OrgCurrency.Model.Interfaces.IOrgCurrency>().GetType();
            List<Winit.Modules.OrgCurrency.Model.Interfaces.IOrgCurrency> orgCurrencyList = await ExecuteQueryAsync<Winit.Modules.OrgCurrency.Model.Interfaces.IOrgCurrency>(orgCurrencySql.ToString(), orgCurrencyParameters, orgCurrencyType);
            return (orgCurrencyList);
        }
        catch (Exception)
        {
            throw;
        }
    }
    */

    public Task<PagedResponse<ISKU>> SelectAllSKUDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        throw new NotImplementedException();
    }

    public Task<ISKU> SelectSKUByUID(string UID)
    {
        throw new NotImplementedException();
    }

    public Task<int> CreateSKU(ISKUV1 sKU)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateSKU(ISKU sKU)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteSKU(string UID)
    {
        throw new NotImplementedException();
    }




    public Task<PagedResponse<ISKUListView>> SelectAllSKUDetailsWebView(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        throw new NotImplementedException();
    }

    public Task<(List<ISKU>, List<ISKUConfig>, List<ISKUUOM>, List<ISKUAttributes>, List<Winit.Modules.CustomSKUField.Model.Interfaces.ICustomSKUFields>, List<Winit.Modules.FileSys.Model.Interfaces.IFileSys>)> SelectSKUMasterByUID(string UID)
    {
        throw new NotImplementedException();
    }

    public Task<Dictionary<string, List<string>>> GetLinkedItemUIDByStore(string linkedItemType, List<string> storeUIDs)
    {
        throw new NotImplementedException();
    }
}

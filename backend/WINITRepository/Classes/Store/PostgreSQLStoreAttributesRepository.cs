using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models.Store;

namespace WINITRepository.Classes.StoreAttributess
{
    public class PostgreSQLStoreAttributesRepository : Interfaces.StoreAttributess.IStoreAttributesRepository
    {
        private readonly string _connectionString;
        public PostgreSQLStoreAttributesRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("PostgreSQL");
        }
        public async Task<IEnumerable<StoreAttributes>> SelectStoreAttributesByName(string attributeName)
        {
            DBManager.PostgresDBManager<StoreAttributes> dbManager = new DBManager.PostgresDBManager<StoreAttributes>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"name",  attributeName}
            };

            //  var sql = @"SELECT * FROM Customer WHERE ""UID"" = @UID";
            var sql = @"SELECT * FROM StoreAttributes WHERE ""name"" = @name";

            IEnumerable<StoreAttributes> storeAttributesList = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(storeAttributesList);
        }
        //public async Task<IEnumerable<StoreAttributes>> SelectAllStoreAttributes()
        //{
        //    DBManager.PostgresDBManager<StoreAttributes> dbManager = new DBManager.PostgresDBManager<StoreAttributes>(_connectionString);

        //    Dictionary<string, object> parameters = new Dictionary<string, object>
        //    {
        //    };

        //    var sql = "SELECT * FROM StoreAttributes";

        //    IEnumerable<StoreAttributes> storeAttributesList = await dbManager.ExecuteQueryAsync(sql, parameters);
        //    return await Task.FromResult(storeAttributesList);
        //}
     


        public async Task<IEnumerable<StoreAttributes>> SelectAllStoreAttributes(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
    int pageSize, List<FilterCriteria> filterCriterias)
        {
            try
            {
                DBManager.PostgresDBManager<StoreAttributes> dbManager = new DBManager.PostgresDBManager<StoreAttributes>(_connectionString);

                var sql = new StringBuilder("SELECT * FROM StoreAttributes");
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    sql.Append(" WHERE ");
                    dbManager.AppendFilterCriteria(filterCriterias, sql, parameters);
                }

                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    dbManager.AppendSortCriteria(sortCriterias, sql);
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }


                IEnumerable<StoreAttributes> storeAttributesList = await dbManager.ExecuteQueryAsync(sql.ToString(), parameters);

                return storeAttributesList;
            }
            catch (Exception ex)
            {
                throw;
            }


        }


        public async Task<StoreAttributes> SelectStoreAttributesByStoreUID(string storeUID)
        {
            DBManager.PostgresDBManager<StoreAttributes> dbManager = new DBManager.PostgresDBManager<StoreAttributes>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"storeUID",  storeUID}
            };

            var sql = @"SELECT * FROM StoreAttributes WHERE ""storeuid"" = @storeUID";

            StoreAttributes StoreAttributesList = await dbManager.ExecuteSingleAsync(sql, parameters);
            return StoreAttributesList;
        }

        public async Task<StoreAttributes> CreateStoreAttributes(StoreAttributes storeAttributes)
        {
            DBManager.PostgresDBManager<StoreAttributes> dbManager = new DBManager.PostgresDBManager<StoreAttributes>(_connectionString);
            try
            {
               
                //                var sql = @"INSERT INTO StoreAttributes (""UID"", ""OrgUID"", ""DistributionChannelUID"", ""StoreUID"", ""Name"", ""Code"", ""Value"",
                //""ParentName"", ""SS"", ""CreatedTime"", ""ModifiedTime"", ""ServerAddTime"", ""ServerModifiedTime"")
                //VALUES (@UID, @OrgUID, @DistributionChannelUID, @StoreUID, @Name, @Code, @Value, @ParentName, @SS, @CreatedDate, @ModifiedTime, @ServerAddTime, @ServerModifiedTime)";

                var sql = @"INSERT INTO StoreAttributes (uid, orguid,companyuid, distributionchanneluid, storeuid, name, code, value,
parentname, createdtime, modifiedtime, serveraddtime, servermodifiedtime)
VALUES (@UID,@CompanyUID, @OrgUID, @DistributionChannelUID, @StoreUID, @Name, @Code, @Value, @ParentName, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
{
   // {"UID", storeAttributes.UID},
    {"CompanyUID", storeAttributes.CompanyUID},
    {"OrgUID", storeAttributes.OrgUID},
    {"DistributionChannelUID", storeAttributes.DistributionChannelUID},
    {"StoreUID", storeAttributes.StoreUID},
    {"Name", storeAttributes.Name},
    {"Code", storeAttributes.Code},
    {"Value", storeAttributes.Value},
    {"ParentName", storeAttributes.ParentName},
    
    {"CreatedTime", storeAttributes.CreatedTime}, 
    {"ModifiedTime", storeAttributes.ModifiedTime},
    {"ServerAddTime", storeAttributes.ServerAddTime},
    {"ServerModifiedTime", storeAttributes.ServerModifiedTime},
};
                await dbManager.ExecuteNonQueryAsync(sql, parameters);
                return storeAttributes;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<int> UpdateStoreAttributes(StoreAttributes StoreAttributes)
        {
            try
            {
                DBManager.PostgresDBManager<StoreAttributes> dbManager = new DBManager.PostgresDBManager<StoreAttributes>(_connectionString);

                var sql = "UPDATE StoreAttributes SET   code = @Code, value = @Value,parentname = @ParentName,modifiedtime=@ModifiedTime," +
                    "servermodifiedtime=@ServerModifiedTime WHERE storeuid = @StoreUID;";

            

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                  //  {"UID", StoreAttributes.UID},
                  //  {"OrgUID", StoreAttributes.OrgUID},
                    //{"DistributionChannelUID", StoreAttributes.DistributionChannelUID},
                    {"StoreUID", StoreAttributes.StoreUID},
                    //{"CompanyUID", StoreAttributes.CompanyUID},
                  //  {"Name", StoreAttributes.Name},
                    {"Code", StoreAttributes.Code},
                    {"Value", StoreAttributes.Value},
                    {"ParentName", StoreAttributes.ParentName},
                    {"ModifiedTime", StoreAttributes.ModifiedTime},
                    {"ServerModifiedTime", StoreAttributes.ServerModifiedTime},
                };
                var updateDetails = await dbManager.ExecuteNonQueryAsync(sql, parameters);

                return updateDetails;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<int> DeleteStoreAttributes(string storeUID)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.Settings> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.Settings>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"storeUID",storeUID}
            };
            var sql = "DELETE  FROM StoreAttributes WHERE StoreUID = @storeUID";

            var status = await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return status;
        }
        public async Task<IEnumerable<StoreAttributes>> GetStoreAttributesFiltered(string Name, String Email)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<StoreAttributes>> GetStoreAttributesPaged(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<StoreAttributes>> GetStoreAttributesSorted(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias)
        {
            throw new NotImplementedException();
        }


        //store


        public async Task<IEnumerable<Store>> SelectAllStore(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
  int pageSize, List<FilterCriteria> filterCriterias)
        {
            try
            {
                DBManager.PostgresDBManager<Store> dbManager = new DBManager.PostgresDBManager<Store>(_connectionString);

                var sql = new StringBuilder("SELECT * FROM Store");
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    sql.Append(" WHERE ");
                    dbManager.AppendFilterCriteria(filterCriterias, sql, parameters);
                }

                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    dbManager.AppendSortCriteria(sortCriterias, sql);
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                IEnumerable<Store> storeList = await dbManager.ExecuteQueryAsync(sql.ToString(), parameters);

                return storeList;
            }
            catch (Exception ex)
            {
                throw;
            }


        }

        public async Task<Store> SelectStoreByUID(string UID)
        {
            DBManager.PostgresDBManager<Store> dbManager = new DBManager.PostgresDBManager<Store>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };

            var sql = @"SELECT * FROM Store WHERE ""uid"" = @UID";

            Store StoreList = await dbManager.ExecuteSingleAsync(sql, parameters);
            return StoreList;
        }
        public async Task<Store> CreateStore(Store store)
        {
            DBManager.PostgresDBManager<Store> dbManager = new DBManager.PostgresDBManager<Store>(_connectionString);
            try
            {

               

                var sql = @"INSERT INTO Store (uid, companyuid, code, number, aliasname, type, billtostoreuid,
shiptostoreuid, soldtostoreuid, status, isactive, storeclass,storerating,isblocked,blockedreasoncode,
blockedreasondescription,createdbyempuid,createdbyjobpositionuid,countryuid,regionuid,cityuid,source,createdtime,
modifiedtime,serveraddtime,servermodifiedtime)VALUES (@UID, @CompanyUID, @Code, @Number, @AliasName, @Type, 
@BillToStoreUID,@ShipToStoreUID, @SoldToStoreUID, @Status, @IsActive, @StoreClass, @StoreRating, @IsBlocked,@BlockedReasonCode, 
@BlockedReasonDescription, @CreatedByEmpUID,@CreatedByJobPositionUID,@CountryUID,@RegionUID,@CityUID,@Source @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime)";


                Dictionary<string, object> parameters = new Dictionary<string, object>
{

    
   // {"UID", store.UID},
    {"CompanyUID", store.CompanyUID},
    {"Code", store.Code},
    {"Number", store.Number},
    {"Name", store.Name},
    {"AliasName", store.AliasName},
    {"Type", store.Type},
    {"BillToStoreUID", store.BillToStoreUID},
    {"ShipToStoreUID", store.ShipToStoreUID},
    {"SoldToStoreUID", store.SoldToStoreUID},
   // {"PayerStoreUID", store.PayerStoreUID},
    {"Status", store.Status},
    {"IsActive", store.IsActive},
    {"StoreClass", store.StoreClass},
    {"StoreRating", store.StoreRating},
   // {"OrgUID", store.OrgUID},
    {"IsBlocked", store.IsBlocked},
    {"BlockedReasonCode", store.BlockedReasonCode},
    {"BlockedReasonDescription", store.BlockedReasonDescription},
    {"CreatedByEmpUID", store.CreatedByEmpUID},
    {"CreatedByJobPositionUID", store.CreatedByJobPositionUID},
    {"CountryUID", store.CountryUID},
    {"RegionUID", store.RegionUID},
    {"CityUID", store.CityUID},
    {"Source", store.Source},
    //{"LocationUID", store.LocationUID},
    //{"IsCreatedFromApp", store.IsCreatedFromApp},
    //{"PictureStatus", store.PictureStatus},
    //{"OrderProfile", store.OrderProfile},
    //{"FRWHOrgUID", store.FRWHOrgUID},
    //{"SS", store.SS},
    {"CreatedTime", store.CreatedTime},
    {"ModifiedTime", store.ModifiedTime},
    {"ServerAddTime", store.ServerAddTime},
    {"ServerModifiedTime", store.ServerModifiedTime},
};
                await dbManager.ExecuteNonQueryAsync(sql, parameters);
                return store;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> UpdateStore(Store Store)
        {
            try
            {
                DBManager.PostgresDBManager<Store> dbManager = new DBManager.PostgresDBManager<Store>(_connectionString);

               

                var sql = "UPDATE Store SET name = @Name, aliasname = @AliasName, type = @Type," +
          " billtostoreuid = @BillToStoreUID, shiptostoreuid = @ShipToStoreUID, soldtostoreuid = @SoldToStoreUID,  status = @Status, isactive = @IsActive, " +
          "storeclass = @StoreClass, storerating = @StoreRating, isblocked = @IsBlocked, blockedreasoncode = @BlockedReasonCode, blockedreasondescription = @BlockedReasonDescription, " +
          "createdbyempuid = @CreatedByEmpUID,createdbyjobpositionuid=@CreatedByJobPositionUID,countryuid=@CountryUID,cityuid=@CityUID,source=@Source, " +
          "modifiedtime = @ModifiedTime, servermodifiedtime = @ServerModifiedTime WHERE uid = @uid;";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                  // {"UID", Store.UID},
                 //  {"Code", Store.Code},
                   {"Number", Store.Number},
                   {"Name", Store.Name},
                   {"AliasName", Store.AliasName},
                   {"BillToStoreUID", Store.BillToStoreUID},
                   {"ShipToStoreUID", Store.ShipToStoreUID},
                   {"SoldToStoreUID", Store.SoldToStoreUID},
                   {"Status", Store.Status},
                   {"IsActive", Store.IsActive},
                   {"StoreClass", Store.StoreClass},
                   {"StoreRating", Store.StoreRating},
                   {"IsBlocked", Store.IsBlocked},
                   {"BlockedReasonCode", Store.BlockedReasonCode},
                   {"BlockedReasonDescription", Store.BlockedReasonDescription},
                   {"CreatedByEmpUID", Store.CreatedByEmpUID},
                   {"CreatedByJobPositionUID", Store.CreatedByJobPositionUID},
                   {"CountryUID", Store.CountryUID},
                   {"CityUID", Store.CityUID},
                   {"Source", Store.Source},
                   {"ModifiedTime", Store.ModifiedTime},
                   {"ServerModifiedTime", Store.ServerModifiedTime},
                  
                };
                var updateDetails = await dbManager.ExecuteNonQueryAsync(sql, parameters);

                return updateDetails;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<int> DeleteStore(string UID)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.Store.Store> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.Store.Store>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",UID}
            };
            var sql = "DELETE  FROM Store WHERE UID = @UID";

            var status = await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return status;
        }



        //store


        public async Task<IEnumerable<StoreAdditionalInfo>> SelectAllStoreAdditionalInfo(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
  int pageSize, List<FilterCriteria> filterCriterias)
        {
            try
            {
                DBManager.PostgresDBManager<StoreAdditionalInfo> dbManager = new DBManager.PostgresDBManager<StoreAdditionalInfo>(_connectionString);

                var sql = new StringBuilder("SELECT * FROM StoreAdditionalInfo");
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    sql.Append(" WHERE ");
                    dbManager.AppendFilterCriteria(filterCriterias, sql, parameters);
                }

                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    dbManager.AppendSortCriteria(sortCriterias, sql);
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                IEnumerable<StoreAdditionalInfo> storeList = await dbManager.ExecuteQueryAsync(sql.ToString(), parameters);

                return storeList;
            }
            catch (Exception ex)
            {
                throw;
            }


        }

        public async Task<StoreAdditionalInfo> SelectStoreAdditionalInfoByUID(string storeUID)
        {
            DBManager.PostgresDBManager<StoreAdditionalInfo> dbManager = new DBManager.PostgresDBManager<StoreAdditionalInfo>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"storeUID",  storeUID}
            };

            var sql = @"SELECT * FROM StoreAdditionalInfo WHERE ""storeuid"" = @storeUID";

            StoreAdditionalInfo StoreList = await dbManager.ExecuteSingleAsync(sql, parameters);
            return StoreList;
        }

        public async Task<StoreAdditionalInfo> CreateStoreAdditionalInfo(StoreAdditionalInfo storeAdditionalInfo)
        {
            DBManager.PostgresDBManager<StoreAdditionalInfo> dbManager = new DBManager.PostgresDBManager<StoreAdditionalInfo>(_connectionString);
            try
            {

                var sql = "INSERT INTO StoreAdditionalInfo (id, uid, storeuid, ordertype, ispromotionsblock, customerstartdate, customerenddate, isschoolcustomer," +
                    " purchaseordernumber, deliverydocketispurchaseorderrequired, iswithprintedinvoices, isalwaysprinted, buildingdeliverycode, deliveryinformation, " +
                    "isstopdelivery, isforecasttopupqty, istemperaturecheck, invoicestartdate, invoiceenddate, invoiceformat, invoicedeliverymethod, displaydeliverydocket, " +
                    "displayprice, showcustpo, invoicetext, invoicefrequency, stockcreditispurchaseorderrequired, adminfeeperbillingcycle, adminfeeperdelivery, latepayementfee, " +
                    "drawer, bankuid, bankaccount, mandatoryponumber, isstorecreditcapturesignaturerequired, storecreditalwaysprinted, isdummycustomer, defaultrun, " +
                    "prospectempuid, isfoccustomer, rssshowprice, rssshowpayment, rssshowcredit, rssshowinvoice, rssisactive, rssdeliveryinstructionstatus, " +
                    "rsstimespentonrssportal, rssorderplacedinrss, rssavgordersperweek, rsstotalordervalue, allowforcecheckin, ismanaualeditallowed, canupdatelatlong," +
                    " istaxapplicable, allowgoodreturn, allowbadreturn, enableasset, enablesurvey, allowreplacement, isinvoicecancellationallowed, isdeliverynoterequired," +
                    " einvoicingenabled, imagerecognizationenabled, maxoutstandinginvoices, negativeinvoiceallowed) " +
                    "VALUES (@Id, @UID, @StoreUID, @OrderType, @IsPromotionsBlock, @CustomerStartDate, @CustomerEndDate, @IsSchoolCustomer," +
                    " @PurchaseOrderNumber,@DeliveryDocketIsPurchaseOrderRequired, @IsWithPrintedInvoices, @IsAlwaysPrinted, @BuildingDeliveryCode," +
                    " @DeliveryInformation, @IsStopDelivery, @IsForeCastTopUpQty, @IsTemperatureCheck, @InvoiceStartDate," +
                    " @InvoiceEndDate, @InvoiceFormat, @InvoiceDeliveryMethod, @DisplayDeliveryDocket,@DisplayPrice, @ShowCustPO, @InvoiceText, " +
                    "@InvoiceFrequency, @StockCreditIsPurchaseOrderRequired, @AdminFeePerBillingCycle, @AdminFeePerDelivery,@LatePayementFee, " +
                    "@Drawer, @BankUID, @BankAccount, @MandatoryPONumber, @IsStoreCreditCaptureSignatureRequired, @StoreCreditAlwaysPrinted, @IsDummyCustomer," +
                    " @DefaultRun, @ProspectEmpUID, @IsFOCCustomer, @RSSShowPrice, @RSSShowPayment, @RSSShowCredit, @RSSShowInvoice, @RSSIsActive, " +
                    "@RSSDeliveryInstructionStatus,@RSSTimeSpentOnRSSPortal, @RSSOrderPlacedInRSS, @RSSAvgOrdersPerWeek, " +
                    "@RSSTotalOrderValue, @AllowForceCheckIn, @IsManaualEditAllowed, @CanUpdateLatLong, @IsTaxApplicable, @AllowGoodReturn, @AllowBadReturn, " +
                    "@EnableAsset, @EnableSurvey, @AllowReplacement, @IsInvoiceCancellationAllowed, @IsDeliveryNoteRequired, @EInvoicingEnabled, @ImageRecognizationEnabled," +
                    " @MaxOutstandingInvoices, @NegativeInvoiceAllowed) ";



                Dictionary<string, object> parameters = new Dictionary<string, object>
{
    //{"UID", storeAdditionalInfo.UID},
    {"StoreUID", storeAdditionalInfo.StoreUID},
    {"OrderType", storeAdditionalInfo.OrderType},
    {"IsPromotionsBlock", storeAdditionalInfo.IsPromotionsBlock},
    {"CustomerStartDate", storeAdditionalInfo.CustomerStartDate},
    {"CustomerEndDate", storeAdditionalInfo.CustomerEndDate},
    {"IsSchoolCustomer", storeAdditionalInfo.IsSchoolCustomer},
    {"PurchaseOrderNumber", storeAdditionalInfo.PurchaseOrderNumber},
    {"DeliveryDocketIsPurchaseOrderRequired", storeAdditionalInfo.DeliveryDocketIsPurchaseOrderRequired},
    {"IsWithPrintedInvoices", storeAdditionalInfo.IsWithPrintedInvoices},
    {"IsAlwaysPrinted", storeAdditionalInfo.IsAlwaysPrinted},
    {"BuildingDeliveryCode", storeAdditionalInfo.BuildingDeliveryCode},
    {"DeliveryInformation", storeAdditionalInfo.DeliveryInformation},
    {"IsStopDelivery", storeAdditionalInfo.IsStopDelivery},
    {"IsForeCastTopUpQty", storeAdditionalInfo.IsForeCastTopUpQty},
    {"IsTemperatureCheck", storeAdditionalInfo.IsTemperatureCheck},
    {"InvoiceStartDate", storeAdditionalInfo.InvoiceStartDate},
    {"InvoiceEndDate", storeAdditionalInfo.InvoiceEndDate},
    {"InvoiceFormat", storeAdditionalInfo.InvoiceFormat},
    {"InvoiceDeliveryMethod", storeAdditionalInfo.InvoiceDeliveryMethod},
    {"DisplayDeliveryDocket", storeAdditionalInfo.DisplayDeliveryDocket},
    {"DisplayPrice", storeAdditionalInfo.DisplayPrice},
    {"ShowCustPO", storeAdditionalInfo.ShowCustPO},
    {"InvoiceText", storeAdditionalInfo.InvoiceText},
    {"InvoiceFrequency", storeAdditionalInfo.InvoiceFrequency},
    {"StockCreditIsPurchaseOrderRequired", storeAdditionalInfo.StockCreditIsPurchaseOrderRequired},
    {"AdminFeePerBillingCycle", storeAdditionalInfo.AdminFeePerBillingCycle},
    {"AdminFeePerDelivery", storeAdditionalInfo.AdminFeePerDelivery},
    {"LatePayementFee", storeAdditionalInfo.LatePayementFee},
    {"Drawer", storeAdditionalInfo.Drawer},
    {"BankUID", storeAdditionalInfo.BankUID},
    {"BankAccount", storeAdditionalInfo.BankAccount},
    {"MandatoryPONumber", storeAdditionalInfo.MandatoryPONumber},
    {"IsStoreCreditCaptureSignatureRequired", storeAdditionalInfo.IsStoreCreditCaptureSignatureRequired},
    {"StoreCreditAlwaysPrinted", storeAdditionalInfo.StoreCreditAlwaysPrinted},
    {"IsDummyCustomer", storeAdditionalInfo.IsDummyCustomer},
    {"DefaultRun", storeAdditionalInfo.DefaultRun},
    {"ProspectEmpUID", storeAdditionalInfo.ProspectEmpUID},
    {"IsFOCCustomer", storeAdditionalInfo.IsFOCCustomer},
    {"RSSShowPrice", storeAdditionalInfo.RSSShowPrice},
    {"RSSShowPayment", storeAdditionalInfo.RSSShowPayment},
    {"RSSShowCredit", storeAdditionalInfo.RSSShowCredit},
    {"RSSShowInvoice", storeAdditionalInfo.RSSShowInvoice},
    {"RSSIsActive", storeAdditionalInfo.RSSIsActive},
    {"RSSDeliveryInstructionStatus", storeAdditionalInfo.RSSDeliveryInstructionStatus},
    {"RSSTimeSpentOnRSSPortal", storeAdditionalInfo.RSSTimeSpentOnRSSPortal},
    {"RSSOrderPlacedInRSS", storeAdditionalInfo.RSSOrderPlacedInRSS},
    {"RSSAvgOrdersPerWeek", storeAdditionalInfo.RSSAvgOrdersPerWeek},
    {"RSSTotalOrderValue", storeAdditionalInfo.RSSTotalOrderValue},
    {"AllowForceCheckIn", storeAdditionalInfo.AllowForceCheckIn},
    {"IsManaualEditAllowed", storeAdditionalInfo.IsManaualEditAllowed},
    {"CanUpdateLatLong", storeAdditionalInfo.CanUpdateLatLong},
    {"IsTaxApplicable", storeAdditionalInfo.IsTaxApplicable},
    {"AllowGoodReturn", storeAdditionalInfo.AllowGoodReturn},
    {"AllowBadReturn", storeAdditionalInfo.AllowBadReturn},
    {"EnableAsset", storeAdditionalInfo.EnableAsset},
    {"AllowReplacement", storeAdditionalInfo.AllowReplacement},
    {"IsInvoiceCancellationAllowed", storeAdditionalInfo.IsInvoiceCancellationAllowed},
    {"IsDeliveryNoteRequired", storeAdditionalInfo.IsDeliveryNoteRequired},
    {"EInvoicingEnabled", storeAdditionalInfo.EInvoicingEnabled},
    {"ImageRecognizationEnabled", storeAdditionalInfo.ImageRecognizationEnabled},
    {"MaxOutstandingInvoices", storeAdditionalInfo.MaxOutstandingInvoices},
    {"NegativeInvoiceAllowed", storeAdditionalInfo.NegativeInvoiceAllowed},
};
                await dbManager.ExecuteNonQueryAsync(sql, parameters);
                return storeAdditionalInfo;
            }
            catch (Exception ex)
            {
                throw;
            }
        }



        public async Task<int> UpdateStoreAdditionalInfo(StoreAdditionalInfo storeAdditionalInfo)
        {
            DBManager.PostgresDBManager<StoreAdditionalInfo> dbManager = new DBManager.PostgresDBManager<StoreAdditionalInfo>(_connectionString);
            try
            {

                var sql = "INSERT INTO StoreAdditionalInfo (id, uid, storeuid, ordertype, ispromotionsblock, customerstartdate, customerenddate, isschoolcustomer," +
                    " purchaseordernumber, deliverydocketispurchaseorderrequired, iswithprintedinvoices, isalwaysprinted, buildingdeliverycode, deliveryinformation, " +
                    "isstopdelivery, isforecasttopupqty, istemperaturecheck, invoicestartdate, invoiceenddate, invoiceformat, invoicedeliverymethod, displaydeliverydocket, " +
                    "displayprice, showcustpo, invoicetext, invoicefrequency, stockcreditispurchaseorderrequired, adminfeeperbillingcycle, adminfeeperdelivery, latepayementfee, " +
                    "drawer, bankuid, bankaccount, mandatoryponumber, isstorecreditcapturesignaturerequired, storecreditalwaysprinted, isdummycustomer, defaultrun, " +
                    "prospectempuid, isfoccustomer, rssshowprice, rssshowpayment, rssshowcredit, rssshowinvoice, rssisactive, rssdeliveryinstructionstatus, " +
                    "rsstimespentonrssportal, rssorderplacedinrss, rssavgordersperweek, rsstotalordervalue, allowforcecheckin, ismanaualeditallowed, canupdatelatlong," +
                    " istaxapplicable, allowgoodreturn, allowbadreturn, enableasset, enablesurvey, allowreplacement, isinvoicecancellationallowed, isdeliverynoterequired," +
                    " einvoicingenabled, imagerecognizationenabled, maxoutstandinginvoices, negativeinvoiceallowed) " +
                    "VALUES (@Id, @UID, @StoreUID, @OrderType, @IsPromotionsBlock, @CustomerStartDate, @CustomerEndDate, @IsSchoolCustomer," +
                    " @PurchaseOrderNumber,@DeliveryDocketIsPurchaseOrderRequired, @IsWithPrintedInvoices, @IsAlwaysPrinted, @BuildingDeliveryCode," +
                    " @DeliveryInformation, @IsStopDelivery, @IsForeCastTopUpQty, @IsTemperatureCheck, @InvoiceStartDate," +
                    " @InvoiceEndDate, @InvoiceFormat, @InvoiceDeliveryMethod, @DisplayDeliveryDocket,@DisplayPrice, @ShowCustPO, @InvoiceText, " +
                    "@InvoiceFrequency, @StockCreditIsPurchaseOrderRequired, @AdminFeePerBillingCycle, @AdminFeePerDelivery,@LatePayementFee, " +
                    "@Drawer, @BankUID, @BankAccount, @MandatoryPONumber, @IsStoreCreditCaptureSignatureRequired, @StoreCreditAlwaysPrinted, @IsDummyCustomer," +
                    " @DefaultRun, @ProspectEmpUID, @IsFOCCustomer, @RSSShowPrice, @RSSShowPayment, @RSSShowCredit, @RSSShowInvoice, @RSSIsActive, " +
                    "@RSSDeliveryInstructionStatus,@RSSTimeSpentOnRSSPortal, @RSSOrderPlacedInRSS, @RSSAvgOrdersPerWeek, " +
                    "@RSSTotalOrderValue, @AllowForceCheckIn, @IsManaualEditAllowed, @CanUpdateLatLong, @IsTaxApplicable, @AllowGoodReturn, @AllowBadReturn, " +
                    "@EnableAsset, @EnableSurvey, @AllowReplacement, @IsInvoiceCancellationAllowed, @IsDeliveryNoteRequired, @EInvoicingEnabled, @ImageRecognizationEnabled," +
                    " @MaxOutstandingInvoices, @NegativeInvoiceAllowed) ";



                Dictionary<string, object> parameters = new Dictionary<string, object>
{
  // {"UID", storeAdditionalInfo.UID},
    {"StoreUID", storeAdditionalInfo.StoreUID},
    {"OrderType", storeAdditionalInfo.OrderType},
    {"IsPromotionsBlock", storeAdditionalInfo.IsPromotionsBlock},
    {"CustomerStartDate", storeAdditionalInfo.CustomerStartDate},
    {"CustomerEndDate", storeAdditionalInfo.CustomerEndDate},
    {"IsSchoolCustomer", storeAdditionalInfo.IsSchoolCustomer},
    {"PurchaseOrderNumber", storeAdditionalInfo.PurchaseOrderNumber},
    {"DeliveryDocketIsPurchaseOrderRequired", storeAdditionalInfo.DeliveryDocketIsPurchaseOrderRequired},
    {"IsWithPrintedInvoices", storeAdditionalInfo.IsWithPrintedInvoices},
    {"IsAlwaysPrinted", storeAdditionalInfo.IsAlwaysPrinted},
    {"BuildingDeliveryCode", storeAdditionalInfo.BuildingDeliveryCode},
    {"DeliveryInformation", storeAdditionalInfo.DeliveryInformation},
    {"IsStopDelivery", storeAdditionalInfo.IsStopDelivery},
    {"IsForeCastTopUpQty", storeAdditionalInfo.IsForeCastTopUpQty},
    {"IsTemperatureCheck", storeAdditionalInfo.IsTemperatureCheck},
    {"InvoiceStartDate", storeAdditionalInfo.InvoiceStartDate},
    {"InvoiceEndDate", storeAdditionalInfo.InvoiceEndDate},
    {"InvoiceFormat", storeAdditionalInfo.InvoiceFormat},
    {"InvoiceDeliveryMethod", storeAdditionalInfo.InvoiceDeliveryMethod},
    {"DisplayDeliveryDocket", storeAdditionalInfo.DisplayDeliveryDocket},
    {"DisplayPrice", storeAdditionalInfo.DisplayPrice},
    {"ShowCustPO", storeAdditionalInfo.ShowCustPO},
    {"InvoiceText", storeAdditionalInfo.InvoiceText},
    {"InvoiceFrequency", storeAdditionalInfo.InvoiceFrequency},
    {"StockCreditIsPurchaseOrderRequired", storeAdditionalInfo.StockCreditIsPurchaseOrderRequired},
    {"AdminFeePerBillingCycle", storeAdditionalInfo.AdminFeePerBillingCycle},
    {"AdminFeePerDelivery", storeAdditionalInfo.AdminFeePerDelivery},
    {"LatePayementFee", storeAdditionalInfo.LatePayementFee},
    {"Drawer", storeAdditionalInfo.Drawer},
    {"BankUID", storeAdditionalInfo.BankUID},
    {"BankAccount", storeAdditionalInfo.BankAccount},
    {"MandatoryPONumber", storeAdditionalInfo.MandatoryPONumber},
    {"IsStoreCreditCaptureSignatureRequired", storeAdditionalInfo.IsStoreCreditCaptureSignatureRequired},
    {"StoreCreditAlwaysPrinted", storeAdditionalInfo.StoreCreditAlwaysPrinted},
    {"IsDummyCustomer", storeAdditionalInfo.IsDummyCustomer},
    {"DefaultRun", storeAdditionalInfo.DefaultRun},
    {"ProspectEmpUID", storeAdditionalInfo.ProspectEmpUID},
    {"IsFOCCustomer", storeAdditionalInfo.IsFOCCustomer},
    {"RSSShowPrice", storeAdditionalInfo.RSSShowPrice},
    {"RSSShowPayment", storeAdditionalInfo.RSSShowPayment},
    {"RSSShowCredit", storeAdditionalInfo.RSSShowCredit},
    {"RSSShowInvoice", storeAdditionalInfo.RSSShowInvoice},
    {"RSSIsActive", storeAdditionalInfo.RSSIsActive},
    {"RSSDeliveryInstructionStatus", storeAdditionalInfo.RSSDeliveryInstructionStatus},
    {"RSSTimeSpentOnRSSPortal", storeAdditionalInfo.RSSTimeSpentOnRSSPortal},
    {"RSSOrderPlacedInRSS", storeAdditionalInfo.RSSOrderPlacedInRSS},
    {"RSSAvgOrdersPerWeek", storeAdditionalInfo.RSSAvgOrdersPerWeek},
    {"RSSTotalOrderValue", storeAdditionalInfo.RSSTotalOrderValue},
    {"AllowForceCheckIn", storeAdditionalInfo.AllowForceCheckIn},
    {"IsManaualEditAllowed", storeAdditionalInfo.IsManaualEditAllowed},
    {"CanUpdateLatLong", storeAdditionalInfo.CanUpdateLatLong},
    {"IsTaxApplicable", storeAdditionalInfo.IsTaxApplicable},
    {"AllowGoodReturn", storeAdditionalInfo.AllowGoodReturn},
    {"AllowBadReturn", storeAdditionalInfo.AllowBadReturn},
    {"EnableAsset", storeAdditionalInfo.EnableAsset},
    {"AllowReplacement", storeAdditionalInfo.AllowReplacement},
    {"IsInvoiceCancellationAllowed", storeAdditionalInfo.IsInvoiceCancellationAllowed},
    {"IsDeliveryNoteRequired", storeAdditionalInfo.IsDeliveryNoteRequired},
    {"EInvoicingEnabled", storeAdditionalInfo.EInvoicingEnabled},
    {"ImageRecognizationEnabled", storeAdditionalInfo.ImageRecognizationEnabled},
    {"MaxOutstandingInvoices", storeAdditionalInfo.MaxOutstandingInvoices},
    {"NegativeInvoiceAllowed", storeAdditionalInfo.NegativeInvoiceAllowed},
};
                var updateDetails= await dbManager.ExecuteNonQueryAsync(sql, parameters);
                return updateDetails;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> DeleteStoreAdditionalInfo(string storeUID)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.Store.StoreAdditionalInfo> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.Store.StoreAdditionalInfo>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"storeUID",storeUID}
            };
            var sql = "DELETE  FROM StoreAdditionalInfo WHERE storeUID = @storeUID";

            var status = await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return status;
        }


    }
}









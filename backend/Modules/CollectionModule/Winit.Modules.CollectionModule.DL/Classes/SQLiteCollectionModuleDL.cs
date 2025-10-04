using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Npgsql;
using System.Data;
using System.Text;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.CollectionModule.DL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Contact.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CollectionModule.DL.Classes
{
    public class SQLiteCollectionModuleDL : Base.DL.DBManager.SqliteDBManager, Interfaces.ICollectionModuleDL
    {
        public SQLiteCollectionModuleDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider)
        {

        }
        private static string sixDigitGUIDString { get; set; } = "";
        private string AccCollectionUID { get; set; }
        private string _creditID { get; set; } = $"{DateTime.Now.Minute:D2}{DateTime.Now.Second:D2}{DateTime.Now.Millisecond:D3}";
        public string creditID()
        {
            return $"{DateTime.Now.Minute:D2}{DateTime.Now.Second:D2}{DateTime.Now.Millisecond:D3}";
        }
        //Common Methods 
        public async Task<string> InsertAccCollection(SqliteConnection conn, SqliteTransaction transaction, ICollections collection)
        {
            try
            {
                Random random = new Random();
                int sixDigitNumber = random.Next(100000, 1000000);
                string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

                // Create a char array to store the random characters
                char[] sixCharString = new char[6];

                // Generate a random 6-character string
                for (int i = 0; i < 6; i++)
                {
                    sixCharString[i] = characters[random.Next(characters.Length)];
                }

                // Convert the char array to a string
                string randomString = new string(sixCharString);
                sixDigitGUIDString = randomString + sixDigitNumber.ToString("D4");
                ICollections accCollection = await GetCustomerName(collection.AccCollection.ReceiptNumber);
                var Retval = "";
                var sql = @"INSERT INTO acc_collection (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                        receipt_number, consolidated_receipt_number, category, amount, currency_uid, default_currency_uid, default_currency_exchange_rate, 
                        default_currency_amount, org_uid, distribution_channel_uid, store_uid, route_uid, job_position_uid, emp_uid, collected_date, 
                        status, remarks, reference_number, is_realized, latitude, longitude, source, is_multimode, trip_date, comments, salesman,
                        route, reversal_receipt_uid, cancelled_on, is_remote_collection, remote_collection_reason, ss)  
                        VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @ReceiptNumber, @ConsolidatedReceiptNumber, @Category, @Amount, @CurrencyUID, @DefaultCurrencyUID, @DefaultCurrencyExchangeRate,
                                @DefaultCurrencyAmount, @OrgUID, @DistributionChannelUID, @StoreUID, @RouteUID, @JobPositionUID, @EmpUID, @CollectedDate, @Status, @Remarks, @ReferenceNumber, @IsRealized, @Latitude, @Longitude, @Source, @IsMultimode, @TripDate,
                                @Comments, @Salesman, @Route, @ReversalReceiptUID, @CancelledOn, @IsRemoteCollection, @RemoteCollectionReason, @SS)";
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
               {"UID", string.IsNullOrEmpty(collection.AccCollection.UID) ? "" : collection.AccCollection.UID},
               {"ReceiptNumber", string.IsNullOrEmpty(collection.AccCollection.ReceiptNumber) ? "":collection.AccCollection.ReceiptNumber},
               {"ConsolidatedReceiptNumber", string.IsNullOrEmpty(collection.AccCollection.ConsolidatedReceiptNumber) ?"":collection.AccCollection.ConsolidatedReceiptNumber},
               {"Category", string.IsNullOrEmpty(collection.AccCollection.Category) ? "" : collection.AccCollection.Category},
               {"Amount", collection.AccCollection.Amount == 0 ? 0 : collection.AccCollection.Amount},
               {"CurrencyUID", string.IsNullOrEmpty(collection.AccCollection.CurrencyUID)?"":collection.AccCollection.CurrencyUID},
               {"DefaultCurrencyUID", string.IsNullOrEmpty(collection.AccCollection.DefaultCurrencyUID)?"":collection.AccCollection.DefaultCurrencyUID},
               {"DefaultCurrencyExchangeRate", collection.AccCollection.DefaultCurrencyExchangeRate==0?0:collection.AccCollection.DefaultCurrencyExchangeRate},
               {"DefaultCurrencyAmount", collection.AccCollection.DefaultCurrencyAmount==0?0:collection.AccCollection.DefaultCurrencyAmount},
               {"OrgUID", string.IsNullOrEmpty(collection.AccCollection.OrgUID)?"":collection.AccCollection.OrgUID},
               {"DistributionChannelUID", string.IsNullOrEmpty(collection.AccCollection.DistributionChannelUID)?"":collection.AccCollection.DistributionChannelUID},
               {"StoreUID", string.IsNullOrEmpty(collection.AccCollection.StoreUID)? string.IsNullOrEmpty(collection.AccCollectionAllotment.FirstOrDefault().StoreUID) ? "" : collection.AccCollectionAllotment.FirstOrDefault().StoreUID : collection.AccCollection.StoreUID},
               {"RouteUID", string.IsNullOrEmpty(collection.AccCollection.RouteUID)?"":collection.AccCollection.RouteUID},
               {"JobPositionUID", string.IsNullOrEmpty(collection.AccCollection.JobPositionUID)?"":collection.AccCollection.JobPositionUID},
               {"EmpUID", string.IsNullOrEmpty(collection.AccCollection.EmpUID)?"":collection.AccCollection.EmpUID},
               {"CollectedDate", collection.AccCollection.CollectedDate==null?DateTime.Now:collection.AccCollection.CollectedDate},
               {"Status", collection.AccCollection.ReceiptNumber.Contains("OA") ? "OnAccount" : string.IsNullOrEmpty(collection.AccCollection.Status)?"":collection.AccCollection.Status},
               {"Remarks", string.IsNullOrEmpty(collection.AccCollection.Remarks)?"":collection.AccCollection.Remarks},
               {"ReferenceNumber", string.IsNullOrEmpty(collection.AccCollection.ReferenceNumber)?"":collection.AccCollection.ReferenceNumber},
               {"IsRealized", collection.AccCollection.IsRealized==false?false:collection.AccCollection.IsRealized},
               {"Latitude", string.IsNullOrEmpty(collection.AccCollection.Latitude)?"":collection.AccCollection.Latitude},
               {"Longitude", string.IsNullOrEmpty(collection.AccCollection.Longitude)?"":collection.AccCollection.Longitude},
               {"Source", string.IsNullOrEmpty(collection.AccCollection.Source)?"":collection.AccCollection.Source},
               {"IsMultimode", collection.AccCollection.IsMultimode==false?false:collection.AccCollection.IsMultimode},
               {"TripDate", collection.AccCollection.TripDate==null ? DateTime.Now : collection.AccCollection.TripDate},
               {"Comments", string.IsNullOrEmpty(collection.AccCollection.Comments) ? "" : collection.AccCollection.Comments},
               {"Salesman", string.IsNullOrEmpty(collection.AccCollection.Salesman) ? "" : collection.AccCollection.Salesman},
               {"Route", string.IsNullOrEmpty(collection.AccCollection.Route) ? "" : collection.AccCollection.Route},
               {"ReversalReceiptUID", string.IsNullOrEmpty(collection.AccCollection.ReversalReceiptUID) ? "" : collection.AccCollection.ReversalReceiptUID},
               {"CancelledOn", collection.AccCollection.CancelledOn == null ? DateTime.Now : collection.AccCollection.CancelledOn},
               {"IsRemoteCollection", collection.AccCollection.IsRemoteCollection},
               {"RemoteCollectionReason", collection.AccCollection.RemoteCollectionReason ?? ""},
               {"CreatedTime", DateTime.Now},
               {"ModifiedTime", DateTime.Now},
               {"ModifiedBy", string.IsNullOrEmpty(collection.AccCollection.ModifiedBy) ? "" : collection.AccCollection.ModifiedBy},
               {"ServerAddTime", DateTime.Now},
               {"ServerModifiedTime", DateTime.Now},
               {"SS", 1},
               {"CreatedBy", string.IsNullOrEmpty(collection.AccCollection.CreatedBy) ? "" : collection.AccCollection.CreatedBy}
            };
                Retval = await CommonMethod(sql, conn, parameters, transaction);
                if (Retval != Const.One)
                {
                    return Retval;
                }
                else
                {
                    return Retval;
                }
            }
            catch (Exception ex)
            {
                throw new();
            }
        }
        public async Task<String> InsertAccCollectionPaymentMode(SqliteConnection conn, SqliteTransaction transaction, ICollections collection)
        {
            var Retval = "";
            var sql1 = @"INSERT INTO acc_collection_payment_mode (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, acc_collection_uid, bank_uid, branch, cheque_no, amount, currency_uid, default_currency_uid, default_currency_exchange_rate, default_currency_amount, cheque_date, status, realization_date, check_list_data, ss)   VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                                @AccCollectionUID, @BankUID, @Branch, @ChequeNo, @Amount, @CurrencyUID, @DefaultCurrencyUID, @DefaultCurrencyExchangeRate, 
                                @DefaultCurrencyAmount, @ChequeDate, @Status, @RealizationDate, @CheckListData, @SS)";
            Dictionary<string, object?> parameters1 = new Dictionary<string, object?>
                        {
                            {"AccCollectionUID", string.IsNullOrEmpty(collection.AccCollection.UID)  ? "" : collection.AccCollection.UID},
                            {"BankUID", string.IsNullOrEmpty(collection.AccCollectionPaymentMode.BankUID) ? "" : collection.AccCollectionPaymentMode.BankUID},
                            {"Branch", string.IsNullOrEmpty(collection.AccCollectionPaymentMode.Branch) ? "" : collection.AccCollectionPaymentMode.Branch},
                            {"ChequeNo", string.IsNullOrEmpty(collection.AccCollectionPaymentMode.ChequeNo) ? "" : collection.AccCollectionPaymentMode.ChequeNo},
                            {"CurrencyUID", string.IsNullOrEmpty(collection.AccCollection.CurrencyUID) ? "" : collection.AccCollection.CurrencyUID},
                            {"DefaultCurrencyUID", string.IsNullOrEmpty(collection.AccCollection.DefaultCurrencyUID) ? "" : collection.AccCollection.DefaultCurrencyUID},
                            {"DefaultCurrencyExchangeRate", collection.AccCollection.DefaultCurrencyExchangeRate==0?0:collection.AccCollection.DefaultCurrencyExchangeRate},
                            {"DefaultCurrencyAmount", collection.AccCollection.DefaultCurrencyAmount==0?0:collection.AccCollection.DefaultCurrencyAmount},
                            {"ChequeDate", collection.AccCollectionPaymentMode.ChequeDate==null?DateTime.Now:collection.AccCollectionPaymentMode.ChequeDate},
                            {"Status", collection.AccCollection.ReceiptNumber.Contains("OA") ? "OnAccount" : string.IsNullOrEmpty(collection.AccCollection.Status)?"":collection.AccCollection.Status},
                            {"RealizationDate", collection.AccCollectionPaymentMode.RealizationDate==null?DateTime.Now:collection.AccCollectionPaymentMode.RealizationDate},
                            {"CreatedTime",DateTime.Now},
                            {"ModifiedTime", DateTime.Now},
                            {"ModifiedBy", string.IsNullOrEmpty(collection.AccCollection.ModifiedBy) ? "" : collection.AccCollection.ModifiedBy},
                            {"ServerAddTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now},
                            {"CreatedBy", string.IsNullOrEmpty(collection.AccCollection.CreatedBy) ? "" : collection.AccCollection.CreatedBy},
                            {"UID", (Guid.NewGuid()).ToString()},
                            {"Amount", collection.AccCollection.Amount},
                            {"CheckListData", collection.AccCollectionPaymentMode.CheckListData ?? ""},
                            {"SS", 1},
                        };
            Retval = await CommonMethod(sql1, conn, parameters1, transaction);
            if (Retval != Const.One)
                return Retval;
            else
                return Retval;
        }
        public async Task<string> InsertAccStoreLedger(SqliteConnection conn, SqliteTransaction transaction, ICollections collection)
        {
            IAccStoreLedger accStore = await GetStoreLedgerCreation(collection.AccCollection.StoreUID);
            if (accStore == null)
            {
                accStore = new Winit.Modules.CollectionModule.Model.Classes.AccStoreLedger();
            }
            var Retval = "";
            var sql3 = @"INSERT INTO acc_store_ledger (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, source_type, source_uid, credit_type, org_uid, store_uid, default_currency_uid, document_number, default_currency_exchange_rate, default_currency_amount, amount, transaction_date_time, collected_amount, currency_uid, balance, ss)
                        VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,@ServerAddTime, @ServerModifiedTime, @SourceType, @SourceUID,
                                   @CreditType, @OrgUID, @StoreUID, @DefaultCurrencyUID,@DocumentNumber, @DefaultCurrencyExchangeRate, @DefaultCurrencyAmount,
                                   @Amount, @TransactionDateTime, @CollectedAmount, @CurrencyUID, @Balance, @SS)";
            Dictionary<string, object?> parameters3 = new Dictionary<string, object?>
                        {
                            {"SourceType", "Collection"},
                            {"SourceUID", collection.AccCollection.UID==null?"": collection.AccCollection.UID},
                            {"CreditType",collection.AccCollection.ReceiptNumber.Contains("OA")||collection.AccCollection.CreditNote!="" ? "CR" : "DR"},
                            {"OrgUID", collection.AccStoreLedger.OrgUID==null?"": collection.AccStoreLedger.OrgUID},
                            {"StoreUID", collection.AccCollection.StoreUID==null?"": collection.AccCollection.StoreUID},
                            {"DefaultCurrencyUID", collection.AccCollection.DefaultCurrencyUID==null?"": collection.AccCollection.DefaultCurrencyUID},
                            {"DocumentNumber", collection.AccCollection.ReceiptNumber == null ? "" : collection.AccCollection.ReceiptNumber},
                            {"DefaultCurrencyExchangeRate", collection.AccCollection.DefaultCurrencyExchangeRate==0?0: collection.AccCollection.DefaultCurrencyExchangeRate},
                            {"DefaultCurrencyAmount", collection.AccCollection.DefaultCurrencyAmount==0?0: collection.AccCollection.DefaultCurrencyAmount},
                            {"Amount", collection.AccCollection.Category == "Cash" ? (collection.AccCollection.DefaultCurrencyAmount == 0 ? 0 : collection.AccCollection.ReceiptNumber.Contains("OA") ? (collection.AccCollection.DefaultCurrencyAmount + collection.AccCollection.DiscountAmount)*-1 : (collection.AccCollection.DefaultCurrencyAmount + collection.AccCollection.DiscountAmount))  : collection.AccCollection.ReceiptNumber.Contains("OA") ? 0 :   0},
                            {"TransactionDateTime", collection.AccStoreLedger.TransactionDateTime == null ? DateTime.Now : DateTime.Now},
                            {"CollectedAmount", /*collection.Amount == 0 ? 0 : collection.Amount + collection.DiscountAmount*/collection.AccCollection.Category == "Cash" ? (collection.AccCollection.DefaultCurrencyAmount == 0 ? 0 : collection.AccCollection.ReceiptNumber.Contains("OA") ? (collection.AccCollection.DefaultCurrencyAmount + collection.AccCollection.DiscountAmount)*-1 : (collection.AccCollection.DefaultCurrencyAmount + collection.AccCollection.DiscountAmount)) : collection.AccCollection.ReceiptNumber.Contains("OA") ? 0 :   0},
                            {"Balance",  collection.AccCollection.Category == "Cash" ? accStore.Balance == 0 ? (collection.AccCollection.ReceiptNumber.Contains("OA")||collection.AccCollection.CreditNote!= string.Empty? 0 - collection.AccCollection.DefaultCurrencyAmount - collection.AccCollection.DiscountAmount : 0 + collection.AccCollection.DefaultCurrencyAmount + collection.AccCollection.DiscountAmount)  : (collection.AccCollection.ReceiptNumber.Contains("OA")||collection.AccCollection.CreditNote!= string.Empty? accStore.Balance  - collection.AccCollection.DefaultCurrencyAmount - collection.AccCollection.DiscountAmount : accStore.Balance + collection.AccCollection.DefaultCurrencyAmount + collection.AccCollection.DiscountAmount) :
                             accStore.Balance == 0 ? (collection.AccCollection.ReceiptNumber.Contains("OA") ? 0  : 0 )  : (collection.AccCollection.ReceiptNumber.Contains("OA") ? accStore.Balance - 0  : accStore.Balance + 0)               },
                            {"CurrencyUID", collection.AccCollection.CurrencyUID ==null ? "" : collection.AccCollection.CurrencyUID},
                            {"CreatedTime", DateTime.Now},
                            {"ModifiedTime", DateTime.Now},
                            {"ModifiedBy", string.IsNullOrEmpty(collection.AccCollection.ModifiedBy) ? "" : collection.AccCollection.ModifiedBy},
                            {"ServerAddTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now},
                            {"CreatedBy", string.IsNullOrEmpty(collection.AccCollection.CreatedBy) ? "" : collection.AccCollection.CreatedBy},
                            {"UID", (Guid.NewGuid()).ToString()},
                            {"SS", 1}
                        };
            Retval = await CommonMethod(sql3, conn, parameters3, transaction);
            if (Retval != Const.One)
                return Retval;
            else
                return Retval;
        }
        public async Task<string> InsertAccCollectionAllotment(SqliteConnection conn, SqliteTransaction transaction, ICollections collection)
        {
            try
            {
                var Retval = "";

                foreach (var list in collection.AccCollectionAllotment)
                {
                    IAccPayable accPayable = await GetAccPayableAmount(list.StoreUID, list.ReferenceNumber);
                    IAccReceivable receive = await GetAccRecAmount(list.StoreUID, list.ReferenceNumber);
                    if (accPayable == null)
                    {
                        accPayable = new Winit.Modules.CollectionModule.Model.Classes.AccPayable();
                    }
                    if (receive == null)
                    {
                        receive = new Winit.Modules.CollectionModule.Model.Classes.AccReceivable();
                    }
                    var sql2 = @"INSERT INTO acc_collection_allotment (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, acc_collection_uid, target_type, target_uid, reference_number, currency_uid, default_currency_uid, default_currency_exchange_rate, default_currency_amount, early_payment_discount_percentage, early_payment_discount_amount, amount, ss)
                                VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @AccCollectionUID,
                                @TargetType, @TargetUID, @ReferenceNumber, @CurrencyUID, @DefaultCurrencyUID, @DefaultCurrencyExchangeRate, @DefaultCurrencyAmount, @EarlyPaymentDiscountPercentage,
                                @EarlyPaymentDiscountAmount, @Amount, @SS)";
                    Dictionary<string, object?> parameters2 = new Dictionary<string, object?>
                {
                                {"AccCollectionUID", collection.AccCollection.UID},
                                {"TargetType", list.TargetType == null ? "" : list.TargetType},
                                {"TargetUID",list.TargetType == "OA - CREDITNOTE" ? list.TargetUID : list.TargetType.Contains("INVOICE") ? accPayable.UID == null ? "" : accPayable.UID : receive.UID == null ? "CREDITNOTE-"+_creditID : receive.UID} ,
                                {"ReferenceNumber", list.TargetType == "OA - CREDITNOTE" ? "CREDITNOTE-"+_creditID : (list.ReferenceNumber == null|| list.ReferenceNumber ==string.Empty) ? "CREDITNOTE-"+_creditID : list.ReferenceNumber},
                                {"CurrencyUID", collection.AccCollection.CurrencyUID==null ? "" : collection.AccCollection.CurrencyUID},
                                {"DefaultCurrencyUID", collection.AccCollection.DefaultCurrencyUID==null ? "" : collection.AccCollection.DefaultCurrencyUID},
                                {"DefaultCurrencyExchangeRate", collection.AccCollection.DefaultCurrencyExchangeRate==0 ? 0 : collection.AccCollection.DefaultCurrencyExchangeRate},
                                {"DefaultCurrencyAmount", collection.AccCollection.DefaultCurrencyAmount==0 ? 0 : collection.AccCollection.DefaultCurrencyAmount},
                                {"EarlyPaymentDiscountPercentage", collection.AccCollection.DiscountValue==0 ? 0 : collection.AccCollection.DiscountValue },
                                {"EarlyPaymentDiscountAmount", list.DiscountAmount==0 ? 0 : list.DiscountAmount },
                                {"EarlyPaymentDiscountReferenceNo", list.DiscountAmount > 0 ? "CREDITNOTE-"+_creditID : "" },
                                {"Amount", list.PaidAmount == 0 ? collection.AccCollection.Amount : list.PaidAmount },
                                {"CreatedTime", DateTime.Now },
                                {"ModifiedTime", DateTime.Now},
                                {"ModifiedBy", Const.ModifiedBy },
                                {"ServerAddTime", DateTime.Now },
                                {"ServerModifiedTime", DateTime.Now },
                                {"CreatedBy", Const.ModifiedBy },
                                {"UID", (Guid.NewGuid()).ToString()},
                                {"SS", 1}
                            };
                    Retval = await CommonMethod(sql2, conn, parameters2, transaction);

                    if (Retval != Const.One)
                        return Retval;
                }
                return Retval;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        public async Task<string> InsertAccCollectionCurrencyDetails(SqliteConnection conn, SqliteTransaction transaction, ICollections collection)
        {
            try
            {
                var Result = "";
                foreach (var list in collection.AccCollectionCurrencyDetails)
                {
                    var sql = @"INSERT INTO acc_collection_currency_details (uid, created_by, created_time, modified_by, modified_time, 
                                server_add_time, server_modified_time, acc_collection_uid, 
                                currency_uid, default_currency_uid, default_currency_exchange_rate, 
                                amount, default_currency_amount, final_default_currency_amount, ss) VALUES (
                                @uid, @created_by, @created_time, @modified_by, @modified_time, 
                                @server_add_time, @server_modified_time, @acc_collection_uid, 
                                @currency_uid, @default_currency_uid, @default_currency_exchange_rate, 
                                @amount, @default_currency_amount, @final_default_currency_amount, @SS);";
                    Dictionary<string, object?> parameters = new Dictionary<string, object?>
                    {
                        //{"id", 1 },
                        {"uid", (Guid.NewGuid()).ToString()},
                        {"created_by", Const.ModifiedBy},
                        {"created_time", DateTime.Now},
                        {"modified_by", Const.ModifiedBy},
                        {"modified_time", DateTime.Now},
                        {"server_add_time", DateTime.Now},
                        {"server_modified_time", DateTime.Now},
                        {"acc_collection_uid", collection.AccCollection.UID},
                        {"currency_uid", list.currency_uid},
                        {"default_currency_uid", list.default_currency_uid},
                        {"default_currency_exchange_rate", list.default_currency_exchange_rate},
                        {"amount", list.amount},
                        {"SS", 1},
                        {"default_currency_amount", list.default_currency_amount},
                        {"final_default_currency_amount", list.final_default_currency_amount}
                    };
                    Result = await CommonMethod(sql, conn, parameters, transaction);

                    if (Result != Const.One)
                        return Result;
                }
                return Result;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }
        public async Task<string> UpdateAccReceivable(SqliteConnection conn, SqliteTransaction transaction, ICollections collection)
        {
            var Retval = "";
            foreach (var list in collection.AccReceivable)
            {
                IAccReceivable receive = await GetAccRecAmount(list.StoreUID, list.ReferenceNumber);
                var sql5 = @"UPDATE acc_receivable
                                    SET modified_time = @ModifiedTime,modified_by = @ModifiedBy,
                                    server_modified_time = @ServerModifiedTime,paid_amount = @PaidAmount
                                    WHERE uid = @UID";
                Dictionary<string, object?> parameters5 = new Dictionary<string, object?>
                            {
                                {"PaidAmount", receive.PaidAmount+list.PaidAmount==0?0 : receive.PaidAmount+list.PaidAmount},
                                {"ModifiedTime", DateTime.Now},
                                {"ModifiedBy", Const.ModifiedBy},
                                {"ServerModifiedTime", DateTime.Now},
                                {"UID", receive.UID}
                            };
                Retval = await CommonMethod(sql5, conn, parameters5, transaction);
                if (Retval != Const.One)
                    return Retval;
            }
            return Retval;
        }
        //public async Task<string> InsertAccReceivable(SqliteConnection conn, SqliteTransaction transaction, ICollections collection)
        //{
        //    var Retval = "";

        //    var sql5 = @"INSERT INTO ""AccReceivable"" (
        //            ""CreatedTime"",""ModifiedTime"",""ServerAddTime"",""ServerModifiedTime"",
        //           ""UID"",""SessionUserCode"",""ChequeNo"",""Amount"",""PaidAmount"", ""ModifiedBy"", ""CreatedBy"")
        //        VALUES (
        //            @CreatedTime,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@UID,@SessionUserCode,@ChequeNo, @Amount, @PaidAmount, @ModifiedBy, @CreatedBy)";

        //    Dictionary<string, object?> parameters5 = new Dictionary<string, object?>
        //                    {
        //                        {"Amount", collection.PaidAmount==0?0 : collection.PaidAmount},
        //                        {"PaidAmount", 0},
        //                        {"CreatedTime", DateTime.Now},
        //                        {"ModifiedTime", DateTime.Now},
        //                        {"ModifiedBy", Const.ModifiedBy},
        //                        {"ServerAddTime", DateTime.Now},
        //                        {"ServerModifiedTime", DateTime.Now},
        //                        {"CreatedBy", Const.ModifiedBy},
        //                        {"UID", (Guid.NewGuid()).ToString()},
        //                        {"SessionUserCode", collection.SessionUserCode==null?"":collection.SessionUserCode},
        //                        {"ChequeNo", collection.ChequeNo==null?"":collection.ChequeNo}
        //                    };
        //    Retval = await CommonMethod(sql5, conn, parameters5, transaction);

        //    if (Retval != Const.One)
        //        return Retval;

        //    return Retval;
        //}
        public async Task<bool> UpdateAccPayable(List<IAccPayable> accpaybles, SqliteTransaction transaction, SqliteConnection sqliteConnection)
        {
            foreach (var accpayble in accpaybles)
            {
                IAccPayable payable = await GetAccPayableAmount(accpayble.StoreUID, accpayble.ReferenceNumber);
                if (payable != null)
                {
                    var sql = @"UPDATE acc_payable
                                    SET modified_time = @ModifiedTime,
                                    modified_by = @ModifiedBy,
                                    server_modified_time = @ServerModifiedTime,
                                    paid_amount = @PaidAmount ,
                                    unsettled_amount  = @UnsettledAmount
                                    WHERE uid = @UID";
                    payable.PaidAmount += accpayble.PaidAmount;
                    payable.UnSettledAmount += accpayble.UnSettledAmount;
                    Dictionary<string, object?> parameters = new Dictionary<string, object?>
                            {
                                {"UnsettledAmount", payable.UnSettledAmount},
                                {"ModifiedTime", DateTime.Now},
                                {"ModifiedBy", Const.ModifiedBy},
                                {"ServerModifiedTime", DateTime.Now},
                                {"PaidAmount", payable.PaidAmount},
                                {"UID", payable.UID}
                            };
                    if (await ExecuteNonQueryAsync(sql, parameters, sqliteConnection, transaction) != 1)
                    {
                        throw new Exception("accpayble failed to update");
                    }
                }
                else throw new Exception("accpayble failed to update");
            }
            return true;
        }

        public async Task<string> InsertAccReceivable(SqliteConnection conn, SqliteTransaction transaction, ICollections collection)
        {
            var Retval = "";
            foreach (var list in collection.AccCollectionAllotment)
            {
                if (list.DiscountAmount > 0)
                {
                    var sql1 = @"INSERT INTO acc_receivable (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, source_type, source_uid, reference_number, org_uid, job_position_uid, amount, paid_amount, store_uid, transaction_date, due_date, source, currency_uid, unsettled_amount) VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime,
                                @ServerModifiedTime, @SourceType, @SourceUID, @ReferenceNumber, @OrgUID,
                                @JobPositionUID, @Amount, @PaidAmount, @StoreUID, @TransactionDate,
                                @DueDate, @Source, @CurrencyUID, @UnSettledAmount)";
                    Dictionary<string, object?> parameters1 = new Dictionary<string, object?>
                            {
                                {"CreatedTime", DateTime.Now },
                                {"ModifiedTime", DateTime.Now},
                                {"ModifiedBy", Const.ModifiedBy },
                                {"ServerAddTime", DateTime.Now },
                                {"ServerModifiedTime", DateTime.Now },
                                {"CreatedBy", Const.ModifiedBy },
                                {"SourceType", "CREDITNOTE" },
                                {"SourceUID", "CREDITNOTE-"+_creditID },
                                {"ReferenceNumber", "CREDITNOTE-"+_creditID },
                                {"OrgUID", collection.AccCollection.OrgUID },
                                {"JobPositionUID", collection.AccCollection.JobPositionUID },
                                {"CurrencyUID", collection.AccCollection.CurrencyUID },
                                {"StoreUID", collection.AccCollection.StoreUID },
                                {"TransactionDate", DateTime.Now },
                                {"DueDate", DateTime.Now.AddYears(1) },
                                {"Source", collection.AccCollection.Source },
                                {"Amount",list.DiscountAmount},
                                {"UnSettledAmount",collection.AccCollection.Category != "Cash" ? list.DiscountAmount : 0},
                                {"PaidAmount", 0},
                                {"UID", (Guid.NewGuid()).ToString()}
                            };

                    Retval = await CommonMethod(sql1, conn, parameters1, transaction);

                    if (Retval != Const.One)
                        return Retval;
                }
            }
            return Retval;
        }
        public async Task<string> InsertEarlyPaymentDiscountAppliedDetails(SqliteConnection conn, SqliteTransaction transaction, ICollections collection)
        {
            var Retval = "";
            foreach (var list in collection.AccCollectionAllotment)
            {
                if (list.DiscountAmount > 0)
                {
                    var sql1 = @"INSERT INTO early_payment_discount_applied_details (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, early_payment_discount_number, invoice_number, invoice_date, invoice_due_amount, sales_org, applicable_type, applicable_code, payment_mode, advance_paid_days, discount_type, discount_value, is_active, valid_from, valid_to, applicable_on_partial_payments, applicable_on_overdue_customers) 
                            VALUES 
                            (@uid, @created_by, @created_time, @modified_by, @modified_time, @server_add_time, @server_modified_time, 
                                @early_payment_discount_number, @invoice_number, @invoice_date, @invoice_due_amount, @sales_org, @applicable_type, 
                                @applicable_code, @payment_mode, @advance_paid_days, @discount_type, @discount_value, @isactive, @valid_from, 
                                @valid_to, @applicable_onpartial_payments, @applicable_onoverdue_customers)";
                    Dictionary<string, object?> parameters1 = new Dictionary<string, object?>
                            {
                                {"early_payment_discount_number", string.IsNullOrEmpty(collection.AccCollection.ReceiptNumber)  ? string.Empty : collection.AccCollection.ReceiptNumber},
                                {"invoice_number", string.IsNullOrEmpty(list.ReferenceNumber) ? string.Empty : list.ReferenceNumber},
                                {"invoice_date",  DateTime.Now},
                                {"invoice_due_amount", list.Balance ==0  ? 0 : list.Balance},
                                {"sales_org", string.IsNullOrEmpty(collection.AccCollection.OrgUID) ? string.Empty : collection.AccCollection.OrgUID},
                                {"applicable_type", "Customer"},
                                {"applicable_code", string.IsNullOrEmpty(collection.AccCollection.StoreUID) ? string.Empty : collection.AccCollection.StoreUID},
                                {"payment_mode", string.IsNullOrEmpty(collection.AccCollection.Category ) ?string.Empty : collection.AccCollection.Category},
                                {"advance_paid_days", collection.AccCollection.AdvancePaidDays == 0 ? 0 : collection.AccCollection.AdvancePaidDays},
                                {"discount_value", collection.AccCollection.DiscountValue == 0 ? 0 : collection.AccCollection.DiscountValue},
                                {"discount_type", "%"},
                                {"isactive",collection.AccCollection.IsActive == null ? false : true},
                                {"valid_from", new DateTime(2024, 01, 01)},
                                {"valid_to", new DateTime(2024, 12, 31)},
                                {"applicable_onpartial_payments", false},
                                {"applicable_onoverdue_customers", false},
                                {"created_time", DateTime.Now},
                                {"modified_time", DateTime.Now},
                                {"modified_by", Const.ModifiedBy},
                                {"created_by", "WINIT"},
                                {"server_modified_time", DateTime.Now},
                                {"server_add_time", DateTime.Now},
                                {"uid", (Guid.NewGuid()).ToString()}
                            };

                    Retval = await CommonMethod(sql1, conn, parameters1, transaction);

                    if (Retval != Const.One)
                        return Retval;
                }
            }
            return Retval;
        }

        //Common Methods end

        //Getting Data from tables to update
        public async Task<string> CheckExists(string Receiptnumber)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"UID",  Receiptnumber}
                };

                var sql = @"SELECT 
                                id AS Id,
                                uid AS Uid,
                                created_by AS CreatedBy,
                                created_time AS CreatedTime,
                                modified_by AS ModifiedBy,
                                modified_time AS ModifiedTime,
                                server_add_time AS ServerAddTime,
                                server_modified_time AS ServerModifiedTime,
                                acc_collection_uid AS AccCollectionUid,
                                collection_amount AS CollectionAmount,
                                received_amount AS ReceivedAmount,
                                has_discrepancy AS HasDiscrepancy,
                                discrepancy_amount AS DiscrepancyAmount,
                                default_currency_uid AS DefaultCurrencyUid,
                                settlement_date AS SettlementDate,
                                cashier_job_position_uid AS CashierJobPositionUid,
                                cashier_emp_uid AS CashierEmpUid,
                                receipt_number AS ReceiptNumber,
                                settled_by AS SettledBy,
                                session_user_code AS SessionUserCode,
                                route AS Route,
                                user_code AS UserCode,
                                target_type AS TargetType,
                                payment_mode AS PaymentMode,
                                transaction_date AS TransactionDate,
                                due_date AS DueDate,
                                is_void AS IsVoid,
                                cash_number AS CashNumber
                            FROM 
                                acc_collection_settlement 
                            WHERE 
                                cash_number = @UID";
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionSettlement>().GetType();
                Model.Interfaces.IAccCollectionSettlement CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IAccCollectionSettlement>(sql, parameters, type);
                var Res = CollectionModuleList != null ? "true" : "false";
                return Res;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public async Task<Model.Interfaces.ICollections> GetCustomerName(string ReceiptNumber)
        {

            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"ReceiptNumber",  ReceiptNumber}
                };

                var sql = @"SELECT 
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            receipt_number AS ReceiptNumber,
                            consolidated_receipt_number AS ConsolidatedReceiptNumber,
                            category AS Category,
                            amount AS Amount,
                            currency_uid AS CurrencyUid,
                            default_currency_uid AS DefaultCurrencyUid,
                            default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                            default_currency_amount AS DefaultCurrencyAmount,
                            org_uid AS OrgUid,
                            distribution_channel_uid AS DistributionChannelUid,
                            store_uid AS StoreUid,
                            route_uid AS RouteUid,
                            job_position_uid AS JobPositionUid,
                            emp_uid AS EmpUid,
                            collected_date AS CollectedDate,
                            status AS Status,
                            remarks AS Remarks,
                            reference_number AS ReferenceNumber,
                            is_realized AS IsRealized,
                            latitude AS Latitude,
                            longitude AS Longitude,
                            source AS Source,
                            is_multimode AS IsMultimode,
                            trip_date AS TripDate,
                            comments AS Comments,
                            salesman AS Salesman,
                            route AS Route,
                            reversal_receipt_uid AS ReversalReceiptUid,
                            cancelled_on AS CancelledOn
                        FROM 
                            acc_collection 
                        WHERE 
                            receipt_number = @ReceiptNumber";

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICollections>().GetType();
                Model.Interfaces.ICollections CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.ICollections>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                return (ICollections)ex;
            }
        }
        public async Task<Model.Interfaces.IAccStoreLedger> GetStoreLedgerCreation(string StoreUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
         {
             {"StoreUID",  StoreUID}
         };

            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        source_type AS SourceType,
                        source_uid AS SourceUid,
                        credit_type AS CreditType,
                        org_uid AS OrgUid,
                        store_uid AS StoreUid,
                        default_currency_uid AS DefaultCurrencyUid,
                        document_number AS DocumentNumber,
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                        default_currency_amount AS DefaultCurrencyAmount,
                        transaction_date_time AS TransactionDateTime,
                        collected_amount AS CollectedAmount,
                        currency_uid AS CurrencyUid,
                        amount AS Amount,
                        balance AS Balance,
                        comments AS Comments
                    FROM 
                        acc_store_ledger 
                    WHERE 
                        store_uid = @StoreUID 
                    ORDER BY 
                        created_time DESC";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccStoreLedger>().GetType();
            Model.Interfaces.IAccStoreLedger CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IAccStoreLedger>(sql, parameters, type);
            return CollectionModuleList;

        }
        public async Task<Model.Interfaces.ICollections> GetAmountCash(string UID)
        {

            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"UID",  UID}
                };

                var sql = @"SELECT 
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            receipt_number AS ReceiptNumber,
                            is_realized AS IsRealized,
                            latitude AS Latitude,
                            longitude AS Longitude,
                            source AS Source,
                            is_multimode AS IsMultimode,
                            trip_date AS TripDate,
                            comments AS Comments,
                            salesman AS Salesman,
                            route AS Route,
                            reversal_receipt_uid AS ReversalReceiptUid,
                            cancelled_on AS CancelledOn,
                            consolidated_receipt_number AS ConsolidatedReceiptNumber,
                            category AS Category,
                            amount AS Amount,
                            currency_uid AS CurrencyUid,
                            default_currency_uid AS DefaultCurrencyUid,
                            default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                            default_currency_amount AS DefaultCurrencyAmount,
                            org_uid AS OrgUid,
                            distribution_channel_uid AS DistributionChannelUid,
                            store_uid AS StoreUid,
                            route_uid AS RouteUid,
                            job_position_uid AS JobPositionUid,
                            emp_uid AS EmpUid,
                            collected_date AS CollectedDate,
                            status AS Status,
                            remarks AS Remarks,
                            reference_number AS ReferenceNumber
                        FROM 
                            acc_collection 
                        WHERE 
                            uid = @UID";

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICollections>().GetType();
                Model.Interfaces.ICollections CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.ICollections>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                return (ICollections)ex;
            }
        }
        public async Task<Model.Interfaces.ICollections> GetCollectAmount(string ReceiptNumber)
        {

            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"ReceiptNumber",  ReceiptNumber}
                };

                var sql = @"Select min(amount) from acc_collection Where receipt_number = @ReceiptNumber";

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICollections>().GetType();
                Model.Interfaces.ICollections CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.ICollections>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                return (ICollections)ex;
            }
        }
        public async Task<Model.Interfaces.IAccCollectionPaymentMode> GetPaymentAmount(string ReceiptNumber)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"AccCollectionUID",  ReceiptNumber}
                };

            var sql = @"SELECT 
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            acc_collection_uid AS AccCollectionUid,
                            bank_uid AS BankUid,
                            branch AS Branch,
                            cheque_no AS ChequeNo,
                            amount AS Amount,
                            currency_uid AS CurrencyUid,
                            default_currency_uid AS DefaultCurrencyUid,
                            default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                            default_currency_amount AS DefaultCurrencyAmount,
                            cheque_date AS ChequeDate,
                            status AS Status,
                            realization_date AS RealizationDate,
                            comments AS Comments,
                            approve_comments AS ApproveComments
                        FROM 
                            acc_collection_payment_mode 
                        WHERE 
                            acc_collection_uid = @AccCollectionUID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionPaymentMode>().GetType();
            Model.Interfaces.IAccCollectionPaymentMode CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IAccCollectionPaymentMode>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.IAccCollectionAllotment>> GetAllotmentAmount(string TargetUID, string AccCollectionUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"TargetUID",  TargetUID},
                    {"AccCollectionUID",  AccCollectionUID}
                };

            var sql = @"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, acc_collection_uid, 
                        target_type, target_uid, reference_number, amount, currency_uid, default_currency_uid, default_currency_exchange_rate, 
                        default_currency_amount, early_payment_discount_percentage, early_payment_discount_amount, early_payment_discount_reference_no
	                  FROM acc_collection_allotment Where acc_collection_uid = @AccCollectionUID 
                        and target_uid = @TargetUID and amount is not null 
                        Union
                        SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, acc_collection_uid, 
                        target_type, target_uid, reference_number, amount, currency_uid, default_currency_uid, default_currency_exchange_rate, 
                        default_currency_amount, early_payment_discount_percentage, early_payment_discount_amount, early_payment_discount_reference_no
	                FROM acc_collection_allotment Where target_uid like '%CN%'  and amount != 0) as table
                        Order By created_time Desc";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionAllotment>().GetType();
            IEnumerable<Model.Interfaces.IAccCollectionAllotment> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionAllotment>(sql, parameters, type);
            return CollectionModuleList;
        }

        public async Task<Model.Interfaces.IAccCollectionAllotment> GetAllotmentExcel(string TargetUID, string AccCollectionUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"TargetUID",  TargetUID},
                    {"AccCollectionUID",  AccCollectionUID}
                };

            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        acc_collection_uid AS AccCollectionUid,
                        target_type AS TargetType,
                        target_uid AS TargetUid,
                        reference_number AS ReferenceNumber,
                        amount AS Amount,
                        currency_uid AS CurrencyUid,
                        default_currency_uid AS DefaultCurrencyUid,
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                        default_currency_amount AS DefaultCurrencyAmount,
                        early_payment_discount_percentage AS EarlyPaymentDiscountPercentage,
                        early_payment_discount_amount AS EarlyPaymentDiscountAmount,
                        early_payment_discount_reference_no AS EarlyPaymentDiscountReferenceNo
                    FROM 
                        acc_collection_allotment 
                    WHERE 
                        target_uid = @TargetUID 
                        AND acc_collection_uid = @AccCollectionUID 
                    ORDER BY 
                        created_time";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionAllotment>().GetType();
            Model.Interfaces.IAccCollectionAllotment CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IAccCollectionAllotment>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.IAccCollectionAllotment>> AllInvoices(string AccCollectionUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"AccCollectionUID",  AccCollectionUID}
                };

            var sql = @"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, acc_collection_uid, 
                        target_type, target_uid, reference_number, amount, currency_uid, default_currency_uid, default_currency_exchange_rate,
                        default_currency_amount, early_payment_discount_percentage, early_payment_discount_amount, early_payment_discount_reference_no
	                FROM acc_collection_allotment Where acc_collection_uid = @AccCollectionUID 
                        and amount != 0 and remaining != 0
                        Union
                        SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, acc_collection_uid, 
                        target_type, target_uid, reference_number, amount, currency_uid, default_currency_uid, default_currency_exchange_rate,
                        default_currency_amount, early_payment_discount_percentage, early_payment_discount_amount, early_payment_discount_reference_no
	                FROM acc_collection_allotment where target_uid like '%CN%' and amount != 0) as table 
                        Order By created_time Desc";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionAllotment>().GetType();
            IEnumerable<Model.Interfaces.IAccCollectionAllotment> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionAllotment>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.IAccCollectionAllotment>> GetCashDetails(string AccCollectionUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"AccCollectionUID",  AccCollectionUID}
                };

            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        acc_collection_uid AS AccCollectionUid,
                        target_type AS TargetType,
                        target_uid AS TargetUid,
                        reference_number AS ReferenceNumber,
                        amount AS Amount,
                        currency_uid AS CurrencyUid,
                        default_currency_uid AS DefaultCurrencyUid,
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                        default_currency_amount AS DefaultCurrencyAmount,
                        early_payment_discount_percentage AS EarlyPaymentDiscountPercentage,
                        early_payment_discount_amount AS EarlyPaymentDiscountAmount,
                        early_payment_discount_reference_no AS EarlyPaymentDiscountReferenceNo
                    FROM 
                        acc_collection_allotment 
                    WHERE 
                        acc_collection_uid = @AccCollectionUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionAllotment>().GetType();
            IEnumerable<Model.Interfaces.IAccCollectionAllotment> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionAllotment>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<PagedResponse<Model.Interfaces.IAccCollection>> PaymentDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string AccCollectionUID)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT 
                                                id AS Id,
                                                uid AS Uid,
                                                created_by AS CreatedBy,
                                                created_time AS CreatedTime,
                                                modified_by AS ModifiedBy,
                                                modified_time AS ModifiedTime,
                                                server_add_time AS ServerAddTime,
                                                server_modified_time AS ServerModifiedTime,
                                                receipt_number AS ReceiptNumber,
                                                consolidated_receipt_number AS ConsolidatedReceiptNumber,
                                                category AS Category,
                                                store_uid AS StoreUid,
                                                is_reversal AS IsReversal,
                                                session_user_code AS SessionUserCode,
                                                reversed_by AS ReversedBy,
                                                paid_amount AS PaidAmount,
                                                cheque_no AS ChequeNo,
                                                cash_number AS CashNumber,
                                                settled_by AS SettledBy,
                                                is_settled AS IsSettled,
                                                is_void AS IsVoid
                                            FROM 
                                                acc_collection as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                                                id AS Id,
                                                uid AS Uid,
                                                created_by AS CreatedBy,
                                                created_time AS CreatedTime,
                                                modified_by AS ModifiedBy,
                                                modified_time AS ModifiedTime,
                                                server_add_time AS ServerAddTime,
                                                server_modified_time AS ServerModifiedTime,
                                                receipt_number AS ReceiptNumber,
                                                consolidated_receipt_number AS ConsolidatedReceiptNumber,
                                                category AS Category,
                                                store_uid AS StoreUid,
                                                is_reversal AS IsReversal,
                                                session_user_code AS SessionUserCode,
                                                reversed_by AS ReversedBy,
                                                paid_amount AS PaidAmount,
                                                cheque_no AS ChequeNo,
                                                cash_number AS CashNumber,
                                                settled_by AS SettledBy,
                                                is_settled AS IsSettled,
                                                is_void AS IsVoid
                                            FROM 
                                                acc_collection) as SubQuery");
                }
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"AccCollectionUID",  AccCollectionUID}
                };
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE is_reversal = true And cash_number Is Not null And ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                else
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE is_reversal = true And cash_number Is Not null");
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY created_time desc");
                    AppendSortCriteria(sortCriterias, sql);
                }
                else
                {
                    sql.Append(" ORDER BY created_time desc");
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
                IEnumerable<Model.Interfaces.IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql.ToString(), parameters, type);
                int totalCount = Const.Num;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> pagedResponse = new PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>
                {
                    PagedData = CollectionModuleList,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<IEnumerable<Model.Interfaces.IAccCollection>> SettledDetails(string AccCollectionUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"AccCollectionUID",  AccCollectionUID}
                };

            var sql = @"SELECT 
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            receipt_number AS ReceiptNumber,
                            consolidated_receipt_number AS ConsolidatedReceiptNumber,
                            category AS Category,
                            amount AS Amount,
                            currency_uid AS CurrencyUid,
                            default_currency_uid AS DefaultCurrencyUid,
                            default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                            default_currency_amount AS DefaultCurrencyAmount,
                            org_uid AS OrgUid,
                            distribution_channel_uid AS DistributionChannelUid,
                            store_uid AS StoreUid,
                            route_uid AS RouteUid,
                            job_position_uid AS JobPositionUid,
                            emp_uid AS EmpUid,
                            collected_date AS CollectedDate,
                            status AS Status,
                            remarks AS Remarks,
                            reference_number AS ReferenceNumber,
                            is_realized AS IsRealized,
                            latitude AS Latitude,
                            longitude AS Longitude,
                            source AS Source,
                            is_multimode AS IsMultimode,
                            trip_date AS TripDate,
                            comments AS Comments,
                            salesman AS Salesman,
                            route AS Route,
                            reversal_receipt_uid AS ReversalReceiptUid,
                            cancelled_on AS CancelledOn
                        FROM 
                            acc_collection 
                        WHERE 
                            status = @AccCollectionUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
            IEnumerable<Model.Interfaces.IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<PagedResponse<Model.Interfaces.IAccCollection>> CashierSettlement(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            var sql = new StringBuilder(@"SELECT 
                        uid AS Uid,
                        receipt_number AS ReceiptNumber,
                        status AS Status,
                        comments AS Comments,
                        store_uid AS StoreUid,
                        modified_time AS ModifiedTime,
                        category AS Category,
                        amount AS Amount,
                        default_currency_amount AS DefaultCurrencyAmount
                    FROM 
                        acc_collection  as SubQuery");
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"select COUNT(1) AS Cnt FROM (SELECT 
                        uid AS Uid,
                        receipt_number AS ReceiptNumber,
                        status AS Status,
                        comments AS Comments,
                        store_uid AS StoreUid,
                        modified_time AS ModifiedTime,
                        category AS Category,
                        amount AS Amount,
                        default_currency_amount AS DefaultCurrencyAmount
                    FROM 
                        acc_collection)  as SubQuery");
            }
            var parameters = new Dictionary<string, object?>();
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE status = 'Collected' And ");
                AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
                sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    sqlCount.Append(sbFilterCriteria);
                }
            }
            else
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE status = 'Collected'");
                sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    sqlCount.Append(sbFilterCriteria);
                }
            }
            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                sql.Append(" ORDER BY \"CreatedTime\" desc, ");
                AppendSortCriteria(sortCriterias, sql);
            }
            else
            {
                sql.Append(" ORDER BY created_time desc");
            }
            if (pageNumber > 0 && pageSize > 0)
            {
                sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            }
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
            IEnumerable<Model.Interfaces.IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql.ToString(), parameters, type);
            int totalCount = Const.Num;
            if (isCountRequired)
            {
                // Get the total count of records
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }
            PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> pagedResponse = new PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>
            {
                PagedData = CollectionModuleList,
                TotalCount = totalCount
            };
            return pagedResponse;
        }
        public async Task<PagedResponse<Model.Interfaces.IAccCollection>> CashierSettlementVoid(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            var sql = new StringBuilder(@"SELECT 
                    uid AS Uid,
                    receipt_number AS ReceiptNumber,
                    status AS Status,
                    comments AS Comments,
                    store_uid AS StoreUid,
                    modified_time AS ModifiedTime,
                    category AS Category,
                    amount AS Amount,
                    default_currency_amount AS DefaultCurrencyAmount
                FROM 
                    acc_collection  as SubQuery");
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"select COUNT(1) AS Cnt FROM (SELECT 
                    uid AS Uid,
                    receipt_number AS ReceiptNumber,
                    status AS Status,
                    comments AS Comments,
                    store_uid AS StoreUid,
                    modified_time AS ModifiedTime,
                    category AS Category,
                    amount AS Amount,
                    default_currency_amount AS DefaultCurrencyAmount
                FROM 
                    acc_collection) as SubQuery");
            }
            var parameters = new Dictionary<string, object?>();
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE status = 'Voided' And ");
                AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
                sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    sqlCount.Append(sbFilterCriteria);
                }
            }
            else
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE status = 'Voided'");
                sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    sqlCount.Append(sbFilterCriteria);
                }
            }
            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                sql.Append(" ORDER BY created_time desc, ");
                AppendSortCriteria(sortCriterias, sql);
            }
            else
            {
                sql.Append(" ORDER BY created_time desc");
            }
            if (pageNumber > 0 && pageSize > 0)
            {
                sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            }
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
            IEnumerable<Model.Interfaces.IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql.ToString(), parameters, type);
            int totalCount = Const.Num;
            if (isCountRequired)
            {
                // Get the total count of records
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }
            PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> pagedResponse = new PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>
            {
                PagedData = CollectionModuleList,
                TotalCount = totalCount
            };
            return pagedResponse;
        }
        public async Task<PagedResponse<Model.Interfaces.IAccCollection>> CashierSettlementSettled(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            var sql = new StringBuilder(@"SELECT 
                            uid AS Uid,
                            receipt_number AS ReceiptNumber,
                            status AS Status,
                            comments AS Comments,
                            store_uid AS StoreUid,
                            modified_time AS ModifiedTime,
                            category AS Category,
                            amount AS Amount,
                            default_currency_amount AS DefaultCurrencyAmount
                        FROM 
                            acc_collection  as SubQuery");
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"select COUNT(1) AS Cnt FROM (SELECT 
                            uid AS Uid,
                            receipt_number AS ReceiptNumber,
                            status AS Status,
                            comments AS Comments,
                            store_uid AS StoreUid,
                            modified_time AS ModifiedTime,
                            category AS Category,
                            amount AS Amount,
                            default_currency_amount AS DefaultCurrencyAmount
                        FROM 
                            acc_collection)  as SubQuery");
            }
            var parameters = new Dictionary<string, object?>();
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE status = 'Reversed' OR status = 'Settled' And");
                AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
                sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    sqlCount.Append(sbFilterCriteria);
                }
            }
            else
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE status = 'Reversed' OR status = 'Settled' ");
                sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    sqlCount.Append(sbFilterCriteria);
                }
            }
            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                sql.Append(" ORDER BY createdTime desc, ");
                AppendSortCriteria(sortCriterias, sql);
            }
            else
            {
                sql.Append(" ORDER BY created_time desc");
            }
            if (pageNumber > 0 && pageSize > 0)
            {
                sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            }
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
            IEnumerable<Model.Interfaces.IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql.ToString(), parameters, type);
            int totalCount = Const.Num;
            if (isCountRequired)
            {
                // Get the total count of records
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }
            PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> pagedResponse = new PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>
            {
                PagedData = CollectionModuleList,
                TotalCount = totalCount
            };
            return pagedResponse;
        }
        public async Task<List<IAccCollection>> PaymentReceipts(string FromDate, string ToDate, string Payment, string Print = "")
        {
            Dictionary<string, object?> parameters;
            Dictionary<string, object?> parameters1;

            DateTime From = DateTime.Parse(FromDate);
            DateTime To = DateTime.Parse(ToDate);
            string from = From.ToString("yyyy-MM-dd hh:mm:ss");
            string to = To.ToString("yyyy-MM-dd hh:mm:ss");
            parameters = new Dictionary<string, object?>
                {
                    {"FromDate",  from},
                    {"ToDate",  to},
                    {"Payment",  Payment},
                };
            parameters1 = new Dictionary<string, object?>
                {
                    {"UID",  Payment},
                };

            var sql = @"SELECT 
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            receipt_number AS ReceiptNumber,
                            consolidated_receipt_number AS ConsolidatedReceiptNumber,
                            category AS Category,
                            amount AS Amount,
                            currency_uid AS CurrencyUid,
                            default_currency_uid AS DefaultCurrencyUid,
                            default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                            default_currency_amount AS DefaultCurrencyAmount,
                            org_uid AS OrgUid,
                            distribution_channel_uid AS DistributionChannelUid,
                            store_uid AS StoreUid,
                            route_uid AS RouteUid,
                            job_position_uid AS JobPositionUid,
                            emp_uid AS EmpUid,
                            collected_date AS CollectedDate,
                            status AS Status,
                            remarks AS Remarks,
                            reference_number AS ReferenceNumber,
                            is_realized AS IsRealized,
                            latitude AS Latitude,
                            longitude AS Longitude,
                            source AS Source,
                            is_multimode AS IsMultimode,
                            trip_date AS TripDate,
                            comments AS Comments,
                            salesman AS Salesman,
                            route AS Route,
                            reversal_receipt_uid AS ReversalReceiptUid,
                            cancelled_on AS CancelledOn
                        FROM 
                            acc_collection 
                        WHERE 
                            Date(created_time) BETWEEN Date(@FromDate) AND Date(@ToDate)";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
            List<Model.Interfaces.IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<List<IAccCollectionAllotment>> AllotmentReceipts(string AccCollectionUID)
        {
            Dictionary<string, object?> parameters;
            parameters = new Dictionary<string, object?>
                {
                    {"AccCollectionUID",  AccCollectionUID},
                };


            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        acc_collection_uid AS AccCollectionUid,
                        target_type AS TargetType,
                        target_uid AS TargetUid,
                        reference_number AS ReferenceNumber,
                        amount AS Amount,
                        currency_uid AS CurrencyUid,
                        default_currency_uid AS DefaultCurrencyUid,
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                        default_currency_amount AS DefaultCurrencyAmount,
                        early_payment_discount_percentage AS EarlyPaymentDiscountPercentage,
                        early_payment_discount_amount AS EarlyPaymentDiscountAmount,
                        early_payment_discount_reference_no AS EarlyPaymentDiscountReferenceNo
                    FROM 
                        acc_collection_allotment 
                    WHERE 
                        acc_collection_uid = @AccCollectionUID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionAllotment>().GetType();
            List<Model.Interfaces.IAccCollectionAllotment> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionAllotment>(sql, parameters, type);
            return CollectionModuleList;


        }

        public async Task<PagedResponse<Model.Interfaces.IAccCollectionAllotment>> ShowPaymentDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT 
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            acc_collection_uid AS AccCollectionUid,
                            target_type AS TargetType,
                            target_uid AS TargetUid,
                            reference_number AS ReferenceNumber,
                            amount AS Amount,
                            currency_uid AS CurrencyUid,
                            default_currency_uid AS DefaultCurrencyUid
                        FROM 
                            acc_collection_allotment  as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"select COUNT(1) AS Cnt FROM (SELECT 
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            acc_collection_uid AS AccCollectionUid,
                            target_type AS TargetType,
                            target_uid AS TargetUid,
                            reference_number AS ReferenceNumber,
                            amount AS Amount,
                            currency_uid AS CurrencyUid,
                            default_currency_uid AS DefaultCurrencyUid
                        FROM 
                            acc_collection_allotment)  as SubQuery");
                }
                var parameters = new Dictionary<string, object?>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY created_time desc");
                    AppendSortCriteria(sortCriterias, sql);
                }
                else
                {
                    sql.Append(" ORDER BY created_time desc");
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionAllotment>().GetType();
                IEnumerable<Model.Interfaces.IAccCollectionAllotment> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionAllotment>(sql.ToString(), parameters, type);
                int totalCount = Const.Num;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionAllotment> pagedResponse = new PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionAllotment>
                {
                    PagedData = CollectionModuleList,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<IEnumerable<Model.Interfaces.IAccCollectionAllotment>> ReceivUnsettle(string UID, string ChequeNo)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                 {"AccCollectionUID",  UID}
            };

            var sql = @"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, acc_collection_uid, 
                        target_type, target_uid, reference_number, amount, currency_uid, default_currency_uid, default_currency_exchange_rate, 
                        default_currency_amount, early_payment_discount_percentage, early_payment_discount_amount, early_payment_discount_reference_no
	                FROM acc_collection_allotment AL 
                        INNER JOIN acc_collection AC on AC.uid = AL.acc_collection_uid where AL.acc_collection_uid = @AccCollectionUID AND target_type LIKE '%INVOICE%'";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionAllotment>().GetType();
            IEnumerable<Model.Interfaces.IAccCollectionAllotment> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionAllotment>(sql, parameters, type);
            return CollectionModuleList;
        }

        public async Task<IEnumerable<Model.Interfaces.IAccPayable>> PayableUnsettle(string AccCollectionUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                 {"AccCollectionUID",  AccCollectionUID}
            };

            var sql = @"SELECT AP.id, AP.uid, AP.created_by, AP.created_time, AP.modified_by, AP.modified_time, AP.server_add_time, AP.server_modified_time,
                AP.acc_collection_uid, AP.target_type, AP.target_uid, AP.reference_number, AP.amount, AP.currency_uid, AP.default_currency_uid,
                AP.default_currency_exchange_rate, AP.default_currency_amount, AP.early_payment_discount_percentage, AP.early_payment_discount_amount,
                AP.early_payment_discount_reference_no
                FROM acc_collection_allotment AS AL 
                INNER JOIN acc_payable AS AP ON AL.target_uid = AP.uid 
                WHERE AL.acc_collection_uid = @AccCollectionUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
            IEnumerable<Model.Interfaces.IAccPayable> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql, parameters, type);
            return CollectionModuleList;
        }
        //getting cheque records for reversal
        public async Task<IEnumerable<Model.Interfaces.IAccCollectionAllotment>> GetCashAllot(string AccCollectionUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                 {"AccCollectionUID",  AccCollectionUID}
            };

            var sql = @"SELECT 
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            acc_collection_uid AS AccCollectionUid,
                            target_type AS TargetType,
                            target_uid AS TargetUid,
                            reference_number AS ReferenceNumber,
                            amount AS Amount,
                            currency_uid AS CurrencyUid,
                            default_currency_uid AS DefaultCurrencyUid,
                            default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                            default_currency_amount AS DefaultCurrencyAmount,
                            early_payment_discount_percentage AS EarlyPaymentDiscountPercentage,
                            early_payment_discount_amount AS EarlyPaymentDiscountAmount,
                            early_payment_discount_reference_no AS EarlyPaymentDiscountReferenceNo
                        FROM 
                            acc_collection_allotment 
                        WHERE 
                            acc_collection_uid = @AccCollectionUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionAllotment>().GetType();
            IEnumerable<Model.Interfaces.IAccCollectionAllotment> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionAllotment>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.IAccPayable>> GetChequePay(string AccCollectionUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                 {"AccCollectionUID",  AccCollectionUID}
            };

            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        source_type AS SourceType,
                        source_uid AS SourceUid,
                        reference_number AS ReferenceNumber,
                        org_uid AS OrgUid,
                        job_position_uid AS JobPositionUid,
                        amount AS Amount,
                        paid_amount AS PaidAmount,
                        store_uid AS StoreUid,
                        transaction_date AS TransactionDate,
                        due_date AS DueDate,
                        balance_amount AS BalanceAmount,
                        unsettled_amount AS UnsettledAmount,
                        source AS Source,
                        currency_uid AS CurrencyUid
                    FROM 
                        acc_payable 
                    WHERE 
                        uid = @AccCollectionUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
            IEnumerable<Model.Interfaces.IAccPayable> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.IAccReceivable>> GetChequeRec(string DocumentType)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                 {"DocumentType",  DocumentType}
            };

            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        source_type AS SourceType,
                        source_uid AS SourceUid,
                        reference_number AS ReferenceNumber,
                        org_uid AS OrgUid,
                        job_position_uid AS JobPositionUid,
                        currency_uid AS CurrencyUid,
                        amount AS Amount,
                        paid_amount AS PaidAmount,
                        store_uid AS StoreUid,
                        transaction_date AS TransactionDate,
                        due_date AS DueDate,
                        unsettled_amount AS UnsettledAmount,
                        balance_amount AS BalanceAmount,
                        source AS Source
                    FROM 
                        acc_receivable 
                    WHERE 
                        uid = @DocumentType";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccReceivable>().GetType();
            IEnumerable<Model.Interfaces.IAccReceivable> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccReceivable>(sql, parameters, type);
            return CollectionModuleList;
        }
        //getting cheque records for reversal


        //getting records to update balance in AccStoreLedger
        public async Task<Model.Interfaces.IAccStoreLedger> GetStoreLedger(string ReceiptNumber)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"SourceUID",  ReceiptNumber}
                };

            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        source_type AS SourceType,
                        source_uid AS SourceUid,
                        credit_type AS CreditType,
                        org_uid AS OrgUid,
                        store_uid AS StoreUid,
                        default_currency_uid AS DefaultCurrencyUid,
                        document_number AS DocumentNumber,
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                        default_currency_amount AS DefaultCurrencyAmount,
                        transaction_date_time AS TransactionDateTime,
                        collected_amount AS CollectedAmount,
                        currency_uid AS CurrencyUid,
                        amount AS Amount,
                        balance AS Balance,
                        comments AS Comments
                    FROM 
                        acc_store_ledger 
                    WHERE 
                        source_uid = @SourceUID";


            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccStoreLedger>().GetType();
            Model.Interfaces.IAccStoreLedger CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IAccStoreLedger>(sql, parameters, type);
            return CollectionModuleList;

        }

        //to get payable details and update in payable table
        public async Task<Model.Interfaces.IAccPayable> GetAccPayableAmount(string StoreUID, string ReferenceNumber)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"StoreUID",  StoreUID},
                    {"ReferenceNumber",  ReferenceNumber}
                };

            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        source_type AS SourceType,
                        source_uid AS SourceUid,
                        reference_number AS ReferenceNumber,
                        org_uid AS OrgUid,
                        job_position_uid AS JobPositionUid,
                        amount AS Amount,
                        paid_amount AS PaidAmount,
                        store_uid AS StoreUid,
                        transaction_date AS TransactionDate,
                        due_date AS DueDate,
                        balance_amount AS BalanceAmount,
                        unsettled_amount AS UnsettledAmount,
                        source AS Source,
                        currency_uid AS CurrencyUid
                    FROM 
                        acc_payable 
                    WHERE 
                        store_uid = @StoreUID 
                        AND reference_number = @ReferenceNumber";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
            Model.Interfaces.IAccPayable CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IAccPayable>(sql, parameters, type);
            return CollectionModuleList;
        }
        //get records for auto allocate 
        public async Task<IEnumerable<Model.Interfaces.IAccPayable>> GetInvoicesAutoAllocate(string StoreUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"StoreUID",  StoreUID},
                };
            var sql = @"SELECT 
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            source_type AS SourceType,
                            source_uid AS SourceUid,
                            reference_number AS ReferenceNumber,
                            org_uid AS OrgUid,
                            job_position_uid AS JobPositionUid,
                            amount AS Amount,
                            paid_amount AS PaidAmount,
                            store_uid AS StoreUid,
                            transaction_date AS TransactionDate,
                            due_date AS DueDate,
                            balance_amount AS BalanceAmount,
                            unsettled_amount AS UnsettledAmount,
                            source AS Source,
                            currency_uid AS CurrencyUid
                        FROM 
                            acc_payable 
                        WHERE 
                            source_uid = @StoreUID 
                        ORDER BY 
                            transaction_date";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
            IEnumerable<Model.Interfaces.IAccPayable> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.IAccPayable>> GetInvoicesAutoAllocateByUID(int Start, int End, string StoreUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"startDay",  Start},
                    {"endDay",  End},
                    {"StoreUID",  StoreUID},
                };

            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        source_type AS SourceType,
                        source_uid AS SourceUid,
                        reference_number AS ReferenceNumber,
                        org_uid AS OrgUid,
                        job_position_uid AS JobPositionUid,
                        amount AS Amount,
                        paid_amount AS PaidAmount,
                        store_uid AS StoreUid,
                        transaction_date AS TransactionDate,
                        due_date AS DueDate,
                        balance_amount AS BalanceAmount,
                        unsettled_amount AS UnsettledAmount,
                        source AS Source,
                        currency_uid AS CurrencyUid
                    FROM 
                        acc_payable 
                    WHERE 
                        store_uid = @StoreUID
                        AND CURRENT_DATE - transaction_date BETWEEN @startDay AND @endDay 
                    ORDER BY 
                        transaction_date";

            var sql1 = @"SELECT 
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            source_type AS SourceType,
                            source_uid AS SourceUid,
                            reference_number AS ReferenceNumber,
                            org_uid AS OrgUid,
                            job_position_uid AS JobPositionUid,
                            amount AS Amount,
                            paid_amount AS PaidAmount,
                            store_uid AS StoreUid,
                            transaction_date AS TransactionDate,
                            due_date AS DueDate,
                            balance_amount AS BalanceAmount,
                            unsettled_amount AS UnsettledAmount,
                            source AS Source,
                            currency_uid AS CurrencyUid
                        FROM 
                            acc_payable 
                        WHERE 
                            store_uid = @StoreUID
                            AND CURRENT_DATE - transaction_date >= @startDay 
                        ORDER BY 
                            transaction_date";


            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
            IEnumerable<Model.Interfaces.IAccPayable> CollectionModuleList;
            if (End == 0)
            {
                CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql1, parameters, type);
            }
            else
            {
                CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql, parameters, type);
            }
            return CollectionModuleList;
        }

        //getting data to update receive table details
        public async Task<Model.Interfaces.IAccReceivable> GetAccRecAmount(string StoreUID, string ReferenceNumber)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"StoreUID",  StoreUID},
                    {"ReferenceNumber",  ReferenceNumber},
                };

            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        source_type AS SourceType,
                        source_uid AS SourceUid,
                        reference_number AS ReferenceNumber,
                        org_uid AS OrgUid,
                        job_position_uid AS JobPositionUid,
                        currency_uid AS CurrencyUid,
                        amount AS Amount,
                        paid_amount AS PaidAmount,
                        store_uid AS StoreUid,
                        transaction_date AS TransactionDate,
                        due_date AS DueDate,
                        unsettled_amount AS UnsettledAmount,
                        balance_amount AS BalanceAmount,
                        source AS Source
                    FROM 
                        acc_receivable 
                    WHERE 
                        store_uid = @StoreUID 
                        AND reference_number = @ReferenceNumber";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccReceivable>().GetType();
            Model.Interfaces.IAccReceivable CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IAccReceivable>(sql, parameters, type);
            return CollectionModuleList;
        }

        //End

        //Collection Module
        public async Task<IEnumerable<Model.Interfaces.ICollectionModule>> GetAllOutstandingTransactionsByCustomerCode(string CustomerCode, string SessionUserCode = null, string SalesOrgCode = null)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"CustomerCode",  CustomerCode}
            };

            var sql = @"SELECT un_settled_amount, target_uid ,CONCAT(S.alias_name, '(', S.code, ')') AS code_name, Ap.uid, amount, paid_amount, store_uid, transaction_date, due_date, balance_amount
                        FROM acc_payable Ap 
                        INNER JOIN store"" S ON S.uid = Ap.store_uid
                         WHERE S.uid = @CustomerCode
                        UNION 
                        SELECT  un_settled_amount,target_uid,CONCAT(S.alias_name, '(', S.code, ')') AS code_name, Ar.uid, amount, paid_amount, store_uid, transaction_date, due_date, balance_amount
                        FROM acc_receivable"" Ar
                        INNER JOIN store S ON S.uid = Ar.store_uid
						WHERE S.uid = @CustomerCode";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICollectionModule>().GetType();
            IEnumerable<Model.Interfaces.ICollectionModule> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.ICollectionModule>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.IExchangeRate>> GetAllConfiguredCurrencyDetailsBySalesOrgCode(string SessionUserCode, string SalesOrgCode = null)
        {


            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                //{"SourceType",  SessionUserCode},
                {"UID",  SessionUserCode}
            };

            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        from_currency_uid AS FromCurrencyUid,
                        to_currency_uid AS ToCurrencyUid,
                        rate AS Rate,
                        effective_date AS EffectiveDate,
                        is_active AS IsActive,
                        source AS Source
                    FROM 
                        exchange_rate";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IExchangeRate>().GetType();
            IEnumerable<Model.Interfaces.IExchangeRate> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IExchangeRate>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.IAccCollectionAllotment>> GetAllConfiguredDocumentTypesBySalesOrgCode(string SessionUserCode, string SalesOrgCode = null)
        {


            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                //{"SourceType",  SessionUserCode},
                {"AccCollectionUID",  SessionUserCode}
            };
            var sql = @"Select Distinct target_uid from acc_collection_allotment Where acc_collection_uid = @AccCollectionUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionAllotment>().GetType();
            IEnumerable<Model.Interfaces.IAccCollectionAllotment> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionAllotment>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.IAccCollection>> GetAllConfiguredPaymentModesBySalesOrgCode(string SessionUserCode, string SalesOrgCode = null)
        {


            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                //{"SourceType",  SessionUserCode},
                {"StoreUID",  SessionUserCode}
            };

            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        source_type AS SourceType,
                        source_uid AS SourceUid,
                        reference_number AS ReferenceNumber,
                        org_uid AS OrgUid,
                        job_position_uid AS JobPositionUid,
                        amount AS Amount,
                        paid_amount AS PaidAmount,
                        store_uid AS StoreUid,
                        transaction_date AS TransactionDate,
                        due_date AS DueDate,
                        balance_amount AS BalanceAmount,
                        unsettled_amount AS UnsettledAmount,
                        source AS Source,
                        currency_uid AS CurrencyUid
                    FROM 
                        acc_payable
                    WHERE 
                        store_uid = @StoreUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
            IEnumerable<Model.Interfaces.IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore>> GetAllCustomersBySalesOrgCode(string SessionUserCode, string SalesOrgCode = null)
        {


            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                //{"SourceType",  SessionUserCode},
                {"StoreUID",  SessionUserCode}
            };

            var sql = @"SELECT uid, '[' || code || ']' || name   AS Name, Code   
                        FROM store";

            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Store.Model.Interfaces.IStore>().GetType();
            IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore> CollectionModuleList = await ExecuteQueryAsync<Winit.Modules.Store.Model.Interfaces.IStore>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.ICollectionModule>> GetAllOutstandingTransactionsByMultipleFilters(string SessionUserCode, string SalesOrgCode = null, string CustomerCode = null, string StartDueDate = null, string EndDueDate = null, string StartInvoiceDate = null, string EndInvoiceDate = null)
        {


            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                //{"SourceType",  SessionUserCode},
                {"StoreUID",  SessionUserCode}
            };

            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        receipt_number AS ReceiptNumber,
                        consolidated_receipt_number AS ConsolidatedReceiptNumber,
                        category AS Category,
                        amount AS Amount,
                        currency_uid AS CurrencyUid,
                        default_currency_uid AS DefaultCurrencyUid,
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                        default_currency_amount AS DefaultCurrencyAmount,
                        org_uid AS OrgUid,
                        distribution_channel_uid AS DistributionChannelUid,
                        store_uid AS StoreUid,
                        route_uid AS RouteUid,
                        job_position_uid AS JobPositionUid,
                        emp_uid AS EmpUid,
                        collected_date AS CollectedDate,
                        status AS Status,
                        remarks AS Remarks,
                        reference_number AS ReferenceNumber,
                        is_realized AS IsRealized,
                        latitude AS Latitude,
                        longitude AS Longitude,
                        source AS Source,
                        is_multimode AS IsMultimode,
                        trip_date AS TripDate,
                        comments AS Comments,
                        salesman AS Salesman,
                        route AS Route,
                        reversal_receipt_uid AS ReversalReceiptUid,
                        cancelled_on AS CancelledOn
                    FROM 
                        acc_collection 
                    WHERE 
                        store_uid = @StoreUID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICollectionModule>().GetType(); ;
            IEnumerable<Model.Interfaces.ICollectionModule> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.ICollectionModule>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.ICollectionModule>> AllocateSelectedInvoiceswithCreditNotes(string SessionUserCode, string TrxCode = null, string TrxType = null, string PaidAmount = null)
        {


            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                //{"SourceType",  TrxCode},
                {"StoreUID",  SessionUserCode}
            };

            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        receipt_number AS ReceiptNumber,
                        consolidated_receipt_number AS ConsolidatedReceiptNumber,
                        category AS Category,
                        amount AS Amount,
                        currency_uid AS CurrencyUid,
                        default_currency_uid AS DefaultCurrencyUid,
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                        default_currency_amount AS DefaultCurrencyAmount,
                        org_uid AS OrgUid,
                        distribution_channel_uid AS DistributionChannelUid,
                        store_uid AS StoreUid,
                        route_uid AS RouteUid,
                        job_position_uid AS JobPositionUid,
                        emp_uid AS EmpUid,
                        collected_date AS CollectedDate,
                        status AS Status,
                        remarks AS Remarks,
                        reference_number AS ReferenceNumber,
                        is_realized AS IsRealized,
                        latitude AS Latitude,
                        longitude AS Longitude,
                        source AS Source,
                        is_multimode AS IsMultimode,
                        trip_date AS TripDate,
                        comments AS Comments,
                        salesman AS Salesman,
                        route AS Route,
                        reversal_receipt_uid AS ReversalReceiptUid,
                        cancelled_on AS CancelledOn
                    FROM 
                        acc_collection 
                    WHERE 
                        store_uid = @StoreUID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICollectionModule>().GetType();
            IEnumerable<Model.Interfaces.ICollectionModule> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.ICollectionModule>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.ICollectionModule>> GetOrgwiseConfigurationsData(string SessionUserCode, string OrgUID = null)
        {


            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                //{"StoreUID",  SessionUserCode},
                {"StoreUID",  SessionUserCode}
            };

            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        receipt_number AS ReceiptNumber,
                        consolidated_receipt_number AS ConsolidatedReceiptNumber,
                        category AS Category,
                        amount AS Amount,
                        currency_uid AS CurrencyUid,
                        default_currency_uid AS DefaultCurrencyUid,
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                        default_currency_amount AS DefaultCurrencyAmount,
                        org_uid AS OrgUid,
                        distribution_channel_uid AS DistributionChannelUid,
                        store_uid AS StoreUid,
                        route_uid AS RouteUid,
                        job_position_uid AS JobPositionUid,
                        emp_uid AS EmpUid,
                        collected_date AS CollectedDate,
                        status AS Status,
                        remarks AS Remarks,
                        reference_number AS ReferenceNumber,
                        is_realized AS IsRealized,
                        latitude AS Latitude,
                        longitude AS Longitude,
                        source AS Source,
                        is_multimode AS IsMultimode,
                        trip_date AS TripDate,
                        comments AS Comments,
                        salesman AS Salesman,
                        route AS Route,
                        reversal_receipt_uid AS ReversalReceiptUid,
                        cancelled_on AS CancelledOn
                    FROM 
                        acc_collection 
                    WHERE 
                        store_uid = @StoreUID";


            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICollectionModule>().GetType(); ;
            IEnumerable<Model.Interfaces.ICollectionModule> CollectionList = await ExecuteQueryAsync<Model.Interfaces.ICollectionModule>(sql, parameters, type);
            return CollectionList;

        }
        public async Task<IEnumerable<Model.Interfaces.ICollectionModule>> GetOrgwiseConfigValueByConfigName(string SessionUserCode, string OrgUID = null, string Configname = null)
        {


            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                //{"StoreUID",  SessionUserCode},
                {"StoreUID",  SessionUserCode}
            };

            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        receipt_number AS ReceiptNumber,
                        consolidated_receipt_number AS ConsolidatedReceiptNumber,
                        category AS Category,
                        amount AS Amount,
                        currency_uid AS CurrencyUid,
                        default_currency_uid AS DefaultCurrencyUid,
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                        default_currency_amount AS DefaultCurrencyAmount,
                        org_uid AS OrgUid,
                        distribution_channel_uid AS DistributionChannelUid,
                        store_uid AS StoreUid,
                        route_uid AS RouteUid,
                        job_position_uid AS JobPositionUid,
                        emp_uid AS EmpUid,
                        collected_date AS CollectedDate,
                        status AS Status,
                        remarks AS Remarks,
                        reference_number AS ReferenceNumber,
                        is_realized AS IsRealized,
                        latitude AS Latitude,
                        longitude AS Longitude,
                        source AS Source,
                        is_multimode AS IsMultimode,
                        trip_date AS TripDate,
                        comments AS Comments,
                        salesman AS Salesman,
                        route AS Route,
                        reversal_receipt_uid AS ReversalReceiptUid,
                        cancelled_on AS CancelledOn
                    FROM 
                        acc_collection 
                    WHERE 
                        store_uid = @StoreUID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICollectionModule>().GetType(); ;
            IEnumerable<Model.Interfaces.ICollectionModule> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.ICollectionModule>(sql, parameters, type);
            return CollectionModuleList;
        }


        public async Task<string> CreateReceipt(Model.Interfaces.ICollections[] collectionData)
        {
            int count = 0;

            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        List<IAccPayable> accPayables = new List<IAccPayable>();
                        foreach (var collection in collectionData)
                        {
                            var val = "";
                            var accCollec = await InsertAccCollection(conn, transaction, collection);

                            var accPaymode = await InsertAccCollectionPaymentMode(conn, transaction, collection);

                            var accStore = await InsertAccStoreLedger(conn, transaction, collection);

                            //var accPay = await UpdateAccPayable(conn, transaction, collection);
                            foreach (var acc in collection.AccPayable)
                            {
                                IAccPayable? accPayable = accPayables.Find(e => e.ReferenceNumber == acc.ReferenceNumber);
                                if (accPayable == null)
                                {
                                    if (collection.AccCollection.Category != Const.Cash)
                                    {
                                        acc.UnSettledAmount += acc.PaidAmount;
                                        acc.PaidAmount = 0;
                                    }
                                    accPayables.Add(acc);
                                    continue;
                                }
                                if (collection.AccCollection.Category == Const.Cash)
                                {
                                    accPayable.PaidAmount += acc.PaidAmount;
                                }
                                else
                                {
                                    accPayable.UnSettledAmount += acc.PaidAmount;
                                }
                            }

                            var accRec = await UpdateAccReceivable(conn, transaction, collection);

                            var accAllot = await InsertAccCollectionAllotment(conn, transaction, collection);

                            var accCollectionCurrencyData = await InsertAccCollectionCurrencyDetails(conn, transaction, collection);

                            if (collection.AccCollection.IsEarlyPayment)
                            {
                                //var Payable = await InsertAccPayable(conn, transaction, collection);
                                if (collection.AccCollection.Category == "Cash")
                                {
                                    var Receive = await InsertAccReceivable(conn, transaction, collection);
                                }

                                var accEarly = await InsertEarlyPaymentDiscountAppliedDetails(conn, transaction, collection);

                                if (accCollec != Const.One || accPaymode != Const.One || accStore != Const.One || accAllot != Const.One ||
                                     accEarly != Const.One)
                                {
                                    throw new();
                                }
                            }
                            if (accCollec != Const.One || accPaymode != Const.One || accStore != Const.One || accAllot != Const.One)
                            {
                                throw new();
                            }
                        }
                        await UpdateAccPayable(accPayables, transaction, conn);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return ex.Message.ToString();
                    }
                }
            }
            return "Success";
        }
        public async Task<string> UpdateAccAllotment(SqliteConnection conn, SqliteTransaction transaction, ICollections collection, string AccCollectionUID, string TargetUID, decimal Remaining, decimal extra)
        {
            var Retval = "";
            IAccCollectionAllotment accCollectionAllotment = await GetAllotmentExcel(TargetUID, AccCollectionUID);
            var sql4 = @"UPDATE acc_collection_allotment
                                SET remaining =@Remaining  WHERE acc_collection_uid = @AccCollectionUID and target_uid = @TargetUID and amount !=0 ;";
            Dictionary<string, object?> parameters4 = new Dictionary<string, object?>
            {
                        {"AccCollectionUID",  AccCollectionUID},
                        {"TargetUID",  TargetUID},
                        {"Remaining",  Remaining}
                    };
            var sql5 = @"UPDATE ""AccCollectionAllotment""
                                SET ""Remaining"" =@Remaining  WHERE ""UID"" = @UID ;";
            Dictionary<string, object?> parameters5 = new Dictionary<string, object?>
            {
                        {"AccCollectionUID",  AccCollectionUID},
                        {"TargetUID",  TargetUID},
                        {"Remaining",  Remaining},
                        {"UID",  accCollectionAllotment.UID}
                    };
            Retval = collection.AccCollection.Excel == true ? await CommonMethod(sql5, conn, parameters5, transaction) : await CommonMethod(sql4, conn, parameters4, transaction);
            if (Retval != Const.One)
                return Retval;
            else
                return Retval;
        }
        public async Task<string> CreateReceiptWithZeroValue(Model.Interfaces.ICollections collection)
        {
            //using (var conn = new SqliteConnection(_connectionString))
            //{
            //    await conn.OpenAsync();
            //    using (var transaction = conn.BeginTransaction())
            //    {
            //        if (collection != null)
            //        {
            //            if (collection.UID != null)
            //            {
            //                decimal sum = 0;
            //                ICollections accCollection = await GetAmount(collection.ReceiptNumber);
            //                IAccCollectionPaymentMode accCollectionPayment = await GetPaymentAmount(collection.ReceiptNumber);
            //                IEnumerable<IAccCollectionAllotment> accCollectionAllotments = await GetAllotmentAmount(collection.ReceiptNumber);
            //                if (!accCollectionAllotments.Any() || accCollectionPayment == null || accCollection == null)
            //                {
            //                    return "Data not present";
            //                }
            //                else
            //                {
            //                    foreach (var acccollection in accCollectionAllotments)
            //                    {
            //                        sum += acccollection.Amount - acccollection.PaidAmount;
            //                    }
            //                }
            //                if (accCollection.Amount == sum)
            //                {
            //                    var val = "";
            //                    var accCollec = await InsertAccCollection(conn, transaction, collection, accCollection);
            //                    if (accCollec != Const.One)
            //                        return accCollec;
            //                    var accPaymode = await InsertAccCollectionPaymentMode(conn, transaction, collection, accCollectionPayment);
            //                    if (accPaymode != Const.One)
            //                        return accPaymode;
            //                    var accStore = await InsertAccStoreLedger(conn, transaction, collection);
            //                    if (accStore != Const.One)
            //                        return accStore;
            //                    val = await GetData(collection);
            //                    if (val == Const.One)
            //                    {
            //                        var accPay = await UpdateAccPayable(conn, transaction, collection);
            //                        var accRec = await UpdateAccReceivable(conn, transaction, collection);
            //                        if (accPay == Const.One && accRec == Const.One)
            //                        {
            //                            var accAllot = await InsertAccCollectionAllotment(conn, transaction, collection);
            //                            if (accAllot == Const.One)
            //                            {
            //                                transaction.Commit();
            //                                return Const.SuccessInsert;
            //                            }
            //                            else
            //                            {
            //                                transaction.Rollback();
            //                                return accAllot;
            //                            }
            //                        }
            //                        else
            //                        {
            //                            return Const.NotFound;
            //                        }
            //                    }
            //                    else
            //                    {
            //                        transaction.Rollback();
            //                        return "Amount Variance";
            //                    }
            //                }
            //                else
            //                {
            //                    transaction.Rollback();
            //                    return Const.MisMatch;
            //                }
            //            }
            //            else
            //            {
            //                transaction.Rollback();
            //                return Const.ParamMissing;
            //            }
            //        }
            //        else
            //        {
            //            transaction.Rollback();
            //            return Const.ParamMissing;
            //        }
            //    }
            //}
            return "";
        }
        public async Task<int> CreateReceiptWithAutoAllocation(Model.Interfaces.ICollections collection)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    decimal enteredAmount = 0;
                    decimal balanceAmount = 0;
                    decimal arrayAmount = 0;
                    decimal extra = 0;
                    var Retval = "";
                    var val = "";
                    var accCollec = await InsertAccCollection(conn, transaction, collection);
                    if (accCollec != Const.One)
                        return 0;
                    var accPaymode = await InsertAccCollectionPaymentMode(conn, transaction, collection);
                    if (accPaymode != Const.One)
                        return 0;
                    var accStore = await InsertAccStoreLedger(conn, transaction, collection);
                    if (accStore != Const.One)
                        return 0;
                    IEnumerable<IAccPayable> accCollectionPayable;
                    if (collection.AccCollectionAllotment.Any())
                    {
                        accCollectionPayable = await GetInvoicesAutoAllocateByUID(collection.AccCollectionAllotment.FirstOrDefault().Start, collection.AccCollectionAllotment.FirstOrDefault().End, collection.AccCollection.StoreUID);
                    }
                    else
                    {
                        accCollectionPayable = await GetInvoicesAutoAllocate(collection.AccCollection.StoreUID);
                    }

                    enteredAmount = collection.AccCollection.DefaultCurrencyAmount;
                    foreach (var list in accCollectionPayable)
                    {
                        if (list.BalanceAmount == 0)
                        {
                            break;
                        }
                        arrayAmount = list.BalanceAmount;
                        if (enteredAmount > list.BalanceAmount)
                        {
                            balanceAmount = enteredAmount - arrayAmount;

                            IAccPayable accPayable = await GetAccPayableAmount(list.StoreUID, list.ReferenceNumber);
                            if (accPayable == null)
                            {
                                accPayable = new Winit.Modules.CollectionModule.Model.Classes.AccPayable();
                            }
                            var sql2 = @"INSERT INTO acc_collection_allotment (uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime, server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,acc_collection_uid AS AccCollectionUID,
                            target_type AS TargetType,target_uid AS TargetUID,reference_number AS ReferenceNumber,currency_uid AS CurrencyUID,default_currency_uid AS DefaultCurrencyUID,
                            default_currency_exchange_reate AS DefaultCurrencyExchangeRate,default_currency_amount AS DefaultCurrencyAmount,early_payment_discount_percentage AS EarlyPaymentDiscountPercentage,
                            early_payment_discount_amount AS EarlyPaymentDiscountAmount,amount AS Amount)
                                VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @AccCollectionUID,
                                @TargetType, @TargetUID, @ReferenceNumber, @CurrencyUID, @DefaultCurrencyUID, @DefaultCurrencyExchangeRate, @DefaultCurrencyAmount, @EarlyPaymentDiscountPercentage,
                                @EarlyPaymentDiscountAmount, @Amount)";
                            Dictionary<string, object?> parameters2 = new Dictionary<string, object?>
                {
                                {"Acccollection.AccCollectionUID", collection.AccCollection.UID},
                                {"TargetType", list.SourceType.Contains("INVOICE") ? "INVOICE" : ""},
                                {"TargetUID", accPayable.UID == null ? "" : accPayable.UID},
                                {"ReferenceNumber",  list.ReferenceNumber == null ? "" : list.ReferenceNumber},
                                {"CurrencyUID", collection.AccCollection.CurrencyUID==null ? "" : collection.AccCollection.CurrencyUID},
                                {"DefaultCurrencyUID", collection.AccCollection.DefaultCurrencyUID==null ? "" : collection.AccCollection.DefaultCurrencyUID},
                                {"DefaultCurrencyExchangeRate", collection.AccCollection.DefaultCurrencyExchangeRate==0 ? 0 : collection.AccCollection.DefaultCurrencyExchangeRate},
                                {"DefaultCurrencyAmount", collection.AccCollection.DefaultCurrencyAmount==0 ? 0 : collection.AccCollection.DefaultCurrencyAmount},
                                {"EarlyPaymentDiscountPercentage",0},
                                {"EarlyPaymentDiscountAmount", 0 },
                                {"Amount", arrayAmount },
                                {"CreatedTime", DateTime.Now },
                                {"ModifiedTime", DateTime.Now},
                                {"ModifiedBy", Const.ModifiedBy },
                                {"ServerAddTime", DateTime.Now },
                                {"ServerModifiedTime", DateTime.Now },
                                {"CreatedBy", Const.ModifiedBy },
                                {"UID", (Guid.NewGuid()).ToString()}
                            };
                            Retval = await CommonMethod(sql2, conn, parameters2, transaction);

                            if (Retval != Const.One)
                                return 0;
                            var sql4 = @"UPDATE acc_payable
                                        SET modified_time = @ModifiedTime,
                                        server_modified_time = @ServerModifiedTime,paid_amount = @PaidAmount 
                                        WHERE uid = @UID";
                            var sql1 = @"UPDATE acc_payable
                                        SET modified_time = @ModifiedTime,
                                        server_modified_time = @ServerModifiedTime, un_settled_amount = @UnSettledAmount
                                        WHERE uid = @UID";
                            Dictionary<string, object?> parameters4 = new Dictionary<string, object?>
                                {
                                    {"PaidAmount", accPayable.PaidAmount + arrayAmount},
                                    {"ModifiedTime", DateTime.Now},
                                    {"ServerModifiedTime", DateTime.Now},
                                    {"UID", accPayable.UID==null ? "" :accPayable.UID}
                                };
                            Dictionary<string, object?> parameters1 = new Dictionary<string, object?>
                                {
                                    {"UnSettledAmount", accPayable.UnSettledAmount + arrayAmount},
                                    {"ModifiedTime", DateTime.Now},
                                    {"ServerModifiedTime", DateTime.Now},
                                    {"UID", accPayable.UID==null? "":accPayable.UID}
                                };

                            Retval = collection.AccCollection.Category == Const.Cash ? await CommonMethod(sql4, conn, parameters4, transaction) : await CommonMethod(sql1, conn, parameters1, transaction);

                            if (Retval != Const.One)
                                return 0;
                            enteredAmount = balanceAmount;
                        }
                        else
                        {
                            if (enteredAmount == 0)
                            {
                                transaction.Commit();
                                return 1;
                            }
                            IAccPayable accPayable = await GetAccPayableAmount(list.StoreUID, list.ReferenceNumber);
                            if (accPayable == null)
                            {
                                accPayable = new Winit.Modules.CollectionModule.Model.Classes.AccPayable();
                            }
                            var sql2 = @"INSERT INTO acc_collection_allotment (uid AS UID, created_by AS CreatedBy,created_time AS CreatedTime,modified_by ModifiedBy,
                            modified_time AS ModifiedTime,server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,acc_collection_uid AS AccCollectionUID,
                            targer_type AS TargetType,target_uid AS TargetUID,reference_number AS ReferenceNumber,currency_uid AS CurrencyUID,default_currency_uid AS DefaultCurrencyUID,
                            default_currency_exchange_rate AS DefaultCurrencyExchangeRate,default_currency_amount AS DefaultCurrencyAmount,early_payment_exchange_percentage AS EarlyPaymentDiscountPercentage,
                            early_payment_discount_amount AS EarlyPaymentDiscountAmount,amount AS Amount)
                                VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @AccCollectionUID,
                                @TargetType, @TargetUID, @ReferenceNumber, @CurrencyUID, @DefaultCurrencyUID, @DefaultCurrencyExchangeRate, @DefaultCurrencyAmount, @EarlyPaymentDiscountPercentage,
                                @EarlyPaymentDiscountAmount, @Amount)";
                            Dictionary<string, object?> parameters2 = new Dictionary<string, object?>
                {
                                {"Acccollection.AccCollectionUID", collection.AccCollection.UID},
                                {"TargetType", list.SourceType.Contains("INVOICE") ? "INVOICE" : ""},
                                {"TargetUID", accPayable.UID == null ? "" : accPayable.UID},
                                {"ReferenceNumber",  list.ReferenceNumber == null ? "" : list.ReferenceNumber},
                                {"CurrencyUID", collection.AccCollection.CurrencyUID==null ? "" : collection.AccCollection.CurrencyUID},
                                {"DefaultCurrencyUID", collection.AccCollection.DefaultCurrencyUID==null ? "" : collection.AccCollection.DefaultCurrencyUID},
                                {"DefaultCurrencyExchangeRate", collection.AccCollection.DefaultCurrencyExchangeRate==0 ? 0 : collection.AccCollection.DefaultCurrencyExchangeRate},
                                {"DefaultCurrencyAmount", collection.AccCollection.DefaultCurrencyAmount==0 ? 0 : collection.AccCollection.DefaultCurrencyAmount},
                                {"EarlyPaymentDiscountPercentage",0},
                                {"EarlyPaymentDiscountAmount", 0 },
                                {"Amount", enteredAmount },
                                {"CreatedTime", DateTime.Now },
                                {"ModifiedTime", DateTime.Now},
                                {"ModifiedBy", Const.ModifiedBy },
                                {"ServerAddTime", DateTime.Now },
                                {"ServerModifiedTime", DateTime.Now },
                                {"CreatedBy", Const.ModifiedBy },
                                {"UID", (Guid.NewGuid()).ToString()}
                            };
                            Retval = await CommonMethod(sql2, conn, parameters2, transaction);

                            if (Retval != Const.One)
                                return 0;

                            var sql4 = @"UPDATE acc_payable
                                        SET modified_time = @ModifiedTime,
                                        server_modified_time = @ServerModifiedTime,""PaidAmount"" = @PaidAmount 
                                        WHERE uid = @UID";
                            var sql1 = @"UPDATE acc_payable
                                        SET modified_time = @ModifiedTime,
                                        server_modified_time = @ServerModifiedTime,""PaidAmount"" = @PaidAmount 
                                        WHERE uid = @UID";
                            Dictionary<string, object?> parameters4 = new Dictionary<string, object?>
                                {
                                    {"PaidAmount", accPayable.PaidAmount + enteredAmount},
                                    {"ModifiedTime", DateTime.Now},
                                    {"ServerModifiedTime", DateTime.Now},
                                    {"UID", accPayable.UID==null ? "" :accPayable.UID}
                                };
                            Dictionary<string, object?> parameters1 = new Dictionary<string, object?>
                                {
                                    {"UnSettledAmount", accPayable.UnSettledAmount + enteredAmount},
                                    {"ModifiedTime", DateTime.Now},
                                    {"ServerModifiedTime", DateTime.Now},
                                    {"UID", accPayable.UID==null? "":accPayable.UID}
                                };

                            Retval = collection.AccCollection.Category == Const.Cash ? await CommonMethod(sql4, conn, parameters4, transaction) : await CommonMethod(sql1, conn, parameters1, transaction);

                            if (Retval != Const.One)
                                return 0;
                            enteredAmount = 0;
                        }
                    }
                    if (enteredAmount != 0)
                    {
                        transaction.Commit();
                        return Convert.ToInt32(enteredAmount);
                    }
                    else
                    {
                        transaction.Commit();
                        return 1;
                    }
                    //var accAllot = await InsertAccCollectionAllotment(conn, transaction, collection);
                    //if (accAllot == Const.One)
                    //{
                    //transaction.Commit();
                    //return Const.SuccessInsert;
                    //}
                    //else
                    //{
                    //transaction.Rollback();
                    //return accAllot;
                    //}

                }
            }
        }
        public async Task<string> CreateOnAccountReceipt(Model.Interfaces.ICollections collection)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        if (collection != null)
                        {
                            if (collection.AccCollection.UID != null)
                            {
                                _creditID = creditID();
                                var val = "";
                                var accCollec = await InsertAccCollection(conn, transaction, collection);
                                if (accCollec != Const.One)
                                    return accCollec;
                                var accPaymode = await InsertAccCollectionPaymentMode(conn, transaction, collection);
                                if (accPaymode != Const.One)
                                    return accPaymode;
                                var accStore = await InsertAccStoreLedger(conn, transaction, collection);
                                if (accStore != Const.One)
                                    return accStore;
                                var accAllot = await InsertAccCollectionAllotment(conn, transaction, collection);
                                if (accAllot != Const.One)
                                    return accAllot;
                                var accCollectionCurrencyData = await InsertAccCollectionCurrencyDetails(conn, transaction, collection);

                                if (collection.AccCollection.Category == "Cash" || collection.AccCollection.Category == "OnAccount")
                                {
                                    var Retval4 = "";
                                    var sql4 = @"INSERT INTO acc_receivable (
                                        uid ,created_by ,created_time ,modified_by ,modified_time ,
                                        server_add_time ,server_modified_time ,source_type,source_uid ,
                                        reference_number ,org_uid ,job_position_uid ,currency_uid ,
                                        amount , paid_amount ,store_uid ,transaction_date , 
                                        due_date ,source ,unsettled_amount )
	                                    VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,
                                         @SourceType, @SourceUID, @ReferenceNumber, @OrgUID, @JobPositionUID, @CurrencyUID, @Amount, @PaidAmount, 
                                         @StoreUID, @TransactionDate, @DueDate, @Source,@UnSettledAmount)";
                                    Dictionary<string, object?> parameters4 = new Dictionary<string, object?>
                                        {
                                            {"UID", collection.AccReceivable.FirstOrDefault().UID},
                                            {"CreatedTime", DateTime.Now },
                                            {"ModifiedTime", DateTime.Now},
                                            {"ModifiedBy", Const.ModifiedBy },
                                            {"ServerAddTime", DateTime.Now },
                                            {"ServerModifiedTime", DateTime.Now },
                                            {"CreatedBy", Const.ModifiedBy },
                                            {"SourceType", "CREDITNOTE" },
                                            {"SourceUID", "CREDITNOTE-"+_creditID },
                                            {"ReferenceNumber", "CREDITNOTE-"+_creditID },
                                            {"OrgUID", collection.AccCollection.OrgUID },
                                            {"JobPositionUID", collection.AccCollection.JobPositionUID },
                                            {"CurrencyUID", collection.AccCollection.CurrencyUID },
                                            {"StoreUID", collection.AccCollection.StoreUID },
                                            {"TransactionDate", DateTime.Now },
                                            {"DueDate", DateTime.Now.AddYears(1) },
                                            {"Source", collection.AccCollection.Source },
                                            {"Amount", collection.AccCollection.DefaultCurrencyAmount ==0 ? 0 : collection.AccCollection.DefaultCurrencyAmount},
                                            {"PaidAmount", 0},
                                            {"UnSettledAmount",collection.AccCollection.Category != "Cash" ? collection.AccCollection.DefaultCurrencyAmount : 0},
                                        };
                                    Retval4 = await CommonMethod(sql4, conn, parameters4, transaction);
                                }

                                transaction.Commit();
                                return Const.SuccessInsert;
                                //}
                                //else
                                //{
                                //    transaction.Rollback();
                                //    return Const.MisMatch;
                                //}
                            }
                            else
                            {
                                transaction.Rollback();
                                return Const.ParamMissing;
                            }

                        }
                        else
                        {
                            transaction.Rollback();
                            return Const.ParamMissing;
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return ex.Message.ToString();
                    }
                }
            }
        }
        public async Task<string> CashCollectionSettlement(string collection)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    if (collection != null)
                    {
                        var Retval = "";
                        ICollections accCollection = await GetAmountCash(collection);
                        IEnumerable<IAccCollectionAllotment> accCollectionAllotments = await GetCashDetails(collection);
                        var sql = @"INSERT INTO acc_collection_settlement (uid AS UID, acc_collection_uid AS AccCollectionUID, settled_by AS SettledBy, 
                            created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, 
                            server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, payment_mode AS PaymentMode, 
                            received_amount AS ReceivedAmount, receipt_number AS ReceiptNumber)
                            VALUES(@UID, @AccCollectionUID, @SettledBy, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, 
                            @ServerAddTime, @ServerModifiedTime, @PaymentMode, @ReceivedAmount, @ReceiptNumber)";

                        Dictionary<string, object?> parameters = new Dictionary<string, object?>
                            {
                               {"accCollection.AccCollectionUID",accCollection.AccCollection.UID==null?"":accCollection.AccCollection.UID},
                               {"UID",(Guid.NewGuid()).ToString()},
                               {"SettledBy", accCollection.AccCollection.ModifiedBy==null?"":accCollection.AccCollection.ModifiedBy},
                               {"ReceivedAmount",accCollection.AccCollection.DefaultCurrencyAmount==0?0:accCollection.AccCollection.DefaultCurrencyAmount},
                               {"PaymentMode",accCollection.AccCollection.Category==null?"":accCollection.AccCollection.Category},
                               {"ReceiptNumber",accCollection.AccCollection.ReceiptNumber==null?"":accCollection.AccCollection.ReceiptNumber},
                               {"CreatedTime", DateTime.Now},
                               {"ModifiedTime", DateTime.Now},
                               {"ModifiedBy", Const.ModifiedBy},
                               {"ServerAddTime", DateTime.Now},
                               {"ServerModifiedTime", DateTime.Now},
                               {"CreatedBy", Const.ModifiedBy},
                            };
                        Retval = await CommonMethod(sql, conn, parameters, transaction);
                        if (Retval != Const.One)
                        {
                            return Retval;
                        }
                        var res = await UpdateCashCollection(collection, accCollection.AccCollection.SessionUserCode, conn, transaction);
                        if (res != Const.One)
                        {
                            return res;
                        }
                        foreach (var list in accCollectionAllotments)
                        {
                            var Retval1 = "";
                            var sql1 = @"INSERT INTO acc_collection_settlement_receipts (uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                            modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, 
                            receipt_number AS ReceiptNumber, target_type AS TargetType , target_uid AS TargetUID, paid_amount AS PaidAmount)
                            VALUES(@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                            @ReceiptNumber, @TargetType, @TargetUID, @PaidAmount)";
                            Dictionary<string, object?> parameters1 = new Dictionary<string, object?>
                        {
                            {"CreatedTime",DateTime.Now},
                            {"ModifiedTime", DateTime.Now},
                            {"ModifiedBy", Const.ModifiedBy},
                            {"ServerAddTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now},
                            {"CreatedBy", Const.ModifiedBy},
                            {"UID", (Guid.NewGuid()).ToString()},
                            {"ReceiptNumber", accCollection.AccCollection.ReceiptNumber==null?"":accCollection.AccCollection.ReceiptNumber},
                            {"PaidAmount", list.DefaultCurrencyAmount==0?0:list.DefaultCurrencyAmount},
                            {"TargetType", list.TargetType==null?"":list.TargetType},
                            {"TargetUID", list.TargetUID==null?"":list.TargetUID}
                        };
                            Retval1 = await CommonMethod(sql1, conn, parameters1, transaction);
                            if (Retval1 != Const.One)
                                return Retval1;
                        }
                        transaction.Commit();
                        return Const.SuccessInsert;

                    }
                    else
                    {
                        transaction.Rollback();
                        return Const.ParamMissing;
                    }
                }
            }
        }
        public async Task<string> CreateCollectionSettlementByCashier(Model.Interfaces.IAccCollectionSettlement collection)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    if (collection != null)
                    {
                        if (collection.Receipts != null)
                        {
                            var Retval = "";
                            var sql = @"INSERT INTO acc_collection_settlement ( uid AS UID,  created_by AS CreatedBy,  created_time AS CreatedTime,modified_by AS ModifiedBy, 
                modified_time AS ModifiedTime,  server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime,  acc_collection_uid AS AccCollectionUID, 
                collection_amount AS CollectionAmount,  received_amount AS ReceivedAmount, has_discrepancy AS HasDiscrepancy,  discrepancy_amount AS DiscrepancyAmount, 
                default_currency_uid AS DefaultCurrencyUID,  settlement_date AS SettlementDate, cashier_job_position_uid AS CashierJobPositionUID, 
                cashier_emp_uid AS CashierEmpUID,  session_user_code AS SessionUserCode)
                VALUES ( @UID,  @CreatedBy,  @CreatedTime,  @ModifiedBy,  @ModifiedTime, @ServerAddTime,  @ServerModifiedTime,  @AccCollectionUID,  @CollectionAmount, 
                @ReceivedAmount, @HasDiscrepancy, iscrepancyAmount,  @DefaultCurrencyUID,  @SettlementDate,  @CashierJobPositionUID,  @CashierEmpUID,  @SessionUserCode   )";

                            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                        {"UID", collection.UID},
                        {"AccCollectionUID", collection.AccCollectionUID},
                        {"CollectionAmount", collection.CollectionAmount},
                        {"ReceivedAmount", collection.ReceivedAmount},
                        {"HasDiscrepancy", collection.HasDiscrepancy},
                        {"DiscrepancyAmount", collection.DiscrepancyAmount},
                        {"DefaultCurrencyUID", collection.DefaultCurrencyUID},
                        {"SettlementDate", collection.SettlementDate},
                        {"CashierJobPositionUID", collection.CashierJobPositionUID},
                        {"CashierEmpUID", collection.CashierEmpUID},
                        {"CreatedTime", collection.CreatedTime},
                        {"ModifiedTime", collection.ModifiedTime},
                        {"ModifiedBy", Const.ModifiedBy},
                        {"ServerAddTime", collection.ServerAddTime},
                        {"ServerModifiedTime", collection.ServerModifiedTime},
                        {"CreatedBy", Const.ModifiedBy},
                        {"SessionUserCode", collection.SessionUserCode}
                };
                            Retval = await CommonMethod(sql, conn, parameters, transaction);
                            if (Retval != Const.One)
                            {
                                return Retval;
                            }
                            foreach (var receipt in collection.Receipts)
                            {
                                var sql1 = @"INSERT INTO acc_collection_settlement_receipts (uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                        modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime,
                        receipt_number AS ReceiptNumber, settled_amount AS SettledAmount, acc_collection_settlement_uid AS AccCollectionSettlementUID,
                        session_user_code AS SessionUserCode) VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,
                        @ReceiptNumber, @SettledAmount, @AccCollectionSettlementUID, @SessionUserCode);";

                                Dictionary<string, object?> parameters1 = new Dictionary<string, object?>
                    {
                        {"UID", receipt.UID},
                        {"ReceiptNumber", receipt.ReceiptNumber},
                        {"SettledAmount", receipt.SettledAmount},
                        {"AccCollectionSettlementUID", receipt.AccCollectionSettlementUID},
                        {"CreatedTime", receipt.CreatedTime},
                        {"ModifiedTime", receipt.ModifiedTime},
                        {"ModifiedBy",Const.ModifiedBy},
                        {"ServerAddTime", receipt.ServerAddTime},
                        {"ServerModifiedTime", receipt.ServerModifiedTime},
                        {"CreatedBy", Const.ModifiedBy},
                        {"SessionUserCode", collection.SessionUserCode}
                    };
                                Retval = await CommonMethod(sql1, conn, parameters1, transaction);
                                if (Retval != Const.One)
                                {
                                    return Retval;
                                }
                            }
                            transaction.Commit();
                            return Const.SuccessInsert;
                        }
                        else
                        {
                            transaction.Rollback();
                            return Const.ParamMissing;
                        }
                    }
                    else
                    {
                        transaction.Rollback();
                        return Const.ParamMissing;
                    }
                }
            }
        }
        public async Task<string> VOIDCollectionByReceiptNumber(string ReceiptNumber, string TargetUID, decimal Amount, string ChequeNo, string SessionUserCode = null, string ReasonforCancelation = null)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    if (ReceiptNumber != null && ReceiptNumber != "")
                    {
                        var exist = await CheckExists(ChequeNo);
                        bool Exist = exist == "false" ? false : true;
                        if (!Exist)
                        {
                            ICollections accCollection = await GetAmountCash(ReceiptNumber);
                            var Retval = "";
                            var sql = @"UPDATE acc_collection
                                SET modified_by = @ModifiedBy,modified_time = @ModifiedTime,
                                server_modified_time = @ServerModifiedTime,
                                amount = @Amount,cancelled_on = @CancelledOn, comments = @Comments WHERE uid = @UID;";
                            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                        {
                            {"UID", accCollection.AccCollection.UID},
                            {"Amount",  accCollection.AccCollection.Amount *0 !=0 ? 0 : accCollection.AccCollection.Amount *0},
                            {"CancelledOn", DateTime.Now},
                            {"Comments",  ReasonforCancelation},
                            {"ModifiedTime", DateTime.Now},
                            {"ModifiedBy", Const.ModifiedBy},
                            {"ServerModifiedTime", DateTime.Now}
                        };
                            Retval = await CommonMethod(sql, conn, parameters, transaction);
                            if (Retval != Const.One)
                            {
                                return Retval;
                            }
                            var tar = await IsReversal2(accCollection.AccCollection.UID, accCollection.AccCollection.ReceiptNumber, ReasonforCancelation, conn, transaction, "Void");
                            IAccCollectionPaymentMode accCollectionPayment = await GetPaymentAmount(accCollection.AccCollection.UID);
                            var sql1 = @"UPDATE acc_collection_payment_mode SET modified_by = @ModifiedBy,modified_time = @ModifiedTime,
                                server_modified_time = @ServerModifiedTime,amount = @Amount,
                                status = @Status WHERE uid = @UID";
                            Dictionary<string, object?> parameters1 = new Dictionary<string, object?>
                        {
                            {"Amount", accCollectionPayment.Amount *0 !=0 ? 0 : accCollectionPayment.Amount *0},
                            {"Status", accCollectionPayment.Status== null ? "": "Voided"},
                            {"ModifiedTime", DateTime.Now},
                            {"ModifiedBy", Const.ModifiedBy},
                            {"ServerModifiedTime", DateTime.Now},
                            {"UID",accCollectionPayment.UID == null ? "": accCollectionPayment.UID}
                        };
                            Retval = await CommonMethod(sql1, conn, parameters1, transaction);
                            if (Retval != Const.One)
                            {
                                return Retval;
                            }
                            IEnumerable<IAccCollectionAllotment> allot = await GetCashAllot(accCollection.AccCollection.UID);
                            IAccStoreLedger accStoreLedger = await GetStoreLedger(accCollection.AccCollection.UID);
                            var sql3 = @"UPDATE acc_store_ledger
                                     SET modified_by = @ModifiedBy,
                                    server_modified_time = @ServerModifiedTime,
                                    collectedmo_amount = @CollectedAmount WHERE uid = @UID";
                            Dictionary<string, object?> parameters3 = new Dictionary<string, object?>
                        {
                            {"CollectedAmount", accStoreLedger.Amount * 0},
                            {"Balance", accStoreLedger.Balance==0 ? 0 : accStoreLedger.Balance + accStoreLedger.Amount},
                            {"ModifiedBy", Const.ModifiedBy},
                            {"ServerModifiedTime", DateTime.Now},
                            {"UID", accStoreLedger.UID==null? "" : accStoreLedger.UID}
                        };
                            Retval = await CommonMethod(sql3, conn, parameters3, transaction);
                            if (Retval != Const.One)
                            {
                                return Retval;
                            }
                            //var Res = await UpdateReverseCashCollection(ChequeNo, conn, transaction);
                            foreach (var accCollectionAllotment in allot)
                            {
                                //await UpdateAllot(accCollectionAllotment.AccCollectionUID, accCollectionAllotment.PaidAmount, accCollectionAllotment.TargetUID, conn, transaction);
                                var sql2 = @"UPDATE acc_collection_allotment
                                        SET modified_by = @ModifiedBy,modified_time = @ModifiedTime,
                                        amount = @Amount,server_modified_time = @ServerModifiedTime WHERE uid = @UID";
                                Dictionary<string, object?> parameters2 = new Dictionary<string, object?>
                            {
                                {"Amount", accCollectionAllotment.Amount * 0},
                                { "ModifiedTime", DateTime.Now},
                                { "ModifiedBy", Const.ModifiedBy},
                                { "ServerModifiedTime", DateTime.Now},
                                { "UID", accCollectionAllotment.UID==null ? "" : accCollectionAllotment.UID}
                            };
                                Retval = await CommonMethod(sql2, conn, parameters2, transaction);
                                if (Retval != Const.One)
                                {
                                    return Retval;
                                }
                                if (accCollectionAllotment.TargetType.Contains("INVOICE"))
                                {
                                    IEnumerable<IAccPayable> payb = await GetChequePay(accCollectionAllotment.TargetUID);
                                    foreach (var list1 in payb)
                                    {
                                        var sql4 = @"UPDATE acc_payable
                                    SET modified_time = @ModifiedTime,
                                    server_modified_time = @ServerModifiedTime, paid_amount = @PaidAmount WHERE uid = @UID ";
                                        Dictionary<string, object?> parameters4 = new Dictionary<string, object?>
                                        {
                                            {"PaidAmount", accCollectionAllotment.PaidAmount*0 == 0 ? list1.PaidAmount - accCollectionAllotment.Amount :  0  },
                                        {"ModifiedTime", DateTime.Now},
                                        {"ServerModifiedTime", DateTime.Now},
                                        {"UID", list1.UID}
                                };
                                        Retval = await CommonMethod(sql4, conn, parameters4, transaction);
                                        if (Retval != Const.One)
                                        {
                                            return Retval;
                                        }
                                    }
                                }
                                else
                                {
                                    IEnumerable<IAccReceivable> recv = await GetChequeRec(accCollectionAllotment.TargetUID);
                                    foreach (var list1 in recv)
                                    {
                                        var sql5 = @"UPDATE acc_receivable
                                    SET modified_time = @ModifiedTime,
                                    server_modified_time = @ServerModifiedTime,paid_amount = @PaidAmount
                                    WHERE uid = @UID";
                                        Dictionary<string, object?> parameters5 = new Dictionary<string, object?>
                                        {
                                            {"PaidAmount", accCollectionAllotment.PaidAmount*0 == 0 ? list1.PaidAmount - accCollectionAllotment.Amount :  0},
                                {"ModifiedTime",DateTime.Now},
                                {"ServerModifiedTime", DateTime.Now},
                                {"UID",  list1.UID}
                            };
                                        Retval = await CommonMethod(sql5, conn, parameters5, transaction);
                                        if (Retval != Const.One)
                                        {
                                            return Retval;
                                        }
                                    }
                                }
                            }
                            transaction.Commit();
                            return Const.Success;
                        }
                        else
                        {
                            transaction.Rollback();
                            return Const.VoidMsg;
                        }
                    }
                    else
                    {
                        transaction.Rollback();
                        return Const.ParamMissing;
                    }

                }
            }
        }
        //stopped adding SessionUserCode here
        public async Task<string> CreateReversalReceiptByReceiptNumber(string ReceiptNumber, string TargetUID, decimal Amount, string ChequeNo, string SessionUserCode = null, string ReasonforCancelation = null)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    ICollections accCollection1 = await GetAmountCash(ReceiptNumber);
                    ReceiptNumber = (ReceiptNumber == "" || ReceiptNumber == null) ? accCollection1.AccCollection.UID : ReceiptNumber;
                    if (ReceiptNumber != null && ReceiptNumber != "")
                    {
                        //var exist = await CheckExists(ChequeNo);
                        bool Exist = true;
                        if (Exist)
                        {
                            var Retval = "";
                            var sql = @"INSERT INTO acc_collection (uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime,
                            receipt_number AS ReceiptNumber, consolidated_receipt_number AS ConsolidatedReceiptNumber, category AS Category, 
                            amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, default_currency_exchange_rate AS 
                            DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount, org_uid AS OrgUID, 
                            distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, route_uid AS RouteUID, job_position_uid AS JobPositionUID,
                            emp_uid AS EmpUID, collected_date AS CollectedDate, status AS Status, remarks AS Remarks, reference_number AS ReferenceNumber,
                            is_realized AS IsRealized, latitude AS Latitude, longitude AS Longitude, source AS Source, is_multimode AS IsMultimode,
                            trip_date AS TripDate, comments AS Comments, salesman AS Salesman, route AS Route, reversal_receipt_uid AS ReversalReceiptUID,
                            cancelled_on AS CancelledOn)
                        VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @ReceiptNumber, 
                        @ConsolidatedReceiptNumber, @Category, @Amount, @CurrencyUID, @DefaultCurrencyUID, @DefaultCurrencyExchangeRate, @DefaultCurrencyAmount,
                        @OrgUID, @DistributionChannelUID, @StoreUID, @RouteUID, @JobPositionUID, @EmpUID, @CollectedDate, @Status, @Remarks, @ReferenceNumber,
                        @IsRealized, @Latitude, @Longitude, @Source, @IsMultimode, @TripDate, @Comments, @Salesman, @Route, @ReversalReceiptUID, @CancelledOn)";

                            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                                {
                                   {"UID", string.IsNullOrEmpty(accCollection1.AccCollection.UID) ? "" : "R - " + accCollection1.AccCollection.UID},
                                   {"ReceiptNumber", string.IsNullOrEmpty(accCollection1.AccCollection.ReceiptNumber) ? "": "R - " + accCollection1.AccCollection.ReceiptNumber},
                                   {"ConsolidatedReceiptNumber", string.IsNullOrEmpty(accCollection1.AccCollection.ConsolidatedReceiptNumber) ?"" : "R - " + accCollection1.AccCollection.ConsolidatedReceiptNumber},
                                   {"Category", string.IsNullOrEmpty(accCollection1.AccCollection.Category) ? "" : accCollection1.AccCollection.Category},
                                   {"Amount", accCollection1.AccCollection.Amount * Const.Num },
                                   {"CurrencyUID", string.IsNullOrEmpty(accCollection1.AccCollection.CurrencyUID)?"":accCollection1.AccCollection.CurrencyUID},
                                   {"DefaultCurrencyUID", string.IsNullOrEmpty(accCollection1.AccCollection.DefaultCurrencyUID)?"":accCollection1.AccCollection.DefaultCurrencyUID},
                                   {"DefaultCurrencyExchangeRate", accCollection1.AccCollection.DefaultCurrencyExchangeRate==0?0:accCollection1.AccCollection.DefaultCurrencyExchangeRate},
                                   {"DefaultCurrencyAmount", accCollection1.AccCollection.DefaultCurrencyAmount==0?0:accCollection1.AccCollection.DefaultCurrencyAmount},
                                   {"OrgUID", string.IsNullOrEmpty(accCollection1.AccCollection.OrgUID)?"":accCollection1.AccCollection.OrgUID},
                                   {"DistributionChannelUID", string.IsNullOrEmpty(accCollection1.AccCollection.DistributionChannelUID)?"":accCollection1.AccCollection.DistributionChannelUID},
                                   {"StoreUID", string.IsNullOrEmpty(accCollection1.AccCollection.StoreUID)?"":accCollection1.AccCollection.StoreUID},
                                   {"RouteUID", string.IsNullOrEmpty(accCollection1.AccCollection.RouteUID)?"":accCollection1.AccCollection.RouteUID},
                                   {"JobPositionUID", string.IsNullOrEmpty(accCollection1.AccCollection.JobPositionUID)?"":accCollection1.AccCollection.JobPositionUID},
                                   {"EmpUID", string.IsNullOrEmpty(accCollection1.AccCollection.EmpUID)?"":accCollection1.AccCollection.EmpUID},
                                   {"CollectedDate", accCollection1.AccCollection.CollectedDate==null?DateTime.Now:accCollection1.AccCollection.CollectedDate},
                                   {"Status", "Reversed"},
                                   {"Remarks", string.IsNullOrEmpty(accCollection1.AccCollection.Remarks)?"":accCollection1.AccCollection.Remarks},
                                   {"ReferenceNumber", string.IsNullOrEmpty(accCollection1.AccCollection.ReferenceNumber)?"":accCollection1.AccCollection.ReferenceNumber},
                                   {"IsRealized", false},
                                   {"Latitude", string.IsNullOrEmpty(accCollection1.AccCollection.Latitude)?"":accCollection1.AccCollection.Latitude},
                                   {"Longitude", string.IsNullOrEmpty(accCollection1.AccCollection.Longitude)?"":accCollection1.AccCollection.Longitude},
                                   {"Source", string.IsNullOrEmpty(accCollection1.AccCollection.Source)?"":accCollection1.AccCollection.Source},
                                   {"IsMultimode", accCollection1.AccCollection.IsMultimode==false?false:accCollection1.AccCollection.IsMultimode},
                                   {"TripDate", accCollection1.AccCollection.TripDate==null ? DateTime.Now : accCollection1.AccCollection.TripDate},
                                   {"Comments", string.IsNullOrEmpty(accCollection1.AccCollection.Comments) ? "" : accCollection1.AccCollection.Comments},
                                   {"Salesman", string.IsNullOrEmpty(accCollection1.AccCollection.Salesman) ? "" : accCollection1.AccCollection.Salesman},
                                   {"Route", string.IsNullOrEmpty(accCollection1.AccCollection.Route) ? "" : accCollection1.AccCollection.Route},
                                   {"ReversalReceiptUID", string.IsNullOrEmpty(accCollection1.AccCollection.ReceiptNumber) ? "" : accCollection1.AccCollection.ReceiptNumber},
                                   {"CancelledOn", DateTime.Now},
                                   {"CreatedTime", DateTime.Now},
                                   {"ModifiedTime", DateTime.Now},
                                   {"ModifiedBy", string.IsNullOrEmpty(accCollection1.AccCollection.ModifiedBy) ? "" : accCollection1.AccCollection.ModifiedBy},
                                   {"ServerAddTime", DateTime.Now},
                                   {"ServerModifiedTime", DateTime.Now},
                                   {"CreatedBy", string.IsNullOrEmpty(accCollection1.AccCollection.CreatedBy) ? "" : accCollection1.AccCollection.CreatedBy}
                                };
                            Retval = await CommonMethod(sql, conn, parameters, transaction);
                            if (Retval != Const.One)
                            {
                                return Retval;
                            }
                            var tar = await IsReversal2(accCollection1.AccCollection.UID, accCollection1.AccCollection.ReceiptNumber, ReasonforCancelation, conn, transaction);
                            IAccCollectionPaymentMode accCollectionPayment = await GetPaymentAmount(ReceiptNumber);
                            //await UpdateCollectionPayment(accCollectionPayment.UID, Amount);
                            var sql1 = @"INSERT INTO acc_collection_payment_mode (uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime,
                                    server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, acc_collection_uid AS AccCollectionUID, 
                                    bank_uid AS BankUID, branch AS Branch, cheque_no AS ChequeNo, amount AS Amount, currency_uid AS CurrencyUID, 
                                    default_currency_uid AS DefaultCurrencyUID, default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                                    default_currency_amount AS DefaultCurrencyAmount, cheque_date AS ChequeDate, status AS Status, realization_date AS RealizationDate)
                                        VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @AccCollectionUID, 
                                    @BankUID, @Branch, @ChequeNo, @Amount, @CurrencyUID, @DefaultCurrencyUID, @DefaultCurrencyExchangeRate, @DefaultCurrencyAmount,
                                    @ChequeDate, @Status, @RealizationDate)";
                            Dictionary<string, object?> parameters1 = new Dictionary<string, object?>
                        {
                            {"AccCollectionUID", string.IsNullOrEmpty(accCollection1.AccCollection.UID)  ? "" : "R - " + accCollection1.AccCollection.UID},
                            {"BankUID", string.IsNullOrEmpty(accCollectionPayment.BankUID) ? "" : accCollectionPayment.BankUID},
                            {"Branch", string.IsNullOrEmpty(accCollectionPayment.Branch) ? "" : accCollectionPayment.Branch},
                            {"ChequeNo", string.IsNullOrEmpty(accCollectionPayment.ChequeNo) ? "" : accCollectionPayment.ChequeNo},
                            {"CurrencyUID", string.IsNullOrEmpty(accCollection1.AccCollection.CurrencyUID) ? "" : accCollection1.AccCollection.CurrencyUID},
                            {"DefaultCurrencyUID", string.IsNullOrEmpty(accCollection1.AccCollection.DefaultCurrencyUID) ? "" : accCollection1.AccCollection.DefaultCurrencyUID},
                            {"DefaultCurrencyExchangeRate", accCollection1.AccCollection.DefaultCurrencyExchangeRate==0?0:accCollection1.AccCollection.DefaultCurrencyExchangeRate},
                            {"DefaultCurrencyAmount", accCollection1.AccCollection.DefaultCurrencyAmount==0?0:accCollection1.AccCollection.DefaultCurrencyAmount},
                            {"ChequeDate", accCollectionPayment.ChequeDate==null?DateTime.Now:accCollectionPayment.ChequeDate},
                            {"Status", "Reversed"},
                            {"RealizationDate", accCollectionPayment.RealizationDate==null?DateTime.Now:accCollectionPayment.RealizationDate},
                            {"CreatedTime",DateTime.Now},
                            {"ModifiedTime", DateTime.Now},
                            {"ModifiedBy", string.IsNullOrEmpty(accCollection1.AccCollection.ModifiedBy) ? "" : accCollection1.AccCollection.ModifiedBy},
                            {"ServerAddTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now},
                            {"CreatedBy", string.IsNullOrEmpty(accCollection1.AccCollection.CreatedBy) ? "" : accCollection1.AccCollection.CreatedBy},
                            {"UID", "R - " + (Guid.NewGuid()).ToString()},
                            {"Amount", accCollectionPayment.Amount * Const.Num}
                        };
                            Retval = await CommonMethod(sql1, conn, parameters1, transaction);
                            if (Retval != Const.One)
                                return Retval;
                            IAccStoreLedger accStoreLedger = await GetStoreLedger(accCollection1.AccCollection.UID);
                            // await UpdateStoreLed(accStoreLedger.UID, Amount);
                            var sql3 = @"INSERT INTO acc_store_ledger (uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, 
                        modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, source_type AS SourceType,
                        source_uid AS SourceUID, credit_type AS CreditType, org_uid AS OrgUID, store_uid AS StoreUID, default_currency_uid AS DefaultCurrencyUID,
                        document_number AS DocumentNumber, default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount,
                        amount AS Amount, transaction_date_time AS TransactionDateTime, collected_amount AS CollectedAmount, currency_uid AS CurrencyUID, balance AS Balance) 
                        VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @SourceType, @SourceUID,
                        @CreditType, @OrgUID, @StoreUID, @DefaultCurrencyUID, @DocumentNumber, @DefaultCurrencyExchangeRate, @DefaultCurrencyAmount, @Amount, 
                        @TransactionDateTime, @CollectedAmount, @CurrencyUID, @Balance)";
                            Dictionary<string, object?> parameters3 = new Dictionary<string, object?>
                        {
                            {"SourceType", "Collection"},
                            {"SourceUID", accCollection1.AccCollection.UID==null?"": accCollection1.AccCollection.UID},
                            {"CreditType",  -1},
                            {"OrgUID", accStoreLedger.OrgUID==null?"": accStoreLedger.OrgUID},
                            {"StoreUID", accCollection1.AccCollection.StoreUID==null?"": accCollection1.AccCollection.StoreUID},
                            {"DefaultCurrencyUID", accCollection1.AccCollection.DefaultCurrencyUID==null?"": accCollection1.AccCollection.DefaultCurrencyUID},
                            {"DocumentNumber", "R - " + accCollection1.AccCollection.ReceiptNumber == null ? "" : "R - " + accCollection1.AccCollection.ReceiptNumber},
                            {"DefaultCurrencyExchangeRate", accCollection1.AccCollection.DefaultCurrencyExchangeRate==0?0: accCollection1.AccCollection.DefaultCurrencyExchangeRate},
                            {"DefaultCurrencyAmount", accCollection1.AccCollection.DefaultCurrencyAmount==0?0: accCollection1.AccCollection.DefaultCurrencyAmount},
                            {"Amount", accCollection1.AccCollection.DefaultCurrencyAmount == 0 ? 0 : accCollection1.AccCollection.DefaultCurrencyAmount},
                            {"TransactionDateTime", accStoreLedger.TransactionDateTime == null ? DateTime.Now : DateTime.Now},
                            {"CollectedAmount", accCollection1.AccCollection.Amount == 0 ? 0 : accCollection1.AccCollection.Amount},
                            {"Balance", accStoreLedger.Balance == 0 ? (0 + accCollection1.AccCollection.DefaultCurrencyAmount * -1) * Const.Num : (accStoreLedger.Balance + accCollection1.AccCollection.DefaultCurrencyAmount * -1) * Const.Num},
                            {"CurrencyUID", accCollection1.AccCollection.CurrencyUID ==null ? "" : accCollection1.AccCollection.CurrencyUID},
                            {"CreatedTime", DateTime.Now},
                            {"ModifiedTime", DateTime.Now},
                            {"ModifiedBy", string.IsNullOrEmpty(accCollection1.AccCollection.ModifiedBy) ? "" : accCollection1.AccCollection.ModifiedBy},
                            {"ServerAddTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now},
                            {"CreatedBy", string.IsNullOrEmpty(accCollection1.AccCollection.CreatedBy) ? "" : accCollection1.AccCollection.CreatedBy},
                            {"UID", "R - " + (Guid.NewGuid()).ToString()}
                        };
                            Retval = await CommonMethod(sql3, conn, parameters3, transaction);
                            if (Retval != Const.One)
                                return Retval;
                            //IAccCollectionAllotment accCollectionAllotments = await GetAllottAmount(ReceiptNumber, TargetUID);
                            //IAccPayable accPayables = await GetAccPayableAmount(ReceiptNumber, accCollectionAllotments.TargetUID);
                            //IEnumerable<IAccReceivable> accReceivables = await GetAccReceivableAmount(ReceiptNumber);
                            //if (accCollectionAllotments == null || accPayables == null || !accReceivables.Any())
                            //{
                            //    return "Data not present in Allotment";
                            //}
                            IEnumerable<IAccCollectionAllotment> allot = await GetCashAllot(accCollection1.AccCollection.UID);
                            // var Res = TargetUID == Const.Cash ? await UpdateReverseCashCollection(ChequeNo, conn, transaction) : await UpdateReverseChequeCollection(ChequeNo, conn, transaction);
                            //if (Res != Const.One)
                            //{
                            //    return Res;
                            //}

                            //await UpdateAllot(list.AccCollectionUID, list.PaidAmount, list.TargetUID, conn, transaction);
                            foreach (var list in allot)
                            {
                                var sql2 = @"INSERT INTO acc_collection_allotment (uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy,
                                modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, acc_collection_uid AS AccCollectionUID, 
                                target_type AS TargetType, target_uid AS TargetUID, reference_number AS ReferenceNumber, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID,
                                default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount, 
                                early_payment_discount_percentage AS EarlyPaymentDiscountPercentage, early_payment_discount_amount AS EarlyPaymentDiscountAmount, amount AS Amount) 
                                VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @AccCollectionUID, @TargetType, 
                                @TargetUID, @ReferenceNumber, @CurrencyUID, @DefaultCurrencyUID, @DefaultCurrencyExchangeRate, @DefaultCurrencyAmount,
                                @EarlyPaymentDiscountPercentage, @EarlyPaymentDiscountAmount, @Amount)";
                                Dictionary<string, object?> parameters2 = new Dictionary<string, object?>
                {
                                {"AccCollectionUID", accCollection1.AccCollection.UID},
                                {"TargetType", list.TargetType == null ? "" : list.TargetType},
                                {"TargetUID", list.UID == null ? "" : list.UID},
                                {"ReferenceNumber",  list.ReferenceNumber == null ? "" : list.ReferenceNumber},
                                {"CurrencyUID", accCollection1.AccCollection.CurrencyUID==null ? "" : accCollection1.AccCollection.CurrencyUID},
                                {"DefaultCurrencyUID", accCollection1.AccCollection.DefaultCurrencyUID==null ? "" : accCollection1.AccCollection.DefaultCurrencyUID},
                                {"DefaultCurrencyExchangeRate", accCollection1.AccCollection.DefaultCurrencyExchangeRate==0 ? 0 : accCollection1.AccCollection.DefaultCurrencyExchangeRate},
                                {"DefaultCurrencyAmount", accCollection1.AccCollection.DefaultCurrencyAmount==0 ? 0 : accCollection1.AccCollection.DefaultCurrencyAmount},
                                {"EarlyPaymentDiscountPercentage", list.EarlyPaymentDiscountPercentage==0 ? 0 : list.EarlyPaymentDiscountPercentage },
                                {"EarlyPaymentDiscountAmount", list.EarlyPaymentDiscountAmount==0 ? 0 : list.EarlyPaymentDiscountAmount },
                                {"Amount", list.PaidAmount * Const.Num },
                                {"CreatedTime", DateTime.Now },
                                {"ModifiedTime", DateTime.Now},
                                {"ModifiedBy", Const.ModifiedBy },
                                {"ServerAddTime", DateTime.Now },
                                {"ServerModifiedTime", DateTime.Now },
                                {"CreatedBy", Const.ModifiedBy },
                                {"UID", (Guid.NewGuid()).ToString()}
                            };
                                Retval = await CommonMethod(sql2, conn, parameters2, transaction);

                                if (Retval != Const.One)
                                    return Retval;
                                if (list.TargetType.Contains("INVOICE"))
                                {
                                    IEnumerable<IAccPayable> payb = await GetChequePay(list.TargetUID);
                                    foreach (var list1 in payb)
                                    {
                                        var sql4 = @"UPDATE acc_payable
                                    SET modified_time = @ModifiedTime,
                                    server_modified_time"" = @ServerModifiedTime, paid_amount = @PaidAmount WHERE uid = @UID ";
                                        Dictionary<string, object?> parameters4 = new Dictionary<string, object?>
                                    {
                                        {"PaidAmount", list1.PaidAmount+list.Amount*Const.Num == 0 ? 0 : list1.PaidAmount+list.Amount*Const.Num},
                                        {"ModifiedTime", DateTime.Now},
                                        {"ServerModifiedTime", DateTime.Now},
                                        {"UID", list1.UID}
                                };
                                        Retval = await CommonMethod(sql4, conn, parameters4, transaction);
                                        if (Retval != Const.One)
                                        {
                                            return Retval;
                                        }
                                    }
                                }
                                else
                                {
                                    IEnumerable<IAccReceivable> recv = await GetChequeRec(list.TargetUID);
                                    foreach (var list1 in recv)
                                    {
                                        var sql5 = @"UPDATE acc_receivable
                                    SET modified_time = @ModifiedTime,
                                    server_modified_time = @ServerModifiedTime,paid_amount = @PaidAmount
                                    WHERE uid = @UID";
                                        Dictionary<string, object?> parameters5 = new Dictionary<string, object?>
                            {
                                {"PaidAmount", list1.PaidAmount+list.Amount*Const.Num == 0 ? 0 : list1.PaidAmount+list.Amount*Const.Num},
                                {"ModifiedTime",DateTime.Now},
                                {"ServerModifiedTime", DateTime.Now},
                                {"UID",  list1.UID}
                            };
                                        Retval = await CommonMethod(sql5, conn, parameters5, transaction);
                                        if (Retval != Const.One)
                                        {
                                            return Retval;
                                        }
                                    }
                                }
                            }

                            transaction.Commit();
                            return Const.SuccessInsert;
                        }
                        else
                        {
                            transaction.Rollback();
                            return Const.ReversalMsg;
                        }
                    }
                    else
                    {
                        transaction.Rollback();
                        return Const.ParamMissing;
                    }

                }
            }
        }
        public async Task<string> UpdatePaymentModeDetails(IAccCollectionPaymentMode collection)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    var Retval = "";
                    var sql = @"UPDATE acc_collection_payment_mode SET created_by = @CreatedBy, created_time = @CreatedTime, modified_by = @ModifiedBy, 
                            modified_time = @ModifiedTime, server_add_time = @ServerAddTime, server_modified_time = @ServerModifiedTime, 
                            acc_collection_uid = @AccCollectionUID, branch = @Branch, cheque_no = @ChequeNo, cheque_date = @ChequeDate, bank_uid = @BankUID 
                            WHERE uid = @UID";
                    Dictionary<string, object?> parameters = new Dictionary<string, object?>
                    {
                            {"AccCollectionUID", collection.AccCollectionUID},
                            {"BankUID", collection.BankUID},
                            {"Branch", collection.Branch},
                            {"ChequeNo", collection.ChequeNo},
                            {"ChequeDate", collection.ChequeDate},
                            {"CreatedTime", DateTime.Now},
                            {"ModifiedTime", DateTime.Now},
                            {"ModifiedBy", Const.ModifiedBy},
                            {"ServerAddTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now},
                            {"CreatedBy", Const.ModifiedBy},
                            {"UID", collection.UID}
                    };
                    Retval = await CommonMethod(sql, conn, parameters, transaction);
                    if (Retval != Const.One)
                    {
                        return Retval;
                    }
                    transaction.Commit();

                    return Const.Success;
                }
            }
        }
        //1st level of approval like settled
        public async Task<string> ValidateChequeReceiptByPaymentMode(string UID, string Button, string Comments, string SessionUserCode, string CashNumber)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    if (UID != null && UID != "")
                    {
                        if (Button == "Approve")
                        {
                            var Retval = "";
                            var sql = @"UPDATE acc_collection_payment_mode 
                                SET modified_time = @ModifiedTime,
                                    server_modified_time = @ServerModifiedTime, 
                                    comments = @Comments,
                                    status = @Status 
                                WHERE acc_collection_uid = @UID";

                            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                    {
                            {"UID", UID},
                            {"Status", "Settled" },
                            {"Comments", Comments },
                            {"ModifiedTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now}
                    };
                            Retval = await CommonMethod(sql, conn, parameters, transaction);
                            if (Retval != Const.One)
                            {
                                return Retval;
                            }
                            //IEnumerable<ICollections> accCollection = await GetAmountCheque(CashNumber);
                            //IEnumerable<IAccCollectionAllotment> accCollectionAllotments = await GetChequeDetails(CashNumber);
                            //foreach (var list in accCollection)
                            //{
                            //    var sql1 = @"INSERT INTO ""AccCollectionSettlement"" (""UID"", ""AccCollectionUID"",""SettledBy"",
                            //        ""CreatedBy"", ""CreatedTime"", ""ModifiedBy"", ""ModifiedTime"", ""ServerAddTime"", 
                            //        ""ServerModifiedTime"", ""SessionUserCode"", ""Route"", ""PaymentMode"", ""ReceivedAmount"", ""ReceiptNumber"", ""IsVoid"", ""CashNumber"")
                            //        VALUES(@UID, @listUID, @SettledBy,  
                            //        @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @SessionUserCode,
                            //        @Route, @PaymentMode, @ReceivedAmount, @ReceiptNumber, @IsVoid, @CashNumber)";
                            //    Dictionary<string, object?> parameters1 = new Dictionary<string, object?>
                            //{
                            //   {"listUID",UID},
                            //   {"UID",(Guid.NewGuid()).ToString()},
                            //   {"SettledBy", list.SessionUserCode==null?"":list.SessionUserCode},
                            //   {"ReceivedAmount",list.PaidAmount==0?0:list.PaidAmount},
                            //   {"Route",list.SessionUserCode==null?"":list.SessionUserCode},
                            //   {"PaymentMode",list.Category==null?"":list.Category},
                            //   {"ReceiptNumber",list.ReceiptNumber==null?"":list.ReceiptNumber},
                            //   {"CashNumber",CashNumber==null? "": CashNumber},
                            //   {"CreatedTime", DateTime.Now},
                            //   {"ModifiedTime", DateTime.Now},
                            //   {"ModifiedBy", Const.ModifiedBy},
                            //   {"ServerAddTime", DateTime.Now},
                            //   {"ServerModifiedTime", DateTime.Now},
                            //   {"CreatedBy", Const.ModifiedBy},
                            //   {"SessionUserCode", list.SessionUserCode==null? "" : list.SessionUserCode},
                            //   {"IsVoid",false}
                            //};

                            //    Retval = await CommonMethod(sql1, conn, parameters1, transaction);
                            //    if (Retval != Const.One)
                            //    {
                            //        return Retval;
                            //    }
                            //}
                            var res = await UpdateChequeCollection(UID, Button, conn, transaction);
                            if (res != Const.One)
                            {
                                return res;
                            }
                        }
                        if (Button == "Reject")
                        {
                            var Retval = "";
                            var sql = @"UPDATE acc_collection_payment_mode 
                                SET modified_time = @ModifiedTime,
                                    server_modified_time = @ServerModifiedTime, 
                                    comments = @Comments,
                                    status = @Status 
                                WHERE acc_collection_uid = @UID";
                            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                    {
                            {"UID", UID},
                            {"Status", "Rejected" },
                            {"Comments", Comments },
                            {"ModifiedTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now}
                    };
                            Retval = await CommonMethod(sql, conn, parameters, transaction);
                            if (Retval != Const.One)
                            {
                                return Retval;
                            }
                            //IEnumerable<ICollections> accCollection = await GetAmountCheque(CashNumber);
                            //IEnumerable<IAccCollectionAllotment> accCollectionAllotments = await GetChequeDetails(CashNumber);
                            //foreach (var list in accCollection)
                            //{
                            //    var sql1 = @"INSERT INTO ""AccCollectionSettlement"" (""UID"", ""AccCollectionUID"",""SettledBy"",
                            //        ""CreatedBy"", ""CreatedTime"", ""ModifiedBy"", ""ModifiedTime"", ""ServerAddTime"", 
                            //        ""ServerModifiedTime"", ""SessionUserCode"", ""Route"", ""PaymentMode"", ""ReceivedAmount"", ""ReceiptNumber"", ""IsVoid"", ""CashNumber"")
                            //        VALUES(@UID, @listUID, @SettledBy,  
                            //        @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @SessionUserCode,
                            //        @Route, @PaymentMode, @ReceivedAmount, @ReceiptNumber, @IsVoid, @CashNumber)";
                            //    Dictionary<string, object?> parameters1 = new Dictionary<string, object?>
                            //{
                            //   {"listUID",UID},
                            //   {"UID",(Guid.NewGuid()).ToString()},
                            //   {"SettledBy", list.SessionUserCode==null?"":list.SessionUserCode},
                            //   {"ReceivedAmount",list.PaidAmount==0?0:list.PaidAmount},
                            //   {"Route",list.SessionUserCode==null?"":list.SessionUserCode},
                            //   {"PaymentMode",list.Category==null?"":list.Category},
                            //   {"ReceiptNumber",list.ReceiptNumber==null?"":list.ReceiptNumber},
                            //   {"CashNumber",CashNumber==null? "": CashNumber},
                            //   {"CreatedTime", DateTime.Now},
                            //   {"ModifiedTime", DateTime.Now},
                            //   {"ModifiedBy", Const.ModifiedBy},
                            //   {"ServerAddTime", DateTime.Now},
                            //   {"ServerModifiedTime", DateTime.Now},
                            //   {"CreatedBy", Const.ModifiedBy},
                            //   {"SessionUserCode", list.SessionUserCode==null? "" : list.SessionUserCode},
                            //   {"IsVoid",false}
                            //};

                            //    Retval = await CommonMethod(sql1, conn, parameters1, transaction);
                            //    if (Retval != Const.One)
                            //    {
                            //        return Retval;
                            //    }
                            //}
                            var res = await UpdateChequeCollection(UID, Button, conn, transaction);

                            var Res = await UpdatePayableUnsettle(UID, Button, conn, transaction);

                            if (res != Const.One && Res != Const.One)
                            {
                                return res;
                            }
                        }
                        transaction.Commit();
                        return Const.Success;
                    }
                    else
                    {
                        transaction.Rollback();
                        return Const.ParamMissing;
                    }
                }
            }
        }
        public async Task<string> ValidatePOSReceiptByPaymentMode(string UID, string Comments, string SessionUserCode)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    if (UID != null && UID != "")
                    {
                        var Retval = "";
                        var sql = @"UPDATE acc_bank 
                            SET created_time = @CreatedTime,
                                modified_time = @ModifiedTime,
                                server_add_time = @ServerAddTime,
                                server_modified_time = @ServerModifiedTime,
                                status = @Status,
                                comments = @Comments,
                                session_user_code = @SessionUserCode 
                            WHERE uid = @UID";
                        Dictionary<string, object?> parameters = new Dictionary<string, object?>
                    {
                            {"UID", UID},
                            //{"ReceiptNumber", SessionUserCode},
                            {"Status", 1 },
                            {"Comments", Comments },
                            {"SessionUserCode", SessionUserCode },
                            {"CreatedTime", DateTime.Now},
                            {"ModifiedTime", DateTime.Now},
                            {"ServerAddTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now}
                    };
                        Retval = await CommonMethod(sql, conn, parameters, transaction);
                        if (Retval != Const.One)
                        {
                            return Retval;
                        }
                        transaction.Commit();
                        return Const.Success;
                    }
                    else
                    {
                        transaction.Rollback();
                        return Const.ParamMissing;
                    }
                }
            }
        }
        public async Task<string> ValidateONLINEReceiptByPaymentMode(string UID, string Comments, string SessionUserCode)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    if (UID != null && UID != "")
                    {
                        var Retval = "";
                        var sql = @"UPDATE acc_bank 
                            SET created_time = @CreatedTime,
                                modified_time = @ModifiedTime,
                                server_add_time = @ServerAddTime,
                                server_modified_time = @ServerModifiedTime,
                                status = @Status,
                                comments = @Comments,
                                session_user_code = @SessionUserCode 
                            WHERE uid = @UID";
                        Dictionary<string, object?> parameters = new Dictionary<string, object?>
                    {
                            {"UID", UID},
                            //{"ReceiptNumber", SessionUserCode},
                            {"Status", 1 },
                            {"Comments", Comments },
                            {"SessionUserCode", SessionUserCode },
                            {"CreatedTime", DateTime.Now},
                            {"ModifiedTime", DateTime.Now},
                            {"ServerAddTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now}
                    };
                        Retval = await CommonMethod(sql, conn, parameters, transaction);
                        if (Retval != Const.One)
                        {
                            return Retval;
                        }
                        transaction.Commit();
                        return Const.Success;
                    }
                    else
                    {
                        transaction.Rollback();
                        return Const.ParamMissing;
                    }
                }
            }
        }
        public async Task<Model.Interfaces.ICollections> GetCollecAmount(string UID)
        {

            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"UID",  UID}
                };

                var sql = @"SELECT 
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            receipt_number AS ReceiptNumber,
                            consolidated_receipt_number AS ConsolidatedReceiptNumber,
                            category AS Category,
                            amount AS Amount,
                            currency_uid AS CurrencyUid,
                            default_currency_uid AS DefaultCurrencyUid,
                            default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                            default_currency_amount AS DefaultCurrencyAmount,
                            org_uid AS OrgUid,
                            distribution_channel_uid AS DistributionChannelUid,
                            store_uid AS StoreUid,
                            route_uid AS RouteUid,
                            job_position_uid AS JobPositionUid,
                            emp_uid AS EmpUid,
                            collected_date AS CollectedDate,
                            status AS Status,
                            remarks AS Remarks,
                            reference_number AS ReferenceNumber,
                            is_realized AS IsRealized,
                            latitude AS Latitude,
                            longitude AS Longitude,
                            source AS Source,
                            is_multimode AS IsMultimode,
                            trip_date AS TripDate,
                            comments AS Comments,
                            salesman AS Salesman,
                            route AS Route,
                            reversal_receipt_uid AS ReversalReceiptUid,
                            cancelled_on AS CancelledOn
                        FROM 
                            acc_collection 
                        WHERE 
                            uid = @UID";


                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICollections>().GetType();
                Model.Interfaces.ICollections CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.ICollections>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                return (ICollections)ex;
            }
        }
        public async Task<string> UpdateUnSettledAmount(string UID, string ChequeNo)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {

                var Retval = "";
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    if (UID != null && UID != "")
                    {
                        IEnumerable<IAccCollectionAllotment> recv = await ReceivUnsettle(UID, ChequeNo);
                        foreach (var list in recv)
                        {
                            IAccPayable payable = await GetAccPayableAmount(list.StoreUID, list.ReferenceNumber);
                            var sql4 = @"UPDATE acc_payable 
                                SET modified_time = @ModifiedTime,
                                    server_modified_time = @ServerModifiedTime,
                                    paid_amount = @PaidAmount,
                                    unsettled_amount = @UnSettledAmount
                                WHERE uid = @UID ";
                            Dictionary<string, object?> parameters4 = new Dictionary<string, object?>
                            {
                                {"PaidAmount",list.Amount+payable.PaidAmount==0 ? 0 :list.Amount+payable.PaidAmount},
                                {"UnSettledAmount", payable.UnSettledAmount-list.Amount==0 ? 0 : payable.UnSettledAmount-list.Amount },
                                {"UID", payable.UID},
                                {"ModifiedTime", DateTime.Now},
                                {"ServerModifiedTime", DateTime.Now}
                            };
                            Retval = await CommonMethod(sql4, conn, parameters4, transaction);
                            if (Retval != Const.One)
                            {
                                return Retval;
                            }
                        }
                        transaction.Commit();
                        return Retval;
                    }
                    else
                    {
                        transaction.Rollback();
                        return Const.ParamMissing;
                    }
                }
            }
        }

        //2nd level of approval
        public async Task<string> ValidateChequeSettlement(string UID, string Comments, string Button, string SessionUserCode, string ReceiptUID, string ChequeNo)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    if ((UID != null && UID != "") || (ReceiptUID != null && ReceiptUID != ""))
                    {
                        var Retval = "";
                        var sql = @"UPDATE acc_collection_payment_mode 
                        SET modified_time = @ModifiedTime,
                            server_modified_time = @ServerModifiedTime, 
                            comments = @Comments,
                            status = @Status 
                        WHERE acc_collection_uid = @UID";
                        Dictionary<string, object?> parameters = new Dictionary<string, object?>
                    {
                            {"UID", UID},
                            {"Status", "Approved" },
                            {"Comments", Comments },
                            {"ModifiedTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now}
                    };
                        Dictionary<string, object?> parameters1 = new Dictionary<string, object?>
                    {
                            {"UID", UID},
                            {"Status", "Bounced" },
                            {"Comments", Comments },
                            {"ModifiedTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now}
                    };
                        Retval = Button == "Approved" ? await CommonMethod(sql, conn, parameters, transaction) : await CommonMethod(sql, conn, parameters1, transaction);
                        if (Retval != Const.One)
                        {
                            return Retval;
                        }
                        if (Button == "Approved")
                        {
                            var retVal = await UpdateUnSettledAmount(UID, ChequeNo);
                            var retVal1 = await UpdateChequeCollection(UID, Button, conn, transaction);
                            if (retVal != Const.One && retVal != Const.One)
                            {
                                return retVal;
                            }
                        }
                        else
                        {
                            var retVal = await UpdatePayableUnsettle(UID, ChequeNo, conn, transaction);
                            var retVal1 = await UpdateChequeCollection(UID, Button, conn, transaction);
                            transaction.Commit();
                            return Const.Success;
                        }
                        transaction.Commit();
                        return Const.Success;
                    }
                    else
                    {
                        transaction.Rollback();
                        return Const.ParamMissing;
                    }
                }
            }
        }
        public async Task<string> ValidatePOSSettlement(string UID, string Comments, string Status, string SessionUserCode)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    if (UID != null && UID != "")
                    {
                        ICollections acc = await GetCollecAmount(UID);
                        var Retval = "";
                        var sql = @"UPDATE acc_collection 
                        SET created_time = @CreatedTime,
                            modified_time = @ModifiedTime,
                            server_add_time = @ServerAddTime,
                            server_modified_time = @ServerModifiedTime,
                            status = @Status,
                            comments = @Comments 
                        WHERE uid = @UID ";
                        Dictionary<string, object?> parameters = new Dictionary<string, object?>
                    {
                            {"Status", Status },
                            {"UID", UID },
                            {"Comments", Comments },
                            {"CreatedTime", DateTime.Now},
                            {"ModifiedTime", DateTime.Now},
                            {"ServerAddTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now}
                    };
                        Retval = await CommonMethod(sql, conn, parameters, transaction);
                        if (Retval != Const.One)
                        {
                            return Retval;
                        }
                        if (Status == Const.One || Status == "Submitted")
                        {
                            //var retVal = await UpdateUnSettledAmount(UID);
                            //if (retVal != Const.One)
                            //{
                            //    return retVal;
                            //}
                        }
                        else
                        {
                            transaction.Commit();
                            return Const.Success;
                        }
                        transaction.Commit();
                        return Const.Success;
                    }
                    else
                    {
                        transaction.Rollback();
                        return Const.ParamMissing;
                    }
                }
            }
        }
        public async Task<string> ValidateONLINESettlement(string UID, string Comments, string Status, string SessionUserCode)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    if (UID != null && UID != "")
                    {
                        ICollections acc = await GetCollecAmount(UID);
                        var Retval = "";
                        var sql = @"UPDATE acc_collection 
                            SET created_time = @CreatedTime,
                                modified_time = @ModifiedTime,
                                server_add_time = @ServerAddTime,
                                server_modified_time = @ServerModifiedTime,
                                status = @Status,
                                comments = @Comments 
                            WHERE uid = @UID";
                        Dictionary<string, object?> parameters = new Dictionary<string, object?>
                    {
                            {"Status", Status },
                            {"UID", UID },
                            {"Comments", Comments },
                            {"CreatedTime", DateTime.Now},
                            {"ModifiedTime", DateTime.Now},
                            {"ServerAddTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now}
                    };
                        Retval = await CommonMethod(sql, conn, parameters, transaction);
                        if (Retval != Const.One)
                        {
                            return Retval;
                        }
                        if (Status == Const.One || Status == "Submitted")
                        {
                            //var retVal = await UpdateUnSettledAmount(UID);
                            //if (retVal != Const.One)
                            //{
                            //    return retVal;
                            //}
                        }
                        else
                        {
                            transaction.Commit();
                            return Const.Success;
                        }
                        transaction.Commit();
                        return Const.Success;
                    }
                    else
                    {
                        transaction.Rollback();
                        return Const.ParamMissing;
                    }
                }
            }
        }
        //Collection Module end


        //to get bank names
        public async Task<IEnumerable<IBank>> GetBankNames()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {

            };

            var sql = @"SELECT 
                        id AS Id, uid AS UID, company_uid AS CompanyUID, bank_name AS BankName, country_uid AS CountryUID, 
                        cheque_fee AS ChequeFee, ss AS Ss, created_time AS CreatedTime, modified_time AS ModifiedTime, 
                        server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, bank_code AS BankCode
                        FROM bank";

            Type type = _serviceProvider.GetRequiredService<IBank>().GetType();
            IEnumerable<IBank> CollectionModuleList = await ExecuteQueryAsync<IBank>(sql, parameters, type);
            return CollectionModuleList;
        }

        //to update bank details when creating payment
        public async Task<int> UpdateChequeDetails(IAccCollectionPaymentMode collection)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    int Retval = 0;
                    var sql = @"INSERT INTO acc_collection_payment_mode ( uid, created_by, created_time, modified_by, modified_time, server_add_time,
                        server_modified_time, acc_collection_uid, bank_uid,  branch, cheque_no, amount, currency_uid, default_currency_uid, 
                        default_currency_exchange_rate, default_currency_amount, cheque_date, status, realization_date)
                        VALUES ( @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @AccCollectionUID, @BankUID, 
                        @Branch, @ChequeNo, @Amount, @CurrencyUID, @DefaultCurrencyUID, @DefaultCurrencyExchangeRate, @DefaultCurrencyAmount,
                        @ChequeDate, @Status, @RealizationDate)";
                    Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"AccCollectionUID", collection.AccCollectionUID==null ? "" : collection.AccCollectionUID },
                {"BankUID", collection.BankUID==null ? "" : collection.BankUID },
                {"Branch", collection.Branch==null ? "" : collection.Branch },
                {"ChequeNo", collection.ChequeNo==null ? "" : collection.ChequeNo },
                {"ChequeDate", collection.ChequeDate==null ? DateTime.Now : collection.ChequeDate },
                {"Amount", collection.Amount==0 ? 0 : collection.Amount },
                {"CurrencyUID",collection.CurrencyUID=="" ? "" : collection.CurrencyUID },
                {"DefaultCurrencyUID", collection.DefaultCurrencyUID==null ? "" : collection.DefaultCurrencyUID },
                {"DefaultCurrencyExchangeRate", collection.DefaultCurrencyExchangeRate==0 ? 0 : collection.DefaultCurrencyExchangeRate },
                {"DefaultCurrencyAmount", 0 },
                {"ChequeDate", (DateTime.Now).Date },
                {"Status", 0 },
                {"RealizationDate", collection.RealizationDate==null ? (DateTime.Now) : collection.RealizationDate },
                {"CreatedBy", collection.CreatedBy==null ? "" : collection.CreatedBy },
                {"ModifiedBy",collection.ModifiedBy==null ? "" : collection.ModifiedBy },
                {"CreatedTime", (DateTime.Now).Date },
                {"ModifiedTime", (DateTime.Now).Date },
                {"ServerAddTime", (DateTime.Now).Date },
                {"ServerModifiedTime", (DateTime.Now).Date },
                {"UID", (Guid.NewGuid()).ToString() }
            };
                    Retval = Convert.ToInt32(await CommonMethod(sql, conn, parameters, transaction));
                    if (Retval != 1)
                    {
                        return Retval;
                    }
                    transaction.Commit();
                    return Retval;
                }
            }
        }
        public async Task<PagedResponse<Model.Interfaces.IAccCollectionPaymentMode>> ShowPending(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, acc_collection_uid, bank_uid, branch, cheque_no, amount, currency_uid, default_currency_uid, default_currency_exchange_rate, default_currency_amount, cheque_date, status, realization_date, comments, approve_comments
	                    FROM acc_collection_payment_mode PM 
                        INNER JOIN acc_collection AC on AC.uid = PM.acc_collection_uid");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM acc_collection_payment_mode PM 
                                                INNER JOIN acc_collection AC on AC.uid = PM.acc_collection_uid");
                }
                var parameters = new Dictionary<string, object?>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE PM.status = 'Submitted' and AC.receipt_number not like '%OA%' And ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                else
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE PM.status = 'Submitted' and AC.receipt_number not like '%OA%'");
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY PM.created_time Desc, ");
                    AppendSortCriteria(sortCriterias, sql);
                }
                else
                {
                    sql.Append(" ORDER BY PM.created_time Desc");
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionPaymentMode>().GetType();
                IEnumerable<Model.Interfaces.IAccCollectionPaymentMode> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionPaymentMode>(sql.ToString(), parameters, type);
                int totalCount = Const.Num;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode> pagedResponse = new PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>
                {
                    PagedData = CollectionModuleList,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<PagedResponse<Model.Interfaces.IAccCollectionPaymentMode>> ShowSettled(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                acc_collection_uid, bank_uid, branch, cheque_no, amount, currency_uid, default_currency_uid, default_currency_exchange_rate, 
                default_currency_amount,cheque_date, status, realization_date, comments, approve_comments
	            FROM acc_collection_payment_mode PM 
                INNER JOIN acc_collection AC on AC.uid = PM.acc_collection_uid");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM acc_collection_payment_mode PM");
                }
                var parameters = new Dictionary<string, object?>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE PM.status = 'Settled' And ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                else
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE PM.status = 'Settled'");
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY PM.createdTime Desc, ");
                    AppendSortCriteria(sortCriterias, sql);
                }
                else
                {
                    sql.Append(" ORDER BY PM.createdTime Desc");
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionPaymentMode>().GetType();
                IEnumerable<Model.Interfaces.IAccCollectionPaymentMode> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionPaymentMode>(sql.ToString(), parameters, type);
                int totalCount = Const.Num;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode> pagedResponse = new PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>
                {
                    PagedData = CollectionModuleList,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<PagedResponse<Model.Interfaces.IAccCollectionPaymentMode>> ShowApproved(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                acc_collection_uid, bank_uid, branch, cheque_no, amount, currency_uid, default_currency_uid, default_currency_exchange_rate, 
                default_currency_amount,cheque_date, status, realization_date, comments, approve_comments
	            FROM acc_collection_payment_mode PM  
                        INNER JOIN acc_collection AC on AC.uid = PM.acc_collection_uid  as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                acc_collection_uid, bank_uid, branch, cheque_no, amount, currency_uid, default_currency_uid, default_currency_exchange_rate, 
                default_currency_amount,cheque_date, status, realization_date, comments, approve_comments
	            FROM acc_collection_payment_mode PM ) as SubQuery");
                }
                var parameters = new Dictionary<string, object?>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE PM.status = 'Approved' And ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                else
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE PM.status = 'Approved'");
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY PM.createdTime Desc, ");
                    AppendSortCriteria(sortCriterias, sql);
                }
                else
                {
                    sql.Append(" ORDER BY PM.createdTime Desc");
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionPaymentMode>().GetType();
                IEnumerable<Model.Interfaces.IAccCollectionPaymentMode> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionPaymentMode>(sql.ToString(), parameters, type);
                int totalCount = Const.Num;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode> pagedResponse = new PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>
                {
                    PagedData = CollectionModuleList,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<PagedResponse<Model.Interfaces.IAccCollectionPaymentMode>> ShowRejected(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                    acc_collection_uid, bank_uid, branch, cheque_no, amount, currency_uid, default_currency_uid, default_currency_exchange_rate, 
                    default_currency_amount, cheque_date, status, realization_date, comments, approve_comments
	                    FROM acc_collection_payment_mode PM 
                        INNER JOIN acc_collection AC on AC.uid = PM.Acc_Collection_UID  as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                    acc_collection_uid, bank_uid, branch, cheque_no, amount, currency_uid, default_currency_uid, default_currency_exchange_rate, 
                    default_currency_amount, cheque_date, status, realization_date, comments, approve_comments
	                    FROM acc_collection_payment_mode PM)  as SubQuery");
                }
                var parameters = new Dictionary<string, object?>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE PM.status = 'Rejected' And ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                else
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE PM.status = 'Rejected'");
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY PM.created_time Desc");
                    AppendSortCriteria(sortCriterias, sql);
                }
                else
                {
                    sql.Append(" ORDER BY PM.created_time Desc");
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionPaymentMode>().GetType();
                IEnumerable<Model.Interfaces.IAccCollectionPaymentMode> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionPaymentMode>(sql.ToString(), parameters, type);
                int totalCount = Const.Num;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode> pagedResponse = new PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>
                {
                    PagedData = CollectionModuleList,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<PagedResponse<Model.Interfaces.IAccCollectionPaymentMode>> ShowBounced(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                    acc_collection_uid, bank_uid, branch, cheque_no, amount, currency_uid, default_currency_uid, default_currency_exchange_rate, 
                    default_currency_amount, cheque_date, status, realization_date, comments, approve_comments
	                    FROM acc_collection_payment_mode PM 
                        INNER JOIN acc_collection AC on AC.uid = PM.acc_collection_uid  as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                    acc_collection_uid, bank_uid, branch, cheque_no, amount, currency_uid, default_currency_uid, default_currency_exchange_rate, 
                    default_currency_amount, cheque_date, status, realization_date, comments, approve_comments
	                    FROM acc_collection_payment_mode PM)  as SubQuery");
                }
                var parameters = new Dictionary<string, object?>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE PM.status = 'Bounced' And ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                else
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE PM.status = 'Bounced'");
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY PM.created_time Desc, ");
                    AppendSortCriteria(sortCriterias, sql);
                }
                else
                {
                    sql.Append(" ORDER BY PM.created_time Desc");
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionPaymentMode>().GetType();
                IEnumerable<Model.Interfaces.IAccCollectionPaymentMode> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionPaymentMode>(sql.ToString(), parameters, type);
                int totalCount = Const.Num;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode> pagedResponse = new PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>
                {
                    PagedData = CollectionModuleList,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Model.Interfaces.IAccUser>> GetUser()
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {

            };
            var sql = @"SELECT 
                        user_name AS UserName,
                        user_code AS UserCode,
                        password AS Password
                    FROM 
                        acc_user";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccUser>().GetType();
            IEnumerable<Model.Interfaces.IAccUser> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccUser>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.IAccCollectionPaymentMode>> GetChequeDetails(string UID, string TargetUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"UID", UID },
                {"TargetUID", TargetUID }
            };
            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        acc_collection_uid AS AccCollectionUid,
                        bank_uid AS BankUid,
                        branch AS Branch,
                        cheque_no AS ChequeNo,
                        amount AS Amount,
                        currency_uid AS CurrencyUid,
                        default_currency_uid AS DefaultCurrencyUid,
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                        default_currency_amount AS DefaultCurrencyAmount,
                        cheque_date AS ChequeDate,
                        status AS Status,
                        realization_date AS RealizationDate,
                        comments AS Comments,
                        approve_comments AS ApproveComments
                    FROM 
                        acc_collection_payment_mode 
                    WHERE 
                        acc_collection_uid = @UID ";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionPaymentMode>().GetType();
            IEnumerable<Model.Interfaces.IAccCollectionPaymentMode> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionPaymentMode>(sql, parameters, type);
            return CollectionModuleList;
        }

        public async Task<IEnumerable<Model.Interfaces.IAccCollection>> IsReversal(string UID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"UID", UID }
            };
            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        receipt_number AS ReceiptNumber,
                        consolidated_receipt_number AS ConsolidatedReceiptNumber,
                        category AS Category,
                        amount AS Amount,
                        currency_uid AS CurrencyUid,
                        default_currency_uid AS DefaultCurrencyUid,
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                        default_currency_amount AS DefaultCurrencyAmount,
                        org_uid AS OrgUid,
                        distribution_channel_uid AS DistributionChannelUid,
                        store_uid AS StoreUid,
                        route_uid AS RouteUid,
                        job_position_uid AS JobPositionUid,
                        emp_uid AS EmpUid,
                        collected_date AS CollectedDate,
                        status AS Status,
                        remarks AS Remarks,
                        reference_number AS ReferenceNumber,
                        is_realized AS IsRealized,
                        latitude AS Latitude,
                        longitude AS Longitude,
                        source AS Source,
                        is_multimode AS IsMultimode,
                        trip_date AS TripDate,
                        comments AS Comments,
                        salesman AS Salesman,
                        route AS Route,
                        reversal_receipt_uid AS ReversalReceiptUid,
                        cancelled_on AS CancelledOn
                    FROM 
                        acc_collection 
                    WHERE 
                        uid = @UID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
            IEnumerable<Model.Interfaces.IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.IAccCollection>> IsReversalCash(string UID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"UID", UID }
            };
            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        receipt_number AS ReceiptNumber,
                        consolidated_receipt_number AS ConsolidatedReceiptNumber,
                        category AS Category,
                        amount AS Amount,
                        currency_uid AS CurrencyUid,
                        default_currency_uid AS DefaultCurrencyUid,
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                        default_currency_amount AS DefaultCurrencyAmount,
                        org_uid AS OrgUid,
                        distribution_channel_uid AS DistributionChannelUid,
                        store_uid AS StoreUid,
                        route_uid AS RouteUid,
                        job_position_uid AS JobPositionUid,
                        emp_uid AS EmpUid,
                        collected_date AS CollectedDate,
                        status AS Status,
                        remarks AS Remarks,
                        reference_number AS ReferenceNumber,
                        is_realized AS IsRealized,
                        latitude AS Latitude,
                        longitude AS Longitude,
                        source AS Source,
                        is_multimode AS IsMultimode,
                        trip_date AS TripDate,
                        comments AS Comments,
                        salesman AS Salesman,
                        route AS Route,
                        reversal_receipt_uid AS ReversalReceiptUid,
                        cancelled_on AS CancelledOn
                    FROM 
                        acc_collection 
                    WHERE 
                        cash_number = @UID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
            IEnumerable<Model.Interfaces.IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql, parameters, type);
            return CollectionModuleList;
        }

        //to update reversal to false
        //public async Task<IEnumerable<Model.Interfaces.IAccCollection>> IsReversal1(string UID, string Reason, SqliteConnection conn, SqliteTransaction transaction)
        //{
        //    Dictionary<string, object?> parameters = new Dictionary<string, object?>
        //    {
        //        {"UID", UID }
        //    };
        //    var sql = @"select * from ""AccCollection"" where ""ChequeNo"" = @UID";

        //    Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
        //    IEnumerable<Model.Interfaces.IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql, parameters, type);
        //    foreach (var list in CollectionModuleList)
        //    {
        //        await UpdateCollection(list.UID, Reason, conn, transaction);
        //    }
        //    return CollectionModuleList;

        //}
        public async Task<IEnumerable<Model.Interfaces.IAccCollection>> IsReversal2(string UID, string ReverseNo, string Reason, SqliteConnection conn, SqliteTransaction transaction, string Status = null)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"UID", UID }
            };
            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        receipt_number AS ReceiptNumber,
                        consolidated_receipt_number AS ConsolidatedReceiptNumber,
                        category AS Category,
                        amount AS Amount,
                        currency_uid AS CurrencyUid,
                        default_currency_uid AS DefaultCurrencyUid,
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                        default_currency_amount AS DefaultCurrencyAmount,
                        org_uid AS OrgUid,
                        distribution_channel_uid AS DistributionChannelUid,
                        store_uid AS StoreUid,
                        route_uid AS RouteUid,
                        job_position_uid AS JobPositionUid,
                        emp_uid AS EmpUid,
                        collected_date AS CollectedDate,
                        status AS Status,
                        remarks AS Remarks,
                        reference_number AS ReferenceNumber,
                        is_realized AS IsRealized,
                        latitude AS Latitude,
                        longitude AS Longitude,
                        source AS Source,
                        is_multimode AS IsMultimode,
                        trip_date AS TripDate,
                        comments AS Comments,
                        salesman AS Salesman,
                        route AS Route,
                        reversal_receipt_uid AS ReversalReceiptUid,
                        cancelled_on AS CancelledOn
                    FROM 
                        acc_collection 
                    WHERE 
                        uid = @UID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
            IEnumerable<Model.Interfaces.IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql, parameters, type);
            foreach (var list in CollectionModuleList)
            {
                await UpdateCollection(list.UID, ReverseNo, Reason, conn, transaction, Status);
            }
            return CollectionModuleList;

        }
        //to update isreversal to false
        public async Task<string> UpdateCollection(string UID, string ReverseNo, string Reason, SqliteConnection conn, SqliteTransaction transaction, string Status = null)
        {
            var Retval = "";
            if (UID != null && UID != "")
            {
                ICollections accCollection = await GetCollectAmount(UID);

                var sql4 = @"UPDATE acc_collection
                SET reversal_receipt_uid = @ReversalReceiptUID,
                    modified_time = @ModifiedTime,
                    comments = @Comments,
                    status = @Status,
                    is_realized = @IsRealized
                WHERE uid = @UID";
                Dictionary<string, object?> parameters4 = new Dictionary<string, object?>
                            {
                                {"ReversalReceiptUID", ReverseNo},
                                {"Status",Status != null ? "Voided" : "Reversed"},
                                {"IsRealized",false},
                                {"ModifiedTime", DateTime.Now},
                                {"Comments", Reason},
                                {"UID", UID}
                            };
                Retval = await CommonMethod(sql4, conn, parameters4, transaction);
                if (Retval != Const.One)
                {
                    return Retval;
                }
            }
            return Retval;
        }

        public async Task<string> UpdateCashCollection(string CashNumber, string SessionUserCode, SqliteConnection conn, SqliteTransaction transaction)
        {
            var Retval = "";
            if (CashNumber != null && CashNumber != "")
            {
                var sql4 = @"UPDATE acc_collection
                    SET status = @Status,
                        modified_time = @ModifiedTime
                    WHERE uid = @UID";
                Dictionary<string, object?> parameters4 = new Dictionary<string, object?>
                            {
                                {"Status", "Settled"},
                                {"ModifiedTime", DateTime.Now},
                                {"UID", CashNumber}
                            };
                Retval = await CommonMethod(sql4, conn, parameters4, transaction);
                if (Retval != Const.One)
                {
                    return Retval;
                }
            }
            return Retval;
        }
        //pending approving time getting details
        public async Task<string> UpdateChequeCollection(string UID, string Button, SqliteConnection conn, SqliteTransaction transaction)
        {
            var Retval = "";
            if (UID != null && UID != "")
            {
                var sql4 = @"UPDATE acc_collection
                    SET status = @Status,
                        is_realized = @IsRealized
                    WHERE uid = @UID";
                Dictionary<string, object?> parameters4 = new Dictionary<string, object?>
                            {
                                {"Status", Button == "Approve" ? "Settled" : Button == "Approved" ? "Approved" : Button == "Bounced" ? "Bounced" : "Rejected"},
                                {"IsRealized", Button == "Approve" ? false : Button == "Approved" ? true : Button == "Bounced" ? false : false},
                                {"UID", UID}
                            };
                Retval = await CommonMethod(sql4, conn, parameters4, transaction);
                if (Retval != Const.One)
                {
                    return Retval;
                }
            }
            return Retval;
        }

        //for rejected or bounced records unsettleamount updation
        public async Task<string> UpdatePayableUnsettle(string AccCollectionUID, string Button, SqliteConnection conn, SqliteTransaction transaction)
        {
            var Retval = "";
            if (AccCollectionUID != null && AccCollectionUID != "")
            {
                IEnumerable<IAccPayable> pay = await PayableUnsettle(AccCollectionUID);
                foreach (var list in pay)
                {
                    var sql4 = @"UPDATE acc_payable
                        SET unsettled_amount = @UnSettledAmount    
                        WHERE uid = @UID";
                    Dictionary<string, object?> parameters4 = new Dictionary<string, object?>
                            {
                                {"UnSettledAmount", 0},
                                {"UID", list.UID}
                            };
                    Retval = await CommonMethod(sql4, conn, parameters4, transaction);
                    if (Retval != Const.One)
                    {
                        return Retval;
                    }
                }
            }
            return Retval;
        }
        public async Task<string> CommonMethod(string sql, SqliteConnection conn, Dictionary<string, object?> parameters, SqliteTransaction transaction)
        {
            int retValue = 0;
            try
            {
                using (var cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Transaction = transaction;
                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
                        }
                    }
                    retValue = await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return ex.ToString();
            }
            return retValue.ToString();
        }

        public async Task<IEnumerable<IAccPayable>> DaysTable(string StoreUID)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                     {"StoreUID",  StoreUID}
                };

                var sql = @"WITH DelayTimeCategories AS (SELECT '0-30 days' AS delay_time
                                                          UNION SELECT '30-60 days' AS delay_time
                                                          UNION SELECT '60-90 days' AS delay_time
                                                          UNION SELECT '90+ days' AS delay_time)
                    SELECT
                      DT.delay_time AS ""DelayTime"",
                      COALESCE(COUNT(AP.transaction_date), 0) AS ""Count"",
                      COALESCE(SUM(AP.balance_amount), 0) AS ""Balance"",
                      AP.store_uid AS ""StoreUID""
                    FROM DelayTimeCategories DT
                    LEFT JOIN acc_payable AP
                      ON DT.delay_time =
                        CASE
                          WHEN AGE(CURRENT_DATE, AP.transaction_date) BETWEEN '0 days' AND '30 days' THEN '0-30 days'
                          WHEN AGE(CURRENT_DATE, AP.transaction_date) BETWEEN '30 days' AND '60 days' THEN '30-60 days'
                          WHEN AGE(CURRENT_DATE, AP.transaction_date) BETWEEN '60 days' AND '90 days' THEN '60-90 days'
                          WHEN AGE(CURRENT_DATE, AP.transaction_date) > '90' THEN '90+ days'
                          ELSE 'Unknown'
                        END
                    WHERE AP.store_uid = @StoreUID OR AP.store_uid IS NULL
                    GROUP BY DT.delay_time, AP.store_uid";
                IEnumerable<Model.Interfaces.IAccPayable> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql, parameters);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IAccPayable> ExcelBalance(string ReceiptNumber, string StoreUID)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                     {"ReceiptNumber",  ReceiptNumber},
                     {"StoreUID",  StoreUID}
                };

                var sql = @"SELECT 
                                SUM(balance_amount) AS balance_amount,
                                SUM(unsettled_amount) AS unsettled_amount
                            FROM 
                                acc_payable
                            WHERE 
                                reference_number = @ReceiptNumber 
                                AND store_uid = @StoreUID";


                var sql1 = @"SELECT 
                                balance_amount,
                                unsettled_amount
                            FROM 
                                acc_payable
                            WHERE 
                                reference_number = @ReceiptNumber 
                                AND store_uid = @StoreUID";


                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
                Model.Interfaces.IAccPayable CollectionModuleList;
                if (ReceiptNumber.Contains("INVOICE"))
                {
                    CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IAccPayable>(sql1, parameters, type);
                }
                else
                {
                    CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IAccPayable>(sql, parameters, type);
                }
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<IEnumerable<IAccPayable>> DaysTableParent(string StoreUID, int startDay, int endDay)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                        {
                             {"StoreUID",  StoreUID},
                             {"startDay",  startDay},
                             {"endDay",  endDay}
                        };

                var sql = @"SELECT 
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            source_type AS SourceType,
                            source_uid AS SourceUid,
                            reference_number AS ReferenceNumber,
                            org_uid AS OrgUid,
                            job_position_uid AS JobPositionUid,
                            amount AS Amount,
                            paid_amount AS PaidAmount,
                            store_uid AS StoreUid,
                            transaction_date AS TransactionDate,
                            due_date AS DueDate,
                            balance_amount AS BalanceAmount,
                            unsettled_amount AS UnsettledAmount,
                            source AS Source,
                            currency_uid AS CurrencyUid
                        FROM 
                            acc_payable
                        WHERE 
                            store_uid = @StoreUID
                            AND CURRENT_DATE - transaction_date BETWEEN @startDay AND @endDay";

                var sql1 = @"SELECT 
                                id AS Id,
                                uid AS Uid,
                                created_by AS CreatedBy,
                                created_time AS CreatedTime,
                                modified_by AS ModifiedBy,
                                modified_time AS ModifiedTime,
                                server_add_time AS ServerAddTime,
                                server_modified_time AS ServerModifiedTime,
                                source_type AS SourceType,
                                source_uid AS SourceUid,
                                reference_number AS ReferenceNumber,
                                org_uid AS OrgUid,
                                job_position_uid AS JobPositionUid,
                                amount AS Amount,
                                paid_amount AS PaidAmount,
                                store_uid AS StoreUid,
                                transaction_date AS TransactionDate,
                                due_date AS DueDate,
                                balance_amount AS BalanceAmount,
                                unsettled_amount AS UnsettledAmount,
                                source AS Source,
                                currency_uid AS CurrencyUid
                            FROM 
                                acc_payable
                            WHERE 
                                store_uid = @StoreUID
                                AND CURRENT_DATE - transaction_date >= @startDay";

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
                if (endDay == 0)
                {
                    IEnumerable<Model.Interfaces.IAccPayable> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql1, parameters, type);
                    if (CollectionModuleList.Count() != 0)
                    {
                        IEnumerable<IAccPayable> credit = await GetCreditNotes(CollectionModuleList.ElementAt(0).StoreUID);
                        List<IAccPayable> newArray = new List<IAccPayable>();
                        newArray.AddRange(CollectionModuleList);
                        newArray.AddRange(credit);
                        IEnumerable<Model.Interfaces.IAccPayable> array = newArray.ToArray();
                        return array;
                    }
                    else
                    {
                        return CollectionModuleList;
                    }
                }
                else
                {
                    IEnumerable<Model.Interfaces.IAccPayable> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql, parameters, type);
                    if (CollectionModuleList.Count() != 0)
                    {
                        IEnumerable<IAccPayable> credit = await GetCreditNotes(CollectionModuleList.ElementAt(0).StoreUID);
                        List<IAccPayable> newArray = new List<IAccPayable>();
                        newArray.AddRange(CollectionModuleList);
                        newArray.AddRange(credit);
                        IEnumerable<Model.Interfaces.IAccPayable> array = newArray.ToArray();
                        return array;
                    }
                    else
                    {
                        return CollectionModuleList;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<IAccPayable>> GetCreditNotes(string AccCollectionUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"StoreUID",  AccCollectionUID}
                };

            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        source_type AS SourceType,
                        source_uid AS SourceUid,
                        reference_number AS ReferenceNumber,
                        org_uid AS OrgUid,
                        job_position_uid AS JobPositionUid,
                        currency_uid AS CurrencyUid,
                        amount AS Amount,
                        paid_amount AS PaidAmount,
                        store_uid AS StoreUid,
                        transaction_date AS TransactionDate,
                        due_date AS DueDate,
                        unsettled_amount AS UnsettledAmount,
                        balance_amount AS BalanceAmount,
                        source AS Source
                    FROM 
                        acc_receivable
                    WHERE 
                        source_uid = @StoreUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
            IEnumerable<IAccPayable> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<IAccCollectionPaymentMode>> GetUnSettleAmount(string AccCollectionUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"StoreUID",  AccCollectionUID}
                };

            var sql = @"SELECT 
                            pm.id,pm.uid, pm.created_by,pm.created_time, pm.modified_by, pm.modified_time, pm.server_add_time, pm.server_modified_time,
                            pm.acc_collection_uid, pm.bank_uid, pm.branch, pm.cheque_no, pm.amount,pm.currency_uid,pm.default_currency_uid,
                            pm.default_currency_exchange_rate, pm.default_currency_amount, pm.cheque_date, pm.status,pm.realization_date, pm.comments, pm.approve_comments
                        FROM 
                            acc_collection_payment_mode AS pm
                        INNER JOIN
                            acc_collection AS ac ON pm.acc_collection_uid = ac.uid
                        WHERE 
                            pm.status != 'Approved'
                            AND pm.status != 'OnAccount'
                            AND ac.store_uid = @StoreUID
                            AND ac.category != 'Cash'";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionPaymentMode>().GetType();
            IEnumerable<IAccCollectionPaymentMode> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionPaymentMode>(sql, parameters, type);
            return CollectionModuleList;
        }

        public async Task<IEnumerable<Winit.Modules.Setting.Model.Interfaces.ISetting>> GetSettingByType(string UID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        type AS Type,
                        name AS Name,
                        value AS Value,
                        data_type AS DataType,
                        is_editable AS IsEditable,
                        ss AS Ss,
                        created_time AS CreatedTime,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime
                    FROM 
                        setting
                    WHERE 
                         value = '1' and data_type = 'Boolean' and name like '%Enable_payment%'";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Setting.Model.Interfaces.ISetting>().GetType();
            IEnumerable<Winit.Modules.Setting.Model.Interfaces.ISetting> SettingDetails = await ExecuteQueryAsync<Winit.Modules.Setting.Model.Interfaces.ISetting>(sql, parameters, type);
            return SettingDetails;
        }

        public async Task<List<IAccPayable>> PopulateCollectionPage(string CustomerCode, string Tabs)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"CustomerCode",  CustomerCode}
                };
            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        source_type AS SourceType,
                        source_uid AS SourceUid,
                        reference_number AS ReferenceNumber,
                        org_uid AS OrgUid,
                        job_position_uid AS JobPositionUid,
                        amount AS Amount,
                        paid_amount AS PaidAmount,
                        store_uid AS StoreUid,
                        transaction_date AS TransactionDate,
                        due_date AS DueDate,
                        balance_amount AS BalanceAmount,
                        unsettled_amount AS UnsettledAmount,
                        source AS Source,
                        currency_uid AS CurrencyUid
                    FROM 
                        acc_payable
                    WHERE 
                        store_uid = @CustomerCode";

            var sql1 = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        source_type AS SourceType,
                        source_uid AS SourceUid,
                        reference_number AS ReferenceNumber,
                        org_uid AS OrgUid,
                        job_position_uid AS JobPositionUid,
                        amount AS Amount,
                        paid_amount AS PaidAmount,
                        store_uid AS StoreUid,
                        transaction_date AS TransactionDate,
                        due_date AS DueDate,
                        balance_amount AS BalanceAmount,
                        unsettled_amount AS UnsettledAmount,
                        source AS Source,
                        currency_uid AS CurrencyUid
                    FROM 
                        acc_payable
                    WHERE 
                        store_uid = @CustomerCode
                        AND DATE(due_date) <= DATE('now')";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
            List<IAccPayable> CollectionModuleList = Tabs == "All" ? await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql, parameters, type) : await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql1, parameters, type);
            return CollectionModuleList;
        }

        public async Task<List<Model.Interfaces.IAccPayable>> GetInvoicesMobile(string AccCollectionUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"StoreUID",  AccCollectionUID}
                };

            var sql = @"SELECT UID AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, 
                        server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, source_type AS SourceType, source_UID AS SourceUID,
                        reference_number AS ReferenceNumber, org_UID AS OrgUID, job_position_UID AS JobPositionUID, amount AS Amount, paid_amount AS PaidAmount, 
                        store_UID AS StoreUID, transaction_date AS TransactionDate, due_date AS DueDate, unsettled_amount AS UnsettledAmount, source AS Source,
                        (balance_amount - unsettled_amount) AS BalanceAmount FROM acc_payable
                        WHERE store_uid = @StoreUID 
                        UNION 
                        SELECT UID AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, source_type AS SourceType, source_UID AS SourceUID,
                        reference_number AS ReferenceNumber, org_UID AS OrgUID, job_position_UID AS JobPositionUID, amount AS Amount, paid_amount AS PaidAmount, 
                        store_UID AS StoreUID, transaction_date AS TransactionDate, due_date AS DueDate, unsettled_amount AS UnsettledAmount, source AS Source,
                        (balance_amount - unsettled_amount) AS BalanceAmount 
                        FROM acc_receivable 
                        WHERE store_UID = @StoreUID 
                        ORDER BY reference_number";


            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
            List<Model.Interfaces.IAccPayable> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql, parameters, type);
            return CollectionModuleList;
        }

        public async Task<List<IAccCollection>> ViewPayments()
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"StoreUID",  ""}
                };

            var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        receipt_number AS ReceiptNumber,
                        consolidated_receipt_number AS ConsolidatedReceiptNumber,
                        category AS Category,
                        amount AS Amount,
                        currency_uid AS CurrencyUid,
                        default_currency_uid AS DefaultCurrencyUid,
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                        default_currency_amount AS DefaultCurrencyAmount,
                        org_uid AS OrgUid,
                        distribution_channel_uid AS DistributionChannelUid,
                        store_uid AS StoreUid,
                        route_uid AS RouteUid,
                        job_position_uid AS JobPositionUid,
                        emp_uid AS EmpUid,
                        collected_date AS CollectedDate,
                        status AS Status,
                        remarks AS Remarks,
                        reference_number AS ReferenceNumber,
                        is_realized AS IsRealized,
                        latitude AS Latitude,
                        longitude AS Longitude,
                        source AS Source,
                        is_multimode AS IsMultimode,
                        trip_date AS TripDate,
                        comments AS Comments,
                        salesman AS Salesman,
                        route AS Route,
                        reversal_receipt_uid AS ReversalReceiptUid,
                        cancelled_on AS CancelledOn
                    FROM 
                        acc_collection 
                    ORDER BY 
                        created_time DESC";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
            List<IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<List<IAccCollectionAllotment>> ViewPaymentsDetails(string UID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"UID",  UID}
                };

            var sql = @"select AL.id, AL.uid, AL.created_by, AL.created_time, AL.modified_by, AL.modified_time, AL.server_add_time, AL.server_modified_time,
                AL.receipt_number, AL.consolidated_receipt_number, AL.category, AL.amount, AL.currency_uid, AL.default_currency_uid,
                AL.default_currency_exchange_rate, AL.default_currency_amount, AL.org_uid, AL.distribution_channel_uid, AL.store_uid,
                AL.route_uid, AL.job_position_uid, AL.emp_uid, AL.collected_date, AL.status, AL.remarks, AL.reference_number, AL.is_realized,
                AL.latitude, AL.longitude, AL.source, AL.is_multimode, AL.trip_date, AL.comments, AL.salesman, AL.route, AL.reversal_receipt_uid,
                AL.cancelled_on
                from acc_collection as AC 
                INNER JOIN acc_collection_allotment as AL 
                on AC.uid = AL.acc_collection_uid
                where AC.uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionAllotment>().GetType();
            List<IAccCollectionAllotment> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionAllotment>(sql, parameters, type);
            return CollectionModuleList;
        }



        public async Task<IEnumerable<IEarlyPaymentDiscountConfiguration>> CheckEligibleForDiscount(string ApplicableCode)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"ApplicableCode",  ApplicableCode}
                };

                var sql = @"SELECT 
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            sales_org AS SalesOrg,
                            applicable_type AS ApplicableType,
                            applicable_code AS ApplicableCode,
                            payment_mode AS PaymentMode,
                            advance_paid_days AS AdvancePaidDays,
                            discount_type AS DiscountType,
                            discount_value AS DiscountValue,
                            isactive AS IsActive,
                            valid_from AS ValidFrom,
                            valid_to AS ValidTo,
                            applicable_on_partial_payments AS ApplicableOnPartialPayments,
                            applicable_on_overdue_customers AS ApplicableOnOverdueCustomers
                        FROM 
                            early_payment_discount_configuration 
                        WHERE 
                            applicable_code = @ApplicableCode";
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IEarlyPaymentDiscountConfiguration>().GetType();
                IEnumerable<IEarlyPaymentDiscountConfiguration> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IEarlyPaymentDiscountConfiguration>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        public async Task<List<IAccCollectionPaymentMode>> ShowPendingRecordsInPopUp(string StoreUID, string InvoiceNumber)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"StoreUID",  StoreUID},
                    {"InvoiceNumber",  InvoiceNumber}
                };

                var sql = @"select AC.receipt_number AS ReceiptNumber, AC.modified_time AS ModifiedTime,AP.cheque_no as ChequeNo, AP.cheque_date as ChequeDate,
                           AC.default_currency_amount AS DefaultCurrencyAmount from acc_collection_payment_mode AP 
                           INNER JOIN acc_collection AC on AP.acc_collection_uid = AC.uid
                           INNER JOIN acc_payable ACP on AC.store_uid = ACP.store_uid 
                           where AC.store_uid = @StoreUID 
                           and ACP.reference_number = @InvoiceNumber AND
                           AC.category NOT IN  ('Cash','CREDITNOTE') and ACP.unsettled_amount != 0";
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionPaymentMode>().GetType();
                List<IAccCollectionPaymentMode> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionPaymentMode>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        public async Task<List<IAccPayable>> GetPendingRecordsFromDB(string StoreUID)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"StoreUID",  StoreUID}
                };
                var sql = @"SELECT 
                        id AS Id,
                        uid AS Uid,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        source_type AS SourceType,
                        source_uid AS SourceUid,
                        reference_number AS ReferenceNumber,
                        org_uid AS OrgUid,
                        job_position_uid AS JobPositionUid,
                        amount AS Amount,
                        paid_amount AS PaidAmount,
                        store_uid AS StoreUid,
                        transaction_date AS TransactionDate,
                        due_date AS DueDate,
                        balance_amount AS BalanceAmount,
                        unsettled_amount AS UnsettledAmount,
                        source AS Source,
                        currency_uid AS CurrencyUid
                    FROM 
                        acc_payable 
                    WHERE 
                        StoreUID = @StoreUID 
                        AND UnsettledAmount != 0";
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
                List<IAccPayable> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        public async Task<List<IAccCollectionPaymentMode>> CPOData(string AccCollectionUID)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"AccCollectionUID",  AccCollectionUID}
                };
                var sql = @"SELECT 
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            acc_collection_uid AS AccCollectionUid,
                            bank_uid AS BankUid,
                            branch AS Branch,
                            cheque_no AS ChequeNo,
                            amount AS Amount,
                            currency_uid AS CurrencyUid,
                            default_currency_uid AS DefaultCurrencyUid,
                            default_currency_exchange_rate AS DefaultCurrencyExchangeRate,
                            default_currency_amount AS DefaultCurrencyAmount,
                            cheque_date AS ChequeDate,
                            status AS Status,
                            realization_date AS RealizationDate,
                            comments AS Comments,
                            approve_comments AS ApproveComments
                        FROM 
                            acc_collection_payment_mode 
                        WHERE 
                            acc_collection_uid = @AccCollectionUID";
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionPaymentMode>().GetType();
                List<IAccCollectionPaymentMode> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionPaymentMode>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        public async Task<List<IExchangeRate>> GetCurrencyRateRecords(string StoreUID)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"StoreUID",  StoreUID}
                };
                var sql = @"select from_currency_uid as FromCurrencyUID, rate as Rate, to_currency_uid as ToCurrencyUID from exchange_rate";
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IExchangeRate>().GetType();
                List<IExchangeRate> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IExchangeRate>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        public async Task<List<IAccCollectionCurrencyDetails>> GetMultiCurrencyDetails(string AccCollectionUID)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"AccCollectionUID",  AccCollectionUID}
                };
                var sql = @"Select * from acc_collection_currency_details where acc_collection_uid = @AccCollectionUID";
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionCurrencyDetails>().GetType();
                List<IAccCollectionCurrencyDetails> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionCurrencyDetails>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }


        public async Task<List<ICollectionPrint>> GetCollectionStoreDataForPrinter(List<string> UID)
        {
            try
            {
                List<ICollectionPrint> PrintDetails = new List<ICollectionPrint>();
                foreach (var data in UID)
                {
                    ICollectionPrint CollectionModuleList = new CollectionPrint();
                    CollectionModuleList.collectionPrintDetails = new List<ICollectionPrintDetails>();

                    Dictionary<string, object?> parameters = new Dictionary<string, object?>
                    {
                        {"UID",  data},
                    };
                    Dictionary<string, object?> parametersList = new Dictionary<string, object?>
                    {
                        {"AccCollectionUID",  data},
                    };

                    var sql = @"SELECT receipt_number as ReceiptNumber,consolidated_receipt_number as ConsolidatedReceiptNumber, category as Category, Ac.amount as Amount,
                    Ac.currency_uid as CurrencyUID, route_uid as RouteUID, collected_date as CollectedDate, Ac.status as Status,
                    is_remote_collection as IsRemoteCollection, remote_collection_reason as RemoteCollectionReason,
                    S.code as Code, S.number as Number, S.name as Name, S.alias_name as AliasName, S.legal_name as LegalName, S.is_active as IsActive,
                    S.store_rating as StoreRating, S.country_uid as CountryUID, S.region_uid as RegionUID, S.city_uid as CityUID,
                    S.outlet_name as OutletName, S.arabic_name as ArabicName, S.tax_doc_number as TaxDocNumber, S.tax_type as TaxType,
                    A.line1 as Line1, A.line2 as Line2, A.line3 as Line3,A.landmark as Landmark,A.area as Area, A.sub_area as SubArea,
                    A.zip_code as ZipCode, A.city as City, A.country_code as CountryCode, A.region_code as RegionCode, A.phone as Phone, 
                    A.phone_extension as PhoneExtension, A.mobile1 as Mobile1, A.mobile2 as Mobile2, A.email as Email, A.state_code as StateCode,
                    A.territory_code as TerritoryCode, A.pan as PAN, A.aadhar as AADHAR, A.ssn as SSN,
                    PM.bank_uid as BankUID, PM.branch as Branch, PM.cheque_no as ChequeNo, PM.cheque_date as ChequeDate
                    FROM acc_collection AS Ac 
                    INNER JOIN store AS S ON Ac.store_uid = S.uid
                    INNER JOIN address AS A ON A.linked_item_uid = S.uid 
                    INNER JOIN acc_collection_payment_mode AS PM ON Ac.uid = PM.acc_collection_uid
                    WHERE Ac.uid = @UID";

                    var sqlList = @"Select AL.reference_number as ReferenceNumber, AL.target_type as TargetType, AL.amount as PaidAmount, 
                                    Ap.amount as TotalAmount from acc_collection_allotment AL INNER JOIN acc_payable Ap 
                                    ON Ap.uid = AL.target_uid Where AL.acc_collection_uid = @AccCollectionUID";

                    //var uidParam = new List<string>();
                    //for (int i = 0; i < UID.Count; i++)
                    //{
                    //    var parameterName = "@UID" + i;
                    //    uidParam.Add(parameterName);
                    //    parameters.Add(parameterName, UID[i]);
                    //    parametersList.Add(parameterName, UID[i]);
                    //}

                    //// Replace the placeholder in the SQL query with the actual placeholders
                    //sql = string.Format(sql, string.Join(", ", uidParam));
                    //sqlList = string.Format(sqlList, string.Join(", ", uidParam));


                    Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICollectionPrint>().GetType();
                    CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.ICollectionPrint>(sql, parameters, type);


                    Type typeList = _serviceProvider.GetRequiredService<Model.Interfaces.ICollectionPrintDetails>().GetType();
                    CollectionModuleList.collectionPrintDetails = await ExecuteQueryAsync<Model.Interfaces.ICollectionPrintDetails>(sqlList, parametersList, typeList);
                    PrintDetails.Add(CollectionModuleList);
                }
                return PrintDetails;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        public async Task<List<IPaymentSummary>> GetPaymentSummary(string FromDate, string ToDate)
        {
            try
            {
                DateTime From = DateTime.Parse(FromDate);
                DateTime To = DateTime.Parse(ToDate);
                string from = From.ToString("yyyy-MM-dd");
                string to = To.ToString("yyyy-MM-dd");
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"FromDate",  from},
                    {"ToDate",  to}
                };
                var sql = @"Select Emp.name as SalesManName, Emp.code as SalesManCode, AC.consolidated_receipt_number as ConsolidatedReceiptNumber,
                            AC.receipt_number as ReceiptNumber, AC.category as Category, AC.status as Status, AC.amount as Amount, S.name as StoreName,
                            S.code as StoreCode,  
                             CASE 
                                    WHEN AC.category = 'Cash' THEN AC.amount 
                                    ELSE 0 
                                END as CashAmount,
                                CASE 
                                    WHEN AC.category = 'Cheque' THEN AC.amount 
                                    ELSE 0 
                                END as ChequeAmount,
                                CASE 
                                    WHEN AC.category = 'POS' THEN AC.amount 
                                    ELSE 0 
                                END as POSAmount,
                                CASE 
                                    WHEN AC.category = 'Online' THEN AC.amount 
                                    ELSE 0 
                                END as OnlineAmount
                            	from emp Emp 
                            Inner Join job_position JB ON Emp.uid = JB.emp_uid
                            Inner Join acc_collection AC ON AC.emp_uid = Emp.uid
                            Inner Join store S ON S.uid = AC.store_uid
                            where Emp.uid = (select emp_uid from acc_collection) and Date(AC.created_time) BETWEEN Date(@FromDate) AND Date(@ToDate)";
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IPaymentSummary>().GetType();
                List<IPaymentSummary> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IPaymentSummary>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                throw new();
            }
        }

        public async Task<List<ICollectionPrintDetails>> GetAllotmentDataForPrinter(string AccCollectionUID)
        {
            try
            {
                throw new Exception();


            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        #region NotImplementedMethods

        Task<string> ICollectionModuleDL.CreateReceipt(ICollections collection)
        {
            throw new NotImplementedException();
        }

        Task<int> ICollectionModuleDL.CreateReceiptWithAutoAllocation(ICollections[] collection)
        {
            throw new NotImplementedException();
        }

        Task<List<IAccStoreLedger>> ICollectionModuleDL.GetAccountStatement(string StoreUID, string FromDate, string ToDate)
        {
            throw new NotImplementedException();
        }

        Task<List<IAccPayable>> ICollectionModuleDL.GetInvoices(string StoreUID)
        {
            throw new NotImplementedException();
        }

        public async Task<List<IStore>> GetCustomerCode(string CustomerCode)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"CustomerCode",  CustomerCode}
                };

                var sql = @"SELECT '(' || code || ')' || name AS Name, uid AS UID FROM store;";
                Type type = _serviceProvider.GetRequiredService<IStore>().GetType();
                List<IStore> CollectionModuleList = await ExecuteQueryAsync<IStore>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                throw new();
            }
        }

        Task<PagedResponse<IAccCollection>> ICollectionModuleDL.ViewPayments(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            throw new NotImplementedException();
        }

        Task<string> ICollectionModuleDL.AddEarlyPayment(IEarlyPaymentDiscountConfiguration EarlyPayment)
        {
            throw new NotImplementedException();
        }

        Task<List<IAccPayable>> ICollectionModuleDL.GetAccountStatementPay(string StoreUID)
        {
            throw new NotImplementedException();
        }
        Task<string> ICollectionModuleDL.CreateReceiptWithEarlyPaymentDiscount(ICollections EarlyPaymentRecords)
        {
            throw new NotImplementedException();
        }

        Task<List<IEarlyPaymentDiscountConfiguration>> ICollectionModuleDL.GetConfigurationDetails()
        {
            throw new NotImplementedException();
        }

        public async Task<List<IAccCollection>> GetReceipts()
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"AccCollectionUID",  ""}
                };
                var sql = @"Select AC.receipt_number as ReceiptNumber,AC.uid as UID, AC.collected_date as CollectedDate, AC.amount as Amount from acc_collection_payment_mode AP 
                            Inner Join acc_collection AC on AC.uid = AP.acc_collection_uid
                            where AC.category = 'Cash' and AC.status = 'Collected' and AC.collection_deposit_status IS NULL";
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
                List<IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }
        public async Task<IAccCollectionAndDeposit> ViewReceipts(string RequestNo)
        {
            try

            {
                IAccCollectionAndDeposit accCollectionAndDeposit = new AccCollectionAndDeposit();
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"RequestNo",  RequestNo}
                };
                var sql = @"SELECT
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            ss AS Ss,
                            emp_uid AS EmpUid,
                            job_position_uid AS JobPositionUid,
                            request_no AS RequestNo,
                            request_date AS RequestDate,
                            amount AS Amount,
                            bank_uid AS BankUid,
                            branch AS Branch,
                            notes AS Notes,
                            comments AS Comments,
                            receipt_nos AS ReceiptNos,
                            approval_date AS ApprovalDate,
                            approved_by_emp_uid AS ApprovedByEmpUid,
                            status AS Status
                            FROM acc_collection_deposit 
                            where request_no = @RequestNo";
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionDeposit>().GetType();
                accCollectionAndDeposit.accCollectionDeposits = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionDeposit>(sql, parameters, type);
                var receiptNumbers = accCollectionAndDeposit.accCollectionDeposits
    .SelectMany(p => JsonConvert.DeserializeObject<List<string>>(p.ReceiptNos ?? "[]")) // Deserialize each ReceiptNos JSON string
    .ToList();
                List<string> UIDs = new List<string>();
                UIDs = receiptNumbers.ToList();
                var sql1 = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, 
                            modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, receipt_number AS ReceiptNumber, 
                            consolidated_receipt_number AS ConsolidatedReceiptNumber, category AS Category, amount AS Amount, currency_uid AS CurrencyUID, 
                            default_currency_uid AS DefaultCurrencyUID, default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount,
                            org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, route_uid AS RouteUID,
                            job_position_uid AS JobPositionUID, emp_uid AS EmpUID, collected_date AS CollectedDate, status AS Status, remarks AS Remarks,
                            reference_number AS ReferenceNumber, is_realized AS IsRealized, latitude AS Latitude, longitude AS Longitude, source AS Source,
                            is_multimode AS IsMultimode, trip_date AS TripDate, comments AS Comments, salesman AS Salesman, route AS Route, 
                            reversal_receipt_uid AS ReversalReceiptUID, cancelled_on AS CancelledOn, is_remote_collection AS IsRemoteCollection,
                            remote_collection_reason AS RemoteCollectionReason FROM acc_collection where receipt_number in @UIDs;";
                dynamic param = new
                {
                    UIDs = UIDs,
                };
                var result = await ExecuteQueryAsync<IAccCollection>(sql1, param);
                accCollectionAndDeposit.accCollections = result;
                return accCollectionAndDeposit;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }
        public async Task<List<IAccCollectionDeposit>> GetRequestReceipts(string Status)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"Status",  Status}
                };
                var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                            modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                            server_modified_time AS ServerModifiedTime, ss AS Ss, emp_uid AS EmpUID, 
                            job_position_uid AS JobPositionUID, request_no AS RequestNo, request_date AS RequestDate, 
                            amount AS Amount, bank_uid AS BankUID, branch AS Branch, notes AS Notes, 
                            comments AS Comments, receipt_nos AS ReceiptNos, approval_date AS ApprovalDate, 
                            approved_by_emp_uid AS ApprovedByEmpUID, status AS Status
                            FROM acc_collection_deposit where status = @Status;";
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionDeposit>().GetType();
                List<IAccCollectionDeposit> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionDeposit>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }
        public async Task<bool> CreateCashDepositRequest(IAccCollectionDeposit accCollectionDeposit)
        {
            try
            {
                using (var conn = new SqliteConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        var Result = "";
                        var sql = @"INSERT INTO acc_collection_deposit 
                                    (uid, created_by, created_time, modified_by, modified_time, 
                                    server_add_time, server_modified_time, ss, emp_uid, job_position_uid, 
                                    request_no, request_date, amount, bank_uid, branch, 
                                    notes, comments,status, receipt_nos, approval_date, approved_by_emp_uid) VALUES (
                                    @uid, @created_by, @created_time, @modified_by, @modified_time, 
                                    @server_add_time, @server_modified_time, @ss, @emp_uid, @job_position_uid, 
                                    @request_no, @request_date, @amount, @bank_uid, @branch, 
                                    @notes, @comments,@status, @receipt_nos, @approval_date, @approved_by_emp_uid);";
                        Dictionary<string, object?> parameters = new Dictionary<string, object?>
                    {
                        //{"id", 1 },
                        {"uid", (Guid.NewGuid()).ToString()},
                        {"created_by", Const.ModifiedBy},
                        {"created_time", DateTime.Now},
                        {"modified_by", Const.ModifiedBy},
                        {"modified_time", DateTime.Now},
                        {"server_add_time", DateTime.Now},
                        {"server_modified_time", DateTime.Now},
                        {"ss", 0},
                        {"emp_uid", accCollectionDeposit.EmpUID ?? ""},
                        {"job_position_uid", accCollectionDeposit.JobPositionUID ?? ""},
                        {"request_no", accCollectionDeposit.RequestNo ?? ""},
                        {"request_date", accCollectionDeposit.RequestDate},
                        {"amount", accCollectionDeposit.Amount  == 0 ? 0 : accCollectionDeposit.Amount},
                        {"bank_uid", accCollectionDeposit.BankUID ?? ""},
                        {"branch", accCollectionDeposit.Branch ?? ""},
                        {"notes", accCollectionDeposit.Notes ?? ""},
                        {"comments", accCollectionDeposit.Comments ?? ""},
                        {"status", accCollectionDeposit.Status ?? ""},
                        {"receipt_nos", accCollectionDeposit.ReceiptNos ?? ""},
                        {"approval_date", accCollectionDeposit.ApprovalDate},
                        {"approved_by_emp_uid", accCollectionDeposit.ApprovedByEmpUID ?? ""}
                    };
                        Result = await CommonMethod(sql, conn, parameters, transaction);

                        var sql1 = @"update acc_collection set collection_deposit_status = 'Pending' where receipt_number in @RequestNo";
                        List<string> receiptNumbers = JsonConvert.DeserializeObject<List<string>>(accCollectionDeposit.ReceiptNos); // Deserialize each ReceiptNos JSON string
                        var params1 = new { RequestNo = receiptNumbers };
                        var Result1 = await ExecuteNonQueryAsync(sql1, params1, conn, transaction);
                        if (Result == Const.One || Result1 == 1)
                        {
                            transaction.Commit();
                            return true;
                        }
                        else
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        public async Task<bool> ApproveOrRejectDepositRequest(IAccCollectionDeposit accCollectionDeposit, string Status)
        {
            try
            {
                int Retval = 0;
                using (var conn = new SqliteConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        var sql = @"UPDATE acc_collection_deposit
                        SET modified_time = @ModifiedTime, modified_by = @ModifiedBy,
                        server_modified_time = @ServerModifiedTime, approval_date = @ApprovalDate, approved_by_emp_uid = @ApprovedByEmpUID,
                        status = @Status ,request_date = @RequestDate, bank_uid = @BankUID, branch = @Branch,notes = @Notes, comments = @Comments
                        WHERE request_no = @UID;";
                        Dictionary<string, object> parameters = new Dictionary<string, object>
                            {
                                {"Comments", accCollectionDeposit.Comments ?? ""},
                                {"Status", "Pending" },
                                {"RequestDate", accCollectionDeposit.RequestDate },
                                {"BankUID", accCollectionDeposit.BankUID },
                                {"Branch", accCollectionDeposit.Branch },
                                {"Notes", accCollectionDeposit.Notes },
                                {"ModifiedTime", DateTime.Now},
                                {"ModifiedBy", Const.ModifiedBy},
                                {"ApprovedByEmpUID", accCollectionDeposit.ApprovedByEmpUID ?? ""},
                                {"ServerModifiedTime", DateTime.Now},
                                {"ApprovalDate", DateTime.Now },
                                {"UID", accCollectionDeposit.RequestNo}
                            };
                        Retval = await ExecuteNonQueryAsync(sql, parameters, conn, transaction);
                        if (Retval == 0)
                        {
                            transaction.Rollback();
                            return false;
                        }
                        var sql1 = @"UPDATE acc_collection
                        SET modified_time = @ModifiedTime, modified_by = @ModifiedBy,
                        server_modified_time = @ServerModifiedTime,
                        collection_deposit_status = @Status WHERE receipt_number in (@UID);";
                        var receiptNumbers = JsonConvert.DeserializeObject<List<string>>(accCollectionDeposit.ReceiptNos ?? "[]");
                        dynamic param = new
                        {
                            UID = receiptNumbers,
                            Status = "Pending",
                            ModifiedTime = DateTime.Now,
                            ServerModifiedTime = DateTime.Now,
                            ModifiedBy = Const.ModifiedBy,
                        };
                        Retval = await ExecuteNonQueryAsync(sql1, param, conn, transaction);
                        if (Retval == 0)
                        {
                            transaction.Rollback();
                            return false;
                        }
                        transaction.Commit();
                        return true;
                    }
                }
            }
            catch (Exception x)
            {
                throw new();
            }
        }
        public async Task<decimal> CheckCollectionLimitForLoggedInUser(string EmpUID)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"EmpUID",  EmpUID}
                };
                var sql = @"select JP.collection_limit from job_position JP 
                            inner join roles R on JP.user_role_uid = R.uid
                            inner join emp E on JP.emp_uid = E.uid 
                            where  R.is_app_user = true and E.uid = @EmpUID";
                decimal Limit = await ExecuteScalarAsync<decimal>(sql, parameters);
                return Limit;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> UpdateCollectionLimit(decimal Limit, string EmpUID, int Action)
        {
            try
            {
                int Retval = 0;
                using (var conn = new SqliteConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        decimal limit = await CheckCollectionLimitForLoggedInUser(EmpUID);
                        limit -= Limit ; 
                        var sql = @"Update job_position set collection_limit = @Limit where emp_uid = @EmpUID ";
                        Dictionary<string, object> parameters = new Dictionary<string, object>
                            {
                                {"Limit", limit },
                                {"EmpUID", EmpUID}
                            };
                        Retval = await ExecuteNonQueryAsync(sql, parameters, conn, transaction);
                        if (Retval == 0)
                        {
                            transaction.Rollback();
                            return false;
                        }
                        transaction.Commit();
                        return true;
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public async Task<int> CUDAccPayable(List<IAccPayable> accPayables, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int count = 0;
            try
            {
                if (accPayables == null || accPayables.Count == 0)
                {
                    return 0;
                }
                List<string> uidList = accPayables.Select(po => po.UID).ToList();
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.AccPayable, uidList, connection, transaction);

                List<IAccPayable>? newAccPayable = null;
                List<IAccPayable>? existingAccPayable = null;
                if (existingUIDs != null && existingUIDs.Count > 0)
                {
                    newAccPayable = accPayables.Where(sol => !existingUIDs.Contains(sol.UID)).ToList();
                    existingAccPayable = accPayables.Where(e => existingUIDs.Contains(e.UID)).ToList();
                }
                else
                {
                    newAccPayable = accPayables;
                }

                if (existingAccPayable != null && existingAccPayable.Any())
                {
                    // No action needed
                }
                if (newAccPayable.Any())
                {
                    count += await CreateAccPayable(newAccPayable, connection, transaction);
                }
            }
            catch
            {
                throw;
            }
            return count;
        }
        private async Task<int> CreateAccPayable(List<IAccPayable> accPayables, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retValue = 0;
            try
            {
                if (accPayables == null || accPayables.Count == 0)
                {
                    return retValue;
                }
                string query = @"INSERT INTO acc_payable (id, uid, ss, created_by, created_time, modified_by, modified_time, 
                        server_add_time, server_modified_time, source_type, source_uid, reference_number, 
                        org_uid, job_position_uid, amount, paid_amount, store_uid, transaction_date, 
                        due_date, unsettled_amount, source, currency_uid) 
                        VALUES (@Id, @UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, 
                        @ServerAddTime, @ServerModifiedTime, @SourceType, @SourceUID, @ReferenceNumber, 
                        @OrgUID, @JobPositionUID, @Amount, @PaidAmount, @StoreUID, @TransactionDate, 
                        @DueDate, @UnSettledAmount, @Source, @CurrencyUID);
                        ";

                retValue = await ExecuteNonQueryAsync(query, accPayables, connection, transaction);

            }
            catch (Exception)
            {
                throw;
            }
            return retValue;
        }
        public async Task<List<IAccCollection>> GetCollectionTabsDetails(string PageName)
        {
            throw new NotImplementedException();
        }

        Task<PagedResponse<IAccCollection>> ICollectionModuleDL.GetCollectionTabsDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, string PageName)
        {
            throw new NotImplementedException();
        }

        Task<bool> ICollectionModuleDL.UpdateBankDetails(string UID, string BankName, string Branch, string ReferenceNumber)
        {
            throw new NotImplementedException();
        }

        Task<PagedResponse<IStoreStatement>> ICollectionModuleDL.StoreStatementRecords(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string StartDate, string EndDate)
        {
            throw new NotImplementedException();
        }

        Task<bool> ICollectionModuleDL.UpdateBalanceConfirmation(IBalanceConfirmation balanceConfirmation)
        {
            throw new NotImplementedException();
        }

        Task<bool> ICollectionModuleDL.InsertDisputeRecords(List<IBalanceConfirmationLine> balanceConfirmationLine)
        {
            throw new NotImplementedException();
        }

        
        Task<bool> ICollectionModuleDL.UpdateBalanceConfirmationForResolvingDispute(IBalanceConfirmation balanceConfirmation)
        {
            throw new NotImplementedException();
        }

        Task<List<IBalanceConfirmationLine>> ICollectionModuleDL.GetBalanceConfirmationLineDetails(string UID)
        {
            throw new NotImplementedException();
        }

        Task<IBalanceConfirmation> ICollectionModuleDL.GetBalanceConfirmationDetails(string StoreUID)
        {
            throw new NotImplementedException();
        }

        Task<List<IBalanceConfirmation>> ICollectionModuleDL.GetBalanceConfirmationListDetails()
        {
            throw new NotImplementedException();
        }

        Task<IContact> ICollectionModuleDL.GetContactDetails(string EmpCode)
        {
            throw new NotImplementedException();
        }
        #endregion


        public class Const
        {
            public const string Cash = "Cash";
            public const string Invoice = "Invoice";
            public const string ModifiedBy = "ADMIN";
            public const int Num = -1;
            public const string Success = "Successfully Updated Data Into tables";
            public const string ParamMissing = "Parameter missing";
            public const string SuccessInsert = "Successfully Inserted Data Into tables";
            public const string ReversalMsg = "Can't Reverse the Receipt as it is already settled or invalid Receipt Number";
            public const string VoidMsg = "Can't Void ths Receipt as it is already Settled";
            public const string NotFound = "Record not found";
            public const string MisMatch = "Amount mismatching";
            public const string Zero = "0";
            public const string One = "1";
        }
    }
}

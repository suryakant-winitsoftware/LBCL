//using DocumentFormat.OpenXml.Office2010.ExcelAc;
//using DocumentFormat.OpenXml.Vml.Office;
using iTextSharp.text;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Newtonsoft.Json;
using Npgsql;
using System.Data;
using System.Text;
using System.Transactions;
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
    public class PGSQLCollectionModuleDL : Base.DL.DBManager.PostgresDBManager, Interfaces.ICollectionModuleDL
    {
        public PGSQLCollectionModuleDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        private static string sixDigitGUIDString { get; set; } = string.Empty;
        private string AccCollectionUID { get; set; }
        private string _creditID { get; set; } = $"{DateTime.Now.Minute:D2}{DateTime.Now.Second:D2}{DateTime.Now.Millisecond:D3}";
        private decimal PayAmount { get; set; } = 0;
        private decimal PayUnSettleAmount { get; set; } = 0;
        private decimal RecAmount { get; set; } = 0;
        private List<Dictionary<string, object>> parameters { get; set; } = new List<Dictionary<string, object>>();
        private List<string> sqlStatements { get; set; } = new List<string>();
        public string CreditID()
        {
            string _creditid = $"{DateTime.Now.Minute:D2}{DateTime.Now.Second:D2}{DateTime.Now.Millisecond:D3}";
            return _creditid;
        }
        //Common Methods 
        public async Task<string> InsertAccCollection(NpgsqlConnection conn, NpgsqlTransaction transaction, ICollections collection)
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
            var Retval = string.Empty;
            var sql = @"INSERT INTO public.acc_collection(uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, receipt_number, consolidated_receipt_number, category, amount, currency_uid,
            default_currency_uid, default_currency_exchange_rate, default_currency_amount, org_uid, distribution_channel_uid, store_uid, route_uid, job_position_uid, emp_uid, collected_date, status, remarks, reference_number, is_realized,
            latitude, longitude, source, is_multimode, trip_date, comments, salesman, route, reversal_receipt_uid, cancelled_on, ss)
            VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @ReceiptNumber, @ConsolidatedReceiptNumber, @Category, @Amount, @CurrencyUID, @DefaultCurrencyUID, @DefaultCurrencyExchangeRate,
                    @DefaultCurrencyAmount, @OrgUID, @DistributionChannelUID, @StoreUID, @RouteUID, @JobPositionUID, @EmpUID, @CollectedDate, @Status, @Remarks, @ReferenceNumber, @IsRealized, @Latitude, @Longitude, @Source, @IsMultimode, @TripDate,
                    @Comments, @Salesman, @Route, @ReversalReceiptUID, @CancelledOn, @SS);";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
               {"UID", string.IsNullOrEmpty(collection.AccCollection.UID) ?  string.Empty: collection.AccCollection.UID},
               {"ReceiptNumber", string.IsNullOrEmpty(collection.AccCollection.ReceiptNumber) ?string.Empty :collection.AccCollection.ReceiptNumber},
               {"ConsolidatedReceiptNumber", string.IsNullOrEmpty(collection.AccCollection.ConsolidatedReceiptNumber) ?string.Empty:collection.AccCollection.ConsolidatedReceiptNumber},
               {"Category", string.IsNullOrEmpty(collection.AccCollection.Category) ?  string.Empty: collection.AccCollection.Category},
               {"Amount", collection.AccCollection.Amount == 0 ? 0 : collection.AccCollection.Amount },
               {"CurrencyUID", string.IsNullOrEmpty(collection.AccCollection.CurrencyUID)?string.Empty:collection.AccCollection.CurrencyUID},
               {"DefaultCurrencyUID", string.IsNullOrEmpty(collection.AccCollection.DefaultCurrencyUID)?string.Empty:collection.AccCollection.DefaultCurrencyUID},
               {"DefaultCurrencyExchangeRate", collection.AccCollection.DefaultCurrencyExchangeRate==0?0:collection.AccCollection.DefaultCurrencyExchangeRate},
               {"DefaultCurrencyAmount", collection.AccCollection.DefaultCurrencyAmount==0?0:collection.AccCollection.DefaultCurrencyAmount},
               {"OrgUID", string.IsNullOrEmpty(collection.AccCollection.OrgUID)?string.Empty:collection.AccCollection.OrgUID},
               {"DistributionChannelUID", string.IsNullOrEmpty(collection.AccCollection.DistributionChannelUID)?string.Empty:collection.AccCollection.DistributionChannelUID},
               {"StoreUID", string.IsNullOrEmpty(collection.AccCollection.StoreUID)? string.IsNullOrEmpty(collection.AccCollectionAllotment.FirstOrDefault().StoreUID) ?string.Empty: collection.AccCollectionAllotment.FirstOrDefault().StoreUID : collection.AccCollection.StoreUID},
               {"RouteUID", null},
               {"JobPositionUID", string.IsNullOrEmpty(collection.AccCollection.JobPositionUID)?string.Empty:collection.AccCollection.JobPositionUID},
               {"EmpUID", string.IsNullOrEmpty(collection.AccCollection.EmpUID)?string.Empty:collection.AccCollection.EmpUID},
               {"CollectedDate", collection.AccCollection.CollectedDate==null?DateTime.Now:collection.AccCollection.CollectedDate},
               {"Status", string.IsNullOrEmpty(collection.AccCollection.Status)?string.Empty:collection.AccCollection.Status},
               {"Remarks", string.IsNullOrEmpty(collection.AccCollection.Remarks)?string.Empty:collection.AccCollection.Remarks},
               {"ReferenceNumber", string.IsNullOrEmpty(collection.AccCollection.ReferenceNumber)?string.Empty:collection.AccCollection.ReferenceNumber},
               {"IsRealized", collection.AccCollection.IsRealized==false?false:collection.AccCollection.IsRealized},
               {"Latitude", string.IsNullOrEmpty(collection.AccCollection.Latitude)?string.Empty:collection.AccCollection.Latitude},
               {"Longitude", string.IsNullOrEmpty(collection.AccCollection.Longitude)?string.Empty:collection.AccCollection.Longitude},
               {"Source", string.IsNullOrEmpty(collection.AccCollection.Source)?string.Empty:collection.AccCollection.Source},
               {"IsMultimode", collection.AccCollection.IsMultimode==false?false:collection.AccCollection.IsMultimode},
               {"TripDate", collection.AccCollection.TripDate==null ? DateTime.Now : collection.AccCollection.TripDate},
               {"Comments", string.IsNullOrEmpty(collection.AccCollection.Comments) ?string.Empty: collection.AccCollection.Comments},
               {"Salesman", string.IsNullOrEmpty(collection.AccCollection.Salesman) ?string.Empty: collection.AccCollection.Salesman},
               {"Route", string.IsNullOrEmpty(collection.AccCollection.Route) ?string.Empty: collection.AccCollection.Route},
               {"ReversalReceiptUID", string.IsNullOrEmpty(collection.AccCollection.ReversalReceiptUID) ?string.Empty: collection.AccCollection.ReversalReceiptUID},
               {"CancelledOn", collection.AccCollection.CancelledOn == null ? DateTime.Now : collection.AccCollection.CancelledOn},
               {"CreatedTime", DateTime.Now},
               {"ModifiedTime", DateTime.Now},
               {"ModifiedBy", string.IsNullOrEmpty(collection.AccCollection.ModifiedBy) ?string.Empty: collection.AccCollection.ModifiedBy},
               {"ServerAddTime", DateTime.Now},
               {"ServerModifiedTime", DateTime.Now},
               {"SS", 0},
               {"CreatedBy", string.IsNullOrEmpty(collection.AccCollection.CreatedBy) ?string.Empty: collection.AccCollection.CreatedBy}
            };
            Retval = await ExecuteNonQueryAsync(sql, conn, parameters, transaction);
            if (Retval != Const.One)
            {
                return Retval;
            }
            else
            {
                return Retval;
            }

        }
        public async Task<String> InsertAccCollectionPaymentMode(NpgsqlConnection conn, NpgsqlTransaction transaction, ICollections collection)
        {
            var Retval = string.Empty;
            var sql1 = @"INSERT INTO acc_collection_payment_mode (uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                            server_modified_time, acc_collection_uid, bank_uid, branch, cheque_no, amount, currency_uid, default_currency_uid, 
                            default_currency_exchange_rate, default_currency_amount, cheque_date, status, realization_date, ss)
                                VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                                @AccCollectionUID, @BankUID, @Branch, @ChequeNo, @Amount, @CurrencyUID, @DefaultCurrencyUID, @DefaultCurrencyExchangeRate, 
                                @DefaultCurrencyAmount, @ChequeDate, @Status, @RealizationDate, @SS)";
            Dictionary<string, object> parameters1 = new Dictionary<string, object>
                        {
                            {"AccCollectionUID", string.IsNullOrEmpty(collection.AccCollection.UID)  ?string.Empty: collection.AccCollection.UID},
                            {"BankUID", string.IsNullOrEmpty(collection.AccCollectionPaymentMode.BankUID) ?string.Empty: collection.AccCollectionPaymentMode.BankUID},
                            {"Branch", string.IsNullOrEmpty(collection.AccCollectionPaymentMode.Branch) ?string.Empty: collection.AccCollectionPaymentMode.Branch},
                            {"ChequeNo", string.IsNullOrEmpty(collection.AccCollectionPaymentMode.ChequeNo) ?string.Empty: collection.AccCollectionPaymentMode.ChequeNo},
                            {"CurrencyUID", string.IsNullOrEmpty(collection.AccCollection.CurrencyUID) ?string.Empty: collection.AccCollection.CurrencyUID},
                            {"DefaultCurrencyUID", string.IsNullOrEmpty(collection.AccCollection.DefaultCurrencyUID) ?string.Empty: collection.AccCollection.DefaultCurrencyUID},
                            {"DefaultCurrencyExchangeRate", collection.AccCollection.DefaultCurrencyExchangeRate==0?0:collection.AccCollection.DefaultCurrencyExchangeRate},
                            {"DefaultCurrencyAmount", collection.AccCollection.DefaultCurrencyAmount==0?0:collection.AccCollection.DefaultCurrencyAmount},
                            {"ChequeDate", collection.AccCollectionPaymentMode.ChequeDate==null?DateTime.Now:collection.AccCollectionPaymentMode.ChequeDate},
                            {"Status", string.IsNullOrEmpty(collection.AccCollection.Status)?string.Empty:collection.AccCollection.Status},
                            {"RealizationDate", collection.AccCollectionPaymentMode.RealizationDate==null?DateTime.Now:collection.AccCollectionPaymentMode.RealizationDate},
                            {"CreatedTime",DateTime.Now},
                            {"ModifiedTime", DateTime.Now},
                            {"ModifiedBy", string.IsNullOrEmpty(collection.AccCollection.ModifiedBy) ?string.Empty: collection.AccCollection.ModifiedBy},
                            {"ServerAddTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now},
                            {"CreatedBy", string.IsNullOrEmpty(collection.AccCollection.CreatedBy) ?string.Empty: collection.AccCollection.CreatedBy},
                            {"UID", (Guid.NewGuid()).ToString()},
                            {"Amount", collection.AccCollection.Amount},
                            {"SS", 0}
                        };
            Retval = await ExecuteNonQueryAsync(sql1, conn, parameters1, transaction);
            if (Retval != Const.One)
                return Retval;
            else
                return Retval;
        }
        public async Task<string> InsertAccStoreLedger(NpgsqlConnection conn, NpgsqlTransaction transaction, ICollections collection)
        {
            IAccStoreLedger accStore = await GetStoreLedgerCreation(collection.AccCollection.StoreUID, conn, transaction);
            if (accStore == null)
            {
                accStore = new Winit.Modules.CollectionModule.Model.Classes.AccStoreLedger();
            }
            var Retval = string.Empty;
            var sql3 = @"INSERT INTO acc_store_ledger (uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                                server_modified_time, source_type, source_uid, credit_type, org_uid, store_uid, default_currency_uid, document_number,
                                default_currency_exchange_rate, default_currency_amount, amount, transaction_date_time, collected_amount, currency_uid, balance, comments, ss) 
                                VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,@ServerAddTime, @ServerModifiedTime, @SourceType, @SourceUID,
                                   @CreditType, @OrgUID, @StoreUID, @DefaultCurrencyUID,@DocumentNumber, @DefaultCurrencyExchangeRate, @DefaultCurrencyAmount,
                                   @Amount, @TransactionDateTime, @CollectedAmount, @CurrencyUID, @Balance, @Comments, @SS)";
            Dictionary<string, object> parameters3 = new Dictionary<string, object>
                        {
                            {"SourceType", "Collection"},
                            {"SourceUID", collection.AccCollection.UID==null?string.Empty: collection.AccCollection.UID},
                            {"CreditType",collection.AccCollection.ReceiptNumber.Contains("OA")||collection.AccCollection.CreditNote!=  string.Empty? "CR" : "DR"},
                            {"OrgUID", collection.AccStoreLedger.OrgUID==null?string.Empty: collection.AccStoreLedger.OrgUID},
                            {"StoreUID", collection.AccCollection.StoreUID==null?string.Empty: collection.AccCollection.StoreUID},
                            {"DefaultCurrencyUID", collection.AccCollection.DefaultCurrencyUID==null?string.Empty: collection.AccCollection.DefaultCurrencyUID},
                            {"DocumentNumber", collection.AccCollection.ReceiptNumber == null ?string.Empty: collection.AccCollection.ReceiptNumber},
                            {"DefaultCurrencyExchangeRate", collection.AccCollection.DefaultCurrencyExchangeRate==0?0: collection.AccCollection.DefaultCurrencyExchangeRate},
                            {"DefaultCurrencyAmount", collection.AccCollection.DefaultCurrencyAmount==0?0: collection.AccCollection.DefaultCurrencyAmount},
                            {"Amount", collection.AccCollection.Category == "Cash" ? (collection.AccCollection.DefaultCurrencyAmount == 0 ? 0 : collection.AccCollection.ReceiptNumber.Contains("OA") ? (collection.AccCollection.DefaultCurrencyAmount + collection.AccCollection.DiscountAmount)*-1 : (collection.AccCollection.DefaultCurrencyAmount + collection.AccCollection.DiscountAmount))  : collection.AccCollection.ReceiptNumber.Contains("OA") ? 0 :   0},
                            {"TransactionDateTime", collection.AccStoreLedger.TransactionDateTime == null ? DateTime.Now : DateTime.Now},
                            {"CollectedAmount", /*collection.Amount == 0 ? 0 : collection.Amount + collection.DiscountAmount*/collection.AccCollection.Category == "Cash" ? (collection.AccCollection.DefaultCurrencyAmount == 0 ? 0 : collection.AccCollection.ReceiptNumber.Contains("OA") ? (collection.AccCollection.DefaultCurrencyAmount + collection.AccCollection.DiscountAmount)*-1 : (collection.AccCollection.DefaultCurrencyAmount + collection.AccCollection.DiscountAmount)) : collection.AccCollection.ReceiptNumber.Contains("OA") ? 0 :   0},
                            {"Balance",  collection.AccCollection.Category == "Cash" ? accStore.Balance == 0 ? (collection.AccCollection.ReceiptNumber.Contains("OA")||collection.AccCollection.CreditNote!= string.Empty? 0 - collection.AccCollection.DefaultCurrencyAmount - collection.AccCollection.DiscountAmount : 0 + collection.AccCollection.DefaultCurrencyAmount + collection.AccCollection.DiscountAmount)  : (collection.AccCollection.ReceiptNumber.Contains("OA")||collection.AccCollection.CreditNote!= string.Empty? accStore.Balance  - collection.AccCollection.DefaultCurrencyAmount - collection.AccCollection.DiscountAmount : accStore.Balance + collection.AccCollection.DefaultCurrencyAmount + collection.AccCollection.DiscountAmount) :
                             accStore.Balance == 0 ? (collection.AccCollection.ReceiptNumber.Contains("OA") ? 0  : 0 )  : (collection.AccCollection.ReceiptNumber.Contains("OA") ? accStore.Balance - 0  : accStore.Balance + 0)               },
                            {"CurrencyUID", collection.AccCollection.CurrencyUID ==null ?string.Empty: collection.AccCollection.CurrencyUID},
                            {"CreatedTime", DateTime.Now},
                            {"ModifiedTime", DateTime.Now},
                            {"ModifiedBy", string.IsNullOrEmpty(collection.AccCollection.ModifiedBy) ?string.Empty: collection.AccCollection.ModifiedBy},
                            {"ServerAddTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now},
                            {"Comments", collection.AccCollection.ReceiptNumber.Contains("OA") ? "OnAccount created Successfully" : "Receipt(s) created Successfully"},
                            {"CreatedBy", string.IsNullOrEmpty(collection.AccCollection.CreatedBy) ?string.Empty: collection.AccCollection.CreatedBy},
                            {"UID", (Guid.NewGuid()).ToString()},
                            {"SS", 0}
                        };
            Retval = await ExecuteNonQueryAsync(sql3, conn, parameters3, transaction);
            if (Retval != Const.One)
                return Retval;
            else
                return Retval;
        }
        public async Task<string> InsertAccCollectionAllotment(NpgsqlConnection conn, NpgsqlTransaction transaction, ICollections collection)
        {
            var Retval = string.Empty;

            foreach (var list in collection.AccCollectionAllotment)
            {
                try
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
                    var sql2 = @"INSERT INTO acc_collection_allotment (uid, created_by, created_time, modified_by, modified_time, 
                            server_add_time, server_modified_time, acc_collection_uid, target_type, target_uid, reference_number, currency_uid,
                            default_currency_uid, default_currency_exchange_rate, default_currency_amount, early_payment_discount_percentage, early_payment_discount_amount, 
                            amount, early_payment_discount_reference_no, ss)
                                VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @AccCollectionUID,
                                @TargetType, @TargetUID, @ReferenceNumber, @CurrencyUID, @DefaultCurrencyUID, @DefaultCurrencyExchangeRate, @DefaultCurrencyAmount, @EarlyPaymentDiscountPercentage,
                                @EarlyPaymentDiscountAmount, @Amount, @EarlyPaymentDiscountReferenceNo, @SS)";
                    Dictionary<string, object> parameters2 = new Dictionary<string, object>
                {
                                {"AccCollectionUID", collection.AccCollection.UID},
                                {"TargetType", list.TargetType == null ?string.Empty: list.TargetType},
                                {"TargetUID",list.TargetType == "OA - CREDITNOTE" ? list.TargetUID : list.TargetType.Contains("INVOICE") ? accPayable.UID == null ?string.Empty: accPayable.UID : receive.UID == null ? "CREDITNOTE-"+_creditID : receive.UID} ,
                                {"ReferenceNumber", list.TargetType == "OA - CREDITNOTE" ? "CREDITNOTE-"+_creditID : (list.ReferenceNumber == null|| list.ReferenceNumber ==string.Empty) ? "CREDITNOTE-"+_creditID : list.ReferenceNumber},
                                {"CurrencyUID", collection.AccCollection.CurrencyUID==null ?string.Empty: collection.AccCollection.CurrencyUID},
                                {"DefaultCurrencyUID", collection.AccCollection.DefaultCurrencyUID==null ?string.Empty: collection.AccCollection.DefaultCurrencyUID},
                                {"DefaultCurrencyExchangeRate", collection.AccCollection.DefaultCurrencyExchangeRate==0 ? 0 : collection.AccCollection.DefaultCurrencyExchangeRate},
                                {"DefaultCurrencyAmount", collection.AccCollection.DefaultCurrencyAmount==0 ? 0 : collection.AccCollection.DefaultCurrencyAmount},
                                {"EarlyPaymentDiscountPercentage", collection.AccCollection.DiscountValue==0 ? 0 : collection.AccCollection.DiscountValue },
                                {"EarlyPaymentDiscountAmount", list.DiscountAmount==0 ? 0 : list.DiscountAmount },
                                {"EarlyPaymentDiscountReferenceNo", list.DiscountAmount > 0 ? "CREDITNOTE-"+_creditID :  string.Empty},
                                {"Amount", list.PaidAmount == 0 ? collection.AccCollection.Amount : list.PaidAmount },
                                {"CreatedTime", DateTime.Now },
                                {"ModifiedTime", DateTime.Now},
                                {"ModifiedBy", string.IsNullOrEmpty(collection.AccCollection.ModifiedBy) ?string.Empty: collection.AccCollection.ModifiedBy },
                                {"ServerAddTime", DateTime.Now },
                                {"ServerModifiedTime", DateTime.Now },
                                {"CreatedBy",string.IsNullOrEmpty(collection.AccCollection.CreatedBy) ?string.Empty: collection.AccCollection.CreatedBy },
                                {"UID", (Guid.NewGuid()).ToString()},
                                {"SS", 0}
                            };
                    Retval = await ExecuteNonQueryAsync(sql2, conn, parameters2, transaction);

                    if (Retval != Const.One)
                        return Retval;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return Retval;
        }
        public async Task<string> InsertAccCollectionCurrencyDetails(NpgsqlConnection conn, NpgsqlTransaction transaction, ICollections collection)
        {
            try
            {
                var Result = "";
                if (collection.AccCollectionCurrencyDetails != null)
                {
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
                                {"created_by", string.IsNullOrEmpty(collection.AccCollection.CreatedBy) ?string.Empty: collection.AccCollection.CreatedBy},
                                {"created_time", DateTime.Now},
                                {"modified_by", string.IsNullOrEmpty(collection.AccCollection.ModifiedBy) ?string.Empty: collection.AccCollection.ModifiedBy},
                                {"modified_time", DateTime.Now},
                                {"server_add_time", DateTime.Now},
                                {"server_modified_time", DateTime.Now},
                                {"acc_collection_uid", collection.AccCollection.UID},
                                {"currency_uid", list.currency_uid},
                                {"default_currency_uid", list.default_currency_uid},
                                {"default_currency_exchange_rate", list.default_currency_exchange_rate},
                                {"amount", list.amount},
                                {"SS", 0},
                                {"default_currency_amount", list.default_currency_amount},
                                {"final_default_currency_amount", list.final_default_currency_amount}
                            };
                        Result = await ExecuteNonQueryAsync(sql, conn, parameters, transaction);

                        if (Result != Const.One)
                            return Result;
                    }
                }
                return Result;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }
        public async Task<string> DeleteCreditNote(string AccCollectionUID, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            try
            {
                var Retval = string.Empty;
                var sql2 = @"DELETE FROM acc_receivable
                            WHERE reference_number IN (
                                SELECT al.reference_number
                                FROM acc_receivable ar
                                INNER JOIN acc_collection_allotment al 
                                ON al.reference_number = ar.reference_number 
                                WHERE acc_collection_uid = @AccCollectionUID)";
                Dictionary<string, object> parameters2 = new Dictionary<string, object>
                        {
                             {"AccCollectionUID", AccCollectionUID}
                        };
                Retval = await ExecuteNonQueryAsync(sql2, conn, parameters2, transaction);
                if (Retval != Const.One)
                {
                    return Retval;
                }
                return Retval;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public async Task<string> UpdateAccReceivable(NpgsqlConnection conn, NpgsqlTransaction transaction, ICollections collection)
        {
            var Retval = string.Empty;
            foreach (var list in collection.AccCollectionAllotment.Where(p => p.TargetType.Contains("CREDITNOTE") && !collection.AccCollection.IsEarlyPayment))
            {
                IAccReceivable receive = await GetAccRecAmount(collection.AccCollection.StoreUID, list.ReferenceNumber);
                var sql5 = @"UPDATE acc_receivable
                SET modified_time = @ModifiedTime, modified_by = @ModifiedBy,
                server_modified_time = @ServerModifiedTime, paid_amount = paid_amount + @PaidAmount
                WHERE uid = @UID;";
                Dictionary<string, object> parameters5 = new Dictionary<string, object>
                            {
                                {"PaidAmount", list.Amount == 0 ? list.PaidAmount : list.Amount},
                                {"ModifiedTime", DateTime.Now},
                                {"ModifiedBy", Const.ModifiedBy},
                                {"ServerModifiedTime", DateTime.Now},
                                {"UID", receive.UID}
                            };
                Retval = await ExecuteNonQueryAsync(sql5, conn, parameters5, transaction);
                if (Retval != Const.One)
                    return Retval;
            }
            return Retval;
        }

        public async Task<string> UpdateAccPayable(NpgsqlConnection conn, NpgsqlTransaction transaction, ICollections collection)
        {
            var Retval = string.Empty;
            foreach (var list in collection.AccCollectionAllotment.Where(p => p.TargetType.Contains("INVOICE")))
            {

                //IAccPayable payable = await GetAccPayableAmount(collection.AccCollection.StoreUID, list.ReferenceNumber);

                //var global = collection.Category == "Cash" ? PayAmount += list.PaidAmount : PayUnSettleAmount += list.PaidAmount;

                var sql4 = @"UPDATE acc_payable
                                    SET modified_time = @ModifiedTime, modified_by = @ModifiedBy,
                                    server_modified_time = @ServerModifiedTime, paid_amount = paid_amount + @PaidAmount 
                                    WHERE store_uid = @StoreUID and reference_number = @ReferenceNumber";
                var sql1 = @"UPDATE acc_payable
                                    SET modified_time = @ModifiedTime, modified_by = @ModifiedBy,
                                    server_modified_time = @ServerModifiedTime, unsettled_amount = unsettled_amount + @UnSettledAmount 
                                    WHERE store_uid = @StoreUID and reference_number = @ReferenceNumber";
                Dictionary<string, object> parameters4 = new Dictionary<string, object>
                            {
                                {"PaidAmount", list.PaidAmount==0 ? 0 :(collection.AccCollection.IsEarlyPayment ? (list.PaidAmount + list.DiscountAmount) : list.PaidAmount)}, //list.Amount==0 ? list.PaidAmount :list.Amount
                                {"ModifiedTime", DateTime.Now},
                                {"ModifiedBy", string.IsNullOrEmpty(collection.AccCollection.ModifiedBy) ?string.Empty: collection.AccCollection.ModifiedBy},
                                {"ServerModifiedTime", DateTime.Now},
                                {"StoreUID", collection.AccCollection.StoreUID},
                                {"ReferenceNumber", list.ReferenceNumber}
                            };
                Dictionary<string, object> parameters1 = new Dictionary<string, object>
                            {
                                {"UnSettledAmount", list.PaidAmount==0 ? 0 :(collection.AccCollection.IsEarlyPayment ? (list.PaidAmount + list.DiscountAmount) : list.PaidAmount)},
                                {"ModifiedTime", DateTime.Now},
                                {"ModifiedBy", string.IsNullOrEmpty(collection.AccCollection.ModifiedBy) ?string.Empty: collection.AccCollection.ModifiedBy},
                                {"ServerModifiedTime", DateTime.Now},
                                {"StoreUID", collection.AccCollection.StoreUID},
                                {"ReferenceNumber", list.ReferenceNumber}
                            };

                Retval = collection.AccCollection.Category == Const.Cash ? await ExecuteNonQueryAsync(sql4, conn, parameters4, transaction) : await ExecuteNonQueryAsync(sql1, conn, parameters1, transaction);
                if (Retval != Const.One)
                    return Retval;
            }
            return Retval;
        }
        public async Task<string> InsertAccPayable(NpgsqlConnection conn, NpgsqlTransaction transaction, ICollections collection)
        {
            var Retval = string.Empty;

            foreach (var list in collection.AccPayable)
            {
                if (await CheckIfUIDExistsInDB(DbTableName.AccPayable, list.UID) != null)
                {
                    try
                    {
                        var sql1 = @"INSERT INTO public.acc_payable(
                            uid, created_by, created_time, modified_by, modified_time, server_add_time,
                            server_modified_time, source_type, source_uid, reference_number, org_uid,
                            job_position_uid, amount, paid_amount, store_uid, transaction_date,
                            due_date, balance_amount, unsettled_amount, source, currency_uid)
                            VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime,
                                @ServerModifiedTime, @SourceType, @SourceUID, @ReferenceNumber, @OrgUID,
                                @JobPositionUID, @Amount, @PaidAmount, @StoreUID, @TransactionDate,
                                @DueDate, @BalanceAmount, @UnSettledAmount, @Source, @CurrencyUID)";
                        Dictionary<string, object> parameters1 = new Dictionary<string, object>
                            {
                                {"SourceType", string.IsNullOrEmpty(list.SourceType)  ? string.Empty : list.SourceType},
                                {"SourceUID", string.IsNullOrEmpty(list.SourceUID) ? string.Empty : list.SourceUID},
                                {"ReferenceNumber", string.IsNullOrEmpty(list.ReferenceNumber)  ? string.Empty : list.ReferenceNumber},
                                {"OrgUID", string.IsNullOrEmpty(list.OrgUID) ? string.Empty : list.OrgUID},
                                {"JobPositionUID", string.IsNullOrEmpty(list.JobPositionUID) ? string.Empty : list.JobPositionUID},
                                {"Amount", list.Amount == 0 ? 0 : list.Amount},
                                {"PaidAmount", list.PaidAmount == 0 ? 0 : list.PaidAmount},
                                {"StoreUID", string.IsNullOrEmpty(list.StoreUID ) ?string.Empty : list.StoreUID},
                                {"TransactionDate", list.TransactionDate == null ? DateTime.Now : list.TransactionDate},
                                {"DueDate", list.DueDate == null ? DateTime.Now : list.DueDate},
                                {"BalanceAmount", list.BalanceAmount == 0 ? 0 : list.BalanceAmount},
                                {"UnSettledAmount", list.UnSettledAmount == 0 ? 0 : list.UnSettledAmount},
                                {"Source", string.IsNullOrEmpty(list.Source)  ? string.Empty : list.Source},
                                {"CurrencyUID", string.IsNullOrEmpty(list.CurrencyUID)  ? string.Empty : list.CurrencyUID},
                                {"CreatedTime", DateTime.Now},
                                {"ModifiedTime", DateTime.Now},
                                {"ModifiedBy", Const.ModifiedBy},
                                {"CreatedBy", "WINIT"},
                                {"ServerModifiedTime", DateTime.Now},
                                {"ServerAddTime", DateTime.Now},
                                {"UID", list.UID}
                            };

                        Retval = await ExecuteNonQueryAsync(sql1, conn, parameters1, transaction);

                        if (Retval != Const.One)
                            return Retval;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    return Retval;
                }
            }
            return Retval;
        }
        public async Task<string> InsertAccReceivable(NpgsqlConnection conn, NpgsqlTransaction transaction, ICollections collection)
        {
            var Retval = string.Empty;
            foreach (var list in collection.AccCollectionAllotment)
            {
                if (list.DiscountAmount > 0)
                {
                    var sql1 = @"INSERT INTO public.acc_receivable(
                            uid, created_by, created_time, modified_by, modified_time, server_add_time,
                            server_modified_time, source_type, source_uid, reference_number, org_uid,
                            job_position_uid, amount, paid_amount, store_uid, transaction_date,
                            due_date, source, currency_uid, unsettled_amount)
                            VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime,
                                @ServerModifiedTime, @SourceType, @SourceUID, @ReferenceNumber, @OrgUID,
                                @JobPositionUID, @Amount, @PaidAmount, @StoreUID, @TransactionDate,
                                @DueDate, @Source, @CurrencyUID, @UnSettledAmount)";
                    Dictionary<string, object> parameters1 = new Dictionary<string, object>
                            {
                                {"CreatedTime", DateTime.Now },
                                {"ModifiedTime", DateTime.Now},
                                {"ModifiedBy", string.IsNullOrEmpty(collection.AccCollection.ModifiedBy) ? string.Empty : collection.AccCollection.ModifiedBy },
                                {"ServerAddTime", DateTime.Now },
                                {"ServerModifiedTime", DateTime.Now },
                                {"CreatedBy", string.IsNullOrEmpty(collection.AccCollection.CreatedBy) ?string.Empty: collection.AccCollection.CreatedBy },
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

                    Retval = await ExecuteNonQueryAsync(sql1, conn, parameters1, transaction);

                    if (Retval != Const.One)
                        return Retval;
                }
            }
            return Retval;
        }
        public async Task<string> InsertEarlyPaymentDiscountAppliedDetails(NpgsqlConnection conn, NpgsqlTransaction transaction, ICollections collection)
        {
            var Retval = string.Empty;
            foreach (var list in collection.AccCollectionAllotment)
            {
                if (list.DiscountAmount > 0)
                {
                    var sql1 = @"INSERT INTO early_payment_discount_applied_details 
                            (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                                early_payment_discount_number, invoice_number, invoice_date, invoice_due_amount, sales_org, applicable_type, 
                                applicable_code, payment_mode, advance_paid_days, discount_type, discount_value, isactive, valid_from, 
                                valid_to, applicable_onpartial_payments, applicable_onoverdue_customers) 
                            VALUES 
                            (@uid, @created_by, @created_time, @modified_by, @modified_time, @server_add_time, @server_modified_time, 
                                @early_payment_discount_number, @invoice_number, @invoice_date, @invoice_due_amount, @sales_org, @applicable_type, 
                                @applicable_code, @payment_mode, @advance_paid_days, @discount_type, @discount_value, @isactive, @valid_from, 
                                @valid_to, @applicable_onpartial_payments, @applicable_onoverdue_customers)";
                    Dictionary<string, object> parameters1 = new Dictionary<string, object>
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
                                {"modified_by", string.IsNullOrEmpty(collection.AccCollection.ModifiedBy) ? string.Empty : collection.AccCollection.ModifiedBy},
                                {"created_by", string.IsNullOrEmpty(collection.AccCollection.CreatedBy) ?string.Empty: collection.AccCollection.CreatedBy},
                                {"server_modified_time", DateTime.Now},
                                {"server_add_time", DateTime.Now},
                                {"UID", (Guid.NewGuid()).ToString()}
                            };

                    Retval = await ExecuteNonQueryAsync(sql1, conn, parameters1, transaction);

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
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",  Receiptnumber}
                };


                var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, 
                            server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, acc_collection_uid AS AccCollectionUID, collection_amount AS CollectionAmount, 
                            received_amount AS ReceivedAmount, has_discrepancy AS HasDiscrepancy, discrepancy_amount AS DiscrepancyAmount, default_currency_uid AS DefaultCurrencyUID, 
                            settlement_date AS SettlementDate, cashier_job_position_uid AS CashierJobPositionUID, cashier_emp_uid AS CashierEmpUID, receipt_number AS ReceiptNumber, 
                            settled_by AS SettledBy, session_user_code AS SessionUserCode, route AS Route, user_code AS UserCode, target_type AS TargetType, payment_mode AS PaymentMode, 
                            transaction_date AS TransactionDate, due_date AS DueDate, is_void AS IsVoid, cash_number AS CashNumber
                        FROM 
                            public.acc_collection_settlement 
                        WHERE cash_number = @UID;";
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

        public async Task<Model.Interfaces.ICollections> GetAmount(string ReceiptNumber)
        {

            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"ReceiptNumber",  ReceiptNumber}
                };


                var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                            modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                            server_modified_time AS ServerModifiedTime, receipt_number AS ReceiptNumber, 
                            consolidated_receipt_number AS ConsolidatedReceiptNumber, category AS Category, 
                            amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                            default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount, 
                            org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, 
                            route_uid AS RouteUID, job_position_uid AS JobPositionUID, emp_uid AS EmpUID, 
                            collected_date AS CollectedDate, status AS Status, remarks AS Remarks, 
                            reference_number AS ReferenceNumber, is_realized AS IsRealized, latitude AS Latitude, 
                            longitude AS Longitude, source AS Source, is_multimode AS IsMultimode, 
                            trip_date AS TripDate, comments AS Comments, salesman AS Salesman, 
                            route AS Route, reversal_receipt_uid AS ReversalReceiptUID, cancelled_on AS CancelledOn FROM  
                            public.acc_collection 
                            Where receipt_number = @ReceiptNumber and amount is not null Order By created_time Desc";

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICollections>().GetType();
                Model.Interfaces.ICollections CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.ICollections>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                return (ICollections)ex;
            }
        }
        public async Task<Model.Interfaces.ICollections> GetCustomerName(string ReceiptNumber)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"ReceiptNumber",  ReceiptNumber}
                };
                var sql = @"SELECT  id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                            modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                            server_modified_time AS ServerModifiedTime, receipt_number AS ReceiptNumber, 
                            consolidated_receipt_number AS ConsolidatedReceiptNumber, category AS Category, 
                            amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                            default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount, 
                            org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, 
                            route_uid AS RouteUID, job_position_uid AS JobPositionUID, emp_uid AS EmpUID, 
                            collected_date AS CollectedDate, status AS Status, remarks AS Remarks, 
                            reference_number AS ReferenceNumber, is_realized AS IsRealized, latitude AS Latitude, 
                            longitude AS Longitude, source AS Source, is_multimode AS IsMultimode, 
                            trip_date AS TripDate, comments AS Comments, salesman AS Salesman, 
                            route AS Route, reversal_receipt_uid AS ReversalReceiptUID, cancelled_on AS CancelledOn FROM 
                            public.acc_collection
                            Where receipt_number = @ReceiptNumber";

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICollections>().GetType();
                Model.Interfaces.ICollections CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.ICollections>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                return (ICollections)ex;
            }
        }
        public async Task<Model.Interfaces.IAccCollection> GetAmountCash(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",  UID}
                };
                var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                                modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                                server_modified_time AS ServerModifiedTime, receipt_number AS ReceiptNumber, 
                                consolidated_receipt_number AS ConsolidatedReceiptNumber, category AS Category, 
                                amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                                default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount, 
                                org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, 
                                route_uid AS RouteUID, job_position_uid AS JobPositionUID, emp_uid AS EmpUID, 
                                collected_date AS CollectedDate, status AS Status, remarks AS Remarks, 
                                reference_number AS ReferenceNumber, is_realized AS IsRealized, latitude AS Latitude, 
                                longitude AS Longitude, source AS Source, is_multimode AS IsMultimode, 
                                trip_date AS TripDate, comments AS Comments, salesman AS Salesman, 
                                route AS Route, reversal_receipt_uid AS ReversalReceiptUID, cancelled_on AS CancelledOn FROM 
                                public.acc_collection 
                                Where uid = @UID";
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
                Model.Interfaces.IAccCollection CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IAccCollection>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                return (IAccCollection)ex;
            }
        }
        public async Task<Model.Interfaces.IAccCollection> GetCollectAmount(string ReceiptNumber)
        {

            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"ReceiptNumber",  ReceiptNumber}
                };

                var sql = @"Select min(amount) from acc_collection Where receipt_number = @ReceiptNumber";

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
                Model.Interfaces.IAccCollection CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IAccCollection>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                return (IAccCollection)ex;
            }
        }
        public async Task<Model.Interfaces.IAccCollection> GetCollectionAmount(string ReceiptNumber)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"AccCollectionUID",  ReceiptNumber}
                };


            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                        modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, receipt_number AS ReceiptNumber, 
                        consolidated_receipt_number AS ConsolidatedReceiptNumber, category AS Category, 
                        amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount, 
                        org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, 
                        route_uid AS RouteUID, job_position_uid AS JobPositionUID, emp_uid AS EmpUID, 
                        collected_date AS CollectedDate, status AS Status, remarks AS Remarks, 
                        reference_number AS ReferenceNumber, is_realized AS IsRealized, latitude AS Latitude, 
                        longitude AS Longitude, source AS Source, is_multimode AS IsMultimode, 
                        trip_date AS TripDate, comments AS Comments, salesman AS Salesman, 
                        route AS Route, reversal_receipt_uid AS ReversalReceiptUID, cancelled_on AS CancelledOn FROM 
                        public.acc_collection  
                        Where uid = @AccCollectionUID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
            Model.Interfaces.IAccCollection CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IAccCollection>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<Model.Interfaces.IAccCollectionPaymentMode> GetPaymentAmount(string ReceiptNumber, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"AccCollectionUID",  ReceiptNumber}
                };

            var sql = @"SELECT 
                        id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                        modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, acc_collection_uid AS AccCollectionUID, 
                        bank_uid AS BankUID, branch AS Branch, cheque_no AS ChequeNo, amount AS Amount, 
                        currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                        default_currency_amount AS DefaultCurrencyAmount, cheque_date AS ChequeDate, 
                        status AS Status, realization_date AS RealizationDate, comments AS Comments, 
                        approve_comments AS ApproveComments FROM 
                        public.acc_collection_payment_mode 
                        Where acc_collection_uid = @AccCollectionUID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionPaymentMode>().GetType();
            Model.Interfaces.IAccCollectionPaymentMode CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IAccCollectionPaymentMode>(sql, parameters, type, conn, transaction);
            return CollectionModuleList;
        }

        public async Task<IEnumerable<Model.Interfaces.IAccCollectionAllotment>> GetAllotmentAmount(string TargetUID, string AccCollectionUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"TargetUID",  TargetUID},
                    {"AccCollectionUID",  AccCollectionUID}
                };


            var sql = @"SELECT 
                        id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                        modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, acc_collection_uid AS AccCollectionUID, 
                        target_type AS TargetType, target_uid AS TargetUID, reference_number AS ReferenceNumber, 
                        amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                        default_currency_amount AS DefaultCurrencyAmount, 
                        early_payment_discount_percentage AS EarlyPaymentDiscountPercentage, 
                        early_payment_discount_amount AS EarlyPaymentDiscountAmount, 
                        early_payment_discount_reference_no AS EarlyPaymentDiscountReferenceNo
                    FROM 
                        acc_collection_allotment 
                    WHERE 
                        (acc_collection_uid = @AccCollectionUID AND target_uid = @TargetUID AND amount IS NOT NULL)
                        OR 
                        (target_uid LIKE '%CN%' AND amount != 0)
                    ORDER BY 
                        created_time DESC;";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionAllotment>().GetType();
            IEnumerable<Model.Interfaces.IAccCollectionAllotment> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionAllotment>(sql, parameters, type);
            return CollectionModuleList;
        }

        public async Task<IEnumerable<Model.Interfaces.IAccCollectionAllotment>> AllInvoices(string AccCollectionUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"AccCollectionUID",  AccCollectionUID}
                };

            var sql = @"SELECT 
                        id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                        modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, acc_collection_uid AS AccCollectionUID, 
                        target_type AS TargetType, target_uid AS TargetUID, reference_number AS ReferenceNumber, 
                        amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                        default_currency_amount AS DefaultCurrencyAmount, 
                        early_payment_discount_percentage AS EarlyPaymentDiscountPercentage, 
                        early_payment_discount_amount AS EarlyPaymentDiscountAmount, 
                        early_payment_discount_reference_no AS EarlyPaymentDiscountReferenceNo
                    FROM 
                        acc_collection_allotment Where acc_collection_uid = @AccCollectionUID 
                        and amount != 0)
                                    or
                    (target_uid like '%CN%' and amount != 0) Order By created_time Desc";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionAllotment>().GetType();
            IEnumerable<Model.Interfaces.IAccCollectionAllotment> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionAllotment>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.IAccCollectionAllotment>> GetCashDetails(string AccCollectionUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"AccCollectionUID",  AccCollectionUID}
                };

            var sql = @"SELECT 
                        id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                        modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, acc_collection_uid AS AccCollectionUID, 
                        target_type AS TargetType, target_uid AS TargetUID, reference_number AS ReferenceNumber, 
                        amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                        default_currency_amount AS DefaultCurrencyAmount, 
                        early_payment_discount_percentage AS EarlyPaymentDiscountPercentage, 
                        early_payment_discount_amount AS EarlyPaymentDiscountAmount, 
                        early_payment_discount_reference_no AS EarlyPaymentDiscountReferenceNo
                    FROM 
                        acc_collection_allotment where acc_collection_uid =@AccCollectionUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionAllotment>().GetType();
            IEnumerable<Model.Interfaces.IAccCollectionAllotment> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionAllotment>(sql, parameters, type);
            return CollectionModuleList;
        }

        public async Task<PagedResponse<Model.Interfaces.IAccCollection>> PaymentDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string AccCollectionUID)
        {
            try
            {
                var sql = new StringBuilder(@"select Id, UID, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime, ServerAddTime, ServerModifiedTime, 
                        ReceiptNumber, ConsolidatedReceiptNumber, Category, StoreUID, IsReversal, SessionUserCode, ReversedBy, 
                        PaidAmount, ChequeNo, CashNumber, SettledBy, IsSettled, IsVoid from AccCollection");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM AccCollection");
                }
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"AccCollectionUID",  AccCollectionUID}
                };
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE \"IsReversal\" = true And \"CashNumber\" Is Not null And ");
                    AppendFilterCriteria<Model.Interfaces.IAccCollection>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                else
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE \"IsReversal\" = true And \"CashNumber\" Is Not null");
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY \"CreatedTime\" desc");
                    AppendSortCriteria(sortCriterias, sql);
                }
                else
                {
                    sql.Append(" ORDER BY \"CreatedTime\" desc");
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


        public async Task<IEnumerable<Model.Interfaces.IAccCollection>> SettledDetails(string Status)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"Status",  Status}
                };

            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                        modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, receipt_number AS ReceiptNumber, 
                        consolidated_receipt_number AS ConsolidatedReceiptNumber, category AS Category, 
                        amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount, 
                        org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, 
                        route_uid AS RouteUID, job_position_uid AS JobPositionUID, emp_uid AS EmpUID, 
                        collected_date AS CollectedDate, status AS Status, remarks AS Remarks, 
                        reference_number AS ReferenceNumber, is_realized AS IsRealized, latitude AS Latitude, 
                        longitude AS Longitude, source AS Source, is_multimode AS IsMultimode, 
                        trip_date AS TripDate, comments AS Comments, salesman AS Salesman, 
                        route AS Route, reversal_receipt_uid AS ReversalReceiptUID, cancelled_on AS CancelledOn FROM 
                        public.acc_collection where status = @Status";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
            IEnumerable<Model.Interfaces.IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<PagedResponse<Model.Interfaces.IAccCollection>> CashierSettlement(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from(select ac.uid as UID, ac.currency_uid as CurrencyUID, ac.receipt_number as ReceiptNumber, ac.status as Status, ac.comments as Comments,
                            ac.created_time as CollectedDate, ac.category as Category, ac.amount as Amount, ac.default_currency_amount as DefaultCurrencyAmount, '('||s.code ||')' || s.name as StoreUID,s.name as storeName, ac.created_time as CreatedTime 
                            from acc_collection as ac join store as s on ac.store_uid = s.uid) as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (select ac.uid as UID, ac.currency_uid as CurrencyUID, ac.receipt_number as ReceiptNumber, ac.status as Status, ac.comments as Comments,
                            ac.created_time as CollectedDate, ac.category as Category, ac.amount as Amount, ac.default_currency_amount as DefaultCurrencyAmount, '('||s.code ||')' || s.name as StoreUID,s.name as storeName, ac.created_time as CreatedTime 
                            from acc_collection as ac join store as s on ac.store_uid = s.uid) as SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE Status = 'Collected' And ");
                    AppendFilterCriteria<Model.Interfaces.IAccCollectionSettlement>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                else
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE Status = 'Collected'");
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY CreatedTime desc, ");
                    AppendSortCriteria(sortCriterias, sql, true);
                }
                else
                {
                    sql.Append(" ORDER BY CreatedTime desc");
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
        public async Task<PagedResponse<Model.Interfaces.IAccCollection>> CashierSettlementVoid(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            var sql = new StringBuilder(@"Select * from (select ac.uid as UID, ac.currency_uid as CurrencyUID, ac.receipt_number as ReceiptNumber, ac.status as Status, ac.comments as Comments, ac.created_time as CollectedDate,
                                    ac.category as Category, ac.amount as Amount, ac.default_currency_amount as DefaultCurrencyAmount, '('||s.code ||')' || s.name as StoreUID,s.name as storeName, ac.created_time as CreatedTime
                                    from acc_collection as ac join store as s on ac.store_uid = s.uid) as SubQuery");
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (select ac.uid as UID, ac.currency_uid as CurrencyUID, ac.receipt_number as ReceiptNumber, ac.status as Status, ac.comments as Comments, ac.created_time as CollectedDate,
                                    ac.category as Category, ac.amount as Amount, ac.default_currency_amount as DefaultCurrencyAmount, '('||s.code ||')' || s.name as StoreUID,s.name as storeName, ac.created_time as CreatedTime
                                    from acc_collection as ac join store as s on ac.store_uid = s.uid) as SubQuery");
            }
            var parameters = new Dictionary<string, object>();
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE Status = 'Voided' And ");
                AppendFilterCriteria<Model.Interfaces.IAccCollectionSettlement>(filterCriterias, sbFilterCriteria, parameters);
                sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    sqlCount.Append(sbFilterCriteria);
                }
            }
            else
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE Status = 'Voided'");
                sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    sqlCount.Append(sbFilterCriteria);
                }
            }
            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                sql.Append(" ORDER BY CreatedTime desc, ");
                AppendSortCriteria(sortCriterias, sql, true);
            }
            else
            {
                sql.Append(" ORDER BY CreatedTime desc");
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
            var sql = new StringBuilder(@"Select * from (select ac.uid as UID, ac.currency_uid as CurrencyUID, ac.receipt_number as ReceiptNumber, ac.status as Status, ac.comments as Comments, ac.created_time as CollectedDate,
                                ac.category as Category, ac.amount as Amount, ac.default_currency_amount as DefaultCurrencyAmount, '('||s.code ||')' || s.name as StoreUID,s.name as storeName, ac.created_time as CreatedTime 
                                from acc_collection as ac join store as s on ac.store_uid = s.uid) as SubQuery");
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (select ac.uid as UID, ac.currency_uid as CurrencyUID, ac.receipt_number as ReceiptNumber, ac.status as Status, ac.comments as Comments, ac.created_time as CollectedDate,
                                ac.category as Category, ac.amount as Amount, ac.default_currency_amount as DefaultCurrencyAmount, '('||s.code ||')' || s.name as StoreUID,s.name as storeName, ac.created_time as CreatedTime 
                                from acc_collection as ac join store as s on ac.store_uid = s.uid) as SubQuery");
            }
            var parameters = new Dictionary<string, object>();
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE Category = 'Cash' and Status in ('Settled','Reversed') And ");
                AppendFilterCriteria<Model.Interfaces.IAccCollectionSettlement>(filterCriterias, sbFilterCriteria, parameters);
                sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    sqlCount.Append(sbFilterCriteria);
                }
            }
            else
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE Category = 'Cash' and Status in ('Settled','Reversed')");
                sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    sqlCount.Append(sbFilterCriteria);
                }
            }
            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                sql.Append(" ORDER BY CreatedTime desc, ");
                AppendSortCriteria(sortCriterias, sql, true);
            }
            else
            {
                sql.Append(" ORDER BY CreatedTime desc");
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

        public async Task<PagedResponse<Model.Interfaces.IAccCollectionAllotment>> ShowPaymentDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"Select * from AccCollectionAllotment Al 
						INNER JOIN AccCollection Ac on Al.AccCollectionUID = Ac.UID");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM AccCollectionAllotment Al");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Model.Interfaces.IAccCollectionAllotment>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY Al.\"CreatedTime\" desc");
                    AppendSortCriteria(sortCriterias, sql);
                }
                else
                {
                    sql.Append(" ORDER BY Al.\"CreatedTime\" desc");
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
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                 {"AccCollectionUID",  UID}
            };

            var sql = @"SELECT 
                    AL.id AS Id, AL.uid AS UID, AL.created_by AS CreatedBy, AL.created_time AS CreatedTime, 
                    AL.modified_by AS ModifiedBy, AL.modified_time AS ModifiedTime, AL.server_add_time AS ServerAddTime,
                    AL.server_modified_time AS ServerModifiedTime, AL.acc_collection_uid AS AccCollectionUid, 
                    AL.target_type AS TargetType, AL.target_uid AS TargetUid, AL.reference_number AS ReferenceNumber, 
                    AL.amount AS Amount, AL.currency_uid AS CurrencyUid, AL.default_currency_uid AS DefaultCurrencyUid, 
                    AL.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, AL.default_currency_amount AS DefaultCurrencyAmount,
                    AL.early_payment_discount_percentage AS EarlyPaymentDiscountPercentage, 
                    AL.early_payment_discount_amount AS EarlyPaymentDiscountAmount, 
                    AL.early_payment_discount_reference_no AS EarlyPaymentDiscountReferenceNo,
                    AC.id AS Id, AC.uid AS UID, AC.created_by AS CreatedBy, AC.created_time AS CreatedTime, 
                    AC.modified_by AS ModifiedBy, AC.modified_time AS ModifiedTime, AC.server_add_time AS ServerAddTime,
                    AC.server_modified_time AS ServerModifiedTime, AC.receipt_number AS ReceiptNumber, 
                    AC.consolidated_receipt_number AS ConsolidatedReceiptNumber, AC.category AS Category, AC.amount AS Amount,
                    AC.currency_uid AS CurrencyUid, AC.default_currency_uid AS DefaultCurrencyUid, 
                    AC.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, AC.default_currency_amount AS DefaultCurrencyAmount, 
                    AC.org_uid AS OrgUid, AC.distribution_channel_uid AS DistributionChannelUid, 
                    AC.store_uid AS StoreUid, AC.route_uid AS RouteUid, AC.job_position_uid AS JobPositionUid, AC.emp_uid AS EmpUid,
                    AC.collected_date AS CollectedDate, AC.status AS Status, AC.remarks AS Remarks, AC.is_realized AS IsRealized, AC.latitude AS Latitude, 
                    AC.longitude AS Longitude, AC.source AS Source, AC.is_multimode AS IsMultimode, AC.trip_date AS TripDate, 
                    AC.comments AS Comments, AC.salesman AS Salesman, AC.route AS Route, AC.reversal_receipt_uid AS ReversalReceiptUid,
                    AC.cancelled_on AS CancelledOn
                FROM 
                    public.acc_collection_allotment AL
                INNER JOIN 
                    public.acc_collection AC ON AC.uid = AL.acc_collection_uid
                WHERE 
                    AL.acc_collection_uid = @AccCollectionUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionAllotment>().GetType();
            IEnumerable<Model.Interfaces.IAccCollectionAllotment> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionAllotment>(sql, parameters, type);
            return CollectionModuleList;
        }

        public async Task<IEnumerable<Model.Interfaces.IAccPayable>> PayableUnsettle(string AccCollectionUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                 {"AccCollectionUID",  AccCollectionUID}
            };

            var sql = @"SELECT 
                AP.id AS Id, AP.uid AS UID, AP.created_by AS CreatedBy, AP.created_time AS CreatedTime, AP.modified_by AS ModifiedBy, AP.modified_time AS ModifiedTime, AP.server_add_time AS ServerAddTime,
            AP.server_modified_time AS ServerModifiedTime, AP.source_type AS SourceType, AP.source_uid AS SourceUID, AP.reference_number AS ReferenceNumber, AP.org_uid AS OrgUID, AP.job_position_uid AS JobPositionUID,
            AP.amount AS Amount, AP.paid_amount AS PaidAmount, AP.store_uid AS StoreUID, AP.transaction_date AS TransactionDate, AP.due_date AS DueDate, AP.balance_amount AS BalanceAmount, AP.unsettled_amount AS UnSettledAmount,
            AP.source AS Source, AP.currency_uid AS CurrencyUID
            FROM 
                public.acc_collection_allotment AL
            INNER JOIN 
                public.acc_payable AP ON AL.target_uid = AP.uid
            WHERE 
                AL.acc_collection_uid = @AccCollectionUID;";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
            IEnumerable<Model.Interfaces.IAccPayable> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql, parameters, type);
            return CollectionModuleList;
        }
        //getting cheque records for reversal

        public async Task<IEnumerable<Model.Interfaces.IAccCollectionAllotment>> GetCashAllot(string AccCollectionUID, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                 {"AccCollectionUID",  AccCollectionUID}
            };

            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
            server_modified_time AS ServerModifiedTime, acc_collection_uid AS AccCollectionUID, target_type AS TargetType, target_uid AS TargetUID, reference_number AS ReferenceNumber, amount AS Amount,
            currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount,
            early_payment_discount_percentage AS EarlyPaymentDiscountPercentage, early_payment_discount_amount AS EarlyPaymentDiscountAmount, early_payment_discount_reference_no AS EarlyPaymentDiscountReferenceNo
            FROM 
            public.acc_collection_allotment where acc_collection_uid = @AccCollectionUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionAllotment>().GetType();
            IEnumerable<Model.Interfaces.IAccCollectionAllotment> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionAllotment>(sql, parameters, type, conn, transaction);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.IAccPayable>> GetChequePay(string AccCollectionUID, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                 {"AccCollectionUID",  AccCollectionUID}
            };


            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime, source_type AS SourceType, source_uid AS SourceUID, reference_number AS ReferenceNumber, org_uid AS OrgUID, job_position_uid AS JobPositionUID,
                amount AS Amount, paid_amount AS PaidAmount, store_uid AS StoreUID, transaction_date AS TransactionDate, due_date AS DueDate, balance_amount AS BalanceAmount, unsettled_amount AS UnSettledAmount,
                source AS Source, currency_uid AS CurrencyUID
                FROM 
                    public.acc_payable
                where uid = @AccCollectionUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
            IEnumerable<Model.Interfaces.IAccPayable> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql, parameters, type, conn, transaction);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.IAccReceivable>> GetChequeRec(string DocumentType, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                 {"DocumentType",  DocumentType}
            };

            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
            server_modified_time AS ServerModifiedTime, source_type AS SourceType, source_uid AS SourceUID, reference_number AS ReferenceNumber, org_uid AS OrgUID, job_position_uid AS JobPositionUID,
            currency_uid AS CurrencyUID, amount AS Amount, paid_amount AS PaidAmount, store_uid AS StoreUID, transaction_date AS TransactionDate, due_date AS DueDate, unsettled_amount AS UnSettledAmount,
            balance_amount AS BalanceAmount, source AS Source FROM 
            public.acc_receivable where uid = @DocumentType";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccReceivable>().GetType();
            IEnumerable<Model.Interfaces.IAccReceivable> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccReceivable>(sql, parameters, type, conn, transaction);
            return CollectionModuleList;
        }
        //getting cheque records for reversal


        //getting records to update balance in AccStoreLedger
        public async Task<Model.Interfaces.IAccStoreLedger> GetStoreLedger(string ReceiptNumber, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"SourceUID",  ReceiptNumber}
                };

            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, 
            server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, source_type AS SourceType, source_uid AS SourceUID, credit_type AS CreditType, org_uid AS OrgUID,
            store_uid AS StoreUID, default_currency_uid AS DefaultCurrencyUID, document_number AS DocumentNumber, default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
            default_currency_amount AS DefaultCurrencyAmount, transaction_date_time AS TransactionDateTime, collected_amount AS CollectedAmount, currency_uid AS CurrencyUID,
            amount AS Amount, balance AS Balance, comments AS Comments FROM public.acc_store_ledger 
            where source_uid = @SourceUID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccStoreLedger>().GetType();
            Model.Interfaces.IAccStoreLedger CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IAccStoreLedger>(sql, parameters, type, conn, transaction);
            return CollectionModuleList;

        }

        public async Task<Model.Interfaces.IAccStoreLedger> GetStoreLedgerCreation(string StoreUID, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
         {
             {"StoreUID",  StoreUID}
         };

            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, 
            server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, source_type AS SourceType, source_uid AS SourceUID, credit_type AS CreditType, org_uid AS OrgUID,
            store_uid AS StoreUID, default_currency_uid AS DefaultCurrencyUID, document_number AS DocumentNumber, default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
            default_currency_amount AS DefaultCurrencyAmount, transaction_date_time AS TransactionDateTime, collected_amount AS CollectedAmount, currency_uid AS CurrencyUID,
            amount AS Amount, balance AS Balance, comments AS Comments FROM public.acc_store_ledger  where store_uid = @StoreUID Order By created_time Desc";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccStoreLedger>().GetType();
            Model.Interfaces.IAccStoreLedger CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IAccStoreLedger>(sql, parameters, type, conn, transaction);
            return CollectionModuleList;

        }

        public async Task<Model.Interfaces.IAccReceivable> GetAccReceivableAmount(string ReferenceNumber)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"ReferenceNumber",  ReferenceNumber}
                };

            var sql = @"Select * from AccReceivable Where ReferenceNumber = @ReferenceNumber";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccReceivable>().GetType();
            Model.Interfaces.IAccReceivable CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IAccReceivable>(sql, parameters, type);
            return CollectionModuleList;
        }

        //to get payable details and update in payable table
        public async Task<Model.Interfaces.IAccPayable> GetAccPayableAmount(string StoreUID, string ReferenceNumber)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"StoreUID",  StoreUID},
                    {"ReferenceNumber",  ReferenceNumber}
                };

            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime, source_type AS SourceType, source_uid AS SourceUID, reference_number AS ReferenceNumber, org_uid AS OrgUID, job_position_uid AS JobPositionUID,
                amount AS Amount, paid_amount AS PaidAmount, store_uid AS StoreUID, transaction_date AS TransactionDate, due_date AS DueDate, balance_amount AS BalanceAmount, unsettled_amount AS UnSettledAmount,
                source AS Source, currency_uid AS CurrencyUID
                FROM 
                    public.acc_payable Where store_uid = @StoreUID and reference_number = @ReferenceNumber";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
            Model.Interfaces.IAccPayable CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IAccPayable>(sql, parameters, type);
            return CollectionModuleList;
        }
        //get records for auto allocate 
        public async Task<IEnumerable<Model.Interfaces.IAccPayable>> GetInvoicesAutoAllocate(string StoreUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"StoreUID",  StoreUID},
                };
            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime, source_type AS SourceType, source_uid AS SourceUID, reference_number AS ReferenceNumber, org_uid AS OrgUID, job_position_uid AS JobPositionUID,
                amount AS Amount, paid_amount AS PaidAmount, store_uid AS StoreUID, transaction_date AS TransactionDate, due_date AS DueDate, balance_amount AS BalanceAmount, unsettled_amount AS UnSettledAmount,
                source AS Source, currency_uid AS CurrencyUID
                FROM 
                    public.acc_payable 
                        Where  store_uid = @StoreUID order by transaction_date";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
            IEnumerable<Model.Interfaces.IAccPayable> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.IAccPayable>> GetInvoicesAutoAllocateByUID(int Start, int End, string StoreUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"startDay",  Start},
                    {"endDay",  End},
                    {"StoreUID",  StoreUID},
                };

            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime, source_type AS SourceType, source_uid AS SourceUID, reference_number AS ReferenceNumber, org_uid AS OrgUID, job_position_uid AS JobPositionUID,
                amount AS Amount, paid_amount AS PaidAmount, store_uid AS StoreUID, transaction_date AS TransactionDate, due_date AS DueDate, balance_amount AS BalanceAmount, unsettled_amount AS UnSettledAmount,
                source AS Source, currency_uid AS CurrencyUID
                FROM 
                    public.acc_payable
                                WHERE store_uid = @StoreUID
                                  AND CURRENT_DATE - transaction_date BETWEEN @startDay AND @endDay Order By transaction_date";

            var sql1 = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime, source_type AS SourceType, source_uid AS SourceUID, reference_number AS ReferenceNumber, org_uid AS OrgUID, job_position_uid AS JobPositionUID,
                amount AS Amount, paid_amount AS PaidAmount, store_uid AS StoreUID, transaction_date AS TransactionDate, due_date AS DueDate, balance_amount AS BalanceAmount, unsettled_amount AS UnSettledAmount,
                source AS Source, currency_uid AS CurrencyUID
                FROM 
                    public.acc_payable
                                WHERE store_uid = @StoreUID
                                  AND CURRENT_DATE - transaction_date >= @startDay Order By transaction_date";

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
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"StoreUID",  StoreUID},
                    {"ReferenceNumber",  ReferenceNumber},
                };

            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
            server_modified_time AS ServerModifiedTime, source_type AS SourceType, source_uid AS SourceUID, reference_number AS ReferenceNumber, org_uid AS OrgUID, job_position_uid AS JobPositionUID,
            currency_uid AS CurrencyUID, amount AS Amount, paid_amount AS PaidAmount, store_uid AS StoreUID, transaction_date AS TransactionDate, due_date AS DueDate, unsettled_amount AS UnSettledAmount,
            balance_amount AS BalanceAmount, source AS Source FROM 
            public.acc_receivable Where store_uid = @StoreUID and reference_number = @ReferenceNumber";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccReceivable>().GetType();
            Model.Interfaces.IAccReceivable CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IAccReceivable>(sql, parameters, type);
            return CollectionModuleList;
        }

        //End

        //Collection Module
        public async Task<IEnumerable<Model.Interfaces.ICollectionModule>> GetAllOutstandingTransactionsByCustomerCode(string CustomerCode, string SessionUserCode = null, string SalesOrgCode = null)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"CustomerCode",  CustomerCode}
            };

            var sql = @"SELECT unsettled_amount AS UnSettledAmount, target_uid AS TargetUID, CONCAT(S.alias_name, '(', S.code, ')') AS CodeName, Ap.uid AS UID, amount AS Amount, paid_amount AS PaidAmount, store_uid AS StoreUID, transaction_date AS TransactionDate, due_date AS DueDate, balance_amount AS BalanceAmount
            FROM acc_payable Ap 
            INNER JOIN store S ON S.uid = Ap.store_uid
            WHERE S.uid = @CustomerCode
            UNION 
            SELECT unsettled_amount AS UnSettledAmount, target_uid AS TargetUID, CONCAT(S.alias_name, '(', S.code, ')') AS CodeName, Ar.uid AS UID, amount AS Amount, paid_amount AS PaidAmount, store_uid AS StoreUID, transaction_date AS TransactionDate, due_date AS DueDate, balance_amount AS BalanceAmount
            FROM acc_receivable Ar
            INNER JOIN store S ON S.uid = Ar.store_uid
            WHERE S.uid = @CustomerCode;";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICollectionModule>().GetType();
            IEnumerable<Model.Interfaces.ICollectionModule> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.ICollectionModule>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.IExchangeRate>> GetAllConfiguredCurrencyDetailsBySalesOrgCode(string SessionUserCode, string SalesOrgCode = null)
        {


            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                //{"SourceType",  SessionUserCode},
                {"UID",  SessionUserCode}
            };

            var sql = @"SELECT id AS Id, uid AS Uid, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime,
            server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, from_currency_uid AS FromCurrencyUid, to_currency_uid AS ToCurrencyUid, rate AS Rate,
            effective_date AS EffectiveDate, is_active AS IsActive, source AS Source
            FROM public.exchange_rate;";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IExchangeRate>().GetType();
            IEnumerable<Model.Interfaces.IExchangeRate> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IExchangeRate>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.IAccCollectionAllotment>> GetAllConfiguredDocumentTypesBySalesOrgCode(string SessionUserCode, string SalesOrgCode = null)
        {


            Dictionary<string, object> parameters = new Dictionary<string, object>
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


            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                //{"SourceType",  SessionUserCode},
                {"StoreUID",  SessionUserCode}
            };

            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime, source_type AS SourceType, source_uid AS SourceUID, reference_number AS ReferenceNumber, org_uid AS OrgUID, job_position_uid AS JobPositionUID,
                amount AS Amount, paid_amount AS PaidAmount, store_uid AS StoreUID, transaction_date AS TransactionDate, due_date AS DueDate, balance_amount AS BalanceAmount, unsettled_amount AS UnSettledAmount,
                source AS Source, currency_uid AS CurrencyUID
                FROM 
                    public.acc_payable Where store_uid = @StoreUID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
            IEnumerable<Model.Interfaces.IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<IStore>> GetAllCustomersBySalesOrgCode(string SessionUserCode, string SalesOrgCode = null)
        {


            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                //{"SourceType",  SessionUserCode},
                {"StoreUID",  SessionUserCode}
            };


            var sql = @"SELECT code AS Code,
                    uid AS UID,
                    CONCAT(name, '(', code, ')') AS Name 
					FROM store 
					where franchisee_org_uid = @StoreUID 
					AND type = 'FRC' AND is_active = 'true'";

            Type type = _serviceProvider.GetRequiredService<IStore>().GetType();
            IEnumerable<IStore> CollectionModuleList = await ExecuteQueryAsync<IStore>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.ICollectionModule>> GetAllOutstandingTransactionsByMultipleFilters(string SessionUserCode, string SalesOrgCode = null, string CustomerCode = null, string StartDueDate = null, string EndDueDate = null, string StartInvoiceDate = null, string EndInvoiceDate = null)
        {


            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                //{"SourceType",  SessionUserCode},
                {"StoreUID",  SessionUserCode}
            };

            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                        modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, receipt_number AS ReceiptNumber, 
                        consolidated_receipt_number AS ConsolidatedReceiptNumber, category AS Category, 
                        amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount, 
                        org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, 
                        route_uid AS RouteUID, job_position_uid AS JobPositionUID, emp_uid AS EmpUID, 
                        collected_date AS CollectedDate, status AS Status, remarks AS Remarks, 
                        reference_number AS ReferenceNumber, is_realized AS IsRealized, latitude AS Latitude, 
                        longitude AS Longitude, source AS Source, is_multimode AS IsMultimode, 
                        trip_date AS TripDate, comments AS Comments, salesman AS Salesman, 
                        route AS Route, reversal_receipt_uid AS ReversalReceiptUID, cancelled_on AS CancelledOn FROM 
                        public.acc_collection Where store_uid = @StoreUID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICollectionModule>().GetType(); ;
            IEnumerable<Model.Interfaces.ICollectionModule> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.ICollectionModule>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.ICollectionModule>> AllocateSelectedInvoiceswithCreditNotes(string SessionUserCode, string TrxCode = null, string TrxType = null, string PaidAmount = null)
        {


            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                //{"SourceType",  TrxCode},
                {"StoreUID",  SessionUserCode}
            };

            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                        modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, receipt_number AS ReceiptNumber, 
                        consolidated_receipt_number AS ConsolidatedReceiptNumber, category AS Category, 
                        amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount, 
                        org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, 
                        route_uid AS RouteUID, job_position_uid AS JobPositionUID, emp_uid AS EmpUID, 
                        collected_date AS CollectedDate, status AS Status, remarks AS Remarks, 
                        reference_number AS ReferenceNumber, is_realized AS IsRealized, latitude AS Latitude, 
                        longitude AS Longitude, source AS Source, is_multimode AS IsMultimode, 
                        trip_date AS TripDate, comments AS Comments, salesman AS Salesman, 
                        route AS Route, reversal_receipt_uid AS ReversalReceiptUID, cancelled_on AS CancelledOn FROM 
                        public.acc_collection Where store_uid = @StoreUID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICollectionModule>().GetType();
            IEnumerable<Model.Interfaces.ICollectionModule> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.ICollectionModule>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.ICollectionModule>> GetOrgwiseConfigurationsData(string SessionUserCode, string OrgUID = null)
        {


            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                //{"StoreUID",  SessionUserCode},
                {"StoreUID",  SessionUserCode}
            };

            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                        modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, receipt_number AS ReceiptNumber, 
                        consolidated_receipt_number AS ConsolidatedReceiptNumber, category AS Category, 
                        amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount, 
                        org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, 
                        route_uid AS RouteUID, job_position_uid AS JobPositionUID, emp_uid AS EmpUID, 
                        collected_date AS CollectedDate, status AS Status, remarks AS Remarks, 
                        reference_number AS ReferenceNumber, is_realized AS IsRealized, latitude AS Latitude, 
                        longitude AS Longitude, source AS Source, is_multimode AS IsMultimode, 
                        trip_date AS TripDate, comments AS Comments, salesman AS Salesman, 
                        route AS Route, reversal_receipt_uid AS ReversalReceiptUID, cancelled_on AS CancelledOn FROM 
                        public.acc_collection Where store_uid = @StoreUID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICollectionModule>().GetType(); ;
            IEnumerable<Model.Interfaces.ICollectionModule> CollectionList = await ExecuteQueryAsync<Model.Interfaces.ICollectionModule>(sql, parameters, type);
            return CollectionList;

        }
        public async Task<IEnumerable<Model.Interfaces.ICollectionModule>> GetOrgwiseConfigValueByConfigName(string SessionUserCode, string OrgUID = null, string Configname = null)
        {


            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                //{"StoreUID",  SessionUserCode},
                {"StoreUID",  SessionUserCode}
            };

            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                        modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, receipt_number AS ReceiptNumber, 
                        consolidated_receipt_number AS ConsolidatedReceiptNumber, category AS Category, 
                        amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount, 
                        org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, 
                        route_uid AS RouteUID, job_position_uid AS JobPositionUID, emp_uid AS EmpUID, 
                        collected_date AS CollectedDate, status AS Status, remarks AS Remarks, 
                        reference_number AS ReferenceNumber, is_realized AS IsRealized, latitude AS Latitude, 
                        longitude AS Longitude, source AS Source, is_multimode AS IsMultimode, 
                        trip_date AS TripDate, comments AS Comments, salesman AS Salesman, 
                        route AS Route, reversal_receipt_uid AS ReversalReceiptUID, cancelled_on AS CancelledOn FROM 
                        public.acc_collection Where store_uid = @StoreUID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICollectionModule>().GetType(); ;
            IEnumerable<Model.Interfaces.ICollectionModule> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.ICollectionModule>(sql, parameters, type);
            return CollectionModuleList;
        }

        public async Task<string> CreateReceipt(Model.Interfaces.ICollections[] Collection)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var res = string.Empty;
                foreach (var collection in Collection)
                {
                    _creditID = CreditID();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            var accCollec = await InsertAccCollection(conn, transaction, collection);

                            var accPaymode = await InsertAccCollectionPaymentMode(conn, transaction, collection);

                            var accStore = await InsertAccStoreLedger(conn, transaction, collection);

                            var accPay = await UpdateAccPayable(conn, transaction, collection);

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
                                if (accCollec != Const.One || accPaymode != Const.One || accStore != Const.One || accPay != Const.One || accAllot != Const.One ||
                                     accEarly != Const.One)
                                {
                                    transaction.Rollback();
                                    return "Failed";
                                }
                            }
                            if (accCollec != Const.One || accPaymode != Const.One || accStore != Const.One || accAllot != Const.One)
                            {
                                transaction.Rollback();
                                return "Failure";
                            }
                            else
                            {
                                transaction.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return "Failure";
                        }
                    }
                }
                return Const.SuccessInsert;
            }
        }

        public async Task<string> CreateReceiptWithZeroValue(Model.Interfaces.ICollections collection)
        {
            //using (var conn = new NpgsqlConnection(_connectionString))
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
            //                    var val = ;
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
            return string.Empty;
        }



        public async Task<int> CreateReceiptWithAutoAllocation(Model.Interfaces.ICollections[] Collection)
        {
            bool iterate = true;
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                foreach (var collection in Collection)
                {
                    using (var transaction = conn.BeginTransaction())
                    {
                        decimal enteredAmount = 0;
                        decimal Balance = 0;
                        decimal balanceAmount = 0;
                        decimal arrayAmount = 0;
                        decimal extra = 0;
                        var Retval = string.Empty;
                        var val = string.Empty;
                        var accCollec = await InsertAccCollection(conn, transaction, collection);

                        var accPaymode = await InsertAccCollectionPaymentMode(conn, transaction, collection);

                        var accStore = await InsertAccStoreLedger(conn, transaction, collection);

                        IEnumerable<IAccPayable> accCollectionPayable;
                        IEnumerable<IEarlyPaymentDiscountConfiguration> eligiblerecords;
                        if (collection.AccCollectionAllotment.Any())
                        {
                            //eligiblerecords = await CheckEligibleForDiscount(collection.StoreUID);
                            accCollectionPayable = await GetInvoicesAutoAllocateByUID(collection.AccCollectionAllotment.FirstOrDefault().Start, collection.AccCollectionAllotment.FirstOrDefault().End, collection.AccCollection.StoreUID);
                        }
                        else
                        {
                            //eligiblerecords = await CheckEligibleForDiscount(collection.StoreUID);
                            accCollectionPayable = await GetInvoicesAutoAllocate(collection.AccCollection.StoreUID);
                        }
                        enteredAmount = collection.AccCollection.DefaultCurrencyAmount + collection.AccCollection.DiscountAmount;
                        foreach (var list in accCollectionPayable)
                        {
                            arrayAmount = list.BalanceAmount;

                            //if (list.Amount == collection.DefaultCurrencyAmount && eligiblerecords.Any())
                            //{
                            //    ApplyDiscountIfEligible(eligiblerecords, accCollectionPayable, enteredAmount, arrayAmount);
                            //}
                            if (list.BalanceAmount != 0)
                            {

                                if (enteredAmount > arrayAmount)
                                {
                                    balanceAmount = enteredAmount - arrayAmount;
                                    IAccPayable accPayable = await GetAccPayableAmount(list.StoreUID, list.ReferenceNumber);
                                    if (accPayable == null)
                                    {
                                        accPayable = new Winit.Modules.CollectionModule.Model.Classes.AccPayable();
                                    }
                                    var sql2 = @"INSERT INTO acc_collection_allotment (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, acc_collection_uid
                                    , target_type, target_uid, reference_number, currency_uid, default_currency_uid, default_currency_exchange_rate, default_currency_amount, 
                                    early_payment_discount_percentage, early_payment_discount_amount, early_payment_discount_reference_no, amount)

                                    VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @AccCollectionUID,
                                    @TargetType, @TargetUID, @ReferenceNumber, @CurrencyUID, @DefaultCurrencyUID, @DefaultCurrencyExchangeRate, @DefaultCurrencyAmount, @EarlyPaymentDiscountPercentage,
                                    @EarlyPaymentDiscountAmount ,@EarlyPaymentDiscountReferenceNo, @Amount)";
                                    Dictionary<string, object> parameters2 = new Dictionary<string, object>
                                    {
                                        {"AccCollectionUID", collection.AccCollection.UID},
                                        {"TargetType", list.SourceType.Contains("INVOICE") ? "INVOICE" : string.Empty},
                                        {"TargetUID", accPayable.UID == null ?string.Empty: accPayable.UID},
                                        {"ReferenceNumber",  list.ReferenceNumber == null ?string.Empty: list.ReferenceNumber},
                                        {"CurrencyUID", collection.AccCollection.CurrencyUID==null ?string.Empty: collection.AccCollection.CurrencyUID},
                                        {"DefaultCurrencyUID", collection.AccCollection.DefaultCurrencyUID==null ?string.Empty: collection.AccCollection.DefaultCurrencyUID},
                                        {"DefaultCurrencyExchangeRate", collection.AccCollection.DefaultCurrencyExchangeRate==0 ? 0 : collection.AccCollection.DefaultCurrencyExchangeRate},
                                        {"DefaultCurrencyAmount", collection.AccCollection.DefaultCurrencyAmount==0 ? 0 : collection.AccCollection.DefaultCurrencyAmount},
                                        {"EarlyPaymentDiscountPercentage",collection.AccCollectionAllotment.Where(p => p.ReferenceNumber == list.ReferenceNumber).Select(p => p.EarlyPaymentDiscountPercentage).FirstOrDefault()},
                                        {"EarlyPaymentDiscountAmount",collection.AccCollectionAllotment.Where(p => p.ReferenceNumber == list.ReferenceNumber).Select(p => p.DiscountAmount).FirstOrDefault() },
                                        {"Amount", arrayAmount - collection.AccCollectionAllotment.Where(p => p.ReferenceNumber == list.ReferenceNumber).Select(p => p.DiscountAmount).FirstOrDefault()},
                                        {"EarlyPaymentDiscountReferenceNo",  collection.AccCollectionAllotment.Where(p => p.ReferenceNumber == list.ReferenceNumber).Select(p => p.DiscountAmount).FirstOrDefault() > 0 ? "CREDITNOTE-"+CreditID() :  string.Empty},
                                        {"CreatedTime", DateTime.Now },
                                        {"ModifiedTime", DateTime.Now},
                                        {"ModifiedBy", string.IsNullOrEmpty(collection.AccCollection.ModifiedBy) ? string.Empty : collection.AccCollection.ModifiedBy},
                                        {"ServerAddTime", DateTime.Now },
                                        {"ServerModifiedTime", DateTime.Now },
                                        {"CreatedBy", string.IsNullOrEmpty(collection.AccCollection.CreatedBy) ?string.Empty: collection.AccCollection.CreatedBy },
                                        {"UID", (Guid.NewGuid()).ToString()}
                                    };
                                    var accAllot = await ExecuteNonQueryAsync(sql2, conn, parameters2, transaction);

                                    var sql4 = @"UPDATE acc_payable
                                                SET modified_time = @ModifiedTime,
                                                    server_modified_time = @ServerModifiedTime,
                                                    paid_amount = @PaidAmount 
                                                WHERE uid = @UID;";
                                    var sql1 = @"UPDATE acc_payable
                                                SET modified_time = @ModifiedTime,
                                                    server_modified_time = @ServerModifiedTime,
                                                    unsettled_amount = @UnSettledAmount
                                                WHERE uid = @UID;";
                                    Dictionary<string, object> parameters4 = new Dictionary<string, object>
                                    {
                                        {"PaidAmount", accPayable.PaidAmount + arrayAmount},
                                        {"ModifiedTime", DateTime.Now},
                                        {"ServerModifiedTime", DateTime.Now},
                                        {"UID", accPayable.UID==null ?string.Empty:accPayable.UID}
                                    };
                                    Dictionary<string, object> parameters1 = new Dictionary<string, object>
                                    {
                                        {"UnSettledAmount", accPayable.UnSettledAmount + arrayAmount},
                                        {"ModifiedTime", DateTime.Now},
                                        {"ServerModifiedTime", DateTime.Now},
                                        {"UID", accPayable.UID==null?string.Empty:accPayable.UID}
                                    };

                                    var accPay = collection.AccCollection.Category == Const.Cash ? await ExecuteNonQueryAsync(sql4, conn, parameters4, transaction) : await ExecuteNonQueryAsync(sql1, conn, parameters1, transaction);

                                    if (accStore != Const.One || accPaymode != Const.One || accCollec != Const.One || accAllot != Const.One || accPay != Const.One)
                                    {
                                        transaction.Rollback();
                                        return 0;
                                    }
                                    enteredAmount = balanceAmount;
                                    arrayAmount = 0;
                                }
                                else
                                {
                                    if (enteredAmount == 0)
                                    {
                                        break;
                                    }
                                    IAccPayable accPayable = await GetAccPayableAmount(list.StoreUID, list.ReferenceNumber);
                                    if (accPayable == null)
                                    {
                                        accPayable = new Winit.Modules.CollectionModule.Model.Classes.AccPayable();
                                    }
                                    var sql2 = @"INSERT INTO acc_collection_allotment (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                                    acc_collection_uid, target_type, target_uid, reference_number, currency_uid, default_currency_uid, default_currency_exchange_rate, default_currency_amount,
                                    early_payment_discount_percentage,early_payment_discount_amount, early_payment_discount_reference_no, amount)
)
                                    VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @AccCollectionUID,
                                    @TargetType, @TargetUID, @ReferenceNumber, @CurrencyUID, @DefaultCurrencyUID, @DefaultCurrencyExchangeRate, @DefaultCurrencyAmount, @EarlyPaymentDiscountPercentage,
                                    @EarlyPaymentDiscountAmount, @EarlyPaymentDiscountReferenceNo, @Amount)";
                                    Dictionary<string, object> parameters2 = new Dictionary<string, object>
                                    {
                                        {"Acccollection.AccCollectionUID", collection.AccCollection.UID},
                                        {"TargetType", list.SourceType.Contains("INVOICE") ? "INVOICE" : string.Empty},
                                        {"TargetUID", accPayable.UID == null ?string.Empty: accPayable.UID},
                                        {"ReferenceNumber",  list.ReferenceNumber == null ?string.Empty: list.ReferenceNumber},
                                        {"CurrencyUID", collection.AccCollection.CurrencyUID==null ?string.Empty: collection.AccCollection.CurrencyUID},
                                        {"DefaultCurrencyUID", collection.AccCollection.DefaultCurrencyUID==null ?string.Empty: collection.AccCollection.DefaultCurrencyUID},
                                        {"DefaultCurrencyExchangeRate", collection.AccCollection.DefaultCurrencyExchangeRate==0 ? 0 : collection.AccCollection.DefaultCurrencyExchangeRate},
                                        {"DefaultCurrencyAmount", collection.AccCollection.DefaultCurrencyAmount==0 ? 0 : collection.AccCollection.DefaultCurrencyAmount},
                                        {"EarlyPaymentDiscountPercentage",collection.AccCollectionAllotment.Where(p => p.ReferenceNumber == list.ReferenceNumber).Select(p => p.EarlyPaymentDiscountPercentage).FirstOrDefault()},
                                        {"EarlyPaymentDiscountAmount",collection.AccCollectionAllotment.Where(p => p.ReferenceNumber == list.ReferenceNumber).Select(p => p.DiscountAmount).FirstOrDefault()  },
                                        {"Amount", enteredAmount - collection.AccCollectionAllotment.Where(p => p.ReferenceNumber == list.ReferenceNumber).Select(p => p.DiscountAmount).FirstOrDefault()},
                                        {"EarlyPaymentDiscountReferenceNo",  collection.AccCollectionAllotment.Where(p => p.ReferenceNumber == list.ReferenceNumber).Select(p => p.DiscountAmount).FirstOrDefault() > 0 ? "CREDITNOTE-"+CreditID() : string.Empty },
                                        {"CreatedTime", DateTime.Now },
                                        {"ModifiedTime", DateTime.Now},
                                        {"ModifiedBy", string.IsNullOrEmpty(collection.AccCollection.ModifiedBy) ? string.Empty : collection.AccCollection.ModifiedBy },
                                        {"ServerAddTime", DateTime.Now },
                                        {"ServerModifiedTime", DateTime.Now },
                                        {"CreatedBy", string.IsNullOrEmpty(collection.AccCollection.CreatedBy) ?string.Empty: collection.AccCollection.CreatedBy },
                                        {"UID", (Guid.NewGuid()).ToString()}
                                    };
                                    var allot = await ExecuteNonQueryAsync(sql2, conn, parameters2, transaction);
                                    if (collection.AccCollectionAllotment.Where(p => p.ReferenceNumber == list.ReferenceNumber).Select(p => p.EarlyPaymentDiscountPercentage).FirstOrDefault() != 0)
                                    {
                                        if (collection.AccCollection.Category == "Cash")
                                        {
                                            var Receive = await InsertAccReceivable(conn, transaction, collection);
                                        }

                                        var accEarly = await InsertEarlyPaymentDiscountAppliedDetails(conn, transaction, collection);
                                    }
                                    var sql4 = @"UPDATE acc_payable
                                    SET modified_time = @ModifiedTime,
                                        server_modified_time = @ServerModifiedTime,
                                        paid_amount = @PaidAmount 
                                    WHERE uid = @UID;";
                                    var sql1 = @"UPDATE acc_payable
                                    SET modified_time = @ModifiedTime,
                                        server_modified_time = @ServerModifiedTime,
                                        unsettled_amount = @UnSettledAmount
                                    WHERE uid = @UID;";
                                    Dictionary<string, object> parameters4 = new Dictionary<string, object>
                                    {
                                        {"PaidAmount", accPayable.PaidAmount + enteredAmount },
                                        {"ModifiedTime", DateTime.Now},
                                        {"ServerModifiedTime", DateTime.Now},
                                        {"UID", accPayable.UID==null ?string.Empty:accPayable.UID}
                                    };
                                    Dictionary<string, object> parameters1 = new Dictionary<string, object>
                                    {
                                        {"UnSettledAmount", accPayable.UnSettledAmount + enteredAmount },
                                        {"ModifiedTime", DateTime.Now},
                                        {"ServerModifiedTime", DateTime.Now},
                                        {"UID", accPayable.UID==null?string.Empty:accPayable.UID}
                                    };

                                    var accPay = collection.AccCollection.Category == Const.Cash ? await ExecuteNonQueryAsync(sql4, conn, parameters4, transaction) : await ExecuteNonQueryAsync(sql1, conn, parameters1, transaction);

                                    if (accStore != Const.One || accPaymode != Const.One || accCollec != Const.One || allot != Const.One || accPay != Const.One)
                                    {
                                        transaction.Rollback();
                                        return 0;
                                    }
                                    arrayAmount = arrayAmount - enteredAmount;
                                    enteredAmount = 0;
                                }
                            }


                        }
                        transaction.Commit();
                    }
                }
                return 1;
            }
        }
        public async Task<string> CreateOnAccountReceipt(Model.Interfaces.ICollections collection)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        if (collection != null)
                        {
                            if (collection.AccCollection.UID != null)
                            {
                                var accCollec = await InsertAccCollection(conn, transaction, collection);

                                var accPaymode = await InsertAccCollectionPaymentMode(conn, transaction, collection);

                                var accStore = await InsertAccStoreLedger(conn, transaction, collection);

                                var accAllot = await InsertAccCollectionAllotment(conn, transaction, collection);

                                var accCollectionCurrencyData = await InsertAccCollectionCurrencyDetails(conn, transaction, collection);
                                if (collection.AccCollection.Category == "Cash" || collection.AccCollection.Category == "OnAccount")
                                {
                                    var Retval4 = string.Empty;
                                    var sql4 = @"INSERT INTO public.acc_receivable (
                                            uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                                            source_type, source_uid, reference_number, org_uid, job_position_uid, currency_uid,
                                            amount, paid_amount, store_uid, transaction_date, due_date, source)

	                                    VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,
                                         @SourceType, @SourceUID, @ReferenceNumber, @OrgUID, @JobPositionUID, @CurrencyUID, @Amount, @PaidAmount, 
                                         @StoreUID, @TransactionDate, @DueDate, @Source);";
                                    Dictionary<string, object> parameters4 = new Dictionary<string, object>
                                        {
                                            {"UID", collection.AccReceivable.FirstOrDefault().UID},
                                            {"CreatedTime", DateTime.Now },
                                            {"ModifiedTime", DateTime.Now},
                                            {"ModifiedBy", string.IsNullOrEmpty(collection.AccCollection.ModifiedBy) ?string.Empty: collection.AccCollection.ModifiedBy },
                                            {"ServerAddTime", DateTime.Now },
                                            {"ServerModifiedTime", DateTime.Now },
                                            {"CreatedBy", string.IsNullOrEmpty(collection.AccCollection.CreatedBy) ?string.Empty: collection.AccCollection.CreatedBy },
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
                                            {"Amount", collection.AccCollection.DefaultCurrencyAmount == 0 ? 0 : collection.AccCollection.DefaultCurrencyAmount},
                                            {"PaidAmount", 0}
                                        };
                                    Retval4 = await ExecuteNonQueryAsync(sql4, conn, parameters4, transaction);
                                }

                                if (accCollec != Const.One || accPaymode != Const.One || accStore != Const.One || accAllot != Const.One)
                                {
                                    transaction.Rollback();
                                    return "Failed";
                                }
                                else
                                {
                                    transaction.Commit();
                                    return Const.SuccessInsert;
                                }
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
            catch (Exception ex)
            {
                throw new Exception();
            }
        }
        public async Task<string> CashCollectionSettlement(string collection)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    if (collection != null)
                    {
                        decimal sum = 0;
                        var Retval = string.Empty;
                        IAccCollection accCollection = await GetAmountCash(collection);
                        IEnumerable<IAccCollectionAllotment> accCollectionAllotments = await GetCashDetails(collection);
                        var sql = @"INSERT INTO Acc_Collection_Settlement (uid, acc_collection_uid, settled_by,
                                   created_by, created_time, modified_by, modified_time, server_add_time, 
                            server_modified_time,  payment_mode, received_amount, receipt_number)
                                      VALUES(@UID, @AccCollectionUID, @SettledBy,  
                                      @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                                 @PaymentMode, @ReceivedAmount, @ReceiptNumber)";
                        Dictionary<string, object> parameters = new Dictionary<string, object>
                            {
                               {"AccCollectionUID",accCollection.UID==null?string.Empty:accCollection.UID},
                               {"UID",(Guid.NewGuid()).ToString()},
                               {"SettledBy", accCollection.ModifiedBy==null?string.Empty:accCollection.ModifiedBy},
                               {"ReceivedAmount",accCollection.DefaultCurrencyAmount==0?0:accCollection.DefaultCurrencyAmount},
                               {"PaymentMode",accCollection.Category==null?string.Empty:accCollection.Category},
                               {"ReceiptNumber",accCollection.ReceiptNumber==null?string.Empty:accCollection.ReceiptNumber},
                               {"CreatedTime", DateTime.Now},
                               {"ModifiedTime", DateTime.Now},
                               {"ModifiedBy", string.IsNullOrEmpty(accCollection.CreatedBy) ?string.Empty: accCollection.CreatedBy},
                               {"ServerAddTime", DateTime.Now},
                               {"ServerModifiedTime", DateTime.Now},
                               {"CreatedBy", string.IsNullOrEmpty(accCollection.CreatedBy) ?string.Empty: accCollection.CreatedBy},
                            };
                        Retval = await ExecuteNonQueryAsync(sql, conn, parameters, transaction);
                        if (Retval != Const.One)
                        {
                            return Retval;
                        }
                        var res = await UpdateCashCollection(collection, accCollection.SessionUserCode, conn, transaction);
                        if (res != Const.One)
                        {
                            return res;
                        }
                        foreach (var list in accCollectionAllotments)
                        {
                            var Retval1 = string.Empty;
                            var sql1 = @"INSERT INTO Acc_Collection_Settlement_Receipts(uid, created_by, created_time, modified_by, modified_time, 
                                        server_add_time, server_modified_time, receipt_number,
                                        target_type, target_uid, paid_amount)
                                        VALUES(@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime
                                        , @ReceiptNumber, @TargetType, @TargetUID, @PaidAmount)";
                            Dictionary<string, object> parameters1 = new Dictionary<string, object>
                        {
                            {"CreatedTime",DateTime.Now},
                            {"ModifiedTime", DateTime.Now},
                            {"ModifiedBy", string.IsNullOrEmpty(accCollection.ModifiedBy) ?string.Empty: accCollection.ModifiedBy},
                            {"ServerAddTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now},
                            {"CreatedBy", string.IsNullOrEmpty(accCollection.CreatedBy) ?string.Empty: accCollection.CreatedBy},
                            {"UID", (Guid.NewGuid()).ToString()},
                            {"ReceiptNumber", accCollection.ReceiptNumber==null?string.Empty:accCollection.ReceiptNumber},
                            {"PaidAmount", list.DefaultCurrencyAmount==0?0:list.DefaultCurrencyAmount},
                            {"TargetType", list.TargetType==null?string.Empty:list.TargetType},
                            {"TargetUID", list.TargetUID==null?string.Empty:list.TargetUID}
                        };
                            Retval1 = await ExecuteNonQueryAsync(sql1, conn, parameters1, transaction);
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
        public async Task<string> CreateCollectionSettlementByCashier(IAccCollectionSettlement collection)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    if (collection != null)
                    {
                        if (collection.Receipts != null)
                        {
                            var Retval = string.Empty;
                            var sql = @"INSERT INTO Acc_Collection_Settlement (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                                      acc_collection_uid, collection_amount, received_amount, has_discrepancy, discrepancy_amount, default_currency_uid,
                                      settlement_date, cashier_job_position_uid, cashier_emp_uid, session_user_code)
                                        VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @AccCollectionUID, @CollectionAmount,
                                        @ReceivedAmount, @HasDiscrepancy, @DiscrepancyAmount, @DefaultCurrencyUID, @SettlementDate, @CashierJobPositionUID, @CashierEmpUID, @SessionUserCode)";
                            Dictionary<string, object> parameters = new Dictionary<string, object>
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
                        {"ModifiedBy", collection.ModifiedBy ?? ""},
                        {"ServerAddTime", collection.ServerAddTime},
                        {"ServerModifiedTime", collection.ServerModifiedTime},
                        {"CreatedBy", collection.CreatedBy ?? ""},
                        {"SessionUserCode", collection.SessionUserCode}
                };
                            Retval = await ExecuteNonQueryAsync(sql, conn, parameters, transaction);
                            if (Retval != Const.One)
                            {
                                return Retval;
                            }
                            foreach (var receipt in collection.Receipts)
                            {
                                var sql1 = @"INSERT INTO Acc_Collection_Settlement_Receipts (uid, created_by, created_time, modified_by, modified_time, server_add_time,
                                server_modified_time, receipt_number, settled_amount, acc_collection_settlement_uid, session_user_code)
                                 VALUES (@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,
                                @ServerAddTime,@ServerModifiedTime,@ReceiptNumber,@SettledAmount,@AccCollectionSettlementUID, @SessionUserCode);";
                                Dictionary<string, object> parameters1 = new Dictionary<string, object>
                    {
                        {"UID", receipt.UID},
                        {"ReceiptNumber", receipt.ReceiptNumber},
                        {"SettledAmount", receipt.SettledAmount},
                        {"AccCollectionSettlementUID", receipt.AccCollectionSettlementUID},
                        {"CreatedTime", receipt.CreatedTime},
                        {"ModifiedTime", receipt.ModifiedTime},
                        {"ModifiedBy",collection.ModifiedBy ?? ""},
                        {"ServerAddTime", receipt.ServerAddTime},
                        {"ServerModifiedTime", receipt.ServerModifiedTime},
                        {"CreatedBy", collection.CreatedBy ?? ""},
                        {"SessionUserCode", collection.SessionUserCode}
                    };
                                Retval = await ExecuteNonQueryAsync(sql1, conn, parameters1, transaction);
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
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    if (ReceiptNumber != null && ReceiptNumber != string.Empty)
                    {
                        var exist = await CheckExists(ChequeNo);
                        bool Exist = exist == "false" ? false : true;
                        if (!Exist)
                        {
                            IAccCollection accCollection = await GetAmountCash(ReceiptNumber);
                            var tar = await IsReversal2(accCollection.UID, accCollection.ReceiptNumber, ReasonforCancelation, conn, transaction, "Void");
                            var Retval = string.Empty;
                            var sql = @"UPDATE Acc_Collection
                                    SET modified_by = @ModifiedBy, modified_time = @ModifiedTime,
                                        server_modified_time = @ServerModifiedTime,
                                        amount = @Amount, cancelled_on = @CancelledOn, comments = @Comments 
                                    WHERE uid = @UID;";
                            Dictionary<string, object> parameters = new Dictionary<string, object>
                        {
                            {"UID", accCollection.UID},
                            {"Amount",  accCollection.Amount *0 !=0 ? 0 : accCollection.Amount *0},
                            {"CancelledOn", DateTime.Now},
                            {"Comments",  ReasonforCancelation},
                            {"ModifiedTime", DateTime.Now},
                            {"ModifiedBy", accCollection.ModifiedBy?? ""},
                            {"ServerModifiedTime", DateTime.Now}
                        };
                            Retval = await ExecuteNonQueryAsync(sql, conn, parameters, transaction);
                            if (Retval != Const.One)
                            {
                                return Retval;
                            }
                            IAccCollectionPaymentMode accCollectionPayment = await GetPaymentAmount(accCollection.UID, conn, transaction);
                            var sql1 = @"UPDATE Acc_Collection_Payment_Mode
                                        SET modified_by = @ModifiedBy, modified_time = @ModifiedTime,
                                            server_modified_time = @ServerModifiedTime, amount = @Amount,
                                            status = @Status 
                                        WHERE uid = @UID;";
                            Dictionary<string, object> parameters1 = new Dictionary<string, object>
                        {
                            {"Amount", accCollectionPayment.Amount *0 !=0 ? 0 : accCollectionPayment.Amount *0},
                            {"Status", accCollectionPayment.Status== null ?string.Empty: "Voided"},
                            {"ModifiedTime", DateTime.Now},
                            {"ModifiedBy", accCollection.ModifiedBy??""},
                            {"ServerModifiedTime", DateTime.Now},
                            {"UID",accCollectionPayment.UID == null ?string.Empty: accCollectionPayment.UID}
                        };
                            Retval = await ExecuteNonQueryAsync(sql1, conn, parameters1, transaction);
                            if (Retval != Const.One)
                            {
                                return Retval;
                            }
                            IEnumerable<IAccCollectionAllotment> allot = await GetCashAllot(accCollection.UID, conn, transaction);
                            IAccStoreLedger accStoreLedger = await GetStoreLedger(accCollection.UID, conn, transaction);
                            var sql3 = @"UPDATE Acc_Store_Ledger
                                        SET modified_by = @ModifiedBy,
                                            server_modified_time = @ServerModifiedTime,
                                            collected_amount = @CollectedAmount, amount = @Amount, comments = @Comments 
                                        WHERE id = @ID;
                                        ";
                            Dictionary<string, object> parameters3 = new Dictionary<string, object>
                        {
                            {"CollectedAmount", accStoreLedger.Amount * 0},
                            {"Amount", accStoreLedger.Amount==0 ? 0 : accStoreLedger.Amount * 0},
                            {"ModifiedBy", accCollection.ModifiedBy??""},
                            {"Comments", "Voided Successfully"},
                            {"ServerModifiedTime", DateTime.Now},
                            {"ID", accStoreLedger.ID==0? 0 : accStoreLedger.ID}
                        };
                            Retval = await ExecuteNonQueryAsync(sql3, conn, parameters3, transaction);
                            if (Retval != Const.One)
                            {
                                return Retval;
                            }
                            //var Res = await UpdateReverseCashCollection(ChequeNo, conn, transaction);
                            foreach (var accCollectionAllotment in allot)
                            {
                                //await UpdateAllot(accCollectionAllotment.AccCollectionUID, accCollectionAllotment.PaidAmount, accCollectionAllotment.TargetUID, conn, transaction);
                                var sql2 = @"UPDATE Acc_Collection_Allotment
                                            SET modified_by = @ModifiedBy, modified_time = @ModifiedTime,
                                                amount = @Amount, early_payment_discount_amount = @EarlyPaymentDiscountAmount, server_modified_time = @ServerModifiedTime 
                                            WHERE uid = @UID;";
                                Dictionary<string, object> parameters2 = new Dictionary<string, object>
                            {
                                {"Amount", accCollectionAllotment.Amount * 0},
                                {"EarlyPaymentDiscountAmount", accCollectionAllotment.EarlyPaymentDiscountAmount * 0},
                                { "ModifiedTime", DateTime.Now},
                                { "ModifiedBy", accCollection.ModifiedBy??""},
                                { "ServerModifiedTime", DateTime.Now},
                                { "UID", accCollectionAllotment.UID==null ?string.Empty: accCollectionAllotment.UID}
                            };
                                Retval = await ExecuteNonQueryAsync(sql2, conn, parameters2, transaction);
                                if (allot.Any(p => p.TargetType == "OA - CREDITNOTE" || p.EarlyPaymentDiscountAmount != 0))
                                {
                                    var del = await DeleteCreditNote(accCollection.UID, conn, transaction);
                                }
                                if (Retval != Const.One)
                                {
                                    return Retval;
                                }
                                if (accCollectionAllotment.TargetType.Contains("INVOICE"))
                                {
                                    IEnumerable<IAccPayable> payb = await GetChequePay(accCollectionAllotment.TargetUID, conn, transaction);
                                    foreach (var list1 in payb)
                                    {
                                        var sql4 = @"UPDATE Acc_Payable
                                                SET modified_time = @ModifiedTime,
                                                    server_modified_time = @ServerModifiedTime, paid_amount = @PaidAmount 
                                                WHERE uid = @UID;";
                                        Dictionary<string, object> parameters4 = new Dictionary<string, object>
                                        {
                                            {"PaidAmount", accCollectionAllotment.PaidAmount*0 == 0 ? list1.PaidAmount - (accCollectionAllotment.Amount + accCollectionAllotment.EarlyPaymentDiscountAmount) :  0  },
                                        {"ModifiedTime", DateTime.Now},
                                        {"ServerModifiedTime", DateTime.Now},
                                        {"UID", list1.UID}
                                };
                                        Retval = await ExecuteNonQueryAsync(sql4, conn, parameters4, transaction);
                                        if (Retval != Const.One)
                                        {
                                            return Retval;
                                        }
                                    }
                                }
                                else
                                {
                                    IEnumerable<IAccReceivable> recv = await GetChequeRec(accCollectionAllotment.TargetUID, conn, transaction);
                                    foreach (var list1 in recv)
                                    {
                                        var sql5 = @"UPDATE Acc_Receivable
                                                SET modified_time = @ModifiedTime,
                                                    server_modified_time = @ServerModifiedTime, paid_amount = @PaidAmount
                                                WHERE uid = @UID;";
                                        Dictionary<string, object> parameters5 = new Dictionary<string, object>
                                        {
                                            {"PaidAmount", accCollectionAllotment.PaidAmount*0 == 0 ? list1.PaidAmount - accCollectionAllotment.Amount :  0},
                                            {"ModifiedTime",DateTime.Now},
                                            {"ServerModifiedTime", DateTime.Now},
                                            {"UID",  list1.UID}
                                        };
                                        Retval = await ExecuteNonQueryAsync(sql5, conn, parameters5, transaction);
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
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    IAccCollection accCollection1 = await GetAmountCash(ReceiptNumber);
                    ReceiptNumber = (ReceiptNumber == string.Empty || ReceiptNumber == null) ? accCollection1.UID : ReceiptNumber;
                    if (ReceiptNumber != null && ReceiptNumber != string.Empty)
                    {
                        //var exist = await CheckExists(ChequeNo);
                        bool Exist = true;
                        if (Exist)
                        {
                            var tar = await IsReversal2(accCollection1.UID, accCollection1.ReceiptNumber, ReasonforCancelation, conn, transaction);
                            IAccCollectionPaymentMode accCollectionPayment = await GetPaymentAmount(ReceiptNumber, conn, transaction);
                            IAccStoreLedger accStoreLedger = await GetStoreLedgerCreation(accCollection1.StoreUID, conn, transaction);
                            IEnumerable<IAccCollectionAllotment> allot = await GetCashAllot(accCollection1.UID, conn, transaction);

                            var Retval = string.Empty;
                            var sql = @"INSERT INTO public.acc_collection(uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, receipt_number, consolidated_receipt_number, 
                        category, amount, currency_uid, default_currency_uid, default_currency_exchange_rate, default_currency_amount, org_uid, distribution_channel_uid, store_uid, route_uid, job_position_uid,
                        emp_uid, collected_date, status, remarks, reference_number, is_realized, latitude, longitude, source, is_multimode, trip_date, comments, salesman, route, reversal_receipt_uid, cancelled_on, ss)
                        VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @ReceiptNumber, @ConsolidatedReceiptNumber, @Category, @Amount, @CurrencyUID, @DefaultCurrencyUID, @DefaultCurrencyExchangeRate,
                                @DefaultCurrencyAmount, @OrgUID, @DistributionChannelUID, @StoreUID, @RouteUID, @JobPositionUID, @EmpUID, @CollectedDate, @Status, @Remarks, @ReferenceNumber, @IsRealized, @Latitude, @Longitude, @Source, @IsMultimode, @TripDate,
                                @Comments, @Salesman, @Route, @ReversalReceiptUID, @CancelledOn, @SS);";
                            Dictionary<string, object> parameters = new Dictionary<string, object>
                                {
                                   {"UID", string.IsNullOrEmpty(accCollection1.UID) ?string.Empty: "R - " + accCollection1.UID},
                                   {"ReceiptNumber", string.IsNullOrEmpty(accCollection1.ReceiptNumber) ?string.Empty: "R - " + accCollection1.ReceiptNumber},
                                   {"ConsolidatedReceiptNumber", string.IsNullOrEmpty(accCollection1.ConsolidatedReceiptNumber) ?string.Empty: "R - " + accCollection1.ConsolidatedReceiptNumber},
                                   {"Category", string.IsNullOrEmpty(accCollection1.Category) ?string.Empty: accCollection1.Category},
                                   {"Amount", accCollection1.Amount * Const.Num },
                                   {"CurrencyUID", string.IsNullOrEmpty(accCollection1.CurrencyUID)?string.Empty:accCollection1.CurrencyUID},
                                   {"DefaultCurrencyUID", string.IsNullOrEmpty(accCollection1.DefaultCurrencyUID)?string.Empty:accCollection1.DefaultCurrencyUID},
                                   {"DefaultCurrencyExchangeRate", accCollection1.DefaultCurrencyExchangeRate==0?0:accCollection1.DefaultCurrencyExchangeRate},
                                   {"DefaultCurrencyAmount", accCollection1.DefaultCurrencyAmount==0?0:accCollection1.DefaultCurrencyAmount},
                                   {"OrgUID", string.IsNullOrEmpty(accCollection1.OrgUID)?string.Empty:accCollection1.OrgUID},
                                   {"DistributionChannelUID", string.IsNullOrEmpty(accCollection1.DistributionChannelUID)?string.Empty:accCollection1.DistributionChannelUID},
                                   {"StoreUID", string.IsNullOrEmpty(accCollection1.StoreUID)?string.Empty:accCollection1.StoreUID},
                                   {"RouteUID", accCollection1.RouteUID},
                                   {"JobPositionUID", string.IsNullOrEmpty(accCollection1.JobPositionUID)?string.Empty:accCollection1.JobPositionUID},
                                   {"EmpUID", string.IsNullOrEmpty(accCollection1.EmpUID)?string.Empty:accCollection1.EmpUID},
                                   {"CollectedDate", accCollection1.CollectedDate==null?DateTime.Now:accCollection1.CollectedDate},
                                   {"Status", "Reversed"},
                                   {"Remarks", string.IsNullOrEmpty(accCollection1.Remarks)?string.Empty:accCollection1.Remarks},
                                   {"ReferenceNumber", string.IsNullOrEmpty(accCollection1.ReferenceNumber)?string.Empty:accCollection1.ReferenceNumber},
                                   {"IsRealized", false},
                                   {"Latitude", string.IsNullOrEmpty(accCollection1.Latitude)?string.Empty:accCollection1.Latitude},
                                   {"Longitude", string.IsNullOrEmpty(accCollection1.Longitude)?string.Empty:accCollection1.Longitude},
                                   {"Source", string.IsNullOrEmpty(accCollection1.Source)?string.Empty:accCollection1.Source},
                                   {"IsMultimode", accCollection1.IsMultimode==false?false:accCollection1.IsMultimode},
                                   {"TripDate", accCollection1.TripDate==null ? DateTime.Now : accCollection1.TripDate},
                                   {"Comments", ReasonforCancelation},
                                   {"Salesman", string.IsNullOrEmpty(accCollection1.Salesman) ?string.Empty: accCollection1.Salesman},
                                   {"Route", string.IsNullOrEmpty(accCollection1.Route) ?string.Empty: accCollection1.Route},
                                   {"ReversalReceiptUID", string.IsNullOrEmpty(accCollection1.ReceiptNumber) ?string.Empty: accCollection1.ReceiptNumber},
                                   {"SS", 0},
                                   {"CancelledOn", DateTime.Now},
                                   {"CreatedTime", DateTime.Now},
                                   {"ModifiedTime", DateTime.Now},
                                   {"ModifiedBy", string.IsNullOrEmpty(accCollection1.ModifiedBy) ?string.Empty: accCollection1.ModifiedBy},
                                   {"ServerAddTime", DateTime.Now},
                                   {"ServerModifiedTime", DateTime.Now},
                                   {"CreatedBy", string.IsNullOrEmpty(accCollection1.CreatedBy) ?string.Empty: accCollection1.CreatedBy}
                                };
                            Retval = await ExecuteNonQueryAsync(sql, conn, parameters, transaction);
                            if (Retval != Const.One)
                            {
                                return Retval;
                            }
                            //await UpdateCollectionPayment(accCollectionPayment.UID, Amount);
                            var sql1 = @"INSERT INTO acc_collection_payment_mode (uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                            server_modified_time, acc_collection_uid, bank_uid, branch, cheque_no, amount, currency_uid, default_currency_uid, 
                            default_currency_exchange_rate, default_currency_amount, cheque_date, status, realization_date, ss)
                                VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                                @AccCollectionUID, @BankUID, @Branch, @ChequeNo, @Amount, @CurrencyUID, @DefaultCurrencyUID, @DefaultCurrencyExchangeRate, 
                                @DefaultCurrencyAmount, @ChequeDate, @Status, @RealizationDate, @SS)";
                            Dictionary<string, object> parameters1 = new Dictionary<string, object>
                        {
                            {"AccCollectionUID", string.IsNullOrEmpty(accCollection1.UID)  ?string.Empty: "R - " + accCollection1.UID},
                            {"BankUID", string.IsNullOrEmpty(accCollectionPayment.BankUID) ?string.Empty: accCollectionPayment.BankUID},
                            {"Branch", string.IsNullOrEmpty(accCollectionPayment.Branch) ?string.Empty: accCollectionPayment.Branch},
                            {"ChequeNo", string.IsNullOrEmpty(accCollectionPayment.ChequeNo) ?string.Empty: accCollectionPayment.ChequeNo},
                            {"CurrencyUID", string.IsNullOrEmpty(accCollection1.CurrencyUID) ?string.Empty: accCollection1.CurrencyUID},
                            {"DefaultCurrencyUID", string.IsNullOrEmpty(accCollection1.DefaultCurrencyUID) ?string.Empty: accCollection1.DefaultCurrencyUID},
                            {"DefaultCurrencyExchangeRate", accCollection1.DefaultCurrencyExchangeRate==0?0:accCollection1.DefaultCurrencyExchangeRate},
                            {"DefaultCurrencyAmount", accCollection1.DefaultCurrencyAmount==0?0:accCollection1.DefaultCurrencyAmount},
                            {"ChequeDate", accCollectionPayment.ChequeDate==null?DateTime.Now:accCollectionPayment.ChequeDate},
                            {"Status", "Reversed"},
                            {"SS", 0},
                            {"RealizationDate", accCollectionPayment.RealizationDate==null?DateTime.Now:accCollectionPayment.RealizationDate},
                            {"CreatedTime",DateTime.Now},
                            {"ModifiedTime", DateTime.Now},
                            {"ModifiedBy", string.IsNullOrEmpty(accCollection1.ModifiedBy) ?string.Empty: accCollection1.ModifiedBy},
                            {"ServerAddTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now},
                            {"CreatedBy", string.IsNullOrEmpty(accCollection1.CreatedBy) ?string.Empty: accCollection1.CreatedBy},
                            {"UID", "R - " + (Guid.NewGuid()).ToString()},
                            {"Amount", accCollectionPayment.Amount * Const.Num}
                        };
                            Retval = await ExecuteNonQueryAsync(sql1, conn, parameters1, transaction);
                            if (Retval != Const.One)
                                return Retval;
                            // await UpdateStoreLed(accStoreLedger.UID, Amount);
                            var sql3 = @"INSERT INTO acc_store_ledger (uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                                server_modified_time, source_type, source_uid, credit_type, org_uid, store_uid, default_currency_uid, document_number,
                                default_currency_exchange_rate, default_currency_amount, amount, transaction_date_time, collected_amount, currency_uid, balance, comments, ss)
                                VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,@ServerAddTime, @ServerModifiedTime, @SourceType, @SourceUID,
                                   @CreditType, @OrgUID, @StoreUID, @DefaultCurrencyUID,@DocumentNumber, @DefaultCurrencyExchangeRate, @DefaultCurrencyAmount,
                                   @Amount, @TransactionDateTime, @CollectedAmount, @CurrencyUID, @Balance, @Comments, @SS)";
                            Dictionary<string, object> parameters3 = new Dictionary<string, object>
                        {
                            {"SourceType", "Collection"},
                            {"SourceUID", accCollection1.UID==null?string.Empty: accCollection1.UID},
                            {"CreditType", "CR"},
                            {"OrgUID", accStoreLedger.OrgUID==null?string.Empty: accStoreLedger.OrgUID},
                            {"StoreUID", accCollection1.StoreUID==null?string.Empty: accCollection1.StoreUID},
                            {"DefaultCurrencyUID", accCollection1.DefaultCurrencyUID==null?string.Empty: accCollection1.DefaultCurrencyUID},
                            {"DocumentNumber", "R - " + accCollection1.ReceiptNumber == null ?string.Empty: "R - " + accCollection1.ReceiptNumber},
                            {"DefaultCurrencyExchangeRate", accCollection1.DefaultCurrencyExchangeRate==0?0: accCollection1.DefaultCurrencyExchangeRate},
                            {"DefaultCurrencyAmount", accCollection1.DefaultCurrencyAmount==0?0: accCollection1.DefaultCurrencyAmount},
                            {"Amount", accCollection1.DefaultCurrencyAmount == 0 ? 0 : accCollection1.DefaultCurrencyAmount * -1},
                            {"Balance", accStoreLedger.Balance == 0 ? (0 - accCollection1.DefaultCurrencyAmount) : (accStoreLedger.Balance - accCollection1.DefaultCurrencyAmount)},
                            {"TransactionDateTime", accStoreLedger.TransactionDateTime == null ? DateTime.Now : DateTime.Now},
                            {"CollectedAmount", (accCollection1.Amount == 0 ? 0 : accCollection1.Amount) *-1},
                            {"CurrencyUID", accCollection1.CurrencyUID ==null ?string.Empty: accCollection1.CurrencyUID},
                            {"SS", 0},
                            {"CreatedTime", DateTime.Now},
                            {"ModifiedTime", DateTime.Now},
                            {"ModifiedBy", string.IsNullOrEmpty(accCollection1.ModifiedBy) ?string.Empty: accCollection1.ModifiedBy},
                            {"ServerAddTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now},
                            {"Comments", "Reversed Successfully"},
                            {"CreatedBy", string.IsNullOrEmpty(accCollection1.CreatedBy) ?string.Empty: accCollection1.CreatedBy},
                            {"UID", "R - " + (Guid.NewGuid()).ToString()}
                        };
                            Retval = await ExecuteNonQueryAsync(sql3, conn, parameters3, transaction);
                            if (Retval != Const.One)
                                return Retval;


                            foreach (var list in allot)
                            {
                                var sql2 = @"INSERT INTO acc_collection_allotment (uid, created_by, created_time, modified_by, modified_time, 
                            server_add_time, server_modified_time, acc_collection_uid, target_type, target_uid, reference_number, currency_uid,
                            default_currency_uid, default_currency_exchange_rate, default_currency_amount, early_payment_discount_percentage, early_payment_discount_amount, 
                            amount, ss)
                                VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @AccCollectionUID,
                                @TargetType, @TargetUID, @ReferenceNumber, @CurrencyUID, @DefaultCurrencyUID, @DefaultCurrencyExchangeRate, @DefaultCurrencyAmount, @EarlyPaymentDiscountPercentage,
                                @EarlyPaymentDiscountAmount, @Amount, @SS)";
                                Dictionary<string, object> parameters2 = new Dictionary<string, object>
                {
                                {"AccCollectionUID", "R - " + accCollection1.UID},
                                {"TargetType", list.TargetType == null ?string.Empty: list.TargetType},
                                {"TargetUID", list.UID == null ?string.Empty: list.UID},
                                {"ReferenceNumber",  list.ReferenceNumber == null ?string.Empty: list.ReferenceNumber},
                                {"CurrencyUID", accCollection1.CurrencyUID==null ?string.Empty: accCollection1.CurrencyUID},
                                {"DefaultCurrencyUID", accCollection1.DefaultCurrencyUID==null ?string.Empty: accCollection1.DefaultCurrencyUID},
                                {"DefaultCurrencyExchangeRate", accCollection1.DefaultCurrencyExchangeRate==0 ? 0 : accCollection1.DefaultCurrencyExchangeRate},
                                {"DefaultCurrencyAmount", accCollection1.DefaultCurrencyAmount==0 ? 0 : accCollection1.DefaultCurrencyAmount},
                                {"EarlyPaymentDiscountPercentage", list.EarlyPaymentDiscountPercentage==0 ? 0 : list.EarlyPaymentDiscountPercentage },
                                {"EarlyPaymentDiscountAmount", list.EarlyPaymentDiscountAmount==0 ? 0 : list.EarlyPaymentDiscountAmount },
                                {"Amount", list.Amount * Const.Num },
                                {"SS", 0 },
                                {"CreatedTime", DateTime.Now },
                                {"ModifiedTime", DateTime.Now},
                                {"ModifiedBy", string.IsNullOrEmpty(accCollection1.ModifiedBy) ?string.Empty: accCollection1.ModifiedBy },
                                {"ServerAddTime", DateTime.Now },
                                {"ServerModifiedTime", DateTime.Now },
                                {"CreatedBy", string.IsNullOrEmpty(accCollection1.CreatedBy) ?string.Empty: accCollection1.CreatedBy} ,
                                {"UID", (Guid.NewGuid()).ToString()}
                            };
                                Retval = await ExecuteNonQueryAsync(sql2, conn, parameters2, transaction);
                                if (allot.Any(p => p.TargetType == "OA - CREDITNOTE" || p.EarlyPaymentDiscountAmount != 0))
                                {
                                    var del = await DeleteCreditNote(accCollection1.UID, conn, transaction);
                                }
                                if (Retval != Const.One)
                                    return Retval;
                                if (list.TargetType.Contains("INVOICE"))
                                {
                                    IEnumerable<IAccPayable> payb = await GetChequePay(list.TargetUID, conn, transaction);
                                    foreach (var list1 in payb)
                                    {
                                        var sql4 = @"UPDATE acc_payable
                                                SET modified_time = @ModifiedTime,
                                                    server_modified_time = @ServerModifiedTime, paid_amount = @PaidAmount 
                                                WHERE uid = @UID;";
                                        Dictionary<string, object> parameters4 = new Dictionary<string, object>
                                    {
                                        {"PaidAmount", list1.PaidAmount+((list.Amount+list.EarlyPaymentDiscountAmount)*Const.Num) == 0 ? 0 : list1.PaidAmount+((list.Amount+list.EarlyPaymentDiscountAmount)*Const.Num)},
                                        {"ModifiedTime", DateTime.Now},
                                        {"ServerModifiedTime", DateTime.Now},
                                        {"UID", list1.UID}
                                };
                                        Retval = await ExecuteNonQueryAsync(sql4, conn, parameters4, transaction);
                                        if (Retval != Const.One)
                                        {
                                            return Retval;
                                        }
                                    }
                                }
                                else
                                {
                                    IEnumerable<IAccReceivable> recv = await GetChequeRec(list.TargetUID, conn, transaction);
                                    foreach (var list1 in recv)
                                    {
                                        var sql5 = @"UPDATE acc_receivable
                                                SET modified_time = @ModifiedTime,
                                                    server_modified_time = @ServerModifiedTime, paid_amount = @PaidAmount
                                                WHERE uid = @UID";
                                        Dictionary<string, object> parameters5 = new Dictionary<string, object>
                                         {
                                             {"PaidAmount", list1.PaidAmount+list.Amount*Const.Num == 0 ? 0 : list1.PaidAmount+list.Amount*Const.Num},
                                             {"ModifiedTime",DateTime.Now},
                                             {"ServerModifiedTime", DateTime.Now},
                                             {"UID",  list1.UID}
                                         };
                                        Retval = await ExecuteNonQueryAsync(sql5, conn, parameters5, transaction);
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
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    var Retval = string.Empty;
                    var sql = @"UPDATE acc_collection_payment_mode 
                            SET created_by = @CreatedBy, created_time = @CreatedTime, modified_by = @ModifiedBy, modified_time = @ModifiedTime, server_add_time = @ServerAddTime,
                                server_modified_time = @ServerModifiedTime, acc_collection_uid = @AccCollectionUID,
                                branch = @Branch, cheque_no = @ChequeNo, cheque_date = @ChequeDate, bank_uid = @BankUID 
                            WHERE uid = @UID";
                    Dictionary<string, object> parameters = new Dictionary<string, object>
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
                            {"CreatedBy", string.IsNullOrEmpty(collection.ModifiedBy) ?string.Empty: collection.ModifiedBy},
                            {"UID", collection.UID}
                    };
                    Retval = await ExecuteNonQueryAsync(sql, conn, parameters, transaction);
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
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    if (UID != null && UID != string.Empty)
                    {
                        if (Button == "Approve")
                        {
                            var Retval = string.Empty;
                            var sql = @"UPDATE acc_collection_payment_mode
                            SET modified_time = @ModifiedTime,
                                server_modified_time = @ServerModifiedTime, comments = @Comments, status = @Status
                            WHERE acc_collection_uid = @UID";
                            Dictionary<string, object> parameters = new Dictionary<string, object>
                    {
                            {"UID", UID},
                            {"Status", "Settled" },
                            {"Comments", Comments },
                            {"ModifiedTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now}
                    };
                            Retval = await ExecuteNonQueryAsync(sql, conn, parameters, transaction);
                            if (Retval != Const.One)
                            {
                                return Retval;
                            }
                            //IEnumerable<ICollections> accCollection = await GetAmountCheque(CashNumber);
                            //IEnumerable<IAccCollectionAllotment> accCollectionAllotments = await GetChequeDetails(CashNumber);
                            //foreach (var list in accCollection)
                            //{
                            //    var sql1 = @"INSERT INTO AccCollectionSettlement (UID, AccCollectionUID,SettledBy,
                            //        CreatedBy, CreatedTime, ModifiedBy, ModifiedTime, ServerAddTime, 
                            //        ServerModifiedTime, SessionUserCode, Route, PaymentMode, ReceivedAmount, ReceiptNumber, IsVoid, CashNumber)
                            //        VALUES(@UID, @listUID, @SettledBy,  
                            //        @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @SessionUserCode,
                            //        @Route, @PaymentMode, @ReceivedAmount, @ReceiptNumber, @IsVoid, @CashNumber)";
                            //    Dictionary<string, object> parameters1 = new Dictionary<string, object>
                            //{
                            //   {"listUID",UID},
                            //   {"UID",(Guid.NewGuid()).ToString()},
                            //   {"SettledBy", list.SessionUserCode==null?:list.SessionUserCode},
                            //   {"ReceivedAmount",list.PaidAmount==0?0:list.PaidAmount},
                            //   {"Route",list.SessionUserCode==null?:list.SessionUserCode},
                            //   {"PaymentMode",list.Category==null?:list.Category},
                            //   {"ReceiptNumber",list.ReceiptNumber==null?:list.ReceiptNumber},
                            //   {"CashNumber",CashNumber==null?string.Empty: CashNumber},
                            //   {"CreatedTime", DateTime.Now},
                            //   {"ModifiedTime", DateTime.Now},
                            //   {"ModifiedBy", Const.ModifiedBy},
                            //   {"ServerAddTime", DateTime.Now},
                            //   {"ServerModifiedTime", DateTime.Now},
                            //   {"CreatedBy", Const.ModifiedBy},
                            //   {"SessionUserCode", list.SessionUserCode==null?string.Empty: list.SessionUserCode},
                            //   {"IsVoid",false}
                            //};

                            //    Retval = await ExecuteNonQueryAsync(sql1, conn, parameters1, transaction);
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
                            var Retval = string.Empty;
                            var sql = @"UPDATE acc_collection_payment_mode
                                    SET modified_time = @ModifiedTime,
                                        server_modified_time = @ServerModifiedTime,
                                        comments = @Comments,
                                        status = @Status 
                                    WHERE acc_collection_uid = @UID;";
                            Dictionary<string, object> parameters = new Dictionary<string, object>
                    {
                            {"UID", UID},
                            {"Status", "Rejected" },
                            {"Comments", Comments },
                            {"ModifiedTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now}
                    };
                            Retval = await ExecuteNonQueryAsync(sql, conn, parameters, transaction);
                            if (Retval != Const.One)
                            {
                                return Retval;
                            }
                            //IEnumerable<ICollections> accCollection = await GetAmountCheque(CashNumber);
                            //IEnumerable<IAccCollectionAllotment> accCollectionAllotments = await GetChequeDetails(CashNumber);
                            //foreach (var list in accCollection)
                            //{
                            //    var sql1 = @"INSERT INTO AccCollectionSettlement (UID, AccCollectionUID,SettledBy,
                            //        CreatedBy, CreatedTime, ModifiedBy, ModifiedTime, ServerAddTime, 
                            //        ServerModifiedTime, SessionUserCode, Route, PaymentMode, ReceivedAmount, ReceiptNumber, IsVoid, CashNumber)
                            //        VALUES(@UID, @listUID, @SettledBy,  
                            //        @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @SessionUserCode,
                            //        @Route, @PaymentMode, @ReceivedAmount, @ReceiptNumber, @IsVoid, @CashNumber)";
                            //    Dictionary<string, object> parameters1 = new Dictionary<string, object>
                            //{
                            //   {"listUID",UID},
                            //   {"UID",(Guid.NewGuid()).ToString()},
                            //   {"SettledBy", list.SessionUserCode==null?:list.SessionUserCode},
                            //   {"ReceivedAmount",list.PaidAmount==0?0:list.PaidAmount},
                            //   {"Route",list.SessionUserCode==null?:list.SessionUserCode},
                            //   {"PaymentMode",list.Category==null?:list.Category},
                            //   {"ReceiptNumber",list.ReceiptNumber==null?:list.ReceiptNumber},
                            //   {"CashNumber",CashNumber==null?string.Empty: CashNumber},
                            //   {"CreatedTime", DateTime.Now},
                            //   {"ModifiedTime", DateTime.Now},
                            //   {"ModifiedBy", Const.ModifiedBy},
                            //   {"ServerAddTime", DateTime.Now},
                            //   {"ServerModifiedTime", DateTime.Now},
                            //   {"CreatedBy", Const.ModifiedBy},
                            //   {"SessionUserCode", list.SessionUserCode==null?string.Empty: list.SessionUserCode},
                            //   {"IsVoid",false}
                            //};

                            //    Retval = await ExecuteNonQueryAsync(sql1, conn, parameters1, transaction);
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
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    if (UID != null && UID != string.Empty)
                    {
                        var Retval = string.Empty;
                        var sql = @"UPDATE acc_bank
                                SET created_time = @CreatedTime, modified_time = @ModifiedTime, server_add_time = @ServerAddTime,
                                    server_modified_time = @ServerModifiedTime, status = @Status,
                                    comments = @Comments, session_user_code = @SessionUserCode 
                                WHERE uid = @UID;";
                        Dictionary<string, object> parameters = new Dictionary<string, object>
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
                        Retval = await ExecuteNonQueryAsync(sql, conn, parameters, transaction);
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
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    if (UID != null && UID != string.Empty)
                    {
                        var Retval = string.Empty;
                        var sql = @"UPDATE acc_bank
                                SET created_time = @CreatedTime, modified_time = @ModifiedTime, server_add_time = @ServerAddTime,
                                    server_modified_time = @ServerModifiedTime, status = @Status,
                                    comments = @Comments, session_user_code = @SessionUserCode 
                                WHERE uid = @UID;";
                        Dictionary<string, object> parameters = new Dictionary<string, object>
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
                        Retval = await ExecuteNonQueryAsync(sql, conn, parameters, transaction);
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
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",  UID}
                };

                var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                        modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, receipt_number AS ReceiptNumber, 
                        consolidated_receipt_number AS ConsolidatedReceiptNumber, category AS Category, 
                        amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount, 
                        org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, 
                        route_uid AS RouteUID, job_position_uid AS JobPositionUID, emp_uid AS EmpUID, 
                        collected_date AS CollectedDate, status AS Status, remarks AS Remarks, 
                        reference_number AS ReferenceNumber, is_realized AS IsRealized, latitude AS Latitude, 
                        longitude AS Longitude, source AS Source, is_multimode AS IsMultimode, 
                        trip_date AS TripDate, comments AS Comments, salesman AS Salesman, 
                        route AS Route, reversal_receipt_uid AS ReversalReceiptUID, cancelled_on AS CancelledOn FROM 
                        public.acc_collection Where uid = @UID";

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICollections>().GetType();
                Model.Interfaces.ICollections CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.ICollections>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                return (ICollections)ex;
            }
        }
        public async Task<string> UpdateUnSettledAmount(string UID, string ChequeNo, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            try
            {
                var Retval = string.Empty;

                if (UID != null && UID != string.Empty)
                {
                    IEnumerable<IAccCollectionAllotment> recv = await ReceivUnsettle(UID, ChequeNo);
                    foreach (var list in recv)
                    {
                        IAccPayable payable = await GetAccPayableAmount(list.StoreUID, list.ReferenceNumber);
                        if (payable != null)
                        {
                            var sql4 = @"UPDATE acc_payable
                                   SET modified_time = @ModifiedTime,
                                       server_modified_time = @ServerModifiedTime, paid_amount = @PaidAmount, unsettled_amount = @UnSettledAmount
                                   WHERE uid = @UID;";
                            Dictionary<string, object> parameters4 = new Dictionary<string, object>
                            {
                                {"PaidAmount",list.Amount+payable.PaidAmount +list.EarlyPaymentDiscountAmount==0 ? 0 :list.Amount+payable.PaidAmount+list.EarlyPaymentDiscountAmount},
                                {"UnSettledAmount", payable.UnSettledAmount-list.Amount - list.EarlyPaymentDiscountAmount==0 ? 0 : payable.UnSettledAmount-list.Amount-list.EarlyPaymentDiscountAmount },
                                {"UID", payable.UID},
                                {"ModifiedTime", DateTime.Now},
                                {"ServerModifiedTime", DateTime.Now}
                            };
                            Retval = await ExecuteNonQueryAsync(sql4, conn, parameters4, transaction);
                        }

                        if (list.EarlyPaymentDiscountAmount != 0)
                        {
                            var sql1 = @"INSERT INTO public.acc_receivable(
	                             uid, created_by, created_time, modified_by, modified_time, server_add_time,
	                             server_modified_time, source_type, source_uid, reference_number, org_uid,
	                             job_position_uid, amount, paid_amount, store_uid, transaction_date,
	                             due_date, source, currency_uid)
                                 VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime,
                                     @ServerModifiedTime, @SourceType, @SourceUID, @ReferenceNumber, @OrgUID,
                                     @JobPositionUID, @Amount, @PaidAmount, @StoreUID, @TransactionDate,
                                     @DueDate, @Source, @CurrencyUID)";
                            Dictionary<string, object> parameters1 = new Dictionary<string, object>
                                     {
                                         {"CreatedTime", DateTime.Now },
                                         {"ModifiedTime", DateTime.Now},
                                         {"ModifiedBy", list.ModifiedBy ??""},
                                         {"ServerAddTime", DateTime.Now },
                                         {"ServerModifiedTime", DateTime.Now },
                                         {"CreatedBy", list.CreatedBy ??"" },
                                         {"SourceType", "CREDITNOTE" },
                                         {"SourceUID", list.EarlyPaymentDiscountReferenceNo },
                                         {"ReferenceNumber", list.EarlyPaymentDiscountReferenceNo },
                                         {"OrgUID", "FR001" },
                                         {"JobPositionUID", "FR001" },
                                         {"CurrencyUID", list.CurrencyUID },
                                         {"StoreUID", list.StoreUID },
                                         {"TransactionDate", DateTime.Now },
                                         {"DueDate", DateTime.Now.AddYears(1) },
                                         {"Source", "SFA" },
                                         {"Amount",list.EarlyPaymentDiscountAmount},
                                         {"PaidAmount", 0},
                                         {"UID", (Guid.NewGuid()).ToString()}
                                     };

                            Retval = await ExecuteNonQueryAsync(sql1, conn, parameters1, transaction);

                            if (Retval != Const.One)
                                return Retval;
                        }
                    }
                    return Retval;
                }
                else
                {
                    transaction.Rollback();
                    return Const.ParamMissing;
                }
            }
            catch (Exception ex)
            {
                throw new();
            }
        }

        //2nd level of approval
        public async Task<string> ValidateChequeSettlement(string UID, string Comments, string Button, string SessionUserCode, string ReceiptUID, string ChequeNo)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        if ((UID != null && UID != string.Empty) || (ReceiptUID != null && ReceiptUID != string.Empty))
                        {
                            var Retval = string.Empty;
                            var sql = @"UPDATE acc_collection_payment_mode
                                        SET modified_time = @ModifiedTime,
                                            server_modified_time = @ServerModifiedTime, approve_comments = @Comments, status = @Status 
                                        WHERE acc_collection_uid = @UID";
                            Dictionary<string, object> parameters = new Dictionary<string, object>
                    {
                            {"UID", UID},
                            {"Status", "Approved" },
                            {"Comments", Comments },
                            {"ModifiedTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now}
                    };
                            Dictionary<string, object> parameters1 = new Dictionary<string, object>
                    {
                            {"UID", UID},
                            {"Status", "Bounced" },
                            {"Comments", Comments },
                            {"ModifiedTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now}
                    };
                            Retval = Button == "Approved" ? await ExecuteNonQueryAsync(sql, conn, parameters, transaction) : await ExecuteNonQueryAsync(sql, conn, parameters1, transaction);
                            if (Retval != Const.One)
                            {
                                return Retval;
                            }
                            if (Button == "Approved")
                            {
                                IAccCollection accCollectionPayment = await GetCollectionAmount(UID);
                                IAccStoreLedger accStoreLedger = await GetStoreLedger(UID, conn, transaction);
                                var sql3 = @"UPDATE acc_store_ledger
                                                SET modified_by = @ModifiedBy,
                                                    server_modified_time = @ServerModifiedTime, balance = @Balance,
                                                    amount = @Amount, collected_amount = @CollectedAmount
                                                WHERE id = @ID;";

                                Dictionary<string, object> parameters3 = new Dictionary<string, object>
                             {
                                 {"Amount", accCollectionPayment.ReceiptNumber.Contains("OA") ? accCollectionPayment.DefaultCurrencyAmount*-1 : accCollectionPayment.DefaultCurrencyAmount},
                                 {"CollectedAmount",  accCollectionPayment.ReceiptNumber.Contains("OA") ? accCollectionPayment.DefaultCurrencyAmount*-1 : accCollectionPayment.DefaultCurrencyAmount},
                                 {"Balance", accStoreLedger.Balance == 0 ? 0 :accStoreLedger.Balance + 0 },
                                 {"ModifiedBy", accCollectionPayment.ModifiedBy ??""},
                                 {"ServerModifiedTime", DateTime.Now},
                                 {"ID", accStoreLedger.ID==0? 0 : accStoreLedger.ID}
                             };

                                var sql4 = @"UPDATE acc_store_ledger
                                        SET modified_by = @ModifiedBy,
                                            server_modified_time = @ServerModifiedTime, balance = @Balance,
                                            amount = @Amount, collected_amount = @CollectedAmount
                                        WHERE id = @ID;";

                                Dictionary<string, object> parameters4 = new Dictionary<string, object>
                             {
                                 {"Amount", accCollectionPayment.ReceiptNumber.Contains("OA") ? accCollectionPayment.DefaultCurrencyAmount*-1 : accCollectionPayment.DefaultCurrencyAmount},
                                 {"CollectedAmount",  accCollectionPayment.ReceiptNumber.Contains("OA") ? accCollectionPayment.DefaultCurrencyAmount*-1 : accCollectionPayment.DefaultCurrencyAmount},
                                 {"Balance",  accStoreLedger.Balance == 0 ?  0 - accCollectionPayment.DefaultCurrencyAmount - accCollectionPayment.DiscountAmount   :  accStoreLedger.Balance - accCollectionPayment.DefaultCurrencyAmount - accCollectionPayment.DiscountAmount  },
                                 {"ModifiedBy", accCollectionPayment.ModifiedBy ??""},
                                 {"ServerModifiedTime", DateTime.Now},
                                 {"ID", accStoreLedger.ID==0? 0 : accStoreLedger.ID}
                             };
                                Retval = accCollectionPayment.ReceiptNumber.Contains("OA") ? await ExecuteNonQueryAsync(sql4, conn, parameters4, transaction) : await ExecuteNonQueryAsync(sql3, conn, parameters3, transaction);
                                if (Retval != Const.One)
                                {
                                    return Retval;
                                }
                                var retVal = await UpdateUnSettledAmount(UID, ChequeNo, conn, transaction);
                                var retVal1 = await UpdateChequeCollection(UID, Button, conn, transaction);
                                if (retVal1 != Const.One && retVal != Const.One)
                                {
                                    return retVal;
                                }
                            }
                            else
                            {
                                var retVal = await UpdatePayableUnsettle(UID, ChequeNo, conn, transaction);
                                var retVal1 = await UpdateChequeCollection(UID, Button, conn, transaction);

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
            catch (Exception ex)
            {
                throw new();
            }
        }
        public async Task<string> ValidatePOSSettlement(string UID, string Comments, string Status, string SessionUserCode)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    if (UID != null && UID != string.Empty)
                    {
                        ICollections acc = await GetCollecAmount(UID);
                        var Retval = string.Empty;
                        var sql = @"UPDATE acc_collection
                                SET created_time = @CreatedTime, modified_time = @ModifiedTime, server_add_time = @ServerAddTime,
                                    server_modified_time = @ServerModifiedTime, status = @Status,
                                    comments = @Comments
                                WHERE uid = @UID;";
                        Dictionary<string, object> parameters = new Dictionary<string, object>
                    {
                            {"Status", Status },
                            {"UID", UID },
                            {"Comments", Comments },
                            {"CreatedTime", DateTime.Now},
                            {"ModifiedTime", DateTime.Now},
                            {"ServerAddTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now}
                    };
                        Retval = await ExecuteNonQueryAsync(sql, conn, parameters, transaction);
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
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    if (UID != null && UID != string.Empty)
                    {
                        ICollections acc = await GetCollecAmount(UID);
                        var Retval = string.Empty;
                        var sql = @"UPDATE acc_collection
                                    SET created_time = @CreatedTime, modified_time = @ModifiedTime, server_add_time = @ServerAddTime,
                                        server_modified_time = @ServerModifiedTime, status = @Status,
                                        comments = @Comments
                                    WHERE uid = @UID;";
                        Dictionary<string, object> parameters = new Dictionary<string, object>
                    {
                            {"Status", Status },
                            {"UID", UID },
                            {"Comments", Comments },
                            {"CreatedTime", DateTime.Now},
                            {"ModifiedTime", DateTime.Now},
                            {"ServerAddTime", DateTime.Now},
                            {"ServerModifiedTime", DateTime.Now}
                    };
                        Retval = await ExecuteNonQueryAsync(sql, conn, parameters, transaction);
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
                        FROM public.bank;";

            Type type = _serviceProvider.GetRequiredService<IBank>().GetType();
            IEnumerable<IBank> CollectionModuleList = await ExecuteQueryAsync<IBank>(sql, parameters, type);
            return CollectionModuleList;
        }

        //to update bank details when creating payment
        public async Task<int> UpdateChequeDetails(IAccCollectionPaymentMode collection)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    int Retval = 0;
                    var sql = @"INSERT INTO public.acc_collection_payment_mode(
	                uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, acc_collection_uid, bank_uid, 
	                branch, cheque_no, amount, currency_uid, default_currency_uid, default_currency_exchange_rate, default_currency_amount, cheque_date, status, realization_date)
	                VALUES 
	                (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @AccCollectionUID, @BankUID, 
	                @Branch, @ChequeNo, @Amount, @CurrencyUID, @DefaultCurrencyUID, @DefaultCurrencyExchangeRate, @DefaultCurrencyAmount, @ChequeDate, @Status, @RealizationDate);";
                    Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"AccCollectionUID", collection.AccCollectionUID==null ?string.Empty: collection.AccCollectionUID },
                {"BankUID", collection.BankUID==null ?string.Empty: collection.BankUID },
                {"Branch", collection.Branch==null ?string.Empty: collection.Branch },
                {"ChequeNo", collection.ChequeNo==null ?string.Empty: collection.ChequeNo },
                {"ChequeDate", collection.ChequeDate==null ? DateTime.Now : collection.ChequeDate },
                {"Amount", collection.Amount==0 ? 0 : collection.Amount },
                {"CurrencyUID",collection.CurrencyUID== string.Empty?string.Empty: collection.CurrencyUID },
                {"DefaultCurrencyUID", collection.DefaultCurrencyUID==null ?string.Empty: collection.DefaultCurrencyUID },
                {"DefaultCurrencyExchangeRate", collection.DefaultCurrencyExchangeRate==0 ? 0 : collection.DefaultCurrencyExchangeRate },
                {"DefaultCurrencyAmount", 0 },
                {"ChequeDate", (DateTime.Now).Date },
                {"Status", 0 },
                {"RealizationDate", collection.RealizationDate==null ? (DateTime.Now) : collection.RealizationDate },
                {"CreatedBy", collection.CreatedBy==null ?string.Empty: collection.CreatedBy },
                {"ModifiedBy",collection.ModifiedBy==null ?string.Empty: collection.ModifiedBy },
                {"CreatedTime", (DateTime.Now).Date },
                {"ModifiedTime", (DateTime.Now).Date },
                {"ServerAddTime", (DateTime.Now).Date },
                {"ServerModifiedTime", (DateTime.Now).Date },
                {"UID", (Guid.NewGuid()).ToString() }
            };
                    Retval = Convert.ToInt32(await ExecuteNonQueryAsync(sql, conn, parameters, transaction));
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
                var sql = new StringBuilder(@"select * from(SELECT 
                            pm.id AS Id, pm.uid AS PmUID, pm.created_by AS CreatedBy, pm.created_time AS CreatedTime, 
                            pm.modified_by AS ModifiedBy, pm.modified_time AS ModifiedTime, pm.server_add_time AS ServerAddTime, 
                            pm.server_modified_time AS ServerModifiedTime, pm.acc_collection_uid AS AccCollectionUID, 
                            pm.bank_uid AS BankUID, pm.branch AS Branch, pm.cheque_no AS ChequeNo, pm.amount AS Amount, 
                            pm.currency_uid AS CurrencyUID, pm.default_currency_uid AS DefaultCurrencyUID, 
                            pm.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            pm.default_currency_amount AS DefaultCurrencyAmount, pm.cheque_date AS ChequeDate, 
                            pm.status AS Status, pm.realization_date AS RealizationDate, pm.comments AS Comments, 
                            pm.approve_comments AS ApproveComments, 
                            ac.id AS Id, ac.uid AS UID, ac.created_by AS CreatedBy, ac.created_time AS AcCreatedTime, 
                            ac.modified_by AS ModifiedBy, ac.modified_time AS ModifiedTime, ac.server_add_time AS ServerAddTime, 
                            ac.server_modified_time AS ServerModifiedTime, ac.receipt_number AS ReceiptNumber, 
                            ac.consolidated_receipt_number AS ConsolidatedReceiptNumber, ac.category AS Category, 
                            ac.amount AS Amount, ac.currency_uid AS CurrencyUID, ac.default_currency_uid AS DefaultCurrencyUID, 
                            ac.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            ac.default_currency_amount AS DefaultCurrencyAmount, ac.org_uid AS OrgUID, 
                            ac.distribution_channel_uid AS DistributionChannelUID, ac.store_uid AS StoreUID, 
                            ac.route_uid AS RouteUID, ac.job_position_uid AS JobPositionUID, ac.emp_uid AS EmpUID, 
                            ac.collected_date AS CollectedDate, ac.status AS AcStatus, ac.remarks AS Remarks, 
                            ac.reference_number AS ReferenceNumber, ac.is_realized AS IsRealized, ac.latitude AS Latitude, 
                            ac.longitude AS Longitude, ac.source AS Source, ac.is_multimode AS IsMultimode, 
                            ac.trip_date AS TripDate, ac.comments AS Comments, ac.salesman AS Salesman, 
                            ac.route AS Route, ac.reversal_receipt_uid AS ReversalReceiptUID, 
                            ac.cancelled_on AS CancelledOn
                        FROM 
                            public.acc_collection_payment_mode AS pm
                        INNER JOIN 
                            public.acc_collection AS ac 
                        ON 
                            ac.uid = pm.acc_collection_uid)as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                            pm.id AS Id, pm.uid AS PmUID, pm.created_by AS CreatedBy, pm.created_time AS CreatedTime, 
                            pm.modified_by AS ModifiedBy, pm.modified_time AS ModifiedTime, pm.server_add_time AS ServerAddTime, 
                            pm.server_modified_time AS ServerModifiedTime, pm.acc_collection_uid AS AccCollectionUID, 
                            pm.bank_uid AS BankUID, pm.branch AS Branch, pm.cheque_no AS ChequeNo, pm.amount AS Amount, 
                            pm.currency_uid AS CurrencyUID, pm.default_currency_uid AS DefaultCurrencyUID, 
                            pm.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            pm.default_currency_amount AS DefaultCurrencyAmount, pm.cheque_date AS ChequeDate, 
                            pm.status AS Status, pm.realization_date AS RealizationDate, pm.comments AS Comments, 
                            pm.approve_comments AS ApproveComments, 
                            ac.id AS Id, ac.uid AS UID, ac.created_by AS CreatedBy, ac.created_time AS AcCreatedTime, 
                            ac.modified_by AS ModifiedBy, ac.modified_time AS ModifiedTime, ac.server_add_time AS ServerAddTime, 
                            ac.server_modified_time AS ServerModifiedTime, ac.receipt_number AS ReceiptNumber, 
                            ac.consolidated_receipt_number AS ConsolidatedReceiptNumber, ac.category AS Category, 
                            ac.amount AS Amount, ac.currency_uid AS CurrencyUID, ac.default_currency_uid AS DefaultCurrencyUID, 
                            ac.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            ac.default_currency_amount AS DefaultCurrencyAmount, ac.org_uid AS OrgUID, 
                            ac.distribution_channel_uid AS DistributionChannelUID, ac.store_uid AS StoreUID, 
                            ac.route_uid AS RouteUID, ac.job_position_uid AS JobPositionUID, ac.emp_uid AS EmpUID, 
                            ac.collected_date AS CollectedDate, ac.status AS AcStatus, ac.remarks AS Remarks, 
                            ac.reference_number AS ReferenceNumber, ac.is_realized AS IsRealized, ac.latitude AS Latitude, 
                            ac.longitude AS Longitude, ac.source AS Source, ac.is_multimode AS IsMultimode, 
                            ac.trip_date AS TripDate, ac.comments AS Comments, ac.salesman AS Salesman, 
                            ac.route AS Route, ac.reversal_receipt_uid AS ReversalReceiptUID, 
                            ac.cancelled_on AS CancelledOn
                        FROM 
                            public.acc_collection_payment_mode AS pm
                        INNER JOIN 
                            public.acc_collection AS ac 
                        ON 
                            ac.uid = pm.acc_collection_uid)as SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE Status = 'Submitted' And ");
                    AppendFilterCriteria<Model.Interfaces.IAccCollectionPaymentMode>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                else
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE Status = 'Submitted'");
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY CreatedTime Desc, ");
                    AppendSortCriteria(sortCriterias, sql, true);
                }
                else
                {
                    sql.Append(" ORDER BY CreatedTime Desc");
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
                var sql = new StringBuilder(@"SELECT * from(SELECT 
                            pm.id AS Id, pm.uid AS PmUID, pm.created_by AS CreatedBy, pm.created_time AS CreatedTime, 
                            pm.modified_by AS ModifiedBy, pm.modified_time AS ModifiedTime, pm.server_add_time AS ServerAddTime, 
                            pm.server_modified_time AS ServerModifiedTime, pm.acc_collection_uid AS AccCollectionUID, 
                            pm.bank_uid AS BankUID, pm.branch AS Branch, pm.cheque_no AS ChequeNo, pm.amount AS Amount, 
                            pm.currency_uid AS CurrencyUID, pm.default_currency_uid AS DefaultCurrencyUID, 
                            pm.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            pm.default_currency_amount AS DefaultCurrencyAmount, pm.cheque_date AS ChequeDate, 
                            pm.status AS Status, pm.realization_date AS RealizationDate, pm.comments AS Comments, 
                            pm.approve_comments AS ApproveComments, 
                            ac.id AS Id, ac.uid AS UID, ac.created_by AS CreatedBy, ac.created_time AS AcCreatedTime, 
                            ac.modified_by AS ModifiedBy, ac.modified_time AS ModifiedTime, ac.server_add_time AS ServerAddTime, 
                            ac.server_modified_time AS ServerModifiedTime, ac.receipt_number AS ReceiptNumber, 
                            ac.consolidated_receipt_number AS ConsolidatedReceiptNumber, ac.category AS Category, 
                            ac.amount AS Amount, ac.currency_uid AS CurrencyUID, ac.default_currency_uid AS DefaultCurrencyUID, 
                            ac.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            ac.default_currency_amount AS DefaultCurrencyAmount, ac.org_uid AS OrgUID, 
                            ac.distribution_channel_uid AS DistributionChannelUID, ac.store_uid AS StoreUID, 
                            ac.route_uid AS RouteUID, ac.job_position_uid AS JobPositionUID, ac.emp_uid AS EmpUID, 
                            ac.collected_date AS CollectedDate, ac.status AS AcStatus, ac.remarks AS Remarks, 
                            ac.reference_number AS ReferenceNumber, ac.is_realized AS IsRealized, ac.latitude AS Latitude, 
                            ac.longitude AS Longitude, ac.source AS Source, ac.is_multimode AS IsMultimode, 
                            ac.trip_date AS TripDate, ac.comments AS Comments, ac.salesman AS Salesman, 
                            ac.route AS Route, ac.reversal_receipt_uid AS ReversalReceiptUID, 
                            ac.cancelled_on AS CancelledOn
                        FROM 
                            public.acc_collection_payment_mode AS pm
                        INNER JOIN 
                            public.acc_collection AS ac 
                        ON 
                            ac.uid = pm.acc_collection_uid)as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                            pm.id AS Id, pm.uid AS PmUID, pm.created_by AS CreatedBy, pm.created_time AS CreatedTime, 
                            pm.modified_by AS ModifiedBy, pm.modified_time AS ModifiedTime, pm.server_add_time AS ServerAddTime, 
                            pm.server_modified_time AS ServerModifiedTime, pm.acc_collection_uid AS AccCollectionUID, 
                            pm.bank_uid AS BankUID, pm.branch AS Branch, pm.cheque_no AS ChequeNo, pm.amount AS Amount, 
                            pm.currency_uid AS CurrencyUID, pm.default_currency_uid AS DefaultCurrencyUID, 
                            pm.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            pm.default_currency_amount AS DefaultCurrencyAmount, pm.cheque_date AS ChequeDate, 
                            pm.status AS Status, pm.realization_date AS RealizationDate, pm.comments AS Comments, 
                            pm.approve_comments AS ApproveComments, 
                            ac.id AS Id, ac.uid AS UID, ac.created_by AS CreatedBy, ac.created_time AS AcCreatedTime, 
                            ac.modified_by AS ModifiedBy, ac.modified_time AS ModifiedTime, ac.server_add_time AS ServerAddTime, 
                            ac.server_modified_time AS ServerModifiedTime, ac.receipt_number AS ReceiptNumber, 
                            ac.consolidated_receipt_number AS ConsolidatedReceiptNumber, ac.category AS Category, 
                            ac.amount AS Amount, ac.currency_uid AS CurrencyUID, ac.default_currency_uid AS DefaultCurrencyUID, 
                            ac.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            ac.default_currency_amount AS DefaultCurrencyAmount, ac.org_uid AS OrgUID, 
                            ac.distribution_channel_uid AS DistributionChannelUID, ac.store_uid AS StoreUID, 
                            ac.route_uid AS RouteUID, ac.job_position_uid AS JobPositionUID, ac.emp_uid AS EmpUID, 
                            ac.collected_date AS CollectedDate, ac.status AS AcStatus, ac.remarks AS Remarks, 
                            ac.reference_number AS ReferenceNumber, ac.is_realized AS IsRealized, ac.latitude AS Latitude, 
                            ac.longitude AS Longitude, ac.source AS Source, ac.is_multimode AS IsMultimode, 
                            ac.trip_date AS TripDate, ac.comments AS Comments, ac.salesman AS Salesman, 
                            ac.route AS Route, ac.reversal_receipt_uid AS ReversalReceiptUID, 
                            ac.cancelled_on AS CancelledOn
                        FROM 
                            public.acc_collection_payment_mode AS pm
                        INNER JOIN 
                            public.acc_collection AS ac 
                        ON 
                            ac.uid = pm.acc_collection_uid)as SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE Status = 'Settled' And ");
                    AppendFilterCriteria<Model.Interfaces.IAccCollectionPaymentMode>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                else
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE Status = 'Settled'");
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY CreatedTime Desc, ");
                    AppendSortCriteria(sortCriterias, sql, true);
                }
                else
                {
                    sql.Append(" ORDER BY Createdtime Desc");
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
                var sql = new StringBuilder(@"SELECT * from(SELECT 
                            pm.id AS Id, pm.uid AS PmUID, pm.created_by AS CreatedBy, pm.created_time AS CreatedTime, 
                            pm.modified_by AS ModifiedBy, pm.modified_time AS ModifiedTime, pm.server_add_time AS ServerAddTime, 
                            pm.server_modified_time AS ServerModifiedTime, pm.acc_collection_uid AS AccCollectionUID, 
                            pm.bank_uid AS BankUID, pm.branch AS Branch, pm.cheque_no AS ChequeNo, pm.amount AS Amount, 
                            pm.currency_uid AS CurrencyUID, pm.default_currency_uid AS DefaultCurrencyUID, 
                            pm.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            pm.default_currency_amount AS DefaultCurrencyAmount, pm.cheque_date AS ChequeDate, 
                            pm.status AS Status, pm.realization_date AS RealizationDate, pm.comments AS Comments, 
                            pm.approve_comments AS ApproveComments, 
                            ac.id AS Id, ac.uid AS UID, ac.created_by AS CreatedBy, ac.created_time AS AcCreatedTime, 
                            ac.modified_by AS ModifiedBy, ac.modified_time AS ModifiedTime, ac.server_add_time AS ServerAddTime, 
                            ac.server_modified_time AS ServerModifiedTime, ac.receipt_number AS ReceiptNumber, 
                            ac.consolidated_receipt_number AS ConsolidatedReceiptNumber, ac.category AS Category, 
                            ac.amount AS Amount, ac.currency_uid AS CurrencyUID, ac.default_currency_uid AS DefaultCurrencyUID, 
                            ac.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            ac.default_currency_amount AS DefaultCurrencyAmount, ac.org_uid AS OrgUID, 
                            ac.distribution_channel_uid AS DistributionChannelUID, ac.store_uid AS StoreUID, 
                            ac.route_uid AS RouteUID, ac.job_position_uid AS JobPositionUID, ac.emp_uid AS EmpUID, 
                            ac.collected_date AS CollectedDate, ac.status AS AcStatus, ac.remarks AS Remarks, 
                            ac.reference_number AS ReferenceNumber, ac.is_realized AS IsRealized, ac.latitude AS Latitude, 
                            ac.longitude AS Longitude, ac.source AS Source, ac.is_multimode AS IsMultimode, 
                            ac.trip_date AS TripDate, ac.comments AS Comments, ac.salesman AS Salesman, 
                            ac.route AS Route, ac.reversal_receipt_uid AS ReversalReceiptUID, 
                            ac.cancelled_on AS CancelledOn
                        FROM 
                            public.acc_collection_payment_mode AS pm
                        INNER JOIN 
                            public.acc_collection AS ac 
                        ON 
                            ac.uid = pm.acc_collection_uid)as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                            pm.id AS Id, pm.uid AS PmUID, pm.created_by AS CreatedBy, pm.created_time AS CreatedTime, 
                            pm.modified_by AS ModifiedBy, pm.modified_time AS ModifiedTime, pm.server_add_time AS ServerAddTime, 
                            pm.server_modified_time AS ServerModifiedTime, pm.acc_collection_uid AS AccCollectionUID, 
                            pm.bank_uid AS BankUID, pm.branch AS Branch, pm.cheque_no AS ChequeNo, pm.amount AS Amount, 
                            pm.currency_uid AS CurrencyUID, pm.default_currency_uid AS DefaultCurrencyUID, 
                            pm.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            pm.default_currency_amount AS DefaultCurrencyAmount, pm.cheque_date AS ChequeDate, 
                            pm.status AS Status, pm.realization_date AS RealizationDate, pm.comments AS Comments, 
                            pm.approve_comments AS ApproveComments, 
                            ac.id AS Id, ac.uid AS UID, ac.created_by AS CreatedBy, ac.created_time AS AcCreatedTime, 
                            ac.modified_by AS ModifiedBy, ac.modified_time AS ModifiedTime, ac.server_add_time AS ServerAddTime, 
                            ac.server_modified_time AS ServerModifiedTime, ac.receipt_number AS ReceiptNumber, 
                            ac.consolidated_receipt_number AS ConsolidatedReceiptNumber, ac.category AS Category, 
                            ac.amount AS Amount, ac.currency_uid AS CurrencyUID, ac.default_currency_uid AS DefaultCurrencyUID, 
                            ac.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            ac.default_currency_amount AS DefaultCurrencyAmount, ac.org_uid AS OrgUID, 
                            ac.distribution_channel_uid AS DistributionChannelUID, ac.store_uid AS StoreUID, 
                            ac.route_uid AS RouteUID, ac.job_position_uid AS JobPositionUID, ac.emp_uid AS EmpUID, 
                            ac.collected_date AS CollectedDate, ac.status AS AcStatus, ac.remarks AS Remarks, 
                            ac.reference_number AS ReferenceNumber, ac.is_realized AS IsRealized, ac.latitude AS Latitude, 
                            ac.longitude AS Longitude, ac.source AS Source, ac.is_multimode AS IsMultimode, 
                            ac.trip_date AS TripDate, ac.comments AS Comments, ac.salesman AS Salesman, 
                            ac.route AS Route, ac.reversal_receipt_uid AS ReversalReceiptUID, 
                            ac.cancelled_on AS CancelledOn
                        FROM 
                            public.acc_collection_payment_mode AS pm
                        INNER JOIN 
                            public.acc_collection AS ac 
                        ON 
                            ac.uid = pm.acc_collection_uid)as SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE (Status = 'Approved' OR Status = 'Reversed') And UID Not like 'R%' And Category != 'Cash' And ");
                    AppendFilterCriteria<Model.Interfaces.IAccCollectionPaymentMode>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                else
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE (Status = 'Approved' OR Status = 'Reversed') And UID Not like 'R%' and Category != 'Cash'");
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY CreatedTime Desc, ");
                    AppendSortCriteria(sortCriterias, sql, true);
                }
                else
                {
                    sql.Append(" ORDER BY CreatedTime Desc");
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
                var sql = new StringBuilder(@"SELECT * FROM(SELECT 
                            pm.id AS Id, pm.uid AS PmUID, pm.created_by AS CreatedBy, pm.created_time AS CreatedTime, 
                            pm.modified_by AS ModifiedBy, pm.modified_time AS ModifiedTime, pm.server_add_time AS ServerAddTime, 
                            pm.server_modified_time AS ServerModifiedTime, pm.acc_collection_uid AS AccCollectionUID, 
                            pm.bank_uid AS BankUID, pm.branch AS Branch, pm.cheque_no AS ChequeNo, pm.amount AS Amount, 
                            pm.currency_uid AS CurrencyUID, pm.default_currency_uid AS DefaultCurrencyUID, 
                            pm.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            pm.default_currency_amount AS DefaultCurrencyAmount, pm.cheque_date AS ChequeDate, 
                            pm.status AS Status, pm.realization_date AS RealizationDate, pm.comments AS Comments, 
                            pm.approve_comments AS ApproveComments, 
                            ac.id AS Id, ac.uid AS UID, ac.created_by AS CreatedBy, ac.created_time AS AcCreatedTime, 
                            ac.modified_by AS ModifiedBy, ac.modified_time AS ModifiedTime, ac.server_add_time AS ServerAddTime, 
                            ac.server_modified_time AS ServerModifiedTime, ac.receipt_number AS ReceiptNumber, 
                            ac.consolidated_receipt_number AS ConsolidatedReceiptNumber, ac.category AS Category, 
                            ac.amount AS Amount, ac.currency_uid AS CurrencyUID, ac.default_currency_uid AS DefaultCurrencyUID, 
                            ac.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            ac.default_currency_amount AS DefaultCurrencyAmount, ac.org_uid AS OrgUID, 
                            ac.distribution_channel_uid AS DistributionChannelUID, ac.store_uid AS StoreUID, 
                            ac.route_uid AS RouteUID, ac.job_position_uid AS JobPositionUID, ac.emp_uid AS EmpUID, 
                            ac.collected_date AS CollectedDate, ac.status AS AcStatus, ac.remarks AS Remarks, 
                            ac.reference_number AS ReferenceNumber, ac.is_realized AS IsRealized, ac.latitude AS Latitude, 
                            ac.longitude AS Longitude, ac.source AS Source, ac.is_multimode AS IsMultimode, 
                            ac.trip_date AS TripDate, ac.comments AS Comments, ac.salesman AS Salesman, 
                            ac.route AS Route, ac.reversal_receipt_uid AS ReversalReceiptUID, 
                            ac.cancelled_on AS CancelledOn
                        FROM 
                            public.acc_collection_payment_mode AS pm
                        INNER JOIN 
                            public.acc_collection AS ac 
                        ON 
                            ac.uid = pm.acc_collection_uid)as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                            pm.id AS Id, pm.uid AS PmUID, pm.created_by AS CreatedBy, pm.created_time AS CreatedTime, 
                            pm.modified_by AS ModifiedBy, pm.modified_time AS ModifiedTime, pm.server_add_time AS ServerAddTime, 
                            pm.server_modified_time AS ServerModifiedTime, pm.acc_collection_uid AS AccCollectionUID, 
                            pm.bank_uid AS BankUID, pm.branch AS Branch, pm.cheque_no AS ChequeNo, pm.amount AS Amount, 
                            pm.currency_uid AS CurrencyUID, pm.default_currency_uid AS DefaultCurrencyUID, 
                            pm.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            pm.default_currency_amount AS DefaultCurrencyAmount, pm.cheque_date AS ChequeDate, 
                            pm.status AS Status, pm.realization_date AS RealizationDate, pm.comments AS Comments, 
                            pm.approve_comments AS ApproveComments, 
                            ac.id AS Id, ac.uid AS UID, ac.created_by AS CreatedBy, ac.created_time AS AcCreatedTime, 
                            ac.modified_by AS ModifiedBy, ac.modified_time AS ModifiedTime, ac.server_add_time AS ServerAddTime, 
                            ac.server_modified_time AS ServerModifiedTime, ac.receipt_number AS ReceiptNumber, 
                            ac.consolidated_receipt_number AS ConsolidatedReceiptNumber, ac.category AS Category, 
                            ac.amount AS Amount, ac.currency_uid AS CurrencyUID, ac.default_currency_uid AS DefaultCurrencyUID, 
                            ac.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            ac.default_currency_amount AS DefaultCurrencyAmount, ac.org_uid AS OrgUID, 
                            ac.distribution_channel_uid AS DistributionChannelUID, ac.store_uid AS StoreUID, 
                            ac.route_uid AS RouteUID, ac.job_position_uid AS JobPositionUID, ac.emp_uid AS EmpUID, 
                            ac.collected_date AS CollectedDate, ac.status AS AcStatus, ac.remarks AS Remarks, 
                            ac.reference_number AS ReferenceNumber, ac.is_realized AS IsRealized, ac.latitude AS Latitude, 
                            ac.longitude AS Longitude, ac.source AS Source, ac.is_multimode AS IsMultimode, 
                            ac.trip_date AS TripDate, ac.comments AS Comments, ac.salesman AS Salesman, 
                            ac.route AS Route, ac.reversal_receipt_uid AS ReversalReceiptUID, 
                            ac.cancelled_on AS CancelledOn
                        FROM 
                            public.acc_collection_payment_mode AS pm
                        INNER JOIN 
                            public.acc_collection AS ac 
                        ON 
                            ac.uid = pm.acc_collection_uid)as SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE Status = 'Rejected' And ");
                    AppendFilterCriteria<Model.Interfaces.IAccCollectionPaymentMode>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                else
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE Status = 'Rejected'");
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY CreatedTime Desc, ");
                    AppendSortCriteria(sortCriterias, sql, true);
                }
                else
                {
                    sql.Append(" ORDER BY CreatedTime Desc");
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
                var sql = new StringBuilder(@"SELECT * FROM(SELECT 
                            pm.id AS Id, pm.uid AS PmUID, pm.created_by AS CreatedBy, pm.created_time AS CreatedTime, 
                            pm.modified_by AS ModifiedBy, pm.modified_time AS ModifiedTime, pm.server_add_time AS ServerAddTime, 
                            pm.server_modified_time AS ServerModifiedTime, pm.acc_collection_uid AS AccCollectionUID, 
                            pm.bank_uid AS BankUID, pm.branch AS Branch, pm.cheque_no AS ChequeNo, pm.amount AS Amount, 
                            pm.currency_uid AS CurrencyUID, pm.default_currency_uid AS DefaultCurrencyUID, 
                            pm.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            pm.default_currency_amount AS DefaultCurrencyAmount, pm.cheque_date AS ChequeDate, 
                            pm.status AS Status, pm.realization_date AS RealizationDate, pm.comments AS Comments, 
                            pm.approve_comments AS ApproveComments, 
                            ac.id AS Id, ac.uid AS UID, ac.created_by AS CreatedBy, ac.created_time AS AcCreatedTime, 
                            ac.modified_by AS ModifiedBy, ac.modified_time AS ModifiedTime, ac.server_add_time AS ServerAddTime, 
                            ac.server_modified_time AS ServerModifiedTime, ac.receipt_number AS ReceiptNumber, 
                            ac.consolidated_receipt_number AS ConsolidatedReceiptNumber, ac.category AS Category, 
                            ac.amount AS Amount, ac.currency_uid AS CurrencyUID, ac.default_currency_uid AS DefaultCurrencyUID, 
                            ac.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            ac.default_currency_amount AS DefaultCurrencyAmount, ac.org_uid AS OrgUID, 
                            ac.distribution_channel_uid AS DistributionChannelUID, ac.store_uid AS StoreUID, 
                            ac.route_uid AS RouteUID, ac.job_position_uid AS JobPositionUID, ac.emp_uid AS EmpUID, 
                            ac.collected_date AS CollectedDate, ac.status AS AcStatus, ac.remarks AS Remarks, 
                            ac.reference_number AS ReferenceNumber, ac.is_realized AS IsRealized, ac.latitude AS Latitude, 
                            ac.longitude AS Longitude, ac.source AS Source, ac.is_multimode AS IsMultimode, 
                            ac.trip_date AS TripDate, ac.comments AS Comments, ac.salesman AS Salesman, 
                            ac.route AS Route, ac.reversal_receipt_uid AS ReversalReceiptUID, 
                            ac.cancelled_on AS CancelledOn
                        FROM 
                            public.acc_collection_payment_mode AS pm
                        INNER JOIN 
                            public.acc_collection AS ac 
                        ON 
                            ac.uid = pm.acc_collection_uid)as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                            pm.id AS Id, pm.uid AS PmUID, pm.created_by AS CreatedBy, pm.created_time AS CreatedTime, 
                            pm.modified_by AS ModifiedBy, pm.modified_time AS ModifiedTime, pm.server_add_time AS ServerAddTime, 
                            pm.server_modified_time AS ServerModifiedTime, pm.acc_collection_uid AS AccCollectionUID, 
                            pm.bank_uid AS BankUID, pm.branch AS Branch, pm.cheque_no AS ChequeNo, pm.amount AS Amount, 
                            pm.currency_uid AS CurrencyUID, pm.default_currency_uid AS DefaultCurrencyUID, 
                            pm.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            pm.default_currency_amount AS DefaultCurrencyAmount, pm.cheque_date AS ChequeDate, 
                            pm.status AS Status, pm.realization_date AS RealizationDate, pm.comments AS Comments, 
                            pm.approve_comments AS ApproveComments, 
                            ac.id AS Id, ac.uid AS UID, ac.created_by AS CreatedBy, ac.created_time AS AcCreatedTime, 
                            ac.modified_by AS ModifiedBy, ac.modified_time AS ModifiedTime, ac.server_add_time AS ServerAddTime, 
                            ac.server_modified_time AS ServerModifiedTime, ac.receipt_number AS ReceiptNumber, 
                            ac.consolidated_receipt_number AS ConsolidatedReceiptNumber, ac.category AS Category, 
                            ac.amount AS Amount, ac.currency_uid AS CurrencyUID, ac.default_currency_uid AS DefaultCurrencyUID, 
                            ac.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            ac.default_currency_amount AS DefaultCurrencyAmount, ac.org_uid AS OrgUID, 
                            ac.distribution_channel_uid AS DistributionChannelUID, ac.store_uid AS StoreUID, 
                            ac.route_uid AS RouteUID, ac.job_position_uid AS JobPositionUID, ac.emp_uid AS EmpUID, 
                            ac.collected_date AS CollectedDate, ac.status AS AcStatus, ac.remarks AS Remarks, 
                            ac.reference_number AS ReferenceNumber, ac.is_realized AS IsRealized, ac.latitude AS Latitude, 
                            ac.longitude AS Longitude, ac.source AS Source, ac.is_multimode AS IsMultimode, 
                            ac.trip_date AS TripDate, ac.comments AS Comments, ac.salesman AS Salesman, 
                            ac.route AS Route, ac.reversal_receipt_uid AS ReversalReceiptUID, 
                            ac.cancelled_on AS CancelledOn
                        FROM 
                            public.acc_collection_payment_mode AS pm
                        INNER JOIN 
                            public.acc_collection AS ac 
                        ON 
                            ac.uid = pm.acc_collection_uid)as SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE Status = 'Bounced' And ");
                    AppendFilterCriteria<Model.Interfaces.IAccCollectionPaymentMode>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                else
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE Status = 'Bounced'");
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY CreatedTime Desc, ");
                    AppendSortCriteria(sortCriterias, sql, true);
                }
                else
                {
                    sql.Append(" ORDER BY CreatedTime Desc");
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
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {

            };
            var sql = @"SELECT user_name, user_code, password FROM public.acc_user;";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccUser>().GetType();
            IEnumerable<Model.Interfaces.IAccUser> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccUser>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.IAccCollectionPaymentMode>> GetChequeDetails(string UID, string TargetUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID", UID },
                {"TargetUID", TargetUID }
            };
            var sql = @"SELECT 
                        id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                        modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, acc_collection_uid AS AccCollectionUID, 
                        bank_uid AS BankUID, branch AS Branch, cheque_no AS ChequeNo, amount AS Amount, 
                        currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                        default_currency_amount AS DefaultCurrencyAmount, cheque_date AS ChequeDate, 
                        status AS Status, realization_date AS RealizationDate, comments AS Comments, 
                        approve_comments AS ApproveComments FROM 
                        public.acc_collection_payment_mode where acc_collection_uid =@UID ";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionPaymentMode>().GetType();
            IEnumerable<Model.Interfaces.IAccCollectionPaymentMode> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionPaymentMode>(sql, parameters, type);
            return CollectionModuleList;
        }

        public async Task<IEnumerable<Model.Interfaces.IAccCollection>> IsReversal(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID", UID }
            };
            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                        modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, receipt_number AS ReceiptNumber, 
                        consolidated_receipt_number AS ConsolidatedReceiptNumber, category AS Category, 
                        amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount, 
                        org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, 
                        route_uid AS RouteUID, job_position_uid AS JobPositionUID, emp_uid AS EmpUID, 
                        collected_date AS CollectedDate, status AS Status, remarks AS Remarks, 
                        reference_number AS ReferenceNumber, is_realized AS IsRealized, latitude AS Latitude, 
                        longitude AS Longitude, source AS Source, is_multimode AS IsMultimode, 
                        trip_date AS TripDate, comments AS Comments, salesman AS Salesman, 
                        route AS Route, reversal_receipt_uid AS ReversalReceiptUID, cancelled_on AS CancelledOn FROM 
                        public.acc_collection where uid = @UID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
            IEnumerable<Model.Interfaces.IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<Model.Interfaces.IAccCollection>> IsReversalCash(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID", UID }
            };
            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                        modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, receipt_number AS ReceiptNumber, 
                        consolidated_receipt_number AS ConsolidatedReceiptNumber, category AS Category, 
                        amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount, 
                        org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, 
                        route_uid AS RouteUID, job_position_uid AS JobPositionUID, emp_uid AS EmpUID, 
                        collected_date AS CollectedDate, status AS Status, remarks AS Remarks, 
                        reference_number AS ReferenceNumber, is_realized AS IsRealized, latitude AS Latitude, 
                        longitude AS Longitude, source AS Source, is_multimode AS IsMultimode, 
                        trip_date AS TripDate, comments AS Comments, salesman AS Salesman, 
                        route AS Route, reversal_receipt_uid AS ReversalReceiptUID, cancelled_on AS CancelledOn FROM 
                        public.acc_collection where uid = @UID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
            IEnumerable<Model.Interfaces.IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql, parameters, type);
            return CollectionModuleList;
        }

        //to update reversal to false
        //public async Task<IEnumerable<Model.Interfaces.IAccCollection>> IsReversal1(string UID, string Reason, NpgsqlConnection conn, NpgsqlTransaction transaction)
        //{
        //    Dictionary<string, object> parameters = new Dictionary<string, object>
        //    {
        //        {"UID", UID }
        //    };
        //    var sql = @"select * from AccCollection where ChequeNo = @UID";

        //    Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
        //    IEnumerable<Model.Interfaces.IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql, parameters, type);
        //    foreach (var list in CollectionModuleList)
        //    {
        //        await UpdateCollection(list.UID, Reason, conn, transaction);
        //    }
        //    return CollectionModuleList;

        //}
        public async Task<IEnumerable<Model.Interfaces.IAccCollection>> IsReversal2(string UID, string ReverseNo, string Reason, NpgsqlConnection conn, NpgsqlTransaction transaction, string Status = null)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID", UID }
            };
            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                        modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, receipt_number AS ReceiptNumber, 
                        consolidated_receipt_number AS ConsolidatedReceiptNumber, category AS Category, 
                        amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount, 
                        org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, 
                        route_uid AS RouteUID, job_position_uid AS JobPositionUID, emp_uid AS EmpUID, 
                        collected_date AS CollectedDate, status AS Status, remarks AS Remarks, 
                        reference_number AS ReferenceNumber, is_realized AS IsRealized, latitude AS Latitude, 
                        longitude AS Longitude, source AS Source, is_multimode AS IsMultimode, 
                        trip_date AS TripDate, comments AS Comments, salesman AS Salesman, 
                        route AS Route, reversal_receipt_uid AS ReversalReceiptUID, cancelled_on AS CancelledOn FROM 
                        public.acc_collection where uid = @UID";

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
            IEnumerable<Model.Interfaces.IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql, parameters, type);
            foreach (var list in CollectionModuleList)
            {
                await UpdateCollection(list.UID, ReverseNo, Reason, conn, transaction, Status);
            }
            return CollectionModuleList;

        }
        //to update isreversal to false
        public async Task<string> UpdateCollection(string UID, string ReverseNo, string Reason, NpgsqlConnection conn, NpgsqlTransaction transaction, string Status = null)
        {
            var Retval = string.Empty;
            if (UID != null && UID != string.Empty)
            {
                IAccCollection accCollection = await GetCollectAmount(UID);

                var sql4 = @"UPDATE acc_collection
                                SET reversal_receipt_uid = @ReversalReceiptUID, modified_time = @ModifiedTime, comments = @Comments, status = @Status, is_realized = @IsRealized 
                                WHERE uid = @UID;
                                ;";
                Dictionary<string, object> parameters4 = new Dictionary<string, object>
                            {
                                {"ReversalReceiptUID", ReverseNo},
                                {"Status",Status != null ? "Voided" : "Reversed"},
                                {"IsRealized",false},
                                {"ModifiedTime", DateTime.Now},
                                {"Comments", Reason},
                                {"UID", UID}
                            };
                Retval = await ExecuteNonQueryAsync(sql4, conn, parameters4, transaction);
                if (Retval != Const.One)
                {
                    return Retval;
                }
                var sql5 = @"UPDATE acc_collection_payment_mode
                            SET status = @Status 
                            WHERE acc_collection_uid = @UID;";
                Dictionary<string, object> parameters5 = new Dictionary<string, object>
                            {
                                {"Status",Status != null ? "Voided" : "Reversed"},
                                {"UID", UID}
                            };
                Retval = await ExecuteNonQueryAsync(sql5, conn, parameters5, transaction);
                if (Retval != Const.One)
                {
                    return Retval;
                }
            }
            return Retval;
        }

        public async Task<string> UpdateCashCollection(string CashNumber, string SessionUserCode, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            var Retval = string.Empty;
            if (CashNumber != null && CashNumber != string.Empty)
            {
                var sql4 = @"UPDATE acc_collection
                    SET status = @Status, modified_time = @ModifiedTime
                    WHERE uid = @UID;";
                Dictionary<string, object> parameters4 = new Dictionary<string, object>
                            {
                                {"Status", "Settled"},
                                {"ModifiedTime", DateTime.Now},
                                {"UID", CashNumber}
                            };
                Retval = await ExecuteNonQueryAsync(sql4, conn, parameters4, transaction);
                if (Retval != Const.One)
                {
                    return Retval;
                }
            }
            return Retval;
        }
        //pending approving time getting details
        public async Task<string> UpdateChequeCollection(string UID, string Button, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            var Retval = string.Empty;
            if (UID != null && UID != string.Empty)
            {
                var sql4 = @"UPDATE acc_collection
                                SET status = @Status, is_realized = @IsRealized
                                WHERE uid = @UID;";
                Dictionary<string, object> parameters4 = new Dictionary<string, object>
                            {
                                {"Status", Button == "Approve" ? "Settled" : Button == "Approved" ? "Approved" : Button == "Bounced" ? "Bounced" : "Rejected"},
                                {"IsRealized", Button == "Approve" ? false : Button == "Approved" ? true : Button == "Bounced" ? false : false},
                                {"UID", UID}
                            };
                Retval = await ExecuteNonQueryAsync(sql4, conn, parameters4, transaction);
                if (Retval != Const.One)
                {
                    return Retval;
                }
            }
            return Retval;
        }

        //for rejected or bounced records unsettleamount updation
        public async Task<string> UpdatePayableUnsettle(string AccCollectionUID, string Button, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            var Retval = string.Empty;
            if (AccCollectionUID != null && AccCollectionUID != string.Empty)
            {
                IAccCollectionPaymentMode accCollectionPayment = await GetPaymentAmount(AccCollectionUID, conn, transaction);
                IEnumerable<IAccPayable> pay = await PayableUnsettle(AccCollectionUID);
                foreach (var list in pay)
                {
                    var sql4 = @"UPDATE acc_payable
                            SET unsettled_amount = @UnSettledAmount
                            WHERE uid = @UID;";
                    Dictionary<string, object> parameters4 = new Dictionary<string, object>
                            {
                                {"UnSettledAmount", list.UnSettledAmount - accCollectionPayment.DefaultCurrencyAmount},
                                {"UID", list.UID}
                            };
                    Retval = await ExecuteNonQueryAsync(sql4, conn, parameters4, transaction);
                    if (Retval != Const.One)
                    {
                        return Retval;
                    }


                }
            }
            return Retval;
        }


        public async Task<string> ExecuteNonQueryAsync(string sql, NpgsqlConnection conn, Dictionary<string, object> parameters, NpgsqlTransaction transaction)
        {
            int retValue = 0;
            try
            {
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Transaction = transaction;
                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            cmd.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
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
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                 {"StoreUID",  StoreUID}
            };

                var sql = @"WITH delay_time_categories AS (
                              SELECT '0-30 days' AS DelayTime
                              UNION SELECT '30-60 days' AS DelayTime
                              UNION SELECT '60-90 days' AS DelayTime
                              UNION SELECT '90+ days' AS DelayTime
                            )
                            SELECT
                              dt.DelayTime,
                              COALESCE(COUNT(ap.transaction_date), 0) AS Count,
                              COALESCE(SUM(ap.balance_amount), 0) AS Balance,
                              ap.store_uid AS StoreUID
                            FROM delay_time_categories dt
                            LEFT JOIN acc_payable ap
                              ON dt.DelayTime =
                                CASE
                                  WHEN AGE(CURRENT_DATE, ap.transaction_date) BETWEEN '0 days' AND '30 days' THEN '0-30 days'
                                  WHEN AGE(CURRENT_DATE, ap.transaction_date) BETWEEN '30 days' AND '60 days' THEN '30-60 days'
                                  WHEN AGE(CURRENT_DATE, ap.transaction_date) BETWEEN '60 days' AND '90 days' THEN '60-90 days'
                                  WHEN AGE(CURRENT_DATE, ap.transaction_date) > '90 days' THEN '90+ days'
                                  ELSE 'Unknown'
                                END
                            WHERE ap.store_uid = @StoreUID OR ap.store_uid IS NULL
                            GROUP BY dt.DelayTime, ap.store_uid";
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
                IEnumerable<Model.Interfaces.IAccPayable> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql, parameters, type);
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
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                     {"ReceiptNumber",  ReceiptNumber},
                     {"StoreUID",  StoreUID}
                };

                var sql = @"SELECT SUM(balance_amount) AS BalanceAmount, SUM(unsettled_amount) AS UnSettledAmount
                            FROM acc_payable
                            WHERE reference_number = @ReceiptNumber AND store_uid = @StoreUID;";

                var sql1 = @"SELECT balance_amount AS BalanceAmount, unsettled_amount AS UnSettledAmount
                            FROM acc_payable
                            WHERE reference_number = @ReceiptNumber AND store_uid = @StoreUID;";

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
                Dictionary<string, object> parameters = new Dictionary<string, object>
                        {
                             {"StoreUID",  StoreUID},
                             {"startDay",  startDay},
                             {"endDay",  endDay}
                        };

                var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime, source_type AS SourceType, source_uid AS SourceUID, reference_number AS ReferenceNumber, org_uid AS OrgUID, job_position_uid AS JobPositionUID,
                amount AS Amount, paid_amount AS PaidAmount, store_uid AS StoreUID, transaction_date AS TransactionDate, due_date AS DueDate, balance_amount AS BalanceAmount, unsettled_amount AS UnSettledAmount,
                source AS Source, currency_uid AS CurrencyUID
                FROM public.acc_payable
                WHERE store_uid = @StoreUID
                AND CURRENT_DATE - transaction_date BETWEEN @startDay AND @endDay;";

                var sql1 = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime, source_type AS SourceType, source_uid AS SourceUID, reference_number AS ReferenceNumber, org_uid AS OrgUID, job_position_uid AS JobPositionUID,
                amount AS Amount, paid_amount AS PaidAmount, store_uid AS StoreUID, transaction_date AS TransactionDate, due_date AS DueDate, balance_amount AS BalanceAmount, unsettled_amount AS UnSettledAmount,
                source AS Source, currency_uid AS CurrencyUID
                FROM public.acc_payable
                WHERE store_uid = @StoreUID
                AND CURRENT_DATE - transaction_date >= @startDay;";

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
                if (endDay == 0)
                {
                    IEnumerable<Model.Interfaces.IAccPayable> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql1, parameters, type);

                    IEnumerable<IAccPayable> credit = await GetCreditNotes(StoreUID);
                    List<IAccPayable> newArray = new List<IAccPayable>();
                    newArray.AddRange(CollectionModuleList);
                    newArray.AddRange(credit);
                    IEnumerable<Model.Interfaces.IAccPayable> array = newArray.ToArray();
                    return array;

                }
                else
                {
                    IEnumerable<Model.Interfaces.IAccPayable> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql, parameters, type);

                    IEnumerable<IAccPayable> credit = await GetCreditNotes(StoreUID);
                    List<IAccPayable> newArray = new List<IAccPayable>();
                    newArray.AddRange(CollectionModuleList);
                    newArray.AddRange(credit);
                    IEnumerable<Model.Interfaces.IAccPayable> array = newArray.ToArray();
                    return array;

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<IAccPayable>> GetCreditNotes(string AccCollectionUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"StoreUID",  AccCollectionUID}
                };

            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
            server_modified_time AS ServerModifiedTime, source_type AS SourceType, source_uid AS SourceUID, reference_number AS ReferenceNumber, org_uid AS OrgUID, job_position_uid AS JobPositionUID,
            currency_uid AS CurrencyUID, amount AS Amount, paid_amount AS PaidAmount, store_uid AS StoreUID, transaction_date AS TransactionDate, due_date AS DueDate, unsettled_amount AS UnSettledAmount,
            balance_amount AS BalanceAmount, source AS Source FROM 
            public.acc_receivable where store_uid = @StoreUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
            IEnumerable<IAccPayable> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql, parameters, type);
            return CollectionModuleList;
        }
        public async Task<IEnumerable<IAccCollectionPaymentMode>> GetUnSettleAmount(string AccCollectionUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"StoreUID",  AccCollectionUID}
                };


            var sql = @"SELECT 
                            pm.id AS Id, pm.uid AS UID, pm.created_by AS CreatedBy, pm.created_time AS CreatedTime, 
                            pm.modified_by AS ModifiedBy, pm.modified_time AS ModifiedTime, pm.server_add_time AS ServerAddTime, 
                            pm.server_modified_time AS ServerModifiedTime, pm.acc_collection_uid AS AccCollectionUID, 
                            pm.bank_uid AS BankUID, pm.branch AS Branch, pm.cheque_no AS ChequeNo, pm.amount AS Amount, 
                            pm.currency_uid AS CurrencyUID, pm.default_currency_uid AS DefaultCurrencyUID, 
                            pm.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            pm.default_currency_amount AS DefaultCurrencyAmount, pm.cheque_date AS ChequeDate, pm.status AS Status, 
                            pm.realization_date AS RealizationDate, pm.comments AS Comments, pm.approve_comments AS ApproveComments, 
                            ac.id AS Id, ac.uid AS UID, ac.created_by AS CreatedBy, ac.created_time AS CreatedTime, 
                            ac.modified_by AS ModifiedBy, ac.modified_time AS ModifiedTime, ac.server_add_time AS ServerAddTime, 
                            ac.server_modified_time AS ServerModifiedTime, ac.receipt_number AS ReceiptNumber, 
                            ac.consolidated_receipt_number AS ConsolidatedReceiptNumber, ac.category AS Category, ac.amount AS Amount, 
                            ac.currency_uid AS CurrencyUID, ac.default_currency_uid AS DefaultCurrencyUID, 
                            ac.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, ac.default_currency_amount AS DefaultCurrencyAmount, 
                            ac.org_uid AS OrgUID, ac.distribution_channel_uid AS DistributionChannelUID, ac.store_uid AS StoreUID, 
                            ac.route_uid AS RouteUID, ac.job_position_uid AS JobPositionUID, ac.emp_uid AS EmpUID, 
                            ac.collected_date AS CollectedDate, ac.status AS Status, ac.remarks AS Remarks, ac.reference_number AS ReferenceNumber, 
                            ac.is_realized AS IsRealized, ac.latitude AS Latitude, ac.longitude AS Longitude, ac.source AS Source, 
                            ac.is_multimode AS IsMultimode, ac.trip_date AS TripDate, ac.comments AS Comments, ac.salesman AS Salesman, 
                            ac.route AS Route, ac.reversal_receipt_uid AS ReversalReceiptUID, ac.cancelled_on AS CancelledOn
                        FROM 
                            acc_collection_payment_mode pm 
                        INNER JOIN
                            acc_collection ac
                        ON 
                            pm.acc_collection_uid = ac.uid
 
                            WHERE 
                                pm.status NOT IN ('Approved', 'OnAccount', 'Reversed', 'Bounced', 'Rejected') 
                                AND ac.store_uid = @StoreUID 
                                AND ac.category != 'Cash';";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionPaymentMode>().GetType();
            IEnumerable<IAccCollectionPaymentMode> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionPaymentMode>(sql, parameters, type);
            return CollectionModuleList;
        }

        public async Task<IEnumerable<Winit.Modules.Setting.Model.Interfaces.ISetting>> GetSettingByType(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT id, uid, type, name, value, data_type, is_editable, ss, created_time, modified_time, server_add_time, server_modified_time FROM public.setting where value = 'True'";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Setting.Model.Interfaces.ISetting>().GetType();
            IEnumerable<Winit.Modules.Setting.Model.Interfaces.ISetting> SettingDetails = await ExecuteQueryAsync<Winit.Modules.Setting.Model.Interfaces.ISetting>(sql, parameters, type);
            return SettingDetails;
        }

        Task<List<IAccPayable>> ICollectionModuleDL.PopulateCollectionPage(string CustomerCode, string Tabs)
        {
            throw new NotImplementedException();
        }

        Task<List<IAccPayable>> ICollectionModuleDL.GetInvoicesMobile(string AccCollectionUID)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResponse<Model.Interfaces.IAccCollection>> ViewPayments(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from(SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                        (select code from emp where uid = acc_collection.created_by) AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, receipt_number AS ReceiptNumber, 
                        consolidated_receipt_number AS ConsolidatedReceiptNumber, category AS Category, 
                        amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount, 
                        org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, 
                        route_uid AS RouteUID, job_position_uid AS JobPositionUID, emp_uid AS EmpUID, 
                        collected_date AS CollectedDate, status AS Status, remarks AS Remarks, 
                        reference_number AS ReferenceNumber, is_realized AS IsRealized, latitude AS Latitude, 
                        longitude AS Longitude, source AS Source, is_multimode AS IsMultimode, 
                        trip_date AS TripDate, comments AS Comments, salesman AS Salesman, 
                        route AS Route, reversal_receipt_uid AS ReversalReceiptUID, cancelled_on AS CancelledOn FROM 
                        public.acc_collection) as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"select COUNT(1) AS Cnt  from (SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                        (select code from emp where uid = acc_collection.created_by) AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, receipt_number AS ReceiptNumber, 
                        consolidated_receipt_number AS ConsolidatedReceiptNumber, category AS Category, 
                        amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount, 
                        org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, 
                        route_uid AS RouteUID, job_position_uid AS JobPositionUID, emp_uid AS EmpUID, 
                        collected_date AS CollectedDate, status AS Status, remarks AS Remarks, 
                        reference_number AS ReferenceNumber, is_realized AS IsRealized, latitude AS Latitude, 
                        longitude AS Longitude, source AS Source, is_multimode AS IsMultimode, 
                        trip_date AS TripDate, comments AS Comments, salesman AS Salesman, 
                        route AS Route, reversal_receipt_uid AS ReversalReceiptUID, cancelled_on AS CancelledOn FROM 
                        public.acc_collection)as SubQuery");
                }
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                };
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE UID NOT LIKE '%R -%' And ");
                    AppendFilterCriteria<IAccCollection>
                        (filterCriterias, sbFilterCriteria, parameters); ;
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                else
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE UID NOT LIKE '%R -%'");
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql, true);
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

        public async Task<List<IAccCollectionAllotment>> ViewPaymentsDetails(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",  UID}
                };


            var sql = @"SELECT AL.id AS Id, AL.uid AS UID, AL.created_by AS CreatedBy, AL.created_time AS CreatedTime, AL.modified_by AS ModifiedBy, AL.modified_time AS ModifiedTime, AL.server_add_time AS ServerAddTime,
                                AL.server_modified_time AS ServerModifiedTime, AL.acc_collection_uid AS AccCollectionUID, AL.target_type AS TargetType, AL.target_uid AS TargetUID, AL.reference_number AS ReferenceNumber, AL.amount AS Amount,
                                AL.currency_uid AS CurrencyUID, AL.default_currency_uid AS DefaultCurrencyUID, AL.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, AL.default_currency_amount AS DefaultCurrencyAmount,
                                AL.early_payment_discount_percentage AS EarlyPaymentDiscountPercentage, AL.early_payment_discount_amount AS EarlyPaymentDiscountAmount, AL.early_payment_discount_reference_no AS EarlyPaymentDiscountReferenceNo
                            FROM 
                                public.acc_collection AC 
                            INNER JOIN
                                public.acc_collection_allotment AL 
                            ON 
                                AC.uid = AL.acc_collection_uid 
                            WHERE 
                                AC.uid = @UID;";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollectionAllotment>().GetType();
            List<IAccCollectionAllotment> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollectionAllotment>(sql, parameters, type);
            return CollectionModuleList;
        }

        public async Task<string> CreateReceiptWithEarlyPaymentDiscount(ICollections EarlyPaymentRecords)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    var accCollec = await InsertAccCollection(conn, transaction, EarlyPaymentRecords);
                    if (accCollec != Const.One)
                        return accCollec;
                    var accPaymode = await InsertAccCollectionPaymentMode(conn, transaction, EarlyPaymentRecords);
                    if (accPaymode != Const.One)
                        return accPaymode;
                    var accStore = await InsertAccStoreLedger(conn, transaction, EarlyPaymentRecords);
                    if (accStore != Const.One)
                        return accStore;
                    var Payable = await InsertAccPayable(conn, transaction, EarlyPaymentRecords);
                    if (Payable != Const.One)
                        return Payable;
                    var Receive = await InsertAccReceivable(conn, transaction, EarlyPaymentRecords);
                    if (Receive != Const.One)
                        return Receive;
                    var accPay = await UpdateAccPayable(conn, transaction, EarlyPaymentRecords);
                    var accRec = await UpdateAccReceivable(conn, transaction, EarlyPaymentRecords);
                    if (accPay == Const.One)
                    {
                        var accAllot = await InsertAccCollectionAllotment(conn, transaction, EarlyPaymentRecords);
                        if (accAllot == Const.One)
                        {
                            transaction.Commit();
                            return Const.SuccessInsert;
                        }
                        else
                        {
                            transaction.Rollback();
                            return accAllot;
                        }
                    }
                    else
                    {
                        transaction.Rollback();
                        return Const.NotFound;
                    }
                }
            }
        }

        public async Task<IEnumerable<IEarlyPaymentDiscountConfiguration>> CheckEligibleForDiscount(string ApplicableCode)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"ApplicableCode",  ApplicableCode}
                };

            var sql = @"select * from early_payment_discount_configuration where applicable_code = @ApplicableCode";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IEarlyPaymentDiscountConfiguration>().GetType();
            IEnumerable<IEarlyPaymentDiscountConfiguration> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IEarlyPaymentDiscountConfiguration>(sql, parameters, type);
            return CollectionModuleList;
        }

        public async Task<List<IStore>> GetCustomerCode(string CustomerCode)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"CustomerCode",  CustomerCode}
                };

            var sql = @"SELECT '(' || code || ')' || name AS Name, uid AS UID FROM store;";
            Type type = _serviceProvider.GetRequiredService<IStore>().GetType();
            List<IStore> CollectionModuleList = await ExecuteQueryAsync<IStore>(sql, parameters, type);
            return CollectionModuleList;
        }



        Task<List<IAccCollectionAllotment>> ICollectionModuleDL.AllotmentReceipts(string AccCollectionUID)
        {
            throw new NotImplementedException();
        }

        Task<string> ICollectionModuleDL.CreateReceipt(ICollections collection)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Winit.Modules.CollectionModule.Model.Interfaces.IAccStoreLedger>> GetAccountStatement(string StoreUID, string FromDate, string ToDate)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"StoreUID",  StoreUID},
                    {"FromDate",  FromDate + " 00:00:00"},
                    {"ToDate",  ToDate + " 23:59:59"}
                };


                var sql = @"WITH StoreInfo AS (
                            SELECT code, name
                            FROM store
                            WHERE uid = @StoreUID)
                            SELECT (SELECT code FROM StoreInfo) AS Code,
                            (SELECT name FROM StoreInfo) AS Name,SL.id AS Id, SL.uid AS UID, SL.created_by AS CreatedBy, SL.created_time AS CreatedTime, 
                            SL.modified_by AS ModifiedBy, SL.modified_time AS ModifiedTime, SL.server_add_time AS ServerAddTime, 
                            SL.server_modified_time AS ServerModifiedTime, SL.source_type AS SourceType, SL.source_uid AS SourceUID, 
                            SL.credit_type AS CreditType, SL.org_uid AS OrgUID, SL.store_uid AS StoreUID, SL.default_currency_uid AS DefaultCurrencyUID, 
                            SL.document_number AS DocumentNumber, SL.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            SL.default_currency_amount AS DefaultCurrencyAmount, SL.transaction_date_time AS TransactionDateTime, 
                            SL.collected_amount AS CollectedAmount, SL.currency_uid AS CurrencyUID, SL.amount AS Amount, 
                            SL.balance AS Balance, SL.comments AS Comments,
                             AC.uid AS UID, AC.created_by AS CreatedBy, AC.created_time AS CreatedTime, 
                            AC.modified_by AS ModifiedBy, AC.modified_time AS ModifiedTime, AC.server_add_time AS ServerAddTime, 
                            AC.server_modified_time AS ServerModifiedTime, AC.receipt_number AS ReceiptNumber, 
                            AC.consolidated_receipt_number AS ConsolidatedReceiptNumber, AC.category AS Category, 
                             AC.currency_uid AS CurrencyUID, AC.default_currency_uid AS DefaultCurrencyUID, 
                            AC.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, AC.default_currency_amount AS DefaultCurrencyAmount, 
                            AC.org_uid AS OrgUID, AC.distribution_channel_uid AS DistributionChannelUID, AC.store_uid AS StoreUID, 
                            AC.route_uid AS RouteUID, AC.job_position_uid AS JobPositionUID, AC.emp_uid AS EmpUID, 
                            AC.collected_date AS CollectedDate, AC.status AS Status, AC.remarks AS Remarks, 
                            AC.reference_number AS ReferenceNumber, AC.is_realized AS IsRealized, AC.latitude AS Latitude, 
                            AC.longitude AS Longitude, AC.source AS Source, AC.is_multimode AS IsMultimode, 
                            AC.trip_date AS TripDate,  AC.salesman AS Salesman, 
                            AC.route AS Route, AC.reversal_receipt_uid AS ReversalReceiptUID, AC.cancelled_on AS CancelledOn
                        FROM 
                            public.acc_store_ledger SL 
                        INNER JOIN public.acc_collection AC ON SL.document_number = AC.receipt_number 
                        INNER JOIN public.store AST ON AST.uid = AC.store_uid WHERE 
                            AC.store_uid = @StoreUID ORDER BY AC.created_time;
                        ";
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccStoreLedger>().GetType();
                List<IAccStoreLedger> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccStoreLedger>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        public async Task<List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable>> GetAccountStatementPay(string StoreUID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"StoreUID",  StoreUID}
                };


                var sql = @"SELECT AP.id AS Id, AP.uid AS UID,S.code AS Code, S.name AS Name, AP.created_by AS CreatedBy, AP.created_time AS CreatedTime, AP.modified_by AS ModifiedBy, 
                                AP.modified_time AS ModifiedTime, AP.server_add_time AS ServerAddTime, AP.server_modified_time AS ServerModifiedTime, 
                                AP.source_type AS SourceType, AP.source_uid AS SourceUID, AP.reference_number AS ReferenceNumber, AP.org_uid AS OrgUID, 
                                AP.job_position_uid AS JobPositionUID, AP.amount AS Amount, AP.paid_amount AS PaidAmount, AP.store_uid AS StoreUID, 
                                AP.transaction_date AS TransactionDate, AP.due_date AS DueDate, AP.balance_amount AS BalanceAmount, AP.unsettled_amount AS UnSettledAmount, 
                                AP.source AS Source, AP.currency_uid AS CurrencyUID, 
                                S.id, S.uid, S.created_by, S.created_time, S.modified_by, S.modified_time, S.server_add_time, S.server_modified_time, 
                                S.company_uid,  S.number, S.alias_name, S.legal_name, S.type, S.bill_to_store_uid, S.ship_to_store_uid, 
                                S.sold_to_store_uid, S.status, S.is_active, S.store_class, S.store_rating, S.is_blocked, S.blocked_reason_code, 
                                S.blocked_reason_description, S.created_by_emp_uid, S.created_by_job_position_uid, S.country_uid, S.region_uid, 
                                S.city_uid, S.source, S.arabic_name, S.outlet_name, S.blocked_by_emp_uid, S.is_tax_applicable, S.tax_doc_number, 
                                S.school_warehouse, S.day_type, S.special_day, S.is_tax_doc_verified, S.store_size, S.prospect_emp_uid, S.tax_key_field, 
                                S.store_image, S.is_vat_qr_capture_mandatory, S.tax_type, S.franchisee_org_uid, S.state_uid, S.route_type, S.price_type
                            FROM 
                                public.acc_payable AP
                            INNER JOIN 
                                public.store S ON AP.store_uid = S.uid
                            WHERE AP.store_uid = @StoreUID;";
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
                List<IAccPayable> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccPayable>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        public async Task<List<IAccPayable>> GetInvoices(string StoreUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"StoreUID",  StoreUID}
                };

            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime, source_type AS SourceType, source_uid AS SourceUID, reference_number AS ReferenceNumber, org_uid AS OrgUID, job_position_uid AS JobPositionUID,
                amount AS Amount, paid_amount AS PaidAmount, store_uid AS StoreUID, transaction_date AS TransactionDate, due_date AS DueDate, balance_amount AS BalanceAmount, unsettled_amount AS UnSettledAmount,
                source AS Source, currency_uid AS CurrencyUID
                FROM 
                    public.acc_payable where store_uid = @StoreUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccPayable>().GetType();
            List<IAccPayable> CollectionModuleList = await ExecuteQueryAsync<IAccPayable>(sql, parameters, type);
            return CollectionModuleList;
        }

        public async Task<string> AddEarlyPayment(IEarlyPaymentDiscountConfiguration EarlyPayment)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        IEarlyPaymentDiscountConfiguration EarlyPaymentData = await GetEarlyPaymentData(EarlyPayment.Applicable_Code, EarlyPayment.Payment_Mode, EarlyPayment.Applicable_Type, EarlyPayment.Valid_From.ToString("yyyy-MM-dd"));
                        bool IsSameCustomer = EarlyPaymentData != null ? true : false;
                        var Retval = string.Empty;
                        var updatesql = @"UPDATE public.early_payment_discount_configuration
                                        SET modified_by = @modified_by, modified_time = @modified_time, server_modified_time = @server_modified_time,
                                            sales_org = @sales_org, applicable_type = @applicable_type, applicable_code = @applicable_code, payment_mode = @payment_mode,
                                            advance_paid_days = @advance_paid_days, discount_type = @discount_type, discount_value = @discount_value, isactive = @isactive,
                                            valid_from = @valid_from, valid_to = @valid_to, applicable_onpartial_payments = @applicable_onpartial_payments, 
                                            applicable_onoverdue_customers = @applicable_onoverdue_customers WHERE uid = @uid";

                        var insertsql = @"INSERT INTO public.early_payment_discount_configuration(uid, created_by, created_time, modified_by, modified_time, 
                                    server_add_time, server_modified_time,sales_org, applicable_type, applicable_code, payment_mode, advance_paid_days, discount_type, 
                                    discount_value,isactive, valid_from, valid_to, applicable_onpartial_payments, applicable_onoverdue_customers)
                                VALUES (@uid, @created_by, @created_time, @modified_by, @modified_time, @server_add_time, @server_modified_time,
                                    @sales_org, @applicable_type, @applicable_code, @payment_mode, @advance_paid_days, @discount_type, @discount_value,
                                    @isactive, @valid_from, @valid_to, @applicable_onpartial_payments, @applicable_onoverdue_customers)";
                        Dictionary<string, object> insertparameters = new Dictionary<string, object>
                        {
                            {"created_by", EarlyPayment.Created_By ??"" },
                            {"created_time", DateTime.Now },
                            {"modified_by", EarlyPayment.Modified_By ??"" },
                            {"modified_time", DateTime.Now },
                            {"server_add_time", DateTime.Now },
                            {"server_modified_time",DateTime.Now },
                            {"sales_org", EarlyPayment.Sales_Org },
                            {"applicable_type", EarlyPayment.Applicable_Type },
                            {"applicable_code", EarlyPayment.Applicable_Code },
                            {"payment_mode", EarlyPayment.Payment_Mode },
                            {"advance_paid_days", EarlyPayment.Advance_Paid_Days },
                            {"discount_type", EarlyPayment.Discount_Type },
                            {"discount_value", EarlyPayment.Discount_Value },
                            {"isactive",EarlyPayment.IsActive },
                            {"valid_from", EarlyPayment.Valid_From },
                            {"valid_to", EarlyPayment.Valid_To },
                            {"applicable_onpartial_payments", EarlyPayment.Applicable_OnPartial_Payments },
                            {"applicable_onoverdue_customers", EarlyPayment.Applicable_OnOverDue_Customers },
                            {"uid", (Guid.NewGuid()).ToString() }
                        };

                        Dictionary<string, object> updateparameters = new Dictionary<string, object>
                        {
                            {"modified_by", EarlyPayment.Modified_By ??"" },
                            {"modified_time", DateTime.Now },
                            {"server_modified_time",DateTime.Now },
                            {"sales_org", EarlyPayment.Sales_Org },
                            {"applicable_type", EarlyPayment.Applicable_Type },
                            {"applicable_code", EarlyPayment.Applicable_Code },
                            {"payment_mode", EarlyPayment.Payment_Mode },
                            {"advance_paid_days",EarlyPayment.Advance_Paid_Days },
                            {"discount_type", EarlyPayment.Discount_Type },
                            {"discount_value", EarlyPayment.Discount_Value },
                            {"isactive",EarlyPayment.IsActive },
                            {"valid_from", EarlyPayment.Valid_From },
                            {"valid_to", EarlyPayment.Valid_To },
                            {"applicable_onpartial_payments", EarlyPayment.Applicable_OnPartial_Payments },
                            {"applicable_onoverdue_customers", EarlyPayment.Applicable_OnOverDue_Customers },
                            {"uid", EarlyPaymentData != null ? EarlyPaymentData.UID : ""}
                        };


                        Retval = IsSameCustomer ? await ExecuteNonQueryAsync(updatesql, conn, updateparameters, transaction) : await ExecuteNonQueryAsync(insertsql, conn, insertparameters, transaction);
                        if (Retval != Const.One)
                        {
                            return Retval;
                        }
                        else
                        {
                            transaction.Commit();
                            if (IsSameCustomer)
                            {
                                return "2";
                            }
                            return Retval;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }


        public async Task<Model.Interfaces.IEarlyPaymentDiscountConfiguration> GetEarlyPaymentData(string UID, string PaymentMode, string ApplicableType, string ValidFrom)
        {

            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",  UID},
                    {"PaymentMode",  PaymentMode},
                    {"ApplicableType",  ApplicableType},
                    {"ValidFrom",  ValidFrom}
                };

                var sql = @"Select * from early_payment_discount_configuration Where applicable_code = @UID and payment_mode = @PaymentMode and applicable_type = @ApplicableType AND CAST(@ValidFrom AS DATE) BETWEEN valid_from AND valid_to";

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IEarlyPaymentDiscountConfiguration>().GetType();
                Model.Interfaces.IEarlyPaymentDiscountConfiguration CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IEarlyPaymentDiscountConfiguration>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                return (IEarlyPaymentDiscountConfiguration)ex;
            }
        }

        Task<List<IAccCollection>> ICollectionModuleDL.PaymentReceipts(string FromDate, string ToDate, string Payment, string Print)
        {
            throw new NotImplementedException();
        }

        Task<List<ICollectionPrint>> ICollectionModuleDL.GetCollectionStoreDataForPrinter(List<string> UID)
        {
            throw new NotImplementedException();
        }

        Task<List<ICollectionPrintDetails>> ICollectionModuleDL.GetAllotmentDataForPrinter(string AccCollectionUID)
        {
            throw new NotImplementedException();
        }

        Task<List<IAccCollectionPaymentMode>> ICollectionModuleDL.ShowPendingRecordsInPopUp(string InvoiceNumber, string StoreUID)
        {
            throw new NotImplementedException();
        }

        Task<List<IAccPayable>> ICollectionModuleDL.GetPendingRecordsFromDB(string StoreUID)
        {
            throw new NotImplementedException();
        }

        Task<List<IAccCollectionPaymentMode>> ICollectionModuleDL.CPOData(string AccCollectionUID)
        {
            throw new NotImplementedException();
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
        public async Task<List<IEarlyPaymentDiscountConfiguration>> GetConfigurationDetails()
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                    {"AccCollectionUID",  AccCollectionUID}
                };
                var sql = @"select * from early_payment_discount_configuration";
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IEarlyPaymentDiscountConfiguration>().GetType();
                List<IEarlyPaymentDiscountConfiguration> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IEarlyPaymentDiscountConfiguration>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
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
                            where AC.category = 'Cash' and AC.status = 'Collected' and AC.collection_deposit_status is null";
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
                List<IAccCollection> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IAccCollection>(sql, parameters, type);
                return CollectionModuleList;
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
                            remote_collection_reason AS RemoteCollectionReason FROM acc_collection where receipt_number = ANY(@UIDs);";
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

        public async Task<bool> ApproveOrRejectDepositRequest(IAccCollectionDeposit accCollectionDeposit, string Status)
        {
            try
            {
                int Retval = 0;
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        var sql = @"UPDATE acc_collection_deposit
                        SET modified_time = @ModifiedTime, modified_by = @ModifiedBy,
                        server_modified_time = @ServerModifiedTime, approval_date = @ApprovalDate, approved_by_emp_uid = @ApprovedByEmpUID,
                        status = @Status, comments = @Comments WHERE request_no = @UID;";
                        Dictionary<string, object> parameters = new Dictionary<string, object>
                            {
                                {"Comments", accCollectionDeposit.Comments ?? ""},
                                {"status", Status },
                                {"ModifiedTime", DateTime.Now},
                                {"ModifiedBy", accCollectionDeposit.ModifiedBy ?? ""},
                                {"ApprovedByEmpUID", accCollectionDeposit.ApprovedByEmpUID},
                                {"ServerModifiedTime", DateTime.Now},
                                {"ApprovalDate", DateTime.Now },
                                {"UID", accCollectionDeposit.RequestNo}
                            };
                        Retval = await ExecuteNonQueryAsync(sql, conn, transaction, parameters);
                        if (Retval == 0)
                        {
                            transaction.Rollback();
                            return false;
                        }
                        var sql1 = @"UPDATE acc_collection
                        SET modified_time = @ModifiedTime, modified_by = @ModifiedBy,
                        server_modified_time = @ServerModifiedTime,
                        collection_deposit_status = @Status WHERE receipt_number = ANY(@UID);";
                        var receiptNumbers = JsonConvert.DeserializeObject<List<string>>(accCollectionDeposit.ReceiptNos ?? "[]");
                        dynamic param = new
                        {
                            UID = receiptNumbers,
                            Status = Status,
                            ModifiedTime = DateTime.Now,
                            ServerModifiedTime = DateTime.Now,
                            ModifiedBy = accCollectionDeposit.ModifiedBy,
                        };
                        Retval = await ExecuteNonQueryAsync(sql1, conn, transaction, param);
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
        Task<List<IPaymentSummary>> ICollectionModuleDL.GetPaymentSummary(string FromDate, string ToDate)
        {
            throw new NotImplementedException();
        }

        Task<bool> ICollectionModuleDL.CreateCashDepositRequest(IAccCollectionDeposit accCollectionDeposit)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateCollectionLimit(decimal Limit, string EmpUID, int Action)
        {
            try
            {
                int Retval = 0;
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        decimal limit = await CheckCollectionLimitForLoggedInUser(EmpUID);
                        limit += Limit;
                        var sql = @"Update job_position set collection_limit = @Limit where emp_uid = @EmpUID ";
                        Dictionary<string, object> parameters = new Dictionary<string, object>
                            {
                                {"Limit", limit },
                                {"EmpUID", EmpUID}
                            };
                        Retval = await ExecuteNonQueryAsync(sql, conn, transaction, parameters);
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
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<string>?> CheckIfUIDExistsInDB(string tableName, List<string> uIDs,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    {"@UIDs",  uIDs}
                };
                string sql = $@"SELECT uid AS UID FROM {tableName} WHERE uid = ANY(@UIDs)";
                return await ExecuteQueryAsync<string>(sql, parameters, null, connection, transaction);
            }
            catch (Exception ex)
            {
                throw;
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
                string query = @"INSERT INTO acc_payable (uid, ss, created_by, created_time, modified_by, modified_time, 
                        server_add_time, server_modified_time, source_type, source_uid, reference_number, 
                        org_uid, job_position_uid, amount, paid_amount, store_uid, transaction_date, 
                        due_date, unsettled_amount, source, currency_uid) 
                        VALUES (@UID, 0, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, 
                        @ServerAddTime, @ServerModifiedTime, @SourceType, @SourceUID, @ReferenceNumber, 
                        @OrgUID, @JobPositionUID, @Amount, @PaidAmount, @StoreUID, @TransactionDate, 
                        @DueDate, @UnSettledAmount, @Source, @CurrencyUID);
                        ";

                retValue = await ExecuteNonQueryAsync(query, connection, transaction, accPayables);

            }
            catch (Exception)
            {
                throw;
            }
            return retValue;
        }
        public async Task<bool> UpdateBankDetails(string UID, string BankName, string Branch, string ReferenceNumber)
        {
            try
            {
                int Retval = 0;
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        var sql = @"Update acc_collection_payment_mode Set bank_uid = @BankName, branch = @Branch, cheque_no = @ReferenceNumber
                                    Where uid = @UID";
                        Dictionary<string, object> parameters = new Dictionary<string, object>
                            {
                                {"UID", UID },
                                {"BankName", BankName },
                                {"Branch", Branch },
                                {"ReferenceNumber", ReferenceNumber },
                            };
                        Retval = await ExecuteNonQueryAsync(sql, conn, transaction, parameters);
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
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<PagedResponse<IAccCollection>> GetCollectionTabsDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, string PageName)
        {
            try
            {
                var sql = new StringBuilder();
                var ViewPaymentsql = new StringBuilder(@"select * from(SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                        (select code from emp where uid = acc_collection.created_by) AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                        server_modified_time AS ServerModifiedTime, receipt_number AS ReceiptNumber, 
                        consolidated_receipt_number AS ConsolidatedReceiptNumber, category AS Category, 
                        amount AS Amount, currency_uid AS CurrencyUID, default_currency_uid AS DefaultCurrencyUID, 
                        default_currency_exchange_rate AS DefaultCurrencyExchangeRate, default_currency_amount AS DefaultCurrencyAmount, 
                        org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, 
                        route_uid AS RouteUID, job_position_uid AS JobPositionUID, emp_uid AS EmpUID, 
                        collected_date AS CollectedDate, status AS Status, remarks AS Remarks, 
                        reference_number AS ReferenceNumber, is_realized AS IsRealized, latitude AS Latitude, 
                        longitude AS Longitude, source AS Source, is_multimode AS IsMultimode, 
                        trip_date AS TripDate, comments AS Comments, salesman AS Salesman, 
                        route AS Route, reversal_receipt_uid AS ReversalReceiptUID, cancelled_on AS CancelledOn FROM 
                        public.acc_collection) as SubQuery where ReceiptNumber not like 'R -%'");

                var NonCashsql = new StringBuilder(@"select * from(SELECT 
                            pm.id AS Id, pm.uid AS PmUID, pm.created_by AS CreatedBy, pm.created_time AS CreatedTime, 
                            pm.modified_by AS ModifiedBy, pm.modified_time AS ModifiedTime, pm.server_add_time AS ServerAddTime, 
                            pm.server_modified_time AS ServerModifiedTime, pm.acc_collection_uid AS AccCollectionUID, 
                            pm.bank_uid AS BankUID, pm.branch AS Branch, pm.cheque_no AS ChequeNo, pm.amount AS Amount, 
                            pm.currency_uid AS CurrencyUID, pm.default_currency_uid AS DefaultCurrencyUID, 
                            pm.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            pm.default_currency_amount AS DefaultCurrencyAmount, pm.cheque_date AS ChequeDate, 
                            pm.status AS Status, pm.realization_date AS RealizationDate, pm.comments AS Comments, 
                            pm.approve_comments AS ApproveComments, 
                            ac.id AS Id, ac.uid AS UID, ac.created_by AS CreatedBy, ac.created_time AS AcCreatedTime, 
                            ac.modified_by AS ModifiedBy, ac.modified_time AS ModifiedTime, ac.server_add_time AS ServerAddTime, 
                            ac.server_modified_time AS ServerModifiedTime, ac.receipt_number AS ReceiptNumber, 
                            ac.consolidated_receipt_number AS ConsolidatedReceiptNumber, ac.category AS Category, 
                            ac.amount AS Amount, ac.currency_uid AS CurrencyUID, ac.default_currency_uid AS DefaultCurrencyUID, 
                            ac.default_currency_exchange_rate AS DefaultCurrencyExchangeRate, 
                            ac.default_currency_amount AS DefaultCurrencyAmount, ac.org_uid AS OrgUID, 
                            ac.distribution_channel_uid AS DistributionChannelUID, ac.store_uid AS StoreUID, 
                            ac.route_uid AS RouteUID, ac.job_position_uid AS JobPositionUID, ac.emp_uid AS EmpUID, 
                            ac.collected_date AS CollectedDate, ac.status AS AcStatus, ac.remarks AS Remarks, 
                            ac.reference_number AS ReferenceNumber, ac.is_realized AS IsRealized, ac.latitude AS Latitude, 
                            ac.longitude AS Longitude, ac.source AS Source, ac.is_multimode AS IsMultimode, 
                            ac.trip_date AS TripDate, ac.comments AS Comments, ac.salesman AS Salesman, 
                            ac.route AS Route, ac.reversal_receipt_uid AS ReversalReceiptUID, 
                            ac.cancelled_on AS CancelledOn
                        FROM 
                            public.acc_collection_payment_mode AS pm
                        INNER JOIN 
                            public.acc_collection AS ac 
                        ON 
                            ac.uid = pm.acc_collection_uid)as SubQuery where Category != 'Cash' and UID not like 'R -%'");

                var Cashsql = new StringBuilder(@"select * from(select ac.uid as UID, ac.currency_uid as CurrencyUID, ac.receipt_number as ReceiptNumber, ac.status as Status, ac.comments as Comments,
                            ac.created_time as CollectedDate, ac.category as Category, ac.amount as Amount, ac.default_currency_amount as DefaultCurrencyAmount, '('||s.code ||')' || s.name as StoreUID,s.name as storeName, ac.created_time as CreatedTime 
                            from acc_collection as ac join store as s on ac.store_uid = s.uid) as SubQuery where Category = 'Cash'");
                switch (PageName)
                {
                    case "ViewPayments":
                        sql = ViewPaymentsql;
                        break;
                    case "CashSettlement":
                        sql = Cashsql;
                        break;
                    case "NonCashSettlement":
                        sql = NonCashsql;
                        break;
                }
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                };
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" And ");
                    AppendFilterCriteria<IAccCollection>
                        (filterCriterias, sbFilterCriteria, parameters); ;
                    sql.Append(sbFilterCriteria);
                }
                else
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" And Status not in ('Reversed','Voided','Rejected', 'Bounced')");
                    sql.Append(sbFilterCriteria);
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql, true);
                }

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAccCollection>().GetType();
                List<IAccCollection> CollectionModuleList = await ExecuteQueryAsync<IAccCollection>(sql.ToString(), parameters, type);
                PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection> pagedResponse = new PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>
                {
                    PagedData = CollectionModuleList,
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<PagedResponse<Model.Interfaces.IStoreStatement>> StoreStatementRecords(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string StartDate, string EndDate)
        {
            try
            {
                var sql = new StringBuilder(@"select * from(SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                                            modified_by AS ModifiedBy, modified_time AS ModifiedTime, 
                                            server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, 
                                            ss AS Ss, org_uid AS OrgUID, store_uid AS StoreUID, 
                                            transaction_type AS TransactionType, source_type AS SourceType, 
                                            source_uid AS SourceUID, document_number AS DocumentNumber, 
                                            opening_balance AS OpeningBalance, amount AS Amount, 
                                            closing_balance AS ClosingBalance, transaction_date_time AS TransactionDateTime
                                            FROM dbo.store_statement) as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"select COUNT(1) AS Cnt  from (SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                                            modified_by AS ModifiedBy, modified_time AS ModifiedTime, 
                                            server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, 
                                            ss AS Ss, org_uid AS OrgUID, store_uid AS StoreUID, 
                                            transaction_type AS TransactionType, source_type AS SourceType, 
                                            source_uid AS SourceUID, document_number AS DocumentNumber, 
                                            opening_balance AS OpeningBalance, amount AS Amount, 
                                            closing_balance AS ClosingBalance, transaction_date_time AS TransactionDateTime
                                            FROM dbo.store_statement)as SubQuery");
                }
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                };
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" And ");
                    AppendFilterCriteria<IAccCollection>
                        (filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreStatement>().GetType();
                IEnumerable<Model.Interfaces.IStoreStatement> CollectionModuleList = await ExecuteQueryAsync<Model.Interfaces.IStoreStatement>(sql.ToString(), parameters, type);
                int totalCount = Const.Num;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IStoreStatement> pagedResponse = new PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IStoreStatement>
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
        public async Task<bool> UpdateBalanceConfirmation(IBalanceConfirmation balanceConfirmation)
        {
            try
            {
                int Retval = 0;
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        var sql = @"Update balance_confirmation Set generated_time = @GeneratedTime, status = @Status, otp = @Otp, confirm_request_time = @ConfirmRequestTime , confirm_competion_time = @ConfirmCompetionTime
                                    request_by_job_position_uid = @RequestByJobPositionUID , request_by_emp_uid = @RequestByEmpUID
                                    Where uid = @UID";
                        Dictionary<string, object> parameters = new Dictionary<string, object>
                            {
                                {"UID", balanceConfirmation.UID },
                                {"GeneratedTime", balanceConfirmation.GeneratedOn },
                                {"Status", balanceConfirmation.Status ?? "" },
                                {"Otp", balanceConfirmation.OTPCode ?? "" },
                                {"ConfirmRequestTime", balanceConfirmation.ConfirmationOrDisputeRequestTime },
                                {"ConfirmCompetionTime", balanceConfirmation.ConfirmationRequestTimeOrDisputeConfirmationTime },
                                {"RequestByJobPositionUID", balanceConfirmation.RequestByJobPositionUID ?? ""},
                                {"RequestByEmpUID", balanceConfirmation.RequestByEmpUID ?? "" }
                            };
                        Retval = await ExecuteNonQueryAsync(sql, conn, transaction, parameters);
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
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<bool> InsertDisputeRecords(List<IBalanceConfirmationLine> balanceConfirmationLine)
        {
            try
            {
                int Retval = 0;
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        foreach (var data in balanceConfirmationLine)
                        {
                            var sql = @"INSERT INTO dbo.balance_confirmation_line 
                                   (uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                                    server_modified_time, ss, balance_confirmation_uid, line_number, scheme_name, 
                                    eligible_amount, received_amount, description, diff_amount, comments) 
                                    VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                                    @ServerModifiedTime, @Ss, @BalanceConfirmationUID, @LineNumber, @SchemeName, 
                                    @EligibleAmount, @ReceivedAmount, @Description, @DiffAmount, @Comments);";
                            Dictionary<string, object> parameters = new Dictionary<string, object>
                        {
                            {"UID", data.UID },
                            {"CreatedBy", data.CreatedBy },
                            {"CreatedTime", data.CreatedTime },
                            {"ModifiedBy", data.ModifiedBy },
                            {"ModifiedTime", data.ModifiedTime },
                            {"ServerAddTime", data.ServerAddTime },
                            {"ServerModifiedTime", data.ServerModifiedTime },
                            {"Ss", 0 },
                            {"BalanceConfirmationUID", "41E744A6-BA74-4969-A1D4-ECCD4AFAA737" },
                            {"LineNumber", data.LineNumber },
                            {"SchemeName", data.SchemeName ?? "" },
                            {"EligibleAmount", data.EligibleAmount },
                            {"ReceivedAmount", data.ReceivedAmount },
                            {"Description", data.Description ?? "" },
                            {"DiffAmount", data.DisputeAmount },
                        };

                            Retval = await ExecuteNonQueryAsync(sql, conn, transaction, parameters);
                            if (Retval == 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
                        }
                        transaction.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<IBalanceConfirmation> GetBalanceConfirmationDetails()
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                };


                var sql = @"SELECT top 1 id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                           modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                           server_modified_time AS ServerModifiedTime, ss AS Ss, org_uid AS OrgUid, store_uid AS StoreUid, 
                           generated_time AS GeneratedTime, start_date AS StartDate, end_date AS EndDate, 
                           opening_balance AS OpeningBalance, debit_amount AS DebitAmount, credit_amount AS CreditAmount, 
                           closing_balance AS ClosingBalance, dispute_amount AS DisputeAmount, status AS Status, 
                           otp AS Otp, confirm_request_time AS ConfirmRequestTime, confirm_competion_time AS ConfirmCompetionTime, 
                           request_by_job_position_uid AS RequestByJobPositionUid, request_by_emp_uid AS RequestByEmpUid, 
                           dispute_confirmation_by_job_position_uid AS DisputeConfirmationByJobPositionUid, 
                           dispute_confirmation_by_emp_uid AS DisputeConfirmationByEmpUid
                          FROM balance_confirmation";
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IBalanceConfirmation>().GetType();
                Model.Interfaces.IBalanceConfirmation CollectionModuleList = await ExecuteSingleAsync<Model.Interfaces.IBalanceConfirmation>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                return new BalanceConfirmation();
            }
        }
        public async Task<List<IBalanceConfirmationLine>> GetBalanceConfirmationLineDetails(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                };


                var sql = @"SELECT id AS Id, uid AS Uid, created_by AS CreatedBy, created_time AS CreatedTime, 
                modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                server_modified_time AS ServerModifiedTime, ss AS Ss, balance_confirmation_uid AS BalanceConfirmationUid, 
                line_number AS LineNumber, scheme_name AS SchemeName, eligible_amount AS EligibleAmount, 
                received_amount AS ReceivedAmount, description AS Description, diff_amount AS DiffAmount
                FROM dbo.balance_confirmation_line";
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IBalanceConfirmationLine>().GetType();
                List<IBalanceConfirmationLine> CollectionModuleList = await ExecuteQueryAsync<IBalanceConfirmationLine>(sql, parameters, type);
                return CollectionModuleList;
            }
            catch (Exception ex)
            {
                return new List<IBalanceConfirmationLine>();
            }
        }
        public async Task<bool> UpdateBalanceConfirmationForResolvingDispute(IBalanceConfirmation balanceConfirmationLine)
        {
            try
            {
                int Retval = 0;
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        var sql = @"Update balance_confirmation Set status = @Status, comments = @Comments 
                                    Where uid = @UID";
                        Dictionary<string, object> parameters = new Dictionary<string, object>
                            {
                                {"UID", balanceConfirmationLine.UID },
                                {"Status", balanceConfirmationLine.Status ?? "" },
                                {"Status", balanceConfirmationLine.Comments ?? "" },
                            };
                        Retval = await ExecuteNonQueryAsync(sql, conn, transaction, parameters);
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
            catch (Exception ex)
            {
                return false;
            }
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

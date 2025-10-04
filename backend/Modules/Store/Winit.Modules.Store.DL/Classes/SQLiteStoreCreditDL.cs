using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.DL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Store.DL.Classes
{
    public class SQLiteStoreCreditDL : Base.DL.DBManager.SqliteDBManager, Interfaces.IStoreCreditDL
    {
        public SQLiteStoreCreditDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public async Task<PagedResponse<Model.Interfaces.IStoreCredit>> SelectAllStoreCredit(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder("SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, " +
                    "server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, store_uid AS StoreUID, payment_term_uid AS PaymentTermUID, credit_type AS CreditType, " +
                    "credit_limit AS CreditLimit, temporary_credit AS TemporaryCredit, org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, preferred_payment_mode AS PreferredPaymentMode, " +
                    "is_active AS IsActive, is_blocked AS IsBlocked, blocking_reason_code AS BlockingReasonCode, blocking_reason_description AS BlockingReasonDescription, price_list AS PriceList, " +
                    "authorized_item_grp_key AS AuthorizedItemGrpKey, message_key AS MessageKey, tax_key_field AS TaxKeyField, promotion_key AS PromotionKey, disabled AS Disabled, " +
                    "bill_to_address_uid AS BillToAddressUid, ship_to_address_uid AS ShipToAddressUid, outstanding_invoices AS OutstandingInvoices, preferred_payment_method AS PreferredPaymentMethod, " +
                    "payment_type AS PaymentType, invoice_admin_fee_per_billing_cycle AS InvoiceAdminFeePerBillingCycle, invoice_admin_fee_per_delivery AS InvoiceAdminFeePerDelivery, " +
                    "invoice_late_payment_fee AS InvoiceLatePaymentFee, is_cancellation_of_invoice_allowed AS IsCancellationOfInvoiceAllowed, is_allow_cash_on_credit_exceed AS IsAllowCashOnCreditExceed, " +
                    "is_outstanding_bill_control AS IsOutstandingBillControl, is_negative_invoice_allowed AS IsNegativeInvoiceAllowed FROM store_credit;");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("SELECT COUNT(1) AS Cnt FROM store_credit");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);

                    sql.Append(sbFilterCriteria);
                    // If count required then add filters to count
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
                    if (sortCriterias != null && sortCriterias.Count > 0)
                    {
                        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                //Data
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreCredit>().GetType();
                IEnumerable<Model.Interfaces.IStoreCredit> StoreCredits = await ExecuteQueryAsync<Model.Interfaces.IStoreCredit>(sql.ToString(), parameters, type);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreCredit> pagedResponse = new PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreCredit>
                {
                    PagedData = StoreCredits,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Model.Interfaces.IStoreCredit> SelectStoreCreditByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, store_uid AS StoreUID, payment_term_uid AS PaymentTermUID, credit_type AS CreditType, 
                        credit_limit AS CreditLimit, temporary_credit AS TemporaryCredit, org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, preferred_payment_mode AS PreferredPaymentMode,
                        is_active AS IsActive, is_blocked AS IsBlocked, blocking_reason_code AS BlockingReasonCode, blocking_reason_description AS BlockingReasonDescription, price_list AS PriceList,
                        authorized_item_grp_key AS AuthorizedItemGrpKey, message_key AS MessageKey, tax_key_field AS TaxKeyField, promotion_key AS PromotionKey, disabled AS Disabled,
                        bill_to_address_uid AS BillToAddressUid, ship_to_address_uid AS ShipToAddressUid, outstanding_invoices AS OutstandingInvoices, preferred_payment_method AS PreferredPaymentMethod,
                        payment_type AS PaymentType, invoice_admin_fee_per_billing_cycle AS InvoiceAdminFeePerBillingCycle, invoice_admin_fee_per_delivery AS InvoiceAdminFeePerDelivery,
                        invoice_late_payment_fee AS InvoiceLatePaymentFee, is_cancellation_of_invoice_allowed AS IsCancellationOfInvoiceAllowed, is_allow_cash_on_credit_exceed AS IsAllowCashOnCreditExceed,
                        is_outstanding_bill_control AS IsOutstandingBillControl, is_negative_invoice_allowed AS IsNegativeInvoiceAllowed 
                        FROM store_credit WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreCredit>().GetType();
            Model.Interfaces.IStoreCredit StoreCreditList = await ExecuteSingleAsync<Model.Interfaces.IStoreCredit>(sql, parameters, type);
            return StoreCreditList;
        }

        public async Task<int> CreateStoreCredit(Model.Interfaces.IStoreCredit storeCredit)
        {
            try
            {
                var sql = @"INSERT INTO store_credit (uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                            server_modified_time, store_uid, payment_term_uid, credit_type, credit_limit, temporary_credit, org_uid, distribution_channel_uid, 
                            preferred_payment_mode, is_active, is_blocked, blocking_reason_code, blocking_reason_description, price_list, authorized_item_grp_key, 
                            message_key, tax_key_field, promotion_key, disabled, bill_to_address_uid, ship_to_address_uid, outstanding_invoices, preferred_payment_method, 
                            payment_type,invoice_admin_fee_per_billing_cycle, invoice_admin_fee_per_delivery, invoice_late_payment_fee, is_cancellation_of_invoice_allowed,
                            is_allow_cash_on_credit_exceed, is_outstanding_bill_control, is_negative_invoice_allowed) VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy,
                            @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @StoreUID, @PaymentTermUID, @CreditType, @CreditLimit, @TemporaryCredit, 
                            @OrgUID, @DistributionChannelUID, @PreferredPaymentMode, @IsActive, @IsBlocked, @BlockingReasonCode, @BlockingReasonDescription, 
                            @PriceList, @AuthorizedItemGRPKey, @MessageKey, @TaxKeyField, @PromotionKey, @Disabled, @BillToAddressUID, @ShipToAddressUID, 
                            @OutstandingInvoices, @PreferredPaymentMethod, @PaymentType, @InvoiceAdminFeePerBillingCycle, @InvoiceAdminFeePerDelivery,
                            @InvoiceLatePaymentFee, @IsCancellationOfInvoiceAllowed, @IsAllowCashOnCreditExceed, @IsOutstandingBillControl, @IsNegativeInvoiceAllowed)";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                      { "@UID", storeCredit.UID },
                   { "@CreatedBy", storeCredit.CreatedBy },
                   { "@CreatedTime", storeCredit.CreatedTime },
                   { "@ModifiedBy", storeCredit.ModifiedBy },
                   { "@ModifiedTime", storeCredit.ModifiedTime },
                   { "@ServerAddTime", storeCredit.ServerAddTime },
                   { "@ServerModifiedTime", storeCredit.ServerModifiedTime },
                   { "@StoreUID", storeCredit.StoreUID },
                   { "@PaymentTermUID", storeCredit.PaymentTermUID },
                   { "@CreditType", storeCredit.CreditType },
                   { "@CreditLimit", storeCredit.CreditLimit },
                   { "@TemporaryCredit", storeCredit.TemporaryCredit },
                   { "@OrgUID", storeCredit.OrgUID },
                   { "@DistributionChannelUID", storeCredit.DistributionChannelUID },
                   { "@PreferredPaymentMode", storeCredit.PreferredPaymentMode },
                   { "@IsActive", storeCredit.IsActive },
                   { "@IsBlocked", storeCredit.IsBlocked },
                   { "@BlockingReasonCode", storeCredit.BlockingReasonCode },
                   { "@BlockingReasonDescription", storeCredit.BlockingReasonDescription },
                   { "@PriceList", storeCredit.PriceList },
                   { "@AuthorizedItemGRPKey", storeCredit.AuthorizedItemGRPKey },
                   { "@MessageKey", storeCredit.MessageKey },
                   { "@TaxKeyField", storeCredit.TaxKeyField },
                   { "@PromotionKey", storeCredit.PromotionKey },
                   { "@Disabled", storeCredit.Disabled },
                   { "@BillToAddressUID", storeCredit.BillToAddressUID },
                   { "@ShipToAddressUID", storeCredit.ShipToAddressUID },
                   { "@OutstandingInvoices", storeCredit.OutstandingInvoices },
                   { "@PreferredPaymentMethod", storeCredit.PreferredPaymentMethod },
                   { "@PaymentType", storeCredit.PaymentType },
                   { "@InvoiceAdminFeePerBillingCycle", storeCredit.InvoiceAdminFeePerBillingCycle },
                   { "@InvoiceAdminFeePerDelivery", storeCredit.InvoiceAdminFeePerDelivery },
                   { "@InvoiceLatePaymentFee", storeCredit.InvoiceLatePaymentFee },
                   { "@IsCancellationOfInvoiceAllowed", storeCredit.IsCancellationOfInvoiceAllowed },
                   { "@IsAllowCashOnCreditExceed", storeCredit.IsAllowCashOnCreditExceed },
                   { "@IsOutstandingBillControl", storeCredit.IsOutstandingBillControl },
                   { "@IsNegativeInvoiceAllowed", storeCredit.IsNegativeInvoiceAllowed },

                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateStoreCredit(Model.Interfaces.IStoreCredit storeCredit)
        {
            try
            {
                var sql = @"UPDATE store_credit SET 
                            modified_by = @ModifiedBy, 
                            modified_time = @ModifiedTime, 
                            server_modified_time = @ServerModifiedTime, 
                            credit_type = @CreditType, 
                            credit_limit = @CreditLimit, 
                            temporary_credit = @TemporaryCredit, 
                            preferre_payment_mode = @PreferredPaymentMode, 
                            is_active = @IsActive, 
                            is_blocked = @IsBlocked, 
                            blocking_reason_code = @BlockingReasonCode, 
                            blocking_reason_description = @BlockingReasonDescription, 
                            price_list = @PriceList, 
                            authorized_item_grp_key = @AuthorizedItemGRPKey, 
                            message_key = @MessageKey, 
                            tax_key_field = @TaxKeyField, 
                            promotion_key = @PromotionKey, 
                            disabled = @Disabled, 
                            outstanding_invoices = @OutstandingInvoices, 
                            preferred_payment_method = @PreferredPaymentMethod, 
                            payment_type = @PaymentType, 
                            invoice_admin_fee_per_billing_cycle = @InvoiceAdminFeePerBillingCycle, 
                            invoice_admin_fee_per_delivery = @InvoiceAdminFeePerDelivery, 
                            invoice_late_payment_fee = @InvoiceLatePaymentFee, 
                            is_cancellation_of_invoice_allowed = @IsCancellationOfInvoiceAllowed, 
                            is_allow_cash_on_credit_exceed = @IsAllowCashOnCreditExceed, 
                            is_outstanding_bill_control = @IsOutstandingBillControl, 
                            is_negative_invoice_allowed = @IsNegativeInvoiceAllowed 
                             WHERE uid = @UID;";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   { "@UID", storeCredit.UID },
                   { "@ModifiedBy", storeCredit.ModifiedBy },
                   { "@ModifiedTime", storeCredit.ModifiedTime },
                   { "@ServerModifiedTime",DateTime.Now },
                   { "@CreditType", storeCredit.CreditType },
                   { "@CreditLimit", storeCredit.CreditLimit },
                   { "@TemporaryCredit", storeCredit.TemporaryCredit },
                   { "@PreferredPaymentMode", storeCredit.PreferredPaymentMode },
                   { "@IsActive", storeCredit.IsActive },
                   { "@IsBlocked", storeCredit.IsBlocked },
                   { "@BlockingReasonCode", storeCredit.BlockingReasonCode },
                   { "@BlockingReasonDescription", storeCredit.BlockingReasonDescription },
                   { "@PriceList", storeCredit.PriceList },
                   { "@AuthorizedItemGRPKey", storeCredit.AuthorizedItemGRPKey },
                   { "@MessageKey", storeCredit.MessageKey },
                   { "@TaxKeyField", storeCredit.TaxKeyField },
                   { "@PromotionKey", storeCredit.PromotionKey },
                   { "@Disabled", storeCredit.Disabled },
                   { "@OutstandingInvoices", storeCredit.OutstandingInvoices },
                   { "@PreferredPaymentMethod", storeCredit.PreferredPaymentMethod },
                   { "@PaymentType", storeCredit.PaymentType },
                   { "@InvoiceAdminFeePerBillingCycle", storeCredit.InvoiceAdminFeePerBillingCycle },
                   { "@InvoiceAdminFeePerDelivery", storeCredit.InvoiceAdminFeePerDelivery },
                   { "@InvoiceLatePaymentFee", storeCredit.InvoiceLatePaymentFee },
                   { "@IsCancellationOfInvoiceAllowed", storeCredit.IsCancellationOfInvoiceAllowed },
                   { "@IsAllowCashOnCreditExceed", storeCredit.IsAllowCashOnCreditExceed },
                   { "@IsOutstandingBillControl", storeCredit.IsOutstandingBillControl },
                   { "@IsNegativeInvoiceAllowed", storeCredit.IsNegativeInvoiceAllowed },

                };
                return  await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteStoreCredit(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",UID}
            };
            var sql = "DELETE  FROM store_credit WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        public async Task<Model.Interfaces.IStoreCredit> SelectStoreCreditByStoreUID(string StoreUID)
        {
            throw new NotImplementedException();
        }

        public Task<List<IStoreCreditLimit>> GetCurrentLimitByStoreAndDivision(List<string> storeUIDs, string divisionUID)
        {
            throw new NotImplementedException();
        }
        public Task<List<IPurchaseOrderCreditLimitBufferRange>> GetPurchaseOrderCreditLimitBufferRanges()
        {
            throw new NotImplementedException();
        }


        Task<int> IStoreCreditDL.UpdateStoreCreditStatus(List<IStoreCredit> storeCredit)
        {
            throw new NotImplementedException();
        }

        
    }
}









using Microsoft.Extensions.Configuration;
using System.Text;
using Winit.Modules.Store.DL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Store.DL.Classes;

public class MSSQLStoreCreditDL : Base.DL.DBManager.SqlServerDBManager, Interfaces.IStoreCreditDL
{
    public MSSQLStoreCreditDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {
    }
    public async Task<PagedResponse<Model.Interfaces.IStoreCredit>> SelectAllStoreCredit(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {

            StringBuilder sql = new(@"SELECT * 
                                                FROM (
                                                    SELECT sc.id AS Id, sc.uid AS UID, sc.created_by AS CreatedBy, sc.created_time AS CreatedTime, 
                                                           sc.modified_by AS ModifiedBy, sc.modified_time AS ModifiedTime, sc.server_add_time AS ServerAddTime,
                                                           sc.server_modified_time AS ServerModifiedTime, sc.store_uid AS StoreUID, sc.payment_term_uid AS PaymentTermUID,
                                                           sc.credit_type AS CreditType, sc.credit_limit AS CreditLimit, sc.temporary_credit AS TemporaryCredit,
                                                           sc.org_uid AS OrgUID, sc.distribution_channel_uid AS DistributionChannelUID, sc.preferred_payment_mode AS PreferredPaymentMode,
                                                           sc.is_active AS IsActive, sc.is_blocked AS IsBlocked, sc.blocking_reason_code AS BlockingReasonCode, 
                                                           sc.blocking_reason_description AS BlockingReasonDescription, sc.price_list AS PriceList, 
                                                           sc.authorized_item_grp_key AS AuthorizedItemGRPKey,sc.message_key AS MessageKey, sc.tax_key_field AS TaxKeyField, 
                                                           sc.promotion_key AS PromotionKey, sc.disabled AS Disabled, 
                                                           sc.bill_to_address_uid AS BillToAddressUID, sc.ship_to_address_uid AS ShipToAddressUID, 
                                                           sc.outstanding_invoices AS OutstandingInvoices,
                                                           sc.preferred_payment_method AS PreferredPaymentMethod, sc.payment_type AS PaymentType,
                                                           sc.invoice_admin_fee_per_billing_cycle AS InvoiceAdminFeePerBillingCycle,
                                                           sc.invoice_admin_fee_per_delivery AS InvoiceAdminFeePerDelivery, sc.invoice_late_payment_fee AS InvoiceLatePaymentFee,
                                                           sc.is_cancellation_of_invoice_allowed AS IsCancellationOfInvoiceAllowed, 
                                                           sc.is_allow_cash_on_credit_exceed AS IsAllowCashOnCreditExceed, 
                                                           sc.is_outstanding_bill_control AS IsOutstandingBillControl, sc.is_negative_invoice_allowed AS IsNegativeInvoiceAllowed,
                                                           sc.credit_days AS CreditDays,sc.temporary_credit_days AS TemporaryCreditDays,sc.division_org_uid AS DivisionOrgUID,
                                                           sc.temporary_credit_approval_date AS TemporaryCreditApprovalDate,
                                                           (select name from emp where uid = sc.division_org_uid) as AsmEmpName,
						                                   (select name from org where uid = sc.division_org_uid) as DivisionName 
                                                    FROM store_credit sc
                                                ) AS subquery");
            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT sc.id AS Id, sc.uid AS UID, sc.created_by AS CreatedBy, sc.created_time AS CreatedTime, 
                                                           sc.modified_by AS ModifiedBy, sc.modified_time AS ModifiedTime, sc.server_add_time AS ServerAddTime,
                                                           sc.server_modified_time AS ServerModifiedTime, sc.store_uid AS StoreUID, sc.payment_term_uid AS PaymentTermUID,
                                                           sc.credit_type AS CreditType, sc.credit_limit AS CreditLimit, sc.temporary_credit AS TemporaryCredit,
                                                           sc.org_uid AS OrgUID, sc.distribution_channel_uid AS DistributionChannelUID, sc.preferred_payment_mode AS PreferredPaymentMode,
                                                           sc.is_active AS IsActive, sc.is_blocked AS IsBlocked, sc.blocking_reason_code AS BlockingReasonCode, 
                                                           sc.blocking_reason_description AS BlockingReasonDescription, sc.price_list AS PriceList, 
                                                           sc.authorized_item_grp_key AS AuthorizedItemGRPKey,sc.message_key AS MessageKey, sc.tax_key_field AS TaxKeyField, 
                                                           sc.promotion_key AS PromotionKey, sc.disabled AS Disabled, 
                                                           sc.bill_to_address_uid AS BillToAddressUID, sc.ship_to_address_uid AS ShipToAddressUID, 
                                                           sc.outstanding_invoices AS OutstandingInvoices,
                                                           sc.preferred_payment_method AS PreferredPaymentMethod, sc.payment_type AS PaymentType,
                                                           sc.invoice_admin_fee_per_billing_cycle AS InvoiceAdminFeePerBillingCycle,
                                                           sc.invoice_admin_fee_per_delivery AS InvoiceAdminFeePerDelivery, sc.invoice_late_payment_fee AS InvoiceLatePaymentFee,
                                                           sc.is_cancellation_of_invoice_allowed AS IsCancellationOfInvoiceAllowed, 
                                                           sc.is_allow_cash_on_credit_exceed AS IsAllowCashOnCreditExceed, 
                                                           sc.is_outstanding_bill_control AS IsOutstandingBillControl, sc.is_negative_invoice_allowed AS IsNegativeInvoiceAllowed,
                                                           sc.credit_days AS CreditDays,sc.temporary_credit_days AS TemporaryCreditDays,sc.division_org_uid AS DivisionOrgUID,
                                                           sc.temporary_credit_approval_date AS TemporaryCreditApprovalDate
                                                    FROM store_credit sc) as subquery");
            }
            Dictionary<string, object> parameters = [];

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<Model.Interfaces.IStoreCredit>(filterCriterias, sbFilterCriteria, parameters);

                _ = sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    _ = sqlCount.Append(sbFilterCriteria);
                }
            }

            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                _ = sql.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sql);
            }

            if (pageNumber > 0 && pageSize > 0)
            {
                _ = sortCriterias != null && sortCriterias.Count > 0
                    ? sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY")
                    : sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            }
            IEnumerable<Model.Interfaces.IStoreCredit> StoreCredits = await ExecuteQueryAsync<Model.Interfaces.IStoreCredit>(sql.ToString(), parameters);
            int totalCount = 0;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreCredit> pagedResponse = new()
            {
                PagedData = StoreCredits, TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public async Task<Model.Interfaces.IStoreCredit> SelectStoreCreditByUID(string UID)
    {
        Dictionary<string, object> parameters = new()
        {
            {
                "UID", UID
            }
        };

        string sql = @"SELECT 
                                            sc.id AS Id, 
                                            sc.uid AS UID, 
                                            sc.created_by AS CreatedBy, 
                                            sc.created_time AS CreatedTime, 
                                            sc.modified_by AS ModifiedBy, 
                                            sc.modified_time AS ModifiedTime, 
                                            sc.server_add_time AS ServerAddTime, 
                                            sc.server_modified_time AS ServerModifiedTime, 
                                            sc.store_uid AS StoreUID, 
                                            sc.payment_term_uid AS PaymentTermUID, 
                                            sc.credit_type AS CreditType, 
                                            sc.credit_limit AS CreditLimit, 
                                            sc.temporary_credit AS TemporaryCredit, 
                                            sc.org_uid AS OrgUID, 
                                            sc.distribution_channel_uid AS DistributionChannelUID, 
                                            sc.preferred_payment_mode AS PreferredPaymentMode, 
                                            sc.is_active AS IsActive, 
                                            sc.is_blocked AS IsBlocked, 
                                            sc.blocking_reason_code AS BlockingReasonCode, 
                                            sc.blocking_reason_description AS BlockingReasonDescription, 
                                            sc.price_list AS PriceList, 
                                            sc.authorized_item_grp_key AS AuthorizedItemGRPKey, 
                                            sc.message_key AS MessageKey, 
                                            sc.tax_key_field AS TaxKeyField, 
                                            sc.promotion_key AS PromotionKey, 
                                            sc.disabled AS Disabled, 
                                            sc.bill_to_address_uid AS BillToAddressUID, 
                                            sc.ship_to_address_uid AS ShipToAddressUID, 
                                            sc.outstanding_invoices AS OutstandingInvoices, 
                                            sc.preferred_payment_method AS PreferredPaymentMethod, 
                                            sc.payment_type AS PaymentType, 
                                            sc.invoice_admin_fee_per_billing_cycle AS InvoiceAdminFeePerBillingCycle, 
                                            sc.invoice_admin_fee_per_delivery AS InvoiceAdminFeePerDelivery, 
                                            sc.invoice_late_payment_fee AS InvoiceLatePaymentFee, 
                                            sc.is_cancellation_of_invoice_allowed AS IsCancellationOfInvoiceAllowed, 
                                            sc.is_allow_cash_on_credit_exceed AS IsAllowCashOnCreditExceed, 
                                            sc.is_outstanding_bill_control AS IsOutstandingBillControl, 
                                            sc.is_negative_invoice_allowed AS IsNegativeInvoiceAllowed,
                                            sc.credit_days AS CreditDays,
                                            sc.temporary_credit_days AS TemporaryCreditDays,
                                            sc.division_org_uid AS DivisionOrgUID,
                                            sc.temporary_credit_approval_date AS TemporaryCreditApprovalDate
                                        FROM 
                                            store_credit sc 
                                        WHERE 
                                            sc.uid = @UID";

        return await ExecuteSingleAsync<Model.Interfaces.IStoreCredit>(sql, parameters);

    }

    public async Task<int> CreateStoreCredit(Model.Interfaces.IStoreCredit storeCredit)
    {
        string sql = @"INSERT INTO store_credit ( id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                            store_uid, payment_term_uid,credit_type, credit_limit, temporary_credit, org_uid, distribution_channel_uid, preferred_payment_mode,
                            is_active, is_blocked, blocking_reason_code, blocking_reason_description, price_list, authorized_item_grp_key, message_key,
                            tax_key_field,promotion_key, disabled, bill_to_address_uid, ship_to_address_uid, outstanding_invoices, preferred_payment_method,
                            payment_type, invoice_admin_fee_per_billing_cycle, invoice_admin_fee_per_delivery, invoice_late_payment_fee, is_cancellation_of_invoice_allowed,
                            is_allow_cash_on_credit_exceed, is_outstanding_bill_control, is_negative_invoice_allowed, store_group_data_uid,
                            credit_days,temporary_credit_days,division_org_uid,temporary_credit_approval_date)
                            VALUES (
                             @Id, @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,
                            @StoreUID, @PaymentTermUID, @CreditType, @CreditLimit, @TemporaryCredit,
                            @OrgUID, @DistributionChannelUID, @PreferredPaymentMode, @IsActive, @IsBlocked,
                            @BlockingReasonCode, @BlockingReasonDescription,@PriceList, @AuthorizedItemGRPKey,
                            @MessageKey, @TaxKeyField, @PromotionKey, @Disabled, @BillToAddressUID, @ShipToAddressUID,
                            @OutstandingInvoices, @PreferredPaymentMethod, @PaymentType,
                            @InvoiceAdminFeePerBillingCycle, @InvoiceAdminFeePerDelivery, @InvoiceLatePaymentFee,
                            @IsCancellationOfInvoiceAllowed, @IsAllowCashOnCreditExceed, @IsOutstandingBillControl,
                            @IsNegativeInvoiceAllowed, @StoreGroupDataUID,@CreditDays,@TemporaryCreditDays,@DivisionOrgUID,@TemporaryCreditApprovalDate);";


        return await ExecuteNonQueryAsync(sql, storeCredit);
    }
    public async Task<int> UpdateStoreCredit(Model.Interfaces.IStoreCredit storeCredit)
    {
        string sql = @"UPDATE store_credit SET modified_by = @ModifiedBy, modified_time = @ModifiedTime, server_modified_time = @ServerModifiedTime, 
                            store_uid = @StoreUID, payment_term_uid = @PaymentTermUID, credit_type = @CreditType, credit_limit = @CreditLimit,
                            temporary_credit = @TemporaryCredit, org_uid = @OrgUID, distribution_channel_uid = @DistributionChannelUID,
                            preferred_payment_mode = @PreferredPaymentMode, is_active = @IsActive, is_blocked = @IsBlocked, blocking_reason_code = @BlockingReasonCode,
                            blocking_reason_description = @BlockingReasonDescription, price_list = @PriceList, authorized_item_grp_key = @AuthorizedItemGRPKey,
                            message_key = @MessageKey, tax_key_field = @TaxKeyField, promotion_key = @PromotionKey, disabled = @Disabled,
                            bill_to_address_uid = @BillToAddressUID, ship_to_address_uid = @ShipToAddressUID, outstanding_invoices = @OutstandingInvoices,
                            preferred_payment_method = @PreferredPaymentMethod, payment_type = @PaymentType, invoice_admin_fee_per_billing_cycle = @InvoiceAdminFeePerBillingCycle,
                            invoice_admin_fee_per_delivery = @InvoiceAdminFeePerDelivery,
                            invoice_late_payment_fee = @InvoiceLatePaymentFee, is_cancellation_of_invoice_allowed = @IsCancellationOfInvoiceAllowed,
                            is_allow_cash_on_credit_exceed = @IsAllowCashOnCreditExceed, is_outstanding_bill_control = @IsOutstandingBillControl,
                            is_negative_invoice_allowed = @IsNegativeInvoiceAllowed,credit_days=@CreditDays,temporary_credit_days=@TemporaryCreditDays,
                            division_org_uid=@DivisionOrgUID,temporary_credit_approval_date=@TemporaryCreditApprovalDate
                              WHERE uid = @UID;";


        return await ExecuteNonQueryAsync(sql, storeCredit);
    }
    public async Task<int> UpdateStoreCreditStatus(List<IStoreCredit> storeCredit)
    {
        string sql = @"UPDATE store_credit SET is_active = @IsActive
                              WHERE uid = @UID;";


        return await ExecuteNonQueryAsync(sql, storeCredit);
    }

    public async Task<int> DeleteStoreCredit(string UID)
    {
        Dictionary<string, object> parameters = new()
        {
            {
                "UID", UID
            }
        };
        string sql = "DELETE  FROM store_credit WHERE uid = @UID";
        return await ExecuteNonQueryAsync(sql, parameters);

    }

    public async Task<IStoreCredit> SelectStoreCreditByStoreUID(string StoreUID)
    {
        try
        {
            Dictionary<string, object> parameters = new()
            {
                {
                    "UID", StoreUID
                }
            };

            string sql = @"SELECT 
                           sc.id AS Id, 
                           sc.uid AS UID, 
                           sc.created_by AS CreatedBy, 
                           sc.created_time AS CreatedTime, 
                           sc.modified_by AS ModifiedBy, 
                           sc.modified_time AS ModifiedTime, 
                           sc.server_add_time AS ServerAddTime, 
                           sc.server_modified_time AS ServerModifiedTime, 
                           sc.store_uid AS StoreUID, 
                           sc.payment_term_uid AS PaymentTermUID, 
                           sc.credit_type AS CreditType, 
                           sc.credit_limit AS CreditLimit, 
                           sc.temporary_credit AS TemporaryCredit, 
                           sc.org_uid AS OrgUID, 
                           sc.distribution_channel_uid AS DistributionChannelUID, 
                           sc.preferred_payment_mode AS PreferredPaymentMode, 
                           sc.is_active AS IsActive, 
                           sc.is_blocked AS IsBlocked, 
                           sc.blocking_reason_code AS BlockingReasonCode, 
                           sc.blocking_reason_description AS BlockingReasonDescription, 
                           sc.price_list AS PriceList, 
                           sc.authorized_item_grp_key AS AuthorizedItemGRPKey, 
                           sc.message_key AS MessageKey, 
                           sc.tax_key_field AS TaxKeyField, 
                           sc.promotion_key AS PromotionKey, 
                           sc.disabled AS Disabled, 
                           sc.bill_to_address_uid AS BillToAddressUID, 
                           sc.ship_to_address_uid AS ShipToAddressUID, 
                           sc.outstanding_invoices AS OutstandingInvoices, 
                           sc.preferred_payment_method AS PreferredPaymentMethod, 
                           sc.payment_type AS PaymentType, 
                           sc.invoice_admin_fee_per_billing_cycle AS InvoiceAdminFeePerBillingCycle, 
                           sc.invoice_admin_fee_per_delivery AS InvoiceAdminFeePerDelivery, 
                           sc.invoice_late_payment_fee AS InvoiceLatePaymentFee, 
                           sc.is_cancellation_of_invoice_allowed AS IsCancellationOfInvoiceAllowed, 
                           sc.is_allow_cash_on_credit_exceed AS IsAllowCashOnCreditExceed, 
                           sc.is_outstanding_bill_control AS IsOutstandingBillControl, 
                           sc.is_negative_invoice_allowed AS IsNegativeInvoiceAllowed,
                           sc.credit_days AS CreditDays,
                           sc.temporary_credit_days AS TemporaryCreditDays,
                           sc.division_org_uid AS DivisionOrgUID,
                           sc.temporary_credit_approval_date AS TemporaryCreditApprovalDate
                       FROM
                           store_credit sc
                       WHERE
                           sc.store_uid = @UID";

            return await ExecuteSingleAsync<Model.Interfaces.IStoreCredit>(sql, parameters);
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }

    public async Task<List<IStoreCreditLimit>> GetCurrentLimitByStoreAndDivision(List<string> storeUIDs, string divisionUID)
    {
        try
        {
            var sql = """
                      WITH StoreOutstandingBalance
                      AS
                      (
                      SELECT division_org_uid, store_uid,SUM(balance_amount) AS balance_amount,
                      MIN(due_date) AS DueDate
                      FROM acc_payable
                      WHERE store_uid IN @StoreUID
                      AND (@DivisionUID = '' OR division_org_uid = @DivisionUID)
                      GROUP BY division_org_uid, store_uid
                      )
                      SELECT SC.store_uid AS StoreUID, SC.division_org_uid, O.Code AS Division, ISNULL(SUM(SC.credit_limit), 0) AS CreditLimit,
                      ISNULL(SUM(SC.temporary_credit), 0) AS TemporaryCreditLimit,
                      ISNULL(SUM(SB.balance_amount), 0) AS CurrentOutstanding,
                      ISNULL(SUM(SC.credit_days), 0) CreditDays,
                      ISNULL(SUM(SC.temporary_credit_days), 0) TemporaryCreditDays,
                      MIN(SB.DueDate) AS DueDate, DATEDIFF(dd, MIN(SB.DueDate), GETDATE()) AS MaxAgingDays,
                      MAX(SC.temporary_credit_approval_date) AS TemporaryCreditApprovalDate,
                      ISNULL(SUM(PB.BlokedAmount), 0) AS BlockedLimit
                      FROM store_credit SC
                      INNER JOIN org O ON o.org_type_uid = 'Supplier' AND O.uid = SC.division_org_uid
                      LEFT JOIN StoreOutstandingBalance SB ON SB.store_uid = SC.store_uid
                      --AND SB.division_org_uid = SC.division_org_uid 
                      --Once client will send division by outstanding then uncomment this line 
                      LEFT JOIN VW_PurchaseOrder_BlockedAmount PB ON PB.OrgUID = SC.store_uid AND PB.DivisionUID = SC.division_org_uid
                      where SC.store_uid IN @StoreUID
                      AND (@DivisionUID = '' OR SC.division_org_uid = @DivisionUID)
                      GROUP BY SC.store_uid, SC.division_org_uid, O.Code
                      """;
            var parameters = new
            {
                DivisionUID = divisionUID, StoreUID = storeUIDs,
            };
            return await ExecuteQueryAsync<IStoreCreditLimit>(sql, parameters);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<List<IPurchaseOrderCreditLimitBufferRange>> GetPurchaseOrderCreditLimitBufferRanges()
    {
        try
        {
            string sql = "select * from purchase_order_credit_limit_buffer_range";
            return await ExecuteQueryAsync<IPurchaseOrderCreditLimitBufferRange>(sql);
        }
        catch (Exception e)
        {
            throw;
        }
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Classes;

namespace Winit.Modules.Store.DL.Classes
{
    public class PGSQLStoreOrgConfigurationDL : Base.DL.DBManager.PostgresDBManager, Interfaces.IStoreOrgConfigurationDL
    {
        public PGSQLStoreOrgConfigurationDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }

        public async Task<Model.Interfaces.IOrgConfiguration> SelectStoreOrgConfigurationByStoreUID(string storeUID)
        {
            Model.Interfaces.IOrgConfiguration orgConfiguration = new OrgConfiguration(); 
            orgConfiguration.StoreCredit =await SelectStoreCreditByStoreUID(storeUID);
            orgConfiguration.StoreAttributes =await SelectStoreAttributesByStoreUID(storeUID);
            return orgConfiguration;
        }

        public async Task<int> CreateStoreOrgConfiguratoion(OrgConfigurationUIModel orgConfiguration)
        {
            int count = 0;
            string storeUID = string.Empty;
            if(orgConfiguration != null)
            {
                if (orgConfiguration.StoreCredit != null)
                {
                    var item = orgConfiguration.StoreCredit.FirstOrDefault();
                    if(item != null)
                    {
                        storeUID = item.StoreUID;
                    }
                }
            }
            List<Model.Interfaces.IStoreCredit> StoreCredits =await SelectStoreCreditByStoreUID(storeUID);
            List<Model.Interfaces.IStoreAttributes> storeAttributes =await SelectStoreAttributesByStoreUID(storeUID);
            bool isExist=false;
            foreach (var item in orgConfiguration.StoreCredit)
            {
                isExist = StoreCredits.Any(p=>p.UID==item.UID);
                count += isExist ? await UpdateStoreCredit(item) : await CreateStoreCredit(item);
            }
            if (count > 0)
            {
                int count1 = 0;
                foreach (var org in orgConfiguration.StoreAttributes)
                {
                    isExist = storeAttributes.Any(p => p.UID == org.UID);
                    count1 += isExist?await UpdateStoreAttributes(org): await CreateStoreAttributes(org);
                }
                count += count1;
            }

            return count;
        }
        protected async Task<int> CreateStoreAttributes(Model.Interfaces.IStoreAttributes storeAttributes)
        {
            try
            {
                var sql = @"INSERT INTO store_attributes (uid, created_by, created_time, modified_by, modified_time, server_add_time,
                            server_modified_time, company_uid, org_uid, distribution_channel_uid, store_uid, name, code, value, parent_name)
                            VALUES (@UID ,@CreatedBy ,@CreatedTime ,@ModifiedBy ,@ModifiedTime ,@ServerAddTime ,@ServerModifiedTime ,@CompanyUID ,@OrgUID ,
                            @DistributionChannelUID ,@StoreUID ,@Name ,@Code ,@Value ,@ParentName)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",storeAttributes.UID},
                    {"CreatedBy",storeAttributes.CreatedBy},
                    {"CreatedTime",storeAttributes.CreatedTime},
                    {"ModifiedBy",storeAttributes.ModifiedBy},
                    {"ModifiedTime",storeAttributes.ModifiedTime},
                    {"ServerAddTime",storeAttributes.ServerAddTime},
                    {"ServerModifiedTime",storeAttributes.ServerModifiedTime},
                    {"CompanyUID",storeAttributes.CompanyUID},
                    {"OrgUID",storeAttributes.OrgUID},
                    {"DistributionChannelUID",storeAttributes.DistributionChannelUID},
                    {"StoreUID",storeAttributes.StoreUID},
                    {"Name",storeAttributes.Name},
                    {"Code",storeAttributes.Code},
                    {"Value",storeAttributes.Value},
                    {"ParentName",storeAttributes.ParentName}
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected async Task<int> CreateStoreCredit(Model.Interfaces.IStoreCredit storeCredit)
        {
            try
            {
                var sql = @"INSERT INTO store_credit (uid, created_by, created_time, modified_by, modified_time, 
                          server_add_time, server_modified_time, store_uid, payment_term_uid, credit_type, credit_limit, 
                          temporary_credit, org_uid, distribution_channel_uid, preferred_payment_mode, is_active, is_blocked, 
                          blocking_reason_code, blocking_reason_description, price_list, authorized_item_grp_key, message_key, 
                          tax_key_field, promotion_key, disabled, bill_to_address_uid, ship_to_address_uid, outstanding_invoices,
                          preferred_payment_method, payment_type, invoice_admin_fee_per_billing_cycle, invoice_admin_fee_per_delivery,
                          invoice_late_payment_fee, is_cancellation_of_invoice_allowed, is_allow_cash_on_credit_exceed, is_outstanding_bill_control,
                          is_negative_invoice_allowed,store_group_data_uid)
                            VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                            @ServerModifiedTime, @StoreUID, @PaymentTermUID, @CreditType, @CreditLimit, @TemporaryCredit, 
                            @OrgUID, @DistributionChannelUID, @PreferredPaymentMode, @IsActive, @IsBlocked, @BlockingReasonCode, 
                            @BlockingReasonDescription, @PriceList, @AuthorizedItemGRPKey,
                            @MessageKey, @TaxKeyField, @PromotionKey, @Disabled, @BillToAddressUID, @ShipToAddressUID, @OutstandingInvoices, 
                            @PreferredPaymentMethod, @PaymentType, @InvoiceAdminFeePerBillingCycle, @InvoiceAdminFeePerDelivery, 
                            @InvoiceLatePaymentFee, @IsCancellationOfInvoiceAllowed, @IsAllowCashOnCreditExceed, @IsOutstandingBillControl, 
                            @IsNegativeInvoiceAllowed,@StoreGroupDataUID)";
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
                            { "@StoreGroupDataUID", storeCredit.StoreGroupDataUID },
                };
                return await ExecuteNonQueryAsync(sql, parameters);

            }
            catch (Exception)
            {
                throw;
            }
        }
        protected async Task<int> UpdateStoreAttributes(Model.Interfaces.IStoreAttributes storeAttributes)
        {
            try
            {
                var sql = @"UPDATE public.store_attributes
	                    SET  modified_by = @ModifiedBy, modified_time = @ModifiedTime,  server_modified_time = @ServerModifiedTime,
                             company_uid = @CompanyUID, org_uid = @OrgUID, distribution_channel_uid = @DistributionChannelUID,
                             store_uid = @StoreUID, name = @Name, code = @Code, value = @Value, parent_name = @ParentName
                        WHERE uid = @UID ";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",storeAttributes.UID},
                    {"ModifiedBy",storeAttributes.ModifiedBy},
                    {"ModifiedTime",storeAttributes.ModifiedTime},
                    {"ServerModifiedTime",storeAttributes.ServerModifiedTime},
                    {"CompanyUID",storeAttributes.CompanyUID},
                    {"OrgUID",storeAttributes.OrgUID},
                    {"DistributionChannelUID",storeAttributes.DistributionChannelUID},
                    {"StoreUID",storeAttributes.StoreUID},
                    {"Name",storeAttributes.Name},
                    {"Code",storeAttributes.Code},
                    {"Value",storeAttributes.Value},
                    {"ParentName",storeAttributes.ParentName}
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected async Task<int> UpdateStoreCredit(Model.Interfaces.IStoreCredit storeCredit)
        {
            try
            {
                var sql = @"UPDATE public.store_credit
	SET modified_by = @ModifiedBy, modified_time = @ModifiedTime, server_modified_time = @ServerModifiedTime, store_uid = @StoreUID, payment_term_uid = @PaymentTermUID, credit_type = @CreditType, 
    credit_limit = @CreditLimit, temporary_credit = @TemporaryCredit, org_uid = @OrgUID, distribution_channel_uid = @DistributionChannelUID, preferred_payment_mode = @PreferredPaymentMode, is_active = @IsActive,
    is_blocked = @IsBlocked, blocking_reason_code = @BlockingReasonCode, blocking_reason_description = @BlockingReasonDescription, price_list = @PriceList, authorized_item_grp_key = @AuthorizedItemGRPKey, 
    message_key = @MessageKey, tax_key_field = @TaxKeyField, promotion_key = @PromotionKey, disabled = @Disabled, bill_to_address_uid = @BillToAddressUID, ship_to_address_uid = @ShipToAddressUID, outstanding_invoices = @OutstandingInvoices,
    preferred_payment_method = @PreferredPaymentMethod, payment_type = @PaymentType, invoice_admin_fee_per_billing_cycle = @InvoiceAdminFeePerBillingCycle, invoice_admin_fee_per_delivery = @InvoiceAdminFeePerDelivery,
    invoice_late_payment_fee = @InvoiceLatePaymentFee, is_cancellation_of_invoice_allowed = @IsCancellationOfInvoiceAllowed, is_allow_cash_on_credit_exceed = @IsAllowCashOnCreditExceed, is_outstanding_bill_control = @IsOutstandingBillControl,
    is_negative_invoice_allowed = @IsNegativeInvoiceAllowed,store_group_data_uid = @StoreGroupDataUID
	WHERE uid = @UID";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                                            {
                            { "@UID", storeCredit.UID },
                            { "@ModifiedBy", storeCredit.ModifiedBy },
                            { "@ModifiedTime", storeCredit.ModifiedTime },
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
                            { "@StoreGroupDataUID", storeCredit.StoreGroupDataUID },
                };
                return await ExecuteNonQueryAsync(sql, parameters);

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<Model.Interfaces.IStoreAttributes>> SelectStoreAttributesByStoreUID(string StoreUID)
        {

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"StoreUID",  StoreUID}
            };

            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, 
       server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, company_uid AS CompanyUID, org_uid AS OrgUID, 
       distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, name AS Name, code AS Code, value AS Value, parent_name AS ParentName
        FROM public.store_attributes
        WHERE store_uid = @StoreUID";

            List<Model.Interfaces.IStoreAttributes> storeAttributesList = await ExecuteQueryAsync<Model.Interfaces.IStoreAttributes>(sql, parameters);
            return await Task.FromResult(storeAttributesList);

        }
        public async Task<List<Model.Interfaces.IStoreAttributes>> SelectStoreAttributesByUID(string UID)
        {

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };

            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, 
       server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, company_uid AS CompanyUID, org_uid AS OrgUID, 
       distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, name AS Name, code AS Code, value AS Value, parent_name AS ParentName
        FROM public.store_attributes WHERE uid =@UID";

            List<Model.Interfaces.IStoreAttributes> storeAttributesList = await ExecuteQueryAsync<Model.Interfaces.IStoreAttributes>(sql, parameters);
            return await Task.FromResult(storeAttributesList);

        }

        public async Task<List<Model.Interfaces.IStoreCredit>> SelectStoreCreditByStoreUID(string storeUID)
        {
            var storeCreditSql = new StringBuilder(@$"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, 
       server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, store_uid AS StoreUID, payment_term_uid AS PaymentTermUID, 
       credit_type AS CreditType, credit_limit AS CreditLimit, temporary_credit AS TemporaryCredit, org_uid AS OrgUid, 
       distribution_channel_uid AS DistributionChannelUID, preferred_payment_mode AS PreferredPaymentMode, is_active AS IsActive, 
       is_blocked AS IsBlocked, blocking_reason_code AS BlockingReasonCode, blocking_reason_description AS BlockingReasonDescription, 
       price_list AS PriceList, authorized_item_grp_key AS AuthorizedItemGrpKey, message_key AS MessageKey, tax_key_field AS TaxKeyField, 
       promotion_key AS PromotionKey, disabled AS Disabled, bill_to_address_uid AS BillToAddressUID, ship_to_address_uid AS ShipToAddressUID, 
       outstanding_invoices AS OutstandingInvoices, preferred_payment_method AS PreferredPaymentMethod, payment_type AS PaymentType, 
       invoice_admin_fee_per_billing_cycle AS InvoiceAdminFeePerBillingCycle, invoice_admin_fee_per_delivery AS InvoiceAdminFeePerDelivery, 
       invoice_late_payment_fee AS InvoiceLatePaymentFee, is_cancellation_of_invoice_allowed AS IsCancellationOfInvoiceAllowed, 
       is_allow_cash_on_credit_exceed AS IsAllowCashOnCreditExceed, is_outstanding_bill_control AS IsOutstandingBillControl, 
       is_negative_invoice_allowed AS IsNegativeInvoiceAllowed ,store_group_data_uid as StoreGroupDataUID
        FROM store_credit 
        WHERE store_uid = '{storeUID}'");

            List<Model.Interfaces.IStoreCredit> storeCreditList = await ExecuteQueryAsync<Model.Interfaces.IStoreCredit>(storeCreditSql.ToString());
            return await Task.FromResult(storeCreditList);

        }
        public async Task<List<Model.Interfaces.IStoreCredit>> SelectStoreCreditByUID(string UID)
        {
            var storeCreditSql = new StringBuilder(@$"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, 
                       server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, store_uid AS StoreUID, payment_term_uid AS PaymentTermUID, 
                       credit_type AS CreditType, credit_limit AS CreditLimit, temporary_credit AS TemporaryCredit, org_uid AS OrgUid, 
                       distribution_channel_uid AS DistributionChannelUID, preferred_payment_mode AS PreferredPaymentMode, is_active AS IsActive, 
                       is_blocked AS IsBlocked, blocking_reason_code AS BlockingReasonCode, blocking_reason_description AS BlockingReasonDescription, 
                       price_list AS PriceList, authorized_item_grp_key AS AuthorizedItemGrpKey, message_key AS MessageKey, tax_key_field AS TaxKeyField, 
                       promotion_key AS PromotionKey, disabled AS Disabled, bill_to_address_uid AS BillToAddressUID, ship_to_address_uid AS ShipToAddressUID, 
                       outstanding_invoices AS OutstandingInvoices, preferred_payment_method AS PreferredPaymentMethod, payment_type AS PaymentType, 
                       invoice_admin_fee_per_billing_cycle AS InvoiceAdminFeePerBillingCycle, invoice_admin_fee_per_delivery AS InvoiceAdminFeePerDelivery, 
                       invoice_late_payment_fee AS InvoiceLatePaymentFee, is_cancellation_of_invoice_allowed AS IsCancellationOfInvoiceAllowed, 
                       is_allow_cash_on_credit_exceed AS IsAllowCashOnCreditExceed, is_outstanding_bill_control AS IsOutstandingBillControl, 
                       is_negative_invoice_allowed AS IsNegativeInvoiceAllowed ,store_group_data_uid as StoreGroupDataUID
                FROM public.store_credit Where UID ='{UID}'");

            List<Model.Interfaces.IStoreCredit> storeCreditList = await ExecuteQueryAsync<Model.Interfaces.IStoreCredit>(storeCreditSql.ToString());
            return await Task.FromResult(storeCreditList);

        }

    }
}

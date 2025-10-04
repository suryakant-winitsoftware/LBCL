using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;
using Winit.Shared.Models.Constants;

namespace SyncManagerDL.Classes
{
    public class OracleIsProcessedStatusUpdateDL : OracleServerDBManager, IIsProcessedStatusUpdateDL
    {
        public OracleIsProcessedStatusUpdateDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<int> UpdateOracleItemIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            try
            {
                var sql = new StringBuilder($@" update  {Int_OracleTableNames.ItemMaster} set  processed_on=TO_DATE(:ProcessedOn, 'DD-MON-YYYY HH24:MI:SS'),is_processed =:IsProcessed ,error_description=:ErrorDescription where item_id=:OracleUID");
                return await ExecuteNonQueryAsync(sql.ToString(), isProcessedstatus);
            }
            catch { throw; }
        }

        public async Task<int> UpdateOraclePriceIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            try
            {
                var sql = new StringBuilder($@"update {Int_OracleTableNames.PricingMaster} set  processed_on=TO_DATE(:ProcessedOn, 'DD-MON-YYYY HH24:MI:SS'),is_processed =:IsProcessed ,error_description=:ErrorDescription where Price_Master_Id=:OracleUID");
                return await ExecuteNonQueryAsync(sql.ToString(), isProcessedstatus);
            }
            catch { throw; }
        }

        public async Task<int> UpdateOraclePriceLadderingIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            try
            {
                var sql = new StringBuilder($@"update {Int_OracleTableNames.PriceLaddering} set  processed_on=TO_DATE(:ProcessedOn, 'DD-MON-YYYY HH24:MI:SS'),is_processed =:IsProcessed ,error_description=:ErrorDescription where laddering_id=:OracleUID");
                return await ExecuteNonQueryAsync(sql.ToString(), isProcessedstatus);
            }
            catch { throw; }
        }

        public async Task<int> UpdateOracleTaxIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            try
            {
                var sql = new StringBuilder($@"update {Int_OracleTableNames.TaxMaster} set  processed_on=TO_DATE(:ProcessedOn, 'DD-MON-YYYY HH24:MI:SS'),is_processed =:IsProcessed ,error_description=:ErrorDescription where hsn_code=:OracleUID");
                return await ExecuteNonQueryAsync(sql.ToString(), isProcessedstatus);
            }
            catch { throw; }
        }

        public async Task<int> UpdateOracleCustomerReadFromOracleStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            try
            {
                var sql = new StringBuilder($@"update {Int_OracleTableNames.CustomerMaster} set read_from_oracle =:IsProcessed ,error_desc=:ErrorDescription where address_key=:OracleUID");
                return await ExecuteNonQueryAsync(sql.ToString(), isProcessedstatus);
            }
            catch { throw; }
        }
        public async Task<int> UpdateOraclePOConfirmationIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            try
            {
                var sql = new StringBuilder($@"update {Int_OracleTableNames.PurchasesOrderStatus} set    processed_on=TO_DATE(:ProcessedOn, 'DD-MON-YYYY HH24:MI:SS'),is_processed =:IsProcessed ,error_description=:ErrorDescription where purchase_order_uid=:OracleUID");
                return await ExecuteNonQueryAsync(sql.ToString(), isProcessedstatus);
            }
            catch { throw; }
        }
        public async Task<int> UpdateOracleInvoiceIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            try
            {
                var sql = new StringBuilder($@"update {Int_OracleTableNames.InvoiceHeader} set  processed_on=TO_DATE(:ProcessedOn, 'DD-MON-YYYY HH24:MI:SS'), is_processed =:IsProcessed ,error_description=:ErrorDescription where delivery_id=:OracleUID");
                return await ExecuteNonQueryAsync(sql.ToString(), isProcessedstatus);
            }
            catch { throw; }
        }
        public async Task<int> UpdateOracleCreditLimitIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            try
            {
                var sql = new StringBuilder($@"update {Int_OracleTableNames.CustomerCreditLimit} set  processed_on=TO_DATE(:ProcessedOn, 'DD-MON-YYYY HH24:MI:SS'), is_processed =:IsProcessed ,error_description=:ErrorDescription where customer_number=:OracleUID");
                return await ExecuteNonQueryAsync(sql.ToString(), isProcessedstatus);
            }
            catch { throw; }
        }
        public async Task<int> UpdateOracleOutstandingInvoiceIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            try
            {
                var sql = new StringBuilder($@"update {Int_OracleTableNames.OutstandingInvoice} set  processed_on=TO_DATE(:ProcessedOn, 'DD-MON-YYYY HH24:MI:SS'), is_processed =:IsProcessed ,error_description=:ErrorDescription where rec_uid=:OracleUID");
                return await ExecuteNonQueryAsync(sql.ToString(), isProcessedstatus);
            }
            catch { throw; }
        }
        public async Task<int> UpdateOracleTemporaryCreditLimitsIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            try
            {
                var sql = new StringBuilder($@"update {Int_OracleTableNames.TemporaryCreditLimit} set processed_on=TO_DATE(:ProcessedOn, 'DD-MON-YYYY HH24:MI:SS'), is_processed =:IsProcessed ,error_description=:ErrorDescription where req_uid=:OracleUID");
                return await ExecuteNonQueryAsync(sql.ToString(), isProcessedstatus);
            }
            catch { throw; }
        }
        public async Task<int> UpdateOracleWarehouseStocksIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            try
            {
                var sql = new StringBuilder($@"update {Int_OracleTableNames.WarehouseStock} set processed_on=TO_DATE(:ProcessedOn, 'DD-MON-YYYY HH24:MI:SS'), is_processed =:IsProcessed ,error_description=:ErrorDescription where sku_code=:OracleUID");
                return await ExecuteNonQueryAsync(sql.ToString(), isProcessedstatus);
            }
            catch { throw; }

        }
        public async Task<int> UpdateOracleProvisionsIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            try
            {
                var sql = new StringBuilder($@"update {Int_OracleTableNames.Provisions} set processed_on=TO_DATE(:ProcessedOn, 'DD-MON-YYYY HH24:MI:SS'), is_processed =:IsProcessed ,error_description=:ErrorDescription where provision_id=:OracleUID");
                return await ExecuteNonQueryAsync(sql.ToString(), isProcessedstatus);
            }
            catch { throw; }

        }
        public async Task<int> UpdateOracleProvisionCreditNotesIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            try
            {
                var sql = new StringBuilder($@"update {Int_OracleTableNames.ProvisionCreditNotes} set processed_on=TO_DATE(:ProcessedOn, 'DD-MON-YYYY HH24:MI:SS'), is_processed =:IsProcessed ,error_description=:ErrorDescription where provision_id=:OracleUID");
                return await ExecuteNonQueryAsync(sql.ToString(), isProcessedstatus);
            }
            catch { throw; }

        }
        public async Task<int> UpdateOraclePayThroughAPMastersIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            try
            {
                var sql = new StringBuilder($@"update {Int_OracleTableNames.PayThroughAPMaster} set processed_on=TO_DATE(:ProcessedOn, 'DD-MON-YYYY HH24:MI:SS'), is_processed =:IsProcessed ,error_description=:ErrorDescription where sku_code=:OracleUID and start_date=TO_DATE(:CommonAttribute1,'DD-MM-YY')");
                return await ExecuteNonQueryAsync(sql.ToString(), isProcessedstatus);
            }
            catch { throw; }

        }
        public async Task<int> UpdateOracleCustomerReferenceIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            try
            {
                var sql = new StringBuilder($@"update {Int_OracleTableNames.CustomerCodeBlock} set processed_on=TO_DATE(:ProcessedOn, 'DD-MON-YYYY HH24:MI:SS'), is_processed =:IsProcessed ,error_description=:ErrorDescription where customer_code=:OracleUID and nvl(is_processed,0)=0 ");
                return await ExecuteNonQueryAsync(sql.ToString(), isProcessedstatus);
            }
            catch { throw; }

        }
    }
}

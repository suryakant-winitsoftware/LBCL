using DocumentFormat.OpenXml.Spreadsheet;
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
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CollectionModule.DL.Classes
{
    public class MSSQLAccPayableCMIDL : Base.DL.DBManager.SqlServerDBManager, Interfaces.IAccPayableCMIDL
    {
        public MSSQLAccPayableCMIDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }

        public async Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayableCMI>> GetAccPayableCMIDetails(List<SortCriteria> sortCriterias, int pageNumber,
                 int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string jobPositionUID)
        {
            try
            {
                string cte = """
                    with Stores as (
                     select s.code,s.name,s.number,s.uid from store s 
                          inner join my_orgs mo on   mo.job_position_uid=@jobPositionUID and mo.org_uid=s.uid
                    	  ),
                     OutstandingInvoices as 
                    (
                         SELECT  apc.ou AS OU  	,s.code as CustomerCode,s.name AS CustomerName,
                              	sum(ap.balance_amount) as TotalBalanceAmount,count(ap.balance_amount) as CountOfInvoices
                              			FROM acc_payable_cmi apc
                              			INNER JOIN Stores s ON s.number = apc.customer_number
                              			INNER JOIN acc_payable ap ON ap.store_uid = s.uid  
                              			group by s.code,s.name,apc.ou
                              			)

                    """;
                StringBuilder sql = new($"""
                                       {cte} 

                    select * from OutstandingInvoices
                    """);
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder($"""
                                                 {cte} 

                        select count(1) as cnt from OutstandingInvoices 
                        """);
                }
                var parameters = new Dictionary<string, object>()
                {
                    { "jobPositionUID",jobPositionUID}
                };
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Bank.Model.Interfaces.IBank>(filterCriterias, sbFilterCriteria, parameters);
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
                    if (sortCriterias != null && sortCriterias.Count > 0)
                    {
                        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append($" ORDER BY CustomerCode OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }

                }
                IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayableCMI> accPayableCMIs = await ExecuteQueryAsync<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayableCMI>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayableCMI> pagedResponse = new PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayableCMI>
                {
                    PagedData = accPayableCMIs,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }



        public async Task<Model.Interfaces.IAccPayableMaster> GetAccPayableMasterByUID(string uID)
        {
            IAccPayableMaster result = null;
            Type accPayableCMIType = _serviceProvider.GetRequiredService<IAccPayableCMI>().GetType();
            Type accPayableType = _serviceProvider.GetRequiredService<IAccPayableView>().GetType();
            try
            {
                var sql = @"SELECT 
                                '[' + s.code +  ']  ' + s.name AS CustomerName,
                                apc.OU AS OU,
                                SUM(ap.balance_amount) AS TotalBalanceAmount
                            FROM 
                                acc_payable ap
                            
                           inner JOIN 
                                store s ON s.uid = ap.store_uid
							inner	JOIN 
                                acc_payable_cmi apc ON apc.customer_number = s.number
                            WHERE apc.uid=@UID
                             
                            GROUP BY 
                                '[' + s.code +  ']  ' + s.name, apc.OU;


                            select ap.source_type AS SourceType ,ap.reference_number As ReferenceNumber,ap.transaction_date AS TransactionDate,
                            ap.amount As Amount,ap.balance_amount As BalanceAmount,
                            apc.tax_invoice_number As TaxInvoiceNumber,apc.tax_invoice_date As TaxInvoiceDate 
							from  acc_payable_cmi apc
                            
                           inner JOIN 
                                store s ON s.number = apc.customer_number
							inner	JOIN 
                                acc_payable ap ON ap.store_uid = s.uid
                            where 
							apc.uid=@UID;";

                var parameters = new Dictionary<string, object>()
                 {
                 { "UID", uID },
                 };
                DataSet ds = await ExecuteQueryDataSetAsync(sql, parameters);
                if (ds != null && ds.Tables.Count == 2)
                {
                    DataTable dataTable0 = ds.Tables[0];
                    DataTable dataTable1 = ds.Tables[1];
                    result = _serviceProvider.CreateInstance<IAccPayableMaster>();
                    if (dataTable0.Rows.Count > 0)
                    {
                        result.AccPayableCMI = new AccPayableCMI();
                        foreach (DataRow row in dataTable0.Rows)
                        {
                            IAccPayableCMI accPayableCMI = ConvertDataTableToObject<IAccPayableCMI>(row, null, accPayableCMIType);
                            result.AccPayableCMI = accPayableCMI;
                        }
                    }
                    if (dataTable1.Rows.Count > 0)
                    {
                        result.AccPayableList = new List<IAccPayableView>();

                        foreach (DataRow row in dataTable1.Rows)
                        {
                            IAccPayableView accPayable = ConvertDataTableToObject<IAccPayableView>(row, null, accPayableType);
                            result.AccPayableList.Add(accPayable);
                        }
                    }
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<List<OutstandingInvoiceView>> OutSTandingInvoicesByStoreCode(string storeCode, int pageNumber, int pageSize)
        {
            try
            {
                var sql = $"""
                                 select ap.source_type AS SourceType ,ap.reference_number As ReferenceNumber,ap.transaction_date AS TransactionDate,
                                            ap.amount As Amount,ap.balance_amount As BalanceAmount,
                                            apc.tax_invoice_number As TaxInvoiceNumber,apc.tax_invoice_date As TaxInvoiceDate ,ap.due_date as InvoiceDueDate
                							from  acc_payable_cmi apc

                                           inner JOIN 
                                                store s ON s.code = apc.customer_number
                							inner	JOIN 
                                                acc_payable ap ON ap.store_uid = s.uid
                                            where 
                							s.code=@StoreCode  
                                            order by  TransactionDate desc offset {(pageNumber - 1) * pageSize} rows fetch next {pageSize} rows only;
                """;
                var parameter = new
                {
                    StoreCode = storeCode
                };
                var invoices = await ExecuteQueryAsync<OutstandingInvoiceView>(sql.ToString(), parameter);
                return invoices;
            }
            catch
            {
                throw;
            }

        }
    }
}

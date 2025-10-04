using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.CreditLimit.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CreditLimit.DL.Classes;

public class MSSQLTemporaryCreditDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ITemporaryCreditDL
{
    public MSSQLTemporaryCreditDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {

    }
    public async Task<PagedResponse<Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit>> SelectTemporaryCreditDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired,string jobPositionUID)
    {
        try
        {
            string cte = $"""
                                with Orgs as
                (select org_uid from my_orgs where job_position_uid=@JobPositionUID),
                TempCreditLimit as(
                Select tc.id
                    ,tc.uid
                    ,tc.created_by
                    ,tc.created_time
                    ,tc.modified_by
                    ,tc.modified_time
                    ,tc.server_add_time
                    ,tc.server_modified_time
                    ,tc.ss
                    ,tc.store_uid As StoreUID
                    ,tc.order_number As OrderNumber
                    ,tc.request_type As RequestType
                    ,tc.effective_from As EffectiveFrom
                    ,tc.effective_upto As EffectiveUpto
                    ,tc.request_amount_days As RequestAmountDays
                    ,tc.remarks as Remarks
                    ,tc.status As Status
                    ,tc.approved_on As ApprovedOn
                    ,tc.division_org_uid As DivisionOrgUID
                     ,tc.credit_data As CreditData
                     ,tc.request_date As RequestDate
                     ,tc.calender_end_date As CalenderEndDate 
                     ,tc.max_days As MaxDays
                     ,tc.collection_mtd as CollectionMtd
                     ,tc.collection_bom as CollectionBom
                     from temporary_credit tc 
                	 inner join Orgs mo on tc.store_uid=mo.org_uid
                	 )
                """;
            var sql = new StringBuilder($"""
                {cte}

                select * from TempCreditLimit
                """);
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder($"""
                    {cte}
                    select count(1) from TempCreditLimit
                    """);
            }
            var parameters = new Dictionary<string, object>()
            {
                {"JobPositionUID",jobPositionUID }
            };
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit>(filterCriterias, sbFilterCriteria, parameters);
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
                    sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

            }
            IEnumerable<Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit> temporaryCredits = await ExecuteQueryAsync<Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit>(sql.ToString(), parameters);
            int totalCount = -1;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }
            PagedResponse<Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit> pagedResponse = new PagedResponse<Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit>
            {
                PagedData = temporaryCredits,
                TotalCount = totalCount
            };
            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit> GetTemporaryCreditByUID(string UID)
    {
        try
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {
                    "UID", UID
                }
            };
            var sql = @"Select id
                                                            ,uid
                                                            ,created_by
                                                            ,created_time
                                                            ,modified_by
                                                            ,modified_time
                                                            ,server_add_time
                                                            ,server_modified_time
                                                            ,ss
                                                            ,store_uid
                                                            ,order_number
                                                            ,request_type
                                                            ,effective_from
                                                            ,effective_upto
                                                            ,request_amount_days
                                                            ,remarks
                                                            ,status
                                                            ,approved_on
                                                            ,division_org_uid
                                                            ,credit_data
                                                             ,request_date
                                                             ,calender_end_date
                                                             ,max_days
                                                            ,collection_mtd as CollectionMtd
                                                             ,collection_bom as CollectionBom
                                                            from temporary_credit
                                                             WHERE uid = @UID";
            return await ExecuteSingleAsync<Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit>(sql, parameters);
        }
        catch
        {
            throw;
        }

    }
    public async Task<int> CreateTemporaryCreditDetails(Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit temporaryCredit)
    {
        try
        {
            var sql = @"
                    INSERT INTO temporary_credit (
                        uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                        server_modified_time,ss,store_uid,order_number,request_type,effective_from,effective_upto,
                                                  request_amount_days,remarks,status,approved_on,division_org_uid,credit_data
                                                             ,request_date
                                                             ,calender_end_date
                                                             ,max_days
                                                             ,collection_mtd 
                                                             ,collection_bom 
                    ) 
                    VALUES (
                        @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                        @ServerModifiedTime,@SS,@StoreUID,@OrderNumber,@RequestType,@EffectiveFrom,@EffectiveUpto,@RequestAmountDays,
                            @Remarks,@Status,@ApprovedOn,@DivisionOrgUID,@CreditData, @RequestDate,@CalenderEndDate,@MaxDays, @CollectionMTD,@CollectionBOM );";

            return await ExecuteNonQueryAsync(sql, temporaryCredit);
        }
        catch
        {
            throw;
        }
    }
    public async Task<int> UpdateTemporaryCreditDetails(Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit temporaryCredit)
    {
        try
        {
            var sql = @"
                                UPDATE temporary_credit
                                SET
                                    modified_by = @ModifiedBy,
                                    modified_time = @ModifiedTime,
                                    server_modified_time = @ServerModifiedTime,
                                    ss = @SS,
                                    store_uid = @StoreUID,
                                    order_number = @OrderNumber,
                                    request_type = @RequestType,
                                    effective_from = @EffectiveFrom,
                                    effective_upto = @EffectiveUpto,
                                    request_amount_days = @RequestAmountDays,
                                    remarks = @Remarks,
                                    status = @Status,
                                    approved_on = @ApprovedOn,
                                    division_org_uid = @DivisionOrgUID,
                                    credit_data = @CreditData,
                                    request_date = @RequestDate,
                                    calender_end_date = @CalenderEndDate,
                                    max_days = @MaxDays,
                                    collection_mtd = @CollectionMTD,
                                    collection_bom = @CollectionBOM
                                WHERE
                                    uid = @UID;
                                ";
            return await ExecuteNonQueryAsync(sql, temporaryCredit);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> DeleteTemporaryCreditDetails(String UID)
    {
        try
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {
                    "UID", UID
                }
            };
            var sql = @"DELETE  FROM temporary_credit WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        catch
        {
            throw;
        }

    }

}

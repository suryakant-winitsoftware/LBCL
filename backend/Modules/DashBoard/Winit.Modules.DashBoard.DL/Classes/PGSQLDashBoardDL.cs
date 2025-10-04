using System.Data;
using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Winit.Modules.DashBoard.DL.Interfaces;
using Winit.Modules.DashBoard.Model.Classes;
using Winit.Modules.DashBoard.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.DashBoard.DL.Classes;

public class PGSQLDashBoardDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IDashBoardDL
{
    protected Winit.Shared.CommonUtilities.Common.CommonFunctions _commonFunctions;


    public PGSQLDashBoardDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {
    }

    public async Task<List<ISalesPerformance>> GetSalesPerformance(int month, int year, int count)
    {
        SqlConnection connection = CreateConnection(ConnectionStringName.SqlServerReports);
        try
        {
            await connection.OpenAsync();
            var subq = new StringBuilder(" si.period_year = @Year ");
            if (month > 0)
            {
                subq.Append(" AND si.period_num = @Month ");
            }
            var sql = new StringBuilder(@$"SELECT TOP   {count}  
                                    si.store_code AS StoreCode,
                                    si.store_name AS StoreName,
                                    SUM(si.net_amount) AS NetAmount
                                FROM 
                                    dbo.stg_invoice si
                                WHERE {subq}
                                    
                                    
                                GROUP BY 
                                    si.store_code, si.store_name
                                ORDER BY 
                                    NetAmount DESC;");
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "Month", month },
                { "Year", year },
                { "Count", count },
            };

            return await ExecuteQueryAsync<ISalesPerformance>(sql.ToString(), parameters, connection: connection);
        }
        catch
        {
            throw;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    public async Task<List<ICategoryPerformance>> GetCategorySalesPerformance(int month, int year, int count)
    {
        SqlConnection connection = CreateConnection(ConnectionStringName.SqlServerReports);
        try
        {
            await connection.OpenAsync();
            var sql = new StringBuilder(@"SELECT 
                                SIL.attribute2_code AS CategoryCode,
                                SIL.attribute2_name AS CategoryName,
                                SUM(SIL.qty) AS TotalVolume
                                FROM stg_invoice_line SIL
                                INNER JOIN stg_invoice SI ON SIL.invoice_uid = SI.invoice_uid
                                WHERE 
                                    SI.period_year = @Year 
                                    AND SI.period_num = @Month
                                GROUP BY 
                                    SIL.attribute2_code, 
                                    SIL.attribute2_name
                                ORDER BY 
                                    TotalVolume DESC;
                                    ");
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "Month", month },
                { "Year", year },
            };

            return await ExecuteQueryAsync<ICategoryPerformance>(sql.ToString(), parameters, connection: connection);
        }
        catch
        {
            throw;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    public async Task<List<IGrowthWiseChannelPartner>> GetTopChanelPartners(int LastYear, int CurrentYear, int count)
    {
        SqlConnection connection = CreateConnection(ConnectionStringName.SqlServerReports);
        try
        {
            await connection.OpenAsync();
            var sql = new StringBuilder(@"WITH SalesData AS (
               
                SELECT 
                    SI.store_uid,
                    SI.store_code AS ChannelPartnerCode,
                    SI.store_name AS ChannelPartnerName,
                    SI.fiscal_year,
                    SUM(SI.net_amount) AS TotalSales
                FROM stg_invoice SI
                WHERE SI.fiscal_year IN (@CurrentYear, @LastYear)
                GROUP BY SI.store_uid, SI.store_code, SI.store_name, SI.fiscal_year
            ),
            PivotedData AS (
               
                SELECT 
                    SD.store_uid,
                    SD.ChannelPartnerCode,
                    SD.ChannelPartnerName,
                    COALESCE(CY.TotalSales, 0) AS CurrentYearSales,  -- If no sales, consider 0
                    COALESCE(LY.TotalSales, 0) AS LastYearSales      -- If no sales, consider 0
                FROM (SELECT DISTINCT store_uid, ChannelPartnerCode, ChannelPartnerName FROM SalesData) SD
                LEFT JOIN SalesData CY ON SD.store_uid = CY.store_uid AND CY.fiscal_year = @CurrentYear
                LEFT JOIN SalesData LY ON SD.store_uid = LY.store_uid AND LY.fiscal_year = @LastYear
            ),
            RankedData AS (
              
                SELECT 
                    PD.*,
                    RANK() OVER (ORDER BY PD.CurrentYearSales DESC) AS SalesRank
                FROM PivotedData PD
            )
            SELECT 
                RD.ChannelPartnerCode,
                RD.ChannelPartnerName,
                RD.CurrentYearSales,
                RD.LastYearSales,
                (RD.CurrentYearSales - RD.LastYearSales) AS SalesGrowth,
                CASE 
                    WHEN RD.LastYearSales = 0 THEN 
                        CASE WHEN RD.CurrentYearSales = 0 THEN 0 ELSE 100 END  -- If both are 0, Growth% is 0
                    ELSE 
                        ((RD.CurrentYearSales - RD.LastYearSales) * 100.0 / RD.LastYearSales) 
                END AS GrowthPercentage
            FROM RankedData RD
            WHERE RD.SalesRank <= @Count
            ORDER BY RD.CurrentYearSales DESC;
                                    ");
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "LastYear", LastYear },
                { "CurrentYear", CurrentYear },
                { "Count", count },
            };

            return await ExecuteQueryAsync<IGrowthWiseChannelPartner>(sql.ToString(), parameters,
                connection: connection);
        }
        catch
        {
            throw;
        }
        finally
        {
            await connection.CloseAsync();
        }

        await connection.CloseAsync();
    }
    public async Task<List<IGrowthWiseChannelPartner>> GetGrowthVsDeGrowth(int LastYear, int CurrentYear, int count)
    {
        SqlConnection connection = CreateConnection(ConnectionStringName.SqlServerReports);
        try
        {
            await connection.OpenAsync();
            var sql = new StringBuilder(@"WITH SalesData AS (
               
                SELECT 
                    SI.store_uid,
                    SI.store_code AS ChannelPartnerCode,
                    SI.store_name AS ChannelPartnerName,
                    SI.fiscal_year,
                    SUM(SI.net_amount) AS TotalSales
                FROM stg_invoice SI
                WHERE SI.fiscal_year IN (@CurrentYear, @LastYear)
                GROUP BY SI.store_uid, SI.store_code, SI.store_name, SI.fiscal_year
            ),
            PivotedData AS (
               
                SELECT 
                    SD.store_uid,
                    SD.ChannelPartnerCode,
                    SD.ChannelPartnerName,
                    COALESCE(CY.TotalSales, 0) AS CurrentYearSales,  -- If no sales, consider 0
                    COALESCE(LY.TotalSales, 0) AS LastYearSales      -- If no sales, consider 0
                FROM (SELECT DISTINCT store_uid, ChannelPartnerCode, ChannelPartnerName FROM SalesData) SD
                LEFT JOIN SalesData CY ON SD.store_uid = CY.store_uid AND CY.fiscal_year = @CurrentYear
                LEFT JOIN SalesData LY ON SD.store_uid = LY.store_uid AND LY.fiscal_year = @LastYear
            ),
            RankedData AS (
              
                SELECT 
                    PD.*,
                    RANK() OVER (ORDER BY PD.CurrentYearSales DESC) AS SalesRank
                FROM PivotedData PD
            )
            SELECT 
                RD.ChannelPartnerCode,
                RD.ChannelPartnerName,
                RD.CurrentYearSales,
                RD.LastYearSales,
                (RD.CurrentYearSales - RD.LastYearSales) AS SalesGrowth,
                CASE 
                    WHEN RD.LastYearSales = 0 THEN 
                        CASE WHEN RD.CurrentYearSales = 0 THEN 0 ELSE 100 END  -- If both are 0, Growth% is 0
                    ELSE 
                        ((RD.CurrentYearSales - RD.LastYearSales) * 100.0 / RD.LastYearSales) 
                END AS GrowthPercentage
            FROM RankedData RD
            WHERE  RD.SalesRank <= @Count 
            ORDER BY RD.CurrentYearSales DESC;
                                    ");
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "LastYear", LastYear },
                { "CurrentYear", CurrentYear },
                { "Count", count },
            };

            return await ExecuteQueryAsync<IGrowthWiseChannelPartner>(sql.ToString(), parameters,
                connection: connection);
        }
        catch
        {
            throw;
        }
        finally
        {
            await connection.CloseAsync();
        }

        await connection.CloseAsync();
    }
    public async Task<List<IDistributorPerformance>> GetTargetVsAchievement(int Year, int Month, int count)
    {
        SqlConnection connection = CreateConnection(ConnectionStringName.SqlServerReports);
        try
        {
            await connection.OpenAsync();
            var sql = new StringBuilder(@" SELECT 
                            dts.distributor_uid, 
                            dts.year, 
                            dts.month, 
                            dts.quarter, 
                            dts.target_value, 
                            dts.achieved_value, 
                            dts.last_year_achieved_value, 
                            dts.achievement_percentage, 
                            dts.growth_percentage
	                        
                        FROM distributor_target_summary  dts  ");
            if (Year != 0 || Month != 0)
            {
                sql.Append(" where ");
            }
            if (Year != 0)
            {
                sql.Append(" dts.year=@Year ");
            }

            if (Year != 0 && Month != 0)
            {
                sql.Append(" and ");
            }
            if (Month != 0)
            {
                sql.Append(" dts.month=@Month");
            }
            sql.Append(" order by achieved_value desc");
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "Year", Year },
                { "Month", Month },
                { "Count", count },
            };

            return await ExecuteQueryAsync<IDistributorPerformance>(sql.ToString(), parameters,
                connection: connection);
        }
        catch
        {
            throw;
        }
        finally
        {
            await connection.CloseAsync();
        }

        await connection.CloseAsync();
    }
    public async Task<List<IGrowthWiseChannelPartner>> GetTopChanelPartnersByCategoryAndGroup(CategoryWiseTopChhannelPartnersRequest request)
    {
        SqlConnection connection = CreateConnection(ConnectionStringName.SqlServerReports);
        try
        {
            await connection.OpenAsync();
            var sql = new StringBuilder(@"WITH SalesData AS (
                                SELECT 
                                    SI.store_uid,
                                    SI.store_code AS ChannelPartnerCode,
                                    SI.store_name AS ChannelPartnerName,
                                    SI.fiscal_year,
                                    SUM(SI.net_amount) AS TotalSales
                                FROM stg_invoice SI
                                INNER JOIN stg_invoice_line SIL ON SI.invoice_uid = SIL.invoice_uid
                                WHERE 
                                    SI.fiscal_year IN (@CurrentYear, @LastYear)
                                    AND (SIL.attribute2_code IN (SELECT value FROM STRING_SPLIT(@ProductGroup, ',')) OR @ProductGroup = '')
                                    AND (SIL.attribute3_code IN (SELECT value FROM STRING_SPLIT(@ProductType, ',')) OR @ProductType = '')
                                GROUP BY SI.store_uid, SI.store_code, SI.store_name, SI.fiscal_year
                            ),
                            PivotedData AS (
                                SELECT 
                                    SD.store_uid,
                                    SD.ChannelPartnerCode,
                                    SD.ChannelPartnerName,
                                    COALESCE(CY.TotalSales, 0) AS CurrentYearSales,
                                    COALESCE(LY.TotalSales, 0) AS LastYearSales
                                FROM (SELECT DISTINCT store_uid, ChannelPartnerCode, ChannelPartnerName FROM SalesData) SD
                                LEFT JOIN SalesData CY ON SD.store_uid = CY.store_uid AND CY.fiscal_year = @CurrentYear
                                LEFT JOIN SalesData LY ON SD.store_uid = LY.store_uid AND LY.fiscal_year = @LastYear
                            ),
                            RankedData AS (
                                SELECT 
                                    PD.*,
                                    RANK() OVER (ORDER BY PD.CurrentYearSales DESC) AS SalesRank
                                FROM PivotedData PD
                            )

                            SELECT 
                                RD.ChannelPartnerCode,
                                RD.ChannelPartnerName,
                                RD.CurrentYearSales,
                                RD.LastYearSales,
                                (RD.CurrentYearSales - RD.LastYearSales) AS SalesGrowth,
                                CASE 
                                    WHEN RD.LastYearSales = 0 THEN 
                                        CASE WHEN RD.CurrentYearSales = 0 THEN 0 ELSE 100 END
                                    ELSE 
                                        ((RD.CurrentYearSales - RD.LastYearSales) * 100.0 / RD.LastYearSales) 
                                END AS GrowthPercentage
                            FROM RankedData RD
                            WHERE RD.SalesRank <= @Count
                            ORDER BY RD.CurrentYearSales DESC;
                                    ");
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "LastYear", request.LastYear },
                { "CurrentYear", request.CurrentYear },
                { "Count", request.Count},
                { "ProductGroup", string.Join(",",request.Groups)},
                { "ProductType", string.Join(",",request.Types)},
            };

            return await ExecuteQueryAsync<IGrowthWiseChannelPartner>(sql.ToString(), parameters,
                connection: connection);
        }
        catch
        {
            throw;
        }
        finally
        {
            await connection.CloseAsync();
        }

        await connection.CloseAsync();
    }
    public async Task<PagedResponse<IBranchSalesReport>> GetBranchSalesReport(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, bool isForExport)
    {
        var sql = new StringBuilder(@"
                  select * from( select  branch_code BranchCode,branch_name BranchName, period_year as PeriodYear,
                    sum(qty) as TotalUnits,sum(net_amount) as TotalSales
                    from stg_invoice_line
                    group by branch_code,branch_name,period_year  ) as SubQuery ");
        var sqlCount = new StringBuilder(@"
                   select count(1) as cnt from(select  branch_code BranchCode,branch_name BranchName, period_year as PeriodYear,
                    sum(qty) as TotalUnits,sum(net_amount) as TotalSales
                    from stg_invoice_line
                    group by branch_code,branch_name,period_year  ) as SubQuery");

        if (isForExport)
        {
            sql = new StringBuilder(@"select * from( select  branch_code BranchCode,branch_name BranchName, period_year as PeriodYear,asm_emp_code ASMCode,asm_emp_name ASMName,
                                        org_code OrgCode,org_name OrgName, 
                                          sum(qty) as TotalUnits,sum(net_amount) as TotalSales
                                          from stg_invoice_line
                                          group by branch_code,branch_name,period_year,asm_emp_code,asm_emp_name,org_code,org_name  ) as SubQuery ");
            pageNumber = 0; pageSize = 0; isCountRequired = false;
        }
        var parameters = new Dictionary<string, object>();

        if (filterCriterias != null && filterCriterias.Count > 0)
        {
            StringBuilder sbFilterCriteria = new StringBuilder();
            sbFilterCriteria.Append(" WHERE ");
            AppendFilterCriteria<IBranchSalesReport>(filterCriterias,
            sbFilterCriteria, parameters);
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
                sql.Append(
                $" ORDER BY PeriodYear OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            }
        }
        SqlConnection connection = CreateConnection(ConnectionStringName.SqlServerReports);
        try
        {
            await connection.OpenAsync();

            var reportLists = await ExecuteQueryAsync<IBranchSalesReport>(sql.ToString(), parameters, connection: connection);
            var count = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters, connection: connection );

            var paged = new PagedResponse<IBranchSalesReport>()
            {
                PagedData = reportLists,
                TotalCount = count
            };
            return paged;
        }
        catch
        {
            throw;
        }
        finally
        {
            await connection.CloseAsync();
        }

    }
    public async Task<List<IBranchSalesReportAsmview>> GetAsmViewByBranchCode(string branchCode)
    {
        string sql = """
                    select  branch_code BranchCode,branch_name BranchName,asm_emp_code ASMCode,asm_emp_name ASMName, period_year as PeriodYear,
                    sum(qty) as TotalUnits,sum(net_amount) as TotalSales
                    from stg_invoice_line where branch_code=@BranchCode
                    group by branch_code,branch_name,period_year,asm_emp_code,asm_emp_name
                    """;
        var parameters = new
        {
            BranchCode = branchCode
        };
        SqlConnection connection = CreateConnection(ConnectionStringName.SqlServerReports);
        try
        {
            await connection.OpenAsync();
            return await ExecuteQueryAsync<IBranchSalesReportAsmview>(sql.ToString(), parameters, connection: connection);
        }
        catch
        {
            throw;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
    public async Task<List<IBranchSalesReportOrgview>> GetOrgViewByBranchCode(string branchCode)
    {
        string sql = """
                    select * from (select  branch_code BranchCode,branch_name BranchName,org_code OrgCode,org_name OrgName, period_year as PeriodYear,
                    sum(qty) as TotalUnits,sum(net_amount) as TotalSales
                    from stg_invoice_line where branch_code=@BranchCode
                    group by branch_code,branch_name,period_year,org_code,org_name) as SubQuery
                    """;
        var parameters = new
        {
            BranchCode = branchCode
        };
        SqlConnection connection = CreateConnection(ConnectionStringName.SqlServerReports);
        try
        {
            await connection.OpenAsync();
            return await ExecuteQueryAsync<IBranchSalesReportOrgview>(sql.ToString(), parameters, connection: connection);
        }
        catch
        {
            throw;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
    public async Task<List<IBranchSalesReport>> GetBranchSalesReportForExportExcel()
    {
        return null;
    }
}
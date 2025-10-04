using Microsoft.Extensions.Configuration;
using System.Text;
using Winit.Modules.PriceLadder.DL.Interfaces;
using Winit.Modules.PriceLadder.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PriceLadder.DL.Classes;

public class PGSQLSKUPriceLadderingDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ISkuPriceLadderingDL
{
    public PGSQLSKUPriceLadderingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {
    }
    public async Task<List<ISKUPriceLadderingData>> GetApplicablePriceLaddering(string broadCustomerClassification,
             DateTime date, List<int>? productCategoryIds = null)
    {

        try
        {
            StringBuilder sql = new("""
                        SELECT product_category_id, SUM(percentage_discount) AS PercentageDiscount 
                        FROM sku_price_laddering 
                        where 
                        broad_customer_classification = @BroadCustomerClassification 
                        AND @Date BETWEEN start_date and end_date
                    """);
            var parameters = new
            {
                BroadCustomerClassification = broadCustomerClassification,
                Date = date,
                ProductCategoryIds = productCategoryIds
            };
            if (productCategoryIds != null && productCategoryIds.Any())
            {
                _ = sql.Append(" AND product_category_id IN @ProductCategoryIds ");
            }
            sql.Append(" GROUP BY product_category_id");
            return await ExecuteQueryAsync<ISKUPriceLadderingData>(sql.ToString(), parameters);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<PagedResponse<IPriceLadderingItemView>> SelectAllThePriceLaddering(
     List<SortCriteria> sortCriterias,
     int pageNumber,
     int pageSize,
     List<FilterCriteria> filterCriterias,
     bool isCountRequired)
    {
        try
        {
            var sql = new StringBuilder(@"
            SELECT 
                spl.operating_unit AS OrgUnit,
                spl.division AS Division,
                sku.code AS ProductCode,
                sed.product_category_name AS ProductCategory,
                spl.branch AS Branch,
                spl.sales_office AS SalesOffice,
                spl.start_date AS StartDate,
                spl.end_date AS EndDate,
                spl.created_time AS CreationDate
            FROM 
                dbo.sku_price_laddering spl
                LEFT JOIN dbo.sku_ext_data sed ON spl.product_category_id = sed.product_category_id
                LEFT JOIN dbo.sku sku ON sed.sku_uid = sku.uid");

            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount.Append(@"
                SELECT COUNT(1) AS Cnt 
                FROM 
                    dbo.sku_price_laddering spl
                    LEFT JOIN dbo.sku_ext_data sed ON spl.product_category_id = sed.product_category_id
                    LEFT JOIN dbo.sku sku ON sed.sku_uid = sku.uid");
            }

            var parameters = new Dictionary<string, object?>();

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<IPriceLadderingItemView>(filterCriterias, sbFilterCriteria, parameters);
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

            IEnumerable<IPriceLadderingItemView> priceLadderingDetails = await ExecuteQueryAsync<IPriceLadderingItemView>(sql.ToString(), parameters);
            int totalCount = -1;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<IPriceLadderingItemView> pagedResponse = new PagedResponse<IPriceLadderingItemView>
            {
                PagedData = priceLadderingDetails,
                TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }


    public async Task<PagedResponse<IPriceLaddering>> GetPriceLadders(
     List<SortCriteria> sortCriterias,
     int pageNumber,
     int pageSize,
     List<FilterCriteria> filterCriterias,
     bool isCountRequired)
    {
        try
        {
            string sss = string.Empty;
            if (filterCriterias != null && filterCriterias.Count > 0 && filterCriterias.Any(f => f.Name.Equals(nameof(IPriceLaddering.SkuCode))))
            {
                sss = $"where s.code Like '%{filterCriterias.Find(f => f.Name.Equals(nameof(IPriceLaddering.SkuCode)))?.Value}%' ";
                filterCriterias.Remove(filterCriterias.Find(f => f.Name.Equals(nameof(IPriceLaddering.SkuCode))));
            }
            // Base SQL query
            var sql = new StringBuilder(@$"SELECT * FROM 
                                    (SELECT DISTINCT 
                                      --  s.code AS SkuCode,
                                        operating_unit AS OperatingUnit, 
                                        division AS Division, 
                                        branch AS Branch, 
                                        sales_office AS SalesOffice, 
                                        broad_customer_classification AS BroadCustomerClassification,
                                        B.name AS BranchName,
                                        '[' + SO.code + '] ' + SO.name AS SalesOfficeName
                                    FROM sku s
                                    INNER JOIN sku_ext_data sed ON s.uid = sed.sku_uid
                                    INNER JOIN sku_price_laddering SPL ON sed.product_category_id = SPL.product_category_id
                                    INNER JOIN branch B ON B.uid = SPL.branch
                                    LEFT JOIN sales_office SO ON SO.uid = SPL.sales_office {sss}) AS subquery");

            // SQL Count query for total number of records
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount.Append(@$"SELECT COUNT(1) AS Cnt FROM 
                                (SELECT DISTINCT 
                                   -- s.code AS SkuCode,
                                    operating_unit AS OperatingUnit, 
                                    division AS Division, 
                                    branch AS Branch, 
                                    sales_office AS SalesOffice, 
                                    broad_customer_classification AS BroadCustomerClassification,
                                    B.name AS BranchName,
                                    '[' + SO.code + '] ' + SO.name AS SalesOfficeName
                                FROM sku s
                                INNER JOIN sku_ext_data sed ON s.uid = sed.sku_uid
                                INNER JOIN sku_price_laddering SPL ON sed.product_category_id = SPL.product_category_id
                                INNER JOIN branch B ON B.uid = SPL.branch
                                LEFT JOIN sales_office SO ON SO.uid = SPL.sales_office {sss}) AS subquery");
            }

            // Parameters dictionary
            var parameters = new Dictionary<string, object?>();

            // Apply filter criteria
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<IPriceLaddering>(filterCriterias, sbFilterCriteria, parameters);
                sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    sqlCount.Append(sbFilterCriteria);
                }
            }

            // Apply sorting
            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                sql.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sql);
            }

            // Apply pagination
            if (pageNumber > 0 && pageSize > 0)
            {
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                else
                {
                    sql.Append($" ORDER BY Division OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
            }

            // Execute the main query
            IEnumerable<IPriceLaddering> priceLadderingDetails = await ExecuteQueryAsync<IPriceLaddering>(sql.ToString(), parameters);

            // Get total count if required
            int totalCount = -1;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            // Prepare the paged response
            PagedResponse<IPriceLaddering> pagedResponse = new PagedResponse<IPriceLaddering>
            {
                PagedData = priceLadderingDetails.ToList(),
                TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception ex)
        {
            throw new Exception("Error retrieving price ladders: " + ex.Message, ex);
        }
    }


    public async Task<List<IPriceLaddering>> GetRelatedData(string operatingUnit, string division, string branch, string salesOffice, string broadCustomerClassification)
    {
        Dictionary<string, object?> parameters = new()
        {
            {"OperatingUnit", operatingUnit},
            {"Division", division},
            {"Branch", branch},
            {"SalesOffice", salesOffice},
            {"BroadCustomerClassification", broadCustomerClassification}
        };

        string sql = @"
                    SELECT 
                        product_category_id AS ProductCategoryId, 
                        discount_type AS DiscountType, 
                        percentage_discount AS PercentageDiscount, 
                        start_date AS StartDate, 
                        end_date AS EndDate 
                    FROM sku_price_laddering
                    WHERE operating_unit = @OperatingUnit 
                      AND division = @Division 
                      AND branch = @Branch 
                      AND (ISNULL(@SalesOffice, '') = '' OR sales_office = @SalesOffice )
                      AND broad_customer_classification = @BroadCustomerClassification
                ";

        IEnumerable<IPriceLaddering> priceLadderingList = await ExecuteQueryAsync<IPriceLaddering>(sql, parameters);
        return priceLadderingList.ToList();
    }
    public async Task<List<ISKU>> GetSkuDetailsFromProductCategoryId(int productCategoryId)
    {
        Dictionary<string, object?> parameters = new()
        {
            {"productCategoryId", productCategoryId}
        };

        string sql = @"
                   select s.org_uid OrgUID, s.code Code, s.name Name from sku_ext_data sed 
                   inner join sku s on sed.sku_uid = s.uid 
                   where sed.product_category_id = @productCategoryId";

        IEnumerable<ISKU> skuDetails = await ExecuteQueryAsync<ISKU>(sql, parameters);
        return skuDetails.ToList();
    }

    public async Task<List<int>> GetProductCategoryIdsByStoreUid(string storeUid, DateTime date, string broadClassification = null, string branchUID = null)
    {
        try
        {
            //var sqlQuery =new StringBuilder(
            //    """
            //    SELECT DISTINCT spl.product_category_id
            //    FROM sku_price_laddering spl 
            //    INNER JOIN store s ON  
            //    s.broad_classification = spl.broad_customer_classification AND  @Date BETWEEN start_date AND end_date 
            //    INNER JOIN address a on a.linked_item_uid = s.uid AND a.[type] = 'Billing' 
            //    AND a.is_default = 1  AND  (spl.sales_office is null OR spl.sales_office = a.sales_office_uid) 
            //    AND a.branch_uid = spl.branch 
            //    """);
            //if(!string.IsNullOrEmpty(storeUid)|| !string.IsNullOrEmpty(broadClassification)|| !string.IsNullOrEmpty(branchUID))
            //{
            //    sqlQuery.Append(" where ");
            //}
            //if (!string.IsNullOrEmpty(storeUid))
            //{
            //    sqlQuery.Append(" s.uid =@StoreUID ");
            //    if(!string.IsNullOrEmpty(broadClassification) || !string.IsNullOrEmpty(branchUID))
            //    {
            //        sqlQuery.Append(" and ");
            //    }
            //}
            //if (!string.IsNullOrEmpty(broadClassification))
            //{
            //    sqlQuery.Append(" s.broad_classification =@BroadClassification ");
            //    if( !string.IsNullOrEmpty(branchUID))
            //    {
            //        sqlQuery.Append(" and ");
            //    }
            //}
            //if (!string.IsNullOrEmpty(branchUID))
            //{
            //    sqlQuery.Append(" a.branch_uid=@BranchUID ");
            //}
            string sql = string.Empty;
            if (!string.IsNullOrEmpty(storeUid))
            {
                sql = """
                                        WITH StoreCTE AS(
                    	SELECT * FROM VW_StoreImpAttributes WHERE store_uid = @StoreUID
                     )
                     SELECT DISTINCT spl.product_category_id
                     FROM sku_price_laddering spl 
                     JOIN StoreCTE ON 
                     spl.branch = StoreCTE.branch_uid AND spl.broad_customer_classification = StoreCTE.broad_classification 
                     AND (spl.sales_office = StoreCTE.sales_office_uid OR spl.sales_office is null ) 
                     AND @Date BETWEEN start_date AND end_date 
                    """;
            }
            else
            {
                sql = """
                                 SELECT DISTINCT spl.product_category_id
                                FROM sku_price_laddering spl 
                                WHERE branch = @Branch AND broad_customer_classification = @BroadClassification
                                AND @Date BETWEEN start_date AND end_date 
                                AND spl.sales_office IS  NULL
                    """;
            }

            var parameters = new
            {
                StoreUID = storeUid,
                BroadClassification = broadClassification,
                Branch = branchUID,
                Date = date.Date,
            };
            return await ExecuteQueryAsync<int>(sql.ToString(), parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
}

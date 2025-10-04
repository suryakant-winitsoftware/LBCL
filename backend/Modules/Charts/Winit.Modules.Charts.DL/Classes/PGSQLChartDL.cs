using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Modules.Chart.DL.Interfaces;
using Nest;
using System.Data;
using Winit.Modules.Chart.Models.Classes;
namespace Winit.Modules.Chart.DL.Classes
{
    public class PGSQLChartDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IChartDL
    {
        public PGSQLChartDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<Dictionary<string, object>> GetPurchaseOrderAndTallyDashBoard()
        {
            try
            {

                                                            var sql = @"-- Query 1
                                            SELECT 
                                                YEAR(poh.order_date) AS order_year,
                                                MONTH(poh.order_date) AS order_month,
                                                sum(poh.qty_count) AS qty_count
                                            	FROM purchase_order_header poh
                                            JOIN purchase_order_line pol 
                                                ON pol.purchase_order_header_uid = poh.uid
                                            GROUP BY 
                                                YEAR(poh.order_date),
                                                MONTH(poh.order_date)
                                            ORDER BY 
                                                order_year DESC, 
                                                order_month DESC;
                                            
                                            -- WHERE poh.status = 'invoiced';
                                            
                                            -- Query 2
                                            SELECT 
                                            YEAR(CAST(sot.basic_datetime_of_invoice AS DATE)) AS SalesYear,
                                            month(cast (sot.basic_datetime_of_invoice as date)) as SalesMoth,
                                            sum(solt.qty) as SalesQty
                                            FROM sales_order_tally sot
                                            JOIN sales_order_line_tally solt 
                                                ON solt.guid = sot.guid
                                            	group by YEAR(CAST(sot.basic_datetime_of_invoice AS DATE)),
                                            	month(cast (sot.basic_datetime_of_invoice as date))
                                            	order by Salesyear,SalesMoth
                                            
                                            --WHERE voucher_type_name = 'Purchase';
                                            
                                            -- Query 3
                                            SELECT 
                                                s.name AS distributor_name, 
                                                sum(poh.qty_count) AS purchase_qty,
                                               ROUND(
                                                    (SUM(poh.qty_count) * 100.0 / 
                                                     (SELECT SUM(poh.qty_count) 
                                                      FROM purchase_order_header poh 
                                                      JOIN purchase_order_line pol 
                                                          ON pol.purchase_order_header_uid = poh.uid)), 
                                                    2
                                                ) AS purchase_percentage
                                            FROM purchase_order_header poh
                                            JOIN purchase_order_line pol 
                                                ON pol.purchase_order_header_uid = poh.uid
                                            JOIN store s 
                                                ON s.uid = poh.org_uid
                                            -- WHERE poh.status = 'invoiced'
                                            GROUP BY s.name
                                            ORDER BY purchase_qty DESC;
                                            
                                            
                                            -- Query 4
                                            SELECT 
                                                SUM(poh.qty_count) AS total_qty,
                                                SUM(pol.available_qty) - SUM(pol.final_qty) AS available_qty,
                                                poh.uid
                                            FROM purchase_order_header poh
                                            JOIN purchase_order_line pol 
                                                ON pol.purchase_order_header_uid = poh.uid
                                            -- JOIN store s 
                                            --    ON s.uid = poh.org_uid
                                            GROUP BY 
                                                poh.uid;
                                            -- Query 5
                                            SELECT 
                                                sa.type AS product_category,
                                                LEFT(CAST(STRING_AGG(sa.code, ', ') AS VARCHAR(MAX)), 100) + '...' AS codes_sample,
                                                sum(pol.final_qty) AS code_count
                                            	--pol.purchase_order_header_uid
                                            FROM sku s
                                            JOIN sku_attributes sa 
                                                ON s.uid = sa.sku_uid
                                            	join purchase_order_line pol on pol.sku_uid=s.uid
                                            	
                                            GROUP BY 
                                                sa.type
                                            	--,pol.purchase_order_header_uid
                                            ORDER BY 
                                                sa.type;
                                            
                                            -- Query 6
                                            SELECT 
                                                sa.type AS product_category,
                                                LEFT(STRING_AGG(CAST(sa.code AS VARCHAR(MAX)), ', ') WITHIN GROUP (ORDER BY sa.code), 100) + '...' AS codes_sample,
                                                AVG(sp.price) AS average_price
                                            FROM sku s
                                            JOIN sku_attributes sa 
                                                ON s.uid = sa.sku_uid
                                            JOIN sku_price sp 
                                                ON s.uid = sa.sku_uid
                                            GROUP BY 
                                                sa.type
                                            ORDER BY 
                                                sa.type;";
                Dictionary<string, object> poAndTallydashBoardDic = new Dictionary<string, object>();
                DataSet ds = await ExecuteQueryDataSetAsync(sql);
                if (ds != null) {


                    if (ds.Tables.Count > 0)
                    {
                        var poAndTallyDashBoard = new List<POAndTallyDashBoard>();

                        var primaryData = new Dictionary<(int year, int month), string>();
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            int year = Convert.ToInt32(row["order_year"]);
                            int month = Convert.ToInt32(row["order_month"]);
                            string qtyCount = row["qty_count"].ToString();

                            primaryData[(year, month)] = qtyCount;
                        }

                        var secondaryData = new Dictionary<(int year, int month), string>();
                        foreach (DataRow row in ds.Tables[1].Rows)
                        {
                            int year = Convert.ToInt32(row["SalesYear"]);
                            int month = Convert.ToInt32(row["SalesMoth"]);
                            string salesQty = row["SalesQty"].ToString();

                            secondaryData[(year, month)] = salesQty;
                        }

                        var combinedKeys = new HashSet<(int year, int month)>(primaryData.Keys);
                        combinedKeys.UnionWith(secondaryData.Keys);

                        foreach (var key in combinedKeys)
                        {
                            poAndTallyDashBoard.Add(new POAndTallyDashBoard
                            {
                                Year = key.year,
                                Month = key.month,
                                Primary = primaryData.ContainsKey(key) ? primaryData[key] : null,
                                Secondary = secondaryData.ContainsKey(key) ? secondaryData[key] : null
                            });
                        }
                        if(poAndTallyDashBoard !=null && poAndTallyDashBoard.Any())
                        {
                            poAndTallydashBoardDic.Add("POAndTallyReport", poAndTallyDashBoard);
                        }

                        if (ds.Tables[2].Rows.Count > 0)
                        {
                            poAndTallydashBoardDic.Add("SumOfQtyByPartyName", ds.Tables[2]);
                        }
                        if (ds.Tables[3].Rows.Count > 0)
                        {
                            poAndTallydashBoardDic.Add("SumOfQty", ds.Tables[3]);
                        }
                        if (ds.Tables[4].Rows.Count > 0)
                        {
                            poAndTallydashBoardDic.Add("SumOfQtyByProductCategory", ds.Tables[4]);
                        }
                        if (ds.Tables[5].Rows.Count > 0)
                        {
                            poAndTallydashBoardDic.Add("AverageUnitPriceByProdctCategory", ds.Tables[5]);
                        }
                    }
                  
                }
                return poAndTallydashBoardDic;
            }
            catch
            {
                throw;
            }
        }

    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Commission.PostgreSQLCommissionRepository;

namespace WINITRepository.Classes.Commission
{
    public class SQLServerCommissionRepository : Interfaces.Commission.ICommissionRepository
    {
        private readonly string _connectionString;
        public SQLServerCommissionRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("SqlServer");
        }

        public async Task<int> ProcessCommission()
        {
            Dictionary<string, object> emptyParameters = new Dictionary<string, object>();
            //    List<CommissionKPI> commissionKPIList = await new DBManager.SqlServerDBManager<WINITSharedObjects.Models.CommissionKPI>(_connectionString)
            //.ExecuteQueryAsync(Commission_Query.CommissionKPI, emptyParameters);


            Task<List<WINITSharedObjects.Models.CommissionKPI>> commissionKPIListTask = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.CommissionKPI>(_connectionString)
                .ExecuteQueryAsync(Commission_Query.CommissionKPI, emptyParameters);

            Task<List<WINITSharedObjects.Models.CommissionKPICustomerMapping>> commissionKPICustomerMappingListTask = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.CommissionKPICustomerMapping>(_connectionString)
                .ExecuteQueryAsync(Commission_Query.CommissionKPICustomerMapping, emptyParameters);

            Task<List<WINITSharedObjects.Models.CommissionKPIProductMapping>> CommissionKPIProductMappingListTask = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.CommissionKPIProductMapping>(_connectionString)
                .ExecuteQueryAsync(Commission_Query.CommissionKPIProductMapping, emptyParameters);

            Task<List<WINITSharedObjects.Models.CommissionKPISlab>> CommissionKPISlabListTask = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.CommissionKPISlab>(_connectionString)
                .ExecuteQueryAsync(Commission_Query.CommissionKPISlab, emptyParameters);

            Task<List<WINITSharedObjects.Models.CommissionUserMapping>> CommissionUserMappingListTask = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.CommissionUserMapping>(_connectionString)
                .ExecuteQueryAsync(Commission_Query.CommissionUserMapping, emptyParameters);

            Task<List<WINITSharedObjects.Models.FactSales>> FactSalesListTask = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.FactSales>(_connectionString)
                .ExecuteQueryAsync(Commission_Query.FactSales, emptyParameters);

            Task<List<WINITSharedObjects.Models.FactSalestarget>> FactSalestargetListTask = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.FactSalestarget>(_connectionString)
                .ExecuteQueryAsync(Commission_Query.FactSalestarget, emptyParameters);

            Task<List<WINITSharedObjects.Models.CustomerAttributes>> CustomerAttributesListTask = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.CustomerAttributes>(_connectionString)
                .ExecuteQueryAsync(Commission_Query.CustomerAttributes, emptyParameters);


            Task<List<WINITSharedObjects.Models.ProductAttributes>> ProductAttributesListTask = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.ProductAttributes>(_connectionString)
                .ExecuteQueryAsync(Commission_Query.ProductAttributes, emptyParameters);

            await Task.WhenAll(commissionKPIListTask, commissionKPICustomerMappingListTask, CommissionKPIProductMappingListTask
                , CommissionKPISlabListTask, CommissionUserMappingListTask, FactSalesListTask, FactSalestargetListTask
                , CustomerAttributesListTask, ProductAttributesListTask);

            //await Task.WhenAll(commissionKPIListTask);

            IEnumerable<WINITSharedObjects.Models.CommissionKPI> commissionKPIList = commissionKPIListTask.Result;
            IEnumerable<WINITSharedObjects.Models.CommissionKPICustomerMapping> commissionKPICustomerMappingList = commissionKPICustomerMappingListTask.Result;
            IEnumerable<WINITSharedObjects.Models.CommissionKPIProductMapping> CommissionKPIProductMappingList = CommissionKPIProductMappingListTask.Result;
            IEnumerable<WINITSharedObjects.Models.CommissionKPISlab> CommissionKPISlabList = CommissionKPISlabListTask.Result;
            IEnumerable<WINITSharedObjects.Models.CommissionUserMapping> CommissionUserMappingList = CommissionUserMappingListTask.Result;
            IEnumerable<WINITSharedObjects.Models.FactSales> FactSalesList = FactSalesListTask.Result;
            IEnumerable<WINITSharedObjects.Models.FactSalestarget> FactSalestargetList = FactSalestargetListTask.Result;
            List<WINITSharedObjects.Models.CustomerAttributes> CustomerAttributesList = CustomerAttributesListTask.Result;
            IEnumerable<WINITSharedObjects.Models.ProductAttributes> ProductAttributesList = ProductAttributesListTask.Result;
            List<WINITSharedObjects.Models.CommissionUserPayout> CommissionUserPayoutReProcess = new List<WINITSharedObjects.Models.CommissionUserPayout>();
            foreach (WINITSharedObjects.Models.CommissionKPI objCommission in commissionKPIList)
            {
                Int64 comm_kpi_id = objCommission.kpi_type_id;
                Int64 comm_id = objCommission.commission_id;
                List<CommissionUserKPIPerformance> CommissionUserKPIPerformanceList = await new DBManager.SqlServerDBManager<WINITSharedObjects.Models.CommissionUserKPIPerformance>(_connectionString)
                                        .ExecuteQueryAsync(string.Format(Commission_Query.CommissionUserKPIPerformance, comm_kpi_id), emptyParameters);
                List<CommissionUserPayout> CommissionUserPayoutList = await new DBManager.SqlServerDBManager<WINITSharedObjects.Models.CommissionUserPayout>(_connectionString)
                                        .ExecuteQueryAsync(string.Format(Commission_Query.CommissionUserPayout, comm_id), emptyParameters);
                var lstCustomerMapping = commissionKPICustomerMappingList.Where(x => x.commission_kpi_id == objCommission.kpi_type_id);
                var lstProductMapping = CommissionKPIProductMappingList.Where(x => x.commission_kpi_id == objCommission.kpi_type_id);
                var KPICustomers = from cm in lstCustomerMapping
                                   join ca in CustomerAttributesList on new { a = cm.linked_customer_type, b = cm.linked_customer_code } equals new { a = ca.hierachy_type, b = ca.hierachy_code }  //  && cm.linked_customer_code == ca.hierachy_code
                                   select new { cm.commission_kpi_id, ca.customer_code };
                var KPIProducts = from cm in lstProductMapping
                                  join ca in ProductAttributesList on new { a = cm.linked_product_type, b = cm.linked_product_code } equals new { a = ca.hierachy_type, b = ca.hierachy_code }  //  && cm.linked_customer_code == ca.hierachy_code
                                  select new { cm.commission_kpi_id, ca.product_code };
                var KPISalesValue = from CU in CommissionUserMappingList
                                    join F in FactSalesList on new { user_code = CU.linked_user_code } equals new { user_code = F.user_code }
                                    join KC in KPICustomers on new { commission_kpi_id = comm_kpi_id, customer_code = F.customer_code } equals new { commission_kpi_id = KC.commission_kpi_id, customer_code = KC.customer_code }
                                    join KP in KPIProducts on new { commission_kpi_id = comm_kpi_id, product_code = F.product_code } equals new { commission_kpi_id = KP.commission_kpi_id, product_code = KP.product_code }
                                    group new { CU, F } by new { CU.commission_id, F.user_code } into g
                                    select new
                                    {
                                        g.Key.commission_id,
                                        g.Key.user_code,
                                        SalesValue = g.Sum(x => x.F.gross_amt)
                                    };
                var KPISalesTargetValue = from CU in CommissionUserMappingList
                                          join F in FactSalestargetList on new { user_code = CU.linked_user_code } equals new { user_code = F.user_code }
                                          join KC in KPICustomers on new { commission_kpi_id = comm_kpi_id, customer_code = F.customer_code } equals new { commission_kpi_id = KC.commission_kpi_id, customer_code = KC.customer_code }
                                          join KP in KPIProducts on new { commission_kpi_id = comm_kpi_id, product_code = F.product_code } equals new { commission_kpi_id = KP.commission_kpi_id, product_code = KP.product_code }
                                          group new { CU, F } by new { CU.commission_id, F.user_code } into g
                                          select new
                                          {
                                              g.Key.commission_id,
                                              g.Key.user_code,
                                              TargetValue = g.Sum(x => x.F.amount)
                                          };
                var SalesTargetPercent = from S in KPISalesValue
                                         join T in KPISalesTargetValue on S.user_code equals T.user_code
                                         select new { S.user_code, SalesPercent = S.SalesValue / T.TargetValue * 100, commission_kpi_id = comm_kpi_id };

                var KPISlabPayout = from SP in SalesTargetPercent
                                    join CK in CommissionKPISlabList on SP.commission_kpi_id equals CK.commission_kpi_id
                                    where SP.SalesPercent >= CK.commission_slab_from && SP.SalesPercent <= CK.commission_slab_to
                                    select new
                                    {
                                        SP.user_code,
                                        CK.commission_kpi_slab_payout,

                                    };




                StringBuilder sbInsUserKPI = new StringBuilder();
                StringBuilder sbUpdUserKPI = new StringBuilder();
                foreach (var obj in KPISlabPayout)
                {
                    if (CommissionUserPayoutReProcess.Count==0 || CommissionUserPayoutReProcess.Where(x => x.commission_id == comm_id).First() == null)
                    {
                        CommissionUserPayout _obj = new CommissionUserPayout();
                        _obj.commission_id = comm_id;
                        CommissionUserPayoutReProcess.Add(_obj);
                    }

                    if (CommissionUserKPIPerformanceList.Count >0 && CommissionUserKPIPerformanceList.Where(x => x.commission_kpi_id == comm_kpi_id && x.user_code == obj.user_code).First() != null)
                    {
                        sbUpdUserKPI.Append(string.Format(Commission_Query.CommissionUserKPIPerformance_Update, obj.commission_kpi_slab_payout, comm_kpi_id, obj.user_code));
                    }
                    else
                    {
                        sbInsUserKPI.Append(string.Format(Commission_Query.CommissionUserKPIPerformance_Insert, comm_id, obj.user_code, comm_kpi_id, obj.commission_kpi_slab_payout, obj.user_code, obj.user_code));
                        //commission_id,user_code,commission_kpi_id,commission_payout,CreatedBy,ModifiedBy
                    }
                }

                if (sbInsUserKPI.ToString() != "")
                {
                    await new DBManager.SqlServerDBManager<int>(_connectionString).ExecuteNonQueryAsync(sbInsUserKPI.ToString(), emptyParameters);
                }
                if (sbUpdUserKPI.ToString() != "")
                {
                    await new DBManager.SqlServerDBManager<int>(_connectionString).ExecuteNonQueryAsync(sbUpdUserKPI.ToString(), emptyParameters);
                }

            }
            foreach (WINITSharedObjects.Models.CommissionUserPayout objCommission in CommissionUserPayoutReProcess)
            {
                Int64 comm_id = objCommission.commission_id;
                List<CommissionUserKPIPerformance> CommissionUserKPIPerformanceList = await new DBManager.SqlServerDBManager<WINITSharedObjects.Models.CommissionUserKPIPerformance>(_connectionString)
                                        .ExecuteQueryAsync(string.Format(Commission_Query.CommissionUserKPIPerformance_ByCMSNID, comm_id), emptyParameters);
                List<CommissionUserPayout> CommissionUserPayoutList = await new DBManager.SqlServerDBManager<WINITSharedObjects.Models.CommissionUserPayout>(_connectionString)
                                        .ExecuteQueryAsync(string.Format(Commission_Query.CommissionUserPayout, comm_id), emptyParameters);
                var commissionPayout = from p in CommissionUserKPIPerformanceList
                                       group p by p.user_code into g
                                       select new
                                       {
                                           user_code = g.Key,
                                           total_payout = g.Sum(p => p.commission_payout)
                                       };
                StringBuilder sbInsUserPayout = new StringBuilder();
                StringBuilder sbUpdUserPayout = new StringBuilder();
                foreach (var obj in commissionPayout)
                {
                    if (CommissionUserPayoutList.Count >0 && CommissionUserPayoutList.Where(x => x.commission_id == comm_id && x.user_code == obj.user_code).First() != null)
                    {
                        sbUpdUserPayout.Append(string.Format(Commission_Query.CommissionUserPayout_Update, obj.total_payout, comm_id, obj.user_code));
                    }
                    else
                    {
                        sbInsUserPayout.Append(string.Format(Commission_Query.CommissionUserPayout_Insert, comm_id, obj.user_code, obj.total_payout, obj.user_code, obj.user_code));
                    }
                }
                if (sbInsUserPayout.ToString() != "")
                {
                    await new DBManager.SqlServerDBManager<int>(_connectionString).ExecuteNonQueryAsync(sbInsUserPayout.ToString(), emptyParameters);
                }
                if (sbUpdUserPayout.ToString() != "")
                {
                    await new DBManager.SqlServerDBManager<int>(_connectionString).ExecuteNonQueryAsync(sbUpdUserPayout.ToString(), emptyParameters);
                }
            }
            return 0; // Update the return value as necessary
        }
        public class Commission_Query
        {
            public const string CommissionKPI = "select C.commission_id,C.org_code,C.commission_name,CP.kpi_type_id, CP.kpi_name,CP.kpi_structure_type,CP.kpi_weight"
                + " from Commission C inner join CommissionKPI CP on CP.commission_id=C.commission_id" +
                " where  Getdate() Between C.commission_start_date and C.commission_end_date";

            public const string CommissionKPICustomerMapping = "select commission_kpi_id,linked_customer_code,linked_customer_type from CommissionKPICustomerMapping ";
            public const string CommissionKPIProductMapping = "select commission_kpi_id,linked_product_code,linked_product_type from CommissionKPIProductMapping ";
            public const string CommissionKPISlab = "select commission_kpi_slab_id,commission_kpi_id,commission_slab_from,commission_slab_to,commission_kpi_slab_payout from CommissionKPISlab";
            public const string CommissionUserMapping = "select commission_id,linked_user_type,linked_user_code from CommissionUserMapping";
            public const string FactSales = "Select  * from FactSales ";
            public const string FactSalestarget = "Select * from FactSalestarget ";
            public const string CustomerAttributes = "select customer_code,hierachy_type,hierachy_code,hierachy_value from CustomerAttributes";
            public const string ProductAttributes = "select * from ProductAttributes";
            public const string CommissionUserKPIPerformance_Insert = " Insert iNTO CommissionUserKPIPerformance(commission_id,user_code,commission_kpi_id,commission_payout,CreatedBy,ModifiedBy) " +
                "  values('{0}','{1}','{2}','{3}','{4}','{5}');";
            public const string CommissionUserKPIPerformance_Update = " update CommissionUserKPIPerformance SET commission_payout='{0}' where commission_kpi_id='{1}' and user_code='{2}';";
            public const string CommissionUserKPIPerformance = " select commission_id,user_code,commission_kpi_id,commission_payout from CommissionUserKPIPerformance where commission_kpi_id={0}";
            public const string CommissionUserKPIPerformance_ByCMSNID = " select commission_id,user_code,commission_kpi_id,commission_payout from CommissionUserKPIPerformance where commission_id={0}";
            public const string CommissionUserPayout = " select commission_id,user_code,commission_payout from CommissionUserPayout where commission_id='{0}'";
            public const string CommissionUserPayout_Update = " update CommissionUserPayout SET commission_payout='{0}' where commission_id='{1}' and user_code='{2}' ;";
            public const string CommissionUserPayout_Insert = " Insert INTO CommissionUserPayout(commission_id,user_code,commission_payout,CreatedBy,ModifiedBy) values('{0}','{1}','{2}','{3}','{4}');";
        }

    }
}

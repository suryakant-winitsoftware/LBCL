using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Survey.Model.Classes;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Survey.Model.Interfaces
{
    public interface IStoreRollingStatsModel:IBaseModel
    {
         string StoreUID { get; set; }
         decimal GrowthPercentage { get; set; }
         decimal GRPercentage { get; set; }
         decimal MonthlyGrowth { get; set; }
         decimal MTDSalesValue { get; set; }
         decimal MTDSalesVolume { get; set; }
         decimal MonthlyGrowthMty { get; set; }
         decimal MtySalesValue { get; set; }
         decimal MTYSalesVolume { get; set; }
         decimal LastOrderValue { get; set; }
         decimal AvgOrderValue { get; set; }
         decimal OutstandingPayment { get; set; }
         int NonProductiveCallCount { get; set; }
         int AvgLinePerCall { get; set; }
         int AvgCategoryPerCall { get; set; }
         decimal Last6OrderValueL3M { get; set; }
         decimal AvgBrandPerCall { get; set; }
         decimal LastMonthSalesValue { get; set; }
         decimal Last3MonthsAvgSalesValue { get; set; }
         decimal LMAvgSaleValue { get; set; }
         decimal LMNoOfInvoices { get; set; }
         decimal LMInvoiceValue { get; set; }
         decimal LastDeliveryAmount { get; set; }
         string LastDeliveryInvoiceNo { get; set; }
         DateTime? LastDeliveryDate { get; set; }
         decimal OutstandingAmount { get; set; }
         int OutstandingInvoiceCount { get; set; }
         int TasksOpen { get; set; }
         int NoOfInvPendingForDelivery { get; set; }
         int NoOfAssets { get; set; }
         int NoOfAccessPoints { get; set; }
         int NoOfVisibilities { get; set; }
         int AgeingOfCreditInDays { get; set; }
         int SKUListed { get; set; }
         DateTime? LastVisitDate { get; set; }
    }



}

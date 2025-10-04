using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;
using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace Winit.Modules.Survey.Model.Classes
{
    public class StoreRollingStatsModel : BaseModel, IStoreRollingStatsModel
    {
       // public StoreRollingStatsModel() { }
        public string StoreUID { get; set; }
        public decimal GrowthPercentage { get; set; }
        public decimal GRPercentage { get; set; }
        public decimal MonthlyGrowth { get; set; }
        public decimal MTDSalesValue { get; set; }
        public decimal MTDSalesVolume { get; set; }
        public decimal MonthlyGrowthMty { get; set; }
        public decimal MtySalesValue { get; set; }
        public decimal MTYSalesVolume { get; set; }
        public decimal LastOrderValue { get; set; }
        public decimal AvgOrderValue { get; set; }
        public decimal OutstandingPayment { get; set; }
        public int NonProductiveCallCount { get; set; }
        public int AvgLinePerCall { get; set; }
        public int AvgCategoryPerCall { get; set; }
        public decimal Last6OrderValueL3M { get; set; }
        public decimal AvgBrandPerCall { get; set; }
        public decimal LastMonthSalesValue { get; set; }
        public decimal Last3MonthsAvgSalesValue { get; set; }
        public decimal LMAvgSaleValue { get; set; }
        public decimal LMNoOfInvoices { get; set; }
        public decimal LMInvoiceValue { get; set; }
        public decimal LastDeliveryAmount { get; set; }
        public string LastDeliveryInvoiceNo { get; set; }
        public DateTime? LastDeliveryDate { get; set; }
        public decimal OutstandingAmount { get; set; }
        public int OutstandingInvoiceCount { get; set; }
        public int TasksOpen { get; set; }
        public int NoOfInvPendingForDelivery { get; set; }
        public int NoOfAssets { get; set; }
        public int NoOfAccessPoints { get; set; }
        public int NoOfVisibilities { get; set; }
        public int AgeingOfCreditInDays { get; set; }
        public int SKUListed { get; set; }
        public DateTime? LastVisitDate { get; set; }

    }
    }

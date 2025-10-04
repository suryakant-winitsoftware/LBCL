using SyncManagerModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Interfaces
{
    public interface IPriceLaddering : ISyncBaseModel
    {
        public long SyncLogId { get; set; }
        public string? UID { get; set; }
        public decimal LadderingId { get; set; }
        public string OperatingUnit { get; set; }
        public string Division { get; set; }
        public decimal ProductCategoryId { get; set; }
        public string Branch { get; set; }
        public string SalesOffice { get; set; }
        public string BroadCustomerClassification { get; set; }
        public string DiscountType { get; set; }
        public decimal PercentageDiscount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

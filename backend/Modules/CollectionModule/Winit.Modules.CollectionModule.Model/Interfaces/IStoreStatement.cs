using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IStoreStatement
    {
        public string? Org { get; set; }
        public string? Store { get; set; }
        public string? TransactionType { get; set; }
        public string? OrderType { get; set; }
        public string? ReferenceNumber { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal Amount { get; set; }
        public decimal ClosingBalance { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}

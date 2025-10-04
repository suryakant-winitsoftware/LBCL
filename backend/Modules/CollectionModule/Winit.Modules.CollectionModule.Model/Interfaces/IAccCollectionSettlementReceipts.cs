using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IAccCollectionSettlementReceipts : IBaseModel
    {
        public string SessionUserCode { get; set; }

        public int Id { get; set; }

       

        public string ReceiptNumber { get; set; }

        public string TargetType { get; set; }

        public string TargetUID { get; set; }

        public decimal? SettledAmount { get; set; }

        public decimal? PaidAmount { get; set; }

        public string AccCollectionSettlementUID { get; set; }
    }
}

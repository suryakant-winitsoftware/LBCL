using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Provisioning.Model.Interfaces
{
    public interface IProvisionItemView : IBaseModel
    {
        public string ChannelPartner { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public string TransactionCode { get; set; }
        public string ModelNo { get; set; }
        public int Qty { get; set; }
        public decimal UnitAmount { get; set; }
        public string Description { get; set; }
    }
}

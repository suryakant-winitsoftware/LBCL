using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IStoreBrandDealingIn
    {
        public string? Brand { get; set; }
        public string? RsTo { get; set; }
        public int? Sn { get; set; }
    }
}

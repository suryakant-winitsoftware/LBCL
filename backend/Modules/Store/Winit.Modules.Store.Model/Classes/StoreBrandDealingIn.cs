using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class StoreBrandDealingIn : IStoreBrandDealingIn
    {
        public string? Brand { get; set; }
        public string? RsTo { get; set; }
        public int? Sn { get; set; }

    }
}

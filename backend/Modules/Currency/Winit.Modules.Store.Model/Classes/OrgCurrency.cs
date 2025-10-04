using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Currency.Model.Interfaces;

namespace Winit.Modules.Currency.Model.Classes
{
    public class OrgCurrency  : Currency, IOrgCurrency
    {
        public bool IsPrimary { get; set; }
        public string OrgUID { get; set; }
        public string CurrencyUID { get; set; }
    }
}

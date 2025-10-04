using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Currency.Model.Interfaces
{
    public interface IOrgCurrency : ICurrency
    {
        bool IsPrimary { get; set; }
        string OrgUID { get; set; }
        string CurrencyUID { get; set; }
    }
}

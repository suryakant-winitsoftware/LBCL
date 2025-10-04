using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IExchangeRate: IBaseModel
    {
        public string FromCurrencyUID { get; set; }
        public string ToCurrencyUID { get; set; }
        public string Source { get; set; }
        public decimal Rate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public bool IsActive { get; set; }

        public decimal CurrencyAmount { get; set; }
        public decimal ConvertedAmount { get; set; }
        public decimal CurrencyAmount_Temp { get; set; }
        public decimal ConvertedAmount_Temp { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal OriginalAmount_Temp { get; set; }
        public decimal TotalAmount_Temp { get; set; }
        public decimal TotalAmount { get; set; }
    }
}

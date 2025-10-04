using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.Model.Classes
{
    public class AccCollectionCurrencyDetails : BaseModel, IAccCollectionCurrencyDetails
    {
        public string? acc_collection_uid { get; set; }
        public string? currency_uid { get; set; }
        public string? default_currency_uid { get; set; }
        public decimal? default_currency_exchange_rate { get; set; }
        public decimal? amount { get; set; }
        public decimal? default_currency_amount { get; set; }
        public decimal? final_default_currency_amount { get; set; }
    }
}

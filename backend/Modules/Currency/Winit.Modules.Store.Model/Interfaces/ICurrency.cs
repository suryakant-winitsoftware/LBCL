using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Currency.Model.Interfaces
{
    public interface ICurrency : IBaseModel
    {
        public string? Name { get; set; }
        public string? Symbol { get; set; }
        public int Digits { get; set; }
        public string? Code { get; set; }
        public string? FractionName { get; set; }
        public decimal RoundOffMinLimit { get; set; }
        public decimal RoundOffMaxLimit { get; set; }
    }
}

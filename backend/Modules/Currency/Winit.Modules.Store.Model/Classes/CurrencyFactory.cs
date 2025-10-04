using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Currency.Model.Classes
{
    public class CurrencyFactory:IFactory
    {
        public readonly string _classname;
        public CurrencyFactory(string classname)
        {
            _classname = classname;
        }
        public object? CreateInstance() 
        { 
            switch(_classname)
            {
                case CurrencyModule.Currency: return new Currency();
                default: return null;
            }
        }
    }
}

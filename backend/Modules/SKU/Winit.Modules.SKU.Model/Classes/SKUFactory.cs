using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.SKU.Model.Classes
{
    public class SKUFactory:IFactory
    {
        private readonly string _className;
        public SKUFactory(string classname)
        {
            _className = classname;
        }
        public object? CreateInstance()
        {
            switch(_className)
            {
                case SKUModule.SKUPrice:
                    return new SKUPrice();
                case SKUModule.SKUPriceList:
                    return new SKUPriceList();
                default: return null;
            }
        }
    }
}

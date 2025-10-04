using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.SKUClass.Model.Classes
{
    public class SKUClassFactory:IFactory
    {
        public readonly string _classname;
        public SKUClassFactory(string classname)
        {
            _classname = classname;
        }
        public object? CreateInstance()
        {
            switch (_classname)
            {
                case SKUClassModule.SKUClass: return new SKUClass();
                case SKUClassModule.SKUClassGroup: return new SKUClassGroup();
                case SKUClassModule.SKUClassGroupItems: return new SKUClassGroupItems();
                default: return null;
            }
        }
    }
}

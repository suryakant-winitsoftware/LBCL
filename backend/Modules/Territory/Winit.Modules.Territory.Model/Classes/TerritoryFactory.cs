using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Territory.Model.Classes
{
    public class TerritoryFactory : IFactory
    {
        private readonly string _className;

        public TerritoryFactory(string className)
        {
            _className = className;
        }

        public object? CreateInstance()
        {
            switch (_className)
            {
                case TerritoryModule.Territory:
                    return new Territory();
                default:
                    return null;
            }
        }
    }
}

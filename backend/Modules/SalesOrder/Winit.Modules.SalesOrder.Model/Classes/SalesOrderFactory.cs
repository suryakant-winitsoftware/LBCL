using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SalesOrder.Model.Interfaces;

namespace Winit.Modules.SalesOrder.Model.Classes
{
    public class SalesOrderFactory: IFactory
    {
        public readonly string _ClassName;
        public SalesOrderFactory(string className ) 
        { 
            _ClassName = className;
        }

        public object? CreateInstance()
        {
           switch (_ClassName)
           {
                case SalesOrderModule.SalesOrderViewModel: return new SalesOrderViewModel();
                case SalesOrderModule.SalesOrder: return new SalesOrder();
                case SalesOrderModule.SalesOrderLine: return new SalesOrderLine();
                
                default: return null;
           }
        }
    }
}

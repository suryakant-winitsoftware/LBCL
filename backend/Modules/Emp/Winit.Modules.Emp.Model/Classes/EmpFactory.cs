using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Emp.Model.Classes
{
    public class EmpFactory:IFactory
    {
        public readonly string _classname;
        public EmpFactory(string classname)
        {
            _classname = classname;
        }
        public object? CreateInstance()
        {
            switch (_classname)
            {
                case EmpModule.Emp: return new Emp();
                case EmpModule.EmpInfo:return new EmpInfo();
                default: return null;
            }
        }
    }
}

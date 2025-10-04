using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Org.Model.Classes
{
    public class OrgFactory:IFactory
    {
        private readonly string _className;

        public OrgFactory(string className)
        {
            _className = className;
        }

        public object? CreateInstance()
        {
            switch (_className)
            {
                case OrgModule.Org:
                    return new Org();
               
                default:
                    return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.JobPosition.Model.Classes
{
    public class JobPositionFactory : IFactory
    {
        private readonly string _className;
        public JobPositionFactory(string className)
        {
            _className = className;
        }
        public object? CreateInstance()
        {
           switch(_className)
           {
                case JobPositionModule.JobPosition:  return new JobPosition();
                default: return null;
           }
        }
    }
}

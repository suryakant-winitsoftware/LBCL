using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IAccUser
    {
        public string UserName { get; set; }
        public string UserCode { get; set; }
        public string Password { get; set; }
    }
}

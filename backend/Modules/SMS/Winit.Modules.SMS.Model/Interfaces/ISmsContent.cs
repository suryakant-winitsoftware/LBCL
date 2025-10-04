using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.SMS.Model.Interfaces
{
    public interface ISmsContent
    {
        public string Message { get; set; }
        public List<string> MobileNumbers { get; set; }
    }
}

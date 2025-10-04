using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.User.Model.Interface
{
    public interface IUserLocationMapping
    {
        string MappingCode { get; set; }
        string MappingName { get; set; }
        bool IsActive {  get; set; }
    }
}

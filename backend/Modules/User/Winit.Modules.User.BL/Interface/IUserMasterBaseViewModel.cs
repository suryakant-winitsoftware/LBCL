using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.User.BL.Classes;

namespace Winit.Modules.User.BL.Interface
{
    public interface IUserMasterBaseViewModel
    {
         Task GetUserMasterData(string logInID);
    }
}

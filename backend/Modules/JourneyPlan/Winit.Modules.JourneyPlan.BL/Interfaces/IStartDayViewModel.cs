using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Route.Model.Interfaces;

namespace Winit.Modules.JourneyPlan.BL.Interfaces
{
    public interface IStartDayViewModel
    {
        public IUserJourney UserJourney { get; set; }
        public IRoute SelectedRoute { get; set; }
        string FolderName { get; set; }
        List<IFileSys> ImageFileSysList { get; set; }
        public Task<int> UploadStartDayUserJourney();
    }
}

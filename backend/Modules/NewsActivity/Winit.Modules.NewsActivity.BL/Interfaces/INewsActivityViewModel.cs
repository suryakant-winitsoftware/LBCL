using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.BL.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.NewsActivity.Models.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.NewsActivity.BL.Interfaces
{
    public interface INewsActivityViewModel : IFilesysViewModel
    {
        List<ISelectionItem> FileTypes { get; set; }
        string SelectedFile { get; set; }
        INewsActivity NewsActivity { get; set; }
        bool IsNew { get; set; }
        bool IsNews { get; }
        bool IsAdvertisement { get; }
        bool IsBusinessCommunication { get; }
        Task PopulateViewModel();
    }
}

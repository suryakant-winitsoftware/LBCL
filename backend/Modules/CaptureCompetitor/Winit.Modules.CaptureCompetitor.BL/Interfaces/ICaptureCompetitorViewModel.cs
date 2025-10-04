using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CaptureCompetitor.Model.Classes;
using Winit.Modules.CaptureCompetitor.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CaptureCompetitor.BL.Interfaces
{
    public interface ICaptureCompetitorViewModel
    {
        ICaptureCompetitor SelectedcaptureCompetitor { get; set; }
        ICaptureCompetitor CreateCaptureCompetitor { get; set; }
        List<ICaptureCompetitor> ListOfCaptureCompetitors { get; set; }
        List<IFileSys> ImageFileSysList { get; set; }
        List<ISelectionItem> Brands { get; set; }
        Task PopulateViewModel();
        Task GetAllCapatureCampitators();
        Task<int> SaveCompitator();
        //Task<List<ItemModel>> GetItemsAsync();
        //Task<List<ISelectionItem>> GetBrandsAsync();
        //Task<List<ItemModel>> SearchItemsAsync(string searchString);
    }
}

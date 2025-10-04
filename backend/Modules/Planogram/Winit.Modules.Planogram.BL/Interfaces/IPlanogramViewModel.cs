using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Planogram.Model.Classes;
using Winit.Modules.Planogram.Model.Interfaces;

namespace Winit.Modules.Planogram.BL.Interfaces
{
    public interface IPlanogramViewModel
    {
        // Data Properties
        List<IPlanogramCategory> Categories { get; set; }
        List<IPlanogramRecommendation> Recommendations { get; set; }
        List<IFileSys> ImageFileSysList { get; set; }

        // State Properties
        bool ShowCategoryList { get; set; }
        bool IsLoadingCategories { get; set; }
        string SelectedCategoryCode { get; set; }
        string RecommendedImagePath { get; set; }
        string ExecutionHeaderUID { get; set; }
        string CurrentPlanogramSetupUID { get; set; }
        bool IsImageCaptured { get; set; }

        // File Capture Properties
        //FileCaptureData FileCaptureData { get; set; }
        string FolderPath { get; set; }

        // Methods
        Task PopulateViewModel();
        Task LoadCategories();
        Task SelectCategory(string categoryCode);
        Task CreateExecutionHeader();
        Task ProceedToExecution();
        Task BackToCategories();
        Task SubmitPlanogram();
        string GetCategoryImage(string categoryCode);
        Task OnImageCapture((string fileName, string folderPath) data);
        void OnImageDeleteClick(string fileName);
        //OSOI
        Task<bool> OnPlanogramOSOISubmit(PlanogramExecutionV1 planogramExecutionV1);
        Task<IPlanogramSetupV1> GetPlanogramSetupByUID(string planogramSetupUID);
    }
}

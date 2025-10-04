using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.CaptureCompetitor.Model.Classes;
using Winit.Modules.CaptureCompetitor.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CaptureCompetitor.BL.Classes
{
    public class CaptureCompetitorAppViewModel : CaptureCompetitorBaseViewModel
    {
        Winit.Modules.CaptureCompetitor.BL.Interfaces.ICaptureCompetitorBL _captureCompetitorBL { set; get; }
        Winit.Modules.FileSys.BL.Interfaces.IFileSysBL _fileSysBL { set; get; }
        public CaptureCompetitorAppViewModel(IServiceProvider serviceProvider, IFilterHelper filter,
            ISortHelper sorter, IListHelper listHelper, IAppUser appUser, IAppSetting appSetting,
            IDataManager dataManager, IAppConfig appConfig, Winit.Modules.CaptureCompetitor.BL.Interfaces.ICaptureCompetitorBL captureCompetitorBL, Winit.Modules.FileSys.BL.Interfaces.IFileSysBL fileSysBL)
            : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting, dataManager, appConfig)
        {
            _captureCompetitorBL = captureCompetitorBL;
            _fileSysBL = fileSysBL;
        }


        public override async Task GetTheBrands()
        {
            Brands = await LoadBrandOptions();
        }

        public override async Task GetAllCapatureCampitators()
        {
            var sortCriterias = new List<SortCriteria>();
            var pageNumber = 1;
            var pageSize = int.MaxValue;
            var filterCriterias = new List<FilterCriteria>();
            var isCountRequired = false;

            var pagedResponse = await _captureCompetitorBL.GetCaptureCompetitorDetails(
                sortCriterias,
                pageNumber,
                pageSize,
                filterCriterias,
                isCountRequired
            );
            ListOfCaptureCompetitors = pagedResponse.PagedData.ToList();
        }
        public override async Task<int> SaveCompitator()
        {
            CreateCapitator(CreateCaptureCompetitor);

            int result = await _captureCompetitorBL.CreateCaptureCompetitor(CreateCaptureCompetitor);

            if (result >= 1 && ImageFileSysList != null && ImageFileSysList.Any())
            {
                try
                {
                    await SaveCapturedImagesAsync();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error while saving captured images: {ex.Message}");
                    throw;
                }
            }
            return result;
        }

        private async Task SaveCapturedImagesAsync()
        {
            // Save the images in bulk using the provided file system logic
            int saveResult = await _fileSysBL.CreateFileSysForBulk(ImageFileSysList);

            // Throw an exception if the image save operation failed
            if (saveResult < 0)
            {
                throw new Exception("Failed to save the captured images.");
            }
        }

        public async Task SaveCapturedImage()
        {
            // CustomerSignatureFilePath = Path.Combine(CustomerSignatureFolderPath, CustomerSignatureFileName);
            //UserSignatureFilePath = Path.Combine(UserSignatureFolderPath, UserSignatureFileName);
            int retValue = await _fileSysBL.CreateFileSysForBulk(ImageFileSysList);
            if (retValue < 0) throw new Exception("Failed to save the Signatures");
        }
        public async Task PrepareSignatureFields()
        {
            //string baseFolder = Path.Combine(_appConfigs.BaseFolderPath, FileSysTemplateControles.GetSignatureFolderPath("Collection", _dataManager.GetData("ReceiptNumber").ToString()));
            //CustomerSignatureFolderPath = baseFolder;
            //UserSignatureFolderPath = baseFolder;
            // CustomerSignatureFileName = $"Customer_{_dataManager.GetData("ReceiptNumber").ToString()}.png";
            //UserSignatureFileName = $"User_{_dataManager.GetData("ReceiptNumber").ToString()}.png";
        }
        private void CreateCapitator(Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor captureCompetitor)
        {
            captureCompetitor.UID = GenerateUID();
            captureCompetitor.StoreUID = _appUser.SelectedCustomer.StoreUID;
            captureCompetitor.Status = 1;
            captureCompetitor.StoreHistoryUID = _appUser.SelectedCustomer.StoreHistoryUID;
            captureCompetitor.BeatHistoryUID = _appUser.SelectedCustomer.BeatHistoryUID;
            captureCompetitor.RouteUID = _appUser.SelectedRoute.UID;
            captureCompetitor.ActivityDate = DateTime.Now;
            captureCompetitor.JobPositionUID = _appUser.SelectedJobPosition.UID;
            captureCompetitor.EmpUID = _appUser?.Emp?.UID;
            captureCompetitor.OurPrice = 0.0m;
            captureCompetitor.OtherPrice = 0.0m;
            captureCompetitor.CreatedBy = _appUser?.Emp?.UID;
            captureCompetitor.ModifiedBy = _appUser?.Emp?.UID;
            captureCompetitor.CreatedTime = DateTime.Now;
            captureCompetitor.ModifiedTime = DateTime.Now;
        }
        private string GenerateUID()
        {
            return Guid.NewGuid().ToString();
        }
        // temp
        private async Task<List<ISelectionItem>> LoadBrandOptions()
        {
            return new List<ISelectionItem>
        {
            new SelectionItem { UID = "B1", Label = "ACLF5005" },
            new SelectionItem { UID = "B2", Label = "ACLF5226" },
            new SelectionItem { UID = "B3", Label = "ACLF5331" },
        };
        }
       
    }
}

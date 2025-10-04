using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Planogram.BL.Interfaces;
using Winit.Modules.Planogram.DL.Interfaces;
using Winit.Modules.Planogram.Model.Classes;
using Winit.Modules.Planogram.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Planogram.BL.Classes;

public class PlanogramAppViewModel : PlanogramBaseViewModel
{
    Winit.Modules.FileSys.BL.Interfaces.IFileSysBL _fileSysBL { set; get; }
    public PlanogramAppViewModel(
        IServiceProvider serviceProvider,
        IFilterHelper filter,
        ISortHelper sorter,
        IListHelper listHelper,
        IAppUser appUser,
        IAppSetting appSetting,
        IDataManager dataManager,
        IAppConfig appConfig,
        IPlanogramBL planogramBL, Winit.Modules.FileSys.BL.Interfaces.IFileSysBL fileSysBL, IPlanogramV1BL planogramV1BL)
        : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting, dataManager, appConfig, planogramBL, planogramV1BL)
    {
        _fileSysBL = fileSysBL;
    }

    // Override LoadCategories with specific implementation
    public override async Task LoadCategories()
    {
        try
        {
            IsLoadingCategories = true;

            // Use business layer to get categories
            Categories = await _planogramBL.GetPlanogramCategoriesAsync();

            // Additional app-specific logic can be added here
            // For example: filtering, sorting, validation, etc.
        }
        catch (Exception ex)
        {
            // Log error or handle app-specific error handling
            throw new Exception($"Failed to load planogram categories: {ex.Message}", ex);
        }
        finally
        {
            IsLoadingCategories = false;
        }
    }

    // Override SelectCategory with specific implementation
    public override async Task SelectCategory(string categoryCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(categoryCode))
                throw new ArgumentException("Category code cannot be null or empty");

            SelectedCategoryCode = categoryCode;

            // Load recommendations for this category using BL
            Recommendations = await _planogramBL.GetPlanogramRecommendationsByCategoryAsync(categoryCode);

            // Get the first recommendation's details
            var firstRecommendation = Recommendations?.FirstOrDefault();
            if (firstRecommendation != null)
            {
                RecommendedImagePath = firstRecommendation.RecommendedImagePath;
                CurrentPlanogramSetupUID = firstRecommendation.UID;
            }

            // Create execution header
            await CreateExecutionHeader();

            ShowCategoryList = false;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to select category '{categoryCode}': {ex.Message}", ex);
        }
    }

    // Override CreateExecutionHeader with specific implementation
    public override async Task CreateExecutionHeader()
    {
        try
        {
            //var header = new PlanogramExecutionHeaderImpl
            //{
            //    BeatHistoryUID = _appUser?.CurrentBeatHistoryUID ?? "BEAT_HISTORY_UID",
            //    StoreHistoryUID = _appUser?.CurrentStoreHistoryUID ?? "STORE_HISTORY_UID",
            //    StoreUID = _appUser?.CurrentStoreUID ?? "STORE_UID",
            //    JobPositionUID = _appUser?.JobPositionUID ?? "JOB_POSITION_UID",
            //    RouteUID = _appUser?.CurrentRouteUID ?? "ROUTE_UID",
            //    Status = "draft",
            //    CreatedBy = _appUser?.UID ?? "CURRENT_USER_UID"
            //};

           // ExecutionHeaderUID = await _planogramBL.CreatePlanogramExecutionHeaderAsync(header);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create execution header: {ex.Message}", ex);
        }
    }

    // Override SubmitPlanogram with specific implementation
    public override async Task SubmitPlanogram()
    {
        //if (!IsImageCaptured)
        //{
        //    throw new InvalidOperationException("Please capture an execution image before submitting.");
        //}

        //if (string.IsNullOrEmpty(ExecutionHeaderUID))
        //{
        //    throw new InvalidOperationException("Execution header not found. Please try again.");
        //}

        //if (string.IsNullOrEmpty(CurrentPlanogramSetupUID))
        //{
        //    throw new InvalidOperationException("Planogram setup not found. Please try again.");
        //}

        try
        {

            if (string.IsNullOrEmpty(ExecutionHeaderUID))
            {
                var header = new PlanogramExecutionHeader
                {
                    UID = Guid.NewGuid().ToString(),
                    BeatHistoryUID = _appUser.SelectedBeatHistory.UID,
                    StoreHistoryUID = _appUser.SelectedCustomer.StoreHistoryUID,
                    StoreUID = _appUser.SelectedCustomer.StoreUID,
                    JobPositionUID = _appUser.SelectedJobPosition.UID,
                    RouteUID = _appUser.SelectedRoute.UID,
                    Status = "Complete",
                    CreatedBy = _appUser?.Emp.UID,
                    CreatedTime = DateTime.Now,
                    ModifiedBy = _appUser?.Emp.UID,
                    ModifiedTime = DateTime.Now
                };

                await _planogramBL.CreatePlanogramExecutionHeaderAsync(header);
                ExecutionHeaderUID = header.UID;
            }

            // 2. Create Detail (currently one, may become multiple)
            var detail = new PlanogramExecutionDetail
            {
                UID = Guid.NewGuid().ToString(),
                PlanogramExecutionHeaderUID = ExecutionHeaderUID,
                PlanogramSetupUID = CurrentPlanogramSetupUID,
                ExecutedOn = DateTime.Now,
                IsCompleted = true,
                CreatedBy = _appUser?.Emp.UID,
                CreatedTime = DateTime.Now,
                ModifiedBy = _appUser?.Emp.UID,
                ModifiedTime= DateTime.Now
            };

            await _planogramBL.CreatePlanogramExecutionDetailAsync(detail);

            // Optionally store in viewmodel for tracking multiple
            //Details.Add(detail);

            // await _planogramBL.CreatePlanogramExecutionDetailAsync(detail);
            if ( ImageFileSysList != null && ImageFileSysList.Any())
            {
                try
                {
                    ImageFileSysList?.ForEach(e => e.LinkedItemUID = $"{ExecutionHeaderUID}");

                    await SaveCapturedImagesAsync();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error while saving captured images: {ex.Message}");
                    throw;
                }
            }
            // Reset state after successful submission
            await BackToCategories();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to submit planogram execution: {ex.Message}", ex);
        }
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
    // Override OnImageCapture with specific implementation
    public override async Task OnImageCapture((string fileName, string folderPath) data)
    {
        try
        {
            //IsImageCaptured = true;

            //// Add captured file to the list
            //var fileSys = new FileSys
            //{
            //    FileName = data.fileName,
            //    FilePath = data.folderPath,
            //    CreatedOn = DateTime.Now,
            //    CreatedBy = _appUser?.UID ?? "CURRENT_USER_UID"
            //};

            //ImageFileSysList.Add(fileSys);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to handle image capture: {ex.Message}", ex);
        }
    }

    // Override OnImageDeleteClick with specific implementation
    public override void OnImageDeleteClick(string fileName)
    {
        try
        {
            // Remove from the list
            var fileToRemove = ImageFileSysList.FirstOrDefault(f => f.FileName == fileName);
            if (fileToRemove != null)
            {
                ImageFileSysList.Remove(fileToRemove);
            }

            // Update captured state
            IsImageCaptured = ImageFileSysList.Any();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete image: {ex.Message}", ex);
        }
    }
    //OSOI
    
    public override async Task<bool> OnPlanogramOSOISubmit(PlanogramExecutionV1 planogramExecutionV1)
    {
        try
        {
            string uid = await _planogramV1BL.CreatePlanogramExecutionV1Async(planogramExecutionV1);
            if (!string.IsNullOrEmpty(uid) && ImageFileSysList != null && ImageFileSysList.Any())
            {
                try
                {
                    ImageFileSysList?.ForEach(e => e.LinkedItemUID = $"{planogramExecutionV1.UID}");
                    await SaveCapturedImagesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error while saving captured images: {ex.Message}");
                    throw;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public override async Task<IPlanogramSetupV1> GetPlanogramSetupByUID(string planogramSetupUID)
    {
        try
        {
            IPlanogramSetupV1 planogramSetupV1 = await _planogramV1BL.GetPlanogramSetupV1ByUIDAsync(planogramSetupUID);
            return planogramSetupV1;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get planogram setup by UID '{planogramSetupUID}': {ex.Message}", ex);
        }
    }
}
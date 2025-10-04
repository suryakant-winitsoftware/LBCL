using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Planogram.BL.Interfaces;
using Winit.Modules.Planogram.DL.Interfaces;
using Winit.Modules.Planogram.Model.Classes;
using Winit.Modules.Planogram.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Planogram.BL.Classes;

public abstract class PlanogramBaseViewModel : IPlanogramViewModel
{
    // Data Properties
    public List<IPlanogramCategory> Categories { get; set; }
    public List<IPlanogramRecommendation> Recommendations { get; set; }
    public List<IFileSys> ImageFileSysList { get; set; }

    // State Properties
    public bool ShowCategoryList { get; set; } = true;
    public bool IsLoadingCategories { get; set; } = false;
    public string SelectedCategoryCode { get; set; } = null;
    public string RecommendedImagePath { get; set; } = null;
    public string ExecutionHeaderUID { get; set; } = null;
    public string CurrentPlanogramSetupUID { get; set; } = null;
    public bool IsImageCaptured { get; set; } = false;

    // File Capture Properties
    //public FileCaptureData FileCaptureData { get; set; }
    public string FolderPath { get; set; }

    // Dependencies
    protected readonly IPlanogramBL _planogramBL;
    protected readonly IPlanogramV1BL _planogramV1BL;
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IFilterHelper _filter;
    protected readonly ISortHelper _sorter;
    protected readonly IListHelper _listHelper;
    protected readonly IAppUser _appUser;
    protected readonly IAppSetting _appSetting;
    protected readonly IDataManager _dataManager;
    protected readonly IAppConfig _appConfig;

    // Constructor
    protected PlanogramBaseViewModel(
        IServiceProvider serviceProvider,
        IFilterHelper filter,
        ISortHelper sorter,
        IListHelper listHelper,
        IAppUser appUser,
        IAppSetting appSetting,
        IDataManager dataManager,
        IAppConfig appConfig,
        IPlanogramBL planogramBL, IPlanogramV1BL planogramV1BL)
    {
        _serviceProvider = serviceProvider;
        _filter = filter;
        _sorter = sorter;
        _listHelper = listHelper;
        _appUser = appUser;
        _appSetting = appSetting;
        _dataManager = dataManager;
        _appConfig = appConfig;
        _planogramBL = planogramBL;
        _planogramV1BL = planogramV1BL;

        // Initialize collections
        Categories = new List<IPlanogramCategory>();
        Recommendations = new List<IPlanogramRecommendation>();
        ImageFileSysList = new List<IFileSys>();

        // Initialize file capture data
        //InitializeFileCaptureData();

        // Set folder path
        //FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Images");
        
    }

    // Virtual methods - can be overridden in derived classes
    public virtual async Task PopulateViewModel()
    {
        await LoadCategories();
    }

    public virtual async Task LoadCategories()
    {
        try
        {
            IsLoadingCategories = true;
            Categories = await _planogramBL.GetPlanogramCategoriesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to load categories: {ex.Message}", ex);
        }
        finally
        {
            IsLoadingCategories = false;
        }
    }

    public virtual async Task SelectCategory(string categoryCode)
    {
        try
        {
            SelectedCategoryCode = categoryCode;

            // Load recommendations for this category
            Recommendations = await _planogramBL.GetPlanogramRecommendationsByCategoryAsync(categoryCode);

            // Get the first recommendation's image path
            var firstRecommendation = Recommendations.FirstOrDefault();
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
            throw new Exception($"Failed to select category: {ex.Message}", ex);
        }
    }

    public virtual async Task CreateExecutionHeader()
    {
        // Base implementation - can be overridden
        await Task.CompletedTask;
    }

    public virtual async Task ProceedToExecution()
    {
        // Base implementation - can be overridden
        await Task.CompletedTask;
    }

    public virtual async Task BackToCategories()
    {
        ShowCategoryList = true;
        SelectedCategoryCode = null;
        RecommendedImagePath = null;
        ExecutionHeaderUID = null;
        CurrentPlanogramSetupUID = null;
        IsImageCaptured = false;
        Recommendations.Clear();
        await Task.CompletedTask;
    }

    public virtual async Task SubmitPlanogram()
    {
        // Base implementation - can be overridden
        await Task.CompletedTask;
    }

    public virtual string GetCategoryImage(string categoryCode)
    {
        return categoryCode?.ToUpper() switch
        {
            "DRINKING WATER" => "/images/categories/drinking-water.png",
            "FOOD PRODUCTS" => "/images/categories/food-products.png",
            "GATORADE" => "/images/categories/gatorade.png",
            "JUICES AND OTHERS" => "/images/categories/juices.png",
            "OTHER BEVERAGES" => "/images/categories/beverages.png",
            "SNACKS AND DIPS" => "/images/categories/snacks.png",
            _ => "/images/categories/default.png"
        };
    }

    public virtual async Task OnImageCapture((string fileName, string folderPath) data)
    {
        IsImageCaptured = true;
        await Task.CompletedTask;
    }

    public virtual void OnImageDeleteClick(string fileName)
    {
        IsImageCaptured = false;
    }

    // Private helper method
    //private void InitializeFileCaptureData()
    //{
    //    FileCaptureData = new FileCaptureData
    //    {
    //        AllowedExtensions = new List<string> { ".jpg", ".png" },
    //        IsCameraAllowed = true,
    //        IsGalleryAllowed = true,
    //        MaxNumberOfItems = 5,
    //        MaxFileSize = 10 * 1024 * 1024, // 10 MB
    //        EmbedLatLong = true,
    //        EmbedDateTime = true,
    //        LinkedItemType = "Planogram",
    //        LinkedItemUID = "PlanogramUID",
    //        EmpUID = _appUser?.UID ?? "EmployeeUID",
    //        JobPositionUID = _appUser?.JobPositionUID ?? "JobPositionUID",
    //        IsEditable = true,
    //        Files = new List<FileSys>()
    //    };
    //}

    public virtual async Task<bool> OnPlanogramOSOISubmit(PlanogramExecutionV1 planogramExecutionV1 )
    {
        return true;
    }
    public virtual async Task<IPlanogramSetupV1> GetPlanogramSetupByUID(string planogramSetupUID)
    {
        // Base implementation - can be overridden
        return default;
    }
}

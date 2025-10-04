using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using WINITMobile.Data;

namespace WINITMobile.Pages.Planogram;

partial class Planogram
{
    private string folderPath = Path.Combine(FileSystem.AppDataDirectory, "Images");
    protected override async Task OnInitializedAsync()
    {
        try
        {
            _backbuttonhandler.ClearCurrentPage();
            await _viewModel.PopulateViewModel();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert("Error", "Failed to initialize page");
        }
        finally
        {
            await base.OnInitializedAsync();
        }
    }

    private async Task SelectCategory(string categoryCode)
    {
        try
        {
            await _viewModel.SelectCategory(categoryCode);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert("Error", "Failed to load category details");
        }
    }

    private async Task ProceedToExecution()
    {
        try
        {
            await _viewModel.ProceedToExecution();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert("Error", "Failed to proceed to execution");
        }
    }

    private async Task BackToCategories()
    {
        try
        {
            await _viewModel.BackToCategories();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert("Error", "Failed to navigate back");
        }
    }

    private async Task SubmitPlanogram()
    {
        try
        {
            await _viewModel.SubmitPlanogram();
            await _alertService.ShowSuccessAlert("Success", "Planogram executed successfully.");
            _navigationManager.NavigateTo("/CustomerCall");
        }
        catch (InvalidOperationException ex)
        {
            await _alertService.ShowErrorAlert("Validation Error", ex.Message);
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert("Error", "Failed to submit planogram execution");
        }
    }




    public string ImgFolderPath { get; set; }

   
  

    // Event handler for capturing an image
    private void OnImageDeleteClick(string fileName)
    {
        IFileSys fileSys = _viewModel.ImageFileSysList.Find
        (e => e.FileName == fileName);
        if (fileSys is not null) _viewModel.ImageFileSysList.Remove(fileSys);
    }
    //private string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Images");
    private async Task OnImageCapture((string fileName, string folderPath) data)
    {
        string relativePath = "";

        string selectedCustomerCode = _appUser?.SelectedJobPosition?.UID;

        if (!string.IsNullOrWhiteSpace(selectedCustomerCode))
        {
            relativePath = FileSysTemplateControles.GetPlanogramImageFolderPath(selectedCustomerCode) ?? "";
        }

        try
        {
            IFileSys fileSys = ConvertFileSys("Planogram", "11","Planogram", "Image",
       data.fileName, _appUser.Emp?.Name, data.folderPath);

            fileSys.SS = -1;
            fileSys.RelativePath = relativePath;
            _viewModel.ImageFileSysList.Add(fileSys);
            // _viewModel.FolderPathImages = folderPath;
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert("Error", "Failed to capture image");
        }
   
    }

    private FileCaptureData fileCaptureData = new FileCaptureData
    {
        AllowedExtensions = new List<string> { ".jpg", ".png" },
        IsCameraAllowed = true,
        IsGalleryAllowed = true,
        MaxNumberOfItems = 5,
        MaxFileSize = 10 * 1024 * 1024, // 10 MB
        EmbedLatLong = true,
        EmbedDateTime = true,
        LinkedItemType = "Planogram",
        LinkedItemUID = "PlanogramUID",
        EmpUID = "EmployeeUID",
        JobPositionUID = "JobPositionUID",
        IsEditable = true,
        Files = new List<FileSys>()
    };

 
}
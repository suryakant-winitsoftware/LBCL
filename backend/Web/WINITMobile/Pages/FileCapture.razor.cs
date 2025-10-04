using Microsoft.AspNetCore.Components;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.CommonUtilities.Common;
using WINITMobile.Data;
namespace WINITMobile.Pages;

public partial class FileCapture : ComponentBase
{
    private bool popupVisible = false;
    private FileSys selectedImage;
    [Parameter]
    public string QuestionId { get; set; }
    [Parameter]
    public FileCaptureData CaptureData { get; set; } = new FileCaptureData();
    [Parameter]
    public bool ShowCapturedImages { get; set; } = true;
    [Parameter]
    public string FolderPath { get; set; }
    [Parameter]
    public bool IsGalleryAllow { get; set; } = false;
    [Parameter]
    public string FileName { get; set; }
    [Parameter]
    public EventCallback<(string fileName, string folderPath)> OnImageCapture { get; set; }
    [Parameter]
    public EventCallback<(string questionId, string fileName, string folderPath)> OnImageSurveyCapture { get; set; }
    [Parameter]
    public EventCallback<string> OnImageDeleteClick { get; set; }
    [Parameter]
    public bool OpenCameraOnLoad { get; set; } = false;
    public List<FileSys> Files { get; set; } = new List<FileSys>();
    private int FileIndex = 1;
    //private async Task ShowOptions(string result)
    //{
    //    //if (CaptureData.IsCameraAllowed && CaptureData.IsGalleryAllowed) { } else {}
    //    try
    //    {
    //        if (result == "Camera")
    //        {
    //            await CaptureFile();
    //        }
    //        else if (result == "Gallery")
    //        {
    //            await PickAndShow();
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //    }
    //}
    private async Task ShowOptions()
    {
        try
        {
            if (IsGalleryAllow)
            {
                var result = await _alertService.ShowConfirmationReturnType("Upload Picture", "Please choose a source to upload your picture.", "Camera", "Gallery");

                if (result)
                {
                    await CaptureFile();
                }
                else
                {
                    await PickAndShow();
                }

            }
            else
            {
                await CaptureFile();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in ShowOptions: " + ex.Message);
        }
    }
    protected override async Task OnParametersSetAsync()
    {

        if (OpenCameraOnLoad)
        {
            OpenCameraOnLoad = false;
            await CaptureFile();
        }
    }

    //public async Task CaptureFile()
    //{
    //    if (CaptureData != null)
    //    {
    //        if (CaptureData.IsCameraAllowed && MediaPicker.Default.IsCaptureSupported)
    //        {
    //            // Capture a photo using the device camera
    //            FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

    //            if (photo != null)
    //            {
    //                string  file = AppDomain.CurrentDomain.BaseDirectory;//await FilePicker.PickAsync(filePickerOptions);

    //                if (file != null)
    //                {
    //                    string folderPath = Path.Combine(file, "Images/ReturnOrder");
    //                    if(!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath); 
    //                    // Save the captured photo to the selected file
    //                    using Stream sourceStream = await photo.OpenReadAsync();
    //                    var filePath = Path.Combine(folderPath, photo.FileName);
    //                    using FileStream localFileStream = File.OpenWrite(filePath);
    //                    await sourceStream.CopyToAsync(localFileStream);
    //                    CaptureData.Files.Add(new FileSys
    //                    {
    //                        FileName = photo.FileName,
    //                        FilePath = filePath
    //                    });
    //                    StateHasChanged();
    //                    // Display the captured image
    //                }
    //            }
    //        }
    //        //else if (CaptureData.IsGalleryAllowed)
    //        //{
    //        //    // Pick a photo from the device's gallery
    //        //    FileResult photo = await MediaPicker.Default.PickPhotoAsync();

    //        //    if (photo != null)
    //        //    {
    //        //        // Handle the selected photo and add it to the Files list
    //        //        // (similar to the camera capture logic above)
    //        //    }
    //        //}
    //        else
    //        {
    //            await Shell.Current.DisplayAlert("Oops", "Capture options are not supported on your device", "Ok");
    //        }
    //    }
    //}

    public async Task CaptureFile()
    {
        if (CaptureData != null)
        {
            if (CaptureData.IsCameraAllowed && MediaPicker.Default.IsCaptureSupported)
            {
                // Capture a photo using the device camera
                FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo != null)
                {
                    // Store the image in the cache directory
                    string fileName = $@"{FileName}_{FileIndex}_{photo.FileName}";
                    string filePath = Path.Combine(FolderPath, fileName);

                    // Save the captured photo to the cache file
                    using Stream sourceStream = await photo.OpenReadAsync();
                    //using FileStream localFileStream = File.OpenWrite(localFilePath);
                    using MemoryStream memoryStream = new MemoryStream();
                    await sourceStream.CopyToAsync(memoryStream);

                    // Add the file information to your data model
                    string base64String = Convert.ToBase64String(memoryStream.ToArray());
                    await CommonFunctions.SaveBase64StringToFile(base64String,
                        FolderPath, fileName);
                    Files.Add(
                        new FileSys
                        {
                            FileName = fileName,
                            FileData = base64String
                        });
                    if (QuestionId != null)
                    {
                        await OnImageSurveyCapture.InvokeAsync((QuestionId, fileName, FolderPath));
                    }
                    else
                    {
                        await OnImageCapture.InvokeAsync((fileName, FolderPath));
                    }
                    // Update the UI if needed
                    FileIndex++;
                    StateHasChanged();


                }
            }

        }

    }



    private async Task PickAndShow()
    {
        try
        {
            FileResult photo = await MediaPicker.Default.PickPhotoAsync();

            if (photo != null)
            {
                // Create a file path or use a base64 string for display
                using Stream sourceStream = await photo.OpenReadAsync();
                using MemoryStream memoryStream = new MemoryStream();
                await sourceStream.CopyToAsync(memoryStream);

                // Convert to base64 string (for storage/display)
                string base64String = Convert.ToBase64String(memoryStream.ToArray());

                // Save the file (or you can add it directly to the Files collection)
                string fileName = $@"{FileName}_{FileIndex}_{photo.FileName}";
                string filePath = Path.Combine(FolderPath, fileName);
                await CommonFunctions.SaveBase64StringToFile(base64String, FolderPath, fileName);

                // Optionally add the image to the Files collection or update UI
                Files.Add(new FileSys
                {
                    FileName = fileName,
                    FileData = base64String
                });
                if (QuestionId != null)
                {
                    await OnImageSurveyCapture.InvokeAsync((QuestionId, fileName, FolderPath));
                }
                else
                {
                    await OnImageCapture.InvokeAsync((fileName, FolderPath));
                }
                FileIndex++;
                // Update the UI after selecting the photo
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error selecting image: {ex.Message}");
        }
    }

    public string ConvertToBase64(string filePath)
    {
        try
        {
            byte[] imageBytes = File.ReadAllBytes(filePath);
            string base64String = Convert.ToBase64String(imageBytes);
            return $"data:image/jpeg;base64,{base64String}";
        }
        catch (Exception ex)
        {
            return string.Empty;
        }
    }

    private void ShowImage(FileSys file)
    {
        // Show the selected image in a popup (using Blazor component state)
        selectedImage = file;
        popupVisible = true;
        //StateHasChanged();  // Request a re-render after state change
    }

    private void ClosePopup()
    {
        // Close the image popup (using Blazor component state)
        popupVisible = false;
        //StateHasChanged();  // Request a re-render after state change
    }

    private void DeleteFile(FileSys file)
    {
        //Delete the selected file from the list (using Blazor component state)
        Files.Remove(file);
        OnImageDeleteClick.InvokeAsync(file.FileName);
        popupVisible = false;
    }

    // Helper method to get the file URL based on the file path
    private string GetFileUrl(string filePath)
    {
        // You may need to adjust the URL based on your specific file storage path
        return $"file://{filePath}";
    }

}
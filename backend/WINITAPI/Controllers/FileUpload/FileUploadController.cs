using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using Winit.Modules.FileSys.Model.Classes;
using WINITSharedObjects.Models;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FileUploadController : ControllerBase
{
    private const string FolderPath = @"Data";
    private readonly IWebHostEnvironment _hostingEnvironment;

    public FileUploadController(IWebHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
    }

    [HttpPost, HttpGet]
    [Route("UploadFile")]
    public ImageUploadResponse UploadFile()
    {

        string logDate = DateTime.Now.ToString("yyyyMMddHHmmssFFF");
        string logRequestFileName = "FileUploadController_" + logDate;
        string SubFolder = "";
        ImageUploadResponse response = new ImageUploadResponse() { SavedImgsPath = new(),FailedImages=new() };
        int filesCount = HttpContext.Request.Form.Files.Count;
        if (filesCount > 0)
        {
            if (HttpContext.Request.Form.ContainsKey("folderPath"))
            {
                SubFolder = HttpContext.Request.Form["folderPath"];
            }

            // Get File(s) 
            for (int i = 0; i < filesCount; i++)
            {
                var formFile = HttpContext.Request.Form.Files[i];

                if (formFile != null && formFile.Length > 0)
                {
                    string SubFolderPath = Path.Combine(FolderPath, SubFolder);
                    string fullimagePath = Path.Combine(SubFolderPath, formFile.FileName);
                    try
                    {
                       

                        string targetFolderPath = Path.Combine(_hostingEnvironment.ContentRootPath, SubFolderPath);
                        if (!Directory.Exists(targetFolderPath))
                        {
                            Directory.CreateDirectory(targetFolderPath);
                        }

                        using (var stream = new FileStream(Path.Combine(targetFolderPath, formFile.FileName), FileMode.Create))
                        {
                            formFile.CopyTo(stream);
                        }
                        response.Status = ImageUploadStatus.SUCCESS;
                        response.Message = "File '" + formFile.FileName + "' successfully saved. Path:" + fullimagePath;
                        response.SavedImgsPath.Add(fullimagePath);
                    }
                    catch (Exception ex)
                    {
                        response.Status = ImageUploadStatus.FAILURE;
                        response.FailedImages.Add(fullimagePath);
                        response.Message = "Unable to process your request.";
                    }
                }
                else
                {
                    response.Status = ImageUploadStatus.FAILURE;
                    response.Message = "No file found.";
                }
            }
        }
        else
        {
            response.Status = ImageUploadStatus.FAILURE;
            response.Message = "No file found.";
        }

        return response;
    }

    [HttpPost]
    [Route("DeleteFile")]
    public IActionResult DeleteFile(string imagePath)
    {
        try
        {
            // Check if the image path is provided
            if (string.IsNullOrEmpty(imagePath))
            {
                return BadRequest("Image path is required.");
            }

            // Map the image path to the physical path
            string physicalPath = Path.Combine(_hostingEnvironment.ContentRootPath, imagePath);

            // Check if the image file exists
            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound("Image not found.");
            }

            // Delete the image file
            System.IO.File.Delete(physicalPath);

            return Ok("Image deleted successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while deleting the image: {ex.Message}");
        }
    }
    [HttpPut]
    [Route("MoveFiles")]
    public ImageUploadResponse MoveFiles([FromQuery] string SourcePath, string DestinationPath)
    {
        ImageUploadResponse imageUploadResponse = new ImageUploadResponse();
        try
        {
            SourcePath = Path.Combine(_hostingEnvironment.ContentRootPath, SourcePath);
            DestinationPath = Path.Combine(_hostingEnvironment.ContentRootPath, DestinationPath);
            DirectoryInfo source = new DirectoryInfo(SourcePath);
            DirectoryInfo dest = new DirectoryInfo(DestinationPath);

            // Check if destination directory already exists
            if (!dest.Exists)
            {
                dest.Create();
                dest.Refresh();
            }

            // Move each file in the source directory
            foreach (FileInfo file in source.GetFiles())
            {
                string destFile = Path.Combine(dest.FullName, file.Name);

                // If file exists in destination, replace it
                if (System.IO.File.Exists(destFile))
                {
                    System.IO.File.Delete(destFile);
                }

                file.MoveTo(destFile);
            }

            imageUploadResponse.Status = ImageUploadStatus.SUCCESS;
            imageUploadResponse.Message = "Moved Successfully!";
            Directory.Delete(SourcePath, true);
        }
        catch (Exception ex)
        {
            imageUploadResponse.Status = ImageUploadStatus.FAILURE;
            imageUploadResponse.Message = $"{ex.Message}";
        }
        return imageUploadResponse;
    }
    [HttpPost]
    [Route("MoveFile")]
    public ImageUploadResponse MoveFile([FromBody] List<FileSys> fileSys)
    {
        //need to pass filesys obj loop through need to move aall files
        ImageUploadResponse imageUploadResponse = new ImageUploadResponse() { SavedImgsPath = new(), FailedImages = new() };
        foreach (FileSys file in fileSys)
        {
            string sourceFolderPath = Path.Combine(_hostingEnvironment.ContentRootPath, $"{file.TempPath}");
            string destinationFolderPath = Path.Combine(_hostingEnvironment.ContentRootPath, $"{file.RelativePath}");




            try
            {
                if (!Directory.Exists(destinationFolderPath))
                {
                    Directory.CreateDirectory(destinationFolderPath);
                }
                // Check if the source file exists
                string sourceFileName = Path.Combine(sourceFolderPath, file.FileName);
                string destinationFileName = Path.Combine(destinationFolderPath, file.FileName);
                if (!System.IO.File.Exists(sourceFileName))
                {
                    imageUploadResponse.Status = ImageUploadStatus.FAILURE;
                    imageUploadResponse.Message = ($"Source file does not exist");
                }

                // Move the file to the destination folder
                System.IO.File.Move(sourceFileName, destinationFileName);
                imageUploadResponse.Status = ImageUploadStatus.SUCCESS;
                imageUploadResponse.SavedImgsPath.Add(destinationFileName);
                imageUploadResponse.Message = ($"File moved successfully");
            }
            catch (Exception ex)
            {
                imageUploadResponse.Status = ImageUploadStatus.FAILURE;
                imageUploadResponse.Message = ($"Error: {ex.Message}");
            }
        }
        return imageUploadResponse;
    }

    //[HttpPost, HttpGet]
    //public ImageUploadResponse UploadFile()
    //{

    //    string logDate = DateTime.Now.ToString("yyyyMMddHHmmssFFF");
    //    string logRequestFileName = "FileUploadController_" + logDate;
    //    string SubFolder = "";
    //    ImageUploadResponse response = new ImageUploadResponse();

    //    if (HttpContext.Request.Form.Files.Count > 0)
    //    {
    //        if (HttpContext.Request.Form.ContainsKey("folderPath"))
    //        {
    //            SubFolder = HttpContext.Request.Form["folderPath"];
    //        }

    //        // Get File(s) 
    //        var formFile = HttpContext.Request.Form.Files[0];

    //        if (formFile != null && formFile.Length > 0)
    //        {
    //            try
    //            {
    //                string SubFolderPath = Path.Combine(FolderPath, SubFolder);
    //                string fullimagePath = Path.Combine(SubFolderPath, formFile.FileName);

    //                string targetFolderPath = Path.Combine(_hostingEnvironment.ContentRootPath, SubFolderPath);
    //                if (!Directory.Exists(targetFolderPath))
    //                {
    //                    Directory.CreateDirectory(targetFolderPath);
    //                }

    //                using (var stream = new FileStream(Path.Combine(targetFolderPath, formFile.FileName), FileMode.Create))
    //                {
    //                    formFile.CopyTo(stream);
    //                }
    //                response.Status = ImageUploadStatus.SUCCESS;
    //                response.Message = "File '" + formFile.FileName + "' successfully saved. Path:" + fullimagePath;
    //                response.ImgPath = fullimagePath;
    //            }
    //            catch (Exception ex)
    //            {
    //                response.Status = ImageUploadStatus.FAILURE;
    //                response.Message = "Unable to process your request.";
    //            }
    //        }
    //        else
    //        {
    //            response.Status = ImageUploadStatus.FAILURE;
    //            response.Message = "No file found.";
    //        }
    //    }
    //    else
    //    {
    //        response.Status = ImageUploadStatus.FAILURE;
    //        response.Message = "No file found.";
    //    }

    //    return response;
    //}


}

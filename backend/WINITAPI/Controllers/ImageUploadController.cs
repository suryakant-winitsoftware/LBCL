using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WINITSharedObjects.Models;

namespace WINITAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class ImageUploadController : ControllerBase
    {

       

        [HttpPost("UploadImage")]
        public ActionResult<ImageUploadResponse> UploadImage([FromForm] string subFolder)
        {
            ImageUploadResponse response = new ImageUploadResponse();

            if (HttpContext.Request.Form.Files.Any())
            {
                var file = HttpContext.Request.Form.Files["file1"];

                if (file != null && file.Length > 0)
                {
                    try
                    {
                        if (file.Length > 5 * 1024 * 1024) // 5MB limit
                        {
                            response.Status = ImageUploadStatus.FAILURE;
                            response.Message = "File size exceeds the limit of 5MB.";
                            return response;
                        }

                        string fileExtension = Path.GetExtension(file.FileName).ToLower();
                        if (fileExtension != ".png" && fileExtension != ".jpg" && fileExtension != ".txt")
                        {
                            response.Status = ImageUploadStatus.FAILURE;
                            response.Message = "Only PNG, JPG, and TXT file extensions are allowed.";
                            return response;
                        }

                        string subFolderPath = Path.Combine(@"D:\WINITAPINEW\WINITAPI\Data\", subFolder);
                        string fullImagePath = Path.Combine(subFolderPath, file.FileName);

                        if (!Directory.Exists(subFolderPath))
                        {
                            Directory.CreateDirectory(subFolderPath);
                        }

                        using (var stream = new FileStream(fullImagePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        response.Status = ImageUploadStatus.SUCCESS;
                        response.Message = $"Image '{file.FileName}' successfully saved. Path: {fullImagePath}";
                        response.ImgPath = fullImagePath;
                    }
                    catch (Exception ex)
                    {
                        response.Status = ImageUploadStatus.FAILURE;
                        response.Message = "Unable to process your request.";
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

        /* public ActionResult<ImageUploadResponse> UploadImage([FromForm] string subFolder)
         {
             ImageUploadResponse response = new ImageUploadResponse();

             if (HttpContext.Request.Form.Files.Any())
             {
                 var file = HttpContext.Request.Form.Files["file1"];

                 if (file != null && file.Length > 0)
                 {
                     try
                     {
                         string subFolderPath = Path.Combine(@"D:\WINITAPINEW\WINITAPI\Data\", subFolder);
                         string fullImagePath = Path.Combine(subFolderPath, file.FileName);

                         if (!Directory.Exists(subFolderPath))
                         {
                             Directory.CreateDirectory(subFolderPath);
                         }

                         using (var stream = new FileStream(fullImagePath, FileMode.Create))
                         {
                             file.CopyTo(stream);
                         }

                         response.Status = ImageUploadStatus.SUCCESS;
                         response.Message = $"Image '{file.FileName}' successfully saved. Path: {fullImagePath}";
                         response.ImgPath = fullImagePath;
                     }
                     catch (Exception ex)
                     {
                         response.Status = ImageUploadStatus.FAILURE;
                         response.Message = "Unable to process your request.";
                     }
                 }
             }
             else
             {
                 response.Status = ImageUploadStatus.FAILURE;
                 response.Message = "No file found.";
             }

             return response;
         }*/

    }
}
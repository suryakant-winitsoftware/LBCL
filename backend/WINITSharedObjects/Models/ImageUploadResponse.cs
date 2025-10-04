using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models
{
    public class ImageUploadResponse
    {
        public ImageUploadStatus Status { get; set; }
        public string Message { get; set; }
        public string ImgPath { get; set; }
        public List<string> SavedImgsPath { get; set; }
        public List<string> FailedImages { get; set; }
    }

    public enum ImageUploadStatus
    {
        FAILURE = 0,
        SUCCESS = 1,
        UNKNOWN = 2
    }

}

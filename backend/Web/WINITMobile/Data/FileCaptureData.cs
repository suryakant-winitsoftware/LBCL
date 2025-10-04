using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WINITMobile.Data
{
    public class FileCaptureData
    {
        public List<string> AllowedExtensions { get; set; }
        public bool IsCameraAllowed { get; set; }
        public bool IsGalleryAllowed { get; set; }
        public int MaxNumberOfItems { get; set; }
        public int MaxFileSize { get; set; }
        public bool EmbedLatLong { get; set; }
        public bool EmbedDateTime { get; set; }
        public string LinkedItemType { get; set; }
        public string LinkedItemUID { get; set; }
        public string EmpUID { get; set; }
        public string JobPositionUID { get; set; }
        public bool IsEditable { get; set; }
        public List<FileSys> Files { get; set; }
    }
}

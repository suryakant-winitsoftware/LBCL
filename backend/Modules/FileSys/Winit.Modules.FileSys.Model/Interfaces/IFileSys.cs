using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.FileSys.Model.Interfaces
{
    public interface IFileSys : Winit.Modules.Base.Model.IBaseModel
    {
        public string LinkedItemType {get; set;}
        public string LinkedItemUID {get; set;}
        public string FileSysType {get; set;}
        public string FileType {get; set;}
        public string FileData {get; set;}
        public string ParentFileSysUID {get; set;}
        public bool IsDirectory {get; set;}
        public string FileName {get; set;}
        public string DisplayName {get; set;}
        public long FileSize {get; set;}
        public string RelativePath {get; set;}
        public string TempPath { get; set;}
        public string Latitude {get; set;}
        public string Longitude {get; set;}
        public string CreatedByJobPositionUID {get; set;}
        public string CreatedByEmpUID { get; set; }
        public bool IsDefault { get; set; }
    }
}

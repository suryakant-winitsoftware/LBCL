using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Newtonsoft.Json;


namespace Winit.Modules.FileSys.Model.Classes
{
    public class FileSys : BaseModel,Winit.Modules.FileSys.Model.Interfaces.IFileSys
    {
        public string LinkedItemType {get; set;}
        public string LinkedItemUID {get; set;}
        public string FileSysType {get; set;}
        public string FileData {get; set;}
        public string FileType {get; set; }
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
        [JsonIgnore]
        public Dictionary<string, List<string>>? RequestUIDDictionary { get; set; }

        public FileType FileSysFileType { get; set;}
    }
    public enum FileType
    {
        None = 0,
        Image=1,
        Pdf=2,
        Doc=3,
        Video =4
    }
}

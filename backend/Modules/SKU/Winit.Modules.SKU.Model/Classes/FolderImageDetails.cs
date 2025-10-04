using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Interfaces;

namespace Winit.Modules.SKU.Model.Classes
{
    public class FolderImageDetails :IFolderImageDetails
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Folderpath { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.SKU.Model.Interfaces
{
    public interface IFolderImageDetails
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Folderpath { get; set; }
    }
}

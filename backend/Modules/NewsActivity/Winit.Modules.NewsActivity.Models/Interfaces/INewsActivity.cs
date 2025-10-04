using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.FileSys.Model.Interfaces;

namespace Winit.Modules.NewsActivity.Models.Interfaces
{
    public interface INewsActivity : IBaseModel
    {
        string ActivityType { get; set; }
        string Title { get; set; }
        string Description { get; set; }
        DateTime? PublishDate { get; set; }
        bool IsActive { get; set; }
        List<IFileSys> FilesysList { get; set; }
    }
}

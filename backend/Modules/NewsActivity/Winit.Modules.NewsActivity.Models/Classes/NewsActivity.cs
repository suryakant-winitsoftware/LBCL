using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.NewsActivity.Models.Interfaces;

namespace Winit.Modules.NewsActivity.Models.Classes
{
    public class NewsActivity : BaseModel, INewsActivity
    {
        public string ActivityType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? PublishDate { get; set; }
        public bool IsActive { get; set; }
        public List<IFileSys> FilesysList { get; set; } = [];
    }
}

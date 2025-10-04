using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Email.Model.Classes
{
    public class EmailFromBodyModelDTO
    {
        public string UID { get; set; }
        public string TemplateName { get; set; }
        public List<string> TemplateNames { get; set; }
        public List<string> OrderUIDs { get; set; }

    }
}

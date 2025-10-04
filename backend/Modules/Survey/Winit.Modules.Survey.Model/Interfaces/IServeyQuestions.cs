using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Survey.Model.Interfaces
{
    public interface IServeyQuestions
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Type { get; set; } // single-select, multi-select, text
        public List<IServeyOptions> Options { get; set; }
    }
}

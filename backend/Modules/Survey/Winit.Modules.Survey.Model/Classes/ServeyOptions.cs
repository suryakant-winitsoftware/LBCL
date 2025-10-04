using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Survey.Model.Interfaces;

namespace Winit.Modules.Survey.Model.Classes
{
    public class ServeyOptions : IServeyOptions
    {
        public string Label { get; set; }
        public int Points { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class SerialNumbers : ISerialNumbers
    {
        public int SlNo { get; set; }
        public int SerialNumber { get; set; }
    }
}

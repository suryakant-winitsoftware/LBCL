using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.UOM.Model.Interfaces
{
    public interface IUOMType : IBaseModel
    {
        public string Name { get; set; }
        public string Label { get; set; }
    }
}

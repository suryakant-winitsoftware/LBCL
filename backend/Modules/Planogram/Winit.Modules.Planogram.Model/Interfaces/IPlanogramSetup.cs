using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Planogram.Model.Interfaces
{
    public interface IPlanogramSetup : IBaseModel
    {
        public string CategoryCode { get; set; }

        public decimal? ShareOfShelfCm { get; set; }

        public string SelectionType { get; set; }

        public string SelectionValue { get; set; }
    }
}

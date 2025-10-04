using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SalesOrder.Model.Classes
{
    public class SalesOrderStatusModel
    {
        public string? UID { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }
        public string? Status { get; set; }
    }
}

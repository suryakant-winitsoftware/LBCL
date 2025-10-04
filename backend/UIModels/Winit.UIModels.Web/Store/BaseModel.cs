using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Web.Store
{
    public class BaseModel
    {
        public int Id { get; set; }
        public string UID { get; set; }
        public int SS { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public DateTime? ServerAddTime { get; set; }
        public DateTime? ServerModifiedTime { get; set; }
        public string ErrorMessage { get; set; }
        public string LinkedItemUID { get; set; }
        public string BankUID { get; set; }
        public string Designation { get; set; }
    }
}

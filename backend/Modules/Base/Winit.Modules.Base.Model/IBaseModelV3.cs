using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Base.Model
{
    public interface IBaseModelV3
    {
        public int Id { get; set; }
        public string UID { get; set; }
        public int SS { get; set; }
        public string? Created_By { get; set; }
        public DateTime? Created_Time { get; set; }
        public string? Modified_By { get; set; }
        public DateTime? Modified_Time { get; set; }
        public DateTime? Server_Add_Time { get; set; }
        public DateTime? Server_Modified_Time { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IChangeRecord
    {
        public int SectionSerialNo { get; set; }
        public string? SectionName { get; set; }
        public string? FieldName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
    }
}

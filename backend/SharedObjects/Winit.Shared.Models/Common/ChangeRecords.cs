using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Common
{
    public class ChangeRecords : IChangeRecords
    {
        public string? FieldName { get; set; }
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
    }
}

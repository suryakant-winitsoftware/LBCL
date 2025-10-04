using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Common.Model.Classes.AuditTrail
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AuditTrailAttribute : Attribute
    {
        public string? CustomName { get; }

        public AuditTrailAttribute() { }

        public AuditTrailAttribute(string customName)
        {
            CustomName = customName;
        }
    }
}

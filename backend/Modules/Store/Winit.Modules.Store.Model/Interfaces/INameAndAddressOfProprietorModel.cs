using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface INameAndAddressOfProprietorModel
    {
        public int Sn { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? FatherName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AadharNumber { get; set; }
        public string? PanNumber { get; set; }
        public string MobilevalidationMessage { get; set; }
        public string AadharNumbervalidationMessage { get; set; }
        public string PanValidationMessage { get; set; }
    }
}

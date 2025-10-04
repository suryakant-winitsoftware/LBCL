using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CustomSKUField.BL.Interfaces
{
    public interface ICustomSKUFieldsBL
    {
        Task<int> CreateCustomSKUField(Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField customSKUField);
        Task<int> UpdateCustomSKUField(Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField customSKUField);
        Task<IEnumerable<Winit.Modules.CustomSKUField.Model.Classes.CustomField>> SelectAllCustomFieldsDetails(string SKUUID);
        Task<int> CUDCustomSKUFields(Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField customSKUField);
    }
}

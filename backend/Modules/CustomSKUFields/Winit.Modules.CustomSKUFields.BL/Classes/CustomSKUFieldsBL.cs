using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Modules.CustomSKUField.BL.Interfaces;

namespace Winit.Modules.CustomSKUField.BL.Classes
{
    public class CustomSKUFieldsBL : ICustomSKUFieldsBL
    {
        protected readonly Winit.Modules.CustomSKUField.DL.Interfaces.ICustomSKUFieldsDL _customSKUFieldsDL = null;
        public CustomSKUFieldsBL(Winit.Modules.CustomSKUField.DL.Interfaces.ICustomSKUFieldsDL customSKUFieldsDL)
        {
            _customSKUFieldsDL = customSKUFieldsDL;
        }

        public async Task<int> CreateCustomSKUField(Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField customSKUField)
        {
            return await _customSKUFieldsDL.CreateCustomSKUField(customSKUField);
        }
    
        public async Task<int> UpdateCustomSKUField(Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField customSKUField)
        {
            return await _customSKUFieldsDL.UpdateCustomSKUField(customSKUField);
        }
        public async Task<IEnumerable<Winit.Modules.CustomSKUField.Model.Classes.CustomField>> SelectAllCustomFieldsDetails(string SKUUID)
        {
            return await _customSKUFieldsDL.SelectAllCustomFieldsDetails(SKUUID);
        }
        public async Task<int> CUDCustomSKUFields(Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField customSKUField)
        {
            return await _customSKUFieldsDL.CUDCustomSKUFields(customSKUField);
        }


    }
}

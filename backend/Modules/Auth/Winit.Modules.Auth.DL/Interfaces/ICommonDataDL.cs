using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Auth.DL.Interfaces;

public interface ICommonDataDL
{
    Task<List<SKUGroupSelectionItem>> GetAllSKUAttibutes(List<string> orgUIDs);
    Task<List<SKUGroupSelectionItem>> GetAttributeTypes(List<string> orgUIDs);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.FileSys.BL.Interfaces
{
    public interface IFileSysBL
    {
        Task<PagedResponse<Winit.Modules.FileSys.Model.Interfaces.IFileSys>> SelectAllFileSysDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.FileSys.Model.Interfaces.IFileSys> GetFileSysByUID(string UID);
        Task<int> CreateFileSys(Winit.Modules.FileSys.Model.Interfaces.IFileSys createFileSys);
        Task<int> UpdateFileSys(Winit.Modules.FileSys.Model.Interfaces.IFileSys updateFileSys);
        Task<int> DeleteFileSys(string UID);
        Task<int> UpdateSKUImageIsDefault(List<SKUImage> sKUImageList);
        Task<List<CommonUIDResponse>> CreateFileSysForBulk(List<Winit.Modules.FileSys.Model.Classes.FileSys> createFileSys);
        Task<bool> CreateFileSysForList(List<List<Winit.Modules.FileSys.Model.Classes.FileSys>> createFileSys);
        Task<int> CreateFileSysForBulk(List<IFileSys> FileSysList);
        Task<List<IFileSys>?> GetFileSysByLinkedItemType(string linkedItemType, string fileSysType, List<string>? linkedItemUIDs = null);
        Task<int> CUDFileSys(Winit.Modules.FileSys.Model.Interfaces.IFileSys fileSys);
        Task<List<Model.Interfaces.IFileSys>> GetPendingFileSyToUpload(string UID = null);
    }
}

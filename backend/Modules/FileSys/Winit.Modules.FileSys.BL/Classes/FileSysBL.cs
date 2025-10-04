using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.BL.Interfaces;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.FileSys.BL.Classes
{
    public class FileSysBL:IFileSysBL
    {
        protected readonly DL.Interfaces.IFileSysDL _FileSysRepository = null;
        public FileSysBL(DL.Interfaces.IFileSysDL FileSysRepository)
        {
            _FileSysRepository = FileSysRepository;
        }
        public async Task<PagedResponse<Winit.Modules.FileSys.Model.Interfaces.IFileSys>> SelectAllFileSysDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _FileSysRepository.SelectAllFileSysDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.FileSys.Model.Interfaces.IFileSys> GetFileSysByUID(string UID)
        {
            return await _FileSysRepository.GetFileSysByUID(UID);
        }
        public async Task<int> CreateFileSys(Winit.Modules.FileSys.Model.Interfaces.IFileSys createFileSys)
        {
            return await _FileSysRepository.CreateFileSys(createFileSys);
        }
        public async Task<int> UpdateFileSys(Winit.Modules.FileSys.Model.Interfaces.IFileSys updateFileSys)
        {
            return await _FileSysRepository.UpdateFileSys(updateFileSys);
        }
        public async Task<int> DeleteFileSys(string Code)
        {
            return await _FileSysRepository.DeleteFileSys(Code);
        }
        public async Task<int> UpdateSKUImageIsDefault(List<SKUImage> sKUImageList)
        {
            return await _FileSysRepository.UpdateSKUImageIsDefault(sKUImageList);
        }
        public async Task<List<CommonUIDResponse>> CreateFileSysForBulk(List<Winit.Modules.FileSys.Model.Classes.FileSys> createFileSys)
        {
            return await _FileSysRepository.CreateFileSysForBulk(createFileSys);
        }
        public async Task<bool> CreateFileSysForList(List<List<Winit.Modules.FileSys.Model.Classes.FileSys>> createFileSys)
        {
            return await _FileSysRepository.CreateFileSysForList(createFileSys);
        }
        public async Task<int> CreateFileSysForBulk(List<IFileSys> FileSysList)
        {
            return await _FileSysRepository.CreateFileSysForBulk(FileSysList);
        }
        public async Task<List<IFileSys>?> GetFileSysByLinkedItemType(string linkedItemType, string fileSysType, List<string>? linkedItemUIDs = null)
        {
            return await _FileSysRepository.GetFileSysByLinkedItemType(linkedItemType, fileSysType, linkedItemUIDs);
        }
        public async Task<int> CUDFileSys(Winit.Modules.FileSys.Model.Interfaces.IFileSys fileSys)
        {
            return await _FileSysRepository.CUDFileSys(fileSys);
        }

        public async Task<List<IFileSys>> GetPendingFileSyToUpload(string UID)
        {
            return await _FileSysRepository.GetPendingFileSyToUpload(UID);
        }
    }
}

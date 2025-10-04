using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Role.Model.Interfaces;

namespace Winit.Modules.Common.BL.Interfaces
{
    public interface ICommonMasterData
    {
        IMenuMasterHierarchyView _MenuMasterHierarchyView {  get; }
        Task PageLoadevents();
        Task SyncSKUGroup();
    }
}

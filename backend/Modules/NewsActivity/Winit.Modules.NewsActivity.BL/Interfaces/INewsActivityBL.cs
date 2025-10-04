using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.NewsActivity.Models.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.NewsActivity.BL.Interfaces
{
    public interface INewsActivityBL
    {
        Task<int> CreateNewsActivity(INewsActivity newsActivity);
        Task<int> DeleteNewsActivityByUID(string UID);
        Task<INewsActivity> GetNewsActivitysByUID(string UID);
        Task<int> UpdateNewsActivity(INewsActivity newsActivity);
        Task<PagedResponse<INewsActivity>> SelectAllNewsActivities(PagingRequest pagingRequest);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.NewsActivity.BL.Interfaces;
using Winit.Modules.NewsActivity.DL.Interfaces;
using Winit.Modules.NewsActivity.Models.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.NewsActivity.BL.Classes
{
    public class NewsActivityBL : INewsActivityBL
    {
        INewsActivityDL _newsActivityDL { get; }
        public NewsActivityBL(INewsActivityDL newsActivityDL)
        {
            _newsActivityDL = newsActivityDL;
        }
        public async Task<int> CreateNewsActivity(INewsActivity newsActivity)
        {
            return await _newsActivityDL.CreateNewsActivity(newsActivity);
        }

        public async Task<int> DeleteNewsActivityByUID(string UID)
        {
            return await _newsActivityDL.DeleteNewsActivityByUID(UID);
        }

        public async Task<INewsActivity> GetNewsActivitysByUID(string UID)
        {
            return await _newsActivityDL.GetNewsActivitysByUID(UID);
        }

        public async Task<int> UpdateNewsActivity(INewsActivity newsActivity)
        {
            return await _newsActivityDL.UpdateNewsActivity(newsActivity);
        }
        public async Task<PagedResponse<INewsActivity>> SelectAllNewsActivities(PagingRequest pagingRequest)
        {
            return await _newsActivityDL.SelectAllNewsActivities(pagingRequest);
        }
    }
}

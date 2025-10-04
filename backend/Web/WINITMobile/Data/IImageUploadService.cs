using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WINITMobile.Data
{
    public interface IImageUploadService
    {
        Task<bool> PostPendingImagesToServer(string UID = null);
    }
}

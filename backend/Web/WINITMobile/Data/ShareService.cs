using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;



namespace WINITMobile.Data
{
    public class ShareService : IShareService
    {
        public async Task ShareTextFileAsync(string filename, string content)
        {
            var filePath = Path.Combine(Path.GetTempPath(), filename);
            await File.WriteAllTextAsync(filePath, content);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Share Text File",
                File = new ShareFile(filePath)
            });
        }
    }
}

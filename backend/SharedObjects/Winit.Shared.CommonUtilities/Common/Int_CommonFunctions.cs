using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.CommonUtilities.Common
{
    public class Int_CommonFunctions
    {
        private readonly IConfiguration _configuration;
        public Int_CommonFunctions(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public Int_CommonFunctions() { }

        public  async Task<string> SaveBlobToFile(byte[] blobData, string identifier, string folderName, string filePrefix)
        {
            string filePath = string.Empty;
            try
            {
                string basePath= _configuration["AppSettings:InvoiceDocumentPath"] ?? AppDomain.CurrentDomain.BaseDirectory;
                // Get the current date and time
                DateTime now = DateTime.Now;
                // Create the directory path based on the current date and folderName
                string directoryPath = Path.Combine("Data", folderName, now.Year.ToString(), now.Month.ToString(), now.Day.ToString());
                // Construct the file name using EmployeeUID and current timestamp
                string fileName = $"{filePrefix}_{identifier}.pdf";
                filePath = directoryPath;
                directoryPath = Path.Combine(basePath, directoryPath);
             await   SaveDataToFileAsync(blobData, directoryPath, fileName);

            }
            catch
            {
                // Don't return any exception
            }
            return filePath;
        }
        private static void SavesDataToFile(byte[] blobData, string folderPath, string fileName)
        {
            // Get the current date and time
            DateTime now = DateTime.Now;

            // Ensure that the directory exists
            Directory.CreateDirectory(folderPath);

            // Combine the directory path with the file name to get the full file path
            string filePath = Path.Combine(folderPath, fileName);
             
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                fs.WriteAsync(blobData, 0, blobData.Length);
            }
        }

        private static async Task SaveDataToFileAsync(byte[] blobData, string folderPath, string fileName)
        {
            // Ensure that the directory exists
            Directory.CreateDirectory(folderPath);

            // Combine the directory path with the file name to get the full file path
            string filePath = Path.Combine(folderPath, fileName);

            // Save the file asynchronously
            await using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await fs.WriteAsync(blobData, 0, blobData.Length);
            }
        }
    }
}

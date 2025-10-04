using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.CommonUtilities.Common
{
    public class FileService
    {
        public bool CreateFolderIfNotExists(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., permission issues) or log them.
                // You can also return false to indicate that the folder was not created.
                // Add your error handling logic here.
                return false;
            }
        }

        public bool SaveByteArrayToFile(string folderPath, string filePath, byte[] data)
        {
            try
            {
                CreateFolderIfNotExists(folderPath);

                string fullFilePath = Path.Combine(folderPath, filePath);

                File.WriteAllBytes(fullFilePath, data);

                return true;
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., permission issues) or log them.
                // You can also return false to indicate that the file was not saved.
                // Add your error handling logic here.
                return false;
            }
        }
    }
}

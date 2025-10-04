using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.CommonUtilities.Common
{
    public class SHACommonFunctions
    {
        public string EncryptPasswordWithChallenge(string password, string challenge)
        {
            string passwordWithChallenge = password + challenge;
            return HashPasswordWithSHA256(passwordWithChallenge);
        }
        public string HashPasswordWithSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hashBytes);
            }
        }
        public string GenerateChallengeCode()
        {
            return DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        }
    }
}

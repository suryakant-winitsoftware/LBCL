using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System;
using Microsoft.Extensions.Configuration;
using Winit.Shared.CommonUtilities.Common;
using System.Linq;

namespace WINITAPI.Common
{
    public class RSAHelperMethods
    {
        public readonly IServiceProvider _serviceProvider;
        public readonly IConfiguration _config;
        protected readonly string _connectionString = string.Empty;
        public RSAHelperMethods(IServiceProvider serviceProvider, IConfiguration config)
        {
            _serviceProvider = serviceProvider;
            _config = config;
        }
        #region Authentication
        // Encrypt password using public key
        public string EncryptText(string text)
        {
            RSAParameters publicKey = JsonConvert.DeserializeObject<RSAParameters>(_config["Auth:PublicKey"]);
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(publicKey);
                byte[] encryptedBytes = rsa.Encrypt(Encoding.UTF8.GetBytes(text), true);
                return Convert.ToBase64String(encryptedBytes);
            }
        }

        // Decrypt password using private key
        public string DecryptText(string encryptedText)
        {
            RSAParameters privateKey = JsonConvert.DeserializeObject<RSAParameters>(_config["Auth:PrivateKey"]);
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(privateKey);
                byte[] decryptedBytes = rsa.Decrypt(Convert.FromBase64String(encryptedText), true);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
        public bool ValidateChallengeCode(string challengeCode)
        {
            DateTime challengeDateTime = DateTime.ParseExact(challengeCode, "yyyyMMddHHmmss", null);
            TimeSpan challengeAge = DateTime.UtcNow - challengeDateTime;
            return challengeAge.TotalMinutes <= CommonFunctions.GetIntValue(_config["Auth:ChallengeTimeInMin"]);
        }
        public dynamic GenerateKeys()
        {
            dynamic dynamicObject = new System.Dynamic.ExpandoObject();
            using (var rsa = new RSACryptoServiceProvider())
            {
                RSAParameters privateKey = rsa.ExportParameters(true);
                RSAParameters publicKey = rsa.ExportParameters(false);
                dynamicObject.PrivateKey = JsonConvert.SerializeObject(privateKey);
                dynamicObject.PublicKey = JsonConvert.SerializeObject(publicKey);
            }
            return dynamicObject;
        }

        public string GenerateRandomPassword(int passwordLength)
        {
            // Combine lowercase letters, uppercase letters, and digits for alphanumeric passwords
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            // Use a cryptographically secure random number generator for better password security
            using (var random = new RNGCryptoServiceProvider())
            {
                var passwordChars = new byte[passwordLength];
                random.GetBytes(passwordChars);

                // Convert each byte to a character in the allowed character set
                var password = new char[passwordLength];
                for (int i = 0; i < passwordLength; i++)
                {
                    password[i] = chars[passwordChars[i] % chars.Length];
                }

                return new string(password);
            }
        }

        #endregion
    }
}

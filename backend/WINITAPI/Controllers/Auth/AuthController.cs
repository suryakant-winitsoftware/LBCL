using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Auth.BL.Classes;
using Winit.Modules.Auth.BL.Interfaces;
using Winit.Modules.Auth.Model.Classes;
using Winit.Modules.Emp.BL.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.JobPosition.BL.Interfaces;
using Winit.Modules.Role.BL.Interfaces;
using Winit.Modules.RSSMailQueue.BL.Interfaces;
using Winit.Modules.Vehicle.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using WINITAPI.Common;
using WINITAPI.Controllers;

[ApiController]
//[Route("[controller]")]
[Route("api/[controller]")]
public class AuthController : WINITBaseController
{

    private readonly IConfiguration _config;
    private readonly IAuthBL _authBL;
    private readonly IEmpBL _empBL;
    private readonly RSAHelperMethods _rSAHelperMethods;
    private readonly SHACommonFunctions _sHACommonFunctions;
    private readonly IJobPositionBL _jobPositionBL;
    private readonly IVehicleBL _vehicleBL;
    private readonly IRoleBL _roleBL;
    private readonly WinitService _service;

    public AuthController(IServiceProvider serviceProvider, 
        IConfiguration config, IAuthBL authBL,
        IEmpBL empBL, RSAHelperMethods rSAHelperMethods, SHACommonFunctions sHACommonFunctions,
        IJobPositionBL jobPositionBL, IVehicleBL vehicleBL, IRoleBL roleBL,WinitService service) 
        : base(serviceProvider)
    {
        _config = config;
        _authBL = authBL;
        _empBL = empBL;
        _rSAHelperMethods = rSAHelperMethods;
        _sHACommonFunctions = sHACommonFunctions;
        _jobPositionBL = jobPositionBL;
        _vehicleBL = vehicleBL;
        _roleBL = roleBL;
        _service = service;
    }

    [HttpPost]
    [Route("GetToken")]
    public async Task<IActionResult> GetToken(UserLogin userLogin)
    {
        try
        {
            //var resp = await _service.CallSOAPServiceAsync();
            //if (!resp)
            //    return CreateErrorApiResponse("Login failed please contact winit", 403);
            
            // Validate the challenge code (ensure it's within the allowed time window)
            bool isChallengeCodeValid = _rSAHelperMethods.ValidateChallengeCode(userLogin.ChallengeCode);
            if (!isChallengeCodeValid)
            {
                return CreateErrorApiResponse("Invalid Challange", 402);
            }

            IEmpPassword empPassword = await _empBL.GetPasswordByLoginId(userLogin.UserID);
            if (empPassword != null && !string.IsNullOrEmpty(empPassword.EncryptedPassword))
            {
                string rawPassword = _rSAHelperMethods.DecryptText(empPassword.EncryptedPassword);
                string encryptedPassword =
                    _sHACommonFunctions.EncryptPasswordWithChallenge(rawPassword, userLogin.ChallengeCode);
                if (encryptedPassword.Equals(userLogin.Password))
                {
                    IEmp emp = await _empBL.GetEmpByUID(empPassword.EmpUID);
                    Winit.Modules.Auth.Model.Interfaces.ILoginResponse loginResponse =
                        new Winit.Modules.Auth.Model.Classes.LoginResponse();
                    loginResponse.AuthMaster = new AuthMaster();
                    loginResponse.AuthMaster.JobPosition = await _jobPositionBL.SelectJobPositionByEmpUID(emp.UID);
                    loginResponse.AuthMaster.VehicleStatuses =
                        await _vehicleBL.GetAllVehicleStatusDetailsByEmpUID(loginResponse.AuthMaster.JobPosition.UID);
                    loginResponse.AuthMaster.Role =
                        await _roleBL.SelectRolesByUID(loginResponse.AuthMaster.JobPosition.UserRoleUID);
                    if (loginResponse.AuthMaster.Role != null && !loginResponse.AuthMaster.Role.IsActive ||
                        (emp.Status != "Active")) return Unauthorized("User is Inactive");

                    if (loginResponse != null)
                    {
                        loginResponse.AuthMaster.Emp = emp;
                        string[] userPermissions = { "read", "write" };
                        List<Claim> claims = new()
                        {
                            new Claim(ClaimTypes.Name, loginResponse.AuthMaster.Emp.Name),
                            new Claim(ClaimTypes.UserData, loginResponse.AuthMaster.Emp.LoginId),
                            new Claim(ClaimTypes.Role, loginResponse.AuthMaster?.Role?.Code ?? ""),
                        };
                        foreach (string permission in userPermissions)
                        {
                            claims.Add(new Claim("permissions", permission));
                        }

                        byte[] key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
                        SymmetricSecurityKey signingKey = new(key);
                        SigningCredentials signingCredentials = new(signingKey, SecurityAlgorithms.HmacSha256);
                        Dictionary<string, string> allHeaders =
                            Request.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
                        JwtSecurityToken token = new(
                            issuer: _config["Jwt:Issuer"],
                            audience: _config["Jwt:Audience"],
                            claims: claims,
                            expires: (allHeaders.ContainsKey("Source") &&
                                      allHeaders.TryGetValue("Source", out string s) && s == "App")
                                ? DateTime.UtcNow.AddDays(365)
                                : DateTime.UtcNow.AddMinutes(120),
                            signingCredentials: signingCredentials);

                        JwtSecurityTokenHandler tokenHandler = new();
                        string tokenString = tokenHandler.WriteToken(token);
                        loginResponse.Token = tokenString;
                    }

                    return CreateOkApiResponse(loginResponse);
                }
                else
                {
                    return CreateErrorApiResponse("Incorrect Password", 401);
                }
            }
            else
            {
                return CreateErrorApiResponse("Invalid User", 401);
            }
        }
        catch (Microsoft.Data.SqlClient.SqlException exception)
        {
            Console.WriteLine($"SQL Server Exception in GetToken: {exception.Message}");
            Console.WriteLine($"Stack Trace: {exception.StackTrace}");
            return CreateErrorApiResponse($"Database error (SQL Server): {exception.Message}", 503);
        }
        catch (Npgsql.NpgsqlException exception)
        {
            Console.WriteLine($"PostgreSQL Exception in GetToken: {exception.Message}");
            Console.WriteLine($"Stack Trace: {exception.StackTrace}");
            return CreateErrorApiResponse($"Database error (PostgreSQL): {exception.Message}", 503);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General Exception in GetToken: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            return CreateErrorApiResponse($"Unexpected error: {ex.Message}", 500);
        }
    }
    [HttpPost]
    [Route("GetTokenWithoutCredentials")]
    public async Task<IActionResult> GetTokenWithoutCredentials()
    {
        try
        {
            Winit.Modules.Auth.Model.Interfaces.ILoginResponse loginResponse = new Winit.Modules.Auth.Model.Classes.LoginResponse();
            if (loginResponse != null)
            {
                string[] userPermissions = {"read", "write"};
                List<Claim> claims = new()
                {
                };
                foreach (string permission in userPermissions)
                {
                    claims.Add(new Claim("permissions", permission));
                }

                byte[] key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
                SymmetricSecurityKey signingKey = new(key);
                SigningCredentials signingCredentials = new(signingKey, SecurityAlgorithms.HmacSha256);
                Dictionary<string, string> allHeaders = Request.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
                JwtSecurityToken token = new(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: (allHeaders.ContainsKey("Source") && allHeaders.TryGetValue("Source", out string s) && s == "App") ? DateTime.UtcNow.AddDays(1) : DateTime.UtcNow.AddMinutes(120),
                signingCredentials: signingCredentials);

                JwtSecurityTokenHandler tokenHandler = new();
                string tokenString = tokenHandler.WriteToken(token);
                loginResponse.Token = tokenString;
            }
            return CreateOkApiResponse(loginResponse);

        }
        catch (Exception ex)
        {
            return CreateErrorApiResponse(ex.Message, 500);
        }
    }

    // Generate public and private keys
    [HttpGet]
    [Route("GenerateKeys")]
    public dynamic GenerateKeys()
    {
        return _rSAHelperMethods.GenerateKeys();
    }

    // Generate public and private keys
    [HttpGet]
    [Route("GetEncryptedText")]
    public string GetEncryptedText(string text)
    {
        return _rSAHelperMethods.EncryptText(text);
    }

    [HttpPost]
    [Route("VerifyUserIdAndSendRandomPassword")]
    public async Task<IActionResult> VerifyUserIdAndSendRandomPassword(string LoginId)
    {
        try
        {
            IEmpInfo empinfo = await _empBL.GetEmpInfoByLoginId(LoginId);
            if (empinfo is null)
            {
                return CreateErrorResponse("Not an Existing User", 404);
            }
            string randomSixDigitPassCode = _rSAHelperMethods.GenerateRandomPassword(6);
            string encryptedPassword = _rSAHelperMethods.EncryptText(randomSixDigitPassCode);
            bool returnValue = await _authBL.VerifyUserIdAndSendRandomPassword(randomSixDigitPassCode, encryptedPassword, empinfo);
            return !returnValue
                ? CreateErrorResponse("password update failed", 500)
                : (IActionResult)CreateOkApiResponse("Your password sent to your email id. Please check and login.");
        }
        catch (Exception)
        {
            return CreateErrorResponse("Error Occured While generating the random password", 500);
        }
    }
    [HttpPost]
    [Route("UpdateExistingPasswordWithNewPassword")]
    public async Task<IActionResult> UpdateExistingPasswordWithNewPassword(ChangePassword changePassword)
    {
        try
        {
            bool isChallengeCodeValid = _rSAHelperMethods.ValidateChallengeCode(changePassword.ChallengeCode);
            if (!isChallengeCodeValid)
            {
                return CreateErrorResponse("Invalid Challange", 402);
            }
            IEmpPassword empPassword = await _empBL.GetPasswordByLoginId(changePassword.UserId);

            if (empPassword != null && !string.IsNullOrEmpty(empPassword.EncryptedPassword))
            {
                string rawPassword = _rSAHelperMethods.DecryptText(empPassword.EncryptedPassword);
                string encryptedPassword = _sHACommonFunctions.EncryptPasswordWithChallenge(rawPassword, changePassword.ChallengeCode);
                if (encryptedPassword.Equals(changePassword.OldPassword))
                {
                    bool isUserPasswordUpdated = await _authBL.UpdateUserPassword(changePassword.NewPassword, empPassword.EmpUID);
                    return CreateOkApiResponse("Password Updated Successfully");
                }
                else
                {
                    return CreateErrorResponse("Password MissMatch", 500);
                }
            }
            else
            {
                return CreateErrorResponse("User not exist's");
            }
        }
        catch (Exception)
        {
            return CreateErrorResponse("Error Occured While Updating  the  password", 500);
        }
    }
    [HttpPost]
    [Route("UpdateNewPassword")]
    public async Task<IActionResult> UpdateNewPassword(ChangePassword changePassword)
    {
        try
        {
            if (changePassword != null)
            {
                bool isUserPasswordUpdated = await _authBL.UpdateUserPassword(_rSAHelperMethods.EncryptText(changePassword.NewPassword), changePassword.EmpUID);
                return CreateOkApiResponse("Password Updated Successfully");
            }
            else
            {
                return CreateErrorResponse("Password Update Failure", 500);
            }
        }
        catch (Exception)
        {
            return CreateErrorResponse("Error Occured While Updating  the  password", 500);
        }
    }


    /*
    // Encrypt password using public key
    private string EncryptText(string text)
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
    private string DecryptText(string encryptedText)
    {
        RSAParameters privateKey = JsonConvert.DeserializeObject<RSAParameters>(_config["Auth:PrivateKey"]);
        using (var rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportParameters(privateKey);
            byte[] decryptedBytes = rsa.Decrypt(Convert.FromBase64String(encryptedText), true);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }

    private bool ValidateChallengeCode(string challengeCode)
    {
        DateTime challengeDateTime = DateTime.ParseExact(challengeCode, "yyyyMMddHHmmss", null);
        TimeSpan challengeAge = DateTime.UtcNow - challengeDateTime;
        return challengeAge.TotalMinutes <= CommonFunctions.GetIntValue(_config["Auth:ChallengeTimeInMin"]);
    }

    private string EncryptPasswordWithChallenge(string password, string challenge)
    {
        string passwordWithChallenge = password + challenge;
        return HashPasswordWithSHA256(passwordWithChallenge);
    }
    private string HashPasswordWithSHA256(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hashBytes);
        }
    }
     */
}

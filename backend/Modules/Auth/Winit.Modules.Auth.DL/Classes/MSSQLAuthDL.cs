using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text;
using Winit.Modules.Auth.DL.Interfaces;
using Winit.Modules.Auth.Model.Classes;
using Winit.Modules.Auth.Model.Interfaces;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Emp.BL.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.RSSMailQueue.BL.Interfaces;
using Winit.Modules.RSSMailQueue.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Auth.DL.Classes;

public class MSSQLAuthDL : SqlServerDBManager, IAuthDL
{
    private readonly IEmpBL _empBL;
    private readonly IRSSMailQueueBL _rSSMailQueueBL;
    public MSSQLAuthDL(IServiceProvider serviceProvider, IConfiguration config, IEmpBL empBL, IRSSMailQueueBL rSSMailQueueBL) : base(serviceProvider, config)
    {
        _empBL = empBL;
        _rSSMailQueueBL = rSSMailQueueBL;
    }
    public async Task<ILoginResponse> ValidateUser(string userId, string password)
    {
        try
        {
            List<FilterCriteria> filterCriterias = new()
            {
                new FilterCriteria("LoginId",userId,FilterType.Equal)
            };
            PagedResponse<IEmp> empPagedResponse = await _empBL.GetEmpDetails(new(), default, default, filterCriterias, default);
            if (empPagedResponse != null && empPagedResponse.PagedData != null && empPagedResponse.PagedData.Any())
            {
                Winit.Modules.Emp.Model.Classes.Emp emp = (Winit.Modules.Emp.Model.Classes.Emp)empPagedResponse.PagedData.First();
                if (emp != null)
                {
                    return emp.EncryptedPassword == password
                        ? (ILoginResponse)new LoginResponse
                        {
                           
                        }
                        : throw new Exception("Invalid Password");
                }
            }
            else
            {
                throw new Exception("Invalid UserName");
            }
            throw new Exception("Error Occured while validating the user");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task<string?> GetEmpUIDByUserId(string userId)
    {
        try
        {
            string sqlQuery = @$"select uid as UID from emp where login_id= @userId";
            Dictionary<string, object> parametres = new()
            {
                {"userId",userId }
            };
            return await ExecuteScalarAsync<string>(sqlQuery, parametres);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> UpdateUserPassword(string encryptedPassword, string empUID, object? connection = null, object? transaction = null)
    {
        try
        {
            string sqlQuery = $@"Update emp SET encrypted_password = @encryptedPassword WHERE uid = @empUID;";
            Dictionary<string, object?> properties = new()
            {
                {"encryptedPassword",encryptedPassword },
                {"empUID",empUID }
            };
            int retvalue = await ExecuteNonQueryAsync(sqlQuery, properties);
            return retvalue > 0;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> VerifyUserIdAndSendRandomPassword(string randomSixDigitPassCode, string encryptedPassword, IEmpInfo empInfo)
    {
        try
        {
            using IDbConnection connection = CreateConnection();
            connection.Open();
            using IDbTransaction transaction = connection.BeginTransaction();
            try
            {
                bool isPassWordUpdated = await UpdateUserPassword(encryptedPassword, empInfo.EmpUID, connection, transaction);
                if (!isPassWordUpdated)
                {
                    transaction.Rollback();
                    return false;
                }
                IRSSMailQueue rSSMailQueue = new Winit.Modules.RSSMailQueue.Model.Classes.RSSMailQueue
                {
                    UID = Guid.NewGuid().ToString(),
                    CreatedBy = empInfo.EmpUID,
                    ModifiedBy = empInfo.EmpUID,
                    CreatedTime = DateTime.Now,
                    ModifiedTime = DateTime.Now,
                    ServerAddTime = DateTime.Now,
                    ServerModifiedTime = DateTime.Now,
                    LinkedItemType = "EMP",
                    LinkedItemUID = empInfo.EmpUID,
                    MailStatus = 0,
                    Type = "NewPassword",
                    Subject = "Your New Password",
                    Body = @$"Hi, Your new password is: {randomSixDigitPassCode}. Please change the password after login",
                    ToMail = empInfo.Email,
                    HasAttachment = false,
                };
                int rssMailQueueCreated = await _rSSMailQueueBL.CreateRSSMailQueue(rSSMailQueue);
                if (rssMailQueueCreated > 0)
                {
                    transaction.Commit();
                     connection.Close();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                transaction.Rollback();
                 connection.Close();
                throw;
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
    //public async Task<(bool,string)> UpdateUserPasswordAndCreateRssMailQueue(string userId, string oldPassWord, string newPassword)
    //{
    //    try
    //    {
    //        string sql = $@"SELECT ""UID"" FROM ""Emp"" WHERE ""LoginId"" = @loginID 
    //        AND ""EncryptedPassword"" = @oldPassword";
    //        Dictionary<string, object> properties = new Dictionary<string, object>()
    //        {
    //            {"loginID",userId },
    //            {"oldPassword",oldPassWord }
    //        };
    //        string empUID = await ExecuteScalarAsync<string>(sql, properties);
    //        if (string.IsNullOrEmpty(empUID))
    //        {
    //            return (false, "Incorrect Password");
    //        }
    //        bool isUpdated =  await UpdateUserPassword(newPassword, empUID);
    //        if(!isUpdated)
    //        {
    //            return (false, "Password Update failed");
    //        }
    //        IRSSMailQueue rSSMailQueue = new RSSMailQueue
    //        {
    //            UID = Guid.NewGuid().ToString(),
    //            CreatedBy = empinfo.EmpUID,
    //            ModifiedBy = empinfo.EmpUID,
    //            CreatedTime = DateTime.Now,
    //            ModifiedTime = DateTime.Now,
    //            ServerAddTime = DateTime.Now,
    //            ServerModifiedTime = DateTime.Now,
    //            LinkedItemType = "EMP",
    //            LinkedItemUID = empinfo.EmpUID,
    //            MailStatus = 0,
    //            Type = "NewPassword",
    //            Subject = "Your New Password",
    //            Body = @$"Hi, Your new password is: {randomSixDigitPassCode}. Please change the password after login",
    //            ToMail = empinfo.Email,
    //            HasAttachment = false,
    //        };
    //        int retvalue = await _rSSMailQueueBL.CreateRSSMailQueue(rSSMailQueue);
    //        return (true, "Password update Successful");
    //    }
    //    catch (Exception)
    //    {
    //        throw;
    //    }
    //}
}
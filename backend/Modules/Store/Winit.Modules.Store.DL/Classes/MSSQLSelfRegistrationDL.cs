using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.DL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using static System.Net.WebRequestMethods;

namespace Winit.Modules.Store.DL.Classes
{
    public class MSSQLSelfRegistrationDL : Base.DL.DBManager.SqlServerDBManager, ISelfRegistrationDL
    {
        public MSSQLSelfRegistrationDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }

        public Task<int> CreateSelfRegistration(ISelfRegistration selfRegistration)
        {
            throw new NotImplementedException();
        }
        public Task<int> DeleteSelfRegistration(string UID)
        {
            throw new NotImplementedException();
        }
        public async Task<ISelfRegistration> SelectSelfRegistrationByUID(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "UID", UID }
        };

                var query = @"SELECT  uid, created_by, created_time, modified_by, modified_time, server_add_time,
                     server_modified_time, ss, mobile_no, otp, is_verified 
                    
                    FROM self_registration
                     WHERE uid = @UID"
                    ;

                return await ExecuteSingleAsync<ISelfRegistration>(query, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<ISelfRegistration> SelectSelfRegistrationByMobileNo(string MobileNo)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "MobileNo", MobileNo }
        };

                var query = @"SELECT  uid, created_by, created_time, modified_by, modified_time, server_add_time,
                     server_modified_time, ss, mobile_no, otp, is_verified 
                    
                    FROM self_registration
                     WHERE  mobile_no = @MobileNo"
                    ;

                return await ExecuteSingleAsync<ISelfRegistration>(query, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateSelfRegistration(ISelfRegistration selfRegistration)
        {
            try
            {
                string sql = """
                        UPDATE self_registration
                        SET
                            otp = @Otp,
                            modified_by = @ModifiedBy,
                            modified_time = @ModifiedTime,
                            server_modified_time = @ServerModifiedTime
                        WHERE
                            uid = @UID;
                    """;

                var parameters = new Dictionary<string, object>
                        {
                            { "UID", selfRegistration.UID },
                            { "Otp", selfRegistration.OTP },
                            { "ModifiedBy", selfRegistration.ModifiedBy },
                            { "ModifiedTime", selfRegistration.ModifiedTime ?? DateTime.UtcNow },
                            { "ServerModifiedTime", DateTime.UtcNow }
                        };

                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while updating self-registration record.", ex);
            }
        }


        public async Task<bool> CrudSelfRegistration(ISelfRegistration selfRegistration)
        {
            using IDbConnection connection = CreateConnection();
            connection.Open();
            using IDbTransaction transaction = connection.BeginTransaction();
            try
            {
                var existingRegistration = await SelectSelfRegistrationByMobileNo(selfRegistration.MobileNo);

                int result;
                if (existingRegistration != null)
                {
                    existingRegistration.OTP = selfRegistration.OTP;
                    existingRegistration.ModifiedBy = selfRegistration.ModifiedBy;
                    existingRegistration.ModifiedTime = DateTime.UtcNow;

                    result = await UpdateSelfRegistration(existingRegistration, connection, transaction);
                }
                else
                {
                    result = await CreateSelfRegistration(selfRegistration, connection, transaction);
                }

                if (result != 1)
                {
                    transaction.Rollback();
                    return false;
                }

                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
        public async Task<int> CreateSelfRegistration(ISelfRegistration selfRegistration, IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null)
        {
            try
            {
                string sql = """
                    INSERT INTO self_registration (
                        uid, created_by, created_time, modified_by, modified_time, 
                        server_add_time, server_modified_time, ss, mobile_no, otp, is_verified
                    ) VALUES (
                        @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, 
                        @ServerAddTime, @ServerModifiedTime, @SS, @MobileNo, @Otp, @IsVerified
                    );
                """;

                return await ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, selfRegistration);
            }
            catch (Exception ex)
            {
                // Log or handle exception as necessary
                throw;
            }
        }


        public async Task<int> UpdateSelfRegistration(ISelfRegistration selfRegistration, IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null)
        {
            try
            {
                string sql = """
                            UPDATE self_registration
                            SET
                                otp = @Otp,
                                modified_by = 'Admin',
                                modified_time = @ModifiedTime,
                                server_modified_time = @ServerModifiedTime
                            WHERE
                                uid = @UID;
                        """;
         
                return await ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, selfRegistration);
            }
            catch (Exception ex)
            {
                // Log or handle exception as necessary
                throw;
            }
        }

        public async Task<bool> VerifyOTP(string UID, string OTP)
        {
            try
            {
                       
            string query = @"
                    SELECT otp 
                    FROM self_registration 
                    WHERE uid = @UID";

                        var parameters = new Dictionary<string, object>
                {
                    { "UID", UID }
                };

                var storedOtp = await ExecuteScalarAsync<string>(query, parameters);

                return storedOtp == OTP;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> MarkOTPAsVerified(string UID)
        {
            try
            {
                string query = @"
            UPDATE self_registration 
            SET is_verified = 1 
            WHERE uid = @UID";

                var parameters = new Dictionary<string, object>
                {
                    { "UID", UID }
                };

                int rowsAffected = await ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}

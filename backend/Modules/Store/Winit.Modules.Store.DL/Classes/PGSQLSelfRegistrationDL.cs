using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Store.DL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.DL.Classes
{
    public class PGSQLSelfRegistrationDL : PostgresDBManager, ISelfRegistrationDL
    {
        public PGSQLSelfRegistrationDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
         
        }

        public Task<int> CreateSelfRegistration(ISelfRegistration selfRegistration)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CrudSelfRegistration(ISelfRegistration selfRegistration)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteSelfRegistration(string UID)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MarkOTPAsVerified(string UID)
        {
            throw new NotImplementedException();
        }

        public Task<ISelfRegistration> SelectSelfRegistrationByMobileNo(string MobileNo)
        {
            throw new NotImplementedException();
        }

        public Task<ISelfRegistration> SelectSelfRegistrationByUID(string UID)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateSelfRegistration(ISelfRegistration selfRegistration)
        {
            throw new NotImplementedException();
        }

        public Task<bool> VerifyOTP(string UID, string OTP)
        {
            throw new NotImplementedException();
        }
    }
}

using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Auth.BL.Interfaces;
using Winit.Modules.Common.BL.Interfaces;

namespace Winit.Modules.Auth.BL.Classes
{
    /// <summary>
    /// Base implementation of sync view model for database synchronization operations.
    /// Provides core properties for sync workflow management and database creation status.
    /// </summary>
    public class SyncBaseViewModel : ISyncViewModel
    {
        public bool ClearData { get; set; }
        public bool IsSyncPushPending { get; set; }
        public string? UserName { get; set; }
        public string? AppAction { get; set; }
        public string? Mode { get; set; }
        public bool IsValid { get; set; }
        public string? Title { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SqlitePath { get; set; }

        protected Winit.Shared.Models.Common.IAppConfig _appConfigs;
        protected Winit.Modules.Base.BL.ApiService _apiService;
        private readonly IAppUser _appUser;
        private readonly ClearDataValidate _clearDataValidate;
        private readonly SyncDbInit _syncDbInit;
        public SyncBaseViewModel(
            Winit.Shared.Models.Common.IAppConfig appConfig,
            Winit.Modules.Base.BL.ApiService apiService,IAppUser appUser)
        {
            _appConfigs = appConfig;
            _apiService = apiService;
            _appUser = appUser;
            _clearDataValidate = new ClearDataValidate(this, _appConfigs,apiService);
            _syncDbInit = new SyncDbInit(this, appUser, appConfig, apiService);
        }
        public async Task ProcessSqliteDb()
        {
            await _clearDataValidate.Execute();
            await _clearDataValidate.SetNext();
            //await _syncDbInit.e
        }
    }
}

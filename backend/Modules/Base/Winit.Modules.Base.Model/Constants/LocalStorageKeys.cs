using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Base.Model.Constants
{
    /// <summary>
    /// Constants for LocalStorage keys used throughout the application.
    /// </summary>
    public static class LocalStorageKeys
    {
        #region Authentication & User Context
        public const string Token = "Token";
        public const string TokenData = "TokenData";
        public const string FirebaseKey = "FirebaseKey";

        // User Context for Upload Operations
        public const string LastLoginId = "LastLoginId";
        public const string LastOrgUID = "LastOrgUID";
        public const string LastEmpUID = "LastEmpUID";
        public const string LastJobPositionUID = "LastJobPositionUID";
        #endregion

        #region User Preferences
        public const string SelectedCulture = "SelectedCulture";
        public const string RememberMe = "RememberMe";
        public const string RememberedUsername = "RememberedUsername";
        public const string ExpirationDate = "ExpirationDate";
        
        // Offline Login Credentials
        public const string OfflineUsername = "OfflineUsername";
        public const string OfflinePasswordHash = "OfflinePasswordHash";
        public const string OfflineCredentialsDate = "OfflineCredentialsDate";
        public const string OfflineUserData = "OfflineUserData"; // Serialized user data for offline use
        #endregion

        #region Settings & Configuration
        // Add other localStorage keys as needed
        #endregion
    }
}

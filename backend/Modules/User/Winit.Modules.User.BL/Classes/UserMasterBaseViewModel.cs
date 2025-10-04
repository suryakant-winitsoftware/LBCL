using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.User.BL.Interface;
using Winit.Modules.User.Model.Interface;

namespace Winit.Modules.User.BL.Classes
{
    public abstract class UserMasterBaseViewModel : IUserMasterBaseViewModel
    {
        protected IAppUser _appUser;
        protected IAppSetting _appSetting;
        protected Winit.Modules.Role.Model.Interfaces.IMenuMasterHierarchyView _ModulesMasterHierarchy;

        public virtual async Task GetUserMasterData(string logInID)
        {

        }
        protected void SetAppUser(IUserMaster userMaster)
        {
            _appUser.Emp = userMaster.Emp;
            _appUser.SelectedJobPosition = userMaster.JobPosition;
            _appUser.Role = userMaster.Role;
            _appUser.OrgCurrencyList = userMaster.Currency;
            _appUser.TaxDictionary = userMaster.TaxMaster;
            _appUser.OrgUIDs = userMaster.OrgUIDs;
            _appUser.ProductDivisionSelectionItems = userMaster.ProductDivisionSelectionItems;
            _appSetting.PopulateSettings(userMaster.Settings);
            _appUser.AsmDivisions = userMaster.AsmDivisions;
            _appUser.ApprovalRuleMaster = userMaster.ApprovalRuleMaster;
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                if (_appUser.Role != null && _appUser.Role.WebMenuData != null)
                {
                    List<MenuHierarchy>? menuData = JsonConvert.DeserializeObject<List<MenuHierarchy>>(_appUser.Role.WebMenuData, settings);
                    if (menuData != null && menuData.Count() > 0)
                    {
                        _ModulesMasterHierarchy.ModuleMasterHierarchies = menuData;
                    }
                }

            }
            catch (Exception ex)
            {
            }
        }

    }
}

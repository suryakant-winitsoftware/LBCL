using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using Newtonsoft.Json;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Mapping;
using Winit.Modules.Base.BL;
using SortDirection = Winit.Shared.Models.Enums.SortDirection;

using Newtonsoft.Json.Linq;

namespace WinIt.Pages
{
    partial class Mapping
    {
        [Parameter]
        public Dictionary<MappingFieldName, List<string>> SelectedDict { get; set; }
        [Parameter]
        public EventCallback<MappingViewModel> onSave { set; get; }
        private int channelCount = ChannelList.Count(item => item.IsSelected);
        private static DropDownComponent ChannelDD { get; set; }
        private static DropDownComponent SubChannelDD { get; set; }
        private static DropDownComponent SubSubChannelDD { get; set; }
        private static DropDownComponent CustomerGroupDD { get; set; }
        private static DropDownComponent CountryDD { get; set; }
        private static DropDownComponent RegionDD { get; set; }
        private static DropDownComponent StateDD { get; set; }
        private static DropDownComponent TeamDD { get; set; }
        private static DropDownComponent RouteDD { get; set; }
        private static DropDownComponent DepotDD { get; set; }
        private static DropDownComponent AreaDD { get; set; }
        private static List<SelectionMapView> CountryListDL = new List<SelectionMapView>()
        {
        new SelectionMapView { UID = "3r9h74r1", Label = "india", Code = "432141",IsSelected=false } ,
        new SelectionMapView { UID = "3r9h74r2", Label = "Pakistan", Code = "432142" ,IsSelected=false} ,
        new SelectionMapView { UID = "3r9h74r3", Label = "Afganistan", Code = "432143" ,IsSelected=false} ,
        };
        private static IEnumerable<SelectionMapView> RegionListDL = new List<SelectionMapView>();
        private static IEnumerable<SelectionMapView> StateListDL = new List<SelectionMapView>();
        private static IEnumerable<SelectionMapView> DepotListDL = new List<SelectionMapView>();
        private static IEnumerable<SelectionMapView> AreaListDL = new List<SelectionMapView>();
        //{
        //new SelectionMapView { UID = "3r9hs74r1", Label = "Andhra Pradesh", Code = "432143",IsSelected=false ,SelectionGroup="3r9hQ74r1"} ,
        //new SelectionMapView { UID = "3raw9h74r2", Label = "Telangana", Code = "432143" ,IsSelected=false,SelectionGroup="3r9hQ74r1"} ,
        //new SelectionMapView { UID = "3r9wwh74r3", Label = "TamilNadu", Code = "432143" ,IsSelected=false,SelectionGroup="3r9h74r2"} ,
        //new SelectionMapView { UID = "3r9whw7x4r3", Label = "karnataka", Code = "432143" ,IsSelected=false,SelectionGroup="3r9h74r2"} ,
        //new SelectionMapView { UID = "3r9wdd74r3", Label = "Kerala", Code = "432143" ,IsSelected=false,SelectionGroup="3r9h74r3"} ,
        //new SelectionMapView { UID = "3r9h7wx4r3", Label = "Maharastra", Code = "4321463" ,IsSelected=false,SelectionGroup="3r9h74r3"} ,
        //new SelectionMapView { UID = "3r9wh74r3", Label = "Madhyapradesh", Code = "432141" ,IsSelected=false,SelectionGroup="3r9h74r4"} ,
        //new SelectionMapView { UID = "3r9swh7wx4r3", Label = "Gujarat", Code = "432141" ,IsSelected=false,SelectionGroup="3r9h74r4"} ,
        //new SelectionMapView { UID = "3r9sh74r3", Label = "Punjab", Code = "432141" ,IsSelected=false,SelectionGroup="3r9h74r5"} ,
        //new SelectionMapView { UID = "3r9hxax74r3", Label = "Delhi", Code = "432141" ,IsSelected=false,SelectionGroup="3r9h74r5"} ,
        //new SelectionMapView { UID = "3r9h7swxx4r3", Label = "Kashmir", Code = "432141" ,IsSelected=false,SelectionGroup="3r9h74r6"} ,
        //new SelectionMapView { UID = "3r9h7x4r3", Label = "Bihar", Code = "432141" ,IsSelected=false,SelectionGroup="3r9h74r6"} ,
        //new SelectionMapView { UID = "3r9a74xr3", Label = "Haryana", Code = "432142" ,IsSelected=false,SelectionGroup="3r9h74r7"} ,
        //new SelectionMapView { UID = "3r9hxx74r3", Label = "Westbengal", Code = "432142" ,IsSelected=false,SelectionGroup="3r9h74r7"} ,
        //new SelectionMapView { UID = "3r9hxa74r3", Label = "Manipur", Code = "432142" ,IsSelected=false,SelectionGroup="3r9h74r7"} ,
        //};
        private static IEnumerable<SelectionMapView> SalesTeamListDL = new List<SelectionMapView>()
        {
        new SelectionMapView { UID = "ougboweferg", Label = "Andhra Pradesh", Code = "432143",IsSelected=false ,SelectionGroup="3r9h74r1"} ,
        new SelectionMapView { UID = "3raw9h7cwkebfiwe4r2", Label = "Telangana", Code = "432143" ,IsSelected=false,SelectionGroup="3r9h74r1"} ,
        new SelectionMapView { UID = "3r9wefwefwelfbeeh74r3", Label = "TamilNadu", Code = "432143" ,IsSelected=false,SelectionGroup="3r9h74r2"} ,
        new SelectionMapView { UID = "3r9whewfw7x4fewffr3", Label = "karnataka", Code = "432143" ,IsSelected=false,SelectionGroup="3r9h74r2"} ,
        new SelectionMapView { UID = "3r9wdfwefewdewr74r3", Label = "Kerala", Code = "432143" ,IsSelected=false,SelectionGroup="3r9h74r3"} ,
        new SelectionMapView { UID = "3r9hewef7wcewx4r3", Label = "Maharastra", Code = "4321463" ,IsSelected=false,SelectionGroup="3r9h74r3"} ,
        new SelectionMapView { UID = "3r9whwef74r3", Label = "Madhyapradesh", Code = "432141" ,IsSelected=false,SelectionGroup="3r9h74r4"} ,
        new SelectionMapView { UID = "3r9swefwh7wx4r3", Label = "Gujarat", Code = "432141" ,IsSelected=false,SelectionGroup="3r9h74r4"} ,
        new SelectionMapView { UID = "3r9swefweffweh74r3", Label = "Punjab", Code = "432141" ,IsSelected=false,SelectionGroup="3r9h74r5"} ,
        new SelectionMapView { UID = "3r9hwefxax74r3", Label = "Delhi", Code = "432141" ,IsSelected=false,SelectionGroup="3r9h74r5"} ,
        new SelectionMapView { UID = "3r9hwef7swxx4r3", Label = "Kashmir", Code = "432141" ,IsSelected=false,SelectionGroup="3r9h74r6"} ,
        new SelectionMapView { UID = "3r9h7fweffx4r3", Label = "Bihar", Code = "432141" ,IsSelected=false,SelectionGroup="3r9h74r6"} ,
        new SelectionMapView { UID = "3r9af74xr3", Label = "Haryana", Code = "432142" ,IsSelected=false,SelectionGroup="3r9h74r7"} ,
        new SelectionMapView { UID = "3r9hxefx74r3", Label = "Westbengal", Code = "432142" ,IsSelected=false,SelectionGroup="3r9h74r7"} ,
        new SelectionMapView { UID = "3r9ewfhxa74r3", Label = "Manipur", Code = "432142" ,IsSelected=false,SelectionGroup="3r9h74r7"} ,
        };
        private static IEnumerable<SelectionMapView> RouteListDL = new List<SelectionMapView>()
        {
        new SelectionMapView { UID = "3r9cshfs74r1", Label = "route1", Code = "432143",IsSelected=false ,SelectionGroup="ougboweferg"} ,
        new SelectionMapView { UID = "3raw9sdfhfs74r2", Label = "route2", Code = "432143" ,IsSelected=false,SelectionGroup="3raw9h7cwkebfiwe4r2"} ,
        new SelectionMapView { UID = "3r9wwsffsfh74r3", Label = "route3", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wefwefwelfbeeh74r3"} ,
        new SelectionMapView { UID = "3r9whwsf7x4r3", Label = "route4", Code = "432143" ,IsSelected=false,SelectionGroup="3r9whewfw7x4fewffr3"} ,
        new SelectionMapView { UID = "3r9wdsfsfd74r3", Label = "route5", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wdfwefewdewr74r3"} ,
        new SelectionMapView { UID = "3r9h7wfsx4r3", Label = "route6", Code = "4321463" ,IsSelected=false,SelectionGroup="3r9hxefx74r3"} ,
        new SelectionMapView { UID = "3r9whsf74r3", Label = "route7", Code = "432141" ,IsSelected=false,SelectionGroup="3r9h7fweffx4r3"} ,
        new SelectionMapView { UID = "3r9swfssfh7wx4r3", Label = "route8", Code = "432141" ,IsSelected=false,SelectionGroup="3r9swefwh7wx4r3"} ,
        new SelectionMapView { UID = "3r9sfh7fs4r3", Label = "route9", Code = "432141" ,IsSelected=false,SelectionGroup="3r9swefweffweh74r3"} ,
        new SelectionMapView { UID = "3r9hxsfax74r3", Label = "route10", Code = "432141" ,IsSelected=false,SelectionGroup="3r9af74xr3"} ,
        };
        private static IEnumerable<SelectionMapView> ChannelListDL = new List<SelectionMapView>()
        {
        new SelectionMapView { UID = "3r9dwwefcwshfs74r1", Label = "Channel1", Code = "432143",IsSelected=false ,SelectionGroup="ougboweferg"} ,
        new SelectionMapView { UID = "3rafwfwf9sdfhwfs74r2", Label = "Channel2", Code = "432143" ,IsSelected=false,SelectionGroup="3raw9h7cwkebfiwe4r2"} ,
        new SelectionMapView { UID = "3r9wfwwsffsfhwf74r3", Label = "Channel3", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wefwefwelfbeeh74r3"} ,
        };
        private static IEnumerable<SelectionMapView> SubChannelListDL = new List<SelectionMapView>()
        {
        new SelectionMapView { UID = "3r9dwfwfwefwefcwshfs74r1", Label = "SubChannel1", Code = "432143",IsSelected=false ,SelectionGroup="3r9dwwefcwshfs74r1"} ,
        new SelectionMapView { UID = "3r9dwfwfwefwefcwshfssfs74r1", Label = "SubChannel2", Code = "432143",IsSelected=false ,SelectionGroup="3r9dwwefcwshfs74r1"} ,
        new SelectionMapView { UID = "3rafwfewfwf9swfwfhwffs74r2", Label = "SubChannel3", Code = "432143" ,IsSelected=false,SelectionGroup="3rafwfwf9sdfhwfs74r2"} ,
        new SelectionMapView { UID = "3r9wfwwfwfsffsffwhsfwf74r3", Label = "SubChannel4", Code = "432143" ,IsSelected=false,SelectionGroup="3rafwfwf9sdfhwfs74r2"} ,
        new SelectionMapView { UID = "3r9wfwwfwfssfffsffwsfhwf74r3", Label = "SubChannel5", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wfwwsffsfhwf74r3"} ,
        new SelectionMapView { UID = "3r9wfwwfwfsffsffsfwhwf74r3", Label = "SubChannel6", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wfwwsffsfhwf74r3"} ,
        };
        private static IEnumerable<SelectionMapView> SubSubChannelListDL = new List<SelectionMapView>()
        {
        new SelectionMapView { UID = "3r9dwfwfwefwgeefgfcwfgewrgshfssfs74r1", Label = "SubSubChannel1", Code = "432143",IsSelected=false ,SelectionGroup="3r9dwfwfwefwefcwshfs74r1"} ,
        new SelectionMapView { UID = "3rafwfewfwgeeggef9swfwfhwffs74r2", Label = "SubSubChannel2", Code = "432143" ,IsSelected=false,SelectionGroup="3r9dwfwfwefwefcwshfs74r1"} ,
        new SelectionMapView { UID = "3r9wfwwfwfeegegsffsffwhsfwf74r3", Label = "SubSubChannel3", Code = "432143" ,IsSelected=false,SelectionGroup="3r9dwfwfwefwefcwshfssfs74r1"} ,
        new SelectionMapView { UID = "3r9wfwwfwfeeegfewfesffsffsfwhwf74r3", Label = "SubSubChannel4", Code = "432143" ,IsSelected=false,SelectionGroup="3r9dwfwfwefwefcwshfssfs74r1"} ,
        new SelectionMapView { UID = "3r9wfwwfwfsgffeffsffsfwhwf74r3", Label = "SubSubChannel5", Code = "432143" ,IsSelected=false,SelectionGroup="3rafwfewfwf9swfwfhwffs74r2"} ,
        new SelectionMapView { UID = "3r9wfwwfwfsgegffsffsfwhwf74r3", Label = "SubSubChannel6", Code = "432143" ,IsSelected=false,SelectionGroup="3rafwfewfwf9swfwfhwffs74r2"} ,
        new SelectionMapView { UID = "3r9wfwwfwfsgefgefsgffsfwhwf74r3", Label = "SubSubChannel7", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wfwwfwfsffsffwhsfwf74r3"} ,
        new SelectionMapView { UID = "3r9wfwwfwfsfgefsffsfwhwf74r3", Label = "SubSubChannel8", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wfwwfwfsffsffwhsfwf74r3"} ,
        new SelectionMapView { UID = "3r9wfwwfwfegesffsgffsfwhwf74r3", Label = "SubSubChannel9", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wfwwfwfssfffsffwsfhwf74r3"} ,
        new SelectionMapView { UID = "3r9wfwwfwfsfegefsffsfwhwf74r3", Label = "SubSubChannel10", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wfwwfwfssfffsffwsfhwf74r3"} ,
        new SelectionMapView { UID = "3r9wfwwfwfgssfegefsffsfwhwfsg4r3", Label = "SubSubChannel11", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wfwwfwfsffsffsfwhwf74r3"} ,
        new SelectionMapView { UID = "3r9wfwwfwsffsfegefsffsfwhwf74r3", Label = "SubSubChannel12", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wfwwfwfsffsffsfwhwf74r3"} ,
        };
        private static IEnumerable<SelectionMapView> CustomerGroupListDL = new List<SelectionMapView>()
        {
        new SelectionMapView { UID = "3r9dwfwfwetfwgeefgfcwfggeewrgshfssfs74r1", Label = "SubSubChannel1", Code = "432143",IsSelected=false ,SelectionGroup="3r9dwfwfwefwgeefgfcwfgewrgshfssfs74r1"} ,
        new SelectionMapView { UID = "3rafwfewtefwteteettgeeggef9gegswfwfhwffs74r2", Label = "SubSubChannel2", Code = "432143" ,IsSelected=false,SelectionGroup="3rafwfewfwgeeggef9swfwfhwffs74r2"} ,
        new SelectionMapView { UID = "3r9wfwwertfwefeegegsffsegeffwhsfwf74r3", Label = "SubSubChannel3", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wfwwfwfeegegsffsffwhsfwf74r3"} ,
        new SelectionMapView { UID = "3r9wfwwftetwefeeegfeegwfesffsffsfwhwf74r3", Label = "SubSubChannel4", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wfwwfwfeeegfewfesffsffsfwhwf74r3"} ,
        new SelectionMapView { UID = "tewtert", Label = "SubSubChannel5", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wfwwfwfsgffeffsffsfwhwf74r3"} ,
        new SelectionMapView { UID = "3r9wfwwftwfsgegffgsffsfwhwf74r3", Label = "SubSubChannel6", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wfwwfwfsfgefsffsfwhwf74r3"} ,
        new SelectionMapView { UID = "3r9wfwwfwfesgefgefsgffsfwhwf74r3", Label = "SubSubChannel7", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wfwwfwfsgefgefsgffsfwhwf74r3"} ,
        new SelectionMapView { UID = "3r9wfwwetfwtfsfgefegsfefsfwhwf74r3", Label = "SubSubChannel8", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wfwwfwfsfgefsffsfwhwf74r3"} ,
        new SelectionMapView { UID = "3r9wfwwgettfwefegesfgfsgffsfwhwf74r3", Label = "SubSubChannel9", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wfwwfwfegesffsgffsfwhwf74r3"} ,
        new SelectionMapView { UID = "3r9wfwwfwfsfegegefegsgffsfwhwf74r3", Label = "SubSubChannel10", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wfwwfwfsfegefsffsfwhwf74r3"} ,
        new SelectionMapView { UID = "egrgegegege", Label = "SubSubChannel11", Code = "432143" ,IsSelected=false, SelectionGroup="3r9wfwwfwfgssfegefsffsfwhwfsg4r3"} ,
        new SelectionMapView { UID = "3r9wfwwfwsffsgefegeegfsffsfwhwf74r3", Label = "SubSubChannel12", Code = "432143" ,IsSelected=false,SelectionGroup="3r9wfwwfwsffsfegefsffsfwhwf74r3"} ,
        };
        private static IEnumerable<SelectionMapView> CountryList = CountryListDL;
        private static IEnumerable<SelectionMapView> RegionList = RegionListDL;
        private static IEnumerable<SelectionMapView> StateList = StateListDL;
        private static IEnumerable<SelectionMapView> SalesTeamList = SalesTeamListDL;
        private static IEnumerable<SelectionMapView> RouteList = RouteListDL;
        private static IEnumerable<SelectionMapView> ChannelList = ChannelListDL;
        private static IEnumerable<SelectionMapView> SubChannelList = SubChannelListDL;
        private static IEnumerable<SelectionMapView> SubSubChannelList = SubSubChannelListDL;
        private static IEnumerable<SelectionMapView> CustomerGroupList = CustomerGroupListDL;
        private static IEnumerable<SelectionMapView> DepotList = DepotListDL;
        private static IEnumerable<SelectionMapView> AreaList = AreaListDL;
        protected override async Task OnInitializedAsync()
        {
            RegionListDL = await GetDataFromAPIAsync("REGION");
            StateListDL = await GetDataFromAPIAsync("STATE");
            DepotListDL = await GetDataFromAPIAsync("DEPOT");
            AreaListDL = await GetDataFromAPIAsync("AREA");
            RegionList = RegionListDL;
            StateList = StateListDL;
            DepotList = DepotListDL;
            AreaList = AreaListDL;
            if (SelectedDict != null)
            {
                foreach (var kvp in SelectedDict)
                {
                    foreach (var value in kvp.Value)
                    {
                        UpdateSelectedvalue(kvp.Key, value, true);
                    }
                }
            }
        }
        private async Task<IEnumerable<SelectionMapView>> GetDataFromAPIAsync(string fieldName)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.SortCriterias = new List<Winit.Shared.Models.Enums.SortCriteria>();
                    pagingRequest.SortCriterias.Add(new Winit.Shared.Models.Enums.SortCriteria
                    {
                        SortParameter = "Name",
                        Direction = Winit.Shared.Models.Enums.SortDirection.Asc
                                                                
                    });
                pagingRequest.FilterCriterias = new List<Winit.Shared.Models.Enums.FilterCriteria>();
                pagingRequest.FilterCriterias.Add(new FilterCriteria { Name = "LocationTypeUID", Type = FilterType.Equal, Value = fieldName });
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Location/SelectAllLocationDetails",HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = GetDataFromResponse(apiResponse.Data);
                    PagedResponse<Winit.Modules.Location.Model.Classes.Location> pagedResponse = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Location.Model.Classes.Location>>(data);
                    if (pagedResponse != null)
                    {
                        List<SelectionMapView> DDdata = new List<SelectionMapView>();
                        foreach(var item in pagedResponse.PagedData)
                        {
                            SelectionMapView selectionMapView = new SelectionMapView();
                            selectionMapView.UID = item.UID;
                            selectionMapView.Label = item.Name;
                            selectionMapView.SelectionGroup = item.ParentUID;
                            DDdata.Add(selectionMapView);
                        }
                        return DDdata;
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private string GetDataFromResponse(string jsonString)
        {
            // Parse the JSON string into a JObject
            JObject jsonObject = JObject.Parse(jsonString);
            if (jsonObject != null)
            {
                // Get the value of the "Data" key
                JToken dataValue = jsonObject["Data"];
                if (dataValue != null)
                {
                    return dataValue.ToString();
                }
            }
            return jsonString;
        }
        public async void CountryHandler()
        {
            if (CountryDD.Items.Count(item => item.IsSelected) > 0)
            {
                //RegionList = RegionListDL
                //    .Where(item => CountryDD.Items
                //    .Where(channel => channel.IsSelected)
                //    .Select(channel => channel.UID).ToList().Contains(item.SelectionGroup));
            }
            else RegionList = RegionListDL;
        }
        public void RegionHandler() 
        {
            if (RegionDD.Items.Count(item => item.IsSelected) > 0)
            {
                StateList = StateListDL.Where(item => RegionDD.Items.Where(channel => channel.IsSelected).Select(channel => channel.UID).ToList<string>().Contains(item.SelectionGroup));
            }
            else StateList = StateListDL;
        }
        public void StateHandler()
        {

        }
        public void TeamHandler()
        {
            if (TeamDD.Items.Count(item => item.IsSelected) > 0)
            {
                RouteList= RouteListDL.Where(item => TeamDD.Items.Where(channel => channel.IsSelected).Select(channel => channel.UID).ToList<string>().Contains(item.SelectionGroup));
            }
            else RouteList = RouteListDL;
        }
        public void RouteHandler()
        {
            
        }
        public void  ChannelHandler()
        {
            if (ChannelDD.Items.Any(item => item.IsSelected))
            {
                 SubChannelList= SubChannelListDL.Where(item => ChannelDD.Items.Where(channel => channel.IsSelected).Select(channel => channel.UID).ToList<string>().Contains(item.SelectionGroup));
            }
            else  SubChannelList= SubChannelListDL;
        }
        public void SubChannelHandler()
        {
            if (SubChannelDD.Items.Count(item => item.IsSelected) > 0)
            {
                SubSubChannelList= SubSubChannelListDL.Where(item => SubChannelDD.Items.Where(Subchannel => Subchannel.IsSelected).Select(subchannel => subchannel.UID).ToList<string>().Contains(item.SelectionGroup));
            }
            else SubSubChannelList= SubSubChannelListDL;
        }
        public void SubSubChannelHandler()
        {
            if (SubSubChannelDD.Items.Count(item => item.IsSelected) > 0)
            {
                CustomerGroupList = CustomerGroupListDL.Where(item => SubSubChannelDD.Items.Where(Subchannel => Subchannel.IsSelected).Select(subchannel => subchannel.UID).ToList<string>().Contains(item.SelectionGroup));
            }
            else CustomerGroupList = CustomerGroupListDL;
        }
        public void CustomerGroupHandler()
        {
        }
        public void DepotHandler()
        {
        }
        public void AreaHandler()
        {
        }
        private async void UpdateSelectedvalue(MappingFieldName fieldName, string uID, bool isSelected)
        {
            switch(fieldName)
            {
                case MappingFieldName.Country:
                    UpdateSelectedCountry(uID, isSelected);
                    break;
                case MappingFieldName.Region:
                    UpdateSelectedRegion(uID, isSelected);
                    break;
                case MappingFieldName.State:
                    UpdateSelectedState(uID, isSelected);
                    break;
                case MappingFieldName.Depot:
                    UpdateSelectedDepot(uID, isSelected);
                    break;
                case MappingFieldName.Area:
                    UpdateSelectedArea(uID, isSelected);
                    break;
                case MappingFieldName.SalesTeam:
                    UpdateSelectedSalesTeam(uID, isSelected);
                    break;
                case MappingFieldName.Route:
                    UpdateSelectedRoute(uID, isSelected);
                    break;
                case MappingFieldName.Channel:
                    UpdateSelectedChannel(uID, isSelected);
                    break;
                case MappingFieldName.SubChannel:
                    UpdateSelectedSubChannel(uID, isSelected);
                    break;
                case MappingFieldName.SubSubChannel:
                    UpdateSelectedSubSubChannel(uID, isSelected);
                    break;
                case MappingFieldName.CustomerGroup:
                    UpdateSelectedCustomerGroup(uID, isSelected);
                    break;
                default:
                    break;
            }
        }
        private async void UpdateSelectedCountry(string UID, bool IsSelected)
        {
            var country =  CountryList.FirstOrDefault(country => country.UID == UID);
            if (country != null)
            {
                country.IsSelected = IsSelected;
            }
        }
        private async void UpdateSelectedRegion(string UID, bool IsSelected)
        {
            var regionToUpdate = RegionList.FirstOrDefault(region => region.UID == UID);
            if (regionToUpdate != null)
            {
                regionToUpdate.IsSelected = IsSelected;
            }
        }
        private async void UpdateSelectedState(string UID, bool IsSelected)
        {
            var stateToUpdate = StateList.FirstOrDefault(state => state.UID == UID);
            if (stateToUpdate != null)
            {
                stateToUpdate.IsSelected = IsSelected;
            }
        }
        private async void UpdateSelectedDepot(string UID, bool IsSelected)
        {
            //var depotToUpdate = DepotList.FirstOrDefault(depot => depot.UID == UID);
            //if (depotToUpdate != null)
            //{
            //    depotToUpdate.IsSelected = IsSelected;
            //}
        }
        private async void UpdateSelectedArea(string UID, bool IsSelected)
        {
            //var areaToUpdate = AreaList.FirstOrDefault(area => area.UID == UID);
            //if (areaToUpdate != null)
            //{
            //    areaToUpdate.IsSelected = IsSelected;
            //}
        }
        private async void UpdateSelectedSalesTeam(string UID, bool IsSelected)
        {
            var salesTeamToUpdate = SalesTeamList.FirstOrDefault(salesTeam => salesTeam.UID == UID);
            if (salesTeamToUpdate != null)
            {
                salesTeamToUpdate.IsSelected = IsSelected;
            }
        }
        private async void UpdateSelectedRoute(string UID, bool IsSelected)
        {
            var RouteToUpdate = RouteList.FirstOrDefault(Route => Route.UID == UID);
            if (RouteToUpdate != null)
            {
                RouteToUpdate.IsSelected = IsSelected;
            }
        }
        private async void UpdateSelectedChannel(string UID, bool IsSelected)
        {
            var channelToUpdate = ChannelList.FirstOrDefault(route => route.UID == UID);
            if (channelToUpdate != null)
            {
                channelToUpdate.IsSelected = IsSelected;
            }
        }
        private async void UpdateSelectedSubChannel(string UID, bool IsSelected)
        {
            var subChannelToUpdate = SubChannelList.FirstOrDefault(subChannel => subChannel.UID == UID);
            if (subChannelToUpdate != null)
            {
                subChannelToUpdate.IsSelected = IsSelected;
            }
        }
        private async void UpdateSelectedSubSubChannel(string UID, bool IsSelected)
        {
            var subSubChannelToUpdate = SubSubChannelList.FirstOrDefault(subSubChannel => subSubChannel.UID == UID);
            if (subSubChannelToUpdate != null)
            {
                subSubChannelToUpdate.IsSelected = IsSelected;
            }
        }
        private async void UpdateSelectedCustomerGroup(string UID, bool IsSelected)
        {
            var customerGroupToUpdate = CustomerGroupList.FirstOrDefault(customerGroup => customerGroup.UID == UID);
            if (customerGroupToUpdate != null)
            {
                customerGroupToUpdate.IsSelected = IsSelected;
            }
        }
        private void SaveClick()  
        {
            MappingViewModel MvM = new();
            if (CountryList!=null) MvM.CountryList = CountryList.Where(country=> country.IsSelected).ToList();
            if(RegionList!=null) MvM.RegionList = RegionList.Where(region=> region.IsSelected).ToList();
            if(StateList!=null) MvM.StateList = StateList.Where(state=> state.IsSelected).ToList();
            //MvM.DepotList = DepotList.Where(depot=> depot.IsSelected).ToList();
            //MvM.AreaList = AreaList.Where(area=> area.IsSelected).ToList();
            if(SalesTeamList!=null) MvM.SalesTeamList = SalesTeamList.Where(salesTeam => salesTeam.IsSelected).ToList();
            if(RouteList!=null) MvM.RouteList = RouteList.Where(route=> route.IsSelected).ToList();
            if(ChannelList!=null) MvM.ChannelList = ChannelList.Where(channel=> channel.IsSelected).ToList();
            if(SubChannelList!=null) MvM.SubChannelList = SubChannelList.Where(subchannel=> subchannel.IsSelected).ToList();
            if(SubSubChannelList!=null) MvM.SubSubChannelList = SubSubChannelList.Where(subsubchannel => subsubchannel.IsSelected).ToList();
            if(CustomerGroupList!=null) MvM.CustomerGroupList = CustomerGroupList.Where(customerGroup=> customerGroup.IsSelected).ToList();
            onSave.InvokeAsync(MvM);
        }
    }
}

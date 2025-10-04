namespace Winit.UIModels.Common.Mapping
{
    public class MappingViewModel
    {
        public string LinkedItemType { get; set; }
        public string LinkedItemUID { get; set; }
        public List<SelectionMapView> CountryList { get; set; }
        public List<SelectionMapView> RegionList { get; set; }
        public List<SelectionMapView> StateList { get; set; }
        public List<SelectionMapView> DepotList { get; set; }
        public List<SelectionMapView> AreaList { get; set; }
        public List<SelectionMapView> SalesTeamList { get; set; }
        public List<SelectionMapView> RouteList { get; set; }
        public List<SelectionMapView> ChannelList { get; set; }
        public List<SelectionMapView> SubChannelList { get; set; }
        public List<SelectionMapView> SubSubChannelList { get; set; }
        public List<SelectionMapView> CustomerGroupList { get; set; }
    }
}

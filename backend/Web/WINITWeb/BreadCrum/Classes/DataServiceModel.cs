namespace WinIt.BreadCrum.Classes
{
    public class DataServiceModel:Interfaces.IDataService
    {
        public string HeaderText { get; set; }
        public List<Interfaces.IBreadCrum> BreadcrumList { get; set; } 
    }
}

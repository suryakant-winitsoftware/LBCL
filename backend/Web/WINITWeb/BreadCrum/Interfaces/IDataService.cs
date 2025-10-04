namespace WinIt.BreadCrum.Interfaces
{
    public interface IDataService
    {
        public string HeaderText { get; set; }
        public List<IBreadCrum> BreadcrumList { get; set; }
    }
}

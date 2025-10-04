namespace WinIt
{
    public class DataService
    {
        public  string HeaderText { get; set; } 
        public  List<BreadCrum> BreadcrumList { get; set; }  =new List<BreadCrum>(); 
    }
    public class BreadCrum
    {
        public int SlNo { get;set; }
        public string Text { get;set; }
        public bool IsClickable {get;set; }
        public string URL { get;set; }
    }
}

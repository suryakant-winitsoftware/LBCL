namespace Winit.Modules.SKU.Model.Interfaces
{
    public interface ISKUListView
    {
        public string SKUUID { get; set; }
        public string SKUCode { get; set; }
        public string SKULongName { get; set; }
        public bool IsActive { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public string BrandOwnershipName { get; set; }
        public string SubCategoryName { get; set; }
        public string Type { get; set; }
        public string ProductCategoryId { get; set; } //Added by aziz
        public string ParentUID { get; set; }
    }
}

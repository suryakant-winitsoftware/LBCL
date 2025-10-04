using SyncManagerModel.Base;

namespace SyncManagerModel.Interfaces
{
    public interface IItemMaster : ISyncBaseModel
    {
        public long SyncLogId { get; set; }
        public string UID { get; set; }
        public string ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public string ModelCode { get; set; }
        public int Year { get; set; }
        public string Division { get; set; }
        public string ProductGroup { get; set; }
        public string ProductType { get; set; }
        public string ItemCategory { get; set; }
        public string Tonnage { get; set; }
        public string Capacity { get; set; }
        public string StarRating { get; set; }
        public string ProductCategory { get; set; }
        public string ProductCategoryId { get; set; }
        public string ParentItemCode { get; set; }
        public string ParentItemId { get; set; }
        public string ItemSeries { get; set; }
        public string ImageName { get; set; }
        public string HsnCode { get; set; }
        public string ItemStatus { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models.Store
{
    public class Store
    {
        //public int Id { get; set; }
        //public string UID { get; set; }
        //public string CompanyUID { get; set; }
        //public string Code { get; set; }
        //public string Number { get; set; }
        //public string Name { get; set; }
        //public string AliasName { get; set; }
        //public string Type { get; set; }
        //public string BillToStoreUID { get; set; }
        //public string ShipToStoreUID { get; set; }
        //public string SoldToStoreUID { get; set; }
        //public string PayerStoreUID { get; set; }
        //public int? Status { get; set; }
        //public bool? IsActive { get; set; }
        //public string StoreClass { get; set; }
        //public string StoreRating { get; set; }
        //public string OrgUID { get; set; }
        //public bool? IsBlocked { get; set; }
        //public string BlockedReasonCode { get; set; }
        //public string BlockedReasonDescription { get; set; }
        //public string CreatedByEmpUID { get; set; }
        //public string LocationUID { get; set; }
        //public bool? IsCreatedFromApp { get; set; }
        //public string PictureStatus { get; set; }
        //public string OrderProfile { get; set; }
        //public string FRWHOrgUID { get; set; }
        //public int? SS { get; set; }
        //public DateTime? CreatedTime { get; set; }
        //public DateTime? ModifiedTime { get; set; }
        //public DateTime? ServerAddTime { get; set; }
        //public DateTime? ServerModifiedTime { get; set; }




        public int Id { get; set; }
        public string UID { get; set; }
        public string CompanyUID { get; set; }
        public string Code { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string AliasName { get; set; }
        public string Type { get; set; }
        public string BillToStoreUID { get; set; }
        public string ShipToStoreUID { get; set; }
        public string SoldToStoreUID { get; set; }
        public int Status { get; set; }
        public bool IsActive { get; set; }
        public string StoreClass { get; set; }
        public string StoreRating { get; set; }
        public bool IsBlocked { get; set; }
        public string BlockedReasonCode { get; set; }
        public string BlockedReasonDescription { get; set; }
        public string CreatedByEmpUID { get; set; }
        public string CreatedByJobPositionUID { get; set; }
        public string CountryUID { get; set; }
        public string RegionUID { get; set; }
        public string CityUID { get; set; }
        public string Source { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }

        public DateTime? CreatedTime { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public DateTime? ServerAddTime { get; set; }
        public DateTime? ServerModifiedTime { get; set; }

    }

}

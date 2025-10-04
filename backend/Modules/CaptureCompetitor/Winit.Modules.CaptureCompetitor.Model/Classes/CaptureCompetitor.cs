using System;
using System.ComponentModel.DataAnnotations;
using Winit.Modules.CaptureCompetitor.Model.Interfaces;

namespace Winit.Modules.CaptureCompetitor.Model.Classes
{
    public class CaptureCompetitor : ICaptureCompetitor
    {
        [Key]
        public string uid { get; set; }
        public string UID { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public DateTime created_date { get; set; }
        public string created_by { get; set; }
        public DateTime? modified_date { get; set; }
        public string? modified_by { get; set; }
        public bool is_active { get; set; }
        public int Status { get; set; }
        public string StoreUID { get; set; }
        public string StoreHistoryUID { get; set; }
        public string BeatHistoryUID { get; set; }
        public string RouteUID { get; set; }
        public DateTime ActivityDate { get; set; }
        public string JobPositionUID { get; set; }
        public string EmpUID { get; set; }
        public decimal OurPrice { get; set; }
        public decimal OtherPrice { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ModifiedTime { get; set; }
    }
}
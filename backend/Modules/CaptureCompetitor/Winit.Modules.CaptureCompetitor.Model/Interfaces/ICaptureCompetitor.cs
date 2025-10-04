using System;

namespace Winit.Modules.CaptureCompetitor.Model.Interfaces
{
    public interface ICaptureCompetitor
    {
        string uid { get; set; }
        string UID { get; set; }
        string name { get; set; }
        string description { get; set; }
        DateTime created_date { get; set; }
        string created_by { get; set; }
        DateTime? modified_date { get; set; }
        string? modified_by { get; set; }
        bool is_active { get; set; }
        int Status { get; set; }
        string StoreUID { get; set; }
        string StoreHistoryUID { get; set; }
        string BeatHistoryUID { get; set; }
        string RouteUID { get; set; }
        DateTime ActivityDate { get; set; }
        string JobPositionUID { get; set; }
        string EmpUID { get; set; }
        decimal OurPrice { get; set; }
        decimal OtherPrice { get; set; }
        string CreatedBy { get; set; }
        string ModifiedBy { get; set; }
        DateTime CreatedTime { get; set; }
        DateTime ModifiedTime { get; set; }
    }
}
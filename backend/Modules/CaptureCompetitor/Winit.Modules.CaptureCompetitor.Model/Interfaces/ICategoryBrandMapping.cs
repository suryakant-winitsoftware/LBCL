using System;

namespace Winit.Modules.CaptureCompetitor.Model.Interfaces
{
    public interface ICategoryBrandMapping
    {
        string uid { get; set; }
        string UID { get; set; }
        string category_uid { get; set; }
        string brand_uid { get; set; }
        DateTime created_date { get; set; }
        string created_by { get; set; }
        DateTime? modified_date { get; set; }
        string? modified_by { get; set; }
        bool is_active { get; set; }
    }
}
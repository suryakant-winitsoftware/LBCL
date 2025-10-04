using System;
using System.ComponentModel.DataAnnotations;
using Winit.Modules.CaptureCompetitor.Model.Interfaces;

namespace Winit.Modules.CaptureCompetitor.Model.Classes
{
    public class CategoryBrandMapping : ICategoryBrandMapping
    {
        [Key]
        public string uid { get; set; }
        public string UID { get; set; }
        public string category_uid { get; set; }
        public string brand_uid { get; set; }
        public DateTime created_date { get; set; }
        public string created_by { get; set; }
        public DateTime? modified_date { get; set; }
        public string? modified_by { get; set; }
        public bool is_active { get; set; }
    }
}
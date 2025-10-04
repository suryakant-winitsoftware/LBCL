using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Interfaces;

namespace Winit.Modules.SKU.Model.Classes;

public class SKUV1 : SKU, ISKUV1
{
    public string? L1 { get; set; }
    public string? L2 { get; set; }
    public string? L3 { get; set; }
    public string? L4 { get; set; }
    public string? L5 { get; set; }
    public string? L6 { get; set; }
    public string ModelCode { get; set; }
    public int Year { get; set; }
    public string Type { get; set; }
    public string ProductType { get; set; }
    public string Category { get; set; }
    public string Tonnage { get; set; }
    public string Capacity { get; set; }
    public string StarRating { get; set; }
    public int ProductCategoryId { get; set; }
    public string ProductCategoryName { get; set; }
    public string ItemSeries { get; set; }
    public string HSNCode { get; set; }
    public bool IsAvailableInApMaster { get; set; }
    public HashSet<string> FilterKeys { get; set; } = new HashSet<string>();
}

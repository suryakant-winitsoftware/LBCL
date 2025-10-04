using Winit.Modules.DashBoard.Model.Interfaces;

namespace Winit.Modules.DashBoard.Model.Classes;

public class GrowthWiseChannelPartner : IGrowthWiseChannelPartner
{
    public string ChannelPartnerCode{get;set;}
    public string ChannelPartnerName{get;set;}
    public double CurrentYearSales{get;set;}
    public double LastYearSales{get;set;}
    public double SalesGrowth{get;set;}
    public double GrowthPercentage{get;set;}
}
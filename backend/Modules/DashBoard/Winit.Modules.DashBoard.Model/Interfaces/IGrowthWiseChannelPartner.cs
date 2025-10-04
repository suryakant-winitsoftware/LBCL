namespace Winit.Modules.DashBoard.Model.Interfaces;

public interface IGrowthWiseChannelPartner
{
    string ChannelPartnerCode{get;set;}
    string ChannelPartnerName{get;set;}
    double CurrentYearSales{get;set;}

    double LastYearSales{get;set;}

    double SalesGrowth{get;set;}

    double GrowthPercentage{get;set;}

}
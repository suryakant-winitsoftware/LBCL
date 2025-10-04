using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.Route.Model.Classes;
using Winit.Shared.Models.Common;
using System.Collections.Generic;
using System;

namespace WINITXUnitTest.UnitTest.Common.Fixtures;

public static class RouteTestFixture
{
    public static List<IRoute> CreateSampleRouteList()
    {
        return new List<IRoute>
        {
            CreateSampleRoute(),
            new Route
            {
                UID = "ROUTE-002",
                Name = "Test Route 2",
                Code = "TR2",
                IsActive = true,
                OrgUID = "ORG-001",
                CompanyUID = "COMP-001",
                WHOrgUID = "WH-001",
                VehicleUID = "VEH-001",
                JobPositionUID = "JP-001",
                LocationUID = "LOC-001",
                Status = "Active",
                ValidFrom = DateTime.Now,
                ValidUpto = DateTime.Now.AddYears(1),
                PrintStanding = true,
                PrintForward = true,
                PrintTopup = true,
                PrintOrderSummary = true,
                AutoFreezeJP = true,
                AddToRun = true,
                AutoFreezeRunTime = "18:00",
                TotalCustomers = 10,
                VisitTime = "09:00",
                EndTime = "17:00",
                VisitDuration = 30,
                TravelTime = 15,
                IsCustomerWithTime = true,
                RoleUID = "ROLE-001"
            }
        };
    }

    public static IRoute CreateSampleRoute()
    {
        return new Route
        {
            UID = "ROUTE-001",
            Name = "Test Route",
            Code = "TR1",
            IsActive = true,
            OrgUID = "ORG-001",
            CompanyUID = "COMP-001",
            WHOrgUID = "WH-001",
            VehicleUID = "VEH-001",
            JobPositionUID = "JP-001",
            LocationUID = "LOC-001",
            Status = "Active",
            ValidFrom = DateTime.Now,
            ValidUpto = DateTime.Now.AddYears(1),
            PrintStanding = true,
            PrintForward = true,
            PrintTopup = true,
            PrintOrderSummary = true,
            AutoFreezeJP = true,
            AddToRun = true,
            AutoFreezeRunTime = "18:00",
            TotalCustomers = 10,
            VisitTime = "09:00",
            EndTime = "17:00",
            VisitDuration = 30,
            TravelTime = 15,
            IsCustomerWithTime = true,
            RoleUID = "ROLE-001"
        };
    }

    public static List<IRouteChangeLog> CreateSampleRouteChangeLogList()
    {
        return new List<IRouteChangeLog>
        {
            new RouteChangeLog
            {
                UID = "CHANGE-001",
                serialNumber = 1,
                CompanyUID = "COMP-001",
                Code = "TR1",
                Name = "Test Route",
                OrgUID = "ORG-001",
                WHOrgUID = "WH-001",
                VehicleUID = "VEH-001",
                JobPositionUID = "JP-001",
                LocationUID = "LOC-001",
                IsActive = true,
                Status = "Active",
                ValidFrom = DateTime.Now,
                ValidUpto = DateTime.Now.AddYears(1),
                PrintStanding = true,
                PrintForward = true,
                PrintTopup = true,
                PrintOrderSummary = true,
                AutoFreezeJP = true,
                AddToRun = true,
                AutoFreezeRunTime = "18:00",
                IsChangeApplied = true,
                User = "USER-001",
                TotalCustomers = 10,
                VisitTime = "09:00",
                EndTime = "17:00",
                VisitDuration = 30,
                TravelTime = 15,
                IsCustomerWithTime = true,
                RoleUID = "ROLE-001"
            }
        };
    }

    public static RouteMaster CreateSampleRouteMaster()
    {
        return new RouteMaster
        {
            Route = new RouteChangeLog
            {
                UID = "ROUTE-001",
                serialNumber = 1,
                CompanyUID = "COMP-001",
                Code = "TR1",
                Name = "Test Route",
                OrgUID = "ORG-001",
                WHOrgUID = "WH-001",
                VehicleUID = "VEH-001",
                JobPositionUID = "JP-001",
                LocationUID = "LOC-001",
                IsActive = true,
                Status = "Active",
                ValidFrom = DateTime.Now,
                ValidUpto = DateTime.Now.AddYears(1),
                PrintStanding = true,
                PrintForward = true,
                PrintTopup = true,
                PrintOrderSummary = true,
                AutoFreezeJP = true,
                AddToRun = true,
                AutoFreezeRunTime = "18:00",
                IsChangeApplied = true,
                User = "USER-001",
                TotalCustomers = 10,
                VisitTime = "09:00",
                EndTime = "17:00",
                VisitDuration = 30,
                TravelTime = 15,
                IsCustomerWithTime = true,
                RoleUID = "ROLE-001"
            },
            RouteSchedule = new RouteSchedule
            {
                UID = "SCHEDULE-001",
                CompanyUID = "COMP-001",
                RouteUID = "ROUTE-001",
                Name = "Test Schedule",
                Type = "Daily",
                StartDate = DateTime.Now,
                Status = 1,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now.AddDays(30),
                StartTime = TimeSpan.Parse("09:00"),
                EndTime = TimeSpan.Parse("17:00"),
                VisitDurationInMinutes = 30,
                TravelTimeInMinutes = 15,
                NextBeatDate = DateTime.Now.AddDays(1),
                LastBeatDate = DateTime.Now,
                AllowMultipleBeatsPerDay = true,
                PlannedDays = "1,2,3,4,5"
            },
            RouteScheduleDaywise = new RouteScheduleDaywise
            {
                UID = "DAYWISE-001",
                RouteScheduleUID = "SCHEDULE-001",
                Monday = 1,
                Tuesday = 1,
                Wednesday = 1,
                Thursday = 1,
                Friday = 1,
                Saturday = 0,
                Sunday = 0
            },
            RouteScheduleFortnight = new RouteScheduleFortnight
            {
                UID = "FORTNIGHT-001",
                CompanyUID = "COMP-001",
                RouteScheduleUID = "SCHEDULE-001",
                Monday = 1,
                Tuesday = 1,
                Wednesday = 1,
                Thursday = 1,
                Friday = 1,
                Saturday = 0,
                Sunday = 0,
                MondayFN = 1,
                TuesdayFN = 1,
                WednesdayFN = 1,
                ThursdayFN = 1,
                FridayFN = 1,
                SaturdayFN = 0,
                SundayFN = 0
            },
            RouteCustomersList = new List<RouteCustomer>
            {
                new RouteCustomer
                {
                    UID = "CUSTOMER-001",
                    RouteUID = "ROUTE-001",
                    StoreUID = "STORE-001",
                    SeqNo = 1,
                    VisitTime = "09:00",
                    VisitDuration = 30,
                    EndTime = "09:30",
                    IsDeleted = false,
                    TravelTime = 15,
                    ActionType = Winit.Shared.Models.Enums.ActionType.Add
                }
            },
            RouteUserList = new List<RouteUser>
            {
                new RouteUser
                {
                    UID = "USER-001",
                    RouteUID = "ROUTE-001",
                    JobPositionUID = "JP-001",
                    FromDate = DateTime.Now,
                    ToDate = DateTime.Now.AddDays(30),
                    IsActive = true,
                    ActionType = Winit.Shared.Models.Enums.ActionType.Add
                }
            }
        };
    }

    public static List<ISelectionItem> CreateSampleVehicleDDL()
    {
        return new List<ISelectionItem>
        {
            new SelectionItem { UID = "VEH-001", Code = "V001", Label = "Vehicle 1", IsSelected = false, IsSelected_InDropDownLevel = false },
            new SelectionItem { UID = "VEH-002", Code = "V002", Label = "Vehicle 2", IsSelected = false, IsSelected_InDropDownLevel = false }
        };
    }

    public static List<ISelectionItem> CreateSampleWarehouseDDL()
    {
        return new List<ISelectionItem>
        {
            new SelectionItem { UID = "WH-001", Code = "WH001", Label = "Warehouse 1", IsSelected = false, IsSelected_InDropDownLevel = false },
            new SelectionItem { UID = "WH-002", Code = "WH002", Label = "Warehouse 2", IsSelected = false, IsSelected_InDropDownLevel = false }
        };
    }

    public static List<ISelectionItem> CreateSampleUserDDL()
    {
        return new List<ISelectionItem>
        {
            new SelectionItem { UID = "USR-001", Code = "U001", Label = "User 1", IsSelected = false, IsSelected_InDropDownLevel = false },
            new SelectionItem { UID = "USR-002", Code = "U002", Label = "User 2", IsSelected = false, IsSelected_InDropDownLevel = false }
        };
    }
} 
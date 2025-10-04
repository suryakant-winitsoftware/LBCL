using Moq;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace IntegrationTest.WINITAPIController.Route;

/// <summary>
/// Helper class for creating test data for Route integration tests.
/// Provides factory methods for creating mock and concrete Route objects.
/// </summary>
public static class RouteTestDataHelper
{
    #region Constants

    public const string DEFAULT_COMPANY_UID = "company-123";
    public const string DEFAULT_CODE = "ROUTE001";
    public const string DEFAULT_NAME = "Test Route";
    public const string DEFAULT_ORG_UID = "org-123";
    public const string DEFAULT_WH_ORG_UID = "wh-org-123";
    public const string DEFAULT_VEHICLE_UID = "vehicle-123";
    public const string DEFAULT_JOB_POSITION_UID = "job-pos-123";
    public const string DEFAULT_LOCATION_UID = "location-123";
    public const string DEFAULT_STATUS = "Active";
    public const string DEFAULT_VISIT_TIME = "09:00";
    public const string DEFAULT_END_TIME = "17:00";
    public const string DEFAULT_AUTO_FREEZE_RUN_TIME = "18:00";
    public const string DEFAULT_ROLE_UID = "role-123";
    public const int DEFAULT_TOTAL_CUSTOMERS = 10;
    public const int DEFAULT_VISIT_DURATION = 30;
    public const int DEFAULT_TRAVEL_TIME = 15;

    #endregion

    #region Mock Object Factory Methods

    /// <summary>
    /// Creates a mock IRoute object for testing.
    /// </summary>
    /// <param name="uid">The UID for the route</param>
    /// <returns>Mock IRoute object</returns>
    public static IRoute CreateMockRoute(string uid = null)
    {
        var mockRoute = new Mock<IRoute>();
        var routeUID = uid ?? Guid.NewGuid().ToString();

        mockRoute.Setup(x => x.UID).Returns(routeUID);
        mockRoute.Setup(x => x.CompanyUID).Returns(DEFAULT_COMPANY_UID);
        mockRoute.Setup(x => x.Code).Returns(DEFAULT_CODE);
        mockRoute.Setup(x => x.Name).Returns(DEFAULT_NAME);
        mockRoute.Setup(x => x.OrgUID).Returns(DEFAULT_ORG_UID);
        mockRoute.Setup(x => x.WHOrgUID).Returns(DEFAULT_WH_ORG_UID);
        mockRoute.Setup(x => x.VehicleUID).Returns(DEFAULT_VEHICLE_UID);
        mockRoute.Setup(x => x.JobPositionUID).Returns(DEFAULT_JOB_POSITION_UID);
        mockRoute.Setup(x => x.LocationUID).Returns(DEFAULT_LOCATION_UID);
        mockRoute.Setup(x => x.IsActive).Returns(true);
        mockRoute.Setup(x => x.Status).Returns(DEFAULT_STATUS);
        mockRoute.Setup(x => x.ValidFrom).Returns(DateTime.Now.AddDays(-30));
        mockRoute.Setup(x => x.ValidUpto).Returns(DateTime.Now.AddDays(30));
        mockRoute.Setup(x => x.PrintStanding).Returns(true);
        mockRoute.Setup(x => x.PrintForward).Returns(true);
        mockRoute.Setup(x => x.PrintTopup).Returns(false);
        mockRoute.Setup(x => x.PrintOrderSummary).Returns(true);
        mockRoute.Setup(x => x.AutoFreezeJP).Returns(false);
        mockRoute.Setup(x => x.AddToRun).Returns(true);
        mockRoute.Setup(x => x.AutoFreezeRunTime).Returns(DEFAULT_AUTO_FREEZE_RUN_TIME);
        mockRoute.Setup(x => x.TotalCustomers).Returns(DEFAULT_TOTAL_CUSTOMERS);
        mockRoute.Setup(x => x.VisitTime).Returns(DEFAULT_VISIT_TIME);
        mockRoute.Setup(x => x.EndTime).Returns(DEFAULT_END_TIME);
        mockRoute.Setup(x => x.VisitDuration).Returns(DEFAULT_VISIT_DURATION);
        mockRoute.Setup(x => x.TravelTime).Returns(DEFAULT_TRAVEL_TIME);
        mockRoute.Setup(x => x.IsCustomerWithTime).Returns(true);
        mockRoute.Setup(x => x.RoleUID).Returns(DEFAULT_ROLE_UID);
        mockRoute.Setup(x => x.ServerAddTime).Returns(DateTime.Now);
        mockRoute.Setup(x => x.ServerModifiedTime).Returns(DateTime.Now);

        return mockRoute.Object;
    }

    /// <summary>
    /// Creates a mock IRouteChangeLog object for testing.
    /// </summary>
    /// <param name="uid">The UID for the route change log</param>
    /// <returns>Mock IRouteChangeLog object</returns>
    public static IRouteChangeLog CreateMockRouteChangeLog(string uid = null)
    {
        var mockRouteChangeLog = new Mock<IRouteChangeLog>();
        var routeChangeLogUID = uid ?? Guid.NewGuid().ToString();

        mockRouteChangeLog.Setup(x => x.UID).Returns(routeChangeLogUID);
        mockRouteChangeLog.Setup(x => x.serialNumber).Returns(1);
        mockRouteChangeLog.Setup(x => x.CompanyUID).Returns(DEFAULT_COMPANY_UID);
        mockRouteChangeLog.Setup(x => x.Code).Returns(DEFAULT_CODE);
        mockRouteChangeLog.Setup(x => x.Name).Returns(DEFAULT_NAME);
        mockRouteChangeLog.Setup(x => x.OrgUID).Returns(DEFAULT_ORG_UID);
        mockRouteChangeLog.Setup(x => x.WHOrgUID).Returns(DEFAULT_WH_ORG_UID);
        mockRouteChangeLog.Setup(x => x.VehicleUID).Returns(DEFAULT_VEHICLE_UID);
        mockRouteChangeLog.Setup(x => x.JobPositionUID).Returns(DEFAULT_JOB_POSITION_UID);
        mockRouteChangeLog.Setup(x => x.LocationUID).Returns(DEFAULT_LOCATION_UID);
        mockRouteChangeLog.Setup(x => x.IsActive).Returns(true);
        mockRouteChangeLog.Setup(x => x.Status).Returns(DEFAULT_STATUS);
        mockRouteChangeLog.Setup(x => x.ValidFrom).Returns(DateTime.Now.AddDays(-30));
        mockRouteChangeLog.Setup(x => x.ValidUpto).Returns(DateTime.Now.AddDays(30));
        mockRouteChangeLog.Setup(x => x.PrintStanding).Returns(true);
        mockRouteChangeLog.Setup(x => x.PrintForward).Returns(true);
        mockRouteChangeLog.Setup(x => x.PrintTopup).Returns(false);
        mockRouteChangeLog.Setup(x => x.PrintOrderSummary).Returns(true);
        mockRouteChangeLog.Setup(x => x.AutoFreezeJP).Returns(false);
        mockRouteChangeLog.Setup(x => x.AddToRun).Returns(true);
        mockRouteChangeLog.Setup(x => x.AutoFreezeRunTime).Returns(DEFAULT_AUTO_FREEZE_RUN_TIME);
        mockRouteChangeLog.Setup(x => x.IsChangeApplied).Returns(false);
        mockRouteChangeLog.Setup(x => x.User).Returns("test-user");
        mockRouteChangeLog.Setup(x => x.TotalCustomers).Returns(DEFAULT_TOTAL_CUSTOMERS);
        mockRouteChangeLog.Setup(x => x.VisitTime).Returns(DEFAULT_VISIT_TIME);
        mockRouteChangeLog.Setup(x => x.EndTime).Returns(DEFAULT_END_TIME);
        mockRouteChangeLog.Setup(x => x.VisitDuration).Returns(DEFAULT_VISIT_DURATION);
        mockRouteChangeLog.Setup(x => x.TravelTime).Returns(DEFAULT_TRAVEL_TIME);
        mockRouteChangeLog.Setup(x => x.IsCustomerWithTime).Returns(true);
        mockRouteChangeLog.Setup(x => x.RoleUID).Returns(DEFAULT_ROLE_UID);
        mockRouteChangeLog.Setup(x => x.ServerAddTime).Returns(DateTime.Now);
        mockRouteChangeLog.Setup(x => x.ServerModifiedTime).Returns(DateTime.Now);

        return mockRouteChangeLog.Object;
    }

    /// <summary>
    /// Creates a mock IRouteMasterView object for testing.
    /// </summary>
    /// <param name="uid">The UID for the route master view</param>
    /// <returns>Mock IRouteMasterView object</returns>
    public static IRouteMasterView CreateMockRouteMasterView(string uid = null)
    {
        var mockRouteMasterView = new Mock<IRouteMasterView>();
        var routeMasterViewUID = uid ?? Guid.NewGuid().ToString();

        mockRouteMasterView.Setup(x => x.Route).Returns(CreateMockRouteChangeLog(routeMasterViewUID));
        mockRouteMasterView.Setup(x => x.RouteSchedule).Returns(CreateMockRouteSchedule());
        mockRouteMasterView.Setup(x => x.RouteScheduleDaywise).Returns(CreateMockRouteScheduleDaywise());
        mockRouteMasterView.Setup(x => x.RouteScheduleFortnight).Returns(CreateMockRouteScheduleFortnight());
        mockRouteMasterView.Setup(x => x.RouteCustomersList).Returns(new List<IRouteCustomer> { CreateMockRouteCustomer() });
        mockRouteMasterView.Setup(x => x.RouteUserList).Returns(new List<IRouteUser> { CreateMockRouteUser() });

        return mockRouteMasterView.Object;
    }

    /// <summary>
    /// Creates a mock IRouteSchedule object for testing.
    /// </summary>
    /// <returns>Mock IRouteSchedule object</returns>
    public static IRouteSchedule CreateMockRouteSchedule()
    {
        var mockRouteSchedule = new Mock<IRouteSchedule>();
        
        mockRouteSchedule.Setup(x => x.UID).Returns(Guid.NewGuid().ToString());
        mockRouteSchedule.Setup(x => x.CompanyUID).Returns(DEFAULT_COMPANY_UID);
        mockRouteSchedule.Setup(x => x.RouteUID).Returns(Guid.NewGuid().ToString());
        mockRouteSchedule.Setup(x => x.Name).Returns("Test Schedule");
        mockRouteSchedule.Setup(x => x.Type).Returns("Daily");
        mockRouteSchedule.Setup(x => x.StartDate).Returns(DateTime.Now);
        mockRouteSchedule.Setup(x => x.Status).Returns(1);
        mockRouteSchedule.Setup(x => x.FromDate).Returns(DateTime.Now);
        mockRouteSchedule.Setup(x => x.ToDate).Returns(DateTime.Now.AddDays(30));
        mockRouteSchedule.Setup(x => x.StartTime).Returns(TimeSpan.FromHours(9));
        mockRouteSchedule.Setup(x => x.EndTime).Returns(TimeSpan.FromHours(17));
        mockRouteSchedule.Setup(x => x.VisitDurationInMinutes).Returns(30);
        mockRouteSchedule.Setup(x => x.TravelTimeInMinutes).Returns(15);
        mockRouteSchedule.Setup(x => x.NextBeatDate).Returns(DateTime.Now.AddDays(1));
        mockRouteSchedule.Setup(x => x.LastBeatDate).Returns(DateTime.Now.AddDays(-1));
        mockRouteSchedule.Setup(x => x.AllowMultipleBeatsPerDay).Returns(false);
        mockRouteSchedule.Setup(x => x.PlannedDays).Returns("1,2,3,4,5");

        return mockRouteSchedule.Object;
    }

    /// <summary>
    /// Creates a mock IRouteScheduleDaywise object for testing.
    /// </summary>
    /// <returns>Mock IRouteScheduleDaywise object</returns>
    public static IRouteScheduleDaywise CreateMockRouteScheduleDaywise()
    {
        var mockRouteScheduleDaywise = new Mock<IRouteScheduleDaywise>();
        
        mockRouteScheduleDaywise.Setup(x => x.UID).Returns(Guid.NewGuid().ToString());
        // Add other properties as needed based on the interface

        return mockRouteScheduleDaywise.Object;
    }

    /// <summary>
    /// Creates a mock IRouteScheduleFortnight object for testing.
    /// </summary>
    /// <returns>Mock IRouteScheduleFortnight object</returns>
    public static IRouteScheduleFortnight CreateMockRouteScheduleFortnight()
    {
        var mockRouteScheduleFortnight = new Mock<IRouteScheduleFortnight>();
        
        mockRouteScheduleFortnight.Setup(x => x.UID).Returns(Guid.NewGuid().ToString());
        // Add other properties as needed based on the interface

        return mockRouteScheduleFortnight.Object;
    }

    /// <summary>
    /// Creates a mock IRouteCustomer object for testing.
    /// </summary>
    /// <returns>Mock IRouteCustomer object</returns>
    public static IRouteCustomer CreateMockRouteCustomer()
    {
        var mockRouteCustomer = new Mock<IRouteCustomer>();
        
        mockRouteCustomer.Setup(x => x.UID).Returns(Guid.NewGuid().ToString());
        mockRouteCustomer.Setup(x => x.RouteUID).Returns(Guid.NewGuid().ToString());
        mockRouteCustomer.Setup(x => x.StoreUID).Returns(Guid.NewGuid().ToString());
        mockRouteCustomer.Setup(x => x.SeqNo).Returns(1);
        mockRouteCustomer.Setup(x => x.VisitTime).Returns("10:00");
        mockRouteCustomer.Setup(x => x.VisitDuration).Returns(30);
        mockRouteCustomer.Setup(x => x.EndTime).Returns("10:30");
        mockRouteCustomer.Setup(x => x.IsDeleted).Returns(false);
        mockRouteCustomer.Setup(x => x.TravelTime).Returns(15);
        mockRouteCustomer.Setup(x => x.ActionType).Returns(ActionType.Add);

        return mockRouteCustomer.Object;
    }

    /// <summary>
    /// Creates a mock IRouteUser object for testing.
    /// </summary>
    /// <returns>Mock IRouteUser object</returns>
    public static IRouteUser CreateMockRouteUser()
    {
        var mockRouteUser = new Mock<IRouteUser>();
        
        mockRouteUser.Setup(x => x.UID).Returns(Guid.NewGuid().ToString());
        mockRouteUser.Setup(x => x.RouteUID).Returns(Guid.NewGuid().ToString());
        mockRouteUser.Setup(x => x.JobPositionUID).Returns(DEFAULT_JOB_POSITION_UID);
        mockRouteUser.Setup(x => x.FromDate).Returns(DateTime.Now);
        mockRouteUser.Setup(x => x.ToDate).Returns(DateTime.Now.AddDays(30));
        mockRouteUser.Setup(x => x.IsActive).Returns(true);
        mockRouteUser.Setup(x => x.ActionType).Returns(ActionType.Add);

        return mockRouteUser.Object;
    }

    /// <summary>
    /// Creates a list of mock ISelectionItem objects for dropdown testing.
    /// </summary>
    /// <returns>List of mock ISelectionItem objects</returns>
    public static List<ISelectionItem> CreateMockSelectionItems()
    {
        var selectionItems = new List<ISelectionItem>();
        
        for (int i = 1; i <= 3; i++)
        {
            var mockSelectionItem = new Mock<ISelectionItem>();
            mockSelectionItem.Setup(x => x.UID).Returns($"uid-{i}");
            mockSelectionItem.Setup(x => x.Code).Returns($"code-{i}");
            mockSelectionItem.Setup(x => x.Label).Returns($"Label {i}");
            mockSelectionItem.Setup(x => x.ExtData).Returns(null);
            mockSelectionItem.Setup(x => x.IsSelected).Returns(false);
            mockSelectionItem.Setup(x => x.IsSelected_InDropDownLevel).Returns(false);
            selectionItems.Add(mockSelectionItem.Object);
        }

        return selectionItems;
    }

    #endregion

    #region Concrete Object Factory Methods

    /// <summary>
    /// Creates a concrete Route object for testing serialization.
    /// </summary>
    /// <param name="uid">The UID for the route</param>
    /// <returns>Concrete Route object</returns>
    public static Winit.Modules.Route.Model.Classes.Route CreateConcreteRoute(string uid = null)
    {
        return new Winit.Modules.Route.Model.Classes.Route
        {
            UID = uid ?? Guid.NewGuid().ToString(),
            CompanyUID = DEFAULT_COMPANY_UID,
            Code = DEFAULT_CODE,
            Name = DEFAULT_NAME,
            OrgUID = DEFAULT_ORG_UID,
            WHOrgUID = DEFAULT_WH_ORG_UID,
            VehicleUID = DEFAULT_VEHICLE_UID,
            JobPositionUID = DEFAULT_JOB_POSITION_UID,
            LocationUID = DEFAULT_LOCATION_UID,
            IsActive = true,
            Status = DEFAULT_STATUS,
            ValidFrom = DateTime.Now.AddDays(-30),
            ValidUpto = DateTime.Now.AddDays(30),
            PrintStanding = true,
            PrintForward = true,
            PrintTopup = false,
            PrintOrderSummary = true,
            AutoFreezeJP = false,
            AddToRun = true,
            AutoFreezeRunTime = DEFAULT_AUTO_FREEZE_RUN_TIME,
            TotalCustomers = DEFAULT_TOTAL_CUSTOMERS,
            VisitTime = DEFAULT_VISIT_TIME,
            EndTime = DEFAULT_END_TIME,
            VisitDuration = DEFAULT_VISIT_DURATION,
            TravelTime = DEFAULT_TRAVEL_TIME,
            IsCustomerWithTime = true,
            RoleUID = DEFAULT_ROLE_UID,
            ServerAddTime = DateTime.Now,
            ServerModifiedTime = DateTime.Now
        };
    }

    /// <summary>
    /// Creates a concrete RouteMaster object for testing serialization.
    /// </summary>
    /// <returns>Concrete RouteMaster object</returns>
    public static RouteMaster CreateConcreteRouteMaster()
    {
        return new RouteMaster
        {
            Route = new RouteChangeLog
            {
                UID = Guid.NewGuid().ToString(),
                serialNumber = 1,
                CompanyUID = DEFAULT_COMPANY_UID,
                Code = DEFAULT_CODE,
                Name = DEFAULT_NAME,
                OrgUID = DEFAULT_ORG_UID,
                WHOrgUID = DEFAULT_WH_ORG_UID,
                VehicleUID = DEFAULT_VEHICLE_UID,
                JobPositionUID = DEFAULT_JOB_POSITION_UID,
                LocationUID = DEFAULT_LOCATION_UID,
                IsActive = true,
                Status = DEFAULT_STATUS,
                ValidFrom = DateTime.Now.AddDays(-30),
                ValidUpto = DateTime.Now.AddDays(30),
                PrintStanding = true,
                PrintForward = true,
                PrintTopup = false,
                PrintOrderSummary = true,
                AutoFreezeJP = false,
                AddToRun = true,
                AutoFreezeRunTime = DEFAULT_AUTO_FREEZE_RUN_TIME,
                IsChangeApplied = false,
                User = "test-user",
                TotalCustomers = DEFAULT_TOTAL_CUSTOMERS,
                VisitTime = DEFAULT_VISIT_TIME,
                EndTime = DEFAULT_END_TIME,
                VisitDuration = DEFAULT_VISIT_DURATION,
                TravelTime = DEFAULT_TRAVEL_TIME,
                IsCustomerWithTime = true,
                RoleUID = DEFAULT_ROLE_UID,
                ServerAddTime = DateTime.Now,
                ServerModifiedTime = DateTime.Now
            },
            RouteCustomersList = new List<RouteCustomer>(),
            RouteUserList = new List<RouteUser>()
        };
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a valid PagingRequest for testing.
    /// </summary>
    /// <returns>Valid PagingRequest object</returns>
    public static PagingRequest CreateValidPagingRequest()
    {
        return new PagingRequest
        {
            PageNumber = 1,
            PageSize = 10,
            IsCountRequired = true,
            SortCriterias = new List<SortCriteria>
            {
                new SortCriteria("Name", SortDirection.Asc)
            },
            FilterCriterias = new List<FilterCriteria>()
        };
    }

    #endregion
} 
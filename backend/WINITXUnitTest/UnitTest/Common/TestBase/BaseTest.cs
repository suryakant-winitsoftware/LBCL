using Microsoft.Extensions.DependencyInjection;
using Moq;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace WINITXUnitTest.UnitTest.Common.TestBase;

public abstract class BaseTest
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly Mock<IServiceProvider> _mockServiceProvider;

    protected BaseTest()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _serviceProvider = _mockServiceProvider.Object;
    }

    protected Mock<T> RegisterMockService<T>() where T : class
    {
        var mockService = new Mock<T>();
        _mockServiceProvider.Setup(x => x.GetService(typeof(T))).Returns(mockService.Object);
        return mockService;
    }

    protected void RegisterService<T>(T service) where T : class
    {
        _mockServiceProvider.Setup(x => x.GetService(typeof(T))).Returns(service);
    }

/// <summary>
/// Creates a list of filter criteria with the specified property name, value and filter operator.
/// </summary>
/// <param name="propertyName">The name of the property to filter on</param>
/// <param name="value">The value to compare against</param>
/// <param name="filterOperator">The type of comparison to perform (defaults to Equal)</param>
/// <returns>A list containing a single FilterCriteria instance</returns>
protected static List<FilterCriteria> CreateFilterCriteria(string propertyName, object value, FilterType filterType = FilterType.Equal)
{
    if (string.IsNullOrEmpty(propertyName))
        throw new ArgumentNullException(nameof(propertyName));

    return new List<FilterCriteria>
    {
        new FilterCriteria(
            name: propertyName,
            value: value,
            type: filterType,
            filterMode: FilterMode.And
        )
    };
}

    protected static List<SortCriteria> CreateSortCriteria(string propertyName, SortDirection sortDirection = SortDirection.Asc)
    {
        return new List<SortCriteria>
        {
            new SortCriteria(propertyName, sortDirection)
        };
    }
} 
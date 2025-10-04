using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WINITAPI.Controllers;
using WINITAPI.Controllers.RuleEngine.Model;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.RuleEngine.BL.Interfaces;
using Winit.Modules.RuleEngine.Model.Classes;
using Winit.Modules.RuleEngine.Model.Interfaces;
using Winit.Shared.Models.Common;
using Xunit;

namespace WINITXUnitTest.UnitTest.Controllers.RuleEngine
{
    public class RuleEngineControllerTests
    {
        private readonly Mock<IRuleEngineBL> _ruleEngineBLMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly RuleEngineController _controller;

        public RuleEngineControllerTests()
        {
            _ruleEngineBLMock = new Mock<IRuleEngineBL>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _controller = new RuleEngineController(
                _serviceProviderMock.Object,
                _ruleEngineBLMock.Object
            );
        }

        [Fact]
        public async Task SelectRuleByUID_ValidUID_ReturnsRule()
        {
            // Arrange
            var rule = new Rule { UID = "test-uid", Name = "Test Rule" };
            _ruleEngineBLMock.Setup(x => x.SelectRuleByUID("test-uid"))
                .ReturnsAsync(rule);

            // Act
            var result = await _controller.SelectRuleByUID("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedRule = Assert.IsType<Rule>(okResult.Value);
            Assert.Equal("test-uid", returnedRule.UID);
        }

        [Fact]
        public async Task SelectRuleByUID_InvalidUID_ReturnsNotFound()
        {
            // Arrange
            _ruleEngineBLMock.Setup(x => x.SelectRuleByUID("invalid-uid"))
                .ReturnsAsync((IRule)null);

            // Act
            var result = await _controller.SelectRuleByUID("invalid-uid");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateRule_ValidRule_ReturnsSuccess()
        {
            // Arrange
            var rule = new Rule { UID = "new-uid", Name = "New Rule" };
            _ruleEngineBLMock.Setup(x => x.CreateRule(It.IsAny<IRule>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreateRule(rule);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task CreateRule_InvalidRule_ReturnsError()
        {
            // Arrange
            var rule = new Rule { UID = "new-uid", Name = "New Rule" };
            _ruleEngineBLMock.Setup(x => x.CreateRule(It.IsAny<IRule>()))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.CreateRule(rule);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task UpdateRule_ValidRule_ReturnsSuccess()
        {
            // Arrange
            var rule = new Rule { UID = "existing-uid", Name = "Updated Rule" };
            _ruleEngineBLMock.Setup(x => x.SelectRuleByUID("existing-uid"))
                .ReturnsAsync(rule);
            _ruleEngineBLMock.Setup(x => x.UpdateRule(It.IsAny<Rule>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.UpdateRule(rule);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task UpdateRule_NonExistentRule_ReturnsNotFound()
        {
            // Arrange
            var rule = new Rule { UID = "non-existent-uid", Name = "Updated Rule" };
            _ruleEngineBLMock.Setup(x => x.SelectRuleByUID("non-existent-uid"))
                .ReturnsAsync((IRule)null);

            // Act
            var result = await _controller.UpdateRule(rule);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteRule_ValidUID_ReturnsSuccess()
        {
            // Arrange
            _ruleEngineBLMock.Setup(x => x.DeleteRule("test-uid"))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.DeleteRule("test-uid");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, okResult.Value);
        }

        [Fact]
        public async Task DeleteRule_InvalidUID_ReturnsError()
        {
            // Arrange
            _ruleEngineBLMock.Setup(x => x.DeleteRule("invalid-uid"))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.DeleteRule("invalid-uid");

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task SelectAllRuleDetails_ValidRequest_ReturnsPagedResponse()
        {
            // Arrange
            var pagingRequest = new PagingRequest
            {
                PageNumber = 1,
                PageSize = 10,
                FilterCriterias = new List<FilterCriteria>(),
                SortCriterias = new List<SortCriteria>()
            };

            var ruleList = new List<IRule>
            {
                new Rule { UID = "rule1", Name = "Rule 1" },
                new Rule { UID = "rule2", Name = "Rule 2" }
            };

            _ruleEngineBLMock.Setup(x => x.SelectAllRuleDetails(
                It.IsAny<List<SortCriteria>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<FilterCriteria>>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new PagedResponse<IRule>
                {
                    PagedData = ruleList,
                    TotalCount = 2
                });

            // Act
            var result = await _controller.SelectAllRuleDetails(pagingRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PagedResponse<IRule>>(okResult.Value);
            Assert.Equal(2, response.TotalCount);
        }

        [Fact]
        public async Task SelectAllRuleDetails_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var pagingRequest = new PagingRequest
            {
                PageNumber = -1,
                PageSize = -1
            };

            // Act
            var result = await _controller.SelectAllRuleDetails(pagingRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task EvaluateRule_ValidRule_ReturnsSuccess()
        {
            // Arrange
            var rule = new Rule { UID = "test-uid", Name = "Test Rule" };
            var context = new Dictionary<string, object>();
            _ruleEngineBLMock.Setup(x => x.EvaluateRule(rule, context))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.EvaluateRule(rule, context);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value);
        }

        [Fact]
        public async Task EvaluateRule_InvalidRule_ReturnsError()
        {
            // Arrange
            var rule = new Rule { UID = "test-uid", Name = "Test Rule" };
            var context = new Dictionary<string, object>();
            _ruleEngineBLMock.Setup(x => x.EvaluateRule(rule, context))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.EvaluateRule(rule, context);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task EvaluateRule_NullContext_ReturnsBadRequest()
        {
            // Arrange
            var rule = new Rule { UID = "test-uid", Name = "Test Rule" };

            // Act
            var result = await _controller.EvaluateRule(rule, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
} 
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WINITAPI.Controllers;
using WINITAPI.Controllers.ImageUpload.Model;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.ImageUpload.BL.Interfaces;
using Winit.Modules.ImageUpload.Model.Classes;
using Winit.Modules.ImageUpload.Model.Interfaces;
using Winit.Shared.Models.Common;
using Xunit;

namespace WINITXUnitTest.UnitTest.Controllers.ImageUpload
{
    public class ImageUploadControllerTests
    {
        private readonly Mock<IImageUploadBL> _imageUploadBLMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly ImageUploadController _controller;

        public ImageUploadControllerTests()
        {
            _imageUploadBLMock = new Mock<IImageUploadBL>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _controller = new ImageUploadController(
                _serviceProviderMock.Object,
                _imageUploadBLMock.Object
            );
        }

        [Fact]
        public async Task UploadImage_ValidImage_ReturnsSuccess()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var content = "Hello World from a Fake File";
            var fileName = "test.jpg";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);

            _imageUploadBLMock.Setup(x => x.UploadImage(It.IsAny<IFormFile>()))
                .ReturnsAsync("uploaded-image-url.jpg");

            // Act
            var result = await _controller.UploadImage(fileMock.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("uploaded-image-url.jpg", okResult.Value);
        }

        [Fact]
        public async Task UploadImage_NullFile_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.UploadImage(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UploadImage_EmptyFile_ReturnsBadRequest()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0);

            // Act
            var result = await _controller.UploadImage(fileMock.Object);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UploadImage_InvalidFileType_ReturnsBadRequest()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var content = "Hello World from a Fake File";
            var fileName = "test.txt";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);

            // Act
            var result = await _controller.UploadImage(fileMock.Object);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UploadImage_UploadFails_ReturnsError()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var content = "Hello World from a Fake File";
            var fileName = "test.jpg";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);

            _imageUploadBLMock.Setup(x => x.UploadImage(It.IsAny<IFormFile>()))
                .ReturnsAsync((string)null);

            // Act
            var result = await _controller.UploadImage(fileMock.Object);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task DeleteImage_ValidImageUrl_ReturnsSuccess()
        {
            // Arrange
            var imageUrl = "test-image.jpg";
            _imageUploadBLMock.Setup(x => x.DeleteImage(imageUrl))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteImage(imageUrl);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value);
        }

        [Fact]
        public async Task DeleteImage_InvalidImageUrl_ReturnsError()
        {
            // Arrange
            var imageUrl = "invalid-image.jpg";
            _imageUploadBLMock.Setup(x => x.DeleteImage(imageUrl))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteImage(imageUrl);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task DeleteImage_NullImageUrl_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.DeleteImage(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
} 
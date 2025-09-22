using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using PropertyManagement.Application.DTOs;
using PropertyManagement.Application.Mappings;
using PropertyManagement.Application.Services;
using PropertyManagement.Domain.Entities;
using PropertyManagement.Domain.Interfaces;
using PropertyManagement.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Tests.Unit;

[TestFixture]
public class PropertyServiceTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IPropertyRepository> _mockPropertyRepository;
    private Mock<IOwnerRepository> _mockOwnerRepository;
    private Mock<IPropertyTraceRepository> _mockPropertyTraceRepository;
    private Mock<ILogger<PropertyService>> _mockLogger;
    private IMapper _mapper;
    private PropertyService _propertyService;

    [SetUp]
    public void Setup()
    {
        // Setup mocks
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPropertyRepository = new Mock<IPropertyRepository>();
        _mockOwnerRepository = new Mock<IOwnerRepository>();
        _mockPropertyTraceRepository = new Mock<IPropertyTraceRepository>();
        _mockLogger = new Mock<ILogger<PropertyService>>();

        // Configure UnitOfWork
        _mockUnitOfWork.Setup(x => x.Properties).Returns(_mockPropertyRepository.Object);
        _mockUnitOfWork.Setup(x => x.Owners).Returns(_mockOwnerRepository.Object);
        _mockUnitOfWork.Setup(x => x.PropertyTraces).Returns(_mockPropertyTraceRepository.Object);

        // Setup AutoMapper
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _propertyService = new PropertyService(_mockUnitOfWork.Object, _mapper, _mockLogger.Object);
    }

    [Test]
    public async Task GetByIdAsync_ExistingProperty_ReturnsSuccessResponse()
    {
        // Arrange
        var propertyId = 1;
        var property = CreateSampleProperty(propertyId);

        _mockPropertyRepository.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync(property);

        // Act
        var result = await _propertyService.GetByIdAsync(propertyId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Name, Is.EqualTo("Test Property"));
        Assert.That(result.Data.IdProperty, Is.EqualTo(propertyId));

        _mockPropertyRepository.Verify(x => x.GetByIdAsync(propertyId), Times.Once);
    }

    [Test]
    public async Task GetByIdAsync_NonExistingProperty_ReturnsErrorResponse()
    {
        // Arrange
        var propertyId = 999;
        _mockPropertyRepository.Setup(x => x.GetByIdAsync(propertyId))
            .ReturnsAsync((Property?)null);

        // Act
        var result = await _propertyService.GetByIdAsync(propertyId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Property not found"));
        Assert.That(result.Data, Is.Null);
    }

    [Test]
    public async Task CreateAsync_ValidProperty_ReturnsSuccessResponse()
    {
        // Arrange
        var createDto = CreateSampleCreatePropertyDto();
        var createdProperty = CreateSampleProperty(1);

        _mockOwnerRepository.Setup(x => x.ExistsAsync(createDto.IdOwner)).ReturnsAsync(true);
        _mockPropertyRepository.Setup(x => x.CreateAsync(It.IsAny<Property>())).ReturnsAsync(createdProperty);
        _mockPropertyTraceRepository.Setup(x => x.CreateAsync(It.IsAny<PropertyTrace>())).ReturnsAsync(new PropertyTrace());

        // Act
        var result = await _propertyService.CreateAsync(createDto);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Name, Is.EqualTo(createDto.Name));
        Assert.That(result.Message, Is.EqualTo("Property created successfully"));

        _mockOwnerRepository.Verify(x => x.ExistsAsync(createDto.IdOwner), Times.Once);
        _mockPropertyRepository.Verify(x => x.CreateAsync(It.IsAny<Property>()), Times.Once);
        _mockPropertyTraceRepository.Verify(x => x.CreateAsync(It.IsAny<PropertyTrace>()), Times.Once);
    }

    [Test]
    public async Task CreateAsync_NonExistingOwner_ReturnsErrorResponse()
    {
        // Arrange
        var createDto = CreateSampleCreatePropertyDto();

        _mockOwnerRepository.Setup(x => x.ExistsAsync(createDto.IdOwner)).ReturnsAsync(false);

        // Act
        var result = await _propertyService.CreateAsync(createDto);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Owner not found"));
        Assert.That(result.Data, Is.Null);

        _mockPropertyRepository.Verify(x => x.CreateAsync(It.IsAny<Property>()), Times.Never);
    }

    [Test]
    public async Task GetFilteredAsync_ValidFilter_ReturnsPagedResults()
    {
        // Arrange
        var filter = new PropertyFilterDto { PageNumber = 1, PageSize = 10, MinPrice = 100000 };
        var domainFilter = new PropertyFilter { PageNumber = 1, PageSize = 10, MinPrice = 100000 };
        var properties = new List<Property> { CreateSampleProperty(1), CreateSampleProperty(2) };
        var pagedResult = new PagedResult<Property>
        {
            Data = properties,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        _mockPropertyRepository.Setup(x => x.GetFilteredAsync(It.IsAny<PropertyFilter>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _propertyService.GetFilteredAsync(filter);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Data.Count(), Is.EqualTo(2));
        Assert.That(result.Data.TotalCount, Is.EqualTo(2));
    }

    [Test]
    public async Task ChangePriceAsync_ValidPrice_ReturnsSuccessResponse()
    {
        // Arrange
        var propertyId = 1;
        var newPrice = 250000m;
        var property = CreateSampleProperty(propertyId);

        _mockPropertyRepository.Setup(x => x.GetByIdAsync(propertyId)).ReturnsAsync(property);
        _mockPropertyRepository.Setup(x => x.UpdateAsync(It.IsAny<Property>())).ReturnsAsync(property);
        _mockPropertyTraceRepository.Setup(x => x.CreateAsync(It.IsAny<PropertyTrace>())).ReturnsAsync(new PropertyTrace());

        // Act
        var result = await _propertyService.ChangePriceAsync(propertyId, newPrice);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.True);
        Assert.That(result.Message, Is.EqualTo("Price updated successfully"));
        Assert.That(property.Price, Is.EqualTo(newPrice));

        //VerifyTransactionFlow();
        _mockPropertyRepository.Verify(x => x.UpdateAsync(It.IsAny<Property>()), Times.Once);
        _mockPropertyTraceRepository.Verify(x => x.CreateAsync(It.IsAny<PropertyTrace>()), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_ExistingProperty_ReturnsSuccessResponse()
    {
        // Arrange
        var propertyId = 1;

        //SetupSuccessfulTransaction();
        _mockPropertyRepository.Setup(x => x.DeleteAsync(propertyId)).ReturnsAsync(true);

        // Act
        var result = await _propertyService.DeleteAsync(propertyId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.True);
        Assert.That(result.Message, Is.EqualTo("Property deleted successfully"));

        //VerifyTransactionFlow();
        _mockPropertyRepository.Verify(x => x.DeleteAsync(propertyId), Times.Once);
    }

    // Helper methods
    private Property CreateSampleProperty(int id)
    {
        return new Property
        {
            IdProperty = id,
            Name = "Test Property",
            Address = "123 Test St",
            Price = 200000,
            CodeInternal = $"TEST{id:000}",
            Year = 2023,
            IdOwner = 1,
            Enabled = true,
            Owner = new Owner { IdOwner = 1, Name = "Test Owner" },
            PropertyImages = new List<PropertyImage>(),
            PropertyTraces = new List<PropertyTrace>()
        };
    }

    private CreatePropertyDto CreateSampleCreatePropertyDto()
    {
        return new CreatePropertyDto
        {
            Name = "New Property",
            Address = "456 New St",
            Price = 300000,
            CodeInternal = "NEW001",
            Year = 2024,
            IdOwner = 1
        };
    }

}

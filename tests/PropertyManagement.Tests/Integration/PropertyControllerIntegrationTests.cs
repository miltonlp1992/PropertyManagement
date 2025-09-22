using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Newtonsoft.Json;
using PropertyManagement.Application.DTOs;
using PropertyManagement.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Tests.Integration;

[TestFixture]

public class PropertyControllerIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private string _adminToken;
    private string _userToken;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();

        // Get authentication tokens
        _adminToken = await GetAuthTokenAsync("admin", "Admin123!");
        _userToken = await GetAuthTokenAsync("testuser", "Test123!");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    private async Task<string> GetAuthTokenAsync(string username, string password)
    {
        var loginDto = new LoginDto { Username = username, Password = password };
        var json = JsonConvert.SerializeObject(loginDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/login", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to login: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var authResponse = JsonConvert.DeserializeObject<ApiResponseDto<AuthResponseDto>>(responseContent);

        return authResponse?.Data?.AccessToken ?? throw new InvalidOperationException("Failed to get auth token");
    }

    private void SetAuthHeader(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Test]
    public async Task Auth_LoginWithValidCredentials_ReturnsToken()
    {
        // Arrange
        var loginDto = new LoginDto { Username = "admin", Password = "Admin123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiResponseDto<AuthResponseDto>>(content);

        Assert.That(result?.Success, Is.True);
        Assert.That(result?.Data?.AccessToken, Is.Not.Null.And.Not.Empty);
        Assert.That(result?.Data?.Username, Is.EqualTo("admin"));
        Assert.That(result?.Data?.Role, Is.EqualTo("Admin"));
    }

    [Test]
    public async Task Properties_GetAll_WithUserAuth_ReturnsSuccess()
    {
        // Arrange
        SetAuthHeader(_userToken);

        // Act
        var response = await _client.GetAsync("/api/properties");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiResponseDto<PagedResultDto<PropertyDto>>>(content);

        Assert.That(result?.Success, Is.True);
        Assert.That(result?.Data, Is.Not.Null);
    }

    [Test]
    public async Task Properties_GetById_WithUserAuth_ReturnsProperty()
    {
        // Arrange
        SetAuthHeader(_userToken);
        var propertyId = 1; // Assuming seed data exists

        // Act
        var response = await _client.GetAsync($"/api/properties/{propertyId}");

        // Assert
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            Assert.Inconclusive("Property with ID 1 not found in test database");
            return;
        }

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiResponseDto<PropertyDto>>(content);

        Assert.That(result?.Success, Is.True);
        Assert.That(result?.Data?.IdProperty, Is.EqualTo(propertyId));
    }

    [Test]
    public async Task Properties_Create_WithAdminAuth_CreatesProperty()
    {
        // Arrange
        SetAuthHeader(_adminToken);
        var createDto = new CreatePropertyDto
        {
            Name = "Integration Test Property",
            Address = "123 Test Integration St",
            Price = 450000,
            CodeInternal = "INT001",
            Year = 2024,
            IdOwner = 1 // Assuming seed data exists
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/properties", createDto);

        // Assert
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorResult = JsonConvert.DeserializeObject<ApiResponseDto<PropertyDto>>(errorContent);
            if (errorResult?.Message?.Contains("Owner not found") == true)
            {
                Assert.Inconclusive("Owner with ID 1 not found in test database");
                return;
            }
        }

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiResponseDto<PropertyDto>>(content);

        Assert.That(result?.Success, Is.True);
        Assert.That(result?.Data?.Name, Is.EqualTo(createDto.Name));
        Assert.That(result?.Data?.Address, Is.EqualTo(createDto.Address));
    }

    [Test]
    public async Task Properties_Create_WithUserAuth_ReturnsForbidden()
    {
        // Arrange
        SetAuthHeader(_userToken);
        var createDto = new CreatePropertyDto
        {
            Name = "Unauthorized Property",
            Address = "123 Forbidden St",
            Price = 100000,
            IdOwner = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/properties", createDto);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task Properties_Create_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;
        var createDto = new CreatePropertyDto
        {
            Name = "No Auth Property",
            Address = "123 No Auth St",
            Price = 100000,
            IdOwner = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/properties", createDto);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Owners_GetAll_WithUserAuth_ReturnsOwners()
    {
        // Arrange
        SetAuthHeader(_userToken);

        // Act
        var response = await _client.GetAsync("/api/owners");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiResponseDto<IEnumerable<OwnerDto>>>(content);

        Assert.That(result?.Success, Is.True);
        Assert.That(result?.Data, Is.Not.Null);
    }

  
}

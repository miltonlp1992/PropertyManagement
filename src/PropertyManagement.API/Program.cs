using AutoMapper;
using PropertyManagement.Application.Mappings;
using PropertyManagement.Infrastructure.Data;
using PropertyManagement.Infrastructure.DependencyInjection;
using PropertyManagement.Application.DependencyInjection;
using PropertyManagement.API.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add Presentation services
builder.Services.AddPresentationServices(builder.Configuration);

// Add AutoMapper
var mapperConfig = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});
builder.Services.AddSingleton(mapperConfig.CreateMapper());

// Add Application services
builder.Services.AddApplicationServices();
// Add Infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Authentication and Authorization (ORDER IS IMPORTANT!)
app.UseAuthentication(); // Must come before UseAuthorization
app.UseAuthorization();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PropertyManagementContext>();
    context.Database.EnsureCreated();
}

app.Run();

/// <summary>
/// Program class for the Property Management API application.
/// </summary>
public partial class Program { }

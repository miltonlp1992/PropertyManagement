using Microsoft.Extensions.DependencyInjection;
using PropertyManagement.Application.DTOs;
using PropertyManagement.Application.DTOs.Auth;
using PropertyManagement.Application.Interfaces;
using PropertyManagement.Application.Mappings;
using PropertyManagement.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Application.DependencyInjection;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // JWT Token Service
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Application Services Registration
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<IPropertyImageService, PropertyImageService>();
        services.AddScoped<IOwnerService, OwnerService>();
        services.AddScoped<IPropertyTraceService, PropertyTraceService>();

        return services;
    }
}

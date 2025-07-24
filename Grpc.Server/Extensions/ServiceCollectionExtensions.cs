// ****************************************************************************
// Project:  Grpc.Server
// File:     ServiceCollectionExtensions.cs
// Author:   Latency McLaughlin
// Date:     07/24/2025
// ****************************************************************************

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Grpc.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, SymmetricSecurityKey securityKey)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer   = false,
                    ValidateActor    = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = securityKey
                };
            });

        return services;
    }

    public static IServiceCollection AddJwtAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
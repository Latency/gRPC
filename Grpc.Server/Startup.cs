using Grpc.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using Grpc.Server.Extensions;

namespace Grpc.Server;

internal class Startup
{
    private static readonly SymmetricSecurityKey SecurityKey = new(GenerateSecurityKey());


    private static byte[] GenerateSecurityKey()
    {
        var byteArray = new byte[256];
        var rnd       = new Random();
        rnd.NextBytes(byteArray); // Fills the array with random byte values
        return byteArray;
    }


    /// <inheritdoc cref="ConfigureServices" />
    /// <remarks>
    ///     Must be public.
    /// </remarks>
    /// <param name="services"></param>
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddGrpc();

        services.AddJwtAuthorization();
        services.AddJwtAuthentication(SecurityKey);
    }


    /// <inheritdoc cref="Configure" />
    /// <remarks>
    ///     Must be public.
    /// </remarks>
    /// <param name="app"></param>
    /// <param name="env"></param>
    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<UsersService>();

            endpoints.MapGet("/jwt", async context =>
            {
                var jwt = GenerateJwt();
                await context.Response.WriteAsync(jwt);
            });

            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
            });
        });
    }

    private static string GenerateJwt()
    {
        var credentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
        var header      = new JwtHeader(credentials);
        var payload     = new JwtPayload("GrpcServer", "GrpcClient", null, DateTime.Now, DateTime.Now.AddSeconds(60));
        var token       = new JwtSecurityToken(header, payload);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Regira.Security.Authentication.Jwt.Extensions;

namespace Web.Security.Testing.Infrastructure.Jwt;

public class JwtStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddJwtAuthentication(o =>
            {
                o.Secret = "0E4D39DB-407A-45D5-A3D9-7DCF03F43931";
                o.LifeSpan = 2;
            });

        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment env)
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
            endpoints
                .MapControllers()
                .RequireAuthorization();
        });
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DockerBlazor
{
    public static class StartupExtensions
    {
        public static void AddSwaggerServices(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(null);
        }

        public static void UseSwaggerUIAndDocs(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DockerBlazor API v1");
                c.RoutePrefix = "swagger";
            });
        }
    }
}
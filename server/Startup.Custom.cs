using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace GpEnerSaf
{
    public partial class Startup
    {
        partial void OnConfigureServices(IServiceCollection services)
        {
            StaticConfig = Configuration;
        }

        partial void OnConfigure(IApplicationBuilder app)
        {
            
        }

        public static IConfiguration StaticConfig { get; private set; }
    }
}
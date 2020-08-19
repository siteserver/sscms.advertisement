using Microsoft.Extensions.DependencyInjection;
using SSCMS.Advertisement.Abstractions;
using SSCMS.Advertisement.Core;
using SSCMS.Plugins;

namespace SSCMS.Advertisement
{
    public class Startup : IPluginConfigureServices
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IAdvertisementRepository, AdvertisementRepository>();
        }
    }
}

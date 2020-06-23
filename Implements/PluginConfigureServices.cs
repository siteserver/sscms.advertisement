using Microsoft.Extensions.DependencyInjection;
using SSCMS.Advertisement.Abstractions;
using SSCMS.Plugins;

namespace SSCMS.Advertisement.Implements
{
    public class PluginConfigureServices : IPluginConfigureServices
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IAdvertisementRepository, AdvertisementRepository>();
        }
    }
}

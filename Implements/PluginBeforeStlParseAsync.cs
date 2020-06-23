using System.Threading.Tasks;
using SSCMS.Advertisement.Abstractions;
using SSCMS.Plugins;

namespace SSCMS.Advertisement.Implements
{
    public class PluginBeforeStlParseAsync : IPluginBeforeStlParseAsync
    {
        private readonly IAdvertisementRepository _advertisementRepository;
        public PluginBeforeStlParseAsync(IAdvertisementRepository advertisementRepository)
        {
            _advertisementRepository = advertisementRepository;
        }

        public async Task BeforeStlParseAsync(IStlParseContext context)
        {
            await _advertisementRepository.AddAdvertisementsAsync(context);
        }
    }
}

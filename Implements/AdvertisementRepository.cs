using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datory;
using SSCMS.Advertisement.Abstractions;
using SSCMS.Advertisement.Core;
using SSCMS.Advertisement.Models;
using SSCMS.Services;

namespace SSCMS.Advertisement.Implements
{
    public class AdvertisementRepository : IAdvertisementRepository
    {
        private readonly IPluginManager _pluginManager;
        private readonly Repository<Models.Advertisement> _repository;

        public AdvertisementRepository(ISettingsManager settingsManager, IPluginManager pluginManager)
        {
            _repository = new Repository<Models.Advertisement>(new Database(settingsManager.DatabaseType, settingsManager.DatabaseConnectionString));
            _pluginManager = pluginManager;
        }

        private static string GetCacheKey(int siteId)
        {
            return $"SSCMS.Advertisement:{siteId}";
        }

        public async Task<Models.Advertisement> GetAsync(int siteId, int advertisementId)
        {
            var advertisements = await GetAllAsync(siteId);
            return advertisements.FirstOrDefault(x => x.Id == advertisementId);
        }

        public async Task<int> InsertAsync(Models.Advertisement ad)
        {
            return await _repository.InsertAsync(ad, Q.CachingRemove(GetCacheKey(ad.SiteId)));
        }

        public async Task<bool> UpdateAsync(Models.Advertisement ad)
        {
            return await _repository.UpdateAsync(ad, Q.CachingRemove(GetCacheKey(ad.SiteId)));
        }

        public async Task DeleteAsync(int siteId, int advertisementId)
        {
            await _repository.DeleteAsync(advertisementId, Q.CachingRemove(GetCacheKey(siteId)));
        }

        public async Task<bool> IsExistsAsync(string advertisementName, int siteId)
        {
            var advertisements = await GetAllAsync(siteId);
            return advertisements.Exists(x => x.AdvertisementName == advertisementName);
        }

        public async Task<List<Models.Advertisement>> GetAllAsync(int siteId)
        {
            return await _repository.GetAllAsync(Q
                .Where(nameof(Models.Advertisement.SiteId), siteId)
                .OrderByDesc(nameof(Models.Advertisement.Id))
                .CachingGet(GetCacheKey(siteId))
            );
        }

        public async Task<List<Models.Advertisement>> GetAllAsync(int siteId, AdvertisementType advertisementType)
        {
            var advertisements = await GetAllAsync(siteId);
            return advertisements.Where(x => x.AdvertisementType == advertisementType).ToList();
        }

        public async Task AddAdvertisementsAsync(IStlParseContext context)
        {
            var advertisements = await GetAllAsync(context.SiteId);
            var plugin = _pluginManager.Current;

            foreach (var advertisement in advertisements)
            {
                if (!AdvertisementUtils.IsAdvertisement(context, advertisement)) continue;

                var scripts = string.Empty;
                if (advertisement.AdvertisementType == AdvertisementType.FloatImage)
                {
                    context.HeadCodes[plugin.PluginId] = @"<script type=""text/javascript"" src=""/assets/adFloating.js""></script>";

                    var floatScript = new ScriptFloating(advertisement);
                    scripts = floatScript.GetScript();
                }
                else if (advertisement.AdvertisementType == AdvertisementType.ScreenDown)
                {
                    if (!context.HeadCodes.ContainsKey("Jquery"))
                    {
                        context.HeadCodes[plugin.PluginId] = @"<script type=""text/javascript"" src=""/assets/jquery-1.9.1.min.js""></script>";
                    }

                    var screenDownScript = new ScriptScreenDown(advertisement);
                    scripts = screenDownScript.GetScript();
                }
                else if (advertisement.AdvertisementType == AdvertisementType.OpenWindow)
                {
                    var openWindowScript = new ScriptOpenWindow(advertisement);
                    scripts = openWindowScript.GetScript();
                }

                context.BodyCodes[$"{plugin.PluginId}_{advertisement.Id}"] = scripts;
            }
        }
    }
}

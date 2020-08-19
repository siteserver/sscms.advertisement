using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Datory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Advertisement.Abstractions;
using SSCMS.Advertisement.Models;
using SSCMS.Advertisement.Utils;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Advertisement.Controllers.Admin
{
    [Authorize(Roles = AuthTypes.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class ListController : ControllerBase
    {
        private const string Route = "advertisement/list";

        private readonly IAuthManager _authManager;
        private readonly IChannelRepository _channelRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly IAdvertisementRepository _advertisementRepository;
        public ListController(IAuthManager authManager, IChannelRepository channelRepository, ITemplateRepository templateRepository, IAdvertisementRepository advertisementRepository)
        {
            _authManager = authManager;
            _channelRepository = channelRepository;
            _templateRepository = templateRepository;
            _advertisementRepository = advertisementRepository;
        }

        [HttpGet, Route(Route)]
        public async Task<ActionResult<ListResult>> GetList([FromQuery] ListRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, AdvertisementUtils.PermissionsList))
            {
                return Unauthorized();
            }

            var types = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(string.Empty, "<所有类型>"),
                    new KeyValuePair<string, string>(AdvertisementType.FloatImage.GetValue(),
                        AdvertisementType.FloatImage.GetDisplayName()),
                    new KeyValuePair<string, string>(AdvertisementType.ScreenDown.GetValue(),
                        AdvertisementType.ScreenDown.GetDisplayName()),
                    new KeyValuePair<string, string>(AdvertisementType.OpenWindow.GetValue(),
                        AdvertisementType.OpenWindow.GetDisplayName())
                };

            var advertisements = string.IsNullOrEmpty(request.AdvertisementType)
                ? await _advertisementRepository.GetAllAsync(request.SiteId)
                : await _advertisementRepository.GetAllAsync(request.SiteId,
                    TranslateUtils.ToEnum(request.AdvertisementType, AdvertisementType.FloatImage));

            foreach (var advertisement in advertisements)
            {
                advertisement.Set("display", await GetDisplayAsync(request.SiteId, advertisement));
                advertisement.Set("scope", advertisement.ScopeType.GetDisplayName());
                advertisement.Set("type", advertisement.AdvertisementType.GetDisplayName());
            }

            return new ListResult
            {
                Advertisements = advertisements,
                Types = types
            };
        }

        private async Task<string> GetDisplayAsync(int siteId, Models.Advertisement ad)
        {
            var builder = new StringBuilder();
            if (ad.ScopeType == ScopeType.Channels)
            {
                foreach (var channelId in ad.ChannelIds)
                {
                    var channelName = await _channelRepository.GetChannelNameNavigationAsync(siteId, channelId);
                    if (!string.IsNullOrEmpty(channelName))
                    {
                        builder.Append(channelName);
                    }
                    builder.Append(",");
                }
                builder.Length--;
            }
            else if (ad.ScopeType == ScopeType.Templates)
            {
                if (ad.TemplateIds != null)
                {
                    foreach (var templateId in ad.TemplateIds)
                    {
                        var templateName = await _templateRepository.GetTemplateNameAsync(templateId);
                        if (!string.IsNullOrEmpty(templateName))
                        {
                            builder.Append(templateName);
                        }
                        builder.Append(",");
                    }
                    builder.Length--;
                }
            }

            return builder.Length > 0 ? $"{ad.ScopeType.GetDisplayName()} - {builder}" : ad.ScopeType.GetDisplayName();
        }

        [HttpDelete, Route(Route)]
        public async Task<ActionResult<DeleteResult>> Delete([FromBody] DeleteRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, AdvertisementUtils.PermissionsList))
            {
                return Unauthorized();
            }

            await _advertisementRepository.DeleteAsync(request.SiteId, request.AdvertisementId);

            var advertisements = string.IsNullOrEmpty(request.AdvertisementType)
                ? await _advertisementRepository.GetAllAsync(request.SiteId)
                : await _advertisementRepository.GetAllAsync(request.SiteId,
                    TranslateUtils.ToEnum(request.AdvertisementType, AdvertisementType.FloatImage));

            foreach (var advertisement in advertisements)
            {
                advertisement.Set("display", await GetDisplayAsync(request.SiteId, advertisement));
                advertisement.Set("scope", advertisement.ScopeType.GetDisplayName());
                advertisement.Set("type", advertisement.AdvertisementType.GetDisplayName());
            }

            return new DeleteResult
            {
                Advertisements = advertisements
            };
        }
    }
}

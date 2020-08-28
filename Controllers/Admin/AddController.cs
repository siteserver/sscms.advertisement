using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Advertisement.Abstractions;
using SSCMS.Advertisement.Models;
using SSCMS.Advertisement.Utils;
using SSCMS.Configuration;
using SSCMS.Dto;
using SSCMS.Extensions;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Advertisement.Controllers.Admin
{
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class AddController : ControllerBase
    {
        private const string Route = "advertisement/add";
        private const string RouteActionsUpload = "advertisement/add/actions/upload";

        private readonly IAuthManager _authManager;
        private readonly IPathManager _pathManager;
        private readonly ISiteRepository _siteRepository;
        private readonly IChannelRepository _channelRepository;
        private readonly IContentRepository _contentRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly IAdvertisementRepository _advertisementRepository;
        public AddController(IAuthManager authManager, IPathManager pathManager, ISiteRepository siteRepository, IChannelRepository channelRepository, IContentRepository contentRepository, ITemplateRepository templateRepository, IAdvertisementRepository advertisementRepository)
        {
            _authManager = authManager;
            _pathManager = pathManager;
            _siteRepository = siteRepository;
            _channelRepository = channelRepository;
            _contentRepository = contentRepository;
            _templateRepository = templateRepository;
            _advertisementRepository = advertisementRepository;
        }

        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get([FromQuery] GetRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, AdvertisementUtils.PermissionsAdd))
            {
                return Unauthorized();
            }

            var advertisement = request.AdvertisementId > 0
                ? await _advertisementRepository.GetAsync(request.SiteId, request.AdvertisementId)
                : new Models.Advertisement
                {
                    AdvertisementType = AdvertisementType.FloatImage,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddMonths(1),
                    RollingType = RollingType.FollowingScreen,
                    PositionType = PositionType.LeftTop,
                    PositionX = 10,
                    PositionY = 120,
                    IsCloseable = true
                };

            var advertisementTypes = ListUtils.GetSelects<AdvertisementType>();
            var scopeTypes = ListUtils.GetSelects<ScopeType>();

            var site = await _siteRepository.GetAsync(request.SiteId);
            if (site == null) return NotFound();

            var channel = await _channelRepository.GetAsync(request.SiteId);
            var cascade = await _channelRepository.GetCascadeAsync(site, channel, async summary =>
            {
                var count = await _contentRepository.GetCountAsync(site, summary);
                return new
                {
                    Count = count
                };
            });

            var templates = await _templateRepository.GetSummariesAsync(request.SiteId);

            var positionTypes = ListUtils.GetSelects<PositionType>();

            var rollingTypes = ListUtils.GetSelects<RollingType>();

            return new GetResult
            {
                Advertisement = advertisement,
                AdvertisementTypes = advertisementTypes,
                ScopeTypes = scopeTypes,
                Channels = cascade,
                Templates = templates,
                PositionTypes = positionTypes,
                RollingTypes = rollingTypes
            };
        }

        [RequestSizeLimit(long.MaxValue)]
        [HttpPost, Route(RouteActionsUpload)]
        public async Task<ActionResult<UploadResult>> Upload([FromQuery] SiteRequest request, [FromForm] IFormFile file)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, AdvertisementUtils.PermissionsAdd))
            {
                return Unauthorized();
            }

            var site = await _siteRepository.GetAsync(request.SiteId);

            if (file == null)
            {
                return this.Error("请选择有效的文件上传");
            }

            var fileName = Path.GetFileName(file.FileName);

            var fileExtName = PathUtils.GetExtension(fileName).ToLower();
            var localDirectoryPath = await _pathManager.GetUploadDirectoryPathAsync(site, fileExtName);
            var localFileName = _pathManager.GetUploadFileName(site, fileName);
            var filePath = PathUtils.Combine(localDirectoryPath, localFileName);

            if (!FileUtils.IsImage(fileExtName))
            {
                return this.Error("请选择有效的图片文件上传");
            }

            await _pathManager.UploadAsync(file, filePath);

            var imageUrl = await _pathManager.GetSiteUrlByPhysicalPathAsync(site, filePath, true);

            var (width, height) = _pathManager.GetImageSize(filePath);

            return new UploadResult
            {
                ImageUrl = imageUrl,
                Width = width,
                Height = height
            };
        }

        [HttpPost, Route(Route)]
        public async Task<ActionResult<BoolResult>> Submit([FromBody] SubmitRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, AdvertisementUtils.PermissionsAdd))
            {
                return Unauthorized();
            }

            Models.Advertisement advertisement;
            if (request.AdvertisementId > 0)
            {
                advertisement = await _advertisementRepository.GetAsync(request.SiteId, request.AdvertisementId);
            }
            else
            {
                if (await _advertisementRepository.IsExistsAsync(request.AdvertisementName, request.SiteId))
                {
                    return this.Error("保存失败，已存在相同名称的广告！");
                }

                advertisement = new Models.Advertisement();
            }

            advertisement.SiteId = request.SiteId;
            advertisement.AdvertisementName = request.AdvertisementName;
            advertisement.AdvertisementType = request.AdvertisementType;
            advertisement.ScopeType = request.ScopeType;
            advertisement.ChannelIds = request.ChannelIds;
            advertisement.IsChannels = request.IsChannels;
            advertisement.IsContents = request.IsContents;
            advertisement.TemplateIds = request.TemplateIds;
            advertisement.IsDateLimited = request.IsDateLimited;
            advertisement.StartDate = request.StartDate;
            advertisement.EndDate = request.EndDate;
            advertisement.NavigationUrl = request.NavigationUrl;
            advertisement.ImageUrl = request.ImageUrl;
            advertisement.Width = request.Width;
            advertisement.Height = request.Height;
            advertisement.RollingType = request.RollingType;
            advertisement.PositionType = request.PositionType;
            advertisement.PositionX = request.PositionX;
            advertisement.PositionY = request.PositionY;
            advertisement.IsCloseable = request.IsCloseable;
            advertisement.Delay = request.Delay;

            if (advertisement.Id > 0)
            {
                await _advertisementRepository.UpdateAsync(advertisement);
            }
            else
            {
                await _advertisementRepository.InsertAsync(advertisement);
            }

            return new BoolResult
            {
                Value = true
            };
        }
    }
}

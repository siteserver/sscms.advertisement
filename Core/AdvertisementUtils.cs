using System;
using SSCMS.Enums;
using SSCMS.Utils;
using ScopeType = SSCMS.Advertisement.Models.ScopeType;

namespace SSCMS.Advertisement.Core
{
    public static class AdvertisementUtils
    {
        public const string PermissionsAdd = "advertisement_add";
        public const string PermissionsList = "advertisement_list";

        public static bool IsAdvertisement(IStlParseContext context, Models.Advertisement advertisement)
        {
            if (advertisement.IsDateLimited)
            {
                if (DateTime.Now < advertisement.StartDate || DateTime.Now > advertisement.EndDate)
                {
                    return false;
                }
            }

            if (advertisement.ScopeType == ScopeType.All)
            {
                return true;
            }

            if (advertisement.ScopeType == ScopeType.Templates)
            {
                return ListUtils.Contains(advertisement.TemplateIds, context.TemplateId);
            }

            if (advertisement.ScopeType == ScopeType.Channels)
            {
                if (context.TemplateType == TemplateType.FileTemplate) return false;
                if (!advertisement.IsChannels && (context.TemplateType == TemplateType.ContentTemplate || context.TemplateType == TemplateType.FileTemplate)) return false;
                if (!advertisement.IsContents && context.TemplateType == TemplateType.ContentTemplate) return false;

                return ListUtils.Contains(advertisement.ChannelIds, context.ChannelId);
            }

            return false;
        }
    }
}

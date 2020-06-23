using System.Collections.Generic;

namespace SSCMS.Advertisement.Controllers.Admin
{
    public partial class ListController
    {
        public class ListRequest
        {
            public int SiteId { get; set; }
            public string AdvertisementType { get; set; }
        }

        public class ListResult
        {
            public List<Models.Advertisement> Advertisements { get; set; }
            public List<KeyValuePair<string, string>>  Types { get; set; }
        }

        public class DeleteRequest
        {
            public int SiteId { get; set; }
            public int AdvertisementId { get; set; }
            public string AdvertisementType { get; set; }
        }

        public class DeleteResult
        {
            public List<Models.Advertisement> Advertisements { get; set; }
        }
    }
}

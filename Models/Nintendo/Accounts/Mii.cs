using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBremen.Models.Nintendo.Accounts
{
    class Mii
    {
        public string clientId { get; set; }

        public Dictionary<string, string> coreData { get; set; }

        public string etag { get; set; }

        public string favoriteColor { get; set; }

        public string id { get; set; }

        public string imageOrigin { get; set; }

        public string imageUriTemplate { get; set; }

        public Dictionary<string, string> storeData { get; set; }

        public string type { get; set; }

        public long updatedAt { get; set; }

        public Uri DefaultImageUri
        {
            get
            {
                return new($"https://{imageOrigin}/2.0.0/mii_images/{id}/{etag}.png");
            }
        }
    }
}

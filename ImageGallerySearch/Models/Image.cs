using Newtonsoft.Json;

namespace ImageGallerySearch.Models
{
    public class Image
    {
        public string Id { get; set; }

        [JsonProperty("cropped_picture")]
        public string Url { get; set; }
    }
}

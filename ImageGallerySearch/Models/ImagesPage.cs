using System.Collections.Generic;

namespace ImageGallerySearch.Models
{
    public class ImagesPage
    {
        public IEnumerable<Image> Pictures { get; set; }
        public int Page { get; set; }
        public int PageCount { get; set; }
        public bool HasMore { get; set; }
    }
}

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ImageGallerySearch.Models;

namespace ImageGallerySearch.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SearchController : ControllerBase
    {
        [HttpGet("{searchTerm}")]
        public IEnumerable<ImageMetadata> Get(string searchTerm)
        {
            return ImageDataCache.SearchImage(searchTerm.Trim());
        }
    }
}

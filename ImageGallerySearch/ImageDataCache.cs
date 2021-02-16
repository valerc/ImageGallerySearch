using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ImageGallerySearch.Models;

namespace ImageGallerySearch
{
    public static class ImageDataCache
    {
        private static IConfiguration _configuration;
        private static ImageGalleryApiHelper _apiHelper;

        private static Dictionary<string, ImageMetadata> _imageMetadataDict = new Dictionary<string, ImageMetadata>();
        private static Dictionary<string, List<string>> _imageIdsDict = new Dictionary<string, List<string>>();

        public static void Init(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiHelper = new ImageGalleryApiHelper(_configuration);

            var reloadTime = TimeSpan.FromMinutes(_configuration.GetValue<double>("CacheReloadTimeMinutes"));
            new System.Threading.Timer(ReloadCacheAsync, null, TimeSpan.Zero, reloadTime);
        }

        public static List<ImageMetadata> SearchImage(string searchTerm)
        {
            if (_imageIdsDict.ContainsKey(searchTerm))
            {
                var imageIds = _imageIdsDict[searchTerm];
                return imageIds.Select(x => _imageMetadataDict[x]).ToList();
            }
            else
            {
                var metadataKeys = _imageIdsDict.Keys.Where(x => x.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                var imageIds = metadataKeys.SelectMany(x => _imageIdsDict[x]);
                return imageIds.Select(x => _imageMetadataDict[x]).ToList();
            }
        }

        private static async void ReloadCacheAsync(object state)
        {
            var newImageMetadataDict = await _apiHelper.LoadImageMetadataAsync();
            _imageIdsDict = GetImageByMetadataDict(newImageMetadataDict.Values);
            _imageMetadataDict = newImageMetadataDict;
        }

        private static Dictionary<string, List<string>> GetImageByMetadataDict(IEnumerable<ImageMetadata> images)
        {
            var resultDict = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var fieldsToRetrieve = _configuration.GetSection("SearchImagesBy").Get<string[]>();

            foreach (var image in images)
            {
                foreach (var field in fieldsToRetrieve)
                {
                    var type = typeof(ImageMetadata);
                    var prop = type.GetProperty(field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    var value = (string)prop.GetValue(image);

                    if (field == "tags")
                    {
                        var tags = value.Split(' ').Select(x => x.Trim(' ', '#'));
                        foreach (var tag in tags)
                        {
                            AddMetadataFieldToDict(tag, resultDict, image);
                        }
                    }
                    else
                    {
                        AddMetadataFieldToDict(value, resultDict, image);
                    }
                }
            }

            return resultDict;
        }

        private static void AddMetadataFieldToDict(string value, Dictionary<string, List<string>> resultDict, ImageMetadata image)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            if (resultDict.TryGetValue(value, out List<string> imageIds))
            {
                imageIds.Add(image.Id);
            }
            else
            {
                resultDict.Add(value, new List<string> { image.Id });
            }
        }
    }
}

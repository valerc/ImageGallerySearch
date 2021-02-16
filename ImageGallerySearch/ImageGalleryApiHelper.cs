using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ImageGallerySearch.Models;

namespace ImageGallerySearch
{
    public class ImageGalleryApiHelper
    {
        private readonly string _apiHostUrl;
        public ImageGalleryApiHelper(IConfiguration configuration)
        {
            Configuration = configuration;
            _apiHostUrl = Configuration.GetValue<string>("EndpointData:ApiHostUrl");
            Authorize();
        }

        public IConfiguration Configuration { get; }
        private string _token;

        public async Task<Dictionary<string, ImageMetadata>> LoadImageMetadataAsync()
        {
            var metadataDict = new Dictionary<string, ImageMetadata>();
            var images = await LoadImagesAsync();

            foreach (var image in images)
            {
                var url = _apiHostUrl + "/images/" + image.Id;
                ImageMetadata imageMetadata;

                try
                {
                    imageMetadata = await MakeGetRequestAsync<ImageMetadata>(url);
                }
                catch (HttpException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Authorize();
                    imageMetadata = await MakeGetRequestAsync<ImageMetadata>(url);
                }

                if (imageMetadata != null)
                {
                    metadataDict.Add(imageMetadata.Id, imageMetadata);
                }
            }
            return metadataDict;
        }

        private async Task<List<Image>> LoadImagesAsync()
        {
            var images = new List<Image>();
            var pageNumber = 1;
            bool hasMore;
            do
            {
                try
                {
                    var page = await LoadImagesPageAsync(pageNumber);
                    images.AddRange(page.Pictures);
                    hasMore = page.HasMore;
                    pageNumber++;
                }
                catch
                {
                    hasMore = false;
                }
            } while (hasMore);

            return images;
        }

        private async Task<ImagesPage> LoadImagesPageAsync(int pageNumber)
        {
            var url = _apiHostUrl + "/images?page=" + pageNumber;
            try
            {
                return await MakeGetRequestAsync<ImagesPage>(url);
            }
            catch (HttpException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                Authorize();
                return await MakeGetRequestAsync<ImagesPage>(url);
            }
        }

        private async Task<T> MakeGetRequestAsync<T>(string url)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            else
            {
                throw new HttpException(response.StatusCode);
            }
        }

        private void Authorize()
        {
            var url = _apiHostUrl + "/auth";
            var apiKey = Configuration.GetValue<string>("EndpointData:ApiKey");
            var json = JsonConvert.SerializeObject(new { apiKey });
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var client = new HttpClient();
            var response = client.PostAsync(url, data).Result;
            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                _token = JsonConvert.DeserializeObject<AuthResponse>(responseContent).Token;
            }
        }
    }
}

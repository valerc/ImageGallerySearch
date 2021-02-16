using System;
using System.Net;

namespace ImageGallerySearch.Models
{
    public class HttpException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public HttpException(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }
    }
}
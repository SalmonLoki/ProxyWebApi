using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace ProxyWebApi.Middleware
{
    public class CacheMiddleware
    {
        private const string Url = "http://alfa-test-api.dev.kroniak.net/api/v1/products/";
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        
        public CacheMiddleware(RequestDelegate next, IMemoryCache memoryCache) {
            _next = next;
            _cache = memoryCache;
                        
            var httpClient = new HttpClient();
            
            var pages = new List<JsonObject>();
            
            var jsonString = httpClient.GetAsync(Url).Result.Content.ReadAsStringAsync().Result;
            var jsonObject = JsonConvert.DeserializeObject<JsonObject>(jsonString);
            var nextUrl = jsonObject.Next;
            pages.Add(jsonObject);           
            
            while (true)
            {
                try
                {
                    jsonString = httpClient.GetAsync(nextUrl).Result.Content.ReadAsStringAsync().Result;
                    jsonObject = JsonConvert.DeserializeObject<JsonObject>(jsonString);
                    nextUrl = jsonObject.Next;
                    pages.Add(jsonObject); 
                }
                catch (Exception e)
                {
                    break;
                }              
            }
            var cacheEntryOptions = new MemoryCacheEntryOptions();            
            _cache.Set(CacheKeys.Pages, pages, cacheEntryOptions);			
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.Value.ToLower() == "/products")
            {
                _cache.TryGetValue(CacheKeys.Pages, out List<JsonObject> cachePages);
                var page = cachePages[0];
                httpContext.Response.ContentType = "application/json";
                var jsonString = JsonConvert.SerializeObject(page);
                await httpContext.Response.WriteAsync(jsonString);
            }
            else {
                await _next.Invoke(httpContext);
            }
        }
    }
}
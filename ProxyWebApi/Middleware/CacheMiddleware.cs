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
            
            //здесь можно добавить чтение страниц в цикле по ссылке в коце страницы
            var jsonString = httpClient.GetAsync(Url).Result.Content.ReadAsStringAsync().Result;
            var jsonObject = JsonConvert.DeserializeObject<JsonObject>(jsonString);

            var cacheEntryOptions = new MemoryCacheEntryOptions();            
            _cache.Set(CacheKeys.JsonObject, jsonObject, cacheEntryOptions);			
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.Value.ToLower() == "/products")
            {
                _cache.TryGetValue(CacheKeys.JsonObject, out JsonObject cacheJsonObject);
                
                httpContext.Response.ContentType = "application/json";
                var jsonString = JsonConvert.SerializeObject(cacheJsonObject);
                await httpContext.Response.WriteAsync(jsonString);
            }
            else {
                await _next.Invoke(httpContext);
            }
        }
    }
}
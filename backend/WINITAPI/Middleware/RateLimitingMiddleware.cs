using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace WINITAPI.Middleware
{
    /// <summary>
    /// Middleware for API rate limiting
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly RateLimitOptions _options;

        public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache, RateLimitOptions options)
        {
            _next = next;
            _cache = cache;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var rateLimitDecorator = endpoint?.Metadata.GetMetadata<RateLimitAttribute>();
            
            if (rateLimitDecorator != null)
            {
                var key = GenerateClientKey(context);
                var rateLimitCounter = GetRateLimitCounter(key, rateLimitDecorator);

                if (rateLimitCounter.TotalRequests > rateLimitDecorator.MaxRequests)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    context.Response.Headers.Add("X-Rate-Limit-Limit", rateLimitDecorator.MaxRequests.ToString());
                    context.Response.Headers.Add("X-Rate-Limit-Remaining", "0");
                    context.Response.Headers.Add("X-Rate-Limit-Reset", rateLimitCounter.ResetTime.ToString("o"));
                    
                    await context.Response.WriteAsync("API rate limit exceeded. Please try again later.");
                    return;
                }

                context.Response.Headers.Add("X-Rate-Limit-Limit", rateLimitDecorator.MaxRequests.ToString());
                context.Response.Headers.Add("X-Rate-Limit-Remaining", 
                    Math.Max(0, rateLimitDecorator.MaxRequests - rateLimitCounter.TotalRequests).ToString());
                context.Response.Headers.Add("X-Rate-Limit-Reset", rateLimitCounter.ResetTime.ToString("o"));
            }

            await _next(context);
        }

        private string GenerateClientKey(HttpContext context)
        {
            var userId = context.User?.Identity?.Name ?? "anonymous";
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var path = context.Request.Path.ToString();
            
            return $"{userId}:{clientIp}:{path}";
        }

        private RateLimitCounter GetRateLimitCounter(string key, RateLimitAttribute rateLimitAttribute)
        {
            var cacheKey = $"rate_limit_{key}";
            
            _cache.TryGetValue(cacheKey, out RateLimitCounter counter);

            if (counter == null)
            {
                counter = new RateLimitCounter
                {
                    Timestamp = DateTime.UtcNow,
                    TotalRequests = 1,
                    ResetTime = DateTime.UtcNow.Add(rateLimitAttribute.Period)
                };
            }
            else
            {
                if (DateTime.UtcNow > counter.ResetTime)
                {
                    counter.Timestamp = DateTime.UtcNow;
                    counter.TotalRequests = 1;
                    counter.ResetTime = DateTime.UtcNow.Add(rateLimitAttribute.Period);
                }
                else
                {
                    counter.TotalRequests++;
                }
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(counter.ResetTime);

            _cache.Set(cacheKey, counter, cacheEntryOptions);

            return counter;
        }
    }

    public class RateLimitCounter
    {
        public DateTime Timestamp { get; set; }
        public int TotalRequests { get; set; }
        public DateTime ResetTime { get; set; }
    }

    public class RateLimitOptions
    {
        public int DefaultMaxRequests { get; set; } = 100;
        public TimeSpan DefaultPeriod { get; set; } = TimeSpan.FromMinutes(1);
        public bool EnableIpWhitelisting { get; set; } = false;
        public List<string> WhitelistedIps { get; set; } = new List<string>();
    }

    /// <summary>
    /// Attribute to configure rate limiting for specific endpoints
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RateLimitAttribute : Attribute
    {
        public int MaxRequests { get; set; }
        public TimeSpan Period { get; set; }

        public RateLimitAttribute(int maxRequests, int periodInSeconds)
        {
            MaxRequests = maxRequests;
            Period = TimeSpan.FromSeconds(periodInSeconds);
        }
    }
}
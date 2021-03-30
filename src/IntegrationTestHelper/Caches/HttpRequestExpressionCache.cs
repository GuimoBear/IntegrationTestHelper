using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;

namespace IntegrationTestHelper.Caches
{
    public class HttpRequestExpressionCache
    {
        private static readonly ConcurrentBag<CachedHttpRequestExpression> cached = new ConcurrentBag<CachedHttpRequestExpression>();

        public static Expression<Predicate<HttpRequestMessage>> Cached(HttpMethod method, string urlSuffix)
        {
            var cachedInstance = cached.FirstOrDefault(c => c.Match(method, urlSuffix));
            if (cachedInstance is not null)
                return cachedInstance;
            Expression<Predicate<HttpRequestMessage>> predicateExpression = request => request.Method == method && request.RequestUri.ToString().EndsWith(urlSuffix);
            cached.Add(new CachedHttpRequestExpression(method, urlSuffix, predicateExpression));
            return predicateExpression;
        }

        public static Expression<Predicate<HttpRequestMessage>> Get(string urlSuffix)
        {
            var cachedInstance = cached.FirstOrDefault(c => c.Match(null, urlSuffix));
            if (cachedInstance is not null)
                return cachedInstance;
            Expression<Predicate<HttpRequestMessage>> predicateExpression = request => request.RequestUri.ToString().EndsWith(urlSuffix);
            cached.Add(new CachedHttpRequestExpression(null, urlSuffix, predicateExpression));
            return predicateExpression;
        }

        public static Expression<Predicate<HttpRequestMessage>> Get(HttpMethod method)
        {
            var cachedInstance = cached.FirstOrDefault(c => c.Match(method, null));
            if (cachedInstance is not null)
                return cachedInstance;
            Expression<Predicate<HttpRequestMessage>> predicateExpression = request => request.Method == method;
            cached.Add(new CachedHttpRequestExpression(method, null, predicateExpression));
            return predicateExpression;
        }

        private class CachedHttpRequestExpression
        {
            private readonly HttpMethod method;
            private readonly string urlSuffix;
            private readonly Expression<Predicate<HttpRequestMessage>> predicateExpression;

            public CachedHttpRequestExpression(HttpMethod method, string urlSuffix, Expression<Predicate<HttpRequestMessage>> predicateExpression)
            {
                this.method = method;
                this.urlSuffix = urlSuffix;
                this.predicateExpression = predicateExpression ?? throw new ArgumentNullException(nameof(predicateExpression));
            }

            public override bool Equals(object obj)
            {
                if (obj is null || obj is not CachedHttpRequestExpression cachedObj)
                    return false;
                return Match(cachedObj.method, cachedObj.urlSuffix);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((method?.GetHashCode() ?? 0) * 1588) ^ ((urlSuffix?.GetHashCode() ?? 0) * 6657);
                }
            }

            public bool Match(HttpMethod method, string urlSuffix)
            {
                if (this.method is null && method is null &&
                    this.urlSuffix is null && urlSuffix is null)
                    return true;
                if ((this.method is null) ||
                    (this.urlSuffix is null))
                    return false;
                return this.method.Equals(method) &&
                       this.urlSuffix.Equals(urlSuffix);
            }

            public static implicit operator Expression<Predicate<HttpRequestMessage>>(CachedHttpRequestExpression cached)
                => cached.predicateExpression;
        }
    }
}

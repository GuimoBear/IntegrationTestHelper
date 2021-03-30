using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Net.Http;
using IntegrationTestHelper.Caches.Equality;

namespace IntegrationTestHelper.Caches
{
    public static class PredicateCache
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Key, Delegate>> _cache = new ConcurrentDictionary<Type, ConcurrentDictionary<Key, Delegate>>();

        public static Predicate<HttpRequestMessage> GetCachedPredicate(Type parentType, Expression<Predicate<HttpRequestMessage>> expression)
        {
            var key = new Key(expression);
            if (_cache.TryGetValue(parentType, out var cache))
                return (Predicate<HttpRequestMessage>)cache.GetOrAdd(key, k => expression.Compile());
            cache = new ConcurrentDictionary<Key, Delegate>();
            var @delegate = expression.Compile();
            cache.TryAdd(key, @delegate);
            _cache.TryAdd(parentType, cache);
            return @delegate;
        }

        private class Key
        {
            private static readonly ExpressionEqualityComparer equalityComparer = new ExpressionEqualityComparer();

            private readonly int hashCode;

            public Key(Expression expression)
            {
                hashCode = equalityComparer.GetHashCode(expression);
            }

            public override int GetHashCode()
                => hashCode;

            public override bool Equals(object obj)
            {
                if (obj is null || obj is not Key key)
                    return false;
                return key.GetHashCode().Equals(GetHashCode());
            }
        }
    }
}

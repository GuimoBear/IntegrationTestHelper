using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace IntegrationTestHelper.Caches
{
    public static class RegexCache
    {
        private static readonly ConcurrentDictionary<string, Regex> regexCache = new();

        public static Regex GetCached(string pattern)
            => GetCached(pattern, RegexOptions.None);

        public static Regex GetCached(string pattern, RegexOptions options)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return default;
            if (regexCache.TryGetValue(pattern, out var reg))
                return reg;
            reg = new Regex(pattern, options);
            regexCache.TryAdd(pattern, reg);
            return reg;
        }
    }
}

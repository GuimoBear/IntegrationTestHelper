using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using IntegrationTestHelper.Caches;
using IntegrationTestHelper.Caches.PooledObjects;

namespace IntegrationTestHelper.Request
{
    public class UrlRequestBuilder
    {
        public static string BasePrefix = "";

        private string prefix;
        public string Prefix
        {
            get => prefix;
            set => SetPrefix(value);
        }

        public string Template { get; }
        public string ApiVersion { get; }
        public Dictionary<string, string> RouteParameters { get; }
        public Dictionary<string, string> QueryParameters { get; }

        public UrlRequestBuilder(ControllerActionDescriptor actionDescriptor, Lazy<Dictionary<string, string>> routeParametersFromExpression = default, Lazy<Dictionary<string, string>> queryParametersFromExpression = default)
        {
            if (actionDescriptor is null)
                throw new ArgumentNullException(nameof(actionDescriptor));

            prefix = BasePrefix;
            ApiVersion = GetApiVersion(actionDescriptor);
            Template = regApiVersionRouteParameter.Replace(GetRouteTemplate(actionDescriptor) + GetActionTemplate(actionDescriptor), ApiVersion);
            RouteParameters = GetRouteParameters(Template, routeParametersFromExpression);
            QueryParameters = new();
            if (queryParametersFromExpression is not null && queryParametersFromExpression.IsValueCreated)
                foreach (var kvp in queryParametersFromExpression.Value)
                    QueryParameters.Add(kvp.Key, kvp.Value);
        }

        public UrlRequestBuilder WithPrefix(string prefix)
        {
            if (!string.IsNullOrWhiteSpace(prefix))
                SetPrefix(prefix);
            return this;
        }

        public UrlRequestBuilder WithQueryParameter(string name, string value, bool ignoreIfStringIsNullOrWriteSpace = true, bool overrideIfExists = true)
        {
            TryAddInDictionary(QueryParameters, name, value, ignoreIfStringIsNullOrWriteSpace, overrideIfExists);

            return this;
        }

        public UrlRequestBuilder WithRouteParameter(string name, string value, bool ignoreIfStringIsNullOrWriteSpace = true, bool overrideIfExists = true)
        {
            TryAddInDictionary(RouteParameters, name, value, ignoreIfStringIsNullOrWriteSpace, overrideIfExists);

            return this;
        }

        public string Build()
        {
            var ret = Template;
            foreach (var routeParameter in RouteParameters)
            {
                var regRouteParameter = RegexCache.GetCached(@$"\{{\s*{routeParameter.Key}(\s*:\s*(?<type>[\w\-_]+))?\s*\}}", RegexOptions.IgnoreCase);
                if (regRouteParameter.IsMatch(ret))
                    ret = regRouteParameter.Replace(ret, routeParameter.Value);
                else
                    ret += $"/{routeParameter.Key}";
            }

            if (QueryParameters.Count > 0)
                ret += $"?{string.Join("&", QueryParameters.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";

            return ret;
        }

        public override string ToString()
        {
            return Build();
        }

        private void TryAddInDictionary(Dictionary<string, string> dict, string name, string value, bool ignoreIfStringIsNullOrWriteSpace = true, bool overrideIfExists = true)
        {
            if (dict is null)
                throw new ArgumentNullException(nameof(dict));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (ignoreIfStringIsNullOrWriteSpace && string.IsNullOrWhiteSpace(value))
                return;

            if (overrideIfExists && dict.ContainsKey(HttpUtility.UrlEncode(name)))
                dict[HttpUtility.UrlEncode(name)] = HttpUtility.UrlEncode(value.Trim().Trim('/'));
            else if (!dict.ContainsKey(HttpUtility.UrlEncode(name)))
                dict.Add(HttpUtility.UrlEncode(name), HttpUtility.UrlEncode(value.Trim().Trim('/')));
        }

        private void SetPrefix(string prefix)
        {
            if (!Uri.TryCreate(prefix, UriKind.Relative, out _))
                throw new ArgumentException("The prefix is not an valid URL part", nameof(prefix));

            this.prefix = $"/{prefix.Trim().Trim('/')}";
        }

        private string GetRouteTemplate(ControllerActionDescriptor actionDescriptor)
        {
            var stringBuilder = PooledStringBuilder.GetInstance();

            foreach (var routeAttribute in actionDescriptor.EndpointMetadata.Where(o => o is RouteAttribute).Select(o => o as RouteAttribute))
                stringBuilder.Builder.AppendFormat("/{0}", SanitizeAndFillRouteWithParameters(routeAttribute.Template, actionDescriptor));
            return stringBuilder.ToStringAndFree();
        }

        private string GetActionTemplate(ControllerActionDescriptor actionDescriptor)
        {
            var httpMethodAttribute = actionDescriptor.EndpointMetadata.FirstOrDefault(o => o is HttpMethodAttribute) as HttpMethodAttribute;
            if (httpMethodAttribute is not null && !string.IsNullOrEmpty(httpMethodAttribute.Template))
                return $"/{SanitizeAndFillRouteWithParameters(httpMethodAttribute.Template, actionDescriptor)}";
            return string.Empty;
        }

        private string GetApiVersion(ControllerActionDescriptor actionDescriptor)
        {
            var apiVersionAttribute = actionDescriptor.EndpointMetadata.FirstOrDefault(o => o is ApiVersionAttribute) as ApiVersionAttribute;
            if (apiVersionAttribute is not null)
            {
                var version = apiVersionAttribute.Versions?.FirstOrDefault();
                if (version is not null)
                    return version.ToString("VVV");
            }
            return null;
        }

        private Dictionary<string, string> GetRouteParameters(string template, Lazy<Dictionary<string, string>> routeParametersFromExpression)
        {
            var parameters = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(template))
            {
                foreach (Match match in regRouteParameter.Matches(template))
                {
                    var name = HttpUtility.UrlEncode(match.Groups["name"].Value);
                    if (routeParametersFromExpression is not null && routeParametersFromExpression.IsValueCreated && routeParametersFromExpression.Value.TryGetValue(name, out string value))
                        parameters.Add(name, value);
                    else
                        parameters.Add(name, string.Empty);
                }
            }
            return parameters;
        }

        private string SanitizeAndFillRouteWithParameters(string route, ControllerActionDescriptor actionDescriptor)
            => route?
                  .Trim('/')?
                  .ToLower()?
                  .Replace("[controller]", actionDescriptor.ControllerName.ToLower(), StringComparison.OrdinalIgnoreCase)?
                  .Replace("[action]", actionDescriptor.ActionName.ToLower(), StringComparison.OrdinalIgnoreCase);

        private static readonly Regex regRouteParameter = new Regex(@"\{(?<name>[\w\-_]+)(:(?<type>[\w\-_]+))?\}", RegexOptions.Compiled);
        private static readonly Regex regApiVersionRouteParameter = new Regex(@"\{([\w\-_]+:)?apiversion\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }
}

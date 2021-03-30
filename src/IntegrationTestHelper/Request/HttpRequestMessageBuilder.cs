using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Web;
using IntegrationTestHelper.Caches;
using IntegrationTestHelper.Expressions;

namespace IntegrationTestHelper.Request
{
    public class HttpRequestMessageBuilder
    {
        private readonly HttpMethod method;

        private object body;
        //private Type bodyType;

        private readonly Lazy<Dictionary<string, string>> headerParameters = new();
        private readonly Lazy<Dictionary<string, string>> queryParameters = new();
        private readonly Lazy<Dictionary<string, string>> routeParameters = new();
        private readonly UrlRequestBuilder urlRequestBuilder;

        public HttpRequestMessageBuilder(MethodCallExpression actionExpression)
        {
            var controllerActionDescriptor = AspNetCache.Actions.FirstOrDefault(cad => cad.MethodInfo == actionExpression.Method);

            if (controllerActionDescriptor is null)
                throw new ArgumentException("The expression don't represent a known endpoint");

            method = GetMethodFromDescriptor(controllerActionDescriptor);

            FillParametersWithExpressionArguments(actionExpression);

            urlRequestBuilder = new UrlRequestBuilder(controllerActionDescriptor, routeParameters, queryParameters);

            FillTemplateRouteParametersWithoutAnnotation(actionExpression, controllerActionDescriptor);
        }

        public HttpRequestMessageBuilder WithUrl(Action<UrlRequestBuilder> configurer)
        {
            if (configurer is not null)
                configurer(urlRequestBuilder);
            return this;
        }

        public HttpRequestMessageBuilder WithBody<TType>(TType body)
        {
            this.body = body;
            //bodyType = typeof(TType);

            return this;
        }

        public HttpRequestMessageBuilder WithHeader(string name, string value, bool ignoreIfStringIsNullOrWriteSpace = true, bool overrideIfExists = true)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (ignoreIfStringIsNullOrWriteSpace && string.IsNullOrWhiteSpace(value))
                return this;

            if (overrideIfExists && headerParameters.Value.ContainsKey(name))
                headerParameters.Value[name] = value.Trim().Trim('/');
            else if (!headerParameters.Value.ContainsKey(name))
                headerParameters.Value.Add(name, value.Trim().Trim('/'));
            return this;
        }

        public HttpRequestMessage Build()
        {
            var req = new HttpRequestMessage(method, urlRequestBuilder.Build());
            if (headerParameters.IsValueCreated)
            {
                foreach (var header in headerParameters.Value)
                    req.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            if (body is not null)
                req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            return req;
        }

        #region constructor methods
        private HttpMethod GetMethodFromDescriptor(ControllerActionDescriptor actionDescriptor)
            => Cast(actionDescriptor.EndpointMetadata.FirstOrDefault(o => o is HttpMethodAttribute) as HttpMethodAttribute);

        private HttpMethod Cast(HttpMethodAttribute httpMethodAttribute)
            => httpMethodAttribute switch
            {
                HttpPostAttribute => HttpMethod.Post,
                HttpPutAttribute => HttpMethod.Put,
                HttpPatchAttribute => HttpMethod.Patch,
                HttpDeleteAttribute => HttpMethod.Delete,
                HttpHeadAttribute => HttpMethod.Head,
                HttpOptionsAttribute => HttpMethod.Options,
                _ => HttpMethod.Get
            };

        private void FillParametersWithExpressionArguments(MethodCallExpression actionExpression)
        {
            
            var parameters = actionExpression.Method.GetParameters();
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                Lazy<object> lazyValue = new Lazy<object>(() => ExpressionUtilities.GetValue(actionExpression.Arguments[index]));
                if (TryGetFromAttribute<FromBodyAttribute>(parameters[index], out _))
                {
                    body = lazyValue.Value;
                    //bodyType = parameter.ParameterType;
                }
                AddNamedParameterInCollectionIfAttributeExists<FromHeaderAttribute>(parameter, headerParameters, lazyValue);
                AddNamedParameterInCollectionIfAttributeExists<FromQueryAttribute>(parameter, queryParameters, lazyValue, (obj, name) => FormatObjectStringRepresentation(obj, $"&{name}="));
                AddNamedParameterInCollectionIfAttributeExists<FromRouteAttribute>(parameter, routeParameters, lazyValue, (obj, name) => FormatObjectStringRepresentation(obj, "/"));
            }
        }

        private void FillTemplateRouteParametersWithoutAnnotation(MethodCallExpression actionExpression, ControllerActionDescriptor actionDescriptor)
        {
            foreach (var routeParameter in urlRequestBuilder.RouteParameters.Where(kvp => string.IsNullOrEmpty(kvp.Value)))
            {
                var parameters = actionExpression.Method.GetParameters();
                for(int index = 0; index < parameters.Length; index++)
                {
                    if (parameters[index].Name.ToLower().Equals(routeParameter.Key))
                    {
                        urlRequestBuilder.WithRouteParameter(routeParameter.Key, ExpressionUtilities.GetValue(actionExpression.Arguments[index])?.ToString());
                    }
                }
            }
        }

        private static void AddNamedParameterInCollectionIfAttributeExists<TAttribute>(ParameterInfo parameter, Lazy<Dictionary<string, string>> lazyCollection, Lazy<object> lazyValue, Func<object, string, string> formatter = default, bool ignoreNullOrWriteSpaceValue = true) where TAttribute : Attribute, IBindingSourceMetadata, IModelNameProvider
        {
            if (TryGetFromAttribute<TAttribute>(parameter, out var namedAttribute))
            {
                var name = namedAttribute.Name ?? parameter.Name;
                var value = (formatter is null ? HttpUtility.UrlEncode(lazyValue.Value?.ToString()) : formatter(lazyValue.Value, name)) ?? "";
                if (!ignoreNullOrWriteSpaceValue || !string.IsNullOrWhiteSpace(value))
                    lazyCollection.Value.Add(HttpUtility.UrlEncode(name), value);
            }
        }

        private static bool TryGetFromAttribute<TAttribute>(ParameterInfo parameter, out TAttribute attribute) where TAttribute : Attribute, IBindingSourceMetadata
        {
            attribute = parameter.GetCustomAttribute<TAttribute>();
            return attribute is not null;
        }

        private string FormatObjectStringRepresentation(object obj, string separator)
        {
            if (obj is string)
                return HttpUtility.UrlEncode(obj as string);
            if (obj is IEnumerable values)
                return string.Join(separator, values.Cast<object>().Select(item => HttpUtility.UrlEncode(item.ToString())));
            return HttpUtility.UrlEncode(obj.ToString());
        }

        #endregion

        public static implicit operator HttpRequestMessage(HttpRequestMessageBuilder builder)
            => builder.Build();
    }
}

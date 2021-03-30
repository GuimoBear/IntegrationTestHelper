using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IntegrationTestHelper.Request
{
    public class Request
    {
        public static HttpRequestMessageBuilder FromAction<TController>(Expression<Func<TController, IActionResult>> actionAccessor)
            where TController : ControllerBase
        {
            if (actionAccessor is null)
                throw new ArgumentNullException(nameof(actionAccessor));

            return BuilderFromActionExpression((MethodCallExpression)actionAccessor.Body);
        }

        public static HttpRequestMessageBuilder FromAction<TController>(Expression<Func<TController, Task<IActionResult>>> asyncActionAccessor)
            where TController : ControllerBase
        {
            if (asyncActionAccessor is null)
                throw new ArgumentNullException(nameof(asyncActionAccessor));

            return BuilderFromActionExpression((MethodCallExpression)asyncActionAccessor.Body);
        }

        private static HttpRequestMessageBuilder BuilderFromActionExpression(MethodCallExpression methodCallExpression)
            => new HttpRequestMessageBuilder(methodCallExpression);
    }
}

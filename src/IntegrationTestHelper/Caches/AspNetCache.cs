using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace IntegrationTestHelper.Caches
{
    public static class AspNetCache
    {
        public static IEnumerable<ControllerActionDescriptor> Actions;

        public static IServiceCollection LoadEndpoints(this IServiceCollection services)
        {
            using var provider = services.BuildServiceProvider();

            var descriptorCollectionProvider = provider.GetRequiredService<IActionDescriptorCollectionProvider>();

            Actions = descriptorCollectionProvider.ActionDescriptors
                .Items
                .Where(x => x is ControllerActionDescriptor)
                .Select(x => x as ControllerActionDescriptor)
                .ToList();

            return services;
        }
    }
}

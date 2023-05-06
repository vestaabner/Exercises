using System;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.PersistanceServiceCollection
{
	public static class ServiceCollectionExtensions
	{
        public static void AddPersistanceServiceCollection(this IServiceCollection services, string apiKey)
        {
            services.PersistanceServiceCollection(nameof(services));
            apiKey.ThrowIfNull(nameof(apiKey));

            services.AddSingleton(fact =>
            {
                SendGridConfig sendGridConfig = new SendGridConfig(apiKey);
                return sendGridConfig;
            });
        }
    }
}


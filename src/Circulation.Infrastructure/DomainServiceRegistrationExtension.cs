using Circulation.Domain.Holds;
using Circulation.Domain.PublicationCopies;
using Circulation.Domain.Publications;
using Microsoft.Extensions.DependencyInjection;

namespace Circulation.Infrastructure
{
    public static class DomainServiceRegistrationExtension
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddScoped<AddPublicationCommandHandler>();
            services.AddScoped<AddPublicationCopyCommandHandler>();
            services.AddScoped<PlaceHoldCommandHandler>();
            return services;
        }
    }
}

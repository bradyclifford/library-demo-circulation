using System;
using Circulation.Domain;
using Circulation.Domain.PublicationCopies;
using Circulation.Domain.Publications;
using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.DependencyInjection;

namespace Circulation.Infrastructure
{
    public static class InfrastructureImplementationRegistrationExtension
    {
        public static IServiceCollection AddInfrastructureImplementations(this IServiceCollection services, string eventHubsNamespaceName)
        {
            string eventHubName = "circulation-events";

            services.AddSingleton(sp => CreateEventHubsClient(eventHubsNamespaceName, eventHubName));

            services.AddScoped<IPublicationRepository, PublicationRepository>();
            services.AddScoped<IPublicationCopyRepository, PublicationCopyRepository>();
            services.AddScoped<IEventPublisher, HubsEventPublisher>();
            services.AddScoped<IPublicationCopyIdGenerator, PublicationCopyIdGenerator>();
            return services;
        }

        private static EventHubClient CreateEventHubsClient(string eventHubConnectionString, string eventHubName)
        {
            // Creates an EventHubsConnectionStringBuilder object from the connection string, and sets the EntityPath.
            // Typically, the connection string should have the entity path in it, but this simple scenario
            // uses the connection string from the namespace.
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(eventHubConnectionString)
            {
                EntityPath = eventHubName
            };

            var eventHubClient = EventHubClient.Create(connectionStringBuilder);

/*
            var eventHubClient = EventHubClient.CreateWithManagedServiceIdentity(new Uri($"sb://{eventHubsNamespaceName}.servicebus.windows.net/"), eventHubName);
*/
            return eventHubClient;

        }
    }
}

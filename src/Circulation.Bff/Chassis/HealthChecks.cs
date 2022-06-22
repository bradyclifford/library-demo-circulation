using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

/*
 * NOTE: All of the code in this file can go away after upgrading the project to .NET Core 2.2 or later.
 */
namespace Circulation.Bff.Chassis
{
    public static class HealthChecksExtension
    {
        /// <summary>
        /// Provides a builder that will register health check classes in the DI container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to register the health checks with.</param>
        /// <returns>An instance of <see cref="IHealthChecksBuilder"/> from which health checks can be registered.</returns>
        public static IHealthChecksBuilder AddHealthChecks(this IServiceCollection services)
        {
            return new HealthChecksBuilder(services);
        }

        /// <summary>
        /// Adds a middleware that provides health check status.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="path">The path on which to provide health check status.</param>
        /// <returns>A reference to the <paramref name="app"/> after the operation has completed.</returns>
        /// <remarks>
        /// The signature of this method was copied directly from ASP.Net Core 2.2 at https://github.com/aspnet/AspNetCore/blob/release/2.2/src/Middleware/HealthChecks/src/Builder/HealthCheckApplicationBuilderExtensions.cs
        /// </remarks>
        public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder app, PathString path)
        {
            app.Map(path, builder => builder.UseMiddleware<HealthCheckMiddleware>());
            return app;
        }
    }

    /// <summary>
    /// Represents a health check, which can be used to check the status of a component in the application, such as a backend service, database or some internal
    /// state.
    /// </summary>
    /// <remarks>
    /// This interface copied directly from ASP.Net Core 2.2 at https://github.com/aspnet/Diagnostics/blob/release/2.2/src/Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions/IHealthCheck.cs
    /// </remarks>
    public interface IHealthCheck
    {
        /// <summary>
        /// Runs the health check, returning the status of the component being checked.
        /// </summary>
        /// <param name="context">A context object associated with the current execution.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the health check.</param>
        /// <returns>A <see cref="Task{HealthCheckResult}"/> that completes when the health check has finished, yielding the status of the component being checked.</returns>
        Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default);
    }

    public class HealthCheckContext
    {
        public HealthCheckContext(HealthCheckRegistration registration)
        {
            Registration = registration;
        }

        public HealthCheckRegistration Registration { get; }
    }

    /// <summary>
    /// Represents the result of a health check.
    /// </summary>
    /// <remarks>
    /// This class copied directly from ASP.Net Core 2.2 at https://github.com/aspnet/Diagnostics/blob/release/2.2/src/Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions/HealthCheckResult.cs
    /// </remarks>
    public struct HealthCheckResult
    {
        private static readonly IReadOnlyDictionary<string, object> _emptyReadOnlyDictionary = new Dictionary<string, object>();

        /// <summary>
        /// Creates a new <see cref="HealthCheckResult"/> with the specified values for <paramref name="status"/>,
        /// <paramref name="exception"/>, <paramref name="description"/>, and <paramref name="data"/>.
        /// </summary>
        /// <param name="status">A value indicating the status of the component that was checked.</param>
        /// <param name="description">A human-readable description of the status of the component that was checked.</param>
        /// <param name="exception">An <see cref="Exception"/> representing the exception that was thrown when checking for status (if any).</param>
        /// <param name="data">Additional key-value pairs describing the health of the component.</param>
        public HealthCheckResult(HealthStatus status, string description = null, Exception exception = null, IReadOnlyDictionary<string, object> data = null)
        {
            Status = status;
            Description = description;
            Exception = exception;
            Data = data ?? _emptyReadOnlyDictionary;
        }

        /// <summary>
        /// Gets additional key-value pairs describing the health of the component.
        /// </summary>
        public IReadOnlyDictionary<string, object> Data { get; }

        /// <summary>
        /// Gets a human-readable description of the status of the component that was checked.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets an <see cref="Exception"/> representing the exception that was thrown when checking for status (if any).
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Gets a value indicating the status of the component that was checked.
        /// </summary>
        public HealthStatus Status { get; }

        /// <summary>
        /// Creates a <see cref="HealthCheckResult"/> representing a healthy component.
        /// </summary>
        /// <param name="description">A human-readable description of the status of the component that was checked. Optional.</param>
        /// <param name="data">Additional key-value pairs describing the health of the component. Optional.</param>
        /// <returns>A <see cref="HealthCheckResult"/> representing a healthy component.</returns>
        public static HealthCheckResult Healthy(string description = null, IReadOnlyDictionary<string, object> data = null)
        {
            return new HealthCheckResult(status: HealthStatus.Healthy, description, exception: null, data);
        }

        /// <summary>
        /// Creates a <see cref="HealthCheckResult"/> representing a degraded component.
        /// </summary>
        /// <param name="description">A human-readable description of the status of the component that was checked. Optional.</param>
        /// <param name="exception">An <see cref="Exception"/> representing the exception that was thrown when checking for status. Optional.</param>
        /// <param name="data">Additional key-value pairs describing the health of the component. Optional.</param>
        /// <returns>A <see cref="HealthCheckResult"/> representing a degraged component.</returns>
        public static HealthCheckResult Degraded(string description = null, Exception exception = null, IReadOnlyDictionary<string, object> data = null)
        {
            return new HealthCheckResult(status: HealthStatus.Degraded, description, exception: null, data);
        }

        /// <summary>
        /// Creates a <see cref="HealthCheckResult"/> representing an unhealthy component.
        /// </summary>
        /// <param name="description">A human-readable description of the status of the component that was checked. Optional.</param>
        /// <param name="exception">An <see cref="Exception"/> representing the exception that was thrown when checking for status. Optional.</param>
        /// <param name="data">Additional key-value pairs describing the health of the component. Optional.</param>
        /// <returns>A <see cref="HealthCheckResult"/> representing an unhealthy component.</returns>
        public static HealthCheckResult Unhealthy(string description = null, Exception exception = null, IReadOnlyDictionary<string, object> data = null)
        {
            return new HealthCheckResult(status: HealthStatus.Unhealthy, description, exception, data);
        }
    }

    /// <summary>
    /// Represents the reported status of a health check result.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A status of <see cref="Unhealthy"/> should be considered the default value for a failing health check. Application
    /// developers may configure a health check to report a different status as desired.
    /// </para>
    /// <para>
    /// The values of this enum or ordered from least healthy to most healthy. So <see cref="HealthStatus.Degraded"/> is
    /// greater than <see cref="HealthStatus.Unhealthy"/> but less than <see cref="HealthStatus.Healthy"/>.
    /// </para>
    /// This enum copied directly from ASP.Net Core 2.2 at https://github.com/aspnet/Diagnostics/blob/release/2.2/src/Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions/HealthStatus.cs
    /// </remarks>
    public enum HealthStatus
    {
        /// <summary>
        /// Indicates that the health check determined that the component was unhealthy, or an unhandled
        /// exception was thrown while executing the health check.
        /// </summary>
        Unhealthy = 0,

        /// <summary>
        /// Indicates that the health check determined that the component was in a degraded state.
        /// </summary>
        Degraded = 1,

        /// <summary>
        /// Indicates that the health check determined that the component was healthy.
        /// </summary>
        Healthy = 2,
    }


    public class HealthCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HealthCheckMiddleware> _logger;
        private readonly IOptions<HealthChecksRegistry> _registry;
        private readonly IServiceScopeFactory _scopeFactory;

        public HealthCheckMiddleware(RequestDelegate next, ILogger<HealthCheckMiddleware> logger, IOptions<HealthChecksRegistry> registry, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _registry = registry;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            using (_logger.BeginScope("HealthCheck"))
            {
                var allResults = new List<HealthCheckResult>();
                using (var scope = _scopeFactory.CreateScope())
                {
                    foreach (HealthCheckRegistration registeredHealthCheck in _registry.Value.RegisteredHealthChecks)
                    {
                        httpContext.RequestAborted.ThrowIfCancellationRequested();

                        using (_logger.BeginScope(registeredHealthCheck.Name))
                        {
                            HealthCheckResult r;

                            try
                            {
                                // Resolve the HealthCheck object from a nested scope in case it has scoped or transient dependencies.
                                var healthCheck = registeredHealthCheck.Factory(scope.ServiceProvider);
                                r = await healthCheck.CheckHealthAsync(new HealthCheckContext(registeredHealthCheck),
                                    httpContext.RequestAborted);
                            }
                            catch (Exception e) when (!(e is OperationCanceledException))
                            {
                                r = new HealthCheckResult(registeredHealthCheck.FailureStatus, exception: e);
                            }

                            allResults.Add(r);
                        }
                    }
                }

                var healthStatus = CalculateAggregateStatus(allResults);

                var headers = httpContext.Response.Headers;
                headers[HeaderNames.CacheControl] = "no-store, no-cache";
                headers[HeaderNames.Pragma] = "no-cache";
                headers[HeaderNames.Expires] = "Thu, 01 Jan 1970 00:00:00 GMT";

                httpContext.Response.ContentType = "text/html";
                httpContext.Response.StatusCode = ResultStatusCodes[healthStatus];
                await httpContext.Response.WriteAsync(healthStatus.ToString());


            }
        }

        /// <summary>
        /// Gets a dictionary mapping the <see cref="HealthStatus"/> to an HTTP status code applied to the response.
        /// This property can be used to configure the status codes returned for each status.
        /// </summary>
        private IDictionary<HealthStatus, int> ResultStatusCodes { get; } = new Dictionary<HealthStatus, int>()
        {
            { HealthStatus.Healthy, StatusCodes.Status200OK },
            { HealthStatus.Degraded, StatusCodes.Status200OK },
            { HealthStatus.Unhealthy, StatusCodes.Status503ServiceUnavailable },
        };

        private HealthStatus CalculateAggregateStatus(IEnumerable<HealthCheckResult> entries)
        {
            // This is basically a Min() check, but we know the possible range, so we don't need to walk the whole list
            var currentValue = HealthStatus.Healthy;
            foreach (var entry in entries)
            {
                if (!Enum.IsDefined(typeof(HealthStatus), entry.Status))
                    throw new InvalidOperationException($"Unsupported {nameof(HealthStatus)} value {entry.Status}.");

                if (currentValue > entry.Status)
                {
                    currentValue = entry.Status;
                }

                if (currentValue == HealthStatus.Unhealthy)
                {
                    // Game over, man! Game over!
                    // (We hit the worst possible status, so there's no need to keep iterating)
                    return currentValue;
                }
            }

            return currentValue;
        }
    }

    public interface IHealthChecksBuilder
    {
        IHealthChecksBuilder AddCheck<T>(string name, HealthStatus? failureStatus = null) where T : class, IHealthCheck;
    }

    public class HealthChecksBuilder : IHealthChecksBuilder
    {
        private readonly IServiceCollection _services;

        public HealthChecksBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public IHealthChecksBuilder AddCheck<T>(string name, HealthStatus? failureStatus = null) where T : class, IHealthCheck
        {
            Func<IServiceProvider, IHealthCheck> factory = ActivatorUtilities.GetServiceOrCreateInstance<T>;
            // .Configure causes HealthChecksRegistry to have an effective Singleton lifetime scope (because
            // IOptionsManager<> is registered as singleton, so resolving IOptions<HealthChecksRegistry> returns
            // the same instance on every call.
            _services.Configure<HealthChecksRegistry>(reg => { reg.Add(name, factory, failureStatus); });
            return this;
        }
    }

    public class HealthChecksRegistry
    {
        private readonly List<HealthCheckRegistration> _all = new List<HealthCheckRegistration>();

        public IEnumerable<HealthCheckRegistration> RegisteredHealthChecks => _all.AsEnumerable();

        public void Add(string name, Func<IServiceProvider, IHealthCheck> factory, HealthStatus? failureStatus)
        {
            _all.Add(new HealthCheckRegistration(name, factory, failureStatus, null));
        }

        public struct Registration
        {

        }
    }

    /// <summary>
    /// Represent the registration information associated with an <see cref="IHealthCheck"/> implementation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The health check registration is provided as a separate object so that application developers can customize
    /// how health check implementations are configured.
    /// </para>
    /// <para>
    /// The registration is provided to an <see cref="IHealthCheck"/> implementation during execution through
    /// <see cref="HealthChecksRegistry.Registration"/>. This allows a health check implementation to access named
    /// options or perform other operations based on the registered name.
    /// </para>
    /// </remarks>
    public sealed class HealthCheckRegistration
    {
        private Func<IServiceProvider, IHealthCheck> _factory;
        private string _name;

        /// <summary>
        /// Creates a new <see cref="HealthCheckRegistration"/> for an existing <see cref="IHealthCheck"/> instance.
        /// </summary>
        /// <param name="name">The health check name.</param>
        /// <param name="instance">The <see cref="IHealthCheck"/> instance.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported upon failure of the health check. If the provided value
        /// is <c>null</c>, then <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used for filtering health checks.</param>
        public HealthCheckRegistration(string name, IHealthCheck instance, HealthStatus? failureStatus, IEnumerable<string> tags)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            Name = name;
            FailureStatus = failureStatus ?? HealthStatus.Unhealthy;
            Tags = new HashSet<string>(tags ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            Factory = (_) => instance;
        }

        /// <summary>
        /// Creates a new <see cref="HealthCheckRegistration"/> for an existing <see cref="IHealthCheck"/> instance.
        /// </summary>
        /// <param name="name">The health check name.</param>
        /// <param name="factory">A delegate used to create the <see cref="IHealthCheck"/> instance.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check reports a failure. If the provided value
        /// is <c>null</c>, then <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used for filtering health checks.</param>
        public HealthCheckRegistration(
            string name,
            Func<IServiceProvider, IHealthCheck> factory,
            HealthStatus? failureStatus,
            IEnumerable<string> tags)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            Name = name;
            FailureStatus = failureStatus ?? HealthStatus.Unhealthy;
            Tags = new HashSet<string>(tags ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            Factory = factory;
        }

        /// <summary>
        /// Gets or sets a delegate used to create the <see cref="IHealthCheck"/> instance.
        /// </summary>
        public Func<IServiceProvider, IHealthCheck> Factory
        {
            get => _factory;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _factory = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="HealthStatus"/> that should be reported upon failure of the health check.
        /// </summary>
        public HealthStatus FailureStatus { get; set; }

        /// <summary>
        /// Gets or sets the health check name.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _name = value;
            }
        }

        /// <summary>
        /// Gets a list of tags that can be used for filtering health checks.
        /// </summary>
        public ISet<string> Tags { get; }
    }
}

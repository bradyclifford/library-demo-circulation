using System.Threading;
using System.Threading.Tasks;
using Circulation.Bff.Chassis;

namespace Circulation.Bff.HealthChecks
{
    //TODO: replace this health check with more meaningful ones.
    public class LivenessCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            // If this code is even reached, we can say we're healthy.
            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
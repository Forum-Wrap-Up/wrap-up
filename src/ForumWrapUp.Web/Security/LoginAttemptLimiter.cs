using System.Threading.RateLimiting;

namespace ForumWrapUp.Web.Security;

public sealed class LoginAttemptLimiter : IDisposable
{
    private readonly PartitionedRateLimiter<string> _limiter;

    public LoginAttemptLimiter()
    {
        _limiter = PartitionedRateLimiter.Create<string, string>(address =>
            RateLimitPartition.GetFixedWindowLimiter(address, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    }

    public RateLimitLease Acquire(HttpContext context)
    {
        string address = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return _limiter.AttemptAcquire(address);
    }

    public void Dispose()
    {
        _limiter.Dispose();
    }
}

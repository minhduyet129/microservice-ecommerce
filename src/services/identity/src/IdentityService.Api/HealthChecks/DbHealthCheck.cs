using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace IdentityService.Api.HealthChecks;

public class SqlServerHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public SqlServerHealthCheck(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            return HealthCheckResult.Healthy("PostgreSQL is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL is unhealthy", ex);
        }
    }
}
# 🚀 DEPLOYMENT GUIDE - HEALTH CHECKS & PRODUCTION SETUP

## 📋 TABLE OF CONTENTS
1. [Health Check Best Practices](#health-check-best-practices)
2. [Security Considerations](#security-considerations)
3. [Production Deployment](#production-deployment)
4. [Kubernetes Configuration](#kubernetes-configuration)
5. [Monitoring Integration](#monitoring-integration)
6. [Troubleshooting](#troubleshooting)

---

## 🏥 HEALTH CHECK BEST PRACTICES

### 1. Three Types of Health Checks

| Type | Purpose | Use Case | K8s Probe |
|------|---------|----------|-----------|
| **Liveness** | Service is alive? | Restart if fail | `livenessProbe` |
| **Readiness** | Ready for traffic? | Stop routing if fail | `readinessProbe` |
| **Startup** | Service started? | Delay startup probe | `startupProbe` |

### 2. Implementation Pattern

```csharp
// Program.cs - Add to all services

// 1. Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Service is running"))
    .AddDbContextCheck<AppDbContext>("database")
    .AddCheck("redis", () => CheckRedis())
    .AddCheck("external-api", () => CheckExternalApi());

// 2. Map endpoints
var app = builder.Build();

// Simple health check (for load balancer)
app.MapHealthChecks("/health");

// Liveness probe (K8s - restart if fail)
app.MapHealthChecks("/health/live")
    .WithPredicate(healthCheck => healthCheck.Tags.Contains("liveness"));

// Readiness probe (K8s - stop routing if fail)
app.MapHealthChecks("/health/ready")
    .WithPredicate(healthCheck => healthCheck.Tags.Contains("readiness"));

// Startup probe (K8s - delay until ready)
app.MapHealthChecks("/health/startup")
    .WithPredicate(healthCheck => healthCheck.Tags.Contains("startup"));
```

### 3. Health Check Response Format

```csharp
// Enable detailed response
builder.Services.AddHealthChecks()
    .AddCheck("database", () => CheckDatabase())
    .AddCheck("redis", () => CheckRedis())
    .AddCheck("external-api", () => CheckExternalApi());

// Response:
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "database": {
      "data": {},
      "status": "Healthy",
      "duration": "00:00:00.0123456"
    },
    "redis": {
      "data": {},
      "status": "Healthy",
      "duration": "00:00:00.0045678"
    }
  }
}
```

### 4. Custom Health Checks

```csharp
// Custom health check for external dependencies
public class ExternalApiHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public ExternalApiHealthCheck(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(_config["ExternalApi:HealthUrl"], cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("External API is responding");
            }
            return HealthCheckResult.Degraded("External API is not responding correctly");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("External API is not available", ex);
        }
    }
}

// Register in Program.cs
builder.Services.AddHealthChecks()
    .AddCheck<ExternalApiHealthCheck>("external-api");
```

---

## 🔒 SECURITY CONSIDERATIONS

### 1. Health Check Endpoint Security

| Environment | Strategy | Implementation |
|-------------|----------|----------------|
| **Development** | Public access | No authentication |
| **Staging** | IP whitelist | Middleware filter |
| **Production** | Auth + IP whitelist | RequireAuthorization |

### 2. Implementation Options

```csharp
// Option 1: Restrict to localhost only (Development)
app.MapHealthChecks("/health").RequireHost("localhost", "127.0.0.1");

// Option 2: IP whitelist (Staging)
app.MapHealthChecks("/health").UseMiddleware<IpWhitelistMiddleware>();

// Option 3: Require authentication (Production)
app.MapHealthChecks("/health").RequireAuthorization();

// Option 4: Custom middleware
app.MapHealthChecks("/health").Use(async (context, next) =>
{
    var clientIp = context.Connection.RemoteIpAddress?.ToString();
    var allowedIps = _config["HealthCheck:AllowedIps"].Split(',');
    
    if (!allowedIps.Contains(clientIp))
    {
        context.Response.StatusCode = 403;
        return;
    }
    
    await next();
});
```

### 3. Health Check Configuration

```json
// appsettings.json
{
  "HealthCheck": {
    "AllowedIps": "127.0.0.1,10.0.0.0/8,192.168.0.0/16",
    "EnableDetailedResponse": true,
    "CacheDuration": "00:00:30"
  }
}
```

### 4. Rate Limiting Health Checks

```csharp
// Prevent health check abuse
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("HealthCheckPolicy", policy =>
        policy.AddSlidingWindowLimiter(partitionName: "health-check",
            permitLimit: 100,
            window: TimeSpan.FromSeconds(60)));
});

app.MapHealthChecks("/health").RequireRateLimiting("HealthCheckPolicy");
```

---

## 🚀 PRODUCTION DEPLOYMENT

### 1. Environment-Specific Configuration

```csharp
// appsettings.Development.json
{
  "HealthCheck": {
    "AllowedIps": "*",
    "EnableDetailedResponse": true
  }
}

// appsettings.Production.json
{
  "HealthCheck": {
    "AllowedIps": "127.0.0.1,10.0.0.0/8",
    "EnableDetailedResponse": false
  }
}
```

### 2. Docker Health Check

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out
COPY --from=build /app/out .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:5001/health || exit 1

ENTRYPOINT ["dotnet", "IdentityService.Api.dll"]
```

### 3. Docker Compose Health Check

```yaml
# docker-compose.yml
services:
  identity:
    image: ecommerce-identity:latest
    ports:
      - "5001:5001"
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5001/health"]
      interval: 30s
      timeout: 3s
      retries: 3
      start_period: 40s
    depends_on:
      postgres:
        condition: service_healthy
```

### 4. Load Balancer Health Check

```nginx
# nginx.conf
upstream identity {
    server identity-1:5001 max_fails=3 fail_timeout=30s;
    server identity-2:5001 max_fails=3 fail_timeout=30s;
    server identity-3:5001 max_fails=3 fail_timeout=30s;
}

server {
    location /health {
        access_log off;
        proxy_pass http://identity/health;
    }
}
```

---

## ☸️ KUBERNETES CONFIGURATION

### 1. Deployment with Health Checks

```yaml
# identity-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: identity-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: identity-service
  template:
    metadata:
      labels:
        app: identity-service
    spec:
      containers:
      - name: identity-service
        image: ecommerce-identity:latest
        ports:
        - containerPort: 5001
        # Liveness probe - restart if fail
        livenessProbe:
          httpGet:
            path: /health/live
            port: 5001
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        # Readiness probe - stop routing if fail
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 5001
          initialDelaySeconds: 5
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 3
        # Startup probe - delay until ready
        startupProbe:
          httpGet:
            path: /health/startup
            port: 5001
          initialDelaySeconds: 0
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 30
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
```

### 2. Service with Health Check

```yaml
# identity-service.yaml
apiVersion: v1
kind: Service
metadata:
  name: identity-service
spec:
  selector:
    app: identity-service
  ports:
  - port: 80
    targetPort: 5001
    name: http
  type: ClusterIP
```

### 3. HorizontalPodAutoscaler

```yaml
# identity-hpa.yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: identity-service-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: identity-service
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

### 4. PodDisruptionBudget

```yaml
# identity-pdb.yaml
apiVersion: policy/v1
kind: PodDisruptionBudget
metadata:
  name: identity-service-pdb
spec:
  minAvailable: 2
  maxUnavailable: 1
```

---

## 📊 MONITORING INTEGRATION

### 1. Prometheus Health Check Exporter

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddPrometheusExporter(options =>
    {
        options.ScrapeResponseCacheDuration = TimeSpan.FromSeconds(30);
        options.ScrapeResponseCacheDuration = TimeSpan.FromSeconds(30);
    });

// Map endpoint
app.MapHealthChecks("/health");
app.MapPrometheusScrapingEndpoint();
```

### 2. OpenTelemetry Integration

```csharp
// Program.cs
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
        tracerProviderBuilder
            .AddAspNetCoreInstrumentation()
            .AddSource("IdentityService"))
    .WithMetrics(metricsProviderBuilder =>
        metricsProviderBuilder
            .AddAspNetCoreInstrumentation()
            .AddMeter("Microsoft.AspNetCore.Hosting")
            .AddMeter("Microsoft.AspNetCore.Http.Kestrel")
            .AddMeter("Microsoft.AspNetCore.RateLimiting"));

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Service is running"))
    .AddCheck("database", () => CheckDatabase())
    .AddCheck("redis", () => CheckRedis());
```

### 3. Grafana Dashboard

```json
{
  "dashboard": {
    "title": "Service Health",
    "panels": [
      {
        "title": "Service Status",
        "targets": [
          {
            "expr": "up{job=\"identity-service\"}",
            "legendFormat": "{{job}}"
          },
          {
            "expr": "up{job=\"product-service\"}",
            "legendFormat": "{{job}}"
          },
          {
            "expr": "up{job=\"order-service\"}",
            "legendFormat": "{{job}}"
          }
        ]
      },
      {
        "title": "Response Time",
        "targets": [
          {
            "expr": "http_request_duration_seconds{job=\"identity-service\"}",
            "legendFormat": "{{job}}"
          }
        ]
      }
    ]
  }
}
```

### 4. Alerting Rules

```yaml
# prometheus-alerts.yaml
groups:
  - name: service-health
    rules:
      - alert: ServiceDown
        expr: up{job=~".*-service"} == 0
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "Service {{ $labels.job }} is down"
          description: "Service {{ $labels.job }} has been down for more than 1 minute"

      - alert: HighErrorRate
        expr: rate(http_requests_total{status=~"5.."}[5m]) > 0.1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High error rate detected"
          description: "Error rate is {{ $value | humanizePercentage }} for {{ $labels.job }}"

      - alert: HighLatency
        expr: histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])) > 1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High latency detected"
          description: "95th percentile latency is {{ $value }}s for {{ $labels.job }}"
```

---

## 🔧 TROUBLESHOOTING

### 1. Health Check Failing

```bash
# Check service logs
kubectl logs -l app=identity-service --tail=100

# Check pod status
kubectl get pods -l app=identity-service

# Describe pod
kubectl describe pod <pod-name>

# Check events
kubectl get events --sort-by='.lastTimestamp'

# Test health check locally
curl http://localhost:5001/health
curl http://localhost:5001/health/live
curl http://localhost:5001/health/ready
```

### 2. Database Connection Issues

```csharp
// Add detailed logging to health check
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database", options =>
    {
        options.AddTest("SELECT 1");  // Simple query
        options.AddTest("SELECT COUNT(*) FROM \"Users\"");  // More specific
    });
```

### 3. External Dependency Issues

```csharp
// Add timeout to external health checks
builder.Services.AddHealthChecks()
    .AddCheck("external-api", async () =>
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        try
        {
            var response = await _httpClient.GetAsync(_config["ExternalApi:HealthUrl"], cts.Token);
            return response.IsSuccessStatusCode 
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Degraded();
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Degraded("Timeout");
        }
    });
```

### 4. Health Check Too Slow

```csharp
// Add caching to health checks
builder.Services.AddHealthChecks()
    .AddCheck("database", () => CheckDatabase())
    .AddCheck("redis", () => CheckRedis())
    .AddCheck("external-api", () => CheckExternalApi())
    .AddCheck("cache", () => CheckCache());

// Cache health check results
builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage()
    .AddCheck("database", () => CheckDatabase())
    .AddCheck("redis", () => CheckRedis())
    .AddCheck("external-api", () => CheckExternalApi())
    .AddCheck("cache", () => CheckCache());
```

### 5. Health Check Not Exposed

```csharp
// Make sure to map health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/startup");

// Verify with curl
curl http://localhost:5001/health
curl http://localhost:5001/health/live
curl http://localhost:5001/health/ready
```

---

## 📋 CHECKLIST

### Pre-Deployment
- [ ] Add health checks to all services
- [ ] Map health check endpoints
- [ ] Add liveness/readiness probes
- [ ] Configure health check security
- [ ] Test health checks locally
- [ ] Add Prometheus exporter
- [ ] Configure alerting rules

### Deployment
- [ ] Update Dockerfile with health check
- [ ] Update docker-compose.yml with health checks
- [ ] Create Kubernetes manifests
- [ ] Configure HPA
- [ ] Configure PDB
- [ ] Set up monitoring
- [ ] Configure alerting

### Post-Deployment
- [ ] Verify health checks in K8s
- [ ] Check Prometheus metrics
- [ ] Verify Grafana dashboards
- [ ] Test alerting rules
- [ ] Monitor pod restarts
- [ ] Check resource usage

---

## 📚 REFERENCES

- [ASP.NET Core Health Checks](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Kubernetes Probes](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/)
- [Prometheus Health Checks](https://prometheus.io/docs/prometheus/latest/configuration/reporting/)
- [Docker Health Checks](https://docs.docker.com/engine/reference/builder/#healthcheck)

---

**Last Updated:** 2026-05-09
**Version:** 1.0

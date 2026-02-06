using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DevOpsApp
{
    /// <summary>
    /// DevOps Learning Application for Alibaba Function Compute
    /// Implements HTTP server without external SDK dependencies
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Initialize logging
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            var logger = loggerFactory.CreateLogger("DevOpsApp");

            logger.LogInformation("================================================");
            logger.LogInformation("DevOps Learning Application Starting");
            logger.LogInformation("================================================");
            logger.LogInformation($"Runtime: .NET {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
            logger.LogInformation($"Environment: {Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "development"}");
            logger.LogInformation($"Log Level: {Environment.GetEnvironmentVariable("LOG_LEVEL") ?? "Information"}");

            // Get port from environment, default to 9000 for Function Compute
            string port = Environment.GetEnvironmentVariable("FC_SERVER_PORT") ?? "9000";
            
            // Create and start HTTP server
            var server = new HttpServer(port, logger);
            
            logger.LogInformation($"Listening on port {port}...");
            logger.LogInformation("================================================");
            logger.LogInformation("Available endpoints:");
            logger.LogInformation("  GET /              - Service overview");
            logger.LogInformation("  GET /health        - Health check");
            logger.LogInformation("  GET /metrics       - Performance metrics");
            logger.LogInformation("  GET /config        - Configuration info");
            logger.LogInformation("  GET /deploy        - Deployment guide");
            logger.LogInformation("  GET /info          - System information");
            logger.LogInformation("  GET /logs          - Logging guidance");
            logger.LogInformation("  POST /invoke       - Function Compute invocation endpoint");
            logger.LogInformation("================================================");

            try
            {
                await server.StartAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fatal error");
                Environment.Exit(1);
            }
        }
    }

    /// <summary>
    /// Simple HTTP server implementation for Function Compute
    /// </summary>
    public class HttpServer
    {
        private readonly HttpListener _listener;
        private readonly ILogger _logger;
        private bool _running = true;
        private readonly Dictionary<string, long> _invocationMetrics = new();
        private readonly object _metricsLock = new();
        private readonly DateTime _startTime = DateTime.UtcNow;

        public HttpServer(string port, ILogger logger)
        {
            _logger = logger;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://+:{port}/");
        }

        public async Task StartAsync()
        {
            _listener.Start();
            _logger.LogInformation("HTTP listener started");

            while (_running)
            {
                try
                {
                    HttpListenerContext context = await _listener.GetContextAsync();
                    _ = HandleRequestAsync(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in request loop");
                }
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            try
            {
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                string path = request.Url?.AbsolutePath ?? "/";
                _logger.LogInformation($"[{DateTime.UtcNow:O}] {request.HttpMethod} {path} (Query: {request.Url?.Query})");
                
                // Debug: Log the exact path being called
                _logger.LogInformation($"DEBUG: Path = '{path}', AbsolutePath = '{request.Url?.AbsolutePath}', PathAndQuery = '{request.Url?.PathAndQuery}'");

                string body = "";
                int statusCode = 200;

                // Route to appropriate handler - handle both exact paths and with trailing slashes
                path = path.TrimEnd('/');
                if (string.IsNullOrEmpty(path)) path = "/";

                (body, statusCode) = path switch
                {
                    "/" => HandleRoot(),
                    "/health" => HandleHealth(),
                    "/metrics" => HandleMetrics(),
                    "/config" => HandleConfig(),
                    "/deploy" => HandleDeploy(),
                    "/info" => HandleInfo(),
                    "/logs" => HandleLogs(),
                    "/invoke" => HandleRoot(),
                    _ => (JsonSerializer.Serialize(new { error = $"Endpoint not found: {path}", debug_path = path }), 404)
                };

                // Set response
                response.StatusCode = statusCode;
                response.ContentType = "application/json; charset=utf-8";
                response.ContentEncoding = System.Text.Encoding.UTF8;
                
                // Add required headers
                response.AddHeader("Date", DateTime.UtcNow.ToString("R"));
                response.AddHeader("Content-Type", "application/json; charset=utf-8");

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(body);
                response.ContentLength64 = buffer.Length;

                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling request");
            }
        }

        private (string, int) HandleRoot()
        {
            RecordMetric("root");
            
            var response = new
            {
                message = "DevOps Learning Application",
                version = "1.0.0",
                framework = ".NET 8.0",
                timestamp = DateTime.UtcNow,
                endpoints = new
                {
                    root = "/ - Service overview",
                    health = "/health - Service health check",
                    metrics = "/metrics - Performance metrics",
                    config = "/config - Configuration info",
                    deploy = "/deploy - Deployment information",
                    info = "/info - System information",
                    logs = "/logs - Recent logs"
                }
            };

            return (JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }), 200);
        }

        private (string, int) HandleHealth()
        {
            RecordMetric("health");

            var health = new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                uptime = GetUptime(),
                checks = new
                {
                    memory = GC.GetTotalMemory(false) < 500_000_000,
                    disk = CheckDiskSpace(),
                    runtime = "operational"
                }
            };

            return (JsonSerializer.Serialize(health), 200);
        }

        private (string, int) HandleMetrics()
        {
            lock (_metricsLock)
            {
                var metrics = new
                {
                    invocations = _invocationMetrics,
                    memory = new
                    {
                        total_mb = GC.GetTotalMemory(false) / 1024 / 1024,
                        collections_gen0 = GC.CollectionCount(0),
                        collections_gen1 = GC.CollectionCount(1),
                        collections_gen2 = GC.CollectionCount(2)
                    },
                    processor_count = Environment.ProcessorCount,
                    timestamp = DateTime.UtcNow
                };

                return (JsonSerializer.Serialize(metrics), 200);
            }
        }

        private (string, int) HandleConfig()
        {
            RecordMetric("config");

            var config = new
            {
                environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "development",
                runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                function_name = Environment.GetEnvironmentVariable("FC_FUNCTION_NAME") ?? "local-development",
                function_version = Environment.GetEnvironmentVariable("FC_FUNCTION_VERSION") ?? "1.0.0",
                request_id = Environment.GetEnvironmentVariable("FC_REQUEST_ID") ?? "local-request",
                memory_limit = Environment.GetEnvironmentVariable("FC_MEMORY_SIZE") ?? "512",
                timeout = Environment.GetEnvironmentVariable("FC_TIMEOUT") ?? "60",
                instance_id = Environment.GetEnvironmentVariable("FC_INSTANCE_ID") ?? "local-instance",
                processor_count = Environment.ProcessorCount,
                os_description = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                process_id = Environment.ProcessId
            };

            return (JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }), 200);
        }

        private (string, int) HandleDeploy()
        {
            RecordMetric("deploy");

            var deployInfo = new
            {
                title = "DevOps Deployment Guide for Alibaba Function Compute",
                runtime = ".NET 8.0 (LTS)",
                deployment_steps = new[]
                {
                    "1. Update project to .NET 8.0 (LTS supported version)",
                    "2. Publish as self-contained: dotnet publish -c Release -r linux-x64",
                    "3. Package function.zip with all dependencies",
                    "4. Upload to Function Compute",
                    "5. Configure HTTP trigger on port 9000",
                    "6. Set environment variables",
                    "7. Deploy and test"
                },
                best_practices = new
                {
                    framework_version = "Use .NET 8.0 LTS for long-term support",
                    cold_start = "Initialize static resources outside request handler",
                    stateless = "Design functions to be stateless and idempotent",
                    http_server = "Implement HTTP server listening on FC_SERVER_PORT (9000)",
                    error_handling = "Comprehensive exception handling and logging",
                    monitoring = "Use /health and /metrics endpoints for observability",
                    security = "Never hardcode secrets - use environment variables"
                },
                quick_commands = new
                {
                    build = "dotnet build -c Release",
                    publish = "dotnet publish -c Release -r linux-x64 -o ./publish",
                    package = "zip -r function.zip .",
                    deploy = "fc3 function update --service-name devops-learning --function-name learning-app --zip-file fileb://./function.zip"
                }
            };

            return (JsonSerializer.Serialize(deployInfo, new JsonSerializerOptions { WriteIndented = true }), 200);
        }

        private (string, int) HandleInfo()
        {
            RecordMetric("info");

            var info = new
            {
                application = "DevOps Learning Application",
                version = "1.0.0",
                framework = ".NET 8.0 LTS",
                build_time = new DateTime(2025, 11, 17),
                platform = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                processor_architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture,
                culture = System.Globalization.CultureInfo.CurrentCulture.Name,
                timezone = TimeZoneInfo.Local.DisplayName,
                started_at = _startTime,
                running_since_seconds = (DateTime.UtcNow - _startTime).TotalSeconds
            };

            return (JsonSerializer.Serialize(info), 200);
        }

        private (string, int) HandleLogs()
        {
            RecordMetric("logs");

            var logs = new
            {
                message = "Logs are available in Alibaba Function Compute log service",
                retrieval = new
                {
                    method = "Use fc3 logs command or Alibaba Cloud Console",
                    command = "fc3 logs get --service-name devops-learning --function-name learning-app",
                    documentation = "https://www.alibabacloud.com/help/en/function-compute"
                },
                best_practices = new[]
                {
                    "Use structured logging with correlation IDs",
                    "Log at appropriate levels (Debug, Info, Warning, Error)",
                    "Include request IDs in all log entries",
                    "Avoid logging sensitive information",
                    "Use Alibaba Logs service for log aggregation",
                    "Monitor error rates and latency trends"
                }
            };

            return (JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true }), 200);
        }

        // ========== Helper Methods ==========

        private void RecordMetric(string endpoint)
        {
            lock (_metricsLock)
            {
                if (_invocationMetrics.ContainsKey(endpoint))
                    _invocationMetrics[endpoint]++;
                else
                    _invocationMetrics[endpoint] = 1;
            }
        }

        private string GetUptime()
        {
            var uptime = DateTime.UtcNow - _startTime;
            return $"{uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
        }

        private bool CheckDiskSpace()
        {
            try
            {
                var drive = System.IO.DriveInfo.GetDrives().FirstOrDefault(d => d.Name == "/");
                return drive?.AvailableFreeSpace > 10_000_000;
            }
            catch
            {
                return true;
            }
        }
    }
}

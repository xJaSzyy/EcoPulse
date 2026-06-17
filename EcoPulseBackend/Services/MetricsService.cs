using Prometheus;

namespace EcoPulseBackend.Services;

public static class MetricsService
{
    public static readonly Counter HttpRequestsTotal =
        Metrics.CreateCounter("http_requests_total", "Total HTTP Requests",
            new CounterConfiguration
            {
                LabelNames = ["method", "endpoint"]
            });

    public static readonly Histogram HttpRequestDuration =
        Metrics.CreateHistogram(
            "http_request_duration_seconds",
            "HTTP Request Duration",
            new HistogramConfiguration
            {
                LabelNames = ["method", "endpoint"],
                Buckets = Histogram.ExponentialBuckets(0.01, 2, 10)
            });
}
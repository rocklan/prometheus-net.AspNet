# prometheus-net.WebApi
A library for exposing prometheus metrics with WebApi, running on full framework dot net 4.5 and above.

# Installation

Add the packge from [nuget](https://www.nuget.org/packages/prometheus-net.WebApi):
>Install-Package: [prometheus-net.WebApi](https://www.nuget.org/packages/prometheus-net.WebApi)

# Usage

To publish your metrics, call `UseMetricsServer` inside your WebApiConfig.Register method, and specify the endpoint. The following code will expose http://localhost/metrics  

```csharp
public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
    {
        config.MapHttpAttributeRoutes();

        PrometheusConfig.UseMetricsServer(config, "metrics");
    }
}
```



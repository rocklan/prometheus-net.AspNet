# prometheus-net.WebApi
A library for exposing prometheus metrics with WebApi, running on full framework dot net 4.5 and above. Basic Auth can also be enabled for the endpoint.

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
        ...

        PrometheusConfig.UseMetricsServer(config, "metrics");
    }
}
```


If you wish to enable Basic Auth protection for your endpoint, pass through the basic auth username and password when calling `UseMetricsServer`:
```csharp
public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
    {
        ...

        PrometheusConfig.UseMetricsServer(config, "metrics", "BasicAuthUsername", "BasicAuthPassword");
    }
}
```

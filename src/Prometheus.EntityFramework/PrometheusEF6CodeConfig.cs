using System.Data.Entity;

namespace Prometheus.EntityFramework
{
    public class PrometheusEF6CodeConfig : DbConfiguration
    {
        public PrometheusEF6CodeConfig()
        {
            this.AddInterceptor(new PrometheusEFLoggerForMetrics());
        }
    }
}

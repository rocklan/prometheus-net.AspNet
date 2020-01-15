using System.Data.Entity;

namespace Prometheus.EF
{
    /// <summary>
    /// Configuration class to add prometheus metrics to your DbContext. To use, add [DbConfigurationType(typeof(PrometheusEF6CodeConfig))] 
    /// to your DbContext
    /// </summary>
    public class PrometheusEF6CodeConfig : DbConfiguration
    {
        /// <summary>
        /// ctor
        /// </summary>
        public PrometheusEF6CodeConfig()
        {
            this.AddInterceptor(new PrometheusEFLoggerForMetrics());
        }
    }
}

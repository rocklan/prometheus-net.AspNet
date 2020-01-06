using System;

using System.Threading.Tasks;
using System.Net;

using System.Web.Http;
using System.Net.Http;

using System.Text;

using Prometheus;
using System.IO;


namespace Prometheus.WebApi
{
    /// <summary>
    /// Class for serving up prometheus metrics
    /// </summary>
    public class PrometheusMetricsController : ApiController
    {
        internal static string BasicAuthUsername { get; set; }
        internal static string BasicAuthPassword { get; set; }
        internal static bool UseBasicAuth { get; set; }

        /// <summary>
        /// Returns prometheus metrics
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<HttpResponseMessage> AppMetrics()
        {
            if (UseBasicAuth && !BasicAuthIsOk())
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized };

            using (MemoryStream ms = new MemoryStream())
            {
                await Metrics.DefaultRegistry.CollectAndExportAsTextAsync(ms);
                ms.Position = 0;

                using (StreamReader sr = new StreamReader(ms))
                {
                    var allmetrics = await sr.ReadToEndAsync();

                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(allmetrics, Encoding.UTF8, "text/plain")
                    };
                }
            }
        }

        private bool BasicAuthIsOk()
        {
            if (Request.Headers.Authorization != null &&
                string.Equals(Request.Headers.Authorization.Scheme, "Basic"))
            {
                var authHeaderParam = Request.Headers.Authorization.Parameter;
                Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                string usernamePassword = null;
                try
                {
                    usernamePassword = encoding.GetString(Convert.FromBase64String(authHeaderParam));
                }
                catch (Exception)
                {
                    return false;
                }

                if (usernamePassword == null || !usernamePassword.Contains(":"))
                    return false;

                int seperatorIndex = usernamePassword.IndexOf(':');
                string username = usernamePassword.Substring(0, seperatorIndex);
                string password = usernamePassword.Substring(seperatorIndex + 1);

                if (string.Equals(username, BasicAuthUsername) && string.Equals(password, BasicAuthPassword))
                    return true;
            }

            return false;
        }

    }
}

using System;
using System.Diagnostics;
using System.Web;

namespace Prometheus.WebApi
{
    /// <summary>
    /// Middleware for tracking basic HTTP request info
    /// </summary>
    public class PrometheusHttpRequestModule : IHttpModule
    {
        private static readonly Counter _globalExceptions = Metrics
          .CreateCounter("global_exceptions", "Number of global exceptions.");

        private static readonly Gauge _httpRequestsInProgress = Metrics
            .CreateGauge("http_requests_in_progress", "The number of HTTP requests currently in progress");

        private static readonly Gauge _httpRequestsTotal = Metrics
            .CreateGauge("http_requests_received_total", "Provides the count of HTTP requests that have been processed by this app ");

        private static readonly Histogram _httpRequestsDuration = Metrics
            .CreateHistogram("http_request_duration_seconds", "The duration of HTTP requests processed by this app.");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpApp"></param>
        public void Init(HttpApplication httpApp)
        {
            httpApp.BeginRequest += OnBeginRequest;
            httpApp.EndRequest += OnEndRequest;
            httpApp.Error += HttpApp_Error;
        }

        private void HttpApp_Error(object sender, EventArgs e)
        {
            _globalExceptions.Inc();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // Record the time of the begin request event.
        public void OnBeginRequest(Object sender, EventArgs e)
        {
            var httpApp = (HttpApplication)sender;
            var timer = new Stopwatch();
            httpApp.Context.Items["Timer"] = timer;
            timer.Start();

            _httpRequestsInProgress.Inc();
            _httpRequestsTotal.Inc();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnEndRequest(Object sender, EventArgs e)
        {
            _httpRequestsInProgress.Dec();

            var httpApp = (HttpApplication)sender;
            var timer = (Stopwatch)httpApp.Context.Items["Timer"];

            if (timer != null)
            {
                timer.Stop();
                var timeTakenMs = ((double)timer.ElapsedTicks / Stopwatch.Frequency) * 1000;
                _httpRequestsDuration.Observe(timeTakenMs);
            }

            httpApp.Context.Items.Remove("Timer");
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose() { /* Not needed */ }
    }
}

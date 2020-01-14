using System;
using System.Diagnostics;
using System.Web;



namespace Prometheus.AspNet
{
    /// <summary>
    /// Library for monitoring HTTP requests
    /// </summary>
    public class PerformanceMonitorModule : IHttpModule
    {
        private static readonly Counter _globalExceptions = Metrics
          .CreateCounter("global_exceptions", "Number of global exceptions.");

        private static readonly Gauge _httpRequestsInProgress = Metrics
            .CreateGauge("http_requests_in_progress", "The number of HTTP requests currently in progress");

        private static readonly Gauge _httpRequestsTotal = Metrics
            .CreateGauge("http_requests_received_total", "Provides the count of HTTP requests that have been processed by this app ");

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
            throw new NotImplementedException();
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnEndRequest(Object sender, EventArgs e)
        {
            var httpApp = (HttpApplication)sender;
            var timer = (Stopwatch)httpApp.Context.Items["Timer"];

            if (timer != null)
            {
                timer.Stop();
                var timeTakenMs = ((double)timer.ElapsedTicks / Stopwatch.Frequency) * 1000;

            }

            httpApp.Context.Items.Remove("Timer");
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose() { /* Not needed */ }
    }
}

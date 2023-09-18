using System;
using System.Diagnostics;
using System.Web;

namespace Prometheus.AspNet
{
    /// <summary>
    /// Middleware for tracking basic HTTP request info
    /// </summary>
    public class PrometheusHttpRequestModule : IHttpModule
    {
        private static Counter _globalExceptions;
        private static Gauge _httpRequestsInProgress;
        private static Gauge _httpRequestsTotal;
        private static Histogram _httpRequestsDuration;
        private const string _timerKey = "PrometheusHttpRequestModule.Timer";

        /// <summary>
        ///
        /// </summary>
        public PrometheusHttpRequestModule()
        {
            if (_globalExceptions == null)
            {
                _globalExceptions = Metrics
                    .CreateCounter("global_exceptions", "Number of global exceptions.");
            }

            if (_httpRequestsInProgress == null)
            {
                _httpRequestsInProgress = Metrics
                    .CreateGauge("http_requests_in_progress", "The number of HTTP requests currently in progress");
            }

            if (_httpRequestsTotal == null)
            {
                _httpRequestsTotal = Metrics
                    .CreateGauge("http_requests_received_total", "Provides the count of HTTP requests that have been processed by this app",
                        new GaugeConfiguration {LabelNames = new[] {"code", "method", "controller", "action"}});
            }

            if (_httpRequestsDuration == null)
            {
                _httpRequestsDuration = Metrics
                    .CreateHistogram("http_request_duration_seconds", "The duration of HTTP requests processed by this app.",
                        new HistogramConfiguration {LabelNames = new[] {"code", "method", "controller", "action"}});
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bucektSize"></param>
        public PrometheusHttpRequestModule(double[] bucektSize)
        {
            if (_globalExceptions == null)
            {
                _globalExceptions = Metrics
                    .CreateCounter("global_exceptions", "Number of global exceptions.");
            }

            if (_httpRequestsInProgress == null)
            {
                _httpRequestsInProgress = Metrics
                    .CreateGauge("http_requests_in_progress", "The number of HTTP requests currently in progress");
            }

            if (_httpRequestsTotal == null)
            {
                _httpRequestsTotal = Metrics
                    .CreateGauge("http_requests_received_total", "Provides the count of HTTP requests that have been processed by this app",
                        new GaugeConfiguration {LabelNames = new[] {"code", "method", "controller", "action"}});
            }

            if (_httpRequestsDuration == null)
            {
                _httpRequestsDuration = Metrics
                    .CreateHistogram("http_request_duration_seconds", "The duration of HTTP requests processed by this app.",
                        new HistogramConfiguration
                        {
                            LabelNames = new[] {"code", "method", "controller", "action"},
                            Buckets = bucektSize
                        });
            }
        }

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
        private void OnBeginRequest(Object sender, EventArgs e)
        {
            _httpRequestsInProgress.Inc();

            var httpApp = (HttpApplication)sender;
            var timer = new Stopwatch();
            httpApp.Context.Items[_timerKey] = timer;
            timer.Start();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEndRequest(Object sender, EventArgs e)
        {
            _httpRequestsInProgress.Dec();

            var httpApp = (HttpApplication)sender;
            var timer = (Stopwatch)httpApp.Context.Items[_timerKey];

            if (timer != null)
            {
                timer.Stop();

                string code = httpApp.Response.StatusCode.ToString();
                string method = httpApp.Request.HttpMethod;
                var routeData = httpApp.Request.RequestContext?.RouteData?.Values;

                string controller = String.Empty;
                string action = String.Empty;

                if (routeData != null)
                {
                    if (routeData.ContainsKey("controller"))
                        controller = routeData["controller"].ToString();
                    if (routeData.ContainsKey("action"))
                        action = routeData["action"].ToString();
                }

                double timeTakenSecs = timer.ElapsedMilliseconds / 1000d;

                _httpRequestsDuration.WithLabels(code, method, controller, action).Observe(timeTakenSecs);
                _httpRequestsTotal.WithLabels(code, method, controller, action).Inc();
            }

            httpApp.Context.Items.Remove(_timerKey);
        }

        /// <summary>
        ///
        /// </summary>
        public void Dispose() { /* Not needed */ }
    }

}
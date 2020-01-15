using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.EF
{
    /// <summary>
    /// DBCommandInterceptor that times SQL calls and then exposes them as prometheus metrics
    /// </summary>
    public class PrometheusEFLoggerForMetrics : IDbCommandInterceptor
    {
        private static readonly Gauge _sqlRequestsTotal = Metrics
            .CreateGauge("sql_requests_made_total", "Provides the count of SQL requests that have been executed by this app",
                new GaugeConfiguration { LabelNames = new[] { "database", "querytype", "success" } });

        private static readonly Histogram _sqlRequestsDuration = Metrics
            .CreateHistogram("sql_request_duration_seconds", "The duration of SQL queries processed by this app.",
                new HistogramConfiguration { LabelNames = new[] { "database", "querytype", "success" } });

        static readonly ConcurrentDictionary<DbCommand, DateTime> m_StartTime = new ConcurrentDictionary<DbCommand, DateTime>();

        /// <summary>
        /// Executes when a reader is executed
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            Log(command, interceptionContext);
        }

        /// <summary>
        /// Executes when a non query is executed (eg, update)
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            Log(command, interceptionContext);
        }

        /// <summary>
        /// Executes when a scalar execute is executed (update and we want to see how many)
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            Log(command, interceptionContext);
        }

        private void Log<T>(DbCommand command, DbCommandInterceptionContext<T> interceptionContext)
        {
            TimeSpan duration = (m_StartTime.TryRemove(command, out DateTime startTime)) ?
            DateTime.Now - startTime :
            TimeSpan.Zero;

            string queryType = String.Empty;
            string sql = command.CommandText.Trim();
            string[] lookFors = { "SELECT", "UPDATE", "DELETE", "INSERT" };

            foreach (string lookFor in lookFors)
                if (sql.StartsWith(lookFor))
                    queryType = lookFor;

            string wasSuccess = interceptionContext.Exception == null ? "true" : "false";

            _sqlRequestsTotal.WithLabels(command.Connection.Database, queryType, wasSuccess).Inc();
            _sqlRequestsDuration.WithLabels(command.Connection.Database, queryType, wasSuccess).Observe(duration.TotalSeconds);
        }

        /// <summary>
        /// Called when non query has started executing
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            OnStart(command);
        }

        /// <summary>
        /// Called when a reader command has started executing
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            OnStart(command);
        }

        /// <summary>
        /// Called when a scalar command has started executing
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            OnStart(command);
        }

        private static void OnStart(DbCommand command)
        {
            m_StartTime.TryAdd(command, DateTime.Now);
        }
    }
}

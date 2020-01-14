using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.EntityFramework
{
    public class PrometheusEFLoggerForMetrics : IDbCommandInterceptor
    {
        private static readonly Gauge _sqlRequestsTotal = Metrics
            .CreateGauge("sql_requests_made_total", "Provides the count of SQL requests that have been executed by this app");

        private static readonly Histogram _sqlRequestsDuration = Metrics
            .CreateHistogram("sql_request_duration_seconds", "The duration of SQL queries processed by this app.",
                new HistogramConfiguration { LabelNames = new[] { "database", "querytype", "success" } });

        static readonly ConcurrentDictionary<DbCommand, DateTime> m_StartTime = new ConcurrentDictionary<DbCommand, DateTime>();

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            Log(command, interceptionContext);
        }

        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            Log(command, interceptionContext);
        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            Log(command, interceptionContext);
        }

        private void Log<T>(DbCommand command, DbCommandInterceptionContext<T> interceptionContext)
        {
            _sqlRequestsTotal.Inc();

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

            _sqlRequestsDuration.WithLabels(command.Connection.Database, queryType, wasSuccess).Observe(duration.TotalSeconds);
        }


        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            OnStart(command);
        }

        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            OnStart(command);
        }

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

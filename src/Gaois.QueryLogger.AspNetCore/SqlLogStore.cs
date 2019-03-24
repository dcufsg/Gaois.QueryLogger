﻿using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Gaois.QueryLogger.AspNetCore
{
    /// <summary>
    /// Stores log data in a SQL Server database
    /// </summary>
    public sealed class SqlLogStore : LogStore
    {
        private readonly IOptionsMonitor<QueryLoggerSettings> _settings;
        private readonly IAlertService _alertService;

        /// <summary>
        /// Stores log data in a SQL Server database
        /// </summary>
        public SqlLogStore(
            IOptionsMonitor<QueryLoggerSettings> settings,
            IAlertService alertService) : base(settings.CurrentValue)
        {
            _settings = settings;
            _alertService = alertService;
        }

        /// <summary>
        /// Alerts designated users in case of possible issues with the query logger service
        /// </summary>
        /// <param name="alert">The <see cref="Alert"/> to be sent</param>
        public override void Alert(Alert alert)
        {
            _ = alert ?? throw new ArgumentNullException(nameof(alert));
            _alertService.Alert(alert);
        }

        /// <summary>
        /// Logs query data to a data store
        /// </summary>
        /// <param name="queries">The <see cref="Query"/> object or objects to be logged</param>
        public override void WriteLog(params Query[] queries)
        {
            _ = queries ?? throw new ArgumentNullException(nameof(queries));

            using (var db = new SqlConnection(_settings.CurrentValue.Store.ConnectionString))
                db.Execute(SqlQueries.WriteLog(_settings.CurrentValue.Store.TableName), queries);
        }

        /// <summary>
        /// Logs query data in a data store asynchronously
        /// </summary>
        /// <param name="queries">The <see cref="Query"/> object or objects to be logged</param>
        public override async Task WriteLogAsync(params Query[] queries)
        {
            _ = queries ?? throw new ArgumentNullException(nameof(queries));

            using (var db = new SqlConnection(_settings.CurrentValue.Store.ConnectionString))
                await db.ExecuteAsync(SqlQueries.WriteLog(_settings.CurrentValue.Store.TableName), queries).ConfigureAwait(false);
        }
    }
}
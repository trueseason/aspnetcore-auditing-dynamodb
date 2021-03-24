using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using AspNetCore.Auditing.DynamoDB.Models;
using AspNetCore.Auditing.Common;
using AspNetCore.Logging.Common;

namespace AspNetCore.Auditing.DynamoDB
{
    public class AuditingDynamoDBLogger : AuditingLogger
    {
        private const int DEFAULT_AUDITLOGTTLDAYS = 90;
        private const string DEFAULT_TABLENAME = "apiAuditLog";
        private readonly ILogger _logger;
        private readonly AuditingDynamoDBConfig _auditingConfig;
        private IAmazonDynamoDB _ddbClient;
        private readonly DynamoDBOperationConfig _ddbConfig = new DynamoDBOperationConfig { Conversion = DynamoDBEntryConversion.V2, IgnoreNullValues = true, SkipVersionCheck = true };

        public AuditingDynamoDBLogger(IOptions<AuditingDynamoDBConfig> auditingConfig, IAuditingEvaluator auditingEvaluator, IAmazonDynamoDB ddbClient, ILoggerFactory loggerFactory)
            : base(auditingConfig, auditingEvaluator)
        {
            _logger = loggerFactory.CreateLogger<AuditingDynamoDBLogger>();
            _auditingConfig = auditingConfig.Value;
            _ddbClient = ddbClient;
            _ddbConfig.OverrideTableName = auditingConfig?.Value?.AuditingTableName ?? DEFAULT_TABLENAME;
        }

        public override async Task LogAuditingInfoAsync(string logType, object logObject, string user)
        {
            try
            {
                if (IsEnabled(logType))
                {
                    var auditingEntry = new AuditingEntry
                    {
                        LogType = logType,
                        Source = _auditingConfig.SourceName,
                        Message = JsonSerializer.Serialize(logObject),
                        User = user,
                        LoggedTime = DateTime.Now.ToString("o"),
                        ExpirationTime = getRecordExpirationTime(_auditingConfig.AuditLogTTLDays)
                    };
                    using var ddbContext = new DynamoDBContext(_ddbClient, _ddbConfig);
                    auditingEntry.LogTraceContent(_logger, "LogAuditingInfoAsync");
                    await ddbContext.SaveAsync(auditingEntry);
                    _logger.LogInformation("LogAuditingInfoAsync completed successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LogAuditingInfoAsync Exception");
            }
        }

        private long getRecordExpirationTime(int configTTLDays)
        {
            int ttlDays = configTTLDays > 0 ? configTTLDays : DEFAULT_AUDITLOGTTLDAYS;
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 86400 * ttlDays;
        }
    }
}

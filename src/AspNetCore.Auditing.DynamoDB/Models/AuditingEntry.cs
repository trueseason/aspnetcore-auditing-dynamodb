using Amazon.DynamoDBv2.DataModel;

namespace AspNetCore.Auditing.DynamoDB.Models
{
    [DynamoDBTable("auditLogs")]
    public class AuditingEntry
    {
        public string LogType { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public string User { get; set; }
        public string LoggedTime { get; set; }
        public long ExpirationTime { get; set; }
    }
}
using AspNetCore.Auditing.Common.Models;

namespace AspNetCore.Auditing.DynamoDB.Models
{
    public class AuditingDynamoDBConfig : AuditingConfig
    {
        public string AuditingTableName { get; set; }
    }
}

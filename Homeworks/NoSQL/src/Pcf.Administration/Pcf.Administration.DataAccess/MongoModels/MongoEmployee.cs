using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Pcf.Administration.DataAccess.MongoModels
{
    public class MongoEmployee
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement("firstName")]
        public string FirstName { get; set; }

        [BsonElement("lastName")]
        public string LastName { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("roleId")]
        [BsonRepresentation(BsonType.String)]
        public Guid RoleId { get; set; }

        [BsonElement("appliedPromocodesCount")]
        public int AppliedPromocodesCount { get; set; }
        
        [BsonIgnore]
        public string FullName => $"{FirstName} {LastName}";
    }
} 
namespace Pcf.Administration.DataAccess.MongoModels
{
    public class MongoDatabaseSettings : IMongoDatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string EmployeesCollectionName { get; set; }
        public string RolesCollectionName { get; set; }
    }

    public interface IMongoDatabaseSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        string EmployeesCollectionName { get; set; }
        string RolesCollectionName { get; set; }
    }
} 
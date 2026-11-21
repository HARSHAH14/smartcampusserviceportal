using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using smartcampusserviceportal.Models;

namespace smartcampusserviceportal.Data
{
    public class ServiceRequestRepository
    {
        private readonly string connectionString;

        public ServiceRequestRepository()
        {
            connectionString = ConfigurationManager.ConnectionStrings["SmartCampus"]?.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ConfigurationErrorsException("The SmartCampus connection string is missing.");
            }
        }

        public IList<ServiceRequest> GetAll()
        {
            EnsureDatabase();
            var requests = new List<ServiceRequest>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT Id, Title, Description, Status, CreatedAt FROM dbo.ServiceRequests ORDER BY CreatedAt DESC, Id DESC",
                connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        requests.Add(Map(reader));
                    }
                }
            }

            return requests;
        }

        public ServiceRequest GetById(int id)
        {
            EnsureDatabase();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT Id, Title, Description, Status, CreatedAt FROM dbo.ServiceRequests WHERE Id = @Id",
                connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public void Add(ServiceRequest request)
        {
            EnsureDatabase();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "INSERT INTO dbo.ServiceRequests (Title, Description, Status, CreatedAt) VALUES (@Title, @Description, @Status, @CreatedAt)",
                connection))
            {
                AddParameters(command, request);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool Update(ServiceRequest request)
        {
            EnsureDatabase();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "UPDATE dbo.ServiceRequests SET Title = @Title, Description = @Description, Status = @Status WHERE Id = @Id",
                connection))
            {
                AddParameters(command, request);
                command.Parameters.Add("@Id", SqlDbType.Int).Value = request.Id;
                connection.Open();
                return command.ExecuteNonQuery() == 1;
            }
        }

        public bool Delete(int id)
        {
            EnsureDatabase();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand("DELETE FROM dbo.ServiceRequests WHERE Id = @Id", connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                connection.Open();
                return command.ExecuteNonQuery() == 1;
            }
        }

        private void EnsureDatabase()
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            var masterBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master"
            };

            using (var connection = new SqlConnection(masterBuilder.ConnectionString))
            using (var command = new SqlCommand(
                "IF DB_ID(@DatabaseName) IS NULL CREATE DATABASE [SmartCampus]",
                connection))
            {
                command.Parameters.Add("@DatabaseName", SqlDbType.NVarChar, 128).Value = databaseName;
                connection.Open();
                command.ExecuteNonQuery();
            }

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(@"
                IF OBJECT_ID(N'dbo.ServiceRequests', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.ServiceRequests
                    (
                        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ServiceRequests PRIMARY KEY,
                        Title NVARCHAR(120) NOT NULL,
                        Description NVARCHAR(1000) NOT NULL,
                        Status NVARCHAR(30) NOT NULL,
                        CreatedAt DATETIME2 NOT NULL
                    )
                END", connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static void AddParameters(SqlCommand command, ServiceRequest request)
        {
            command.Parameters.Add("@Title", SqlDbType.NVarChar, 120).Value = request.Title;
            command.Parameters.Add("@Description", SqlDbType.NVarChar, 1000).Value = request.Description;
            command.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value = request.Status;
            if (command.CommandText.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
            {
                command.Parameters.Add("@CreatedAt", SqlDbType.DateTime2).Value = request.CreatedAt;
            }
        }

        private static ServiceRequest Map(IDataRecord record)
        {
            return new ServiceRequest
            {
                Id = (int)record["Id"],
                Title = (string)record["Title"],
                Description = (string)record["Description"],
                Status = (string)record["Status"],
                CreatedAt = (DateTime)record["CreatedAt"]
            };
        }
    }
}

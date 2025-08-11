using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using D2G.Iris.ML.Core.Models;

namespace D2G.Iris.ML.ConfigUI.WPF.Services
{
    public class DatabaseSchemaLoader : IDatabaseSchemaLoader
    {
        public List<string> LoadTableColumns(DatabaseConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrWhiteSpace(config.Server) ||
                string.IsNullOrWhiteSpace(config.Database) ||
                string.IsNullOrWhiteSpace(config.TableName))
            {
                throw new ArgumentException("Database configuration is incomplete.");
            }

            var connectionString = BuildConnectionString(config);
            var columns = new List<string>();

            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                var (schemaName, tableName) = ParseTableName(config.TableName);

                var query = @"
                    SELECT COLUMN_NAME
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = @TableName
                    AND (@SchemaName IS NULL OR TABLE_SCHEMA = @SchemaName)
                    ORDER BY ORDINAL_POSITION";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@TableName", tableName);
                command.Parameters.AddWithValue("@SchemaName", (object)schemaName ?? DBNull.Value);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    columns.Add(reader.GetString("COLUMN_NAME"));
                }

                if (columns.Count == 0)
                {
                    throw new InvalidOperationException($"Table '{config.TableName}' not found or has no columns.");
                }

                return columns;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error loading table columns: {ex.Message}", ex);
            }
        }

        public bool TestConnection(DatabaseConfig config)
        {
            try
            {
                var connectionString = BuildConnectionString(config);
                using var connection = new SqlConnection(connectionString);
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        private string BuildConnectionString(DatabaseConfig config)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = config.Server,
                InitialCatalog = config.Database,
                IntegratedSecurity = true,
                TrustServerCertificate = true,
                ConnectTimeout = 30
            };

            return builder.ConnectionString;
        }

        private (string schema, string table) ParseTableName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return (null, tableName);

            tableName = tableName.Trim('[', ']');

            var parts = tableName.Split('.');

            if (parts.Length == 1)
            {
                return (null, parts[0].Trim('[', ']'));
            }
            else if (parts.Length == 2)
            {
                return (parts[0].Trim('[', ']'), parts[1].Trim('[', ']'));
            }
            else
            {
                var table = parts[parts.Length - 1].Trim('[', ']');
                var schema = string.Join(".", parts[0..^1]).Trim('[', ']');
                return (schema, table);
            }
        }
    }
}
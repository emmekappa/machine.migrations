using System;
using Machine.Migrations.DatabaseProviders;
using Machine.Migrations.SchemaProviders;
using Machine.Migrations.Services;
using Machine.Migrations.Services.Impl;

namespace Machine.Migrations
{
	public static class DetectDataBase
	{
		public static void Detect(IConfiguration configuration, string databaseName)
		{
			switch (databaseName)
			{
				case "sqlserver":
					configuration.ConnectionProviderType = typeof(SqlServerConnectionProvider);
					configuration.SchemaProviderType = typeof(SqlServerSchemaProvider);
					configuration.DatabaseProviderType = typeof(SqlServerDatabaseProvider);
					configuration.SchemaStateManager = typeof(SqlServerSchemaStateManager);
					break;

				case "oracle":
					configuration.ConnectionProviderType = typeof(OracleConnectionProvider);
					configuration.SchemaProviderType = typeof(OracleSchemaProvider);
					configuration.DatabaseProviderType = typeof(OracleDatabaseProvider);
					configuration.SchemaStateManager = typeof(OracleSchemaStateManager);
					break;
				default:
					throw new ArgumentException("Supported type are SqlServer or Oracle");
			}
		}
	}
}
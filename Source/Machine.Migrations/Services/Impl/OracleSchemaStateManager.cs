using System;
using System.Collections.Generic;
using Machine.Migrations.DatabaseProviders;
using Machine.Migrations.SchemaProviders;

namespace Machine.Migrations.Services.Impl
{
	public class OracleSchemaStateManager : AbstractSchemaStateManager
	{
		public OracleSchemaStateManager(IDatabaseProvider databaseProvider, ISchemaProvider schemaProvider)
			: base(databaseProvider, schemaProvider)
		{
		}

		public override IEnumerable<long> GetAppliedMigrationVersions(string scope)
		{
			CheckSchemaInfoTable();
			if (string.IsNullOrEmpty(scope))
			{
				return
					//SELECT CAST("version" AS NUMBER) FROM schema_info WHERE "scope" IS NULL ORDER BY "version"
					_databaseProvider.ExecuteScalarArray<Int64>(
						"SELECT CAST(\"{1}\" AS NUMBER) FROM \"{0}\" WHERE \"{2}\" IS NULL ORDER BY \"{1}\"",
						TableName, VersionColumnName, ScopeColumnName);
			}
			else
			{
				return
					_databaseProvider.ExecuteScalarArray<Int64>(
						"SELECT CAST({1} AS NUMBER) FROM {0} WHERE {2} = '{3}' ORDER BY {1}",
						TableName, VersionColumnName, ScopeColumnName, scope);
			}
		}
	}
}
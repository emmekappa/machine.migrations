using System;
using System.Collections.Generic;
using Machine.Migrations.DatabaseProviders;
using Machine.Migrations.SchemaProviders;

namespace Machine.Migrations.Services.Impl
{
	public class SqlServerSchemaStateManager : AbstractSchemaStateManager
	{
		public SqlServerSchemaStateManager(IDatabaseProvider databaseProvider, ISchemaProvider schemaProvider)
			: base(databaseProvider, schemaProvider)
		{
		}

		protected override string VersionColumnType
		{
			get { return "bigint"; }
		}

		public override IEnumerable<long> GetAppliedMigrationVersions(string scope)
		{
			CheckSchemaInfoTable();
			if (string.IsNullOrEmpty(scope))
			{
				return
					_databaseProvider.ExecuteScalarArray<Int64>(
						"SELECT CAST({1} AS BIGINT) FROM {0} WHERE {2} IS NULL ORDER BY {1}",
						TableName, VersionColumnName, ScopeColumnName);
			}
			else
			{
				return
					_databaseProvider.ExecuteScalarArray<Int64>(
						"SELECT CAST({1} AS BIGINT) FROM {0} WHERE {2} = '{3}' ORDER BY {1}",
						TableName, VersionColumnName, ScopeColumnName, scope);
			}
		}

		public override void SetMigrationVersionApplied(long version, string scope)
		{
			if (string.IsNullOrEmpty(scope))
			{
				_databaseProvider.ExecuteNonQuery("INSERT INTO {0} ({1}, {2}) VALUES ({3}, NULL)",
												  TableName, VersionColumnName, ScopeColumnName, version);
			}
			else
			{
				_databaseProvider.ExecuteNonQuery("INSERT INTO {0} ({1}, {2}) VALUES ({3}, '{4}')",
												  TableName, VersionColumnName, ScopeColumnName, version, scope);
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Migrations.DatabaseProviders;
using Machine.Migrations.SchemaProviders;

namespace Machine.Migrations.Services.Impl
{
	public class OracleSchemaStateManager : AbstractSchemaStateManager
	{
		private readonly string _sequenceName = "SEQ_SCHEMA_INFO";

		public OracleSchemaStateManager(IDatabaseProvider databaseProvider, ISchemaProvider schemaProvider)
			: base(databaseProvider, schemaProvider)
		{
		}

		protected override string VersionColumnType
		{
			get { return "NUMBER"; }
		}

		public override IEnumerable<long> GetAppliedMigrationVersions(string scope)
		{
			CheckSchemaInfoTable();

			if( _databaseProvider.ExecuteScalar<Decimal>("SELECT COUNT(*) FROM SEQ WHERE SEQUENCE_NAME = '{0}'", _sequenceName) == 0)
                _databaseProvider.ExecuteNonQuery("CREATE SEQUENCE {0} MINVALUE 1 START WITH 1 INCREMENT BY 1", _sequenceName);

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
					_databaseProvider.ExecuteScalarArray<long>(
						"SELECT CAST({1} AS NUMBER) FROM {0} WHERE {2} = '{3}' ORDER BY {1}",
						TableName, VersionColumnName, ScopeColumnName, scope);
			}
		}
        
		public override void SetMigrationVersionApplied(long version, string scope)
		{
			if (string.IsNullOrEmpty(scope))
			{
				_databaseProvider.ExecuteNonQuery("INSERT INTO \"{0}\" (\"id\", \"{1}\", \"{2}\") VALUES ({3}.NEXTVAL, {4}, NULL)",
				                                  TableName, VersionColumnName, ScopeColumnName, _sequenceName, version);
			}
			else
			{
				_databaseProvider.ExecuteNonQuery("INSERT INTO \"{0}\" (\"id\", \"{1}\", \"{2}\") VALUES ({3}.NEXTVAL, {4}, '{5}')",
												  TableName, VersionColumnName, ScopeColumnName, _sequenceName, version, scope);
			}
		}
	}
}
using System;
using System.Collections.Generic;
using Machine.Migrations.DatabaseProviders;
using Machine.Migrations.SchemaProviders;

namespace Machine.Migrations.Services.Impl
{
	public abstract class AbstractSchemaStateManager : ISchemaStateManager
	{
		private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(SqlServerSchemaStateManager));
		protected readonly string TableName = "schema_info";
		private readonly string IdColumnName = "id";
		protected readonly string VersionColumnName = "version";
		protected readonly string ScopeColumnName = "scope";
		protected IDatabaseProvider _databaseProvider;
		private ISchemaProvider _schemaProvider;

		public AbstractSchemaStateManager(IDatabaseProvider databaseProvider, ISchemaProvider schemaProvider)
		{
			_databaseProvider = databaseProvider;
			_schemaProvider = schemaProvider;
		}

		public void CheckSchemaInfoTable()
		{
			if (_schemaProvider.HasTable(TableName))
			{
				if (!_schemaProvider.HasColumn(TableName, ScopeColumnName))
				{
					_log.InfoFormat("Adding {0} column to {1}...", ScopeColumnName, TableName);
					_schemaProvider.AddColumn(TableName, ScopeColumnName, typeof(string), 25, false, true);
				}

				if (!_schemaProvider.IsColumnOfType(TableName, VersionColumnName, VersionColumnType))
				{
					_log.InfoFormat("Changing {0} column to {1}...", VersionColumnName, VersionColumnType);
					_schemaProvider.ChangeColumn(TableName, VersionColumnName, typeof(Int64), 8, false);
				}

				return;
			}

			_log.InfoFormat("Creating {0}...", TableName);

			Column[] columns = new Column[]
			                   	{
			                   		new Column(IdColumnName, typeof(Int32), 4, true),
			                   		new Column(VersionColumnName, typeof(Int64), 8, false),
			                   		new Column(ScopeColumnName, typeof(string), 25, false, true)
			                   	};
			_schemaProvider.AddTable(TableName, columns);
		}

		protected abstract string VersionColumnType { get; }

		public abstract IEnumerable<long> GetAppliedMigrationVersions(string scope);

		public abstract void SetMigrationVersionApplied(long version, string scope);

		public void SetMigrationVersionUnapplied(long version, string scope)
		{
			if (string.IsNullOrEmpty(scope))
			{
				_databaseProvider.ExecuteNonQuery("DELETE FROM {0} WHERE {1} = {2} AND {3} IS NULL",
				                                  TableName, VersionColumnName, version, ScopeColumnName);
			}
			else
			{
				_databaseProvider.ExecuteNonQuery("DELETE FROM {0} WHERE {1} = {2} AND {3} = '{4}'",
				                                  TableName, VersionColumnName, version, ScopeColumnName, scope);
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using log4net;
using Machine.Core.LoggingUtilities;

namespace Machine.Migrations.Services.Impl
{
	public class MigrationRunner : IMigrationRunner
	{
		#region Logging

		private static readonly ILog _log = LogManager.GetLogger(typeof (MigrationRunner));

		#endregion

		#region Member Data

		private readonly IConfiguration _configuration;
		private readonly IMigrationFactoryChooser _migrationFactoryChooser;
		private readonly IMigrationInitializer _migrationInitializer;
		private readonly ISchemaStateManager _schemaStateManager;
		private readonly ITransactionProvider _transactionProvider;

		#endregion

		#region MigrationRunner()

		public MigrationRunner(IMigrationFactoryChooser migrationFactoryChooser, IMigrationInitializer migrationInitializer,
		                       ISchemaStateManager schemaStateManager, IConfiguration configuration,
		                       ITransactionProvider transactionProvider)
		{
			_schemaStateManager = schemaStateManager;
			_transactionProvider = transactionProvider;
			_configuration = configuration;
			_migrationInitializer = migrationInitializer;
			_migrationFactoryChooser = migrationFactoryChooser;
		}

		#endregion

		#region IMigrationRunner Members

		public bool CanMigrate(IDictionary<string, List<MigrationStep>> steps)
		{
			foreach (MigrationStep step in steps.SelectMany(row => row.Value).OrderBy(row => row.Version))
			{
				MigrationReference migrationReference = step.MigrationReference;
				IMigrationFactory migrationFactory = _migrationFactoryChooser.ChooseFactory(migrationReference);
				IDatabaseMigration migration = migrationFactory.CreateMigration(migrationReference);
				step.DatabaseMigration = migration;
				_migrationInitializer.InitializeMigration(migration);
			}
			_log.Info("All migrations are initialized.");
			return true;
		}

		public void Migrate(IDictionary<string, List<MigrationStep>> steps)
		{
			foreach (MigrationStep step in sort(steps))
			{
				_log.Info(step);
				using (Log4NetNdc.Push("{0}", step.MigrationReference.Name))
				{
					_configuration.ActiveConfigurationKey = step.MigrationReference.ConfigurationKey;
					if (!_configuration.ShowDiagnostics)
					{
						IDbTransaction transaction = null;
						try
						{
							transaction = _transactionProvider.Begin();
							step.Apply();
							if (step.Reverting)
							{
								_schemaStateManager.SetMigrationVersionUnapplied(step.Version, _configuration.Scope);
							}
							else
							{
								_schemaStateManager.SetMigrationVersionApplied(step.Version, _configuration.Scope);
							}
							_log.InfoFormat("Comitting");
							transaction.Commit();
						}
						catch (Exception)
						{
							if (transaction != null)
							{
								_log.InfoFormat("Rollback");
								transaction.Rollback();
							}
							throw;
						}
						finally
						{
							_configuration.ActiveConfigurationKey = null;
						}
					}
				}
			}
		}

		#endregion

		private IOrderedEnumerable<MigrationStep> sort(IDictionary<string, List<MigrationStep>> steps)
		{
			if (steps.SelectMany(row => row.Value).Any(x => x.Reverting))
				return steps.SelectMany(row => row.Value).OrderByDescending(row => row.Version);
			else
				return steps.SelectMany(row => row.Value).OrderBy(row => row.Version);
		}
	}
}
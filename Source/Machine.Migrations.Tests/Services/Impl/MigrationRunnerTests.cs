using System;
using System.Collections.Generic;
using System.Data;
using Machine.Core;
using NUnit.Framework;
using Rhino.Mocks;

namespace Machine.Migrations.Services.Impl
{
	[TestFixture]
	public class MigrationRunnerTests : StandardFixture<MigrationRunner>
	{
		private ISchemaStateManager _schemaStateManager;
		private IMigrationFactoryChooser _migrationFactoryChooser;
		private IMigrationInitializer _migrationInitializer;
		private IMigrationFactory _migrationFactory;
		private IConfiguration _configuration;
		private IDatabaseMigration _migration1;
		private IDatabaseMigration _migration2;
		private ITransactionProvider _transactionProvider;
		private IDbTransaction _transaction;
		private Dictionary<string, List<MigrationStep>> _steps;

		public override MigrationRunner Create()
		{
			_steps = new Dictionary<string, List<MigrationStep>>();
			_steps[string.Empty] = new List<MigrationStep>();
			_steps[string.Empty].Add(new MigrationStep(new MigrationReference(1, "A", "001_a.cs"), false));
			_steps[string.Empty].Add(new MigrationStep(new MigrationReference(2, "B", "002_b.cs"), false));
			_migration1 = _mocks.StrictMock<IDatabaseMigration>();
			_migration2 = _mocks.StrictMock<IDatabaseMigration>();
			_schemaStateManager = _mocks.DynamicMock<ISchemaStateManager>();
			_migrationFactoryChooser = _mocks.DynamicMock<IMigrationFactoryChooser>();
			_migrationInitializer = _mocks.DynamicMock<IMigrationInitializer>();
			_migrationFactory = _mocks.DynamicMock<IMigrationFactory>();
			_configuration = _mocks.DynamicMock<IConfiguration>();
			_transactionProvider = _mocks.DynamicMock<ITransactionProvider>();
			_transaction = _mocks.StrictMock<IDbTransaction>();
			return new MigrationRunner(_migrationFactoryChooser, _migrationInitializer, _schemaStateManager, _configuration,
			                           _transactionProvider);
		}

		[Test]
		public void CanMigrate_Always_GetsFactoryAndInitializes()
		{
			using (_mocks.Record())
			{
				SetupResult.For(_migrationFactoryChooser.ChooseFactory(_steps[string.Empty][0].MigrationReference)).Return(
					_migrationFactory);
				SetupResult.For(_migrationFactoryChooser.ChooseFactory(_steps[string.Empty][1].MigrationReference)).Return(
					_migrationFactory);
				SetupResult.For(_migrationFactory.CreateMigration(_steps[string.Empty][0].MigrationReference)).Return(_migration1);
				SetupResult.For(_migrationFactory.CreateMigration(_steps[string.Empty][1].MigrationReference)).Return(_migration2);
				_migrationInitializer.InitializeMigration(_migration1);
				_migrationInitializer.InitializeMigration(_migration2);
			}
			_target.CanMigrate(_steps);
			_mocks.VerifyAll();
			Assert.AreEqual(_migration1, _steps[string.Empty][0].DatabaseMigration);
			Assert.AreEqual(_migration2, _steps[string.Empty][1].DatabaseMigration);
		}

		[Test]
		public void Migrate_Diagnostics_DoesNotApply()
		{
			_steps[string.Empty][0].DatabaseMigration = _migration1;
			_steps[string.Empty][1].DatabaseMigration = _migration2;
			using (_mocks.Record())
			{
				SetupResult.For(_configuration.ShowDiagnostics).Return(true);
			}
			_target.Migrate(_steps);
			_mocks.VerifyAll();
		}

		[Test]
		public void Migrate_NoDiagnosticsAndThrows_Rollsback()
		{
			_steps[string.Empty][0].DatabaseMigration = _migration1;
			_steps[string.Empty][1].DatabaseMigration = _migration2;
			using (_mocks.Record())
			{
				Expect.Call(_transactionProvider.Begin()).Return(_transaction);
				_migration1.Up();
				_schemaStateManager.SetMigrationVersionApplied(1, null);
				_transaction.Commit();
				Expect.Call(_transactionProvider.Begin()).Return(_transaction);
				_migration2.Up();
				LastCall.Throw(new ArgumentException());
				_transaction.Rollback();
			}
			bool caught = false;
			try
			{
				_target.Migrate(_steps);
			}
			catch (ArgumentException)
			{
				caught = true;
			}
			Assert.IsTrue(caught);
			_mocks.VerifyAll();
		}

		[Test]
		public void Migrate_NoDiagnostics_Applies()
		{
			_steps[string.Empty][0].DatabaseMigration = _migration1;
			_steps[string.Empty][1].DatabaseMigration = _migration2;
			using (_mocks.Record())
			{
				Expect.Call(_transactionProvider.Begin()).Return(_transaction);
				_migration1.Up();
				_schemaStateManager.SetMigrationVersionApplied(1, null);
				_transaction.Commit();
				Expect.Call(_transactionProvider.Begin()).Return(_transaction);
				_migration2.Up();
				_schemaStateManager.SetMigrationVersionApplied(2, null);
				_transaction.Commit();
			}
			_target.Migrate(_steps);
			_mocks.VerifyAll();
		}

		[Test]
		public void 
			Migrate_should_revert_migrations_from_the_newest_to_the_oldest()
		{
			_steps.Clear();
			_steps = new Dictionary<string, List<MigrationStep>>();
			_steps[string.Empty] = new List<MigrationStep>();
			_steps[string.Empty].Add(new MigrationStep(new MigrationReference(0, "A", "001_A"), true));
			_migration1 = _mocks.StrictMock<IDatabaseMigration>();
			_steps[string.Empty][0].DatabaseMigration = _migration1;
			_steps[string.Empty].Add(new MigrationStep(new MigrationReference(1, "B", "002_B"), true));
			_migration2 = _mocks.StrictMock<IDatabaseMigration>();
			_steps[string.Empty][1].DatabaseMigration = _migration2;
			var dbTransaction = _mocks.Stub<IDbTransaction>();

			using (_mocks.Record())
			{
				SetupResult.For(_transactionProvider.Begin()).Return(dbTransaction);
				SetupResult.For(_configuration.ShowDiagnostics).Return(false);											

				using (_mocks.Ordered())
				{
					Expect.Call(_migration2.Down);
					Expect.Call(_migration1.Down);
				}				
			}

			_target.Migrate(_steps);

			_mocks.VerifyAll();
		}
	}
}
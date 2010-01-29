using Machine.Migrations.DatabaseProviders;
using NUnit.Framework;
using Rhino.Mocks;

namespace Machine.Migrations.SchemaProviders
{
	[TestFixture]
	public class OracleSchemaProviderTests
	{
		private IDatabaseProvider databaseProviderStub;
		private OracleSchemaProvider oracleSchemaProvider;

		[SetUp]
		public void SetUp()
		{
			databaseProviderStub = MockRepository.GenerateStub<IDatabaseProvider>();
			oracleSchemaProvider = new OracleSchemaProvider(databaseProviderStub);
		}

		[Test]
		public void RenameTable_should_generate_a_valid_DDL_statement()
		{
			oracleSchemaProvider.RenameTable("TableName", "NewTableName");
			databaseProviderStub.AssertWasCalled(x =>
				x.ExecuteNonQuery("ALTER TABLE {0} RENAME TO {1}", "\"TableName\"", "\"NewTableName\""));
		}

		[Test]
		public void RenameColumn_should_generate_a_valid_DDL_statement()
		{
			oracleSchemaProvider.RenameColumn("TableName", "ColumnName", "NewColumnName");
			databaseProviderStub.AssertWasCalled(x =>
				x.ExecuteNonQuery("ALTER TABLE {0} RENAME COLUMN {1} TO {2}", "\"TableName\"", "\"ColumnName\"", "\"NewColumnName\""));
		}
	}
}
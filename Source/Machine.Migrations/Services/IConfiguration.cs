using System;

namespace Machine.Migrations.Services
{
	public interface IConfiguration
	{
		string Scope { get; }

		Type ConnectionProviderType { get; }

		Type TransactionProviderType { get; }

		Type SchemaProviderType { get; }

		Type DatabaseProviderType { get; }

		string ConnectionString { get; }

		string ActiveConfigurationKey { get; set; }

		string MigrationsDirectory { get; }

		string CompilerVersion { get; }

		long DesiredVersion { get; }

		bool ShowDiagnostics { get; }

		string[] References { get; }

		int CommandTimeout { get; }

		Type SchemaStateManager { get; }

		string ConnectionStringByKey(string key);

		void SetCommandTimeout(int commandTimeout);
	}
}
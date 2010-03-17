using System;

namespace Machine.Migrations.Services
{
	public interface IConfiguration
	{
		string Scope { get; }

		Type ConnectionProviderType { get; set; }

		Type TransactionProviderType { get; }

		Type SchemaProviderType { get; set; }

		Type DatabaseProviderType { get; set; }

		string ConnectionString { get; }

		string ActiveConfigurationKey { get; set; }

		string MigrationsDirectory { get; }

		string CompilerVersion { get; }

		long DesiredVersion { get; }

		bool ShowDiagnostics { get; }

		string[] References { get; }

		int CommandTimeout { get; }

		Type SchemaStateManager { get; set; }

		string ConnectionStringByKey(string key);

		void SetCommandTimeout(int commandTimeout);

		string DriverAssemblyName { get;  }
	}
}
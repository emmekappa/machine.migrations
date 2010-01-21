using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using Machine.Migrations.DatabaseProviders;
using Machine.Migrations.SchemaProviders;
using Machine.Migrations.Services;
using Machine.Migrations.Services.Impl;

namespace Machine.Migrations.ConsoleRunner
{
	public class Program
	{
		public Program(IConsole console)
		{
		}

		public static void Main(string[] args)
		{
			var appender = new ConsoleAppender() {Layout = new PatternLayout("%-5p %x %m%n")};
			appender.ActivateOptions();

			BasicConfigurator.Configure(appender);

			var program = new Program(new DefaultConsole());
			ExitCode exitCode = program.Run(args);

			Environment.Exit((int) exitCode);
		}

		public ExitCode Run(string[] args)
		{
			try
			{
				var options = new Options();
				options.ParseArguments(args);

				var task = new Migrator();
				task.Run(new Configuration(options));
			}
			catch (Exception err)
			{
				Console.Error.WriteLine(err.ToString());
				return ExitCode.Failure;
			}

			return ExitCode.Success;
		}
	}

	public enum ExitCode
	{
		Success = 0,
		Failure = 1
	}

	public interface IConsole
	{
	}

	public class DefaultConsole : IConsole
	{
	}

	public class Configuration : IConfiguration
	{
		private readonly IDictionary<string, string> _connectionStrings;

		public Configuration(Options options)
		{
			Scope = options.Scope;
			setDatabaseOptions(options);			
			MigrationsDirectory = options.MigrationsDirectory;
			CompilerVersion = options.CompilerVersion;
			DesiredVersion = options.ToMigration;
			ShowDiagnostics = options.ShowDiagnostics;
			References = options.References.ToArray();
			CommandTimeout = options.CommandTimeout;
			_connectionStrings = options.ParseConnectionStrings();
		}

		#region IConfiguration Members

		public string Scope { get; set; }
		public Type ConnectionProviderType { get; set; }
		public Type TransactionProviderType { get; set; }
		public Type SchemaProviderType { get; set; }
		public Type DatabaseProviderType { get; set; }
		public string ActiveConfigurationKey { get; set; }

		public string ConnectionStringByKey(string key)
		{
			if (!_connectionStrings.ContainsKey(key))
			{
				throw new KeyNotFoundException("No connection string for key: " + key + " only have them for " +
				                               string.Join(" ", _connectionStrings.Keys.ToArray()));
			}
			return _connectionStrings[key];
		}

		public string ConnectionString
		{
			get { return ConnectionStringByKey(ActiveConfigurationKey); }
		}

		public string MigrationsDirectory { get; set; }
		public string CompilerVersion { get; set; }
		public long DesiredVersion { get; set; }
		public bool ShowDiagnostics { get; set; }
		public string[] References { get; set; }
		public int CommandTimeout { get; set; }
		public Type SchemaStateManager { get; set; }		

		public void SetCommandTimeout(int commandTimeout)
		{
			CommandTimeout = commandTimeout;
		}

		#endregion

		private void setDatabaseOptions(Options options)
		{
			TransactionProviderType = typeof (TransactionProvider);
			switch (options.Database.ToLower())
			{
				case "sqlserver":
					ConnectionProviderType = typeof (SqlServerConnectionProvider);
					SchemaProviderType = typeof (SqlServerSchemaProvider);
					DatabaseProviderType = typeof (SqlServerDatabaseProvider);
					SchemaStateManager = typeof (SqlServerSchemaStateManager);
					break;

				case "oracle":
					ConnectionProviderType = typeof(OracleConnectionProvider);					
					SchemaProviderType = typeof(OracleSchemaProvider);
					DatabaseProviderType = typeof(OracleDatabaseProvider);
					SchemaStateManager = typeof(OracleSchemaStateManager);
					break;
				default:
					throw new ArgumentException("Supported type are SqlServer or Oracle");
			}
		}
	}
}
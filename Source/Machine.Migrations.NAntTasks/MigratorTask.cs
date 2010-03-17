using System;
using log4net.Appender;
using log4net.Core;
using Machine.Migrations.Services;
using Machine.Migrations.Services.Impl;
using NAnt.Core.Attributes;
using NAntCore = NAnt.Core;


namespace Machine.Migrations.NAnt.Tasks
{
	[TaskName("migrator")]
	public class MigratorTask : NAntCore.Task, IConfiguration
	{
		private string[] _references;

		public MigratorTask()
		{
			DesiredVersion = -1;
			CommandTimeout = 30;
			MigrationsDirectory = Environment.CurrentDirectory;
		}

		[TaskAttribute("connectionString", Required = true)]
		[StringValidator(AllowEmpty = false)]
		public string ConnectionString { get; set; }

		public string Scope { get; set; }

		public string ActiveConfigurationKey { get; set; }

		[TaskAttribute("migrationsDirectory", Required = true)]
		[StringValidator(AllowEmpty = false)]
		public string MigrationsDirectory { get; set; }
        
		[TaskAttribute("toVersion", Required = false)]
		public long DesiredVersion { get; set; }

		[TaskAttribute("compilerVersion")]
		public string CompilerVersion { get; set; }

		[TaskAttribute("database")]
		public string DataBase { get; set; }

		[TaskAttribute("driverAssemblyName")]
		public string DriverAssemblyName { get; set; }

		public string ConnectionStringByKey(string key)
		{
			return ConnectionString;
		}

		[TaskAttribute("timeout")]
		public int CommandTimeout { get; set; }

		public void SetCommandTimeout(int commandTimeout)
		{
			CommandTimeout = commandTimeout;
		}

		public bool ShowDiagnostics { get; set; }

		public string[] References
		{
			get
			{
				if (_references == null)
				{
					return new string[0];
				}
				return _references;
			}
			set { _references = value; }
		}

		public virtual Type TransactionProviderType
		{
			get { return typeof(TransactionProvider); }
		}

		public Type SchemaStateManager { get; set; }

		public virtual Type ConnectionProviderType { get; set; }

		public virtual Type SchemaProviderType { get; set; }

		public virtual Type DatabaseProviderType { get; set; }

		protected override void ExecuteTask()
		{
			var logAppender = new ConsoleAppender(); // new EventLogAppender();
			logAppender.Layout = new log4net.Layout.PatternLayout("%-6level %-13date{HH:mm:ss,fff} %m %newline");
			log4net.Config.BasicConfigurator.Configure(logAppender);

			DetectDataBase.Detect(this, this.DataBase);

			var migrator = new Migrator();
			migrator.Run(this);			
		}
	}
}
using System;
using System.Data;
using System.Reflection;

namespace Machine.Migrations.Services.Impl
{
	public class OracleConnectionProvider : AbstractConnectionProvider
	{
		private const string ConnectionTypeName = "Oracle.DataAccess.Client.OracleConnection";
		private const string DriverAssemblyName = "Oracle.DataAccess, Version=2.111.6.0, Culture=neutral, PublicKeyToken=89b483f429c47342";

		public OracleConnectionProvider(IConfiguration configuration) : base(configuration)
		{			
		}

		protected override IDbConnection CreateConnection(IConfiguration configuration, string key)
		{			
			var assemblies = Assembly.Load(DriverAssemblyName);
			var type = assemblies.GetType(ConnectionTypeName);
			var connectionString = configuration.ConnectionStringByKey(key);
			return (IDbConnection)Activator.CreateInstance(type, connectionString);
		}	
	}
}
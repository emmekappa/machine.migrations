using System;
using System.Collections.Generic;
using System.Data;
using log4net;
using Machine.Migrations.Services;

namespace Machine.Migrations.DatabaseProviders
{
	public class OracleDatabaseProvider : IDatabaseProvider
	{
		#region Logging

		private static readonly ILog _log = LogManager.GetLogger(typeof (SqlServerDatabaseProvider));

		#endregion

		#region Member Data

		private readonly IConfiguration _configuration;
		private readonly IConnectionProvider _connectionProvider;
		private readonly ITransactionProvider _transactionProvider;

		#endregion

		#region SqlServerDatabaseProvider()

		public OracleDatabaseProvider(IConnectionProvider connectionProvider, ITransactionProvider transactionProvider,
		                              IConfiguration configuration)
		{
			_configuration = configuration;
			_connectionProvider = connectionProvider;
			_transactionProvider = transactionProvider;
		}

		#endregion

		#region IDatabaseProvider Members

		public void Open()
		{
			_connectionProvider.OpenConnection();
		}

		public object ExecuteScalar(string sql, params object[] objects)
		{
			try
			{
				IDbCommand command = PrepareCommand(sql, objects);
				return command.ExecuteScalar();
			}
			catch (Exception ex)
			{
				throw new Exception("ExecuteScalar: Error executing " + string.Format(sql, objects), ex);
			}
		}

		public T ExecuteScalar<T>(string sql, params object[] objects)
		{
			try
			{
				IDbCommand command = PrepareCommand(sql, objects);
				return (T) command.ExecuteScalar();
			}
			catch (Exception ex)
			{
				throw new Exception("ExecuteScalar<>: Error executing " + string.Format(sql, objects), ex);
			}
		}

		public T[] ExecuteScalarArray<T>(string sql, params object[] objects)
		{
			try
			{
				IDataReader reader = ExecuteReader(sql, objects);
				var values = new List<T>();
				while (reader.Read())
				{
					values.Add((T)Convert.ChangeType(reader.GetValue(0), typeof(T)));
				}
				reader.Close();
				return values.ToArray();
			}
			catch (Exception ex)
			{
				throw new Exception("ExecuteScalarArray: Error executing " + string.Format(sql, objects), ex);
			}
		}

		public IDataReader ExecuteReader(string sql, params object[] objects)
		{
			try
			{
				IDbCommand command = PrepareCommand(sql, objects);
				return command.ExecuteReader();
			}
			catch (Exception ex)
			{
				throw new Exception("ExecuteReader: Error executing " + string.Format(sql, objects), ex);
			}
		}

		public bool ExecuteNonQuery(string sql, params object[] objects)
		{
			try
			{
				IDbCommand command = PrepareCommand(sql, objects);
				command.ExecuteNonQuery();
				return true;
			}
			catch (Exception ex)
			{
				throw new Exception("ExecuteNonQuery: Error executing " + string.Format(sql, objects), ex);
			}
		}

		public void Close()
		{
			if (DatabaseConnection != null)
			{
				DatabaseConnection.Close();
			}
		}

		#endregion

		#region Member Data

		protected virtual IDbConnection DatabaseConnection
		{
			get { return _connectionProvider.CurrentConnection; }
		}

		private IDbCommand PrepareCommand(string sql, object[] objects)
		{
			IDbCommand command = DatabaseConnection.CreateCommand();
			command.CommandTimeout = _configuration.CommandTimeout;
			command.CommandText = String.Format(sql, objects);
			_transactionProvider.Enlist(command);
			_log.Info(command.CommandText);
			return command;
		}

		#endregion
	}
}
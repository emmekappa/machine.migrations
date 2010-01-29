using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Machine.Core.LoggingUtilities;
using Machine.Migrations.Builders;
using Machine.Migrations.DatabaseProviders;

namespace Machine.Migrations.SchemaProviders
{
	public class OracleSchemaProvider : ISchemaProvider
	{
		#region Member Data

		private readonly IDatabaseProvider _databaseProvider;

		#endregion

		#region Properties

		protected IDatabaseProvider DatabaseProvider
		{
			get { return _databaseProvider; }
		}

		#endregion

		#region SqlServerSchemaProvider()

		public OracleSchemaProvider(IDatabaseProvider databaseProvider)
		{
			_databaseProvider = databaseProvider;
		}

		#endregion

		#region ISchemaProvider Members

		public void AddTable(string table, ICollection<Column> columns)
		{
			if (columns.Count == 0)
			{
				throw new ArgumentException("columns");
			}
			using (Log4NetNdc.Push("AddTable"))
			{
				var sb = new StringBuilder();				
				sb.Append("CREATE TABLE " + quote(table)).Append(" (");
				bool first = true;
				foreach (Column column in columns)
				{
					if (!first) sb.Append(",");
					sb.AppendLine().Append(ColumnToCreateTableSql(column));
					first = false;
				}

				foreach (Column column in columns)
				{
					if (column.IsIdentity)
						throw new NotSupportedException("Identity not supported");

					if (column.IsSequence || column.IsNative)
					{
						var sequenceName = string.IsNullOrEmpty(column.SequenceName) ? "SEQ_" + table.ToUpper() : column.SequenceName;
						if (sequenceName.Length > 30)
							sequenceName = sequenceName.Substring(0, 29);						
							if (
								_databaseProvider.ExecuteScalar<Decimal>("SELECT COUNT(*) FROM SEQ WHERE SEQUENCE_NAME = '{0}'", sequenceName) == 0)
								_databaseProvider.ExecuteNonQuery("CREATE SEQUENCE {0} MINVALUE 1 START WITH 1 INCREMENT BY 1", quote(sequenceName));
					}

					string sql = ColumnToConstraintsSql(table, column);
					if (sql != null)
					{
						sb.Append(",").AppendLine().Append(sql);
					}
				}

				sb.AppendLine().Append(")");
				_databaseProvider.ExecuteNonQuery(sb.ToString());
			}
		}

		public void DropTable(string table)
		{
			using (Log4NetNdc.Push("DropTable({0})", table))
			{
				_databaseProvider.ExecuteNonQuery("DROP TABLE {0}", quote(table));
			}
		}

		public virtual bool HasTable(string table)
		{
			using (Log4NetNdc.Push("HasTable({0})", table))
			{
				return
					_databaseProvider.ExecuteScalar<Decimal>(
						"SELECT COUNT(*) FROM TABS WHERE TABLE_NAME = '{0}'", table) > 0;
			}
		}

		public void AddColumn(string table, string column, Type type)
		{
			AddColumn(table, column, type, -1, false, false);
		}

		public void AddColumn(string table, string column, Type type, bool allowNull)
		{
			AddColumn(table, column, type, 0, false, allowNull);
		}

		public void AddColumn(string table, string column, Type type, short size, bool allowNull)
		{
			AddColumn(table, column, type, size, false, allowNull);
		}

		public void AddColumn(string table, string column, Type type, short size, bool isPrimaryKey, bool allowNull)
		{
			try
			{
				addColumnInternal(table, column, type, size, isPrimaryKey, allowNull);
			}
			catch (Exception e)
			{
				if (tableNotFound(e))
				{
					addColumnInternal(quote(table), column, type, size, isPrimaryKey, allowNull);
					return;
				}
				throw;
			}
		}

		private bool tableNotFound(Exception e)
		{
			if (e.InnerException.GetType().Name.Equals("OracleException"))
				if (e.InnerException.Message.Contains("ORA-00942"))
					return true;
			return false;
		}

		public void addColumnInternal(string table, string column, Type type, short size, bool isPrimaryKey, bool allowNull)
		{
			_databaseProvider.ExecuteNonQuery("ALTER TABLE {0} ADD {1}", table,
											  ColumnToCreateTableSql(new Column(column, type, size, isPrimaryKey, allowNull)));
		}

		public void RemoveColumn(string table, string column)
		{
			using (Log4NetNdc.Push("RemoveColumn({0}.{1}) ", table, column))
			{
				_databaseProvider.ExecuteNonQuery("ALTER TABLE {0} DROP COLUMN {1}", quote(table), quote(column));
			}
		}

		public void RenameTable(string table, string newName)
		{
			using (Log4NetNdc.Push("RenameTable({0}.{1}) ", table, newName))
			{
				_databaseProvider.ExecuteNonQuery("ALTER TABLE {0} RENAME TO {1}", quote(table), quote(newName));
			}
		}

		public void RenameColumn(string table, string column, string newName)
		{
			using (Log4NetNdc.Push("RenameTable({0}.{1}) ", table, newName))
			{
				_databaseProvider.ExecuteNonQuery("ALTER TABLE {0} RENAME COLUMN {1} TO {2}",
				                                  quote(table), quote(column), quote(newName));
			}
		}

		public void AddSchema(string schemaName)
		{
			throw new NotImplementedException();
		}

		public void RemoveSchema(string schemaName)
		{
			throw new NotImplementedException();
		}

		public bool HasSchema(string schemaName)
		{
			throw new NotImplementedException();
		}

		public virtual bool HasColumn(string table, string column)
		{
			using (Log4NetNdc.Push("HasColumn({0}.{1})", table, column))
			{
				return
					_databaseProvider.ExecuteScalar<Decimal>(
						"SELECT COUNT(*) FROM USER_TAB_COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'", 
						table, column) > 0;
			}
		}

		public virtual bool IsColumnOfType(string table, string column, string type)
		{
			using (Log4NetNdc.Push("IsColumnOfType({0}.{1}.{2})", table, column, type))
			{
				return
					_databaseProvider.ExecuteScalar<Decimal>(
						"SELECT COUNT(*) FROM USER_TAB_COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}' AND DATA_TYPE = '{2}'", table, column, type)
						> 0;
			}
		}

		public void ChangeColumn(string table, string column, Type type, short size, bool allowNull)
		{
			_databaseProvider.ExecuteNonQuery("ALTER TABLE {0} MODIFY {1}", quote(table),
			                                  ColumnToCreateTableSql(new Column(column, type, size, false, allowNull)));
		}

		public virtual string[] Columns(string table)
		{
			using (
				IDataReader reader =
					_databaseProvider.ExecuteReader(
						"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}'", table))
			{
				return GetColumnAsArray(reader, 0);
			}
		}

		public virtual string[] Tables()
		{
			using (IDataReader reader = _databaseProvider.ExecuteReader("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES"))
			{
				return GetColumnAsArray(reader, 0);
			}
		}

		public void AddForeignKeyConstraint(string table, string name, string column, string foreignTable, string foreignColumn)
		{
			using (Log4NetNdc.Push("AddForeignKeyConstraint({0}.{1})", table, name))
			{
				_databaseProvider.ExecuteNonQuery(
					"ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4})", 
					quote(table), quote(name), quote(column), quote(foreignTable), quote(foreignColumn));
			}
		}

		public void AddUniqueConstraint(string table, string name, params string[] columns)
		{
			if (columns.Length == 0)
				throw new ArgumentException("AddUniqueConstraint requires at least one column name", "columns");

			string colList = "";
			foreach (string column in columns)
			{
				if (colList.Length != 0)
					colList += ", ";
				colList += "\"" + column + "\" ";
			}

			_databaseProvider.ExecuteNonQuery(
				"ALTER TABLE {0} ADD CONSTRAINT {1} UNIQUE ({2})", quote(table), quote(name), colList);
		}

		public void DropConstraint(string table, string name)
		{
			using (Log4NetNdc.Push("DropConstraint({0}.{1})", table, name))
			{
				_databaseProvider.ExecuteNonQuery("ALTER TABLE {0} DROP CONSTRAINT {1}", quote(table), quote(name));
			}
		}

		public void AddIndex(string table, string indexName, bool isUnique, bool recomputeStatistics, params string[] columns)
		{
			if (columns.Length == 0)
				throw new ArgumentException("AddIndex requires at least one column name", "columns");

			string unique = isUnique ? "UNIQUE" : "";
			string statistics = recomputeStatistics ? " COMPUTE STATISTICS " : "";
			List<string> cols = new List<string>();
			columns.ToList().ForEach(x => cols.Add(quote(x.Trim())));
			_databaseProvider.ExecuteNonQuery("CREATE {0} INDEX {1} ON {2} ({3}) {4}", 
				unique, quote(indexName), quote(table), string.Join(",", cols.ToArray()), statistics);
		}

		public void DropIndex(string table, string indexName)
		{
			using (Log4NetNdc.Push("DropIndex({0})", indexName))
			{
				_databaseProvider.ExecuteNonQuery("DROP INDEX {0}", quote(indexName));
			}
		}

		#endregion

		#region Member Data

		public static string[] GetColumnAsArray(IDataReader reader, int columnIndex)
		{
			var values = new List<string>();
			while (reader.Read())
			{
				values.Add(reader.GetString(columnIndex));
			}
			return values.ToArray();
		}

		public virtual string ColumnToCreateTableSql(Column column)
		{
			return String.Format("\"{0}\" {1} {2}",
			                     column.Name,
			                     ToDataBaseType(column.ColumnType, column.Size),
			                     column.AllowNull ? "" : "NOT NULL ENABLE");
		}

		public virtual string ColumnToConstraintsSql(string tableName, Column column)
		{
			if (column.IsPrimaryKey)
			{
				return String.Format("CONSTRAINT PK_{0}_{1} PRIMARY KEY (\"{1}\")", SchemaUtils.Normalize(tableName),
				                     SchemaUtils.Normalize(column.Name));
			}
			else if (column.IsUnique)
			{
				return String.Format("CONSTRAINT UK_{0}_{1} UNIQUE (\"{1}\" ASC)", SchemaUtils.Normalize(tableName),
				                     SchemaUtils.Normalize(column.Name));
			}
			return null;
		}

		public virtual string ToDataBaseType(ColumnType type, int size)
		{
			switch (type)
			{
				case ColumnType.Int16:
				case ColumnType.Int32:
					return "NUMBER(10,0)";
				case ColumnType.Long:
					return "NUMBER(20,0)";
				case ColumnType.Money:
					return "NUMBER(20,2)";
				case ColumnType.NVarChar:
					if (size == 0)
					{
						return "NVARCHAR2(150)"; // Why 150? Kind of arbitrary? -jlewalle
					}
					return String.Format("NVARCHAR2({0})", size);
				case ColumnType.Real:
					return "REAL";
				case ColumnType.Text:
					return "TEXT";
				case ColumnType.Binary:
					return "RAW(2000)";
				case ColumnType.Bool:
					return "NUMBER(1,0)";
				case ColumnType.Char:
					return "CHAR(1)";
				case ColumnType.DateTime:
					return "DATE";
				case ColumnType.Decimal:
					return "NUMBER(19,5)";
				case ColumnType.Image:
					return "RAW(2000)";
				case ColumnType.Guid:
					return "RAW(16)";
			}

			throw new ArgumentException("type");
		}

		#endregion

		private string quote(string name)
		{
			return "\"" + name.Trim('\"') + "\"";
		}
	}
}
namespace Machine.Migrations.Builder
{
	using System;
	using System.Collections.Generic;
	using SchemaProviders;

	public class SimpleColumnBuilder : ColumnBuilder<SimpleColumnBuilder>
	{
		public SimpleColumnBuilder(string name, Type columnType, short? size) : base(name, columnType, size)
		{
		}

		public SimpleColumnBuilder(string name, Type columnType) : base(name, columnType)
		{
		}

		public SimpleColumnBuilder(string name, ColumnType colType) : base(name, colType)
		{
		}

		public SimpleColumnBuilder(string name, ColumnType colType, short size) : base(name, colType, size)
		{
		}

		public override Column Build(TableBuilder table, ISchemaProvider schemaBuilder, IList<PostProcess> posts)
		{
			Column col = base.Build(table, schemaBuilder, posts);
			col.IsPrimaryKey = false;
			return col;
		}
	}
}

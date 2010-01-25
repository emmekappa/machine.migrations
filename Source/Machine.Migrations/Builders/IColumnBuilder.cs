namespace Machine.Migrations.Builders
{
  public interface IColumnBuilder
  {
		IColumnBuilder Identity();
		IColumnBuilder Sequence(string sequenceName);
		IColumnBuilder Native();
		IColumnBuilder Nullable();
		IColumnBuilder Unique();
  }
}
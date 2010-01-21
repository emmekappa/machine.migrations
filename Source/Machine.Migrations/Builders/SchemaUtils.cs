namespace Machine.Migrations.Builders
{
  public static class SchemaUtils
  {
    public static string Normalize(string content)
    {
      return content.
        Replace(".", "_").
        Replace("[", "").
        Replace("]", "").
        Replace("`", "");
    }
  }
}
using System.Linq;

namespace Machine.Migrations.Builders
{
	public static class Query
	{
		public static string Format(string format, params object[] objects)
		{
			string[] values = objects.Select(x => "\"" + x.ToString() + "\"").ToArray();
			return string.Format(format, values);
		}
	}
}
using System.Web.Mvc;

namespace AdminMigrationUtility.CustomPages
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
			filters.Add(new MyCustomErrorHandler());
		}
	}
}
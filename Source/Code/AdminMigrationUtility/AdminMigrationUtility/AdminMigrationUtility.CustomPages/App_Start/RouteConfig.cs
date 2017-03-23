using System.Web.Mvc;
using System.Web.Routing;

namespace AdminMigrationUtility.CustomPages
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"TemplateGenerator",
				"TemplateGenerator/Download/{migrationObjectType}",
				new { controller = "TemplateGenerator", action = "Download", migrationObjectType = "" }
			);

			routes.MapRoute(
					name: "Default",
					url: "{controller}/{action}/{id}",
					defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}
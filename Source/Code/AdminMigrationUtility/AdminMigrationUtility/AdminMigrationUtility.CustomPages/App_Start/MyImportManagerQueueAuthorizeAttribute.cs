using AdminMigrationUtility.Helpers.Rsapi;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using Relativity.API;
using Relativity.CustomPages;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AdminMigrationUtility.CustomPages
{
	public class MyImportManagerQueueAuthorizeAttribute : AuthorizeAttribute
	{
		protected override Boolean AuthorizeCore(HttpContextBase httpContext)
		{
			Boolean isAuthorized = false;

			if (httpContext.Session != null)
			{
				Int32 caseArtifactId = -1;
				Int32.TryParse(httpContext.Request.QueryString["appid"], out caseArtifactId);

				IArtifactQueries query = new ArtifactQueries();
				Boolean res = query.UserHasAccessToArtifact(
												ConnectionHelper.Helper().GetServicesManager(),
												ExecutionIdentity.CurrentUser,
												caseArtifactId,
												Helpers.Constant.Guids.Tabs.ImportUtilityJob,
												"Tab");
				isAuthorized = res;
			}

			return isAuthorized;
		}

		protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
		{
			filterContext.Result = new RedirectToRouteResult(
				new RouteValueDictionary
					{
							{"action", "AccessDenied"},
							{"controller", "Error"}
					});
		}
	}
}
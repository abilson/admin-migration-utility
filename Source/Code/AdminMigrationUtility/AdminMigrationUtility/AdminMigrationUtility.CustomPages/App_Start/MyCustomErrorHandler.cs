using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.SQL;
using Relativity.API;
using Relativity.CustomPages;
using System;
using System.Web.Mvc;

namespace AdminMigrationUtility.CustomPages
{
	public class MyCustomErrorHandler : HandleErrorAttribute
	{
		public void OnExportException(ExceptionContext filterContext)
		{
			base.OnException(filterContext);
			Int32 caseArtifactId = -1;
			Int32.TryParse(filterContext.HttpContext.Request.QueryString["appid"], out caseArtifactId);

			var sqlQueryHelper = new SqlQueryHelper();

			if (filterContext.Exception != null)
			{
				try
				{
					//try to log the error to the errors tab in Relativity
					Helpers.Rsapi.ErrorQueries.WriteError(ConnectionHelper.Helper().GetServicesManager(),
					ExecutionIdentity.CurrentUser, caseArtifactId, filterContext.Exception);
				}
				catch
				{
					//if the error cannot be logged, add the error to our custom Errors table
					sqlQueryHelper.InsertRowIntoExportErrorLogAsync(ConnectionHelper.Helper().GetDBContext(-1), caseArtifactId, Constant.Tables.ExportWorkerQueue, 0, 0, filterContext.Exception.ToString()).Wait();
				}
			}
		}

		public void OnImportException(ExceptionContext filterContext)
		{
			base.OnException(filterContext);
			Int32 caseArtifactId = -1;
			Int32.TryParse(filterContext.HttpContext.Request.QueryString["appid"], out caseArtifactId);

			var sqlQueryHelper = new SqlQueryHelper();

			if (filterContext.Exception != null)
			{
				try
				{
					//try to log the error to the errors tab in Relativity
					Helpers.Rsapi.ErrorQueries.WriteError(ConnectionHelper.Helper().GetServicesManager(),
					ExecutionIdentity.CurrentUser, caseArtifactId, filterContext.Exception);
				}
				catch
				{
					//if the error cannot be logged, add the error to our custom Errors table
					sqlQueryHelper.InsertRowIntoImportErrorLogAsync(ConnectionHelper.Helper().GetDBContext(-1), caseArtifactId, Constant.Tables.ImportWorkerQueue, 0, 0, filterContext.Exception.ToString()).Wait();
				}
			}
		}
	}
}
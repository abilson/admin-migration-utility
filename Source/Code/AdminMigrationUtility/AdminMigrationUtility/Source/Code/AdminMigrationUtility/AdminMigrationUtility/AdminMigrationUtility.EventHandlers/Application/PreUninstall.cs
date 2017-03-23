using kCura.EventHandler;
using Relativity.API;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.EventHandlers.Application
{
	[kCura.EventHandler.CustomAttributes.Description("Pre Uninstall EventHandler")]
	[System.Runtime.InteropServices.Guid("BC6875B9-5A13-4A1B-ADBA-6793A669BA4F")]
	public class PreUninstall : PreUninstallEventHandler
	{
		public override Response Execute()
		{
			//Construct a response object with default values.
			var response = new Response { Success = true, Message = string.Empty };

			try
			{
				var dbContextEdds = Helper.GetDBContext(-1);
				var workspaceArtifactId = Helper.GetActiveCaseID();
				var dbContext = Helper.GetDBContext(workspaceArtifactId);
				var sqlQueryHelper = new Helpers.SQL.SqlQueryHelper();
				var logger = GetLoggerAsync().Result;

				var importJob = new PreUninstallJob(dbContextEdds, dbContext, sqlQueryHelper, workspaceArtifactId, logger);
				response = importJob.ExecutePreUninstall().Result;
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.ToString();
			}

			return response;
		}

		private async Task<IAPILog> GetLoggerAsync()
		{
			return await Task.Run(() => Helper.GetLoggerFactory().GetLogger());
		}
	}
}
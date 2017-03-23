using AdminMigrationUtility.Helpers.Rsapi;
using kCura.EventHandler;
using Relativity.API;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.EventHandlers.Export
{
	[kCura.EventHandler.CustomAttributes.Description("This Event Handler will check for Export Utility Job dependencies prior to deletion.")]
	[System.Runtime.InteropServices.Guid("A08FF310-F12A-41CE-90FA-367AD603B41B")]
	public class ExportJobPreCascadeDelete : PreCascadeDeleteEventHandler
	{
		public override Response Execute()
		{
			var response = new Response() { Success = true, Message = "" };

			try
			{
				var artifactQueries = new ArtifactQueries();
				var sqlQueryHelper = new Helpers.SQL.SqlQueryHelper();
				var workspaceArtifactId = Helper.GetActiveCaseID();
				var svcManager = Helper.GetServicesManager();
				var identityCurrentUser = ExecutionIdentity.CurrentUser;
				var tempTableName = TempTableNameWithParentArtifactsToDelete;
				var logger = GetLoggerAsync().Result;
				var dbContext = Helper.GetDBContext(workspaceArtifactId);

				var exportJob = new ExportJob(svcManager, sqlQueryHelper, artifactQueries, workspaceArtifactId, identityCurrentUser, dbContext, tempTableName, logger);
				response = exportJob.ExecutePreCascadeDelete().Result;
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Exception = new SystemException("Pre Cascade Delete Failure: " + ex.Message);
			}

			return response;
		}

		public override FieldCollection RequiredFields
		{
			get { return new FieldCollection(); }
		}

		public override void Rollback()
		{
		}

		public override void Commit()
		{
		}

		private async Task<IAPILog> GetLoggerAsync()
		{
			return await Task.Run(() => Helper.GetLoggerFactory().GetLogger());
		}
	}
}
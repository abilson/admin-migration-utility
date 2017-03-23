using AdminMigrationUtility.Helpers.Rsapi;
using kCura.EventHandler;
using Relativity.API;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.EventHandlers.Import
{
	[kCura.EventHandler.CustomAttributes.Description("This Event Handler will check for Import Utility Job dependencies prior to deletion.")]
	[System.Runtime.InteropServices.Guid("C31FFA3B-533F-40E2-9BD2-4C2B38E19F8F")]
	public class ImportJobPreCascadeDelete : PreCascadeDeleteEventHandler
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

				var importJob = new ImportJob(svcManager, sqlQueryHelper, artifactQueries, workspaceArtifactId, identityCurrentUser, dbContext, tempTableName, logger);
				response = importJob.ExecutePreCascadeDelete().Result;
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
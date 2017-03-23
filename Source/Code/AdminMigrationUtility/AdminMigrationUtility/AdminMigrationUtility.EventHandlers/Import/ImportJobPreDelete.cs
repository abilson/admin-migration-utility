using AdminMigrationUtility.Helpers.Rsapi;
using kCura.EventHandler;
using Relativity.API;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.EventHandlers.Import
{
	[kCura.EventHandler.CustomAttributes.Description("This Event Handler will verifiy that the current Import Utility Job can be deleted based on the current status.")]
	[System.Runtime.InteropServices.Guid("004FB2C9-7CB9-40E8-B0E1-EBCFA308AFDC")]
	public class ImportJobPreDelete : PreDeleteEventHandler
	{
		public override Response Execute()
		{
			var response = new Response() { Success = true, Message = "" };

			try
			{
				var artifactQueries = new ArtifactQueries();
				var workspaceArtifactId = Helper.GetActiveCaseID();
				var svcManager = Helper.GetServicesManager();
				var identityCurrentUser = ExecutionIdentity.CurrentUser;
				var activeArtifactId = ActiveArtifact.ArtifactID;
				var logger = GetLoggerAsync().Result;

				var importJob = new ImportJob(svcManager, artifactQueries, workspaceArtifactId, identityCurrentUser, activeArtifactId, logger);
				response = importJob.ExecutePreDelete().Result;
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Exception = new SystemException("Pre Delete Failure: " + ex.Message);
			}

			return response;
		}

		public override FieldCollection RequiredFields => new FieldCollection();

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
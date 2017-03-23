using AdminMigrationUtility.Helpers.Rsapi;
using kCura.EventHandler;
using Relativity.API;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.EventHandlers.Export
{
	[kCura.EventHandler.CustomAttributes.Description("This Event Handler will verifiy that the current Export Utility Job can be deleted based on the current status.")]
	[System.Runtime.InteropServices.Guid("23A9E8C5-4670-4247-A161-DF8280695014")]
	public class ExportJobPreDelete : PreDeleteEventHandler
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

				var exportJob = new ExportJob(svcManager, artifactQueries, workspaceArtifactId, identityCurrentUser, activeArtifactId, logger);
				response = exportJob.ExecutePreDelete().Result;
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
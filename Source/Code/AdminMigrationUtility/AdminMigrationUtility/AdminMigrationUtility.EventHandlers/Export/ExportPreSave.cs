using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Rsapi;
using kCura.EventHandler;
using Relativity.API;
using System.Threading.Tasks;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;

namespace AdminMigrationUtility.EventHandlers.Export
{
	[kCura.EventHandler.CustomAttributes.Description("Sets fields on the Export Job.")]
	[System.Runtime.InteropServices.Guid("88365714-3E07-452C-A38F-0611BD22590B")]
	public class ExportPreSave : PreSaveEventHandler
	{
        public IArtifactQueries ArtifactQueries = new ArtifactQueries();

        public override Response Execute()
		{
			var response = new Response() { Message = string.Empty, Success = true };

			if (Utility.UserIsAdmin(Helper, Helper.GetAuthenticationManager().UserInfo.ArtifactID, ArtifactQueries))
			{
				var layoutArtifactIdByGuid = GetArtifactIdByGuid(Constant.Guids.Layouts.ExportUtilityJob.ExportUtilityJobLayout);
				var layoutArtifactId = ActiveLayout.ArtifactID;

				//check if this is the Export Job layout
				if (layoutArtifactId == layoutArtifactIdByGuid)
				{
					var svcManager = Helper.GetServicesManager();
					var artifactQueries = new ArtifactQueries();
					var sqlQueryHelper = new Helpers.SQL.SqlQueryHelper();
					var eddsDbContext = Helper.GetDBContext(-1);
					var workspaceArtifactId = Helper.GetActiveCaseID();
					var activeArtifact = ActiveArtifact;
					var identityCurrentUser = ExecutionIdentity.CurrentUser;
					var logger = GetLoggerAsync().Result;
					var exportJob = new ExportJob(svcManager, artifactQueries, eddsDbContext, sqlQueryHelper, workspaceArtifactId, identityCurrentUser, activeArtifact, logger);

					response = exportJob.ExecutePreSave(ActiveArtifact.IsNew, GetArtifactIdByGuid(Constant.Guids.Field.ExportUtilityJob.Status)).Result;
				}
			}
			else
			{
				response.Success = false;
				response.Message = "Only System Administrators are allowed to Create Export Jobs";
			}

			return response;
		}

		public override FieldCollection RequiredFields
		{
			get
			{
				FieldCollection fieldCollection = new FieldCollection
				{
					new Field(Constant.Guids.Field.ExportUtilityJob.Status),
					new Field(Constant.Guids.Field.ExportUtilityJob.EmailAddresses)
				};

				return fieldCollection;
			}
		}

		private async Task<IAPILog> GetLoggerAsync()
		{
			return await Task.Run(() => Helper.GetLoggerFactory().GetLogger());
		}
	}
}
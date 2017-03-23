using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Rsapi;
using kCura.EventHandler;
using Relativity.API;
using System.Threading.Tasks;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;

namespace AdminMigrationUtility.EventHandlers.Import
{
	[kCura.EventHandler.CustomAttributes.Description("Sets fields on the Import Job.")]
	[System.Runtime.InteropServices.Guid("2C0FB6F8-38F9-44B0-BC05-FAD8E738ED10")]
	public class ImportPreSave : PreSaveEventHandler
	{
        public IArtifactQueries ArtifactQueries = new ArtifactQueries();
        public override Response Execute()
		{
			var response = new Response() { Message = string.Empty, Success = true };

			if (Utility.UserIsAdmin(Helper, Helper.GetAuthenticationManager().UserInfo.ArtifactID, ArtifactQueries))
			{
				var layoutArtifactIdByGuid = GetArtifactIdByGuid(Constant.Guids.Layouts.ImportUtilityJob.ImportUtilityJobLayout);
				var layoutArtifactId = ActiveLayout.ArtifactID;

				//check if this is the Import Job layout
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
					var importJob = new ImportJob(svcManager, artifactQueries, eddsDbContext, sqlQueryHelper, workspaceArtifactId, identityCurrentUser, activeArtifact, logger);

					response = importJob.ExecutePreSave(ActiveArtifact.IsNew, GetArtifactIdByGuid(Constant.Guids.Field.ImportUtilityJob.Status)).Result;
				}
			}
			else
			{
				response.Success = false;
				response.Message = "Only System Administrators are allowed to Create Import Jobs";
			}

			return response;
		}

		public override FieldCollection RequiredFields
		{
			get
			{
				FieldCollection fieldCollection = new FieldCollection
				{
					new Field(Constant.Guids.Field.ImportUtilityJob.Status),
					new Field(Constant.Guids.Field.ImportUtilityJob.EmailAddresses)
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
using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Exceptions;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.SQL;
using Relativity.API;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AdminMigrationUtility.EventHandlers.Import
{
	public class ImportConsoleJob
	{
		public IServicesMgr SvcManager { get; set; }
		public IDBContext DbContextEdds { get; set; }
		public IDBContext DbContext { get; set; }
		public ExecutionIdentity IdentityCurrentUser { get; set; }
		public ExecutionIdentity IdentitySystem { get; set; }
		public IArtifactQueries ArtifactQueries { get; set; }
		public ISqlQueryHelper SqlQueryHelper { get; set; }
		public IAPILog Logger { get; set; }
		public int WorkspaceArtifactId { get; set; }
		public int CurrentArtifactId { get; set; }
		public int CurrentUserArtifactId { get; set; }
		public string ButtonName { get; set; }
		public string SelectedObjectType { get; set; }
		public string ImportManagerResultsTableName { get; set; }

		public ImportConsoleJob(IServicesMgr svcManager, IDBContext dbContextEdds, IDBContext dbContext, ExecutionIdentity currentUserIdentity, ExecutionIdentity systemIdentity, IArtifactQueries artifactQueries, ISqlQueryHelper sqlQueryHelper, IAPILog logger, int workspaceArtifactId, int currentArtifactId, int currentUserArtifactId, string buttonName, string selectedObjectType, string importManagerResultsTableName)
		{
			SvcManager = svcManager;
			DbContextEdds = dbContextEdds;
			DbContext = dbContext;
			IdentityCurrentUser = currentUserIdentity;
			IdentitySystem = systemIdentity;
			ArtifactQueries = artifactQueries;
			SqlQueryHelper = sqlQueryHelper;
			WorkspaceArtifactId = workspaceArtifactId;
			CurrentArtifactId = currentArtifactId;
			CurrentUserArtifactId = currentUserArtifactId;
			ButtonName = buttonName;
			SelectedObjectType = selectedObjectType;
			ImportManagerResultsTableName = importManagerResultsTableName;
			Logger = logger;
		}

		public async Task ExecuteAsync()
		{
			try
			{
				bool recordExists;

				switch (ButtonName)
				{
					case Constant.Buttons.VALIDATE:
						Logger.LogDebug($"{Constant.Names.ApplicationName} - {Constant.Buttons.VALIDATE} button clicked.");
						recordExists = await DoesRecordExistAsync();

						if (recordExists == false)
						{
							await DeleteJobErrors(SvcManager, IdentityCurrentUser, WorkspaceArtifactId, CurrentArtifactId);

							//Add the record to the Import Manager queue table
							var resourceGroupId = await ArtifactQueries.GetResourcePoolArtifactIdAsync(SvcManager, IdentitySystem, WorkspaceArtifactId);
							await SqlQueryHelper.InsertRowIntoImportManagerQueueAsync(DbContextEdds, WorkspaceArtifactId, 1, CurrentUserArtifactId, CurrentArtifactId, resourceGroupId, SelectedObjectType, Constant.ImportUtilityJob.JobType.VALIDATE);

							//Set the Import Job RDO status to Validating
							await UpdateJobStatusAsync(SvcManager, WorkspaceArtifactId, IdentityCurrentUser, Constant.Guids.ObjectType.ImportUtilityJob, CurrentArtifactId, Constant.Guids.Field.ImportUtilityJob.Status, Constant.Status.Job.SUBMITTED);
						}
						break;

					case Constant.Buttons.SUBMIT:
						Logger.LogDebug($"{Constant.Names.ApplicationName} - {Constant.Buttons.SUBMIT} button clicked.");
						recordExists = await DoesRecordExistAsync();

						if (recordExists == false)
						{
							await DeleteJobErrors(SvcManager, IdentityCurrentUser, WorkspaceArtifactId, CurrentArtifactId);

							//Add the record to the Import Manager queue table
							Int32 resourceGroupId = await ArtifactQueries.GetResourcePoolArtifactIdAsync(SvcManager, IdentitySystem, WorkspaceArtifactId);
							await SqlQueryHelper.InsertRowIntoImportManagerQueueAsync(DbContextEdds, WorkspaceArtifactId, 1, CurrentUserArtifactId, CurrentArtifactId, resourceGroupId, SelectedObjectType, Constant.ImportUtilityJob.JobType.VALIDATE_SUBMIT);

							//Set the Import Job RDO status to Validating
							await UpdateJobStatusAsync(SvcManager, WorkspaceArtifactId, IdentityCurrentUser, Constant.Guids.ObjectType.ImportUtilityJob, CurrentArtifactId, Constant.Guids.Field.ImportUtilityJob.Status, Constant.Status.Job.SUBMITTED);

							//Mark job as being submitted for migration
							await ArtifactQueries.MarkImportJobAsSubmittedForMigration(SvcManager, IdentityCurrentUser, WorkspaceArtifactId, CurrentArtifactId);
						}
						break;

					case Constant.Buttons.CANCEL:
						Logger.LogDebug($"{Constant.Names.ApplicationName} - {Constant.Buttons.CANCEL} button clicked.");
						var id = await RetrieveIdByArtifactIdAsync();

						if (id > 0)
						{
							//Set the Import Job RDO status to Cancel Requested
							await ArtifactQueries.UpdateRdoJobTextFieldAsync(SvcManager, WorkspaceArtifactId, IdentityCurrentUser, Constant.Guids.ObjectType.ImportUtilityJob, CurrentArtifactId, Constant.Guids.Field.ImportUtilityJob.Status, Constant.Status.Job.CANCELREQUESTED);

							//Set the Import Job record in the Manager Queue to Cancel Requested
							await UpdateJobStatusInManagerQueueAsync(DbContextEdds, WorkspaceArtifactId, CurrentArtifactId, Constant.Status.Queue.CANCELLATION_REQUESTED);
						}
						break;
				}
			}
			catch (AdminMigrationUtilityException ex)
			{
				throw new AdminMigrationUtilityException(ex.Message);
			}
		}

		public async Task<string> RetrieveJobStatusAsync(IServicesMgr svcManager, Int32 workspaceArtifactId, ExecutionIdentity identity, Int32 artifactId)
		{
			var status = await ArtifactQueries.RetrieveRdoJobStatusAsync(svcManager, workspaceArtifactId, identity, artifactId);
			return status;
		}

		public async Task<Boolean> DoesRecordExistAsync()
		{
			var dataRow = await SqlQueryHelper.RetrieveSingleInImportManagerQueueByArtifactIdAsync(DbContextEdds, CurrentArtifactId, WorkspaceArtifactId);
			return dataRow != null;
		}

		private async Task DeleteJobErrors(IServicesMgr svcManager, ExecutionIdentity identityCurrentUser, Int32 workspaceArtifactId, Int32 jobArtifactID)
		{
			var jobErrorArtifactIDs = await ArtifactQueries.QueryJobErrors(svcManager, identityCurrentUser, workspaceArtifactId, jobArtifactID, Constant.Guids.ObjectType.ImportUtilityJobErrors, Constant.Guids.Field.ImportUtilityJobErrors.ImportUtilityJob);
			if (jobErrorArtifactIDs != null & jobErrorArtifactIDs.Any())
			{
				var artifactTypeId = SqlQueryHelper.RetrieveArtifactTypeIdByGuidAsync(DbContext, Constant.Guids.ObjectType.ImportUtilityJobErrors).Result;
				await ArtifactQueries.DeleteJobErrors(svcManager, identityCurrentUser, workspaceArtifactId, jobErrorArtifactIDs.ToList(), artifactTypeId);
			}
		}

		private async Task UpdateJobStatusAsync(IServicesMgr svcManager, Int32 workspaceArtifactId, ExecutionIdentity identity, Guid objectTypeGuid, Int32 artifactId, Guid fieldGuid, String status)
		{
			await ArtifactQueries.UpdateRdoJobTextFieldAsync(svcManager, workspaceArtifactId, identity, objectTypeGuid, artifactId, fieldGuid, status);
		}

		private async Task<Int32> RetrieveIdByArtifactIdAsync()
		{
			var dataRow = await SqlQueryHelper.RetrieveSingleInImportManagerQueueByArtifactIdAsync(DbContextEdds, CurrentArtifactId, WorkspaceArtifactId);
			return dataRow != null ? Convert.ToInt32(dataRow["ID"]) : 0;
		}

		private async Task UpdateJobStatusInManagerQueueAsync(IDBContext dbContextEdds, Int32 workspaceArtifactId, Int32 currentArtifactId, Int32 status)
		{
			await SqlQueryHelper.UpdateStatusInImportManagerQueueByWorkspaceJobIdAsync(dbContextEdds, workspaceArtifactId, currentArtifactId, status);
		}
	}
}
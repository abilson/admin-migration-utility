using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Exceptions;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.SQL;
using Relativity.API;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AdminMigrationUtility.EventHandlers.Export
{
	public class ExportConsoleJob
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
		public string ButtonName { get; set; }
		public string SelectedObjectType { get; set; }
		public string ExportManagerResultsTableName { get; set; }
		public Int32 Priority { get; set; }

		public ExportConsoleJob(IServicesMgr svcManager, IDBContext dbContextEdds, IDBContext dbContext, ExecutionIdentity currentUserIdentity, ExecutionIdentity systemIdentity, IArtifactQueries artifactQueries, ISqlQueryHelper sqlQueryHelper, IAPILog logger, int workspaceArtifactId, int currentArtifactId, string buttonName, string selectedObjectType, int priority)
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
			ButtonName = buttonName;
			SelectedObjectType = selectedObjectType;
			Logger = logger;
			Priority = priority;
		}

		public async Task ExecuteAsync()
		{
			try
			{
				switch (ButtonName)
				{
					case Constant.Buttons.SUBMIT:
						Logger.LogDebug($"{Constant.Names.ApplicationName} - {Constant.Buttons.SUBMIT} button clicked.");
						var recordExists = await DoesRecordExistAsync();

						if (recordExists == false)
						{
							await DeleteJobErrors(SvcManager, IdentityCurrentUser, WorkspaceArtifactId, CurrentArtifactId);

							//Add the record to the Export Manager queue table
							var resourceGroupId = await ArtifactQueries.GetResourcePoolArtifactIdAsync(SvcManager, IdentitySystem, WorkspaceArtifactId);
							await SqlQueryHelper.InsertRowIntoExportManagerQueueAsync(DbContextEdds, WorkspaceArtifactId, Priority, CurrentArtifactId, resourceGroupId, SelectedObjectType);

							//Clear reset Job Statistics
							await ResetJobStatisticsAsync(SvcManager, WorkspaceArtifactId, IdentityCurrentUser, CurrentArtifactId);

							//Set the Export Job RDO status to Validating
							await UpdateJobStatusAsync(SvcManager, WorkspaceArtifactId, IdentityCurrentUser, Constant.Guids.ObjectType.ExportUtilityJob, CurrentArtifactId, Constant.Guids.Field.ExportUtilityJob.Status, Constant.Status.Job.SUBMITTED);
						}
						break;

					case Constant.Buttons.CANCEL:
						Logger.LogDebug($"{Constant.Names.ApplicationName} - {Constant.Buttons.CANCEL} button clicked.");
						var artifactId = await RetrieveIdByArtifactIdAsync();

						//Clear reset Job Statistics
						await ResetJobStatisticsAsync(SvcManager, WorkspaceArtifactId, IdentityCurrentUser, CurrentArtifactId);

						if (artifactId > 0)
						{
							//Set the Export Job RDO status to Cancel Requested
							await UpdateJobStatusAsync(SvcManager, WorkspaceArtifactId, IdentityCurrentUser, Constant.Guids.ObjectType.ExportUtilityJob, CurrentArtifactId, Constant.Guids.Field.ExportUtilityJob.Status, Constant.Status.Job.CANCELREQUESTED);

							//Set the Export Job record in the Manager Queue to Cancel Requested
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

		private async Task DeleteJobErrors(IServicesMgr svcManager, ExecutionIdentity identityCurrentUser, Int32 workspaceArtifactId, Int32 jobArtifactID)
		{
			var jobErrorArtifactIDs = await ArtifactQueries.QueryJobErrors(svcManager, identityCurrentUser, workspaceArtifactId, jobArtifactID, Constant.Guids.ObjectType.ExportUtilityJobErrors, Constant.Guids.Field.ExportUtilityJobErrors.ExportUtilityJob);
			if (jobErrorArtifactIDs != null & jobErrorArtifactIDs.Any())
			{
				var artifactTypeId = SqlQueryHelper.RetrieveArtifactTypeIdByGuidAsync(DbContext, Constant.Guids.ObjectType.ExportUtilityJobErrors).Result;
				await ArtifactQueries.DeleteJobErrors(svcManager, identityCurrentUser, workspaceArtifactId, jobErrorArtifactIDs.ToList(), artifactTypeId);
			}
		}

		public async Task<Boolean> DoesRecordExistAsync()
		{
			var dataRow = await SqlQueryHelper.RetrieveSingleInExportManagerQueueByArtifactIdAsync(DbContextEdds, CurrentArtifactId, WorkspaceArtifactId);
			return dataRow != null;
		}

		private async Task<Int32> RetrieveIdByArtifactIdAsync()
		{
			var dataRow = await SqlQueryHelper.RetrieveSingleInExportManagerQueueByArtifactIdAsync(DbContextEdds, CurrentArtifactId, WorkspaceArtifactId);
			if (dataRow != null)
			{
				return Convert.ToInt32(dataRow["ID"]);
			}
			return 0;
		}

		public async Task<string> RetrieveJobStatusAsync(IServicesMgr svcManager, Int32 workspaceArtifactId, ExecutionIdentity identity, Int32 artifactId)
		{
			var status = await ArtifactQueries.RetrieveRdoJobStatusAsync(svcManager, workspaceArtifactId, identity, artifactId);
			return status;
		}

		private async Task ResetJobStatisticsAsync(IServicesMgr svcManager, Int32 workspaceArtifactId, ExecutionIdentity identity, Int32 artifactId)
		{
			await ArtifactQueries.ResetExportJobStatisticsAsync(svcManager, workspaceArtifactId, identity, artifactId);
		}

		private async Task UpdateJobStatusAsync(IServicesMgr svcManager, Int32 workspaceArtifactId, ExecutionIdentity identity, Guid objectTypeGuid, Int32 artifactId, Guid fieldGuid, String status)
		{
			await ArtifactQueries.UpdateRdoJobTextFieldAsync(svcManager, workspaceArtifactId, identity, objectTypeGuid, artifactId, fieldGuid, status);
		}

		private async Task UpdateJobStatusInManagerQueueAsync(IDBContext dbContextEdds, Int32 workspaceArtifactId, Int32 currentArtifactId, Int32 status)
		{
			await SqlQueryHelper.UpdateStatusInExportManagerQueueByWorkspaceJobIdAsync(dbContextEdds, workspaceArtifactId, currentArtifactId, status);
		}
	}
}
using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Models;
using AdminMigrationUtility.Helpers.Models.Interfaces;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.Serialization;
using AdminMigrationUtility.Helpers.SQL;
using kCura.Relativity.Client;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Agents.Import
{
	[kCura.Agent.CustomAttributes.Name("AdminMigrationUtility - Import Worker Job")]
	[System.Runtime.InteropServices.Guid("2D6AC005-EC8E-4C37-AC40-B4A416971F70")]
	public class ImportWorkerJob : AgentJobBase
	{
		private readonly IRsapiRepositoryGroup _repositoryGroup;
		private readonly IArtifactQueries _artifactQueryHelper;
		private readonly APIOptions _apiOptions;
		private readonly ISerializationHelper _serializationHelper;

		public ImportWorkerJob(Int32 agentId, IAgentHelper agentHelper, ISqlQueryHelper sqlQueryHelper, IArtifactQueries artifactQueryHelper, DateTime processedOnDateTime, IRsapiRepositoryGroup rsapiRepositoryGroup, IEnumerable<Int32> resourceGroupIds, IAPILog logger, APIOptions apiOptions, ISerializationHelper serializationHelper)
		{
			TableRowId = 0;
			WorkspaceArtifactId = -1;
			AgentId = agentId;
			AgentHelper = agentHelper;
			SqlQueryHelper = sqlQueryHelper;
			QueueTable = Constant.Tables.ImportWorkerQueue;
			ProcessedOnDateTime = processedOnDateTime;
			AgentResourceGroupIds = resourceGroupIds;
			Logger = logger;
			_repositoryGroup = rsapiRepositoryGroup;
			_artifactQueryHelper = artifactQueryHelper;
			_apiOptions = apiOptions;
			_serializationHelper = serializationHelper;
		}

		public override async Task ExecuteAsync()
		{
			if (await IsOffHoursAsync(ProcessedOnDateTime))
			{
				//Check for jobs which stopped unexpectedly on this agent thread
				RaiseAndLogDebugMessage($"Resetting records which failed. [Table = {QueueTable}]");
				await ResetUnfinishedJobsAsync(AgentHelper.GetDBContext(-1));

				//Retrieve the next record to work on
				RaiseAndLogDebugMessage($"Retrieving next record(s) in the queue. [Table = {QueueTable}]");
				var delimitedListOfResourceGroupIds = GetCommaDelimitedListOfResourceIds(AgentResourceGroupIds);
				if (delimitedListOfResourceGroupIds != String.Empty)
				{
					DataTable batch = await RetrieveBatchAsync(delimitedListOfResourceGroupIds);

					if (TableIsNotEmpty(batch))
					{
						ImportWorkerQueueRecord record = null;
						var errors = new List<ImportJobError>();

						foreach (DataRow row in batch.Rows)
						{
							try
							{
								record = new ImportWorkerQueueRecord(row);

								SetJobProperties(record);

								RaiseAndLogDebugMessage($"Retrieved record(s) in the queue. [Table = {QueueTable}, ID = {TableRowId}, Workspace Artifact ID = {WorkspaceArtifactId}]");

								//Update Status
								var status = await GetImportJobStatus(record.WorkspaceArtifactID, record.JobID);
								if (status == Constant.Status.Job.COMPLETED_MANAGER)
								{
									await UpdateJobStatus(record.WorkspaceArtifactID, record.JobID, Constant.Status.Job.IN_PROGRESS_WORKER);
								}

								//Process the record(s)
								RaiseAndLogDebugMessage($"Processing record(s). [Table = {QueueTable}, ID = {TableRowId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
								await ProcessRecordsAsync(AgentHelper, AgentHelper.GetDBContext(-1), _repositoryGroup, SqlQueryHelper, record, errors);
								RaiseAndLogDebugMessage($"Processed record(s). [Table = {QueueTable}, ID = {TableRowId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
							}
							catch (Exception ex)
							{
								if (record != null)
								{
									var exceptionError = new ImportJobError() { Message = String.Format(Constant.ErrorMessages.ImportWorkerLineNumberError, record.ImportRowID, ex.Message), Type = Constant.ImportUtilityJob.ErrorType.DataLevel, LineNumber = record.ImportRowID };
									errors.Add(exceptionError);
								}
								else
								{
									throw;
								}
							}
						}

						if (record != null)
						{
							if (errors.Any())
							{
								RaiseAndLogDebugMessage($"Recording Error(s). [Table = {QueueTable}, ID = {TableRowId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
								await _artifactQueryHelper.CreateImportJobErrorRecordsAsync(_apiOptions, record.WorkspaceArtifactID, record.JobID, _repositoryGroup.RdoRepository, errors, record.ObjectType);
							}

							RaiseAndLogDebugMessage($"Deleting Batch. [Table = {QueueTable}, ID = {TableRowId}, Workspace Artifact ID = {WorkspaceArtifactId}]");
							await SqlQueryHelper.DeleteRecordFromQueueAsync(AgentHelper.GetDBContext(-1), AgentId, record.WorkspaceArtifactID, record.JobID, Constant.Tables.ImportWorkerQueue);
						}
					}
					else
					{
						RaiseAndLogDebugMessage("No records in the queue for this resource pool.");
					}
				}
				else
				{
					RaiseAndLogDebugMessage("This agent server is not part of any resource pools.  Agent execution skipped.");
				}
			}
			else
			{
				RaiseAndLogDebugMessage($"Current time is not between {OffHoursStartTime} and {OffHoursEndTime}. Agent execution skipped.");
			}
		}

		private void SetJobProperties(ImportWorkerQueueRecord importWorkerQueueRecord)
		{
			WorkspaceArtifactId = importWorkerQueueRecord.WorkspaceArtifactID;
			TableRowId = importWorkerQueueRecord.ImportRowID;
		}

		private async Task UpdateJobStatus(Int32 workspaceArtifactID, Int32 jobArtifactID, String status)
		{
			await _artifactQueryHelper.UpdateRdoTextFieldValueAsync(_apiOptions, workspaceArtifactID, _repositoryGroup.RdoRepository, jobArtifactID, Constant.Guids.ObjectType.ImportUtilityJob, Constant.Guids.Field.ImportUtilityJob.Status, status);
		}

		private async Task<String> GetImportJobStatus(Int32 workspaceArtifactID, Int32 jobArtifactID)
		{
			var job = await _artifactQueryHelper.RetrieveJob(_repositoryGroup.RdoRepository, _apiOptions, workspaceArtifactID, jobArtifactID);
			return job[Constant.Guids.Field.ImportUtilityJob.Status].ValueAsFixedLengthText ?? String.Empty;
		}

		private Boolean TableIsNotEmpty(DataTable table)
		{
			return (table != null && table.Rows.Count > 0);
		}

		public async Task<DataTable> RetrieveBatchAsync(String delimiitedListOfResourceGroupIds)
		{
			DataTable next = await SqlQueryHelper.RetrieveNextBatchInImportWorkerQueueAsync(AgentHelper.GetDBContext(-1), AgentId, Constant.BatchSizes.ImportWorker, delimiitedListOfResourceGroupIds);
			return next;
		}

		public async Task ProcessRecordsAsync(IHelper helper, IDBContext eddsDbContext, IRsapiRepositoryGroup repositoryGroup, ISqlQueryHelper sqlQueryHelper, ImportWorkerQueueRecord record, List<ImportJobError> errors)
		{
			var currentRecordErrors = new List<ImportJobError>();
			IAdminObject migrationObject = null;

			try
			{
				migrationObject = await _serializationHelper.DeserializeToAdminObjectAsync(record.MetaData);
			}
			catch (Exception ex)
			{
				var serializationError = new ImportJobError() { Message = Constant.ErrorMessages.GeneralDeSerializationError, Details = ex.ToString(), Type = Constant.ImportUtilityJob.ErrorType.DataLevel, LineNumber = record.ImportRowID };
				currentRecordErrors.Add(serializationError);
			}

			if (migrationObject != null && !currentRecordErrors.Any())
			{
				var violations = new List<String>();
				violations.AddRange(await migrationObject.ValidateAsync(_apiOptions, repositoryGroup, _artifactQueryHelper, eddsDbContext, sqlQueryHelper));
				if (record.JobType == Constant.ImportUtilityJob.JobType.VALIDATE_SUBMIT && !violations.Any())
				{
					violations.AddRange(await migrationObject.ImportAsync(_apiOptions, repositoryGroup, _artifactQueryHelper, helper, eddsDbContext, sqlQueryHelper));
				}

				currentRecordErrors.AddRange(
						violations.Select(x => new ImportJobError() { Message = x, Type = Constant.ImportUtilityJob.ErrorType.DataLevel, LineNumber = record.ImportRowID })
					);
			}
			errors.AddRange(currentRecordErrors);
		}
	}
}
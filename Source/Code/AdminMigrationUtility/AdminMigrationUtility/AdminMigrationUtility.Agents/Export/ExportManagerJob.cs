using AdminMigrationUtility.Emailer;
using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Exceptions;
using AdminMigrationUtility.Helpers.Models;
using AdminMigrationUtility.Helpers.Models.Interfaces;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.Serialization;
using AdminMigrationUtility.Helpers.SQL;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Agents.Export
{
	[kCura.Agent.CustomAttributes.Name("AdminMigrationUtility - Export Manager Job")]
	[System.Runtime.InteropServices.Guid("B9FB37D6-8F70-4F35-A6FB-EC9B2B8B75DF")]
	public class ExportManagerJob : AgentJobBase
	{
		public Int32 ExportJobArtifactId { get; set; }
		public APIOptions RsapiApiOptions { get; set; }
		public IRsapiRepositoryGroup RsapiRepositoryGroup { get; set; }
		public ISerializationHelper SerializationHelper { get; set; }

		public ExportManagerJob(Int32 agentId, IAgentHelper agentHelper, ISqlQueryHelper sqlQueryHelper, DateTime processedOnDateTime, IEnumerable<Int32> resourceGroupIds, IAPILog logger, IArtifactQueries artifactQueries, APIOptions rsapiApiOptions, IRsapiRepositoryGroup rsapiRepositoryGroup, ISerializationHelper serializationHelper)
		{
			TableRowId = 0;
			WorkspaceArtifactId = -1;
			AgentId = agentId;
			AgentHelper = agentHelper;
			SqlQueryHelper = sqlQueryHelper;
			ProcessedOnDateTime = processedOnDateTime;
			QueueTable = Constant.Tables.ExportManagerQueue;
			AgentResourceGroupIds = resourceGroupIds;
			Logger = logger;
			ArtifactQueries = artifactQueries;
			RsapiApiOptions = rsapiApiOptions;
			RsapiRepositoryGroup = rsapiRepositoryGroup;
			SerializationHelper = serializationHelper;
		}

		public override async Task ExecuteAsync()
		{
			if (await IsOffHoursAsync(ProcessedOnDateTime))
			{
				//Check for jobs which stopped unexpectedly on this agent thread
				await ResetUnfinishedExportManagerJobsAsync();

				//Retrieve the next record to work on
				String commaDelimitedListOfResourceIds = GetCommaDelimitedListOfResourceIds(AgentResourceGroupIds);

				//Process the record(s)
				await ProcessNewRecordAsync(commaDelimitedListOfResourceIds);

				RaiseAndLogDebugMessage($"Querying All Manager Jobs [Table = {QueueTable}]");
				var managerJobs = await GetAllManagerJobsAsync();

				RaiseAndLogDebugMessage($"Looking for new cancellation requests [Table = {QueueTable}]");
				await ProcessCancelledJobsAsync(managerJobs);

				RaiseAndLogDebugMessage($"Checking Job Completion [Table = {QueueTable}]");
				await CheckIfWorkersCompletedJob(managerJobs);
			}
			else
			{
				RaiseAndLogDebugMessage($"Current time is not between {OffHoursStartTime} and {OffHoursEndTime}. Agent execution skipped.");
			}
		}

		public async Task ProcessCancelledJobsAsync(IEnumerable<ExportManagerQueueRecord> exportManagerQueueRecords)
		{
			var canceledManagerJobs = exportManagerQueueRecords.Where(x => x.QueueStatus == Constant.Status.Queue.CANCELLATION_REQUESTED);
			foreach (var exportManagerQueueRecord in canceledManagerJobs)
			{
				try
				{
					SetJobProperties(exportManagerQueueRecord);
					if (!String.IsNullOrWhiteSpace(exportManagerQueueRecord.ResultsTableName))
					{
						await DropExportWorkerResultsTableAsync(exportManagerQueueRecord.ResultsTableName);
					}
					await UpdateExportJobStatus(Constant.Status.Job.CANCELLED, exportManagerQueueRecord);
					await ClearRecordsFromQueueTables(exportManagerQueueRecord);
					await SendEmail(exportManagerQueueRecord, Constant.Status.Job.CANCELLED);
				}
				catch (Exception ex)
				{
					RaiseAndLogDebugMessage($"error while attempting to cancel job {exportManagerQueueRecord.ExportJobArtifactId} in workspace {exportManagerQueueRecord.WorkspaceArtifactId}");
					throw;
				}
			}
		}

		public async Task CheckIfWorkersCompletedJob(IEnumerable<ExportManagerQueueRecord> exportManagerQueueRecords)
		{
			var jobsInTheWorkerQueue = exportManagerQueueRecords.Where(x => x.QueueStatus == Constant.Status.Queue.WAITING_FOR_WORKERS_TO_FINISH);
			foreach (var exportManagerQueueRecord in jobsInTheWorkerQueue)
			{
				try
				{
					SetJobProperties(exportManagerQueueRecord);
					Int32 numberOfWorkerRecords = await SqlQueryHelper.CountNumberOfExportWorkerRecordsAsync(
						eddsDbContext: AgentHelper.GetDBContext(-1),
						workspaceArtifactId: exportManagerQueueRecord.WorkspaceArtifactId,
						exportJobArtifactId: exportManagerQueueRecord.ExportJobArtifactId);

					if (numberOfWorkerRecords == 0)
					{
						FileInfo exportFileInfo = await ConstructExportFileNameAsync(exportManagerQueueRecord.ObjectType, exportManagerQueueRecord.ExportJobArtifactId);

						//create export file
						Int32 numberOfRecordsExported = await CreateExportFileAsync(exportManagerQueueRecord, exportFileInfo);

						//attach export file to RDO
						await AttachFileToExportJobAsync(exportManagerQueueRecord, exportFileInfo.FullName);

						//delete export file from temp directory
						await DeleteExportFileAsync(exportFileInfo);

						//drop results table
						await DropExportWorkerResultsTableAsync(exportManagerQueueRecord.ResultsTableName);

						//delete record from export manager queue
						await ClearRecordsFromQueueTables(exportManagerQueueRecord);

						Boolean jobErrorsRecorded = await ArtifactQueries.JobErrorRecordExistsAsync(RsapiApiOptions, exportManagerQueueRecord.WorkspaceArtifactId, RsapiRepositoryGroup.RdoRepository, Constant.Guids.ObjectType.ExportUtilityJobErrors, Constant.Guids.Field.ExportUtilityJobErrors.ExportUtilityJob, exportManagerQueueRecord.ExportJobArtifactId);

						await FinalizeJobStatiaticsAsync(exportManagerQueueRecord, numberOfRecordsExported);
						//update status of export job to complete
						if (jobErrorsRecorded)
						{
							await UpdateExportJobStatus(Constant.Status.Job.COMPLETED_WITH_ERRORS, exportManagerQueueRecord);
							await SendEmail(exportManagerQueueRecord, Constant.Status.Job.COMPLETED_WITH_ERRORS);
						}
						else
						{
							await UpdateExportJobStatus(Constant.Status.Job.COMPLETED, exportManagerQueueRecord);
							await SendEmail(exportManagerQueueRecord, Constant.Status.Job.COMPLETED);
						}
					}
				}
				catch (Exception ex)
				{
					throw new AdminMigrationUtilityException(Constant.ErrorMessages.CheckIfExportWorkersAreStillWorkingOnExportJobInExportWorkerQueueTableError, ex);
				}
			}
		}

		private async Task<List<ExportManagerQueueRecord>> GetAllManagerJobsAsync()
		{
			var retVal = new List<ExportManagerQueueRecord>();
			var dt = await SqlQueryHelper.RetrieveAllExportManagerQueueRecords(AgentHelper.GetDBContext(-1));
			foreach (DataRow row in dt.Rows)
			{
				retVal.Add(new ExportManagerQueueRecord(row));
			}
			return retVal;
		}

		public async Task ResetUnfinishedExportManagerJobsAsync()
		{
			try
			{
				RaiseAndLogDebugMessage($"Resetting records which failed in Export Manager Queue Table.");
				await SqlQueryHelper.ResetUnfishedExportManagerJobsAsync(AgentHelper.GetDBContext(-1), AgentId);
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.ResetUnfinishedJobsInExportManagerQueueTableError, ex);
			}
		}

		public Boolean TableIsNotEmpty(DataTable table)
		{
			return (table != null && table.Rows.Count > 0);
		}

		public async Task<DataTable> RetrieveNextRecordInExportManagerQueueTableAsync(String commaDelimitedListOfResourceIds)
		{
			try
			{
				RaiseAndLogDebugMessage("Retrieving next record in the export manager queue table.");

				DataTable dataTable = await SqlQueryHelper.RetrieveNextRowInExportManagerQueueAsync(
					eddsDbContext: AgentHelper.GetDBContext(-1),
					agentId: AgentId,
					commaDelimitedResourceAgentIds: commaDelimitedListOfResourceIds);

				RaiseAndLogDebugMessage("Retrieved next record in the export manager queue table.");

				return dataTable;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.RetrieveNextRecordInExportManagerQueueTableError, ex);
			}
		}

		public async Task ProcessNewRecordAsync(String delimitedListOfResourceGroupIds)
		{
			if (delimitedListOfResourceGroupIds != String.Empty)
			{
				DataTable dataTable = await RetrieveNextRecordInExportManagerQueueTableAsync(delimitedListOfResourceGroupIds);

				if (TableIsNotEmpty(dataTable))
				{
					ExportManagerQueueRecord exportManagerQueueRecord = new ExportManagerQueueRecord(dataTable.Rows[0]);

					SetJobProperties(exportManagerQueueRecord);

					String context = $"[Table = {QueueTable}, ID = {TableRowId}, Workspace Artifact ID = {WorkspaceArtifactId}, Export Job Artifact ID = {ExportJobArtifactId}]";
					RaiseAndLogDebugMessage($"Processing record(s). {context}");

					try
					{
						Int32 numberOfExpectedExports;

						//update export job status to in progress
						await UpdateExportJobStatus(Constant.Status.Job.IN_PROGRESS_MANAGER, exportManagerQueueRecord);

						//update export manager queue record with export results table name
						String exportResultsTableName = $"{Constant.Names.ExportWorkerResultsTablePrefix}{Guid.NewGuid()}";
						await SqlQueryHelper.UpdateResultsTableNameInExportManagerQueueAsync(AgentHelper.GetDBContext(-1), exportManagerQueueRecord.TableRowId, exportResultsTableName);

						// Add specific logic for other types of objects here #DifferentObject
						Constant.Enums.SupportedObjects migrationObjectType = (Constant.Enums.SupportedObjects)Enum.Parse(typeof(Constant.Enums.SupportedObjects), exportManagerQueueRecord.ObjectType);
						switch (migrationObjectType)
						{
							case Constant.Enums.SupportedObjects.User:
								numberOfExpectedExports = await ProcessUsers(exportManagerQueueRecord, exportResultsTableName);
								break;

							default:
								throw new AdminMigrationUtilityException(Constant.ErrorMessages.InvalidAdminObjectTypeError);
						}

						//Update status and statistics long as cancellation is not requested
						if (!await CancellationRequestedAsync(exportManagerQueueRecord))
						{
							await SqlQueryHelper.UpdateStatusInExportManagerQueueByWorkspaceJobIdAsync(
							eddsDbContext: AgentHelper.GetDBContext(-1),
							workspaceArtifactId: exportManagerQueueRecord.WorkspaceArtifactId,
							exportJobArtifactId: exportManagerQueueRecord.ExportJobArtifactId,
							queueStatus: Constant.Status.Queue.WAITING_FOR_WORKERS_TO_FINISH);

							await UpdateExportJobStatus(Constant.Status.Job.COMPLETED_MANAGER, exportManagerQueueRecord);

							await UpdateExportJobExpectedNumberOfExports(numberOfExpectedExports, exportManagerQueueRecord);
						}
					}
					catch (Exception ex)
					{
						//update export job status to error
						await UpdateExportJobStatus(Constant.Status.Job.ERROR, exportManagerQueueRecord);

						String details = ExceptionMessageFormatter.GetExceptionMessageIncludingAllInnerExceptions(ex);
						RaiseAndLogDebugMessage(details);

						//remove record from export manager queue table
						await ClearRecordsFromQueueTables(exportManagerQueueRecord);
					}
				}
			}
		}

		public void SetJobProperties(ExportManagerQueueRecord exportManagerQueueRecord)
		{
			WorkspaceArtifactId = exportManagerQueueRecord.WorkspaceArtifactId;
			ExportJobArtifactId = exportManagerQueueRecord.ExportJobArtifactId;
			TableRowId = exportManagerQueueRecord.TableRowId;
		}

		public async Task<Int32?> CurrentQueueRecordStatus(ExportManagerQueueRecord exportManagerQueueRecord)
		{
			return await SqlQueryHelper.GetQueueRecordCurrentStatus(
							eddsDbContext: AgentHelper.GetDBContext(-1),
							workspaceIDColumnName: Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceArtifactId,
							workspaceArtifactID: exportManagerQueueRecord.WorkspaceArtifactId,
							JobIDColumnName: Constant.Sql.ColumnsNames.ExportManagerQueue.ExportJobArtifactId,
							jobID: exportManagerQueueRecord.ExportJobArtifactId,
							tableName: Constant.Tables.ExportManagerQueue,
							statusColumnName: Constant.Sql.ColumnsNames.ExportManagerQueue.QueueStatus);
		}

		public async Task UpdateExportJobStatus(String status, ExportManagerQueueRecord exportManagerQueueRecord)
		{
			try
			{
				RaiseAndLogDebugMessage("Updating the status of export job.");

				await ArtifactQueries.UpdateRdoTextFieldValueAsync(
				rsapiApiOptions: RsapiApiOptions,
				workspaceArtifactId: exportManagerQueueRecord.WorkspaceArtifactId,
				rdoRepository: RsapiRepositoryGroup.RdoRepository,
				rdoArtifactId: exportManagerQueueRecord.ExportJobArtifactId,
				jobObjectGuid: Constant.Guids.ObjectType.ExportUtilityJob,
				textFieldGuid: Constant.Guids.Field.ExportUtilityJob.Status,
				fieldValue: status);

				RaiseAndLogDebugMessage("Updated the status of export job.");
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.UpdateExportJobStatusError, ex);
			}
		}

		public async Task CreateExportJobErrorRecordAsync(String objectType, String status, String details)
		{
			try
			{
				await ArtifactQueries.CreateJobErrorRecordAsync(
					rsapiApiOptions: RsapiApiOptions,
					workspaceArtifactId: WorkspaceArtifactId,
					rdoRepository: RsapiRepositoryGroup.RdoRepository,
					rdoGuid: Constant.Guids.ObjectType.ExportUtilityJobErrors,
					jobArtifactId: ExportJobArtifactId,
					jobType: objectType,
					statusFieldValue: status,
					detailsFieldValue: details);
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.CreateExportJobErrorRecordError, ex);
			}
		}

		#region query data

		public async Task ClearRecordsFromQueueTables(ExportManagerQueueRecord exportManagerQueueRecord)
		{
			await SqlQueryHelper.DeleteRecordFromQueueAsync(AgentHelper.GetDBContext(-1), Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceArtifactId, exportManagerQueueRecord.WorkspaceArtifactId, Constant.Sql.ColumnsNames.ExportWorkerQueue.ExportJobArtifactId, exportManagerQueueRecord.ExportJobArtifactId, Constant.Tables.ExportWorkerQueue);
			await SqlQueryHelper.DeleteRecordFromQueueAsync(AgentHelper.GetDBContext(-1), Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceArtifactId, exportManagerQueueRecord.WorkspaceArtifactId, Constant.Sql.ColumnsNames.ExportManagerQueue.ExportJobArtifactId, exportManagerQueueRecord.ExportJobArtifactId, Constant.Tables.ExportManagerQueue);
		}

		#endregion query data

		#region create export file and attach it to export job

		private async Task SendEmail(ExportManagerQueueRecord exportManagerQueueRecord, String status)
		{
			var exportJob = await RetrieveExportJobRDO(exportManagerQueueRecord);
			var emailTo = exportJob[Constant.Guids.Field.ExportUtilityJob.EmailAddresses].ValueAsFixedLengthText;
			var jobName = exportJob[Constant.Guids.Field.ExportUtilityJob.Name].ValueAsFixedLengthText;
			String emailSubject = String.Format(Constant.EmailSubject, "export");
			String emailBody = String.Format(Constant.EmailBody, "Export", jobName, exportManagerQueueRecord.WorkspaceName, exportManagerQueueRecord.WorkspaceArtifactId, status);
			if (!String.IsNullOrWhiteSpace(emailTo))
			{
				await EmailUtility.SendEmail(AgentHelper.GetDBContext(-1), emailTo, emailSubject, emailBody, new SmtpClientSettings(AgentHelper.GetDBContext(-1), SqlQueryHelper));
			}
		}

		private async Task<RDO> RetrieveExportJobRDO(ExportManagerQueueRecord exportManagerQueueRecord)
		{
			return await ArtifactQueries.RetrieveJob(RsapiRepositoryGroup.RdoRepository, RsapiApiOptions, exportManagerQueueRecord.WorkspaceArtifactId, exportManagerQueueRecord.ExportJobArtifactId);
		}

		public async Task UpdateExportJobExpectedNumberOfExports(Int32 expectedNumberOfExports, ExportManagerQueueRecord exportManagerQueueRecord)
		{
			try
			{
				RaiseAndLogDebugMessage("Updating export job expected number of exports.");

				await ArtifactQueries.UpdateExportJobStatitics(RsapiRepositoryGroup.RdoRepository, RsapiApiOptions, exportManagerQueueRecord.WorkspaceArtifactId, exportManagerQueueRecord.ExportJobArtifactId, expectedNumberOfExports, 0, 0, 0);

				RaiseAndLogDebugMessage("Updated export job expected number of exports.");
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.UpdateExportJobExpectedNumberError, ex);
			}
		}

		private async Task FinalizeJobStatiaticsAsync(ExportManagerQueueRecord exportManagerQueueRecord, Int32 actualNumberOfExportedRecords)
		{
			var jobRDO = await RetrieveExportJobRDO(exportManagerQueueRecord);
			var expectedNumberOfExports = jobRDO[Constant.Guids.Field.ExportUtilityJob.Expected].ValueAsWholeNumber.GetValueOrDefault();
			if (expectedNumberOfExports > 0)
			{
				var notExported = (await ArtifactQueries.QueryJobErrors(RsapiApiOptions, RsapiRepositoryGroup.RdoRepository, exportManagerQueueRecord.WorkspaceArtifactId, exportManagerQueueRecord.ExportJobArtifactId)).Count();
				await ArtifactQueries.UpdateExportJobStatitics(
					rdoRepository: RsapiRepositoryGroup.RdoRepository,
					rsapiApiOptions: RsapiApiOptions,
					workspaceArtifactId: exportManagerQueueRecord.WorkspaceArtifactId,
					jobArtifactID: exportManagerQueueRecord.ExportJobArtifactId,
					expectedNumberOfExports: expectedNumberOfExports,
					actualNumberOfExports: actualNumberOfExportedRecords,
					notExported: notExported,
					recordsThatHaveBeenProcessed: expectedNumberOfExports);
			}
		}

		public async Task<List<ExportWorkerResultsTableRecord>> QueryBatchFromExportResultsTableAsync(ExportManagerQueueRecord exportManagerQueueRecord)
		{
			try
			{
				DataTable dataTable = await SqlQueryHelper.RetrieveBatchFromExportWorkerResultsTableAsync(
				eddsDbContext: AgentHelper.GetDBContext(-1),
				agentId: exportManagerQueueRecord.AgentId.GetValueOrDefault(),
				batchSize: Constant.BatchSizes.ExportManagerCSVQuery,
				exportResultsTableName: exportManagerQueueRecord.ResultsTableName);

				List<ExportWorkerResultsTableRecord> exportWorkerResultsTableRecords = new List<ExportWorkerResultsTableRecord>();

				if (dataTable?.Rows != null && dataTable.Rows.Count > 0)
				{
					foreach (DataRow currentDataRow in dataTable.Rows)
					{
						ExportWorkerResultsTableRecord newExportWorkerResultsTableRecord = new ExportWorkerResultsTableRecord(currentDataRow);
						exportWorkerResultsTableRecords.Add(newExportWorkerResultsTableRecord);
					}
				}

				return exportWorkerResultsTableRecords;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.QueryBatchFromExportWorkerResultsTableError, ex);
			}
		}

		public async Task DeleteProcessedRecordsFromExportResultsTableAsync(ExportManagerQueueRecord exportManagerQueueRecord, List<ExportWorkerResultsTableRecord> exportWorkerResultsTableRecords)
		{
			List<Int32> processedResultRecordTableRowIds = exportWorkerResultsTableRecords.OrderBy(x => x.TableRowId).Select(x => x.TableRowId).ToList();
			await SqlQueryHelper.DeleteRecordsFromExportWorkerResultsTableAsync(
				eddsDbContext: AgentHelper.GetDBContext(-1),
				exportResultsTableName: exportManagerQueueRecord.ResultsTableName,
				tableRowIds: processedResultRecordTableRowIds);
		}

		public async Task<Int32> CreateExportFileAsync(ExportManagerQueueRecord exportManagerQueueRecord, FileInfo exportFileInfo)
		{
			try
			{
				Int32 rowCountInExportWorkerResultsTable = await SqlQueryHelper.RetrieveTotalRowCountInExportWorkerResultsTableAsync(AgentHelper.GetDBContext(-1), exportManagerQueueRecord.ResultsTableName);

				if (rowCountInExportWorkerResultsTable == 0)
				{
					throw new AdminMigrationUtilityException(Constant.ErrorMessages.ZeroRowsInExportWorkerResultsTableError);
				}

				//insert headers into the export file
				await WriteColumnHeadersToExportFileAsync(exportFileInfo, exportManagerQueueRecord.ObjectType);

				//insert records in batch into the export file
				var actualNumberOfExports = await InsertExportWorkerResultTableUserRecordsIntoExportFileInBatchesAsync(exportManagerQueueRecord, exportFileInfo);

				return actualNumberOfExports;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.CreateExportFileError, ex);
			}
		}

		public async Task<FileInfo> ConstructExportFileNameAsync(String objectType, Int32 exportJobArtifactId)
		{
			return await Task.Run(() =>
			{
				String exportFileNameWithExtension = String.Format(
					Constant.ExportFile.ExportFileNameWithExtension,
					objectType,
					exportJobArtifactId,
					DateTime.Now.ToString(Constant.ExportFile.ExportFileNameDateTimeFormat)
				);
				String tempPath = Path.GetTempPath();
				String fullyQualifiedExportFileNameWithExtension = $@"{tempPath}{Constant.ExportFile.ExportFileFolderName}\{exportFileNameWithExtension}";
				FileInfo exportFileInfo = new FileInfo(fullyQualifiedExportFileNameWithExtension);

				//delete file if it already exists
				if (exportFileInfo.Directory != null)
				{
					if (exportFileInfo.Directory.Exists)
					{
						if (exportFileInfo.Exists)
						{
							exportFileInfo.Delete();
						}
					}
					else
					{
						//create directory with application name if it doesn't already exist
						DirectoryInfo directoryInfo = new DirectoryInfo(exportFileInfo.Directory.FullName);
						directoryInfo.Create();
					}
				}

				return exportFileInfo;
			});
		}

		public async Task WriteColumnHeadersToExportFileAsync(FileInfo tempFileInfo, String objectTypeEnum)
		{
			try
			{
				using (StreamWriter streamWriter = tempFileInfo.AppendText()) //creates file if it does not already exists
				{
					IAdminObject migrationObject = await Utility.GetImportObjectSelectionAsync(objectTypeEnum);
					String[] headers = (await migrationObject.GetColumnsAsync()).Select(x => x.ColumnName).ToArray();
					String csvFormattedColumns = await Utility.FormatCsvLineAsync(headers);
					//write header
					await streamWriter.WriteLineAsync(csvFormattedColumns);
				}
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.WritingColumnHeadersToExportFileError, ex);
			}
		}

		public async Task AttachFileToExportJobAsync(ExportManagerQueueRecord exportManagerQueueRecord, String fileLocation)
		{
			try
			{
				Int32 fileFieldArtifactId = await ArtifactQueries.RetrieveFieldArtifactIdByGuid(
					rsapiApiOptions: RsapiApiOptions,
					workspaceArtifactId: exportManagerQueueRecord.WorkspaceArtifactId,
					fieldRepository: RsapiRepositoryGroup.FieldRepository,
					fieldGuid: Constant.Guids.Field.ExportUtilityJob.ExportFile
					);

				await ArtifactQueries.AttachFileToExportJob(
				rsapiClient: AgentHelper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser),
				rsapiApiOptions: RsapiApiOptions,
				workspaceArtifactId: exportManagerQueueRecord.WorkspaceArtifactId,
				exportJobArtifactId: exportManagerQueueRecord.ExportJobArtifactId,
				fileFieldArtifactId: fileFieldArtifactId,
				fileLocation: fileLocation
				);
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.AttachFileToExportJobError, ex);
			}
		}

		public async Task DeleteExportFileAsync(FileInfo exportFileInfo)
		{
			try
			{
				await Task.Run(() =>
				{
					if (exportFileInfo.Directory != null && exportFileInfo.Directory.Exists)
					{
						if (exportFileInfo.Exists)
						{
							exportFileInfo.Delete();
						}
					}
				});
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.DeleteTempExportFileError, ex);
			}
		}

		public async Task DropExportWorkerResultsTableAsync(String exportWorkerResultsTableName)
		{
			try
			{
				await SqlQueryHelper.DropTableAsync(AgentHelper.GetDBContext(-1), $"[{exportWorkerResultsTableName}]");
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.DropExportWorkerResultsTableError, ex);
			}
		}

		#endregion create export file and attach it to export job

		#region Users

		public async Task<Int32> InsertExportWorkerResultTableUserRecordsIntoExportFileInBatchesAsync(ExportManagerQueueRecord exportManagerQueueRecord, FileInfo tempFileInfo)
		{
			try
			{
				Int32 actualNumberOfExports = 0;
				//query for records in the results table in batches
				List<ExportWorkerResultsTableRecord> exportWorkerResultsTableRecords = await QueryBatchFromExportResultsTableAsync(exportManagerQueueRecord);

				do
				{
					foreach (ExportWorkerResultsTableRecord currentExportWorkerResultsTableRecord in exportWorkerResultsTableRecords)
					{
						IAdminObject migrationObject = await SerializationHelper.DeserializeToAdminObjectAsync(currentExportWorkerResultsTableRecord.MetaData);
						using (StreamWriter streamWriter = tempFileInfo.AppendText()) //creates file if it does not already exists
						{
							String[] orderedCells = (await migrationObject.ExportAsync()).ToArray();
							String csvFormatedCells = await Utility.FormatCsvLineAsync(orderedCells);
							await streamWriter.WriteLineAsync(csvFormatedCells);
							actualNumberOfExports++;
						}
					}

					//delete processed records from export results table
					await DeleteProcessedRecordsFromExportResultsTableAsync(exportManagerQueueRecord, exportWorkerResultsTableRecords);

					//query for records in the results table in batches
					exportWorkerResultsTableRecords = await QueryBatchFromExportResultsTableAsync(exportManagerQueueRecord);
				} while (exportWorkerResultsTableRecords.Count > 0);
				return actualNumberOfExports;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.WritingResultRecordsToExportFileError, ex);
			}
		}

		public async Task<Int32> ProcessUsers(ExportManagerQueueRecord exportManagerQueueRecord, String exportResultTableName)
		{
			Int32 retVal;
			try
			{
				//query all users
				var userResults = await QueryAllUserArtifactIDs();
				retVal = userResults.TotalCount;
				await InsertUserArtifactIDBatchIntoExportWorkerQueueTableAsync(exportManagerQueueRecord, exportResultTableName, userResults);
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.ExportManagerJobUserError, ex);
			}

			return retVal;
		}

		public async Task<QueryResultSet<kCura.Relativity.Client.DTOs.User>> QueryAllUserArtifactIDs()
		{
			try
			{
				return await ArtifactQueries.QueryAllUsersArtifactIDsAsync(
					rsapiApiOptions: RsapiApiOptions,
					userRepository: RsapiRepositoryGroup.UserRepository);
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.QueryAllUserAdminObjectsError, ex);
			}
		}

		public async Task<Boolean> CancellationRequestedAsync(ExportManagerQueueRecord exportManagerQueueRecord)
		{
			return (await CurrentQueueRecordStatus(exportManagerQueueRecord)) == Constant.Status.Queue.CANCELLATION_REQUESTED;
		}

		public async Task InsertUserArtifactIDBatchIntoExportWorkerQueueTableAsync(ExportManagerQueueRecord exportManagerQueueRecord, String exportResultsTableName, QueryResultSet<kCura.Relativity.Client.DTOs.User> users)
		{
			var currentTime = DateTime.UtcNow;

			var dt = new DataTable();
			dt.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceArtifactId, typeof(Int32));
			dt.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.ExportJobArtifactId, typeof(Int32));
			dt.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.ObjectType, typeof(String));
			dt.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.Priority, typeof(Int32));
			dt.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceResourceGroupArtifactId, typeof(Int32));
			dt.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.QueueStatus, typeof(Int32));
			dt.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.ArtifactId, typeof(Int32));
			dt.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.ResultsTableName, typeof(String));
			dt.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.MetaData, typeof(String));
			dt.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.TimeStampUtc, typeof(DateTime));

			try
			{
				foreach (var user in users.Results)
				{
					dt.Rows.Add(exportManagerQueueRecord.WorkspaceArtifactId, exportManagerQueueRecord.ExportJobArtifactId, exportManagerQueueRecord.ObjectType, exportManagerQueueRecord.Priority, exportManagerQueueRecord.WorkspaceResourceGroupArtifactId, Constant.Status.Queue.NOT_STARTED, user.Artifact.ArtifactID, exportResultsTableName, String.Empty, currentTime);
				}
				await SqlQueryHelper.BulkInsertIntoTableAsync(AgentHelper.GetDBContext(-1), dt, Constant.Tables.ExportWorkerQueue, Constant.BatchSizes.ExportManagerIntoWorkerQueue);
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.UnableToInsertUserArtifactIDsIntoWorkerQueue, ex);
			}
		}

		#endregion Users
	}
}
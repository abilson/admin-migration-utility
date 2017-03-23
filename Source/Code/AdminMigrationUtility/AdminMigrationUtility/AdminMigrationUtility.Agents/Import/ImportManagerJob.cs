using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Exceptions;
using AdminMigrationUtility.Helpers.Models;
using AdminMigrationUtility.Helpers.Models.Interfaces;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.Serialization;
using AdminMigrationUtility.Helpers.SQL;
using AdminMigrationUtility.Parser;
using AdminMigrationUtility.Parser.Interfaces;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("AdminMigrationUtility.Agents.NUnit")]

namespace AdminMigrationUtility.Agents.Import
{
	[kCura.Agent.CustomAttributes.Name("AdminMigrationUtility - Import Manager Job")]
	[System.Runtime.InteropServices.Guid("2E0BB548-CD38-4A88-B107-264AB7CABC07")]
	public class ImportManagerJob : AgentJobBase, IDisposable
	{
		private bool _disposed;
		private String _localTempFilePath;
		private IRsapiRepositoryGroup _rsapiRepositoryGroup;
		private APIOptions _apiOptions;
		private IRSAPIClient _rsapiClient;
		private IArtifactQueries _artifactQueryHelper;
		private ISerializationHelper _serializationHelper;

		public String LocalTempFilePath
		{
			get
			{
				if (_localTempFilePath == null)
				{
					_localTempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
					while (File.Exists(_localTempFilePath))
					{
						_localTempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
					}
				}
				return _localTempFilePath;
			}
		}

		public ImportManagerJob(Int32 agentId, IAgentHelper agentHelper, ISqlQueryHelper sqlQueryHelper, IArtifactQueries artifactQueryHelper, DateTime processedOnDateTime, IEnumerable<Int32> resourceGroupIds, IAPILog logger, IRSAPIClient rsapiClient, IRsapiRepositoryGroup rsapiRepositoryGroup, APIOptions apiOptions, ISerializationHelper serializationHelper)
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
			_rsapiRepositoryGroup = rsapiRepositoryGroup;
			_apiOptions = apiOptions;
			_rsapiClient = rsapiClient;
			_artifactQueryHelper = artifactQueryHelper;
			_serializationHelper = serializationHelper;
		}

		public override async Task ExecuteAsync()
		{
			if (await IsOffHoursAsync(ProcessedOnDateTime))
			{
				RaiseAndLogDebugMessage($"Resetting records which failed. [Table = {QueueTable}]");
				await ResetUnfinishedImportManagerJobsAsync();

				RaiseAndLogDebugMessage($"Retrieving next record(s) in the queue. [Table = {QueueTable}]");
				var delimitedListOfResourceGroupIds = GetCommaDelimitedListOfResourceIds(AgentResourceGroupIds);

				RaiseAndLogDebugMessage($"Looking for new jobs to process [Table = {QueueTable}]");
				await PopulateWorkerQueueRecordsAsync(delimitedListOfResourceGroupIds);

				RaiseAndLogDebugMessage($"Querying All Manager Jobs [Table = {QueueTable}]");
				var managerJobs = await GetAllManagerJobsAsync();

				RaiseAndLogDebugMessage($"Looking for new cancellation requests [Table = {QueueTable}]");
				await CheckForCancelledJobsAsync(managerJobs);

				RaiseAndLogDebugMessage($"Checking Job Completion [Table = {QueueTable}]");
				await CheckForCompletedJobsAsync(managerJobs);
			}
			else
			{
				RaiseAndLogDebugMessage($"Current time is not between {OffHoursStartTime} and {OffHoursEndTime}. Agent execution skipped.");
			}
		}

		private async Task ResetUnfinishedImportManagerJobsAsync()
		{
			await SqlQueryHelper.ResetUnfishedImportManagerJobsAsync(AgentHelper.GetDBContext(-1), AgentId);
		}

		private Boolean TableIsNotEmpty(DataTable table)
		{
			return (table != null && table.Rows.Count > 0);
		}

		public async Task<DataTable> RetrieveNextAsync(String delimitedListOfResourceGroupIds)
		{
			DataTable next = await SqlQueryHelper.RetrieveNextInImportManagerQueueAsync(AgentHelper.GetDBContext(-1), AgentId, delimitedListOfResourceGroupIds);
			return next;
		}

		private async Task PopulateWorkerQueueRecordsAsync(String delimitedListOfResourceGroupIds)
		{
			IParser parser = null;
			FileStream downloadedFile = null;
			ImportManagerQueueRecord managerQueueRecord = null;
			RDO importJob;
			var violations = new List<ImportJobError>();
			try
			{
				DataTable next = await RetrieveNextAsync(delimitedListOfResourceGroupIds);

				if (TableIsNotEmpty(next))
				{
					managerQueueRecord = new ImportManagerQueueRecord(next.Rows[0]);
					SetJobProperties(managerQueueRecord);
					await UpdateJobStatusAsync(managerQueueRecord.WorkspaceArtifactId, managerQueueRecord.JobId, Constant.Status.Job.IN_PROGRESS_MANAGER);

					RaiseAndLogDebugMessage($"Initializing Job {managerQueueRecord.JobId}. [Table = {QueueTable}, ID = {managerQueueRecord.RecordId}, Workspace Artifact ID = {managerQueueRecord.WorkspaceArtifactId}]");
					importJob = await GetImportJob(managerQueueRecord.WorkspaceArtifactId, managerQueueRecord.JobId);
					var jobFileField = importJob[Constant.Guids.Field.ImportUtilityJob.ImportFile];

					RaiseAndLogDebugMessage($"Downloading File {jobFileField.ValueAsFixedLengthText}. [Table = {QueueTable}, ID = {managerQueueRecord.RecordId}, Workspace Artifact ID = {managerQueueRecord.WorkspaceArtifactId}]");
					await DownloadFile(managerQueueRecord.WorkspaceArtifactId, managerQueueRecord.JobId, jobFileField.ArtifactID, LocalTempFilePath);

					RaiseAndLogDebugMessage($"Opening File {LocalTempFilePath}. [Table = {QueueTable}, ID = {managerQueueRecord.RecordId}, Workspace Artifact ID = {managerQueueRecord.WorkspaceArtifactId}]");
					downloadedFile = await OpenFileAsync(LocalTempFilePath);

					RaiseAndLogDebugMessage($"Creating Delimited File Parser. [Table = {QueueTable}, ID = {managerQueueRecord.RecordId}, Workspace Artifact ID = {managerQueueRecord.WorkspaceArtifactId}]");
					parser = await CreateParserAsync(downloadedFile, violations);

					RaiseAndLogDebugMessage($"Creating Import Object. [Table = {QueueTable}, ID = {managerQueueRecord.RecordId}, Workspace Artifact ID = {managerQueueRecord.WorkspaceArtifactId}]");
					//var migrationObject = await Utility.GetImportObjectSelection(managerQueueRecord.ObjectType, violations);
					var migrationObject = await GetMigrationObject(managerQueueRecord.ObjectType, violations);

					RaiseAndLogDebugMessage($"Validating load file columns. [Table = {QueueTable}, ID = {managerQueueRecord.RecordId}, Workspace Artifact ID = {managerQueueRecord.WorkspaceArtifactId}]");
					await ValidateColumnsAsync(migrationObject, parser, violations);
					await ValidateFileDoesNotContainExtraColumnsAsync(migrationObject, parser, managerQueueRecord.ObjectType, violations);

					RaiseAndLogDebugMessage($"Validating load file minimum data requirement. [Table = {QueueTable}, ID = {managerQueueRecord.RecordId}, Workspace Artifact ID = {managerQueueRecord.WorkspaceArtifactId}]");
					await ValidateFileHasMetaData(parser, violations);

					if (violations.Any())
					{
						RaiseAndLogDebugMessage($"Violations found, recording them in the Job's error log. [Table = {QueueTable}, ID = {managerQueueRecord.RecordId}, Workspace Artifact ID = {managerQueueRecord.WorkspaceArtifactId}]");
						await _artifactQueryHelper.CreateImportJobErrorRecordsAsync(_apiOptions, managerQueueRecord.WorkspaceArtifactId, managerQueueRecord.JobId, _rsapiRepositoryGroup.RdoRepository, violations, managerQueueRecord.ObjectType);
						await UpdateJobStatusAsync(managerQueueRecord.WorkspaceArtifactId, managerQueueRecord.JobId, Constant.Status.Job.ERROR);
						await ClearQueueRecords(managerQueueRecord.WorkspaceArtifactId, managerQueueRecord.JobId);
						await SendEmail(managerQueueRecord, Constant.Status.Job.ERROR);
					}
					else
					{
						try
						{
							RaiseAndLogDebugMessage($"Populating Worker Queue Table. [Table = {QueueTable}, ID = {managerQueueRecord.RecordId}, Workspace Artifact ID = {managerQueueRecord.WorkspaceArtifactId}]");
							var numberProcessed = await PopulateWorkerQueueAsync(managerQueueRecord, parser, migrationObject, violations);
							await UpdateJobStatisticsAsync(managerQueueRecord, numberProcessed);
							await _artifactQueryHelper.CreateImportJobErrorRecordsAsync(_apiOptions, managerQueueRecord.WorkspaceArtifactId, managerQueueRecord.JobId, _rsapiRepositoryGroup.RdoRepository, violations, managerQueueRecord.ObjectType);
							await SqlQueryHelper.UpdateQueueStatusAsync(AgentHelper.GetDBContext(-1), managerQueueRecord.WorkspaceArtifactId, managerQueueRecord.JobId, Constant.Tables.ImportManagerQueue, Constant.Sql.ColumnsNames.ImportManagerQueue.QueueStatus, Constant.Status.Queue.WAITING_FOR_WORKERS_TO_FINISH);
							await UpdateJobStatusAsync(managerQueueRecord.WorkspaceArtifactId, managerQueueRecord.JobId, Constant.Status.Job.COMPLETED_MANAGER);
						}
						catch (Exception ex)
						{
							var error = new ImportJobError() { Message = ex.Message, Type = Constant.ImportUtilityJob.ErrorType.FileLevel, Details = ex.ToString(), LineNumber = null };
							await _artifactQueryHelper.CreateImportJobErrorRecordsAsync(_apiOptions, managerQueueRecord.WorkspaceArtifactId, managerQueueRecord.JobId, _rsapiRepositoryGroup.RdoRepository, new[] { error }, managerQueueRecord.ObjectType);
							await UpdateJobStatusAsync(managerQueueRecord.WorkspaceArtifactId, managerQueueRecord.JobId, Constant.Status.Job.ERROR);
							await ClearQueueRecords(managerQueueRecord.WorkspaceArtifactId, managerQueueRecord.JobId);
							await SendEmail(managerQueueRecord, Constant.Status.Job.ERROR);
						}
					}
				}
				else
				{
					RaiseAndLogDebugMessage("No records in the queue for this resource pool.");
				}
			}
			catch (Exception ex)
			{
				if (managerQueueRecord != null)
				{
					var error = new ImportJobError() { Message = ex.Message, Type = Constant.ImportUtilityJob.ErrorType.JobLevel, LineNumber = null, Details = ex.ToString() };
					await _artifactQueryHelper.CreateImportJobErrorRecordsAsync(_apiOptions, managerQueueRecord.WorkspaceArtifactId, managerQueueRecord.JobId, _rsapiRepositoryGroup.RdoRepository, new[] { error }, managerQueueRecord.ObjectType);
					await UpdateJobStatusAsync(managerQueueRecord.WorkspaceArtifactId, managerQueueRecord.JobId, Constant.Status.Job.RETRY);
				}
				throw;
			}
			finally
			{
				if (parser != null)
				{
					parser.Dispose();
				}

				if (downloadedFile != null)
				{
					downloadedFile.Close();
				}
			}
		}

		private void SetJobProperties(ImportManagerQueueRecord importManagerQueueRecord)
		{
			WorkspaceArtifactId = importManagerQueueRecord.WorkspaceArtifactId;
			TableRowId = importManagerQueueRecord.RecordId;
		}

		private async Task SendEmail(ImportManagerQueueRecord job, String status)
		{
			var importJob = await GetImportJob(job.WorkspaceArtifactId, job.JobId);
			var emailTo = importJob[Constant.Guids.Field.ImportUtilityJob.EmailAddresses].ValueAsFixedLengthText;
			var jobName = importJob[Constant.Guids.Field.ImportUtilityJob.Name].ValueAsFixedLengthText;
			String emailSubject = String.Format(Constant.EmailSubject, "import");
			String emailBody = String.Format(Constant.EmailBody, "Import", jobName, job.WorkspaceName, job.WorkspaceArtifactId, status);
			if (!String.IsNullOrWhiteSpace(emailTo))
			{
				await Emailer.EmailUtility.SendEmail(AgentHelper.GetDBContext(-1), emailTo, emailSubject, emailBody, new SmtpClientSettings(AgentHelper.GetDBContext(-1), SqlQueryHelper));
			}
		}

		private async Task<IAdminObject> GetMigrationObject(String objectType, List<ImportJobError> violations)
		{
			IAdminObject migrationObject = null;
			try
			{
				migrationObject = await Utility.GetImportObjectSelectionAsync(objectType);
			}
			catch (Exception ex)
			{
				var error = new ImportJobError() { Message = ex.Message, Type = Constant.ImportUtilityJob.ErrorType.JobLevel, Details = ex.ToString(), LineNumber = null };
				violations.Add(error);
			}
			return migrationObject;
		}

		private async Task CheckForCancelledJobsAsync(IEnumerable<ImportManagerQueueRecord> managerJobs)
		{
			if (managerJobs != null && managerJobs.Any())
			{
				var cancelledJobs = managerJobs.Where(x => x.QueueStatus == Constant.Status.Queue.CANCELLATION_REQUESTED);
				if (cancelledJobs.Any())
				{
					foreach (var job in cancelledJobs)
					{
						SetJobProperties(job);
						IEnumerable<ImportJobError> jobErrors = await _artifactQueryHelper.GetImportJobErrorsAsync(_apiOptions, job.WorkspaceArtifactId, _rsapiRepositoryGroup.RdoRepository, job.JobId);
						Int32 numberOfWorkerRecords = await SqlQueryHelper.CountImportWorkerRecordsAsync(AgentHelper.GetDBContext(-1), job.WorkspaceArtifactId, job.JobId);
						await UpdateJobStatisticsAsync(job, jobErrors, numberOfWorkerRecords);
						await UpdateJobStatusAsync(job.WorkspaceArtifactId, job.JobId, Constant.Status.Job.CANCELLED);
						await ClearQueueRecords(job.WorkspaceArtifactId, job.JobId);
						await SendEmail(job, Constant.Status.Job.CANCELLED);
					}
				}
			}
		}

		private async Task CheckForCompletedJobsAsync(IEnumerable<ImportManagerQueueRecord> managerJobs)
		{
			if (managerJobs != null && managerJobs.Any())
			{
				var jobsInProgress = managerJobs.Where(x => x.QueueStatus == Constant.Status.Queue.WAITING_FOR_WORKERS_TO_FINISH);
				if (jobsInProgress.Any())
				{
					foreach (var job in jobsInProgress)
					{
						SetJobProperties(job);
						Int32 numberOfWorkerRecords = await SqlQueryHelper.CountImportWorkerRecordsAsync(AgentHelper.GetDBContext(-1), job.WorkspaceArtifactId, job.JobId);
						if (numberOfWorkerRecords == 0)
						{
							IEnumerable<ImportJobError> jobErrors = await _artifactQueryHelper.GetImportJobErrorsAsync(_apiOptions, job.WorkspaceArtifactId, _rsapiRepositoryGroup.RdoRepository, job.JobId);

							// Job Statistics should only be updated when they job type is validate & submit
							if (job.JobType == Constant.ImportUtilityJob.JobType.VALIDATE_SUBMIT)
							{
								await UpdateJobStatisticsAsync(job, jobErrors, numberOfWorkerRecords);
							}
							if (jobErrors.Any())
							{
								await UpdateJobStatusAsync(job.WorkspaceArtifactId, job.JobId, Constant.Status.Job.COMPLETED_WITH_ERRORS);
								await SendEmail(job, Constant.Status.Job.COMPLETED_WITH_ERRORS);
							}
							else
							{
								await UpdateJobStatusAsync(job.WorkspaceArtifactId, job.JobId, Constant.Status.Job.COMPLETED);
								await SendEmail(job, Constant.Status.Job.COMPLETED);
							}
							await ClearQueueRecords(job.WorkspaceArtifactId, job.JobId);
						}
					}
				}
			}
		}

		private async Task UpdateJobStatisticsAsync(ImportManagerQueueRecord managerQueueRecord, IEnumerable<ImportJobError> jobErrors, Int32 numberOfWorkerRecords)
		{
			var job = await GetImportJob(managerQueueRecord.WorkspaceArtifactId, managerQueueRecord.JobId);
			var expectedNumberOfImports = job[Constant.Guids.Field.ImportUtilityJob.Expected].ValueAsWholeNumber.GetValueOrDefault();
			var imported = Utility.CalculateImportJobImports(jobErrors, expectedNumberOfImports, numberOfWorkerRecords);
			var notImported = Utility.CalculateImportJobObjectsThatWereNotImported(jobErrors, expectedNumberOfImports, numberOfWorkerRecords);
			await _artifactQueryHelper.UpdateImportJobStatistics(_rsapiRepositoryGroup.RdoRepository, _apiOptions, managerQueueRecord.WorkspaceArtifactId, managerQueueRecord.JobId, imported, notImported);
		}

		private async Task ClearQueueRecords(Int32 workspaceArtifactID, Int32 jobArtifactID)
		{
			await SqlQueryHelper.DeleteRecordFromQueueAsync(AgentHelper.GetDBContext(-1), Constant.Sql.ColumnsNames.ImportWorkerQueue.WorkspaceArtifactID, workspaceArtifactID, Constant.Sql.ColumnsNames.ImportWorkerQueue.JobID, jobArtifactID, Constant.Tables.ImportWorkerQueue);
			await SqlQueryHelper.DeleteRecordFromQueueAsync(AgentHelper.GetDBContext(-1), Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceArtifactID, workspaceArtifactID, Constant.Sql.ColumnsNames.ImportManagerQueue.JobID, jobArtifactID, Constant.Tables.ImportManagerQueue);
		}

		private async Task<List<ImportManagerQueueRecord>> GetAllManagerJobsAsync()
		{
			var retVal = new List<ImportManagerQueueRecord>();
			var dt = await SqlQueryHelper.RetrieveAllImportManagerQueueRecords(AgentHelper.GetDBContext(-1));
			foreach (DataRow row in dt.Rows)
			{
				retVal.Add(new ImportManagerQueueRecord(row));
			}
			return retVal;
		}

		private async Task<RDO> GetImportJob(Int32 workspaceArtifactID, Int32 jobArtifactID)
		{
			return await _artifactQueryHelper.RetrieveJob(_rsapiRepositoryGroup.RdoRepository, _apiOptions, workspaceArtifactID, jobArtifactID);
		}

		private async Task DownloadFile(Int32 workspaceArtifactID, Int32 jobArtifactID, Int32 fileFieldArtifactID, String fullPath)
		{
			await _artifactQueryHelper.DownloadFile(_rsapiClient, _apiOptions, workspaceArtifactID, jobArtifactID, fileFieldArtifactID, fullPath);
		}

		private async Task<FileStream> OpenFileAsync(String fullFilePath)
		{
			return await Task.Run(() =>
			{
				try
				{
					return File.Open(fullFilePath, FileMode.Open);
				}
				catch (Exception ex)
				{
					throw new AdminMigrationUtilityException(Constant.ErrorMessages.UnableToOpenFileError, ex);
				}
			});
		}

		private async Task<IParser> CreateParserAsync(FileStream downloadedFile, List<ImportJobError> violations)
		{
			return await Task.Run(() =>
			{
				DelimitedFileParser retVal = null;
				try
				{
					retVal = new DelimitedFileParser(downloadedFile, Constant.CommaSeparator);
				}
				catch (Exception ex)
				{
					var error = new ImportJobError() { Message = ex.Message, Details = ex.ToString(), Type = Constant.ImportUtilityJob.ErrorType.FileLevel, LineNumber = null };
					violations.Add(error);
				}
				return retVal;
			});
		}

		private async Task ValidateColumnsAsync(IAdminObject migrationObject, IParser parser, List<ImportJobError> violations)
		{
			if (migrationObject != null && parser != null)
			{
				var missingColumns = new List<String>();
				var requiredColumns = (await migrationObject.GetColumnsAsync()).Select(x => x.ColumnName).ToList();
				var loadFileColumns = parser.ParseColumns();

				foreach (var requiredColumn in requiredColumns)
				{
					if (!loadFileColumns.Contains(requiredColumn))
					{
						missingColumns.Add(requiredColumn);
					}
				}

				if (missingColumns.Any())
				{
					var formattedMissingColumnList = String.Join(Constant.CommaSeparator, missingColumns);
					violations.Add(new ImportJobError() { Message = String.Format(Constant.ErrorMessages.MissingColumnsError, formattedMissingColumnList), LineNumber = null, Type = Constant.ImportUtilityJob.ErrorType.FileLevel });
				}
			}
		}

		private async Task ValidateFileDoesNotContainExtraColumnsAsync(IAdminObject migrationObject, IParser parser, String migrationObjectType, List<ImportJobError> violations)
		{
			if (migrationObject != null && parser != null)
			{
				var requiredColumns = (await migrationObject.GetColumnsAsync()).Select(x => x.ColumnName).ToList();
				var loadFileColumns = parser.ParseColumns();

				var extraColumns = loadFileColumns.Except(requiredColumns);

				if (extraColumns.Any())
				{
					var formattedExtraColumnList = String.Join(Constant.CommaSeparator, extraColumns);
					violations.Add(new ImportJobError() { Message = String.Format(Constant.ErrorMessages.ExtraColumnsError, migrationObjectType, formattedExtraColumnList), LineNumber = null, Type = Constant.ImportUtilityJob.ErrorType.FileLevel });
				}
			}
		}

		private async Task ValidateFileHasMetaData(IParser parser, List<ImportJobError> violations)
		{
			await Task.Run(() =>
			{
				if (parser != null && parser.EndOfData)
				{
					violations.Add(new ImportJobError() { Message = Constant.ErrorMessages.FileContainsColumnsButNoMetaDataError, LineNumber = null, Type = Constant.ImportUtilityJob.ErrorType.DataLevel });
				}
			});
		}

		private async Task<Int32> PopulateWorkerQueueAsync(ImportManagerQueueRecord managerQueueRecord, IParser parser, IAdminObject migrationObject, List<ImportJobError> violations)
		{
			var loadFileColumns = parser.ParseColumns();
			var workerQueue = new Dictionary<long, String>();
			Int32 lineNumber = LongToInt(parser.LineNumber);
			var timeOfInsert = DateTime.UtcNow;
			try
			{
				while (parser.Read())
				{
					try
					{
						var adminObjectDictionary = new Dictionary<String, String>();
						foreach (var column in loadFileColumns)
						{
							var value = parser[column] == null ? String.Empty : parser[column].ToString();
							adminObjectDictionary.Add(column, value);
						}
						await migrationObject.MapObjectColumnsAsync(adminObjectDictionary);

						var serializedJson = await _serializationHelper.SerializeAdminObjectAsync(migrationObject);

						workerQueue.Add(lineNumber, serializedJson);
					}
					catch (Exception ex)
					{
						violations.Add(new ImportJobError() { Message = String.Format(Constant.ErrorMessages.ImportQueueManagerLineNumberError, lineNumber, ex.Message), LineNumber = lineNumber >= Int32.MaxValue ? null : (Int32?)lineNumber, Type = Constant.ImportUtilityJob.ErrorType.DataLevel, Details = ex.ToString() });
					}

					if ((workerQueue.Count % Constant.BatchSizes.ImportManagerIntoWorkerQueue) == 0)
					{
						await WriteQueueRecordsAsync(managerQueueRecord, workerQueue, violations, timeOfInsert);
						workerQueue.Clear();
					}
					//The parser returns the current line from the Read above and advances the cursor to the next line. We want our logs should report the current line number of data values so linenumber should be updated last.
					//parser.LineNumber is -1 when the parser get to the end of the file and we don't want to report that value
					if (parser.LineNumber != -1)
					{
						lineNumber = LongToInt(parser.LineNumber);
					}
				}
				if (workerQueue.Any())
				{
					await WriteQueueRecordsAsync(managerQueueRecord, workerQueue, violations, timeOfInsert);
				}
				return (lineNumber - 1) < 0 ? 0 : lineNumber - 1; //minus 1 because the header row should not be counted
			}
			catch (Exception ex)
			{
				// This is thrown instead of being recorded in the violation list because parser errors indicate load file issues, the user should updated the file before attempting the job again
				throw new AdminMigrationUtilityException(String.Format(Constant.ErrorMessages.UnableToParseLineNumberError, lineNumber), ex);
			}
		}

		private Int32 LongToInt(long value)
		{
			//Highly unlikely to be over 2 billion users, matters, clients or groups in a Relativity instance, we will continue to report the max Int value if there is
			return value >= Int32.MaxValue ? Int32.MaxValue : (Int32)value;
		}

		private async Task WriteQueueRecordsAsync(ImportManagerQueueRecord managerQueueRecord, Dictionary<long, String> queueRecordToPopulate, List<ImportJobError> violations, DateTime timeOfInsert)
		{
			try
			{
				var dt = new DataTable();
				dt.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.JobID, typeof(Int32));
				dt.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.WorkspaceArtifactID, typeof(Int32));
				dt.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.ObjectType, typeof(String));
				dt.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.JobType, typeof(String));
				dt.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.MetaData, typeof(String));
				dt.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.ImportRowID, typeof(Int32));
				dt.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.QueueStatus, typeof(Int32));
				dt.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.Priority, typeof(Int32));
				dt.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.ResourceGroupID, typeof(Int32));
				dt.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.TimeStampUTC, typeof(DateTime));

				foreach (var record in queueRecordToPopulate)
				{
					dt.Rows.Add(managerQueueRecord.JobId, managerQueueRecord.WorkspaceArtifactId, managerQueueRecord.ObjectType, managerQueueRecord.JobType, record.Value, record.Key, Constant.Status.Queue.NOT_STARTED, managerQueueRecord.Priority, managerQueueRecord.ResourceGroupId, timeOfInsert);
				}

				await SqlQueryHelper.BulkInsertIntoTableAsync(AgentHelper.GetDBContext(-1), dt, Constant.Tables.ImportWorkerQueue, Constant.BatchSizes.ImportManagerIntoWorkerQueue);
			}
			catch (Exception ex)
			{
				var beginErrorLine = queueRecordToPopulate.Keys.Min();
				var endErrorLine = queueRecordToPopulate.Keys.Max();
				violations.Add(new ImportJobError() { Message = String.Format(Constant.ErrorMessages.ImportQueueManagerPopulatingImportWorkerQueueError, beginErrorLine, endErrorLine), LineNumber = null, Type = Constant.ImportUtilityJob.ErrorType.DataLevel, Details = ex.ToString() });
			}
		}

		private async Task UpdateJobStatusAsync(Int32 workspaceArtifactID, Int32 jobArtifactID, String status)
		{
			await _artifactQueryHelper.UpdateRdoTextFieldValueAsync(_apiOptions, workspaceArtifactID, _rsapiRepositoryGroup.RdoRepository, jobArtifactID, Constant.Guids.ObjectType.ImportUtilityJob, Constant.Guids.Field.ImportUtilityJob.Status, status);
		}

		private async Task UpdateJobStatisticsAsync(ImportManagerQueueRecord managerQueueRecord, Int32 expectedNumberOfImports)
		{
			if (managerQueueRecord.JobType == Constant.ImportUtilityJob.JobType.VALIDATE_SUBMIT)
			{
				await _artifactQueryHelper.UpdateRdoTextFieldValueAsync(_apiOptions, managerQueueRecord.WorkspaceArtifactId, _rsapiRepositoryGroup.RdoRepository, managerQueueRecord.JobId, Constant.Guids.ObjectType.ImportUtilityJob, Constant.Guids.Field.ImportUtilityJob.Expected, expectedNumberOfImports);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (File.Exists(LocalTempFilePath))
					{
						try
						{
							File.Delete(LocalTempFilePath);
						}
						catch (Exception)
						{
							RaiseAndLogDebugMessage("Unable to delete tempor file: " + LocalTempFilePath);
						}
					}
				}
			}
			_disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
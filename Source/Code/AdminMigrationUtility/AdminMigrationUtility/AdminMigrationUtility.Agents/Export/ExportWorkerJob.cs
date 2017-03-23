using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Exceptions;
using AdminMigrationUtility.Helpers.Kepler;
using AdminMigrationUtility.Helpers.Models;
using AdminMigrationUtility.Helpers.Models.AdminObject;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.Serialization;
using AdminMigrationUtility.Helpers.SQL;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Services.Security.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Agents.Export
{
	[kCura.Agent.CustomAttributes.Name("AdminMigrationUtility - Export Worker Job")]
	[System.Runtime.InteropServices.Guid("091BCD99-D044-412C-B8DB-C6C03C6F5C01")]
	public class ExportWorkerJob : AgentJobBase
	{
		public String BatchTableName => _batchTableName ?? (_batchTableName = "[" + Constant.Names.TablePrefix + "ExportWorker_" + Guid.NewGuid() + "_" + AgentId + "]");
		private String _batchTableName;
		public Int32 ExportJobArtifactId { get; set; }
		public APIOptions RsapiApiOptions { get; set; }
		public IRsapiRepositoryGroup RsapiRepositoryGroup { get; set; }
		public IAuthenticationHelper AuthenticationHelper { get; set; }
		public ISerializationHelper SerializationHelper { get; set; }

		public ExportWorkerJob(Int32 agentId, IAgentHelper agentHelper, ISqlQueryHelper sqlQueryHelper, DateTime processedOnDateTime, IEnumerable<Int32> resourceGroupIds, IAPILog logger, IArtifactQueries artifactQueries, APIOptions rsapiApiOptions, IRsapiRepositoryGroup rsapiRepositoryGroup, IAuthenticationHelper authenticationHelper, ISerializationHelper serializationHelper)
		{
			TableRowId = 0;
			WorkspaceArtifactId = -1;
			AgentId = agentId;
			AgentHelper = agentHelper;
			SqlQueryHelper = sqlQueryHelper;
			QueueTable = Constant.Tables.ExportWorkerQueue;
			ProcessedOnDateTime = processedOnDateTime;
			AgentResourceGroupIds = resourceGroupIds;
			Logger = logger;
			ArtifactQueries = artifactQueries;
			RsapiApiOptions = rsapiApiOptions;
			RsapiRepositoryGroup = rsapiRepositoryGroup;
			AuthenticationHelper = authenticationHelper;
			SerializationHelper = serializationHelper;
		}

		public override async Task ExecuteAsync()
		{
			try
			{
				Boolean isOffHours = await IsOffHoursAsync(ProcessedOnDateTime);

				if (!isOffHours)
				{
					RaiseAndLogDebugMessage($"Current time is not between {OffHoursStartTime} and {OffHoursEndTime}. Agent execution skipped.");
				}
				else
				{
					//Check for jobs which stopped unexpectedly on this agent thread and reset them
					await ResetUnfinishedExportWorkerJobsAsync();

					//Retrieve the next record to work on
					String commaDelimitedListOfResourceIds = GetCommaDelimitedListOfResourceIds(AgentResourceGroupIds);
					if (commaDelimitedListOfResourceIds == String.Empty)
					{
						RaiseAndLogDebugMessage("This agent server is not part of any resource pools.  Agent execution skipped.");
					}
					else
					{
						DataTable batchDataTable = await RetrieveNextBatchOfRecordsInExportWorkerQueueTableAsync(commaDelimitedListOfResourceIds);

						if (IsTableEmpty(batchDataTable))
						{
							RaiseAndLogDebugMessage("No records in the export worker queue table for this resource pool.");
						}
						else
						{
							// Use first record to check the status of the Job RDO.  All records are from the same job
							var workerQueueRecord = new ExportWorkerQueueRecord(batchDataTable.Rows[0]);

							SetJobProperties(workerQueueRecord);

							var objectType = (Constant.Enums.SupportedObjects)Enum.Parse(typeof(Constant.Enums.SupportedObjects), workerQueueRecord.ObjectType);
							String exportJobStatus = await RetrieveExportJobStatusAsync(workerQueueRecord);

							if (exportJobStatus != Constant.Status.Job.IN_PROGRESS_WORKER)
							{
								//update export job status to in progress
								await UpdateExportJobStatus(workerQueueRecord.WorkspaceArtifactId, workerQueueRecord.ExportJobArtifactId, Constant.Status.Job.IN_PROGRESS_WORKER);
							}

							//Create results table name if it does not already exists
							await CreateExportWorkerResultsTableIfItDoesNotAlreadyExistsAsync(workerQueueRecord);

							await ProcessBatchRecordsAsync(batchDataTable, workerQueueRecord.WorkspaceArtifactId, workerQueueRecord.ExportJobArtifactId, objectType);
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.ExportWorkerAgentError, ex);
			}
			finally
			{
				//drop batch table
				await DropExportWorkerBatchTableAsync();
			}
		}

		public void SetJobProperties(ExportWorkerQueueRecord exportWorkerQueueRecord)
		{
			WorkspaceArtifactId = exportWorkerQueueRecord.WorkspaceArtifactId;
			TableRowId = exportWorkerQueueRecord.QueueRowId;
		}

		public async Task ResetUnfinishedExportWorkerJobsAsync()
		{
			try
			{
				RaiseAndLogDebugMessage("Resetting records which failed in export worker queue table.");

				await SqlQueryHelper.ResetUnfishedExportWorkerJobsAsync(
					eddsDbContext: AgentHelper.GetDBContext(-1),
					agentId: AgentId);

				RaiseAndLogDebugMessage("Resetted records which failed in export worker queue table.");
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.ResetUnfinishedJobsInExportWorkerQueueTableError, ex);
			}
		}

		private Boolean IsTableEmpty(DataTable table)
		{
			Boolean isTableEmpty = table != null && table.Rows.Count < 1;
			return isTableEmpty;
		}

		public async Task<DataTable> RetrieveNextBatchOfRecordsInExportWorkerQueueTableAsync(String commaDelimitedListOfResourceIds)
		{
			try
			{
				RaiseAndLogDebugMessage("Retrieving next batch of records in the export worker queue table.");

				DataTable dataTable = await SqlQueryHelper.RetrieveNextBatchInExportWorkerQueueAsync(
					eddsDbContext: AgentHelper.GetDBContext(-1),
					agentId: AgentId,
					batchSize: Constant.BatchSizes.ExportWorkerJobQueue,
					uniqueBatchTableName: BatchTableName,
					commaDelimitedResourceAgentIds: commaDelimitedListOfResourceIds);

				RaiseAndLogDebugMessage("Retrieved next batch of records in the export worker queue table.");

				return dataTable;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.RetrieveNextBatchOfRecordsInExportWorkerQueueTableError, ex);
			}
		}

		public async Task ProcessBatchRecordsAsync(DataTable batchDataTable, Int32 workspaceArtifactID, Int32 jobArtifactID, Constant.Enums.SupportedObjects objectType)
		{
			try
			{
				RaiseAndLogDebugMessage($"Processing batch of records in export worker agent. [{nameof(AgentId)} = {AgentId}]");

				switch (objectType)
				{
					case Constant.Enums.SupportedObjects.User:
						await ProcessUsers(workspaceArtifactID, jobArtifactID, batchDataTable);
						break;

					default:
						throw new AdminMigrationUtilityException(Constant.ErrorMessages.InvalidAdminObjectTypeError);
				}

				RaiseAndLogDebugMessage($"Processed batch of records in export worker agent. [{nameof(AgentId)} = {AgentId}]");
			}
			catch (Exception ex)
			{
				//update export job status to error
				await UpdateExportJobStatus(workspaceArtifactID, jobArtifactID, Constant.Status.Job.ERROR);

				//rethrow exception
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.ExportManagerJobError, ex);
			}
			finally
			{
				//remove batch records from export worker queue table
				await RemoveBatchFromExportWorkerQueueTableAsync();
			}
		}

		#region helpers

		public async Task UpdateExportJobStatus(Int32 workspaceArtifactID, Int32 jobArtifactID, String status)
		{
			try
			{
				RaiseAndLogDebugMessage("Updating the status of export job.");

				await ArtifactQueries.UpdateRdoTextFieldValueAsync(
					rsapiApiOptions: RsapiApiOptions,
					workspaceArtifactId: workspaceArtifactID,
					rdoRepository: RsapiRepositoryGroup.RdoRepository,
					rdoArtifactId: jobArtifactID,
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

		public async Task CreateExportWorkerResultsTableIfItDoesNotAlreadyExistsAsync(ExportWorkerQueueRecord workerQueueRecord)
		{
			try
			{
				RaiseAndLogDebugMessage("Creating the export worker results table if it doesn't already exists.");

				await SqlQueryHelper.CreateExportWorkerResultsTableIfItDoesNotAlreadyExistsAsync(
					eddsDbContext: AgentHelper.GetDBContext(-1),
					tableName: workerQueueRecord.ResultTableName);

				RaiseAndLogDebugMessage("Created the export worker results table if it doesn't already exists.");
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.CreateExportWorkerResultsTableIfItDoesNotAlreadyExistsError, ex);
			}
		}

		public async Task CreateExportJobErrorRecordAsync(Int32 workspaceArtifactID, Int32 jobArtifactID, String objectType, String status, String details)
		{
			try
			{
				await ArtifactQueries.CreateJobErrorRecordAsync(
					rsapiApiOptions: RsapiApiOptions,
					workspaceArtifactId: workspaceArtifactID,
					rdoRepository: RsapiRepositoryGroup.RdoRepository,
					rdoGuid: Constant.Guids.ObjectType.ExportUtilityJobErrors,
					jobArtifactId: jobArtifactID,
					jobType: objectType,
					statusFieldValue: status,
					detailsFieldValue: details);
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.CreateExportJobErrorRecordError, ex);
			}
		}

		public async Task RemoveBatchFromExportWorkerQueueTableAsync()
		{
			try
			{
				await SqlQueryHelper.RemoveBatchFromExportWorkerQueueTableAsync(
					eddsDbContext: AgentHelper.GetDBContext(-1),
					batchTableName: BatchTableName);
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.RemoveBatchFromExportWorkerQueueTableError, ex);
			}
		}

		public async Task DropExportWorkerBatchTableAsync()
		{
			try
			{
				await SqlQueryHelper.DropTableAsync(
					eddsDbContext: AgentHelper.GetDBContext(-1),
					tableName: BatchTableName);
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.DropExportWorkerBatchTableError, ex);
			}
		}

		public async Task<String> RetrieveExportJobStatusAsync(ExportWorkerQueueRecord exportWorkerQueueRecord)
		{
			try
			{
				RaiseAndLogDebugMessage("Retrieving the status of export job.");

				String exportJobStatus = await ArtifactQueries.RetrieveRdoTextFieldValueAsync(
					rsapiApiOptions: RsapiApiOptions,
					workspaceArtifactId: exportWorkerQueueRecord.WorkspaceArtifactId,
					rdoRepository: RsapiRepositoryGroup.RdoRepository,
					rdoArtifactId: exportWorkerQueueRecord.ExportJobArtifactId,
					textFieldGuid: Constant.Guids.Field.ExportUtilityJob.Status);

				RaiseAndLogDebugMessage("Retrieved the status of export job.");

				return exportJobStatus;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.RetrieveExportJobStatusError, ex);
			}
		}

		#endregion helpers

		#region Users

		public async Task ProcessUsers(Int32 workspaceArtifactID, Int32 jobArtifactID, DataTable batchDataTable)
		{
			try
			{
				RaiseAndLogDebugMessage("Processing all users in export worker batch.");

				foreach (DataRow currentDataRow in batchDataTable.AsEnumerable())
				{
					ExportWorkerQueueRecord exportWorkerQueueRecord = new ExportWorkerQueueRecord(currentDataRow);
					await ProcessSingleUserAsync(workspaceArtifactID, jobArtifactID, exportWorkerQueueRecord);
				}

				RaiseAndLogDebugMessage("Processed all users in export worker batch.");
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.ProcessAllUsersInExportWorkerBatchError, ex);
			}
		}

		public async Task ProcessSingleUserAsync(Int32 workspaceArtifactID, Int32 jobArtifactID, ExportWorkerQueueRecord exportWorkerQueueRecord)
		{
			try
			{
				RaiseAndLogDebugMessage($"Processing a single user in export worker batch. [{nameof(exportWorkerQueueRecord.ArtifactId)}] = {exportWorkerQueueRecord.ArtifactId}");

				kCura.Relativity.Client.DTOs.User userRDO = await ArtifactQueries.RetrieveUserAsync(RsapiApiOptions, RsapiRepositoryGroup.UserRepository, exportWorkerQueueRecord.ArtifactId);

				//query user Auth data
				LoginProfile userLoginProfile = await AuthenticationHelper.RetrieveExistingUserLoginProfileAsync(userRDO.ArtifactID);

				//query keywords and notes for user
				KeywordsNotesModel userKeywordsNotesModel = await SqlQueryHelper.RetrieveKeywordsAndNotesForUserAsync(
					eddsDbContext: AgentHelper.GetDBContext(-1),
					userArtifactId: userRDO.ArtifactID);

				//query groups user is part of
				IEnumerable<String> userGroupNameList = await QueryGroupsNamesUserIsPartOfAsync(userRDO);

				UserAdminObject userAdminObject = await ConvertUserResultToUserAdminObjectAsync(userRDO);

				//add keywords and notes, auth and groups data to UserAdminObject
				await AddKeywordsNotesAuthInfoAndGroupsToUserAdminObjectAsync(userAdminObject, userKeywordsNotesModel, userGroupNameList, userLoginProfile);

				//insert user data info into the export worker results table
				await InsertUserDataIntoExportWorkerResultsTableAsync(exportWorkerQueueRecord, userAdminObject);

				RaiseAndLogDebugMessage($"Processed a single user in export worker batch. [{nameof(exportWorkerQueueRecord.ArtifactId)}] = {exportWorkerQueueRecord.ArtifactId}");
			}
			catch (Exception ex)
			{
				//create export job error record
				String details = ExceptionMessageFormatter.GetExceptionMessageIncludingAllInnerExceptions(ex);
				await CreateExportJobErrorRecordAsync(workspaceArtifactID, jobArtifactID, exportWorkerQueueRecord.ObjectType, Constant.Status.JobErrors.ERROR, details);
			}
		}

		public async Task<IEnumerable<String>> QueryGroupsNamesUserIsPartOfAsync(kCura.Relativity.Client.DTOs.User user)
		{
			try
			{
				var userGroupArtifactIDs = user.Groups.Select(x => x.ArtifactID).ToList();
				IEnumerable<String> groupNameList = await ArtifactQueries.QueryGroupsNamesForArtifactIdsAsync(
					rsapiApiOptions: RsapiApiOptions,
					eddsWorkspaceArtifactId: -1,
					groupRepository: RsapiRepositoryGroup.GroupRepository,
					groupArtifactIdList: userGroupArtifactIDs);
				return groupNameList;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.QueryGroupNamesUserIsPartOfError, ex);
			}
		}

		public async Task<UserAdminObject> ConvertUserResultToUserAdminObjectAsync(kCura.Relativity.Client.DTOs.User userResult)
		{
			return new UserAdminObject()
			{
				ArtifactId = userResult.ArtifactID,
				FirstName = { Data = userResult.FirstName },
				LastName = { Data = userResult.LastName },
				EmailAddress = { Data = userResult.EmailAddress },
				Type = { Data = await ArtifactQueries.GetChoiceNameByArtifactID(RsapiRepositoryGroup.ChoiceRepository, RsapiApiOptions, userResult.Type.ArtifactID) },
				Client = { Data = await ArtifactQueries.GetClientNameByArtifactID(RsapiRepositoryGroup.ClientRepository, RsapiApiOptions, userResult.Client.ArtifactID) },
				RelativityAccess = { Data = userResult.RelativityAccess.ToString() },
				DocumentSkip = { Data = await ArtifactQueries.GetChoiceNameByArtifactID(RsapiRepositoryGroup.ChoiceRepository, RsapiApiOptions, userResult.DocumentSkip.ArtifactID) },
				BetaUser = { Data = userResult.BetaUser.ToString() },
				ChangeSettings = { Data = userResult.ChangeSettings.ToString() },
				KeyboardShortcuts = { Data = userResult.KeyboardShortcuts.ToString() },
				ItemListPageLength = { Data = userResult.ItemListPageLength.ToString() },
				DefaultSelectedFileType = { Data = await ArtifactQueries.GetChoiceNameByArtifactID(RsapiRepositoryGroup.ChoiceRepository, RsapiApiOptions, userResult.DefaultSelectedFileType.ArtifactID) },
				SkipDefaultPreference = { Data = await ArtifactQueries.GetChoiceNameByArtifactID(RsapiRepositoryGroup.ChoiceRepository, RsapiApiOptions, userResult.SkipDefaultPreference.ArtifactID) },
				EnforceViewerCompatibility = { Data = userResult.EnforceViewerCompatibility.ToString() },
				AdvancedSearchPublicByDefault = { Data = userResult.AdvancedSearchPublicByDefault.ToString() },
				NativeViewerCacheAhead = { Data = userResult.NativeViewerCacheAhead.ToString() },
				ChangeDocumentViewer = { Data = userResult.CanChangeDocumentViewer.ToString() },
				DocumentViewer = { Data = await ArtifactQueries.GetChoiceNameByArtifactID(RsapiRepositoryGroup.ChoiceRepository, RsapiApiOptions, userResult.DocumentViewer.ArtifactID) },
				Keywords = { Data = null },
				Notes = { Data = null },
				Groups = { Data = null },
				WindowsAccount = { Data = null },
				UserMustChangePasswordOnNextLogin = { Data = userResult.ChangePasswordNextLogin.GetValueOrDefault().ToString() },
				CanChangePassword = { Data = userResult.ChangePassword.GetValueOrDefault().ToString() },
				MaximumPasswordAgeInDays = { Data = userResult.MaximumPasswordAge.GetValueOrDefault().ToString() },
				TwoFactorMode = { Data = null },
				TwoFactorInfo = { Data = null }
			};
		}

		public async Task AddKeywordsNotesAuthInfoAndGroupsToUserAdminObjectAsync(UserAdminObject userAdminObject, KeywordsNotesModel userKeywordsNotesModel, IEnumerable<String> groupNameList, LoginProfile userLoginProfile)
		{
			try
			{
				await Task.Run(() =>
				{
					//add keywords and notes data to UserAdminObject instance
					userAdminObject.Keywords.Data = userKeywordsNotesModel.Keywords;
					userAdminObject.Notes.Data = userKeywordsNotesModel.Notes;

					//add groups data to UserAdminObject instance
					userAdminObject.Groups.Data = String.Join(Constant.SemiColonSeparator, groupNameList);

					//add integrated authentication data to UserAdminObject instance
					if (userLoginProfile.IntegratedAuthentication != null)
					{
						userAdminObject.WindowsAccount.Data = userLoginProfile.IntegratedAuthentication.IsEnabled ? userLoginProfile.IntegratedAuthentication.Account : String.Empty;
					}

					//add password authentication data to UserAdminObject instance
					if (userLoginProfile.Password != null)
					{
						//No need to set the following fields UserMustChangePasswordOnNextLogin, CanChangePassword, MaximumPasswordAgeInDays
						userAdminObject.TwoFactorMode.Data = userLoginProfile.Password.TwoFactorMode.ToString();
						userAdminObject.TwoFactorInfo.Data = userLoginProfile.Password.TwoFactorInfo;
					}
				});
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.AddKeywordsNotesAuthGroupsDataToUserAdminObjectError, ex);
			}
		}

		public async Task InsertUserDataIntoExportWorkerResultsTableAsync(ExportWorkerQueueRecord exportWorkerQueueRecord, UserAdminObject userAdminObject)
		{
			try
			{
				//deserialize user admin object
				String metadata = await SerializationHelper.SerializeAdminObjectAsync(userAdminObject);

				//insert into export worker results table
				await SqlQueryHelper.InsertRowIntoExportWorkerResultsTableAsync(
					eddsDbContext: AgentHelper.GetDBContext(-1),
					exportWorkerResultTableName: exportWorkerQueueRecord.ResultTableName,
					workspaceArtifactId: WorkspaceArtifactId,
					exportJobArtifactId: exportWorkerQueueRecord.ExportJobArtifactId,
					objectType: exportWorkerQueueRecord.ObjectType,
					workspaceResourceGroupArtifactId: exportWorkerQueueRecord.WorkspaceResourceGroupArtifactId,
					queueStatus: Constant.Status.Queue.NOT_STARTED,
					metadata: metadata,
					timeStampUtc: DateTime.UtcNow);
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.InsertUserDataIntoExportWorkerResultsTableError, ex);
			}
		}

		#endregion Users
	}
}
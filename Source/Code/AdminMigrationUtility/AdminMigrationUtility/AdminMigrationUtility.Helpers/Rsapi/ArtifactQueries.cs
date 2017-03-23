using AdminMigrationUtility.Helpers.Exceptions;
using AdminMigrationUtility.Helpers.Models;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.Client.Repositories;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Rsapi
{
	public class ArtifactQueries : IArtifactQueries
	{
		//Do not convert to async
		public Boolean UserHasAccessToArtifact(IServicesMgr servicesMgr, ExecutionIdentity executionIdentity, Int32 workspaceArtifactId, Guid guid, String artifactTypeName)
		{
			Response<Boolean> result = UserHasAccessToRdoByType(servicesMgr, executionIdentity, workspaceArtifactId, guid, artifactTypeName);
			Boolean hasAccess = result.Success;

			return hasAccess;
		}

		//Do not convert to async
		public Response<Boolean> UserHasAccessToRdoByType(IServicesMgr servicesMgr, ExecutionIdentity executionIdentity, Int32 workspaceArtifactId, Guid guid, String artifactTypeName)
		{
			ResultSet<RDO> results;

			using (IRSAPIClient client = servicesMgr.CreateProxy<IRSAPIClient>(executionIdentity))
			{
				client.APIOptions.WorkspaceID = workspaceArtifactId;
				var relApp = new RDO(guid)
				{
					ArtifactTypeName = artifactTypeName
				};

				results = client.Repositories.RDO.Read(relApp);
			}

			var res = new Response<Boolean>
			{
				Results = results.Success,
				Success = results.Success,
				Message = MessageFormatter.FormatMessage(results.Results.Select(x => x.Message).ToList(), results.Message, results.Success)
			};

			return res;
		}

		public async Task<Int32> GetResourcePoolArtifactIdAsync(IServicesMgr svcMgr, ExecutionIdentity executionIdentity, Int32 workspaceArtifactId)
		{
			return await Task.Run(() =>
			{
				Int32 resourcePoolId = 0;
				using (IRSAPIClient proxy = svcMgr.CreateProxy<IRSAPIClient>(executionIdentity))
				{
					Int32? result = proxy.Repositories.Workspace.ReadSingle(workspaceArtifactId).ResourcePoolID;
					if (result.HasValue)
					{
						resourcePoolId = result.Value;
					}

					return resourcePoolId;
				}
			});
		}

		public async Task<QueryResultSet<kCura.Relativity.Client.DTOs.User>> QueryAllUsersArtifactIDsAsync(APIOptions rsapiApiOptions, IGenericRepository<kCura.Relativity.Client.DTOs.User> userRepository)
		{
			try
			{
				rsapiApiOptions.WorkspaceID = -1;

				Query<kCura.Relativity.Client.DTOs.User> userQuery = new Query<kCura.Relativity.Client.DTOs.User>
				{
					Fields = FieldValue.NoFields
				};

				QueryResultSet<kCura.Relativity.Client.DTOs.User> retVal;
				try
				{
					retVal = await Task.Run(() => userRepository.Query(userQuery, Constant.BatchSizes.UserQuery));
				}
				catch (Exception ex)
				{
					throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.QueryUsersError} Query.", ex);
				}

				if (!retVal.Results.Any())
				{
					throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.QueryUsersError} {Constant.ErrorMessages.NoUsersFound}.");
				}

				return retVal;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.QueryUsersError, ex);
			}
		}

		public async Task<kCura.Relativity.Client.DTOs.User> RetrieveUserAsync(APIOptions rsapiApiOptions, IGenericRepository<kCura.Relativity.Client.DTOs.User> userRepository, Int32 userArtifactID)
		{
			try
			{
				rsapiApiOptions.WorkspaceID = -1;

				var retVal = await Task.Run(() => userRepository.ReadSingle(userArtifactID));

				return retVal;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(String.Format(Constant.ErrorMessages.ReadSingleUserError, userArtifactID), ex);
			}
		}

		public async Task<IEnumerable<Result<kCura.Relativity.Client.DTOs.User>>> QueryAllUsersAsync(APIOptions rsapiApiOptions, IGenericRepository<kCura.Relativity.Client.DTOs.User> userRepository)
		{
			try
			{
				rsapiApiOptions.WorkspaceID = -1;

				Query<kCura.Relativity.Client.DTOs.User> userQuery = new Query<kCura.Relativity.Client.DTOs.User>
				{
					Fields = FieldValue.AllFields
				};

				IEnumerable<Result<kCura.Relativity.Client.DTOs.User>> retVal;
				try
				{
					retVal = await Task.Run(() => QuerySubset.PerformQuerySubset(userRepository, userQuery, Constant.BatchSizes.UserQuery));
				}
				catch (Exception ex)
				{
					throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.QueryUsersError} Query.", ex);
				}

				if (!retVal.Any())
				{
					throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.QueryUsersError} {Constant.ErrorMessages.NoUsersFound}.");
				}

				return retVal;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.QueryUsersError, ex);
			}
		}

		public async Task<QueryResultSet<Client>> GetClientAsync(APIOptions apiOptions, IGenericRepository<Client> clientRdoRepository, String clientName)
		{
			QueryResultSet<Client> retVal = null;
			await Task.Run(() =>
			{
				try
				{
					apiOptions.WorkspaceID = -1;
					var clientQuery = new Query<Client>()
					{
						Condition = new TextCondition(ClientFieldNames.Name, TextConditionEnum.EqualTo, clientName),
						Fields = new List<FieldValue> { new FieldValue(ClientFieldNames.Name) }
					};
					retVal = clientRdoRepository.Query(clientQuery);
				}
				catch (Exception ex)
				{
					retVal = new QueryResultSet<Client>()
					{
						Success = false,
						Message = ex.ToString()
					};
				}
			});
			return retVal;
		}

		public async Task<QueryResultSet<Group>> QueryGroupsAsync(APIOptions apiOptions, IGenericRepository<Group> groupRepository, String[] groupNames)
		{
			QueryResultSet<Group> retVal = null;
			await Task.Run(() =>
			{
				try
				{
					apiOptions.WorkspaceID = -1;
					var groupQuery = new Query<Group>()
					{
						Condition = new TextCondition(GroupFieldNames.Name, TextConditionEnum.In, groupNames),
						Fields = new List<FieldValue> { new FieldValue(GroupFieldNames.Name) }
					};
					retVal = groupRepository.Query(groupQuery);
				}
				catch (Exception ex)
				{
					retVal = new QueryResultSet<Group>()
					{
						Success = false,
						Message = ex.ToString()
					};
				}
			});
			return retVal;
		}

		public async Task<String> RetrieveRdoJobStatusAsync(IServicesMgr svcMgr, Int32 workspaceArtifactId, ExecutionIdentity identity, Int32 artifactId)
		{
			return await Task.Run(() =>
			{
				try
				{
					using (IRSAPIClient client = svcMgr.CreateProxy<IRSAPIClient>(identity))
					{
						client.APIOptions.WorkspaceID = workspaceArtifactId;
						RDO jobRdo;

						try
						{
							jobRdo = client.Repositories.RDO.ReadSingle(artifactId);
						}
						catch (Exception ex)
						{
							throw new AdminMigrationUtilityException("An error occurred when querying for the status of the RDO (ReadSingle).", ex);
						}

						return jobRdo.Fields.Get("Status").ToString();
					}
				}
				catch (Exception ex)
				{
					throw new AdminMigrationUtilityException("An error occurred when querying for the status of the RDO.", ex);
				}
			});
		}

		public async Task MarkImportJobAsSubmittedForMigration(IServicesMgr svcMgr, ExecutionIdentity identity, Int32 workspaceArtifactId, Int32 jobArtifactId)
		{
			await Task.Run(() =>
			{
				try
				{
					using (IRSAPIClient client = svcMgr.CreateProxy<IRSAPIClient>(identity))
					{
						client.APIOptions.WorkspaceID = workspaceArtifactId;
						RDO jobRdo = new RDO(jobArtifactId)
						{
							ArtifactTypeGuids = new List<Guid> { Constant.Guids.ObjectType.ImportUtilityJob },
							Fields = new List<FieldValue>
						{
							new FieldValue(Constant.Guids.Field.ImportUtilityJob.SubmittedForMigration) {Value = true}
						}
						};

						try
						{
							client.Repositories.RDO.UpdateSingle(jobRdo);
						}
						catch (Exception ex)
						{
							throw new AdminMigrationUtilityException("An error occurred while recording this job as being submitted for migration.", ex);
						}
					}
				}
				catch (Exception ex)
				{
					throw new AdminMigrationUtilityException("An error occurred while updating the text field of an RDO.", ex);
				}
			});
		}

		public async Task ResetExportJobStatisticsAsync(IServicesMgr svcMgr, Int32 workspaceArtifactId, ExecutionIdentity identity, Int32 artifactId)
		{
			try
			{
				await Task.Run(() =>
				{
					using (IRSAPIClient client = svcMgr.CreateProxy<IRSAPIClient>(identity))
					{
						client.APIOptions.WorkspaceID = workspaceArtifactId;
						RDO jobRdo = new RDO(artifactId)
						{
							ArtifactTypeGuids = new List<Guid> { Constant.Guids.ObjectType.ExportUtilityJob },
							Fields = new List<FieldValue>
						{
							new FieldValue(Constant.Guids.Field.ExportUtilityJob.Expected) {Value = 0},
							new FieldValue(Constant.Guids.Field.ExportUtilityJob.Exported) {Value = 0},
							new FieldValue(Constant.Guids.Field.ExportUtilityJob.NotExported) {Value = 0},
							new FieldValue(Constant.Guids.Field.ExportUtilityJob.ExportFile) { Value = null},
							new FieldValue(Constant.Guids.Field.ExportUtilityJob.RecordsThatHaveBeenProcessed) { Value = 0}
						}
						};

						try
						{
							client.Repositories.RDO.UpdateSingle(jobRdo);
						}
						catch (Exception ex)
						{
							throw new AdminMigrationUtilityException("An error occurred while reseting the statistics of the Export Job.", ex);
						}
					}
				});
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException("An error occurred while updating the text field of an RDO.", ex);
			}
		}

		public async Task UpdateRdoJobTextFieldAsync(IServicesMgr svcMgr, Int32 workspaceArtifactId, ExecutionIdentity identity, Guid objectTypeGuid, Int32 artifactId, Guid textFieldGuid, String textFieldValue)
		{
			try
			{
				await Task.Run(() =>
				{
					using (IRSAPIClient client = svcMgr.CreateProxy<IRSAPIClient>(identity))
					{
						client.APIOptions.WorkspaceID = workspaceArtifactId;
						RDO jobRdo = new RDO(artifactId)
						{
							ArtifactTypeGuids = new List<Guid> { objectTypeGuid },
							Fields = new List<FieldValue>
						{
							new FieldValue(textFieldGuid) {Value = textFieldValue}
						}
						};

						try
						{
							client.Repositories.RDO.UpdateSingle(jobRdo);
						}
						catch (Exception ex)
						{
							throw new AdminMigrationUtilityException("An error occurred while updating the text field of an RDO (UpdateSingle).", ex);
						}
					}
				});
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException("An error occurred while updating the text field of an RDO.", ex);
			}
		}

		public async Task<IEnumerable<Int32>> QueryJobErrors(IServicesMgr svcManager, ExecutionIdentity identity, Int32 workspaceArtifactID, Int32 jobArtifactID, Guid rdoErrorJob, Guid rdoErrorJobField)
		{
			List<Int32> retVal = null;
			await Task.Run(() =>
			{
				using (IRSAPIClient client = svcManager.CreateProxy<IRSAPIClient>(identity))
				{
					client.APIOptions.WorkspaceID = workspaceArtifactID;
					var jobErrorsQuery = new Query<RDO>()
					{
						ArtifactTypeGuid = rdoErrorJob,
						Condition = new ObjectCondition(rdoErrorJobField, ObjectConditionEnum.EqualTo, jobArtifactID),
						Fields = FieldValue.NoFields
					};

					try
					{
						var jobErrorQueryResults = QuerySubset.PerformQuerySubset(client.Repositories.RDO, jobErrorsQuery, 500); //Throws when query is unsuccessful
						retVal = jobErrorQueryResults.Select(x => x.Artifact.ArtifactID).ToList();
					}
					catch (Exception ex)
					{
						throw new AdminMigrationUtilityException("An error occurred while querying import job error RDOs.", ex);
					}
				}
			});
			return retVal;
		}

		public async Task<IEnumerable<Int32>> QueryJobErrors(APIOptions apiOptions, IGenericRepository<RDO> rdoRepository, Int32 workspaceArtifactID, Int32 jobArtifactID)
		{
			try
			{
				QueryResultSet<RDO> results = null;
				apiOptions.WorkspaceID = workspaceArtifactID;
				var jobErrorsQuery = new Query<RDO>()
				{
					ArtifactTypeGuid = Constant.Guids.ObjectType.ExportUtilityJobErrors,
					Condition = new ObjectCondition(Constant.Guids.Field.ExportUtilityJobErrors.ExportUtilityJob, ObjectConditionEnum.EqualTo, jobArtifactID),
					Fields = FieldValue.NoFields
				};

				await Task.Run(() => results = rdoRepository.Query(jobErrorsQuery));

				if (results.Success == false)
				{
					throw new Exception(results.Message);
				}
				return results.Results.Select(x => x.Artifact.ArtifactID).ToList();
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.ErrorQueryingJobErrors, ex);
			}
		}

		public async Task DeleteJobErrors(IServicesMgr svcManager, ExecutionIdentity identity, Int32 workspaceArtifactID, List<Int32> jobErrorArtifactIDs, Int32 artifactTypeId)
		{
			await Task.Run(() =>
			{
				if (jobErrorArtifactIDs != null && jobErrorArtifactIDs.Any())
				{
					using (IRSAPIClient client = svcManager.CreateProxy<IRSAPIClient>(identity))
					{
						client.APIOptions.WorkspaceID = workspaceArtifactID;
						try
						{
							var deleteOptions = new MassDeleteOptions(artifactTypeId) { CascadeDelete = true };
							var result = client.MassDelete(client.APIOptions, deleteOptions, jobErrorArtifactIDs);

							if (!result.Success)
							{
								throw new AdminMigrationUtilityException($"An error occurred while deleting job error RDOs: {result.Message}");
							}
						}
						catch (Exception ex)
						{
							throw new AdminMigrationUtilityException("An error occurred while deleting job error RDOs.", ex);
						}
					}
				}
			});
		}

		public async Task<String> RetrieveRdoTextFieldValueAsync(APIOptions rsapiApiOptions, Int32 workspaceArtifactId, IGenericRepository<RDO> rdoRepository, Int32 rdoArtifactId, Guid textFieldGuid)
		{
			String errorContext = $"[{nameof(rdoArtifactId)} = {rdoArtifactId}, {nameof(textFieldGuid)} = {textFieldGuid}]";
			try
			{
				rsapiApiOptions.WorkspaceID = workspaceArtifactId;

				RDO jobRdo;

				try
				{
					jobRdo = await Task.Run(() => rdoRepository.ReadSingle(rdoArtifactId));
				}
				catch (Exception ex)
				{
					throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.RdoTextFieldQueryError} (ReadSingle). {errorContext}", ex);
				}

				String textFieldValue = jobRdo.Fields.Get(textFieldGuid).ToString();
				return textFieldValue;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.RdoTextFieldQueryError} {errorContext}", ex);
			}
		}

		public async Task UpdateRdoTextFieldValueAsync(APIOptions rsapiApiOptions, Int32 workspaceArtifactId, IGenericRepository<RDO> rdoRepository, Int32 rdoArtifactId, Guid jobObjectGuid, Guid textFieldGuid, Object fieldValue)
		{
			String errorContext = $"[{nameof(workspaceArtifactId)} = {workspaceArtifactId}, {nameof(rdoArtifactId)} = {rdoArtifactId}, {nameof(jobObjectGuid)} = {jobObjectGuid}, {nameof(textFieldGuid)} = {textFieldGuid}, {nameof(fieldValue)} = {fieldValue}]";

			try
			{
				rsapiApiOptions.WorkspaceID = workspaceArtifactId;

				RDO jobRdo = new RDO(rdoArtifactId)
				{
					ArtifactTypeGuids = new List<Guid>
					{
						jobObjectGuid
					},
					Fields = new List<FieldValue>
					{
						new FieldValue(textFieldGuid) {Value = fieldValue}
					}
				};

				try
				{
					await Task.Run(() => rdoRepository.UpdateSingle(jobRdo));
				}
				catch (Exception ex)
				{
					throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.RdoTextFieldUpdateError} (UpdateSingle). {errorContext}", ex);
				}
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.RdoTextFieldUpdateError} {errorContext}", ex);
			}
		}

		public async Task<Int32?> FindUserByEmailAddressAsync(APIOptions rsapiApiOptions, IGenericRepository<kCura.Relativity.Client.DTOs.User> userRepository, String emailAddress)
		{
			try
			{
				Int32? retVal = null;
				QueryResultSet<kCura.Relativity.Client.DTOs.User> queryResults = null;
				rsapiApiOptions.WorkspaceID = -1;

				var query = new Query<kCura.Relativity.Client.DTOs.User>()
				{
					Condition = new TextCondition(UserFieldNames.EmailAddress, TextConditionEnum.EqualTo, emailAddress)
				};

				await Task.Run(() => queryResults = userRepository.Query(query));
				if (queryResults.Success && queryResults.TotalCount > 0)
				{
					retVal = queryResults.Results[0].Artifact.ArtifactID;
				}
				return retVal;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.FindUserByEmailError, ex);
			}
		}

		public async Task<Int32> RetrieveWorkspaceResourcePoolArtifactIdAsync(APIOptions rsapiApiOptions, Int32 eddsWorkspaceArtifactId, IGenericRepository<Workspace> workspaceRepository, Int32 workspaceArtifactId)
		{
			String errorContext = $"[{nameof(workspaceArtifactId)} = {workspaceArtifactId}]";

			try
			{
				Int32? resourcePoolId;

				rsapiApiOptions.WorkspaceID = eddsWorkspaceArtifactId;

				try
				{
					resourcePoolId = await Task.Run(() => workspaceRepository.ReadSingle(workspaceArtifactId).ResourcePoolID);
				}
				catch (Exception ex)
				{
					throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.RetrieveWorkspaceResourcePoolArtifactIdError} ReadSingle. {errorContext}", ex);
				}

				if (resourcePoolId == null)
				{
					throw new AdminMigrationUtilityException(Constant.ErrorMessages.RetrieveWorkspaceResourcePoolArtifactIdNullError);
				}

				return resourcePoolId.Value;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.RetrieveWorkspaceResourcePoolArtifactIdError} {errorContext}", ex);
			}
		}

		public async Task CreateJobErrorRecordAsync(APIOptions rsapiApiOptions, Int32 workspaceArtifactId, IGenericRepository<RDO> rdoRepository, Guid rdoGuid, Int32 jobArtifactId, String jobType, String statusFieldValue, String detailsFieldValue)
		{
			String errorContext = $"[{nameof(rdoGuid)} = {rdoGuid}, {nameof(statusFieldValue)} = {statusFieldValue}, {nameof(detailsFieldValue)} = {detailsFieldValue}]";

			try
			{
				rsapiApiOptions.WorkspaceID = workspaceArtifactId;

				RDO jobRdo = new RDO
				{
					ArtifactTypeGuids = new List<Guid>
					{
						rdoGuid
					}
				};

				String name = Guid.NewGuid().ToString();

				if (rdoGuid == Constant.Guids.ObjectType.ExportUtilityJobErrors)
				{
					jobRdo.Fields = new List<FieldValue>
					{
						new FieldValue(Constant.Guids.Field.ExportUtilityJobErrors.Name) {Value = name},
						new FieldValue(Constant.Guids.Field.ExportUtilityJobErrors.Status) {Value = statusFieldValue},
						new FieldValue(Constant.Guids.Field.ExportUtilityJobErrors.Details) {Value = detailsFieldValue},
						new FieldValue(Constant.Guids.Field.ExportUtilityJobErrors.ExportUtilityJob) {Value = new RDO(jobArtifactId)},
						new FieldValue(Constant.Guids.Field.ExportUtilityJobErrors.ObjectType) {Value = jobType}
					};
				}
				else if (rdoGuid == Constant.Guids.ObjectType.ImportUtilityJobErrors)
				{
					jobRdo.Fields = new List<FieldValue>
					{
						new FieldValue(Constant.Guids.Field.ImportUtilityJobErrors.Name) {Value = name},
						new FieldValue(Constant.Guids.Field.ImportUtilityJobErrors.Message) {Value = statusFieldValue},
						new FieldValue(Constant.Guids.Field.ImportUtilityJobErrors.Details) {Value = detailsFieldValue},
						new FieldValue(Constant.Guids.Field.ImportUtilityJobErrors.ImportUtilityJob) {Value = new RDO(jobArtifactId)}
					};
				}
				else
				{
					throw new AdminMigrationUtilityException(Constant.ErrorMessages.NotValidErrorsObjectTypeGuidError);
				}

				try
				{
					await Task.Run(() => rdoRepository.CreateSingle(jobRdo));
				}
				catch (Exception ex)
				{
					throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.CreateJobErrorRecordError} (CreateSingle). {errorContext}", ex);
				}
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.CreateJobErrorRecordError} {errorContext}", ex);
			}
		}

		public async Task CreateImportJobErrorRecordsAsync(APIOptions rsapiApiOptions, Int32 workspaceArtifactId, Int32 jobArtifactId, IGenericRepository<RDO> rdoRepository, IEnumerable<ImportJobError> errors, String jobType)
		{
			await Task.Run(() =>
			{
				try
				{
					if (errors.Any())
					{
						rsapiApiOptions.WorkspaceID = workspaceArtifactId;
						var updateList = new List<RDO>();

						foreach (var error in errors)
						{
							updateList.Add(
								new RDO()
								{
									ArtifactTypeGuids = new List<Guid> { Constant.Guids.ObjectType.ImportUtilityJobErrors },
									Fields = new List<FieldValue>
									{
								new FieldValue(Constant.Guids.Field.ImportUtilityJobErrors.Name) { Value = Guid.Empty.ToString()},
								new FieldValue(Constant.Guids.Field.ImportUtilityJobErrors.Type) { Value = error.Type == null ? String.Empty : Enum.GetName(typeof(Constant.ImportUtilityJob.ErrorType), error.Type)},
								new FieldValue(Constant.Guids.Field.ImportUtilityJobErrors.LineNumber) { Value = error.LineNumber},
								new FieldValue(Constant.Guids.Field.ImportUtilityJobErrors.Message) { Value = error.Message},
								new FieldValue(Constant.Guids.Field.ImportUtilityJobErrors.Details) { Value = error.Details},
								new FieldValue(Constant.Guids.Field.ImportUtilityJobErrors.ObjectType) { Value = jobType},
								new FieldValue(Constant.Guids.Field.ImportUtilityJobErrors.ImportUtilityJob) { Value = new RDO(jobArtifactId)}
									}
								});
						}

						var result = rdoRepository.Create(updateList);
						if (result.Success == false)
						{
							throw new AdminMigrationUtilityException(result.Message);
						}
					}
				}
				catch (Exception ex)
				{
					throw new AdminMigrationUtilityException(Constant.ErrorMessages.CreateImportErrorsError, ex);
				}
			});
		}

		public async Task<Boolean> JobErrorRecordExistsAsync(APIOptions rsapiApiOptions, Int32 workspaceArtifactId, IGenericRepository<RDO> rdoRepository, Guid rdoGuid, Guid associatedJobFieldGuid, Int32 jobArtifactId)
		{
			try
			{
				var retVal = false;
				QueryResultSet<RDO> results = null;
				rsapiApiOptions.WorkspaceID = workspaceArtifactId;
				var query = new Query<RDO>()
				{
					ArtifactTypeGuid = rdoGuid,
					Fields = FieldValue.NoFields,
					Condition = new ObjectCondition(associatedJobFieldGuid, ObjectConditionEnum.EqualTo, jobArtifactId)
				};
				await Task.Run(() => results = rdoRepository.Query(query));
				if (results.Success)
				{
					if (results.Results.Any())
					{
						retVal = true;
					}
				}
				else
				{
					throw new AdminMigrationUtilityException(results.Message);
				}
				return retVal;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.QueryJobErrrorsError, ex);
			}
		}

		public async Task<IEnumerable<ImportJobError>> GetImportJobErrorsAsync(APIOptions rsapiApiOptions, Int32 workspaceArtifactId, IGenericRepository<RDO> rdoRepository, Int32 jobArtifactId)
		{
			try
			{
				List<ImportJobError> retVal = new List<ImportJobError>();
				IEnumerable<Result<RDO>> results;
				rsapiApiOptions.WorkspaceID = workspaceArtifactId;
				var query = new Query<RDO>()
				{
					ArtifactTypeGuid = Constant.Guids.ObjectType.ImportUtilityJobErrors,
					Fields = new List<FieldValue>()
					{
						new FieldValue(Constant.Guids.Field.ImportUtilityJobErrors.Message),
						new FieldValue(Constant.Guids.Field.ImportUtilityJobErrors.Type),
						new FieldValue(Constant.Guids.Field.ImportUtilityJobErrors.LineNumber),
						new FieldValue(Constant.Guids.Field.ImportUtilityJobErrors.Details),
					},
					Condition = new ObjectCondition(Constant.Guids.Field.ImportUtilityJobErrors.ImportUtilityJob, ObjectConditionEnum.EqualTo, jobArtifactId)
				};
				await Task.Run(() =>
					{
						results = QuerySubset.PerformQuerySubset(rdoRepository, query, Constant.BatchSizes.UserQuery);
						foreach (var result in results)
						{
							retVal.Add(
								new ImportJobError()
								{
									ArtifactID = result.Artifact.ArtifactID,
									Message = result.Artifact[Constant.Guids.Field.ImportUtilityJobErrors.Message].ValueAsFixedLengthText,
									Type = (Constant.ImportUtilityJob.ErrorType)Enum.Parse(typeof(Constant.ImportUtilityJob.ErrorType), result.Artifact[Constant.Guids.Field.ImportUtilityJobErrors.Type].ValueAsFixedLengthText),
									LineNumber = result.Artifact[Constant.Guids.Field.ImportUtilityJobErrors.LineNumber].ValueAsWholeNumber,
									Details = result.Artifact[Constant.Guids.Field.ImportUtilityJobErrors.Details].ValueAsLongText
								}
							);
						}
					}
				);

				return retVal;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.QueryJobErrrorsError, ex);
			}
		}

		public async Task<RDO> RetrieveJob(IGenericRepository<RDO> rdoRepository, APIOptions rsapiApiOptions, Int32 workspaceArtifactId, Int32 jobArtifactID)
		{
			RDO result;
			String errorContext = $"[{nameof(workspaceArtifactId)} = {workspaceArtifactId}]";

			try
			{
				rsapiApiOptions.WorkspaceID = workspaceArtifactId;
				result = await Task.Run(() => rdoRepository.ReadSingle(jobArtifactID));
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.RetrieveImportJobError} {errorContext}", ex);
			}

			return result;
		}

		public async Task UpdateImportJobStatistics(IGenericRepository<RDO> rdoRepository, APIOptions rsapiApiOptions, Int32 workspaceArtifactId, Int32 jobArtifactID, Int32 imported, Int32 notImported)
		{
			try
			{
				var job = new RDO(jobArtifactID)
				{
					ArtifactTypeGuids = new List<Guid> { Constant.Guids.ObjectType.ImportUtilityJob },
					Fields = new List<FieldValue>
					{
						new FieldValue(Constant.Guids.Field.ImportUtilityJob.Imported) {Value = imported},
						new FieldValue(Constant.Guids.Field.ImportUtilityJob.NotImported) {Value = notImported}
					}
				};

				rsapiApiOptions.WorkspaceID = workspaceArtifactId;
				await Task.Run(() => rdoRepository.UpdateSingle(job));
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.UpdateImportJobStatisticsError, ex);
			}
		}

		public async Task UpdateExportJobStatitics(IGenericRepository<RDO> rdoRepository, APIOptions rsapiApiOptions, Int32 workspaceArtifactId, Int32 jobArtifactID, Int32 expectedNumberOfExports, Int32 actualNumberOfExports, Int32 notExported, Int32 recordsThatHaveBeenProcessed)
		{
			try
			{
				var job = new RDO(jobArtifactID)
				{
					ArtifactTypeGuids = new List<Guid> { Constant.Guids.ObjectType.ExportUtilityJob },
					Fields = new List<FieldValue>
					{
						new FieldValue(Constant.Guids.Field.ExportUtilityJob.Expected) {Value = expectedNumberOfExports},
						new FieldValue(Constant.Guids.Field.ExportUtilityJob.Exported) {Value = actualNumberOfExports},
						new FieldValue(Constant.Guids.Field.ExportUtilityJob.NotExported) {Value = notExported},
						new FieldValue(Constant.Guids.Field.ExportUtilityJob.RecordsThatHaveBeenProcessed) {Value = recordsThatHaveBeenProcessed}
					}
				};

				rsapiApiOptions.WorkspaceID = workspaceArtifactId;
				await Task.Run(() => rdoRepository.UpdateSingle(job));
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.UpdateExportJobStatisticsError, ex);
			}
		}

		public async Task<String> GetChoiceNameByArtifactID(IGenericRepository<kCura.Relativity.Client.DTOs.Choice> choiceRepository, APIOptions rsapiApiOptions, Int32 choiceArtifactId)
		{
			String retVal;
			try
			{
				rsapiApiOptions.WorkspaceID = -1;
				var result = await Task.Run(() => choiceRepository.ReadSingle(choiceArtifactId));
				retVal = result.Name;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException("Errror while querying choice", ex);
			}
			return retVal;
		}

		public async Task<String> GetClientNameByArtifactID(IGenericRepository<Client> clientRepository, APIOptions rsapiApiOptions, Int32 clientArtifactId)
		{
			String retVal;
			try
			{
				rsapiApiOptions.WorkspaceID = -1;
				var result = await Task.Run(() => clientRepository.ReadSingle(clientArtifactId));
				retVal = result.Name;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException("Errror while querying client", ex);
			}
			return retVal;
		}

		public async Task DownloadFile(IRSAPIClient client, APIOptions apiOptions, Int32 workspaceArtifactID, Int32 objectArtifactID, Int32 fieldArtifactID, String fullDownloadLocation)
		{
			try
			{
				var fileRequest = new FileRequest(apiOptions)
				{
					Target = new TargetField
					{
						WorkspaceId = workspaceArtifactID,
						FieldId = fieldArtifactID,
						ObjectArtifactId = objectArtifactID
					}
				};

				await Task.Run(() => client.Download(fileRequest, fullDownloadLocation));
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.DownloadFileError, ex);
			}
		}

		public async Task<List<Int32>> QueryGroupArtifactIdsUserIsPartOfAsync(APIOptions rsapiApiOptions, Int32 eddsWorkspaceArtifactId, IGenericRepository<kCura.Relativity.Client.DTOs.User> userRepository, Int32 userArtifactId)
		{
			String errorContext = $"[{nameof(userArtifactId)} = {userArtifactId}]";

			try
			{
				rsapiApiOptions.WorkspaceID = eddsWorkspaceArtifactId;
				kCura.Relativity.Client.DTOs.User userDto;

				try
				{
					userDto = await Task.Run(() => userRepository.ReadSingle(userArtifactId));
				}
				catch (Exception ex)
				{
					throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.QueryGroupArtifactIdsUserIsPartOfError} (ReadSingle). {errorContext}", ex);
				}

				List<Int32> groupArtifactIdList = userDto.Groups.Select(x => x.ArtifactID).ToList();
				return groupArtifactIdList;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.QueryGroupArtifactIdsUserIsPartOfError} {errorContext}", ex);
			}
		}

		public async Task<IEnumerable<String>> QueryGroupsNamesForArtifactIdsAsync(APIOptions rsapiApiOptions, Int32 eddsWorkspaceArtifactId, IGenericRepository<Group> groupRepository, IEnumerable<Int32> groupArtifactIdList)
		{
			String errorContext = $"[{nameof(groupArtifactIdList)} = {String.Join(Constant.CommaSeparator, groupArtifactIdList)}]";

			try
			{
				rsapiApiOptions.WorkspaceID = eddsWorkspaceArtifactId;
				QueryResultSet<Group> groupQueryResultSet;
				WholeNumberCondition groupWholeNumberCondition = new WholeNumberCondition(Constant.Rsapi.GroupArtifactIdField, NumericConditionEnum.In, groupArtifactIdList.ToList());
				Query<Group> groupQuery = new Query<Group>
				{
					Condition = groupWholeNumberCondition,
					Fields = new List<FieldValue>
						{
							new FieldValue(GroupFieldNames.Name)
						}
				};

				try
				{
					groupQueryResultSet = await Task.Run(() => groupRepository.Query(groupQuery));
				}
				catch (Exception ex)
				{
					throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.QueryGroupsNamesForArtifactIdsError} (Query). {errorContext}", ex);
				}

				if (!groupQueryResultSet.Success)
				{
					throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.QueryGroupsNamesForArtifactIdsError} ErrorMessage:{groupQueryResultSet.Message}. {errorContext}");
				}

				List<String> groupNameList = groupQueryResultSet.Results.Select(x => x.Artifact.Name).ToList();
				return groupNameList;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.QueryGroupsNamesForArtifactIdsError} {errorContext}", ex);
			}
		}

		public async Task AttachFileToExportJob(IRSAPIClient rsapiClient, APIOptions rsapiApiOptions, Int32 workspaceArtifactId, Int32 exportJobArtifactId, Int32 fileFieldArtifactId, String fileLocation)
		{
			String errorContext = $"[{nameof(workspaceArtifactId)} = {workspaceArtifactId}, {nameof(exportJobArtifactId)} = {exportJobArtifactId}, {nameof(fileFieldArtifactId)} = {fileFieldArtifactId}, {nameof(fileLocation)} = {fileLocation}]";

			try
			{
				using (rsapiClient)
				{
					rsapiApiOptions.WorkspaceID = workspaceArtifactId;

					UploadRequest uploadRequest = new UploadRequest(rsapiApiOptions)
					{
						Metadata =
						{
							FileName = @fileLocation
						}
					};

					Int64 fileLength;
					FileInfo exportFileInfo = new FileInfo(fileLocation);
					if (exportFileInfo.Exists)
					{
						fileLength = exportFileInfo.Length;
					}
					else
					{
						throw new AdminMigrationUtilityException("File not exists.");
					}

					uploadRequest.Metadata.FileSize = fileLength;
					uploadRequest.Overwrite = true;
					uploadRequest.Target.FieldId = fileFieldArtifactId;
					uploadRequest.Target.ObjectArtifactId = exportJobArtifactId;

					try
					{
						await Task.Run(() => rsapiClient.Upload(uploadRequest));
					}
					catch (Exception ex)
					{
						throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.ExportFileUploadError} (Upload). {errorContext}", ex);
					}
				}
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.ExportFileUploadError} {errorContext}", ex);
			}
		}

		public async Task<Int32> RetrieveFieldArtifactIdByGuid(APIOptions rsapiApiOptions, Int32 workspaceArtifactId, IGenericRepository<kCura.Relativity.Client.DTOs.Field> fieldRepository, Guid fieldGuid)
		{
			String errorContext = $"[{nameof(workspaceArtifactId)} = {workspaceArtifactId}, {nameof(fieldGuid)} = {fieldGuid}]";

			try
			{
				rsapiApiOptions.WorkspaceID = workspaceArtifactId;
				kCura.Relativity.Client.DTOs.Field fieldDto;

				try
				{
					fieldDto = await Task.Run(() => fieldRepository.ReadSingle(fieldGuid));
				}
				catch (Exception ex)
				{
					throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.RetrieveFieldArtifactIdByGuidError} (ReadSingle). {errorContext}", ex);
				}

				Int32 fieldArtifactId = fieldDto.ArtifactID;
				return fieldArtifactId;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.RetrieveFieldArtifactIdByGuidError} {errorContext}", ex);
			}
		}

		public async Task<Int32> RetrieveSystemAdminGroupArtifactID(APIOptions rsapiApiOptions, IGenericRepository<Group> groupRepository)
		{
			try
			{
				Int32 retVal = -1;
				QueryResultSet<Group> groupQueryResults = null;
				rsapiApiOptions.WorkspaceID = -1;
				var groupQuery = new Query<Group>()
				{
					ArtifactTypeName = ArtifactTypeNames.Group,
					Condition = new WholeNumberCondition(GroupFieldNames.GroupType, NumericConditionEnum.EqualTo, Constants.Group.GroupType.SYSTEM_ADMINISTRATORS),
					Fields = FieldValue.NoFields
				};
				await Task.Run(() => groupQueryResults = groupRepository.Query(groupQuery));
				if (groupQueryResults != null && groupQueryResults.Success && groupQueryResults.Results.Any())
				{
					retVal = groupQueryResults.Results[0].Artifact.ArtifactID;
				}
				return retVal;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.QuerySystemAdminGroupError}", ex);
			}
		}

		public async Task<IEnumerable<Int32>> RetrieveUserGroupArtifactIDs(APIOptions rsapiApiOptions, IGenericRepository<kCura.Relativity.Client.DTOs.User> userRepository, Int32 eddsUserArtifactID)
		{
			try
			{
				var retVal = new List<Int32>();
				kCura.Relativity.Client.DTOs.User userRDO = null;
				rsapiApiOptions.WorkspaceID = -1;

				await Task.Run(() => userRDO = userRepository.ReadSingle(eddsUserArtifactID));
				if (userRDO != null)
				{
					retVal = userRDO.Groups.Select(x => x.ArtifactID).ToList();
				}
				return retVal;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.UserQueryError, ex);
			}
		}

		public async Task<Boolean> UserIsInAdministratorGroup(APIOptions rsapiApiOptions, IGenericRepository<kCura.Relativity.Client.DTOs.User> userRepository, IGenericRepository<Group> groupRepository, Int32 eddsUserArtifactID)
		{
			try
			{
				var retVal = false;
				rsapiApiOptions.WorkspaceID = -1;
				var systemAdminGroupArtifactID = await RetrieveSystemAdminGroupArtifactID(rsapiApiOptions, groupRepository);
				if (systemAdminGroupArtifactID > -1)
				{
					var userGroupArtifactIDs = await RetrieveUserGroupArtifactIDs(rsapiApiOptions, userRepository, eddsUserArtifactID);
					if (userGroupArtifactIDs != null && userGroupArtifactIDs.Any())
					{
						if (userGroupArtifactIDs.Contains(systemAdminGroupArtifactID))
						{
							retVal = true;
						}
					}
				}
				return retVal;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.CheckIfUserIsAdminError, ex);
			}
		}
	}
}
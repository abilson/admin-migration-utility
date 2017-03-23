using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.SQL;
using kCura.EventHandler;
using Relativity.API;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdminMigrationUtility.EventHandlers.Import
{
	public class ImportJob
	{
		public IServicesMgr SvcManager { get; set; }
		public IArtifactQueries ArtifactQueries { get; set; }
		public ExecutionIdentity IdentityCurrentUser { get; set; }
		public int WorkspaceArtifactId { get; set; }
		public IDBContext DbContext { get; set; }
		public ISqlQueryHelper SqlQueryHelper { get; set; }
		public Artifact ActiveArtifact { get; set; }
		public Int32 ActiveArtifactId { get; set; }
		public string TempTableName { get; set; }
		public IAPILog Logger { get; set; }

		public ImportJob()
		{
		}

		public ImportJob(IServicesMgr svcMgr, IArtifactQueries artifactQueries, IDBContext dbContext, ISqlQueryHelper sqlQueryHelper, Int32 workspaceArtifactId, ExecutionIdentity identity, Artifact activeArtifact, IAPILog logger)
		{
			SvcManager = svcMgr;
			ArtifactQueries = artifactQueries;
			DbContext = dbContext;
			SqlQueryHelper = sqlQueryHelper;
			WorkspaceArtifactId = workspaceArtifactId;
			IdentityCurrentUser = identity;
			ActiveArtifact = activeArtifact;
			Logger = logger;
		}

		public ImportJob(IServicesMgr svcMgr, IArtifactQueries artifactQueries, Int32 workspaceArtifactId, ExecutionIdentity identity, Int32 activeArtifactId, IAPILog logger)
		{
			SvcManager = svcMgr;
			ArtifactQueries = artifactQueries;
			IdentityCurrentUser = identity;
			WorkspaceArtifactId = workspaceArtifactId;
			ActiveArtifactId = activeArtifactId;
			Logger = logger;
		}

		public ImportJob(IServicesMgr svcMgr, ISqlQueryHelper sqlQueryHelper, IArtifactQueries artifactQueries, Int32 workspaceArtifactId, ExecutionIdentity identity, IDBContext dbContext, string tempJobErrorsTableName, IAPILog logger)
		{
			SvcManager = svcMgr;
			SqlQueryHelper = sqlQueryHelper;
			ArtifactQueries = artifactQueries;
			IdentityCurrentUser = identity;
			WorkspaceArtifactId = workspaceArtifactId;
			DbContext = dbContext;
			TempTableName = tempJobErrorsTableName;
			Logger = logger;
		}

		public async Task<Response> ExecutePreSave(bool isArtifactNew, Int32 statusFieldArtifactId)
		{
			var response = new Response { Success = true, Message = string.Empty };

			try
			{
				ValidateFileExtension(response);
				ValidateEmailTo(response);
				if (isArtifactNew)
				{
					//Update the Status field
					ActiveArtifact.Fields[statusFieldArtifactId].Value.Value = Constant.Status.Job.NEW;
				}
				else
				{
					var status = await ArtifactQueries.RetrieveRdoJobStatusAsync(SvcManager, WorkspaceArtifactId, IdentityCurrentUser, ActiveArtifact.ArtifactID);

					if (!string.IsNullOrEmpty(status))
					{
						switch (status)
						{
							case Constant.Status.Job.SUBMITTED:
							case Constant.Status.Job.IN_PROGRESS:
							case Constant.Status.Job.IN_PROGRESS_MANAGER:
							case Constant.Status.Job.IN_PROGRESS_WORKER:
							case Constant.Status.Job.COMPLETED_MANAGER:
							case Constant.Status.Job.CANCELREQUESTED:
							case Constant.Status.Job.RETRY:
								response.Success = false;
								response.Message = Constant.ErrorMessages.NotSupportedJobEdit;
								break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = $"{Constant.ErrorMessages.DefaultErrorPrepend}, Error Message: {ex}";
			}
			return response;
		}

		public async Task<Response> ExecutePreCascadeDelete()
		{
			var response = new Response { Success = true, Message = string.Empty };

			try
			{
				var dtImportJobErrors = SqlQueryHelper.RetrieveJobErrorsFromTempTable(DbContext, TempTableName);

				if (dtImportJobErrors != null && dtImportJobErrors.Rows.Count > 0)
				{
					var jobArtifactIDs = dtImportJobErrors.Rows.OfType<DataRow>().Select(dr => dr.Field<Int32>("ArtifactID")).ToList();
					var setStatus = await ArtifactQueries.RetrieveRdoJobStatusAsync(SvcManager, WorkspaceArtifactId, IdentityCurrentUser, jobArtifactIDs[0]);

					if (!string.IsNullOrEmpty(setStatus))
					{
						switch (setStatus)
						{
							case Constant.Status.Job.SUBMITTED:
							case Constant.Status.Job.IN_PROGRESS_MANAGER:
							case Constant.Status.Job.IN_PROGRESS_WORKER:
							case Constant.Status.Job.COMPLETED_MANAGER:
							case Constant.Status.Job.CANCELREQUESTED:
							case Constant.Status.Job.RETRY:
								response.Success = false;
								response.Exception = new SystemException(Constant.ErrorMessages.NotSupportedJobDeletion);
								break;

							case Constant.Status.Job.CANCELLED:
							case Constant.Status.Job.COMPLETED:
							case Constant.Status.Job.COMPLETED_WITH_ERRORS:
							case Constant.Status.Job.ERROR:
							case Constant.Status.Job.NEW:
								Logger.LogDebug($"{Constant.Names.ApplicationName} - Delete Import Job button clicked.");

								if (jobArtifactIDs.Count > 0)
								{
									Logger.LogDebug($"{Constant.Names.ApplicationName} - Delete Import Job button click, {jobArtifactIDs.Count} Import Job Errors found.");

									var jobErrorArtifactIDs = await ArtifactQueries.QueryJobErrors(SvcManager, IdentityCurrentUser, WorkspaceArtifactId, jobArtifactIDs[0], Constant.Guids.ObjectType.ImportUtilityJobErrors, Constant.Guids.Field.ImportUtilityJobErrors.ImportUtilityJob);
									var errorArtifactIDs = jobErrorArtifactIDs as int[] ?? jobErrorArtifactIDs.ToArray();
									if (jobErrorArtifactIDs != null & errorArtifactIDs.Any())
									{
										var artifactTypeId = SqlQueryHelper.RetrieveArtifactTypeIdByGuidAsync(DbContext, Constant.Guids.ObjectType.ImportUtilityJobErrors).Result;
										await ArtifactQueries.DeleteJobErrors(SvcManager, IdentityCurrentUser, WorkspaceArtifactId, jobErrorArtifactIDs.ToList(), artifactTypeId);
										Logger.LogDebug($"{Constant.Names.ApplicationName} - Delete Import Job button click, {jobArtifactIDs.Count} Import Job Errors deleted.");
									}
								}
								else
								{
									Logger.LogDebug($"{Constant.Names.ApplicationName} - Delete Import Job button click, no Import Job Errors found.");
								}

								Logger.LogDebug($"{Constant.Names.ApplicationName} - Delete Import Job button click action completed.");
								break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Exception = new SystemException($@"{Constant.ErrorMessages.DefaultErrorPrepend}, Error Message: {ex}");
			}

			return response;
		}

		public async Task<Response> ExecutePreDelete()
		{
			Logger.LogDebug($"{Constant.Names.ApplicationName} - Entering ExecutePreDelete().");

			var response = new Response { Success = true, Message = string.Empty };

			try
			{
				var setStatus = await ArtifactQueries.RetrieveRdoJobStatusAsync(SvcManager, WorkspaceArtifactId, IdentityCurrentUser, ActiveArtifactId);

				if (!string.IsNullOrEmpty(setStatus))
				{
					switch (setStatus)
					{
						case Constant.Status.Job.SUBMITTED:
						case Constant.Status.Job.IN_PROGRESS_MANAGER:
						case Constant.Status.Job.IN_PROGRESS_WORKER:
						case Constant.Status.Job.COMPLETED_MANAGER:
						case Constant.Status.Job.CANCELREQUESTED:
						case Constant.Status.Job.RETRY:
							response.Success = false;
							response.Exception = new SystemException(Constant.ErrorMessages.NotSupportedJobDeletion);
							break;

						case Constant.Status.Job.CANCELLED:
						case Constant.Status.Job.COMPLETED:
						case Constant.Status.Job.COMPLETED_WITH_ERRORS:
						case Constant.Status.Job.ERROR:
						case Constant.Status.Job.NEW:
							//Do nothing
							break;
					}
				}
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Exception = new SystemException($"{Constant.ErrorMessages.DefaultErrorPrepend}, Error Message: {ex}");
			}

			Logger.LogDebug($"{Constant.Names.ApplicationName} - Exiting ExecutePreDelete().");
			return response;
		}

		private void ValidateEmailTo(Response response)
		{
			var field = ActiveArtifact.Fields[Constant.Guids.Field.ImportUtilityJob.EmailAddresses.ToString()];
			if (field.Value != null && field.Value.Value != null)
			{
				var emailTo = field.Value.Value.ToString();
				Utility.ValidateEmailAddresses(emailTo, Constant.CommaSeparator, response);
			}
		}

		private void ValidateFileExtension(Response response)
		{
			var field = ActiveArtifact.Fields[Constant.Guids.Field.ImportUtilityJob.ImportFile.ToString()];
			if (field != null)
			{
				var fieldValue = (FileFieldValue)field.Value;
				var fileName = fieldValue.FileValue.FileName;
				var supportedFileExtensions = Enum.GetNames(typeof(Constant.SupportedImportFileTypes)).Select(x => "." + x.ToLower()).ToList();
				var extension = Path.GetExtension(fileName);
				if (extension != null && !supportedFileExtensions.Contains(extension.ToLower()))
				{
					var errorMsg = String.Format(Constant.ErrorMessages.UnsupportedFileType, String.Join(",", supportedFileExtensions));
					response.Success = false;
					response.Message = errorMsg;
				}
			}
		}
	}
}
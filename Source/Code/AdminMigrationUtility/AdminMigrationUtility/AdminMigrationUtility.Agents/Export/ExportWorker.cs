using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Kepler;
using AdminMigrationUtility.Helpers.Rsapi;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.Serialization;
using AdminMigrationUtility.Helpers.SQL;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Services.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Agents.Export
{
	// Change the name and guid for your application
	[kCura.Agent.CustomAttributes.Name("AdminMigrationUtility - Export Worker")]
	[System.Runtime.InteropServices.Guid("B79CF829-8B4E-4152-885F-F5232F3E37BA")]
	public class ExportWorker : kCura.Agent.AgentBase
	{
		private IAPILog _logger;

		public override void Execute()
		{
			ExecuteAsync().Wait();
		}

		public async Task ExecuteAsync()
		{
			ISqlQueryHelper sqlQueryHelper = new SqlQueryHelper();
			IEnumerable<Int32> resourceGroupIds = GetResourceGroupIDs();
			_logger = Helper.GetLoggerFactory().GetLogger();
			IArtifactQueries artifactQueries = new ArtifactQueries();

			//Setup RSAPI repositories
			APIOptions rsapiApiOptions = new APIOptions
			{
				WorkspaceID = -1
			};
			IRSAPIClient rsapiClient = Helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser);
			rsapiClient.APIOptions = rsapiApiOptions;
			IRsapiRepositoryGroup rsapiRepositoryGroup = new RSAPIiRepositoryGroup(rsapiClient);
			ILoginProfileManager loginProfileManager = Helper.GetServicesManager().CreateProxy<ILoginProfileManager>(ExecutionIdentity.CurrentUser);
			IAuthenticationHelper authenticationHelper = new AuthenticationHelper(loginProfileManager);
			ISerializationHelper serializationHelper = new SerializationHelper();

			ExportWorkerJob job = new ExportWorkerJob(
				agentId: AgentID,
				agentHelper: Helper,
				sqlQueryHelper: sqlQueryHelper,
				processedOnDateTime: DateTime.Now,
				resourceGroupIds: resourceGroupIds,
				logger: _logger,
				artifactQueries: artifactQueries,
				rsapiApiOptions: rsapiApiOptions,
				rsapiRepositoryGroup: rsapiRepositoryGroup,
				authenticationHelper: authenticationHelper,
				serializationHelper: serializationHelper);
			job.OnMessage += MessageRaised;

			try
			{
				RaiseMessage("Enter Agent", 10);
				await job.ExecuteAsync();
				RaiseMessage("Exit Agent", 10);
			}
			catch (Exception ex)
			{
				//Raise an error on the agents tab and event viewer
				RaiseError(ex.ToString(), ex.ToString());
				_logger.LogError(ex, $"{Constant.Names.ApplicationName} - {ex}");

				//Add the error to our custom Errors table
				sqlQueryHelper.InsertRowIntoExportErrorLogAsync(Helper.GetDBContext(-1), job.WorkspaceArtifactId, Constant.Tables.ExportWorkerQueue, job.TableRowId, job.AgentId, ex.ToString()).Wait();

				//Add the error to the Relativity Errors tab
				//this second try catch is in case we have a problem connecting to the RSAPI
				try
				{
					ErrorQueries.WriteError(Helper.GetServicesManager(), ExecutionIdentity.System, job.WorkspaceArtifactId, ex);
				}
				catch (Exception rsapiException)
				{
					RaiseError(rsapiException.ToString(), rsapiException.ToString());
					_logger.LogError(rsapiException, $"{Constant.Names.ApplicationName} - {rsapiException}");
				}

				//Set the status in the queue to error
				sqlQueryHelper.UpdateStatusInExportWorkerQueueAsync(Helper.GetDBContext(-1), Constant.Status.Queue.ERROR, job.BatchTableName).Wait();
			}
			finally
			{
				rsapiClient.Dispose();
			}
		}

		public override String Name
		{
			get
			{
				return "AdminMigrationUtility - Worker";
			}
		}

		private void MessageRaised(Object sender, String message)
		{
			RaiseMessage(message, 10);
		}
	}
}
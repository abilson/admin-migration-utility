using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Rsapi;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.Serialization;
using AdminMigrationUtility.Helpers.SQL;
using kCura.Relativity.Client;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Agents.Export
{
	// Change the name and guid for your application
	[kCura.Agent.CustomAttributes.Name("AdminMigrationUtility - Export Manager")]
	[System.Runtime.InteropServices.Guid("0A4297DC-6EE9-4CB4-B71A-D98B967403FB")]
	public class ExportManager : kCura.Agent.AgentBase
	{
		private IAPILog _logger;

		public override void Execute()
		{
			ExecuteAsync().Wait();
		}

		public async Task ExecuteAsync()
		{
			SqlQueryHelper sqlQueryHelper = new SqlQueryHelper();
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
			ISerializationHelper serializationHelper = new SerializationHelper();

			ExportManagerJob job = new ExportManagerJob(
				agentId: AgentID,
				agentHelper: Helper,
				sqlQueryHelper: sqlQueryHelper,
				processedOnDateTime: DateTime.Now,
				resourceGroupIds: resourceGroupIds,
				logger: _logger,
				artifactQueries: artifactQueries,
				rsapiApiOptions: rsapiApiOptions,
				rsapiRepositoryGroup: rsapiRepositoryGroup,
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
				sqlQueryHelper.InsertRowIntoExportErrorLogAsync(Helper.GetDBContext(-1), job.WorkspaceArtifactId, Constant.Tables.ExportManagerQueue, job.TableRowId, job.AgentId, ex.ToString()).Wait();

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
				sqlQueryHelper.UpdateStatusInExportManagerQueueAsync(
					eddsDbContext: Helper.GetDBContext(-1),
					queueRowId: job.TableRowId,
					queueStatus: Constant.Status.Queue.ERROR).Wait();
			}
			finally
			{
				rsapiClient.Dispose();
			}
		}

		public override String Name
		{
			get { return "AdminMigrationUtility - Export Manager"; }
		}

		private void MessageRaised(Object sender, String message)
		{
			RaiseMessage(message, 10);
		}
	}
}
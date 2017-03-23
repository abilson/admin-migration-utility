using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Rsapi;
using AdminMigrationUtility.Helpers.Serialization;
using AdminMigrationUtility.Helpers.SQL;
using kCura.Relativity.Client;
using Relativity.API;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Agents.Import
{
	[kCura.Agent.CustomAttributes.Name("AdminMigrationUtility - Import Worker")]
	[System.Runtime.InteropServices.Guid("09C5E4C2-B163-4E78-B95F-BD8BF25C674B")]
	public class ImportWorker : kCura.Agent.AgentBase
	{
		private IAPILog _logger;

		public override void Execute()
		{
			ExecuteAsync().Wait();
		}

		public async Task ExecuteAsync()
		{
			var sqlQueryHelper = new SqlQueryHelper();
			var resourceGroupIds = GetResourceGroupIDs();
			_logger = Helper.GetLoggerFactory().GetLogger();
			var rsapiApiOptions = new APIOptions { WorkspaceID = -1 };
			var rsapiClient = Helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser);
			rsapiClient.APIOptions = rsapiApiOptions;
			var rsapiGroupRepository = new RSAPIiRepositoryGroup(rsapiClient);
			var artifactQueries = new ArtifactQueries();

			var job = new ImportWorkerJob(AgentID, Helper, sqlQueryHelper, artifactQueries, DateTime.Now, rsapiGroupRepository, resourceGroupIds, _logger, rsapiApiOptions, new SerializationHelper());
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
				sqlQueryHelper.InsertRowIntoImportErrorLogAsync(Helper.GetDBContext(-1), job.WorkspaceArtifactId, Constant.Tables.ExportWorkerQueue, job.TableRowId, job.AgentId, ex.ToString()).Wait();

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
			}
			finally
			{
				rsapiClient.Dispose();
			}
		}

		public override String Name
		{
			get { return "AdminMigrationUtility - Import Worker"; }
		}

		private void MessageRaised(Object sender, String message)
		{
			RaiseMessage(message, 10);
		}
	}
}
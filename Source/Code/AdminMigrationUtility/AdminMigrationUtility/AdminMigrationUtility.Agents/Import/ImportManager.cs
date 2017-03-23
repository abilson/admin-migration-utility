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
	[kCura.Agent.CustomAttributes.Name("AdminMigrationUtility - Import Manager")]
	[System.Runtime.InteropServices.Guid("A7CDA46D-002D-4B84-8EB9-E216D49B2F04")]
	public class ImportManager : kCura.Agent.AgentBase
	{
		private IAPILog _logger;
		private ImportManagerJob _managerJob;

		public override void Execute()
		{
			ExecuteAsync().Wait();
		}

		public async Task ExecuteAsync()
		{
			var sqlQueryHelper = new SqlQueryHelper();
			var artifactQueryHelper = new ArtifactQueries();
			var resourceGroupIds = GetResourceGroupIDs();
			_logger = Helper.GetLoggerFactory().GetLogger();
			var rsapiApiOptions = new APIOptions { WorkspaceID = -1 };
			var rsapiClient = Helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser);
			rsapiClient.APIOptions = rsapiApiOptions;
			var rsapiGroupRepository = new RSAPIiRepositoryGroup(rsapiClient);
			_managerJob = new ImportManagerJob(AgentID, Helper, sqlQueryHelper, artifactQueryHelper, DateTime.Now, resourceGroupIds, _logger, rsapiClient, rsapiGroupRepository, rsapiApiOptions, new SerializationHelper());
			_managerJob.OnMessage += MessageRaised;

			try
			{
				RaiseMessage("Enter Agent", 10);
				await _managerJob.ExecuteAsync();
				RaiseMessage("Exit Agent", 10);
			}
			catch (Exception ex)
			{
				//Raise an error on the agents tab and event viewer
				RaiseError(ex.ToString(), ex.ToString());
				_logger.LogError(ex, $"{Constant.Names.ApplicationName} - {ex}", _managerJob);

				//Add the error to our custom Errors table
				sqlQueryHelper.InsertRowIntoImportErrorLogAsync(Helper.GetDBContext(-1), _managerJob.WorkspaceArtifactId, Constant.Tables.ExportManagerQueue, _managerJob.TableRowId, _managerJob.AgentId, ex.ToString()).Wait();

				//Add the error to the Relativity Errors tab
				//this second try catch is in case we have a problem connecting to the RSAPI
				try
				{
					ErrorQueries.WriteError(Helper.GetServicesManager(), ExecutionIdentity.System, _managerJob.WorkspaceArtifactId, ex);
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
				_managerJob.Dispose();
			}
		}

		public override String Name
		{
			get { return "AdminMigrationUtility - Import Manager"; }
		}

		private void MessageRaised(Object sender, String message)
		{
			RaiseMessage(message, 10);
		}
	}
}
using AdminMigrationUtility.Helpers;
using Relativity.API;
using System;
using System.Threading.Tasks;
using AdminMigrationUtility.Helpers.SQL;

namespace AdminMigrationUtility.EventHandlers
{
	[kCura.EventHandler.CustomAttributes.RunOnce(false)]
	[kCura.EventHandler.CustomAttributes.Description("Creates the underlying tables for the application.")]
	[System.Runtime.InteropServices.Guid("A9483F1A-54C8-4991-AE0B-C7675532CF18")]
	public class PostInstallSetup : kCura.EventHandler.PostInstallEventHandler
	{
		private IAPILog _logger;

		public override kCura.EventHandler.Response Execute()
		{
			return ExecuteAsync().Result;
		}

		public async Task<kCura.EventHandler.Response> ExecuteAsync()
		{
			return await Task.Run(() =>
			{
				_logger = Helper.GetLoggerFactory().GetLogger();

				var response = new kCura.EventHandler.Response
				{
					Success = true,
					Message = String.Empty
				};
				var sqlQueryHelper = new SqlQueryHelper();

				//Create the Export Utility Job Manager queue table if it doesn't already exist
				_logger.LogDebug($"{Constant.Names.ApplicationName} - Creating Export Utility Job Manager queue table if it doesn't already exist");
				var exportManagerTableTask = sqlQueryHelper.CreateExportManagerQueueTableAsync(Helper.GetDBContext(-1));
				_logger.LogDebug($"{Constant.Names.ApplicationName} - Created Export Utility Job Manager queue table if it doesn't already exist");

				//Create the Export Utility Job Worker queue table if it doesn't already exist
				_logger.LogDebug($"{Constant.Names.ApplicationName} - Creating Export Utility Job Worker queue table if it doesn't already exist");
				var exportWorkerTableTask = sqlQueryHelper.CreateExportWorkerQueueTableAsync(Helper.GetDBContext(-1));
				_logger.LogDebug($"{Constant.Names.ApplicationName} - Created Export Utility Job Worker queue table if it doesn't already exist");

				//Create the Export Utility Job Error log table if it doesn't already exist
				_logger.LogDebug($"{Constant.Names.ApplicationName} - Creating Export Utility Job Error Log table if it doesn't already exist");
				var exportErrorLogTableTask = sqlQueryHelper.CreateExportErrorLogTableAsync(Helper.GetDBContext(-1));
				_logger.LogDebug($"{Constant.Names.ApplicationName} - Created Export Utility Job Error Log table if it doesn't already exist");

				//Create the Import Utility Job Manager queue table if it doesn't already exist
				_logger.LogDebug($"{Constant.Names.ApplicationName} - Creating Import Utility Job Manager queue table if it doesn't already exist");
				var importManagerTableTask = sqlQueryHelper.CreateImportManagerQueueTableAsync(Helper.GetDBContext(-1));
				_logger.LogDebug($"{Constant.Names.ApplicationName} - Created Import Utility Job Manager queue table if it doesn't already exist");

				//Create the Import Utility Job Worker queue table if it doesn't already exist
				_logger.LogDebug($"{Constant.Names.ApplicationName} - Creating Import Utility Job Worker queue table if it doesn't already exist");
				var importWorkerTableTask = sqlQueryHelper.CreateImportWorkerQueueTableAsync(Helper.GetDBContext(-1));
				_logger.LogDebug($"{Constant.Names.ApplicationName} - Created Import Utility Job Worker queue table if it doesn't already exist");

				//Create the Import Utility Job Error log table if it doesn't already exist
				_logger.LogDebug($"{Constant.Names.ApplicationName} - Creating Import Utility Job Error Log table if it doesn't already exist");
				var importErrorLogTableTask = sqlQueryHelper.CreateImportErrorLogTableAsync(Helper.GetDBContext(-1));
				_logger.LogDebug($"{Constant.Names.ApplicationName} - Created Import Utility Job Error Log table if it doesn't already exist");


				try
				{
					// Waits for all tasks, otherwise exceptions would be lost
					Task.WaitAll(exportManagerTableTask, exportWorkerTableTask, exportErrorLogTableTask, importManagerTableTask, importWorkerTableTask, importErrorLogTableTask);
				}
				catch (AggregateException aex)
				{
					_logger.LogError(aex, $"{Constant.Names.ApplicationName} - Post-Install failed. {aex}");
					var ex = aex.Flatten();
					var message = ex.Message + " : " + (ex.InnerException?.Message ?? "None");
					response.Success = false;
					response.Message = "Post-Install field rename failed with message: " + message;
				}
				return response;
			});
		}
	}
}

using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Exceptions;
using AdminMigrationUtility.Helpers.SQL;
using Relativity.API;
using System.Threading.Tasks;

namespace AdminMigrationUtility.EventHandlers.Application
{
	public class PostInstallSetupJob
	{
		public IDBContext DbContextEdds { get; set; }
		public ISqlQueryHelper SqlQueryHelper { get; set; }
		public IAPILog Logger { get; set; }

		public PostInstallSetupJob(IDBContext dbContextEdds, ISqlQueryHelper sqlQueryHelper, IAPILog logger)
		{
			DbContextEdds = dbContextEdds;
			SqlQueryHelper = sqlQueryHelper;
			Logger = logger;
		}

		public async Task<kCura.EventHandler.Response> ExecuteAsync()
		{
			var response = new kCura.EventHandler.Response { Success = true, Message = string.Empty };

			try
			{
				Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Creating tables...", this);

				//Create the Export Utility Job Manager queue table if it doesn't already exist
				Logger.LogDebug($"{Constant.Names.ApplicationName} - Creating Export Utility Job Manager queue table if it doesn't already exist.");
				await SqlQueryHelper.CreateExportManagerQueueTableAsync(DbContextEdds);
				Logger.LogDebug($"{Constant.Names.ApplicationName} - Created Export Utility Job Manager queue table if it doesn't already exist.");

				//Create the Export Utility Job Worker queue table if it doesn't already exist
				Logger.LogDebug($"{Constant.Names.ApplicationName} - Creating Export Utility Job Worker queue table if it doesn't already exist.");
				await SqlQueryHelper.CreateExportWorkerQueueTableAsync(DbContextEdds);
				Logger.LogDebug($"{Constant.Names.ApplicationName} - Created Export Utility Job Worker queue table if it doesn't already exist.");

				//Create the Export Utility Job Error log table if it doesn't already exist
				Logger.LogDebug($"{Constant.Names.ApplicationName} - Creating Export Utility Job Error Log table if it doesn't already exist.");
				await SqlQueryHelper.CreateExportErrorLogTableAsync(DbContextEdds);
				Logger.LogDebug($"{Constant.Names.ApplicationName} - Created Export Utility Job Error Log table if it doesn't already exist.");

				//Create the Import Utility Job Manager queue table if it doesn't already exist
				Logger.LogDebug($"{Constant.Names.ApplicationName} - Creating Import Utility Job Manager queue table if it doesn't already exist.");
				await SqlQueryHelper.CreateImportManagerQueueTableAsync(DbContextEdds);
				Logger.LogDebug($"{Constant.Names.ApplicationName} - Created Import Utility Job Manager queue table if it doesn't already exist.");

				//Create the Import Utility Job Worker queue table if it doesn't already exist
				Logger.LogDebug($"{Constant.Names.ApplicationName} - Creating Import Utility Job Worker queue table if it doesn't already exist.");
				await SqlQueryHelper.CreateImportWorkerQueueTableAsync(DbContextEdds);
				Logger.LogDebug($"{Constant.Names.ApplicationName} - Created Import Utility Job Worker queue table if it doesn't already exist.");

				//Create the Import Utility Job Error log table if it doesn't already exist
				Logger.LogDebug($"{Constant.Names.ApplicationName} - Creating Import Utility Job Error Log table if it doesn't already exist.");
				await SqlQueryHelper.CreateImportErrorLogTableAsync(DbContextEdds);
				Logger.LogDebug($"{Constant.Names.ApplicationName} - Created Import Utility Job Error Log table if it doesn't already exist.");

				Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Created tables successfully...", this);
			}
			catch (AdminMigrationUtilityException ex)
			{
				var message = ex.Message + " : " + (ex.InnerException?.Message ?? "None");
				Logger.LogError($"{Constant.Names.ApplicationName} - Post-Install failed. {message}");

				response.Success = false;
				response.Message = $"Post-Install table creation(s) failed with message: {ex.Message}";
				throw new AdminMigrationUtilityException(ex.Message);
			}

			return response;
		}
	}
}
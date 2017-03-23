using AdminMigrationUtility.Helpers.SQL;
using Relativity.API;
using System.Threading.Tasks;

namespace AdminMigrationUtility.EventHandlers.Application
{
	[kCura.EventHandler.CustomAttributes.RunOnce(false)]
	[kCura.EventHandler.CustomAttributes.Description("Creates the underlying tables for the application.")]
	[System.Runtime.InteropServices.Guid("A9483F1A-54C8-4991-AE0B-C7675532CF18")]
	public class PostInstallSetup : kCura.EventHandler.PostInstallEventHandler
	{
		public ISqlQueryHelper SqlQueryHelper = new SqlQueryHelper();
		public IAPILog Logger;
		public IDBContext DbContextEdds;
		public PostInstallSetupJob PostInstallJob;

		public override kCura.EventHandler.Response Execute()
		{
			return ExecuteAsync().Result;
		}

		public async Task<kCura.EventHandler.Response> ExecuteAsync()
		{
			LoadVariables();
			return await Task.Run(() => PostInstallJob.ExecuteAsync());
		}

		private void LoadVariables()
		{
			Logger = GetLoggerAsync().Result;
			DbContextEdds = Helper.GetDBContext(-1);
			PostInstallJob = new PostInstallSetupJob(DbContextEdds, SqlQueryHelper, Logger);
		}

		private async Task<IAPILog> GetLoggerAsync()
		{
			return await Task.Run(() => Helper.GetLoggerFactory().GetLogger());
		}
	}
}
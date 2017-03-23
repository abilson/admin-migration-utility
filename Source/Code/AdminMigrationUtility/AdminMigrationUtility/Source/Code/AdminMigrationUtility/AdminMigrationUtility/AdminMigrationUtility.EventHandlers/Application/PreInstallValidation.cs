using AdminMigrationUtility.Helpers.SQL;
using Relativity.API;
using System.Threading.Tasks;

namespace AdminMigrationUtility.EventHandlers.Application
{
	[kCura.EventHandler.CustomAttributes.Description("Determines whether the application has been installed in other Workspaces in the environment.")]
	[System.Runtime.InteropServices.Guid("64A1DD57-72C2-4186-9F1D-962B67FDDCD9")]
	public class PreInstallValidation : kCura.EventHandler.PreInstallEventHandler
	{
		public int WorkspaceArtifactId;
		public ISqlQueryHelper SqlQueryHelper = new SqlQueryHelper();
		public IAPILog Logger;
		public IDBContext DbContextEdds;
		public PreInstallValidationJob PreInstallJob;

		public override kCura.EventHandler.Response Execute()
		{
			return ExecuteAsync().Result;
		}

		public async Task<kCura.EventHandler.Response> ExecuteAsync()
		{
			LoadVariables();
			return await Task.Run(() => PreInstallJob.ExecuteAsync());
		}

		private void LoadVariables()
		{
			WorkspaceArtifactId = Helper.GetActiveCaseID();
			Logger = GetLoggerAsync().Result;
			DbContextEdds = Helper.GetDBContext(-1);
			PreInstallJob = new PreInstallValidationJob(WorkspaceArtifactId, DbContextEdds, SqlQueryHelper, Logger);
		}

		private async Task<IAPILog> GetLoggerAsync()
		{
			return await Task.Run(() => Helper.GetLoggerFactory().GetLogger());
		}
	}
}
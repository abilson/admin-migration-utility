using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.SQL;
using kCura.EventHandler;
using Relativity.API;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.EventHandlers.Application
{
	public class PreUninstallJob
	{
		public IDBContext DbContextEdds { get; set; }
		public IDBContext DbContext { get; set; }
		public int WorkspaceArtifactId { get; set; }
		public ISqlQueryHelper SqlQueryHelper { get; set; }
		public IAPILog Logger { get; set; }

		public PreUninstallJob()
		{
		}

		public PreUninstallJob(IDBContext dbContextEdds, IDBContext dbContext, ISqlQueryHelper sqlQueryHelper, Int32 workspaceArtifactId, IAPILog logger)
		{
			DbContextEdds = dbContextEdds;
			DbContext = dbContext;
			SqlQueryHelper = sqlQueryHelper;
			WorkspaceArtifactId = workspaceArtifactId;
			Logger = logger;
		}

		public async Task<Response> ExecutePreUninstall()
		{
			var response = new Response { Success = true, Message = string.Empty };

			try
			{
				Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Checking to see if this application exists in more than one Workspace.", this);

				if (SqlQueryHelper.IsLastApplicationInEnvironment(DbContextEdds, WorkspaceArtifactId).Result)
				{
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] This application exists in more than one Workspace, proceeding with deletion of Workspace-specific records from EDDS tables.", this);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Starting to delete Workspace records from the " + Constant.Tables.ExportManagerQueue + " table.", this);
					await SqlQueryHelper.RemoveWorkspaceEntriesFromEddsTable(DbContextEdds, WorkspaceArtifactId, Constant.Tables.ExportManagerQueue);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Completed deletion of Workspace records from the " + Constant.Tables.ExportManagerQueue + " table.", this);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Starting to delete Workspace records from the " + Constant.Tables.ExportWorkerQueue + " table.", this);
					await SqlQueryHelper.RemoveWorkspaceEntriesFromEddsTable(DbContextEdds, WorkspaceArtifactId, Constant.Tables.ExportWorkerQueue);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Completed deletion of Workspace records from the " + Constant.Tables.ExportWorkerQueue + " table.", this);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Starting to delete Workspace records from the " + Constant.Tables.ExportErrorLog + " table.", this);
					await SqlQueryHelper.RemoveWorkspaceEntriesFromEddsTable(DbContextEdds, WorkspaceArtifactId, Constant.Tables.ExportErrorLog);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Completed deletion of Workspace records from the " + Constant.Tables.ExportErrorLog + " table.", this);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Starting to delete Workspace records from the " + Constant.Tables.ImportManagerQueue + " table.", this);
					await SqlQueryHelper.RemoveWorkspaceEntriesFromEddsTable(DbContextEdds, WorkspaceArtifactId, Constant.Tables.ImportManagerQueue);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Completed deletion of Workspace records from the " + Constant.Tables.ImportManagerQueue + " table.", this);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Starting to delete Workspace records from the " + Constant.Tables.ImportWorkerQueue + " table.", this);
					await SqlQueryHelper.RemoveWorkspaceEntriesFromEddsTable(DbContextEdds, WorkspaceArtifactId, Constant.Tables.ImportWorkerQueue);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Completed deletion of Workspace records from the " + Constant.Tables.ImportWorkerQueue + " table.", this);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Starting to delete Workspace records from the " + Constant.Tables.ImportErrorLog + " table.", this);
					await SqlQueryHelper.RemoveWorkspaceEntriesFromEddsTable(DbContextEdds, WorkspaceArtifactId, Constant.Tables.ImportErrorLog);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Completed deletion of Workspace records from the " + Constant.Tables.ImportErrorLog + " table.", this);
				}
				else
				{
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] This application does not exist in more than one Workspace, proceeding with deletion of application tables from EDDS.", this);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Attempting to drop the " + Constant.Tables.ExportManagerQueue + " table.", this);
					await SqlQueryHelper.DropEddsWorkspaceTableByTableName(DbContextEdds, Constant.Tables.ExportManagerQueue);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Completed dropping the " + Constant.Tables.ExportManagerQueue + " table.", this);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Attempting to drop the " + Constant.Tables.ExportWorkerQueue + " table.", this);
					await SqlQueryHelper.DropEddsWorkspaceTableByTableName(DbContextEdds, Constant.Tables.ExportWorkerQueue);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Completed dropping the " + Constant.Tables.ExportWorkerQueue + " table.", this);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Attempting to drop the " + Constant.Tables.ExportErrorLog + " table.", this);
					await SqlQueryHelper.DropEddsWorkspaceTableByTableName(DbContextEdds, Constant.Tables.ExportErrorLog);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Completed dropping the " + Constant.Tables.ExportErrorLog + " table.", this);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Attempting to drop the " + Constant.Tables.ImportManagerQueue + " table.", this);
					await SqlQueryHelper.DropEddsWorkspaceTableByTableName(DbContextEdds, Constant.Tables.ImportManagerQueue);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Completed dropping the " + Constant.Tables.ImportManagerQueue + " table.", this);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Attempting to drop the " + Constant.Tables.ImportWorkerQueue + " table.", this);
					await SqlQueryHelper.DropEddsWorkspaceTableByTableName(DbContextEdds, Constant.Tables.ImportWorkerQueue);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Completed dropping the " + Constant.Tables.ImportWorkerQueue + " table.", this);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Attempting to drop the " + Constant.Tables.ImportErrorLog + " table.", this);
					await SqlQueryHelper.DropEddsWorkspaceTableByTableName(DbContextEdds, Constant.Tables.ImportErrorLog);
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Completed dropping the " + Constant.Tables.ImportErrorLog + " table.", this);
				}
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = $"{Constant.ErrorMessages.DefaultErrorPrepend}, Error Message: {ex}";
			}
			return response;
		}
	}
}
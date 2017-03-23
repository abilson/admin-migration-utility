using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Exceptions;
using AdminMigrationUtility.Helpers.SQL;
using Relativity.API;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.EventHandlers.Application
{
	public class PreInstallValidationJob
	{
		public int WorkspaceArtifactId { get; set; }
		public IDBContext DbContextEdds { get; set; }
		public ISqlQueryHelper SqlQueryHelper { get; set; }
		public IAPILog Logger { get; set; }

		public PreInstallValidationJob(int workspaceArtifactId, IDBContext dbContextEdds, ISqlQueryHelper sqlQueryHelper, IAPILog logger)
		{
			WorkspaceArtifactId = workspaceArtifactId;
			DbContextEdds = dbContextEdds;
			SqlQueryHelper = sqlQueryHelper;
			Logger = logger;
		}

		public async Task<kCura.EventHandler.Response> ExecuteAsync()
		{
			var response = new kCura.EventHandler.Response { Success = true, Message = string.Empty };

			try
			{
				Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Checking to see if this application can be installed in this version of Relativity.", this);

				var currentRelativityVersion = Utility.GetRelativityVersion(typeof(kCura.EventHandler.Application));
				var supportedRelativityVersion = new Version(Constant.Relativity.SupportedVersion);

				if (currentRelativityVersion >= supportedRelativityVersion)
				{
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Checking to see if application is installed elsewhere.", this);

					if (await ApplicationInstalledInAnotherWorkspace(DbContextEdds))
					{
						Logger.LogDebug($"{Constant.Names.ApplicationName} - Application cannot be installed as it currently exists in another Workspace.");
						throw new AdminMigrationUtilityException(Constant.ErrorMessages.ApplicationInstallationFailure);
					}
					else
					{
						Logger.LogDebug($"{Constant.Names.ApplicationName} - Application does not exist elsewhere, proceeding with installation.");
					}
				}
				else
				{
					Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] This application cannot be installed in this version of Relativity. Your version of Relativity (" + currentRelativityVersion + ") must be equal to or greater than " + Constant.Relativity.SupportedVersion + ".", this);
					response.Success = false;
					response.Message = "Pre-Install failed with error: " + string.Format(Constant.ErrorMessages.UnsupportedRelativityVersion, Constant.Relativity.SupportedVersion);
				}
			}
			catch (AdminMigrationUtilityException aex)
			{
				Logger.LogError(aex, $"{Constant.Names.ApplicationName} - Pre-Install failed. {aex}");
				response.Success = false;
				response.Message = "Pre-Install failed with error: " + aex.Message;
			}

			Logger.LogDebug(Constant.Names.ApplicationName + " - [{@this}] Successfully checked to see if application is installed elsewhere.", this);
			return response;
		}

		private async Task<Boolean> ApplicationInstalledInAnotherWorkspace(IDBContext eddsDbContext)
		{
			var applicationDataRow = await SqlQueryHelper.RetrieveWorkspacesWhereAppIsInstalledAsync(eddsDbContext);
			return (applicationDataRow != null && (int)applicationDataRow["CaseArtifactId"] != WorkspaceArtifactId);
		}
	}
}
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using System;
using System.Collections.Generic;

namespace AdminMigrationUtility.Helpers.Rsapi
{
	public class ErrorQueries
	{
		#region Public Methods

		//Do not convert to async
		public static void WriteError(IServicesMgr svcMgr, ExecutionIdentity identity, Int32 workspaceArtifactID, Exception ex)
		{
			using (var client = svcMgr.CreateProxy<IRSAPIClient>(identity))
			{
				client.APIOptions.WorkspaceID = workspaceArtifactID;

				var res = WriteError(client, workspaceArtifactID, ex);
				if (!res.Success)
				{
					throw new Exception(res.Message);
				}
			}
		}

		#endregion Public Methods

		#region Private Methods

		//Do not convert to async
		private static Response<IEnumerable<Error>> WriteError(IRSAPIClient proxy, int workspaceArtifactId, Exception ex)
		{
			var artifact = new Error
			{
				FullError = ex.StackTrace,
				Message = ex.Message,
				SendNotification = false,
				Server = Environment.MachineName,
				Source = $"{Constant.Names.ApplicationName} [Guid={Constant.Guids.Application.ApplicationGuid}]",
				Workspace = new Workspace(workspaceArtifactId)
			};
			var theseResults = proxy.Repositories.Error.Create(artifact);
			return Response<Error>.CompileWriteResults(theseResults);
		}

		#endregion Private Methods
	}
}
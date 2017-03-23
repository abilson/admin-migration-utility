using Relativity.Services.Security.Models;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Kepler
{
	public interface IAuthenticationHelper
	{
		Task<LoginProfile> RetrieveExistingUserLoginProfileAsync(Int32 userArtifactId);
	}
}
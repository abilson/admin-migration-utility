using AdminMigrationUtility.Helpers.Exceptions;
using Relativity.Services.Security;
using Relativity.Services.Security.Models;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Kepler
{
	public class AuthenticationHelper : IAuthenticationHelper
	{
		public ILoginProfileManager LoginProfileManager { get; set; }

		public AuthenticationHelper(ILoginProfileManager loginProfileManager)
		{
			LoginProfileManager = loginProfileManager;
		}

		public async Task<LoginProfile> RetrieveExistingUserLoginProfileAsync(Int32 userArtifactId)
		{
			String errorContext = $"[UserArtifactId = {userArtifactId}]";

			try
			{
				LoginProfile userLoginProfile = await LoginProfileManager.GetLoginProfileAsync(userArtifactId);
				return userLoginProfile;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.RetrieveExistingUserLoginProfileError} {errorContext}", ex);
			}
		}
	}
}
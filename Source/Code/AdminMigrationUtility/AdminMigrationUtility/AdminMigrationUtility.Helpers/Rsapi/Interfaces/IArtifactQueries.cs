using AdminMigrationUtility.Helpers.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.Client.Repositories;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Rsapi.Interfaces
{
	public interface IArtifactQueries
	{
		Boolean UserHasAccessToArtifact(IServicesMgr servicesMgr, ExecutionIdentity executionIdentity, Int32 workspaceArtifactId, Guid guid, String artifactTypeName);

		Response<Boolean> UserHasAccessToRdoByType(IServicesMgr servicesMgr, ExecutionIdentity executionIdentity, Int32 workspaceArtifactId, Guid guid, String artifactTypeName);

		Task<Int32> GetResourcePoolArtifactIdAsync(IServicesMgr svcMgr, ExecutionIdentity executionIdentity, Int32 workspaceArtifactId);

		Task<IEnumerable<Result<kCura.Relativity.Client.DTOs.User>>> QueryAllUsersAsync(APIOptions rsapiApiOptions, IGenericRepository<kCura.Relativity.Client.DTOs.User> userRepository);

		Task<QueryResultSet<kCura.Relativity.Client.DTOs.User>> QueryAllUsersArtifactIDsAsync(APIOptions rsapiApiOptions, IGenericRepository<kCura.Relativity.Client.DTOs.User> userRepository);

		Task<kCura.Relativity.Client.DTOs.User> RetrieveUserAsync(APIOptions rsapiApiOptions, IGenericRepository<kCura.Relativity.Client.DTOs.User> userRepository, Int32 userArtifactID);

		Task<QueryResultSet<Client>> GetClientAsync(APIOptions apiOptions, IGenericRepository<Client> clientRdoRepository, String clientName);

		Task<QueryResultSet<Group>> QueryGroupsAsync(APIOptions apiOptions, IGenericRepository<Group> groupRepository, String[] groupNames);

		Task<String> RetrieveRdoJobStatusAsync(IServicesMgr svcMgr, Int32 workspaceArtifactId, ExecutionIdentity identity, Int32 artifactId);

		Task UpdateRdoJobTextFieldAsync(IServicesMgr svcMgr, Int32 workspaceArtifactId, ExecutionIdentity identity, Guid objectTypeGuid, Int32 artifactId, Guid textFieldGuid, String textFieldValue);

		Task<String> RetrieveRdoTextFieldValueAsync(APIOptions rsapiApiOptions, Int32 workspaceArtifactId, IGenericRepository<RDO> rdoRepository, Int32 rdoArtifactId, Guid textFieldGuid);

		Task UpdateRdoTextFieldValueAsync(APIOptions rsapiApiOptions, Int32 workspaceArtifactId, IGenericRepository<RDO> rdoRepository, Int32 rdoArtifactId, Guid jobObjectGuid, Guid textFieldGuid, Object fieldValue);

		Task<Int32> RetrieveWorkspaceResourcePoolArtifactIdAsync(APIOptions rsapiApiOptions, Int32 eddsWorkspaceArtifactId, IGenericRepository<Workspace> workspaceRepository, Int32 workspaceArtifactId);

		Task CreateJobErrorRecordAsync(APIOptions rsapiApiOptions, Int32 workspaceArtifactId, IGenericRepository<RDO> rdoRepository, Guid rdoGuid, Int32 jobArtifactId, String jobType, String statusFieldValue, String detailsFieldValue);

		Task<RDO> RetrieveJob(IGenericRepository<RDO> rdoRepository, APIOptions rsapiApiOptions, Int32 workspaceArtifactId, Int32 jobArtifactID);

		Task<String> GetChoiceNameByArtifactID(IGenericRepository<kCura.Relativity.Client.DTOs.Choice> choiceRepository, APIOptions rsapiApiOptions, Int32 choiceArtifactId);

		Task<String> GetClientNameByArtifactID(IGenericRepository<Client> clientRepository, APIOptions rsapiApiOptions, Int32 clientArtifactId);

		Task DownloadFile(IRSAPIClient client, APIOptions apiOptions, Int32 workspaceArtifactID, Int32 objectArtifactID, Int32 fieldArtifactID, String fullDownloadLocation);

		Task ResetExportJobStatisticsAsync(IServicesMgr svcMgr, Int32 workspaceArtifactId, ExecutionIdentity identity, Int32 artifactId);

		Task<List<Int32>> QueryGroupArtifactIdsUserIsPartOfAsync(APIOptions rsapiApiOptions, Int32 eddsWorkspaceArtifactId, IGenericRepository<kCura.Relativity.Client.DTOs.User> userRepository, Int32 userArtifactId);

		Task<IEnumerable<String>> QueryGroupsNamesForArtifactIdsAsync(APIOptions rsapiApiOptions, Int32 eddsWorkspaceArtifactId, IGenericRepository<Group> groupRepository, IEnumerable<Int32> groupArtifactIdList);

		Task<Boolean> JobErrorRecordExistsAsync(APIOptions rsapiApiOptions, Int32 workspaceArtifactId, IGenericRepository<RDO> rdoRepository, Guid rdoGuid, Guid associatedJobFieldGuid, Int32 jobArtifactId);

		Task<IEnumerable<ImportJobError>> GetImportJobErrorsAsync(APIOptions rsapiApiOptions, Int32 workspaceArtifactId, IGenericRepository<RDO> rdoRepository, Int32 jobArtifactId);

		Task UpdateImportJobStatistics(IGenericRepository<RDO> rdoRepository, APIOptions rsapiApiOptions, Int32 workspaceArtifactId, Int32 jobArtifactID, Int32 imported, Int32 notImported);

		Task UpdateExportJobStatitics(IGenericRepository<RDO> rdoRepository, APIOptions rsapiApiOptions, Int32 workspaceArtifactId, Int32 jobArtifactID, Int32 expectedNumberOfExports, Int32 actualNumberOfExports, Int32 notExported, Int32 recordsThatHaveBeenProcessed);

		Task CreateImportJobErrorRecordsAsync(APIOptions rsapiApiOptions, Int32 workspaceArtifactId, Int32 jobArtifactId, IGenericRepository<RDO> rdoRepository, IEnumerable<ImportJobError> errors, String jobType);

		Task AttachFileToExportJob(IRSAPIClient rsapiClient, APIOptions rsapiApiOptions, Int32 workspaceArtifactId, Int32 exportJobArtifactId, Int32 fileFieldArtifactId, String fileLocation);

		Task<Int32> RetrieveFieldArtifactIdByGuid(APIOptions rsapiApiOptions, Int32 workspaceArtifactId, IGenericRepository<kCura.Relativity.Client.DTOs.Field> fieldRepository, Guid fieldGuid);

		Task MarkImportJobAsSubmittedForMigration(IServicesMgr svcMgr, ExecutionIdentity identity, Int32 workspaceArtifactId, Int32 jobArtifactId);

		Task<IEnumerable<Int32>> QueryJobErrors(IServicesMgr svcManager, ExecutionIdentity identity, Int32 workspaceArtifactID, Int32 jobArtifactID, Guid rdoErrorJob, Guid rdoErrorJobField);

		Task DeleteJobErrors(IServicesMgr svcManager, ExecutionIdentity identity, Int32 workspaceArtifactID, List<Int32> jobErrorArtifactIDs, Int32 artifactTypeId);

		Task<Int32?> FindUserByEmailAddressAsync(APIOptions rsapiApiOptions, IGenericRepository<kCura.Relativity.Client.DTOs.User> userRepository, String emailAddress);

		Task<IEnumerable<Int32>> QueryJobErrors(APIOptions apiOptions, IGenericRepository<RDO> rdoRepository, Int32 workspaceArtifactID, Int32 jobArtifactID);

		Task<Boolean> UserIsInAdministratorGroup(APIOptions rsapiApiOptions, IGenericRepository<kCura.Relativity.Client.DTOs.User> userRepository, IGenericRepository<Group> groupRepository, Int32 eddsUserArtifactID);

		Task<IEnumerable<Int32>> RetrieveUserGroupArtifactIDs(APIOptions rsapiApiOptions, IGenericRepository<kCura.Relativity.Client.DTOs.User> userRepository, Int32 eddsUserArtifactID);
	}
}
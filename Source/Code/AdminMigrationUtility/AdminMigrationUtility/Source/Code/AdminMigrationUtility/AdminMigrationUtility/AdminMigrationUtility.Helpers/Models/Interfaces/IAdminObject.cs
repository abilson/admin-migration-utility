using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.SQL;
using kCura.Relativity.Client;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Models.Interfaces
{
	public interface IAdminObject
	{
		Task<IEnumerable<IObjectColumn>> GetColumnsAsync();

		Task<IEnumerable<String>> ExportAsync();

		Task MapObjectColumnsAsync(Dictionary<String, String> metaData);

		Task<Dictionary<String, String>> GetColumnsAsDictionaryAsync();

		Task<IEnumerable<String>> ImportAsync(APIOptions apiOptions, IRsapiRepositoryGroup repositoryGroup, IArtifactQueries artifactQueryHelper, IHelper helper, IDBContext eddsDbContext, ISqlQueryHelper sqlQueryHelper);

		Task<IEnumerable<String>> ValidateAsync(APIOptions apiOptions, IRsapiRepositoryGroup rsapiRepositoryGroup, IArtifactQueries artifactQueryHelper, IDBContext eddsDbContext, ISqlQueryHelper sqlQueryHelper);
	}
}
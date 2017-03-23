using AdminMigrationUtility.Helpers.Models;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.SQL
{
	public interface ISqlQueryHelper
	{
		Task CreateExportManagerQueueTableAsync(IDBContext eddsDbContext);

		Task CreateExportWorkerQueueTableAsync(IDBContext eddsDbContext);

		Task CreateImportManagerQueueTableAsync(IDBContext eddsDbContext);

		Task CreateImportWorkerQueueTableAsync(IDBContext eddsDbContext);

		Task CreateExportErrorLogTableAsync(IDBContext eddsDbContext);

		Task CreateImportErrorLogTableAsync(IDBContext eddsDbContext);

		Task InsertRowIntoExportErrorLogAsync(IDBContext eddsDbContext, Int32 workspaceArtifactId, String queueTableName, Int32 queueRecordId, Int32 agentId, String errorMessage);

		Task InsertRowIntoImportErrorLogAsync(IDBContext eddsDbContext, Int32 workspaceArtifactId, String queueTableName, Int32 queueRecordId, Int32 agentId, String errorMessage);

		Task<DataTable> RetrieveNextRowInExportManagerQueueAsync(IDBContext eddsDbContext, Int32 agentId, String commaDelimitedResourceAgentIds);

		Task<DataTable> RetrieveNextInImportManagerQueueAsync(IDBContext eddsDbContext, Int32 agentId, String commaDelimitedResourceAgentIds);

		Task ResetUnfishedJobsAsync(IDBContext eddsDbContext, Int32 agentId, String queueTableName);

		Task ResetUnfishedExportManagerJobsAsync(IDBContext eddsDbContext, Int32 agentId);

		Task ResetUnfishedExportWorkerJobsAsync(IDBContext eddsDbContext, Int32 agentId);

		Task RemoveRecordFromTableByIdAsync(IDBContext eddsDbContext, String queueTableName, Int32 id);

		Task InsertRowIntoExportManagerQueueAsync(IDBContext eddsDbContext, Int32 workspaceArtifactId, Int32 priority, Int32 exportJobArtifactId, Int32 resourceGroupId, string objectType);

		Task InsertRowIntoImportManagerQueueAsync(IDBContext eddsDbContext, Int32 workspaceArtifactId, Int32 priority, Int32 createdBy, Int32 artifactId, Int32 resourceGroupId, string objectType, string jobType);

		Task InsertRowsIntoImportWorkerQueueAsync(IDBContext eddsDbContext, Int32 jobId, Int32 priority, Int32 workspaceArtifactId, Int32 resourceGroupId, string objectType, string jobType, string metaData, int importRowId);

		Task UpdateStatusInExportManagerQueueAsync(IDBContext eddsDbContext, Int32 queueRowId, Int32 queueStatus);

		Task UpdateStatusInExportManagerQueueByWorkspaceJobIdAsync(IDBContext eddsDbContext, Int32 workspaceArtifactId, Int32 exportJobArtifactId, Int32 queueStatus);

		Task<DataTable> RetrieveAllExportManagerQueueRecords(IDBContext eddsDbContext);

		Task UpdateStatusInExportWorkerQueueAsync(IDBContext eddsDbContext, Int32 statusId, String uniqueTableName);

		Task UpdateStatusInImportManagerQueueAsync(IDBContext eddsDbContext, Int32 statusId, Int32 id);

		Task UpdateStatusInImportManagerQueueByWorkspaceJobIdAsync(IDBContext eddsDbContext, Int32 workspaceArtifactId, Int32 importJobArtifactId, Int32 queueStatus);

		Task UpdateStatusInImportWorkerQueueAsync(IDBContext eddsDbContext, Int32 statusId, String uniqueTableName);

		Task<DataTable> RetrieveNextBatchInExportWorkerQueueAsync(IDBContext eddsDbContext, Int32 agentId, Int32 batchSize, String uniqueBatchTableName, String commaDelimitedResourceAgentIds);

		Task<DataTable> RetrieveNextBatchInImportWorkerQueueAsync(IDBContext eddsDbContext, Int32 agentId, Int32 batchSize, String commaDelimitedResourceAgentIds);

		Task RemoveBatchFromExportWorkerQueueTableAsync(IDBContext eddsDbContext, String batchTableName);

		Task RemoveBatchFromImportWorkerQueueAsync(IDBContext eddsDbContext, String uniqueTableName);

		Task DropTableAsync(IDBContext eddsDbContext, String tableName);

		Task<DataTable> RetrieveAllInExportManagerQueueAsync(IDBContext eddsDbContext);

		Task<DataTable> RetrieveAllInImportManagerQueueAsync(IDBContext eddsDbContext);

		Task<DataRow> RetrieveSingleInExportManagerQueueByArtifactIdAsync(IDBContext eddsDbContext, Int32 exportJobArtifactId, Int32 workspaceArtifactId);

		Task<DataRow> RetrieveSingleInImportManagerQueueByArtifactIdAsync(IDBContext eddsDbContext, Int32 importJobArtifactId, Int32 workspaceArtifactId);

		Task<DataTable> RetrieveAllInExportWorkerQueueAsync(IDBContext eddsDbContext);

		Task<DataTable> RetrieveAllInImportWorkerQueueAsync(IDBContext eddsDbContext);

		Task<DataTable> RetrieveOffHoursAsync(IDBContext eddsDbContext);

		Task<KeywordsNotesModel> RetrieveKeywordsAndNotesForUserAsync(IDBContext eddsDbContext, Int32 userArtifactId);

		Task<DataTable> QueryChoiceArtifactId(IDBContext eddsDbContext, String codeTypeName, String codeName);

		Task InsertRowIntoExportWorkerQueueTableAsync(IDBContext eddsDbContext, Int32 workspaceArtifactId, Int32 exportJobArtifactId, String objectType, Int32? priority, Int32 workspaceResourceGroupArtifactId, Int32 queueStatus, Int32 artifactId, String exportWorkerResultTableName, String metadata, DateTime timeStampUtc);

		Task UpdateKeywordAndNotes(IDBContext eddsDbContext, Int32 objectArtifactID, String keywords, String notes);

		Task<DataRow> RetrieveWorkspacesWhereAppIsInstalledAsync(IDBContext eddsDbContext);

		Task BulkInsertIntoTableAsync(IDBContext dbContext, DataTable sourceDataTable, List<SqlBulkCopyColumnMapping> columnMappings, String destinationTableName, Int32 batchSize);

		Task BulkInsertIntoTableAsync(IDBContext dbContext, DataTable sourceDataTable, String destinationTableName, Int32 batchSize);

		Task CreateExportWorkerResultsTableIfItDoesNotAlreadyExistsAsync(IDBContext eddsDbContext, String tableName);

		Task InsertRowIntoExportWorkerResultsTableAsync(IDBContext eddsDbContext, String exportWorkerResultTableName, Int32 workspaceArtifactId, Int32 exportJobArtifactId, String objectType, Int32 workspaceResourceGroupArtifactId, Int32 queueStatus, String metadata, DateTime timeStampUtc);

		Task<Int32> CountImportWorkerRecordsAsync(IDBContext eddsDbContext, Int32 workspaceArtifactID, Int32 jobID);

		Task<DataTable> RetrieveAllImportManagerQueueRecords(IDBContext eddsDbContext);

		Task UpdateQueueStatusAsync(IDBContext eddsDbContext, Int32 workspaceArtifactID, Int32 jobID, String tableName, String statusColumnName, Int32 status);

		Task DeleteRecordFromQueueAsync(IDBContext eddsDbContext, String workspaceArtifactIDColumnName, Int32 workspaceArtifactID, String jobIDColumnName, Int32 jobID, String tableName);

		Task DeleteRecordFromQueueAsync(IDBContext eddsDbContext, Int32 agentID, Int32 workspaceArtifactID, Int32 jobID, String tableName);

		Task UpdateResultsTableNameInExportManagerQueueAsync(IDBContext eddsDbContext, Int32 queueRowId, String exportResultsTableName);

		Task UpdateStatusAndAgentIdInExportManagerQueueAsync(IDBContext eddsDbContext, Int32 queueRowId, Int32 queueStatus, Int32? agentId);

		Task<DataTable> RetrieveBatchFromExportWorkerResultsTableAsync(IDBContext eddsDbContext, Int32 agentId, Int32 batchSize, String exportResultsTableName);

		Task<Int32> RetrieveTotalRowCountInExportWorkerResultsTableAsync(IDBContext eddsDbContext, String exportResultsTableName);

		Task DeleteRecordsFromExportWorkerResultsTableAsync(IDBContext eddsDbContext, String exportResultsTableName, List<Int32> tableRowIds);

		Task<Int32> CountNumberOfExportWorkerRecordsAsync(IDBContext eddsDbContext, Int32 workspaceArtifactId, Int32 exportJobArtifactId);

		Task<DataTable> RetrieveAllQueueRecordsAsync(IDBContext eddsDbContext, String tableName, IEnumerable<String> orderByColumns);

		Task<String> RetrieveConfigurationValue(IDBContext eddsDbContext, String sectionName, String name);

		Task ResetUnfishedImportManagerJobsAsync(IDBContext eddsDbContext, Int32 agentId);

		Task<Int32?> GetQueueRecordCurrentStatus(IDBContext eddsDbContext, String workspaceIDColumnName, Int32 workspaceArtifactID, String JobIDColumnName, Int32 jobID, String tableName, String statusColumnName);

		DataTable RetrieveJobErrorsFromTempTable(IDBContext dbContext, string tempTableName);

		Task<Int32> RetrieveArtifactTypeIdByGuidAsync(IDBContext eddsDbContext, Guid objectTypeGuid);

		Task<bool> IsLastApplicationInEnvironment(IDBContext dbContextEdds, Int32 currentWorkspaceId);

		Task RemoveWorkspaceEntriesFromEddsTable(IDBContext dbContextEdds, Int32 currentWorkspaceId, string tableName);

		Task DropEddsWorkspaceTableByTableName(IDBContext dbContextEdds, string tableName);
	}
}
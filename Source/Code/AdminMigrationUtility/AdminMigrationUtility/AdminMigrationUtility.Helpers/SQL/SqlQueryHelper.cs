using AdminMigrationUtility.Helpers.Exceptions;
using AdminMigrationUtility.Helpers.Models;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.SQL
{
	public class SqlQueryHelper : ISqlQueryHelper
	{
		public async Task CreateExportManagerQueueTableAsync(IDBContext eddsDbContext)
		{
			String sql = $@"
				IF OBJECT_ID('[EDDSDBO].[{Constant.Tables.ExportManagerQueue}]') IS NULL
				BEGIN
					CREATE TABLE [EDDSDBO].[{Constant.Tables.ExportManagerQueue}]
					(
						[{Constant.Sql.ColumnsNames.ExportManagerQueue.TableRowId}] INT IDENTITY(1,1) PRIMARY KEY
						,[{Constant.Sql.ColumnsNames.ExportManagerQueue.AgentId}] INT
						,[{Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceArtifactId}] INT
						,[{Constant.Sql.ColumnsNames.ExportManagerQueue.ExportJobArtifactId}] INT
						,[{Constant.Sql.ColumnsNames.ExportManagerQueue.ObjectType}] NVARCHAR(200)
						,[{Constant.Sql.ColumnsNames.ExportManagerQueue.Priority}] INT
						,[{Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceResourceGroupArtifactId}] INT
						,[{Constant.Sql.ColumnsNames.ExportManagerQueue.QueueStatus}] INT
						,[{Constant.Sql.ColumnsNames.ExportManagerQueue.ResultsTableName}] NVARCHAR(200)
						,[{Constant.Sql.ColumnsNames.ExportManagerQueue.TimeStampUtc}] DATETIME
						,[{Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceName}] NVARCHAR(50)
					)
				END";

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
		}

		public async Task CreateExportWorkerQueueTableAsync(IDBContext eddsDbContext)
		{
			String sql = $@"
				IF OBJECT_ID('[EDDSDBO].[{Constant.Tables.ExportWorkerQueue}]') IS NULL
				BEGIN
					CREATE TABLE [EDDSDBO].[{Constant.Tables.ExportWorkerQueue}]
					(
						[{Constant.Sql.ColumnsNames.ExportWorkerQueue.TableRowId}] INT IDENTITY(1,1) PRIMARY KEY
						,[{Constant.Sql.ColumnsNames.ExportWorkerQueue.AgentId}] INT
						,[{Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceArtifactId}] INT
						,[{Constant.Sql.ColumnsNames.ExportWorkerQueue.ExportJobArtifactId}] INT
						,[{Constant.Sql.ColumnsNames.ExportWorkerQueue.ObjectType}] NVARCHAR(200)
						,[{Constant.Sql.ColumnsNames.ExportWorkerQueue.Priority}] INT
						,[{Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceResourceGroupArtifactId}] INT
						,[{Constant.Sql.ColumnsNames.ExportWorkerQueue.QueueStatus}] INT
						,[{Constant.Sql.ColumnsNames.ExportWorkerQueue.ArtifactId}] INT
						,[{Constant.Sql.ColumnsNames.ExportWorkerQueue.ResultsTableName}] NVARCHAR(200)
						,[{Constant.Sql.ColumnsNames.ExportWorkerQueue.MetaData}] NVARCHAR(MAX)
						,[{Constant.Sql.ColumnsNames.ExportWorkerQueue.TimeStampUtc}] DATETIME
					)
				END";

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
		}

		public async Task CreateExportWorkerResultsTableIfItDoesNotAlreadyExistsAsync(IDBContext eddsDbContext, String tableName)
		{
			String sql = $@"
				IF OBJECT_ID('[EDDSDBO].[{tableName}]') IS NULL
				BEGIN
					CREATE TABLE [EDDSDBO].[{tableName}]
					(
						[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.TableRowId}] INT IDENTITY(1,1) PRIMARY KEY
						,[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.AgentId}] INT
						,[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.WorkspaceArtifactId}] INT
						,[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.ExportJobArtifactId}] INT
						,[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.ObjectType}] NVARCHAR(200)
						,[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.WorkspaceResourceGroupArtifactId}] INT
						,[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.QueueStatus}] INT
						,[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.MetaData}] NVARCHAR(MAX)
						,[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.TimeStampUtc}] DATETIME
					)
				END";

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
		}

		public async Task CreateExportErrorLogTableAsync(IDBContext eddsDbContext)
		{
			String sql = $@"
				IF OBJECT_ID('[EDDSDBO].{Constant.Tables.ExportErrorLog}') IS NULL BEGIN
					CREATE TABLE [EDDSDBO].{Constant.Tables.ExportErrorLog}
					(
						ID INT IDENTITY(1,1)
						,AgentID INT
						,WorkspaceArtifactID INT
						,ApplicationName VARCHAR(500)
						,ApplicationGuid uniqueidentifier
						,QueueTableName NVARCHAR(MAX)
						,QueueRecordID INT
						,[Message] NVARCHAR(MAX)
						,[TimeStampUTC] DATETIME
					)
				END";

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
		}

		public async Task CreateImportManagerQueueTableAsync(IDBContext eddsDbContext)
		{
			String sql = $@"
				IF OBJECT_ID('[EDDSDBO].{Constant.Tables.ImportManagerQueue}') IS NULL BEGIN
					CREATE TABLE [EDDSDBO].{Constant.Tables.ImportManagerQueue}
					(
						[{Constant.Sql.ColumnsNames.ImportManagerQueue.ID}] INT IDENTITY(1,1) PRIMARY KEY
						,[{Constant.Sql.ColumnsNames.ImportManagerQueue.AgentID}] INT
						,[{Constant.Sql.ColumnsNames.ImportManagerQueue.JobID}] INT
						,[{Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceArtifactID}] INT
						,[{Constant.Sql.ColumnsNames.ImportManagerQueue.ObjectType}] NVARCHAR(200)
						,[{Constant.Sql.ColumnsNames.ImportManagerQueue.JobType}] NVARCHAR(100)
						,[{Constant.Sql.ColumnsNames.ImportManagerQueue.QueueStatus}] INT
						,[{Constant.Sql.ColumnsNames.ImportManagerQueue.Priority}] INT
						,[{Constant.Sql.ColumnsNames.ImportManagerQueue.ResourceGroupID}] INT
						,[{Constant.Sql.ColumnsNames.ImportManagerQueue.CreatedBy}] INT
						,[{Constant.Sql.ColumnsNames.ImportManagerQueue.TimeStampUTC}] DATETIME
						,[{Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceName}] NVARCHAR(50)
					)
				END";

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
		}

		public async Task CreateImportWorkerQueueTableAsync(IDBContext eddsDbContext)
		{
			String sql = $@"
				IF OBJECT_ID('[EDDSDBO].{Constant.Tables.ImportWorkerQueue}') IS NULL BEGIN
					CREATE TABLE [EDDSDBO].{Constant.Tables.ImportWorkerQueue}
					(
						[{Constant.Sql.ColumnsNames.ImportWorkerQueue.ID}] INT IDENTITY(1,1) PRIMARY KEY
						,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.AgentID}] INT
						,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.JobID}] INT
						,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.WorkspaceArtifactID}] INT
						,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.ObjectType}] NVARCHAR(200)
						,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.JobType}] NVARCHAR(100)
						,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.MetaData}] NVARCHAR(MAX)
						,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.ImportRowID}] INT
						,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.QueueStatus}] INT
						,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.Priority}] INT
						,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.ResourceGroupID}] INT
						,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.TimeStampUTC}] DATETIME
					)
				END";

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
		}

		public async Task CreateImportErrorLogTableAsync(IDBContext eddsDbContext)
		{
			String sql = $@"
				IF OBJECT_ID('[EDDSDBO].{Constant.Tables.ImportErrorLog}') IS NULL BEGIN
					CREATE TABLE [EDDSDBO].{Constant.Tables.ImportErrorLog}
					(
						ID INT IDENTITY(1,1)
						,AgentID INT
						,WorkspaceArtifactID INT
						,ApplicationName VARCHAR(500)
						,ApplicationGuid uniqueidentifier
						,QueueTableName NVARCHAR(MAX)
						,QueueRecordID INT
						,[Message] NVARCHAR(MAX)
						,[TimeStampUTC] DATETIME
					)
				END";

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
		}

		public async Task InsertRowIntoExportErrorLogAsync(IDBContext eddsDbContext, Int32 workspaceArtifactId, String queueTableName, Int32 queueRecordId, Int32 agentId, String errorMessage)
		{
			String sql = $@"
			INSERT INTO [EDDSDBO].{Constant.Tables.ExportErrorLog}
			(
				AgentID
				,WorkspaceArtifactID
				,ApplicationName
				,ApplicationGuid
				,QueueTableName
				,QueueRecordID
				,[Message]
				,[TimeStampUTC]
			)
			VALUES
			(
				@agentID
				,@workspaceArtifactID
				,@applicationName
				,@applicationGuid
				,@queueTableName
				,@queueRecordID
				,@message
				,GetUTCDate()
			)";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@agentID", SqlDbType.Int) {Value = agentId},
				new SqlParameter("@workspaceArtifactID", SqlDbType.Int) {Value = workspaceArtifactId},
				new SqlParameter("@applicationName", SqlDbType.VarChar) {Value = Constant.Names.ApplicationName},
				new SqlParameter("@applicationGuid", SqlDbType.UniqueIdentifier) {Value = Constant.Guids.Application.ApplicationGuid},
				new SqlParameter("@queueTableName", SqlDbType.VarChar) {Value = queueTableName},
				new SqlParameter("@queueRecordID", SqlDbType.Int) {Value = queueRecordId},
				new SqlParameter("@message", SqlDbType.NVarChar) {Value = errorMessage}
			};

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
		}

		public async Task InsertRowIntoImportErrorLogAsync(IDBContext eddsDbContext, Int32 workspaceArtifactId, String queueTableName, Int32 queueRecordId, Int32 agentId, String errorMessage)
		{
			String sql = $@"
			INSERT INTO [EDDSDBO].{Constant.Tables.ImportErrorLog}
			(
				AgentID
				,WorkspaceArtifactID
				,ApplicationName
				,ApplicationGuid
				,QueueTableName
				,QueueRecordID
				,[Message]
				,[TimeStampUTC]
			)
			VALUES
			(
				@agentID
				,@workspaceArtifactID
				,@applicationName
				,@applicationGuid
				,@queueTableName
				,@queueRecordID
				,@message
				,GetUTCDate()
			)";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@agentID", SqlDbType.Int) {Value = agentId},
				new SqlParameter("@workspaceArtifactID", SqlDbType.Int) {Value = workspaceArtifactId},
				new SqlParameter("@applicationName", SqlDbType.VarChar) {Value = Constant.Names.ApplicationName},
				new SqlParameter("@applicationGuid", SqlDbType.UniqueIdentifier) {Value = Constant.Guids.Application.ApplicationGuid},
				new SqlParameter("@queueTableName", SqlDbType.VarChar) {Value = queueTableName},
				new SqlParameter("@queueRecordID", SqlDbType.Int) {Value = queueRecordId},
				new SqlParameter("@message", SqlDbType.NVarChar) {Value = errorMessage}
			};

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
		}

		public async Task<DataTable> RetrieveNextRowInExportManagerQueueAsync(IDBContext eddsDbContext, Int32 agentId, String commaDelimitedResourceAgentIds)
		{
			String sql = $@"
				SET NOCOUNT ON

				DECLARE @QueueRowId INT

				BEGIN TRAN
					SELECT TOP 1
						@QueueRowId = [{Constant.Sql.ColumnsNames.ExportManagerQueue.TableRowId}]
					FROM
						[EDDSDBO].[{Constant.Tables.ExportManagerQueue}] WITH(UPDLOCK,READPAST)
					WHERE
						[{Constant.Sql.ColumnsNames.ExportManagerQueue.AgentId}] IS NULL
						AND [{Constant.Sql.ColumnsNames.ExportManagerQueue.QueueStatus}] = @notStartedStatus
						AND [{Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceResourceGroupArtifactId}] IN ({commaDelimitedResourceAgentIds})
					ORDER BY
						[{Constant.Sql.ColumnsNames.ExportManagerQueue.Priority}] ASC
						,[{Constant.Sql.ColumnsNames.ExportManagerQueue.TimeStampUtc}] ASC

					UPDATE
						[EDDSDBO].[{Constant.Tables.ExportManagerQueue}]
					SET
						[{Constant.Sql.ColumnsNames.ExportManagerQueue.AgentId}] = @agentId,
						[{Constant.Sql.ColumnsNames.ExportManagerQueue.QueueStatus}] = @inProgressStatus
					WHERE
						[{Constant.Sql.ColumnsNames.ExportManagerQueue.TableRowId}] = @QueueRowId

				COMMIT
				SET NOCOUNT OFF

				SELECT
					[{Constant.Sql.ColumnsNames.ExportManagerQueue.TableRowId}]
					,[{Constant.Sql.ColumnsNames.ExportManagerQueue.AgentId}]
					,[{Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceArtifactId}]
					,[{Constant.Sql.ColumnsNames.ExportManagerQueue.ExportJobArtifactId}]
					,[{Constant.Sql.ColumnsNames.ExportManagerQueue.ObjectType}]
					,[{Constant.Sql.ColumnsNames.ExportManagerQueue.Priority}]
					,[{Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceResourceGroupArtifactId}]
					,[{Constant.Sql.ColumnsNames.ExportManagerQueue.QueueStatus}]
					,[{Constant.Sql.ColumnsNames.ExportManagerQueue.ResultsTableName}]
					,[{Constant.Sql.ColumnsNames.ExportManagerQueue.TimeStampUtc}]
					,[{Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceName}]
				FROM
					[EDDSDBO].[{Constant.Tables.ExportManagerQueue}] WITH(NOLOCK)
				WHERE
					[{Constant.Sql.ColumnsNames.ExportManagerQueue.TableRowId}] = @QueueRowId";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@notStartedStatus", SqlDbType.Int) {Value = Constant.Status.Queue.NOT_STARTED},
				new SqlParameter("@inProgressStatus", SqlDbType.Int) {Value = Constant.Status.Queue.IN_PROGRESS},
				new SqlParameter("@agentID", SqlDbType.Int) {Value = agentId}
			};

			DataTable dataTable = await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
			return dataTable;
		}

		public async Task<DataTable> RetrieveAllQueueRecordsAsync(IDBContext eddsDbContext, String tableName, IEnumerable<String> orderByColumns)
		{
			//formats a list of columns as "ORDER BY [PRIORITY],[TimeStampUTC]"
			var formattedOrderByColumns = Utility.FormatOrderByColumns(orderByColumns);

			String sql = $@"
					SELECT TOP {Constant.BatchSizes.MaximumQueueSelect} *
					FROM
						[EDDSDBO].[{tableName}] WITH(NOLOCK) 
					{formattedOrderByColumns}
					";
			DataTable dataTable = await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql));
			return dataTable;
		}

		public async Task<DataTable> RetrieveNextInImportManagerQueueAsync(IDBContext eddsDbContext, Int32 agentId, String commaDelimitedResourceAgentIds)
		{
			String validStatuses = String.Join(",", new[]
			{
				Constant.Status.Queue.NOT_STARTED
			});
			String sql = $@"
				SET NOCOUNT ON

				DECLARE @ID INT
				DECLARE @AgentArtifactID INT
				DECLARE @WorkspaceArtifactID INT
				DECLARE @Priority INT
				DECLARE @JobID INT
				DECLARE @ResourceGroupID INT
				DECLARE @ObjectType NVARCHAR(255)
				DECLARE @JobType NVARCHAR(255)
				DECLARE @QueueStatus INT
				DECLARE @WorkspaceName NVARCHAR(50)

				BEGIN TRAN
					SELECT TOP 1
							@ID = [{Constant.Sql.ColumnsNames.ImportManagerQueue.ID}],
							@WorkspaceArtifactID = [{Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceArtifactID}],
							@Priority = [{Constant.Sql.ColumnsNames.ImportManagerQueue.Priority}],
							@JobID = [{Constant.Sql.ColumnsNames.ImportManagerQueue.JobID}],
							@ResourceGroupID = [{Constant.Sql.ColumnsNames.ImportManagerQueue.ResourceGroupID}],
							@ObjectType = [{Constant.Sql.ColumnsNames.ImportManagerQueue.ObjectType}],
							@JobType = [{Constant.Sql.ColumnsNames.ImportManagerQueue.JobType}],
							@QueueStatus = [{Constant.Sql.ColumnsNames.ImportManagerQueue.QueueStatus}],
							@WorkspaceName = [{Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceName}]
					FROM [EDDSDBO].{Constant.Tables.ImportManagerQueue} WITH(UPDLOCK,READPAST)
					WHERE
						[QueueStatus] in ({validStatuses})
						AND ResourceGroupID IN ({commaDelimitedResourceAgentIds})
						AND [{Constant.Sql.ColumnsNames.ImportManagerQueue.AgentID}] IS NULL
					ORDER BY
						Priority ASC
						,[TimeStampUTC] ASC

					UPDATE [EDDSDBO].{Constant.Tables.ImportManagerQueue}
					SET
						[{Constant.Sql.ColumnsNames.ImportManagerQueue.QueueStatus}] = @inProgressQueueStatus,
						[{Constant.Sql.ColumnsNames.ImportManagerQueue.AgentID}] = @agentId
					WHERE [ID] = @ID

				COMMIT
				SET NOCOUNT OFF

				SELECT
					@ID [{Constant.Sql.ColumnsNames.ImportManagerQueue.ID}],
					@AgentArtifactID [{Constant.Sql.ColumnsNames.ImportManagerQueue.AgentID}],
					@WorkspaceArtifactID [{Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceArtifactID}],
					@Priority [{Constant.Sql.ColumnsNames.ImportManagerQueue.Priority}],
					@JobID [{Constant.Sql.ColumnsNames.ImportManagerQueue.JobID}],
					@ResourceGroupID [{Constant.Sql.ColumnsNames.ImportManagerQueue.ResourceGroupID}],
					@ObjectType [{Constant.Sql.ColumnsNames.ImportManagerQueue.ObjectType}],
					@JobType [{Constant.Sql.ColumnsNames.ImportManagerQueue.JobType}],
					@QueueStatus [{Constant.Sql.ColumnsNames.ImportManagerQueue.QueueStatus}],
					@WorkspaceName [{Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceName}]
				WHERE @ID IS NOT NULL";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@agentID", SqlDbType.Int) {Value = agentId},
				new SqlParameter("@inProgressQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.IN_PROGRESS}
			};

			DataTable dt = await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
			return dt;
		}

		public async Task<DataTable> RetrieveAllImportManagerQueueRecords(IDBContext eddsDbContext)
		{
			try
			{
				String sql = $@"
				SELECT
					*
				FROM
					[EDDSDBO].[{Constant.Tables.ImportManagerQueue}] WITH(NOLOCK)";

				DataTable dt = await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql));
				return dt;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.QueryImportManagerQueueError, ex);
			}
		}

		public async Task<DataTable> RetrieveAllExportManagerQueueRecords(IDBContext eddsDbContext)
		{
			try
			{
				String sql = $@"
				SELECT
					*
				FROM
					[EDDSDBO].[{Constant.Tables.ExportManagerQueue}] WITH(NOLOCK)";

				DataTable dt = await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql));
				return dt;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException("Error while querying manager queue.", ex);
			}
		}

		public async Task ResetUnfishedImportManagerJobsAsync(IDBContext eddsDbContext, Int32 agentId)
		{
			String sql = $@"
				UPDATE
					[EDDSDBO].{Constant.Tables.ImportManagerQueue}
				SET
					[{Constant.Sql.ColumnsNames.ImportManagerQueue.QueueStatus}] = @notStartedQueueStatus,
					[{Constant.Sql.ColumnsNames.ImportManagerQueue.AgentID}] = NULL
				WHERE
					[{Constant.Sql.ColumnsNames.ImportManagerQueue.AgentID}] = @agentId AND
					[{Constant.Sql.ColumnsNames.ImportManagerQueue.QueueStatus}] = @inProgressStatus";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@agentId", SqlDbType.Int) {Value = agentId},
				new SqlParameter("@notStartedQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.NOT_STARTED},
				new SqlParameter("@inProgressStatus", SqlDbType.Int) {Value = Constant.Status.Queue.IN_PROGRESS}
			};

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
		}

		public async Task ResetUnfishedJobsAsync(IDBContext eddsDbContext, Int32 agentId, String queueTableName)
		{
			String sql = $@"
				UPDATE
					[EDDSDBO].{queueTableName}
				SET
					[{Constant.Sql.ColumnsNames.ImportManagerQueue.QueueStatus}] = @notStartedQueueStatus,
					[{Constant.Sql.ColumnsNames.ImportManagerQueue.AgentID}] = NULL
				WHERE
					[{Constant.Sql.ColumnsNames.ImportManagerQueue.AgentID}] = @agentId AND
					[{Constant.Sql.ColumnsNames.ImportManagerQueue.QueueStatus}] != @waitingForWorkersToFinish";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@agentId", SqlDbType.Int) {Value = agentId},
				new SqlParameter("@notStartedQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.NOT_STARTED},
				new SqlParameter("@waitingForWorkersToFinish", SqlDbType.Int) {Value = Constant.Status.Queue.WAITING_FOR_WORKERS_TO_FINISH}
			};

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
		}

		public async Task ResetUnfishedExportManagerJobsAsync(IDBContext eddsDbContext, Int32 agentId)
		{
			String sql = $@"

				DECLARE @CurrentQueueStatus INT

				BEGIN TRAN
					UPDATE
						[EDDSDBO].[{Constant.Tables.ExportManagerQueue}]
					SET
						[{Constant.Sql.ColumnsNames.ExportManagerQueue.AgentId}] = NULL,
						[{Constant.Sql.ColumnsNames.ExportManagerQueue.QueueStatus}] = @notStartedStatus
					WHERE
						[{Constant.Sql.ColumnsNames.ExportManagerQueue.AgentId}] = @agentId AND
						[{Constant.Sql.ColumnsNames.ExportManagerQueue.QueueStatus}] = @inProgressStatus
				COMMIT
			";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@agentId", SqlDbType.Int) {Value = agentId},
				new SqlParameter("@notStartedStatus", SqlDbType.Int) {Value = Constant.Status.Queue.NOT_STARTED},
				new SqlParameter("@inProgressStatus", SqlDbType.Int) {Value = Constant.Status.Queue.IN_PROGRESS},
			};

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
		}

		public async Task ResetUnfishedExportWorkerJobsAsync(IDBContext eddsDbContext, Int32 agentId)
		{
			try
			{
				String sql = $@"
					UPDATE
						[EDDSDBO].[{Constant.Tables.ExportWorkerQueue}]
					SET
						[{Constant.Sql.ColumnsNames.ExportWorkerQueue.AgentId}] = NULL,
						[{Constant.Sql.ColumnsNames.ExportWorkerQueue.QueueStatus}] = @notStartedQueueStatus
					WHERE
						[{Constant.Sql.ColumnsNames.ExportWorkerQueue.AgentId}] = @agentId
			";

				List<SqlParameter> sqlParams = new List<SqlParameter>
				{
					new SqlParameter("@agentId", SqlDbType.Int) {Value = agentId},
					new SqlParameter("@notStartedQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.NOT_STARTED}
				};

				await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.ResetUnfishedExportWorkerJobsError, ex);
			}
		}

		public async Task RemoveRecordFromTableByIdAsync(IDBContext eddsDbContext, String queueTableName, Int32 id)
		{
			String sql = $@"DELETE FROM [EDDSDBO].{queueTableName} WHERE ID = @id";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@id", SqlDbType.Int) {Value = id}
			};

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
		}

		public async Task InsertRowIntoExportManagerQueueAsync(IDBContext eddsDbContext, Int32 workspaceArtifactId, Int32 priority, Int32 exportJobArtifactId, Int32 resourceGroupArtifactId, string objectType)
		{
			String sql = $@"
			INSERT INTO [EDDSDBO].{Constant.Tables.ExportManagerQueue}
			(
				[{Constant.Sql.ColumnsNames.ExportManagerQueue.AgentId}]
				,[{Constant.Sql.ColumnsNames.ExportManagerQueue.ExportJobArtifactId}]
				,[{Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceArtifactId}]
				,[{Constant.Sql.ColumnsNames.ExportManagerQueue.ObjectType}]
				,[{Constant.Sql.ColumnsNames.ExportManagerQueue.QueueStatus}]
				,[{Constant.Sql.ColumnsNames.ExportManagerQueue.Priority}]
				,[{Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceResourceGroupArtifactId}]
				,[{Constant.Sql.ColumnsNames.ExportManagerQueue.TimeStampUtc}]
				,[{Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceName}]
			)
			VALUES
			(
				NULL
				,@exportJobArtifactId
				,@workspaceArtifactId
				,@objectType
				,@queueStatus
				,@priority
				,@resourceGroupID
				,GetUTCDate()
				,(SELECT [Name] FROM [EDDS].[eddsdbo].[ExtendedCase] WITH(NOLOCK) WHERE [ArtifactID] = @workspaceArtifactId)
			)";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@workspaceArtifactId", SqlDbType.Int) {Value = workspaceArtifactId},
				new SqlParameter("@queueStatus", SqlDbType.VarChar) {Value = Constant.Status.Queue.NOT_STARTED},
				new SqlParameter("@priority", SqlDbType.Int) {Value = priority},
				new SqlParameter("@exportJobArtifactId", SqlDbType.Int) {Value = exportJobArtifactId},
				new SqlParameter("@resourceGroupID", SqlDbType.Int) {Value = resourceGroupArtifactId},
				new SqlParameter("@objectType", SqlDbType.NVarChar) {Value = objectType}
			};

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
		}

		public async Task InsertRowIntoImportManagerQueueAsync(IDBContext eddsDbContext, Int32 workspaceArtifactId, Int32 priority, Int32 createdBy, Int32 artifactId, Int32 resourceGroupId, string objectType, string jobType)
		{
			String sql = $@"
			INSERT INTO [EDDSDBO].{Constant.Tables.ImportManagerQueue}
			(
				[{Constant.Sql.ColumnsNames.ImportManagerQueue.AgentID}]
				,[{Constant.Sql.ColumnsNames.ImportManagerQueue.JobID}]
				,[{Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceArtifactID}]
				,[{Constant.Sql.ColumnsNames.ImportManagerQueue.ObjectType}]
				,[{Constant.Sql.ColumnsNames.ImportManagerQueue.JobType}]
				,[{Constant.Sql.ColumnsNames.ImportManagerQueue.QueueStatus}]
				,[{Constant.Sql.ColumnsNames.ImportManagerQueue.Priority}]
				,[{Constant.Sql.ColumnsNames.ImportManagerQueue.ResourceGroupID}]
				,[{Constant.Sql.ColumnsNames.ImportManagerQueue.CreatedBy}]
				,[{Constant.Sql.ColumnsNames.ImportManagerQueue.TimeStampUTC}]
				,[{Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceName}]
			)
			VALUES
			(
				NULL
				,@artifactID
				,@workspaceArtifactId
				,@objectType
				,@jobType
				,@queueStatus
				,@priority
				,@resourceGroupID
				,@createdBy
				,GetUTCDate()
				,(SELECT [Name] FROM [EDDS].[eddsdbo].[ExtendedCase] WITH(NOLOCK) WHERE [ArtifactID] = @workspaceArtifactId)
			)";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@workspaceArtifactId", SqlDbType.Int) {Value = workspaceArtifactId},
				new SqlParameter("@queueStatus", SqlDbType.VarChar) {Value = Constant.Status.Queue.NOT_STARTED},
				new SqlParameter("@priority", SqlDbType.Int) {Value = priority},
				new SqlParameter("@createdBy", SqlDbType.Int) {Value = createdBy},
				new SqlParameter("@artifactID", SqlDbType.Int) {Value = artifactId},
				new SqlParameter("@resourceGroupID", SqlDbType.Int) {Value = resourceGroupId},
				new SqlParameter("@objectType", SqlDbType.NVarChar) {Value = objectType},
				new SqlParameter("@jobType", SqlDbType.NVarChar) {Value = jobType}
			};

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
		}

		public async Task InsertRowsIntoImportWorkerQueueAsync(IDBContext eddsDbContext, Int32 jobId, Int32 priority, Int32 workspaceArtifactId, Int32 resourceGroupId, string objectType, string jobType, string metaData, int importRowId)
		{
			String sql = $@"
				INSERT INTO [EDDSDBO].{Constant.Tables.ImportWorkerQueue}
				(
					[{Constant.Sql.ColumnsNames.ImportWorkerQueue.AgentID}]
					,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.JobID}]
					,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.WorkspaceArtifactID}]
					,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.ObjectType}]
					,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.JobType}]
					,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.MetaData}]
					,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.ImportRowID}]
					,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.QueueStatus}]
					,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.ResourceGroupID}]
					,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.TimeStampUTC}]
				)
				SELECT
					NULL
					,@jobID
					,@workspaceArtifactID
					,@objectType
					,@jobType
					,@metaData
					,@importRowId
					,@notStartedQueueStatus
					,@priority
					,@resourceGroupID
					,@timeStamp
				UNION ALL
				SELECT
					NULL
					,@jobID
					,@workspaceArtifactID
					,@objectType
					,@jobType
					,@metaData
					,@importRowId
					,@notStartedQueueStatus
					,@priority
					,@resourceGroupID
					,@timeStamp
				UNION ALL
				SELECT
					NULL
					,@jobID
					,@workspaceArtifactID
					,@objectType
					,@jobType
					,@metaData
					,@importRowId
					,@notStartedQueueStatus
					,@priority
					,@resourceGroupID
					,@timeStamp
				";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@jobID", SqlDbType.Int) {Value = jobId},
				new SqlParameter("@priority", SqlDbType.Int) {Value = priority},
				new SqlParameter("@workspaceArtifactID", SqlDbType.Int) {Value = workspaceArtifactId},
				new SqlParameter("@timeStamp", SqlDbType.DateTime) {Value = DateTime.UtcNow},
				new SqlParameter("@notStartedQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.NOT_STARTED},
				new SqlParameter("@resourceGroupID", SqlDbType.Int) {Value = resourceGroupId},
				new SqlParameter("@objectType", SqlDbType.NVarChar) {Value = objectType},
				new SqlParameter("@jobType", SqlDbType.NVarChar) {Value = jobType},
				new SqlParameter("@metaData", SqlDbType.NVarChar) {Value = metaData},
				new SqlParameter("@importRowId", SqlDbType.Int) {Value = importRowId}
			};

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
		}

		public async Task UpdateStatusInExportManagerQueueAsync(IDBContext eddsDbContext, Int32 queueRowId, Int32 queueStatus)
		{
			String sql = $@"
					UPDATE
						[EDDSDBO].[{Constant.Tables.ExportManagerQueue}]
					SET
						[{Constant.Sql.ColumnsNames.ExportManagerQueue.QueueStatus}] = @queueStatus
					WHERE
						[{Constant.Sql.ColumnsNames.ExportManagerQueue.ExportJobArtifactId}] = @queueRowId";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@queueRowId", SqlDbType.Int) {Value = queueRowId},
				new SqlParameter("@queueStatus", SqlDbType.Int) {Value = queueStatus}
			};

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
		}

		public async Task UpdateStatusInExportManagerQueueByWorkspaceJobIdAsync(IDBContext eddsDbContext, Int32 workspaceArtifactId, Int32 exportJobArtifactId, Int32 queueStatus)
		{
			String sql = $@"
					UPDATE
						[EDDSDBO].[{Constant.Tables.ExportManagerQueue}]
					SET
						[{Constant.Sql.ColumnsNames.ExportManagerQueue.QueueStatus}] = @queueStatus
					WHERE
						[{Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceArtifactId}] = @workspaceArtifactId
						AND [{Constant.Sql.ColumnsNames.ExportManagerQueue.ExportJobArtifactId}] = @exportJobArtifactId";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@workspaceArtifactId", SqlDbType.Int) {Value = workspaceArtifactId},
				new SqlParameter("@exportJobArtifactId", SqlDbType.Int) {Value = exportJobArtifactId},
				new SqlParameter("@queueStatus", SqlDbType.Int) {Value = queueStatus}
			};

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
		}

		public async Task<Int32?> GetQueueRecordCurrentStatus(IDBContext eddsDbContext, String workspaceIDColumnName, Int32 workspaceArtifactID, String JobIDColumnName, Int32 jobID, String tableName, String statusColumnName)
		{
			Int32? retVal = null;
			String sql = $@"
					SELECT TOP 1
						[{statusColumnName}]
					FROM
						[EDDSDBO].[{tableName}] WITH(NOLOCK)
					WHERE
						[{workspaceIDColumnName}] = @workspaceArtifactID AND
						[{JobIDColumnName}] = @jobID";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@workspaceArtifactID", SqlDbType.Int) {Value = workspaceArtifactID},
				new SqlParameter("@jobID", SqlDbType.Int) {Value = jobID}
			};

			retVal = await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsScalar<Int32?>(sql, sqlParams));

			return retVal;
		}

		public async Task UpdateStatusInExportWorkerQueueAsync(IDBContext eddsDbContext, Int32 statusId, String uniqueTableName)
		{
			String sql = $@"
					UPDATE S
					SET QueueStatus = @statusId
					FROM [EDDSDBO].{uniqueTableName} B
					INNER JOIN [EDDSDBO].{Constant.Tables.ExportWorkerQueue} S ON B.ID = S.ID";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@statusId", SqlDbType.Int) {Value = statusId}
			};

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
		}

		public async Task UpdateStatusInImportManagerQueueAsync(IDBContext eddsDbContext, Int32 statusId, Int32 id)
		{
			String sql = $@"
					UPDATE [EDDSDBO].{Constant.Tables.ImportManagerQueue}
					SET QueueStatus = @statusId
					WHERE ID = @id";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@statusId", SqlDbType.Int) {Value = statusId},
				new SqlParameter("@id", SqlDbType.Int) {Value = id}
			};

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
		}

		public async Task UpdateStatusInImportManagerQueueByWorkspaceJobIdAsync(IDBContext eddsDbContext, Int32 workspaceArtifactId, Int32 importJobArtifactId, Int32 queueStatus)
		{
			String sql = $@"
					UPDATE
						[EDDSDBO].[{Constant.Tables.ImportManagerQueue}]
					SET
						[{Constant.Sql.ColumnsNames.ImportManagerQueue.QueueStatus}] = @queueStatus
					WHERE
						[{Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceArtifactID}] = @workspaceArtifactId
						AND [{Constant.Sql.ColumnsNames.ImportManagerQueue.JobID}] = @importJobArtifactId";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@workspaceArtifactId", SqlDbType.Int) {Value = workspaceArtifactId},
				new SqlParameter("@importJobArtifactId", SqlDbType.Int) {Value = importJobArtifactId},
				new SqlParameter("@queueStatus", SqlDbType.Int) {Value = queueStatus}
			};

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
		}

		public async Task UpdateStatusInImportWorkerQueueAsync(IDBContext eddsDbContext, Int32 statusId, String uniqueTableName)
		{
			String sql = $@"
					UPDATE S
						SET QueueStatus = @statusId
					FROM [EDDSDBO].{uniqueTableName} B
						INNER JOIN [EDDSDBO].{Constant.Tables.ImportWorkerQueue} S ON B.ID = S.ID";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@statusId", SqlDbType.Int) {Value = statusId}
			};

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
		}

		public async Task<DataTable> RetrieveNextBatchInExportWorkerQueueAsync(IDBContext eddsDbContext, Int32 agentId, Int32 batchSize, String uniqueBatchTableName, String commaDelimitedResourceAgentIds)
		{
			try
			{
				String sql = $@"
				BEGIN TRAN
					IF NOT OBJECT_ID('[EDDSDBO].{uniqueBatchTableName}') IS NULL
					BEGIN
						DROP TABLE
							[EDDSDBO].{uniqueBatchTableName}
					END

					CREATE TABLE
							[EDDSDBO].{uniqueBatchTableName}([{Constant.Sql.ColumnsNames.ExportWorkerQueueBatch.TableRowId}] INT)

					DECLARE @exportjobArtifactId INT
					DECLARE @workspaceArtifactId INT

					SELECT
						TOP 1
						@exportjobArtifactId = [EWQ].[{Constant.Sql.ColumnsNames.ExportWorkerQueue.ExportJobArtifactId}],
						@workspaceArtifactId = [EWQ].[{Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceArtifactId}]
					FROM
						[EDDSDBO].[{Constant.Tables.ExportWorkerQueue}] AS [EWQ] WITH(NOLOCK)
					INNER JOIN
						[EDDSDBO].[{Constant.Tables.ExportManagerQueue}] AS [EMQ] WITH(NOLOCK)
					ON
						[EMQ].[{Constant.Sql.ColumnsNames.ExportManagerQueue.ExportJobArtifactId}] = [EWQ].[{Constant.Sql.ColumnsNames.ExportWorkerQueue.ExportJobArtifactId}] AND
						[EMQ].[{Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceArtifactId}] = [EWQ].[{Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceArtifactId}]
					WHERE
						[EMQ].[{Constant.Sql.ColumnsNames.ExportManagerQueue.QueueStatus}] = @waitingForWorkersToFinishStatus
						AND [EWQ].[{Constant.Sql.ColumnsNames.ExportWorkerQueue.QueueStatus}] = @notStartedQueueStatus
						AND [EWQ].[{Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceResourceGroupArtifactId}] IN ({commaDelimitedResourceAgentIds})
					ORDER BY
						[EWQ].[{Constant.Sql.ColumnsNames.ExportWorkerQueue.Priority}] ASC
						,[EWQ].[{Constant.Sql.ColumnsNames.ExportWorkerQueue.TimeStampUtc}] ASC

					UPDATE
						[EDDSDBO].[{Constant.Tables.ExportWorkerQueue}]
					SET
						[{Constant.Sql.ColumnsNames.ExportWorkerQueue.AgentId}] = @agentID,
						[{Constant.Sql.ColumnsNames.ExportWorkerQueue.QueueStatus}] = @inProgressQueueStatus
					OUTPUT
						inserted.ID
					INTO
						[EDDSDBO].{uniqueBatchTableName}([{Constant.Sql.ColumnsNames.ExportWorkerQueueBatch.TableRowId}])
					FROM
						[EDDSDBO].[{Constant.Tables.ExportWorkerQueue}] WITH(UPDLOCK,READPAST)
					WHERE
						ID IN
						(
							SELECT
								TOP (@batchSize) [{Constant.Sql.ColumnsNames.ExportWorkerQueue.TableRowId}]
							FROM
								[EDDSDBO].[{Constant.Tables.ExportWorkerQueue}] WITH(UPDLOCK,READPAST)
							WHERE
								[{Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceArtifactId}] = @workspaceArtifactId
								AND [{Constant.Sql.ColumnsNames.ExportWorkerQueue.ExportJobArtifactId}] = @exportjobArtifactId
								AND [{Constant.Sql.ColumnsNames.ExportWorkerQueue.QueueStatus}] = @notStartedQueueStatus
							ORDER BY
								[{Constant.Sql.ColumnsNames.ExportWorkerQueue.Priority}] ASC
								,[{Constant.Sql.ColumnsNames.ExportWorkerQueue.TimeStampUtc}] ASC
						)
				COMMIT

				SELECT
					E.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.TableRowId}]
					,E.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.AgentId}]
					,E.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceArtifactId}]
					,E.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.ExportJobArtifactId}]
					,E.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.ObjectType}]
					,E.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.Priority}]
					,E.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceResourceGroupArtifactId}]
					,E.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.QueueStatus}]
					,E.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.ArtifactId}]
					,E.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.ResultsTableName}]
					,E.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.MetaData}]
					,E.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.TimeStampUtc}]
				FROM
					[EDDSDBO].{uniqueBatchTableName} B
					INNER JOIN [EDDSDBO].[{Constant.Tables.ExportWorkerQueue}] E ON B.[{Constant.Sql.ColumnsNames.ExportWorkerQueueBatch.TableRowId}] = E.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.TableRowId}]	";

				List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@agentID", SqlDbType.Int) {Value = agentId},
				new SqlParameter("@batchSize", SqlDbType.Int) {Value = batchSize},
				new SqlParameter("@notStartedQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.NOT_STARTED},
				new SqlParameter("@inProgressQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.IN_PROGRESS},
				new SqlParameter("@waitingForWorkersToFinishStatus", SqlDbType.Int) {Value = Constant.Status.Queue.WAITING_FOR_WORKERS_TO_FINISH}
			};

				return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.RetrieveNextBatchInExportWorkerQueueError, ex);
			}
		}

		public async Task<DataTable> RetrieveNextBatchInImportWorkerQueueAsync(IDBContext eddsDbContext, Int32 agentId, Int32 batchSize, String commaDelimitedResourceAgentIds)
		{
			String validStatuses = String.Join(",", new[] { Constant.Status.Queue.NOT_STARTED });
			String sql = $@"
				SET NOCOUNT ON
				DECLARE @GroupWorkspaceArtifactID INT
				DECLARE @GroupJobID INT
				DECLARE @Priority INT
				DECLARE @TimeStamp DATETIME

				--Groupby is used so that the worker only works on a single job during a interation
				SELECT TOP 1
					@GroupWorkspaceArtifactID = [IWQ].[{Constant.Sql.ColumnsNames.ImportWorkerQueue.WorkspaceArtifactID}],
					@GroupJobID = [IWQ].[{Constant.Sql.ColumnsNames.ImportWorkerQueue.JobID}],
					@Priority = [IWQ].[{Constant.Sql.ColumnsNames.ImportWorkerQueue.Priority}],
					@TimeStamp = [IWQ].[{Constant.Sql.ColumnsNames.ImportWorkerQueue.TimeStampUTC}]
				FROM
					[EDDSDBO].{Constant.Tables.ImportWorkerQueue} AS [IWQ] WITH(NOLOCK)
				INNER JOIN
					[EDDSDBO].{Constant.Tables.ImportManagerQueue} AS [IMQ] WITH(NOLOCK)
				ON
					[IMQ].[{Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceArtifactID}] = [IWQ].[{Constant.Sql.ColumnsNames.ImportWorkerQueue.WorkspaceArtifactID}] AND
					[IMQ].[{Constant.Sql.ColumnsNames.ImportManagerQueue.JobID}] = [IWQ].[{Constant.Sql.ColumnsNames.ImportWorkerQueue.JobID}]
				WHERE
					[IMQ].[{Constant.Sql.ColumnsNames.ImportManagerQueue.QueueStatus}] = @waitingForWorkersToFinishStatus
				GROUP BY
					[IWQ].[{Constant.Sql.ColumnsNames.ImportWorkerQueue.WorkspaceArtifactID}],
					[IWQ].[{Constant.Sql.ColumnsNames.ImportWorkerQueue.JobID}],
					[IWQ].[{Constant.Sql.ColumnsNames.ImportWorkerQueue.Priority}],
					[IWQ].[{Constant.Sql.ColumnsNames.ImportWorkerQueue.TimeStampUTC}]
				ORDER BY
					[IWQ].[{Constant.Sql.ColumnsNames.ImportWorkerQueue.Priority}] ASC,
					[IWQ].[{Constant.Sql.ColumnsNames.ImportWorkerQueue.TimeStampUTC}] ASC

				BEGIN TRANSACTION
					UPDATE
						T
					SET
						[{Constant.Sql.ColumnsNames.ImportWorkerQueue.QueueStatus}] = @inProgressQueueStatus,
						[{Constant.Sql.ColumnsNames.ImportWorkerQueue.AgentID}] = @agentId
					FROM
						(
							SELECT TOP {batchSize}
								[{Constant.Sql.ColumnsNames.ImportWorkerQueue.ID}],
								[{Constant.Sql.ColumnsNames.ImportWorkerQueue.AgentID}],
								[{Constant.Sql.ColumnsNames.ImportWorkerQueue.JobID}],
								[{Constant.Sql.ColumnsNames.ImportWorkerQueue.WorkspaceArtifactID}],
								[{Constant.Sql.ColumnsNames.ImportWorkerQueue.ObjectType}],
								[{Constant.Sql.ColumnsNames.ImportWorkerQueue.JobType}],
								[{Constant.Sql.ColumnsNames.ImportWorkerQueue.MetaData}],
								[{Constant.Sql.ColumnsNames.ImportWorkerQueue.ImportRowID}],
								[{Constant.Sql.ColumnsNames.ImportWorkerQueue.QueueStatus}],
								[{Constant.Sql.ColumnsNames.ImportWorkerQueue.ResourceGroupID}],
								[{Constant.Sql.ColumnsNames.ImportWorkerQueue.Priority}],
								[{Constant.Sql.ColumnsNames.ImportWorkerQueue.TimeStampUTC}]
							FROM
								[EDDSDBO].{Constant.Tables.ImportWorkerQueue} WITH(UPDLOCK,READPAST)
							WHERE
								[{Constant.Sql.ColumnsNames.ImportWorkerQueue.QueueStatus}] IN ({validStatuses}) AND
								[{Constant.Sql.ColumnsNames.ImportWorkerQueue.ResourceGroupID}] IN ({commaDelimitedResourceAgentIds}) AND
								[{Constant.Sql.ColumnsNames.ImportWorkerQueue.AgentID}] IS NULL AND
								[{Constant.Sql.ColumnsNames.ImportWorkerQueue.WorkspaceArtifactID}] = @GroupWorkspaceArtifactID AND
								[{Constant.Sql.ColumnsNames.ImportWorkerQueue.JobID}] = @GroupJobID
							ORDER BY
								[{Constant.Sql.ColumnsNames.ImportWorkerQueue.Priority}] ASC
								,[{Constant.Sql.ColumnsNames.ImportWorkerQueue.TimeStampUTC}] ASC
						) T
				COMMIT TRANSACTION

				SELECT
					[{Constant.Sql.ColumnsNames.ImportWorkerQueue.ID}],
					[{Constant.Sql.ColumnsNames.ImportWorkerQueue.AgentID}],
					[{Constant.Sql.ColumnsNames.ImportWorkerQueue.JobID}],
					[{Constant.Sql.ColumnsNames.ImportWorkerQueue.WorkspaceArtifactID}],
					[{Constant.Sql.ColumnsNames.ImportWorkerQueue.ObjectType}],
					[{Constant.Sql.ColumnsNames.ImportWorkerQueue.JobType}],
					[{Constant.Sql.ColumnsNames.ImportWorkerQueue.MetaData}],
					[{Constant.Sql.ColumnsNames.ImportWorkerQueue.ImportRowID}],
					[{Constant.Sql.ColumnsNames.ImportWorkerQueue.QueueStatus}],
					[{Constant.Sql.ColumnsNames.ImportWorkerQueue.ResourceGroupID}],
					[{Constant.Sql.ColumnsNames.ImportWorkerQueue.Priority}],
					[{Constant.Sql.ColumnsNames.ImportWorkerQueue.TimeStampUTC}]
				FROM
					[EDDSDBO].{Constant.Tables.ImportWorkerQueue}
				WHERE
					[{Constant.Sql.ColumnsNames.ImportWorkerQueue.AgentID}] = @agentID
				";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@agentID", SqlDbType.Int) {Value = agentId},
				new SqlParameter("@notStartedQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.NOT_STARTED},
				new SqlParameter("@inProgressQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.IN_PROGRESS},
				new SqlParameter("@waitingForWorkersToFinishStatus ", SqlDbType.Int) {Value = Constant.Status.Queue.WAITING_FOR_WORKERS_TO_FINISH}
			};

			return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
		}

		public async Task RemoveBatchFromExportWorkerQueueTableAsync(IDBContext eddsDbContext, String batchTableName)
		{
			String sql = $@"
				DELETE [EDDSDBO].{Constant.Tables.ExportWorkerQueue}
				FROM [EDDSDBO].{Constant.Tables.ExportWorkerQueue} S
					INNER JOIN [EDDSDBO].{batchTableName} B ON B.ID = S.ID	";

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
		}

		public async Task RemoveBatchFromImportWorkerQueueAsync(IDBContext eddsDbContext, String uniqueTableName)
		{
			String sql = $@"
				DELETE [EDDSDBO].{Constant.Tables.ImportWorkerQueue}
				FROM [EDDSDBO].{Constant.Tables.ImportWorkerQueue} S
					INNER JOIN [EDDSDBO].{uniqueTableName} B ON B.ID = S.ID	";

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
		}

		public async Task DropTableAsync(IDBContext eddsDbContext, String tableName)
		{
			String sql = $@"
				IF NOT OBJECT_ID('[EDDSDBO].{tableName}') IS NULL
					BEGIN DROP TABLE [EDDSDBO].{tableName}
				END";

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
		}

		public async Task<DataTable> RetrieveAllInExportManagerQueueAsync(IDBContext eddsDbContext)
		{
			String sql = $@"
				DECLARE @offset INT SET @offset = (SELECT DATEDIFF(HOUR,GetUTCDate(),GetDate()))

				SELECT
					Q.[{Constant.Sql.ColumnsNames.ExportManagerQueue.TableRowId}]
					,DATEADD(HOUR,@offset,Q.[{Constant.Sql.ColumnsNames.ExportManagerQueue.TimeStampUtc}]) [Added On]
					,Q.[{Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceArtifactId}] [Workspace Artifact ID]
					,C.Name [Workspace Name]
					,CASE Q.[{Constant.Sql.ColumnsNames.ExportManagerQueue.QueueStatus}]
						WHEN @notStartedStatusId THEN 'Waiting'
						WHEN @inProgressStatusId THEN 'In Progress'
						WHEN @errorStatusId THEN 'Error'
						END [Status]
					,Q.[{Constant.Sql.ColumnsNames.ExportManagerQueue.AgentId}] [Agent Artifact ID]
					,Q.[{Constant.Sql.ColumnsNames.ExportManagerQueue.Priority}]
				FROM [EDDSDBO].{Constant.Tables.ExportManagerQueue} Q
					INNER JOIN EDDS.[EDDSDBO].ExtendedCase C ON Q.WorkspaceArtifactID = C.ArtifactID
				ORDER BY
					Q.[{Constant.Sql.ColumnsNames.ExportManagerQueue.Priority}] ASC
					,Q.[{Constant.Sql.ColumnsNames.ExportManagerQueue.TimeStampUtc}] ASC";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@notStartedStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.NOT_STARTED},
				new SqlParameter("@inProgressStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.IN_PROGRESS},
				new SqlParameter("@errorStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.ERROR}
			};

			return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
		}

		public async Task<DataTable> RetrieveAllInImportManagerQueueAsync(IDBContext eddsDbContext)
		{
			String sql = $@"
				DECLARE @offset INT SET @offset = (SELECT DATEDIFF(HOUR,GetUTCDate(),GetDate()))

				SELECT
					Q.[{Constant.Sql.ColumnsNames.ImportManagerQueue.ID}]
					,DATEADD(HOUR,@offset,Q.[{Constant.Sql.ColumnsNames.ImportManagerQueue.TimeStampUTC}]) [Added On]
					,Q.[{Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceArtifactID}] [Workspace Artifact ID]
					,C.Name [Workspace Name]
					,CASE Q.[{Constant.Sql.ColumnsNames.ImportManagerQueue.QueueStatus}]
						WHEN @notStartedStatusId THEN 'Waiting'
						WHEN @inProgressStatusId THEN 'In Progress'
						WHEN @errorStatusId THEN 'Error'
						END [Status]
					,Q.[{Constant.Sql.ColumnsNames.ImportManagerQueue.AgentID}] [Agent Artifact ID]
					,Q.[{Constant.Sql.ColumnsNames.ImportManagerQueue.Priority}]
				FROM [EDDSDBO].{Constant.Tables.ImportManagerQueue} Q
					INNER JOIN EDDS.[EDDSDBO].ExtendedCase C ON Q.WorkspaceArtifactID = C.ArtifactID
				ORDER BY
					Q.[{Constant.Sql.ColumnsNames.ImportManagerQueue.Priority}] ASC
					,Q.[{Constant.Sql.ColumnsNames.ImportManagerQueue.TimeStampUTC}] ASC";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@notStartedStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.NOT_STARTED},
				new SqlParameter("@inProgressStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.IN_PROGRESS},
				new SqlParameter("@errorStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.ERROR}
			};

			return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
		}

		public async Task<DataRow> RetrieveSingleInExportManagerQueueByArtifactIdAsync(IDBContext eddsDbContext, Int32 exportJobArtifactId, Int32 workspaceArtifactId)
		{
			String sql = $@"

				DECLARE @offset INT SET @offset = (SELECT DATEDIFF(HOUR,GetUTCDate(),GetDate()))

				SELECT
					Q.[{Constant.Sql.ColumnsNames.ExportManagerQueue.TableRowId}]
					,DATEADD(HOUR,@offset,Q.[{Constant.Sql.ColumnsNames.ExportManagerQueue.TimeStampUtc}]) [Added On]
					,Q.[{Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceArtifactId}] [Workspace Artifact ID]
					,C.Name [Workspace Name]
					,CASE Q.[{Constant.Sql.ColumnsNames.ExportManagerQueue.QueueStatus}]
						WHEN @notStartedStatusId THEN 'Waiting'
						WHEN @inProgressStatusId THEN 'In Progress'
						WHEN @errorStatusId THEN 'Error'
						END [Status]
					,Q.[{Constant.Sql.ColumnsNames.ExportManagerQueue.AgentId}] [Agent Artifact ID]
					,Q.[{Constant.Sql.ColumnsNames.ExportManagerQueue.Priority}]
					,Q.[{Constant.Sql.ColumnsNames.ExportManagerQueue.ExportJobArtifactId}] [Record Artifact ID]
				FROM [EDDSDBO].{Constant.Tables.ExportManagerQueue} Q
					INNER JOIN EDDS.[EDDSDBO].[ExtendedCase] C ON Q.[{Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceArtifactId}] = C.ArtifactID
				WHERE
					Q.[{Constant.Sql.ColumnsNames.ExportManagerQueue.ExportJobArtifactId}] = @exportJobArtifactId
					AND Q.[{Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceArtifactId}] = @workspaceArtifactId";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@notStartedStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.NOT_STARTED},
				new SqlParameter("@inProgressStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.IN_PROGRESS},
				new SqlParameter("@errorStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.ERROR},
				new SqlParameter("@exportJobArtifactId", SqlDbType.Int) {Value = exportJobArtifactId},
				new SqlParameter("@workspaceArtifactId", SqlDbType.Int) {Value = workspaceArtifactId}
			};

			DataTable dataTable = await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
			DataRow dataRow = dataTable.Rows.Count > 0 ? dataTable.Rows[0] : null;
			return dataRow;
		}

		public async Task<DataRow> RetrieveSingleInImportManagerQueueByArtifactIdAsync(IDBContext eddsDbContext, Int32 importJobArtifactId, Int32 workspaceArtifactId)
		{
			String sql =
				$@"

				DECLARE @offset INT SET @offset = (SELECT DATEDIFF(HOUR,GetUTCDate(),GetDate()))

				SELECT
					Q.[ID]
					,DATEADD(HOUR,@offset,Q.[TimeStampUTC]) [Added On]
					,Q.WorkspaceArtifactID [Workspace Artifact ID]
					,C.Name [Workspace Name]
					,CASE Q.[QueueStatus]
						WHEN @notStartedStatusId THEN 'Waiting'
						WHEN @inProgressStatusId THEN 'In Progress'
						WHEN @errorStatusId THEN 'Error'
						END [Status]
					,Q.AgentID [Agent Artifact ID]
					,Q.[Priority]
					,U.LastName + ', ' + U.FirstName [Added By]
					,Q.JobID [Record Artifact ID]
				FROM [EDDSDBO].{Constant.Tables.ImportManagerQueue} Q
					INNER JOIN EDDS.[EDDSDBO].ExtendedCase C ON Q.WorkspaceArtifactID = C.ArtifactID
					LEFT JOIN EDDS.[EDDSDBO].[User] U ON Q.CreatedBy = U.ArtifactID
				WHERE Q.JobID = @artifactId
					AND Q.WorkspaceArtifactID = @workspaceArtifactId";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@notStartedStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.NOT_STARTED},
				new SqlParameter("@inProgressStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.IN_PROGRESS},
				new SqlParameter("@errorStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.ERROR},
				new SqlParameter("@artifactId", SqlDbType.Int) {Value = importJobArtifactId},
				new SqlParameter("@workspaceArtifactId", SqlDbType.Int) {Value = workspaceArtifactId}
			};

			DataTable dt = await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
			if (dt.Rows.Count > 0)
			{
				return dt.Rows[0];
			}
			return null;
		}

		public async Task<DataTable> RetrieveAllInExportWorkerQueueAsync(IDBContext eddsDbContext)
		{
			String sql = $@"
				DECLARE @offset INT SET @offset = (SELECT DATEDIFF(HOUR,GetUTCDate(),GetDate()))

				SELECT
					Q.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.ExportJobArtifactId}] [ID]
					,DATEADD(HOUR,@offset,Q.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.TimeStampUtc}]) [Added On]
					,Q.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceArtifactId}] [Workspace Artifact ID]
					,C.Name [Workspace Name]
					,CASE Q.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.QueueStatus}]
						WHEN @notStartedStatusId THEN 'Waiting'
						WHEN @inProgressStatusId THEN 'In Progress'
						WHEN @errorStatusId THEN 'Error'
						END [Status]
					,Q.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.AgentId}] [Agent Artifact ID]
					,Q.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.Priority}]
					,COUNT(Q.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.TableRowId}]) [# Records Remaining]
				FROM [EDDSDBO].{Constant.Tables.ExportWorkerQueue} Q
					INNER JOIN EDDS.[EDDSDBO].ExtendedCase C ON Q.WorkspaceArtifactID = C.ArtifactID
				GROUP BY
					Q.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.ExportJobArtifactId}]
					,Q.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.TimeStampUtc}]
					,C.Name
					,Q.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceArtifactId}]
					,Q.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.QueueStatus}]
					,Q.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.AgentId}]
					,Q.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.Priority}]
				ORDER BY
					Q.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.Priority}] ASC
					,Q.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.TimeStampUtc}] ASC
					,Q.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.ExportJobArtifactId}] ASC
					,Q.[{Constant.Sql.ColumnsNames.ExportWorkerQueue.QueueStatus}] DESC";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@notStartedStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.NOT_STARTED},
				new SqlParameter("@inProgressStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.IN_PROGRESS},
				new SqlParameter("@errorStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.ERROR}
			};

			return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
		}

		public async Task<DataTable> RetrieveAllInImportWorkerQueueAsync(IDBContext eddsDbContext)
		{
			String sql = $@"
				DECLARE @offset INT SET @offset = (SELECT DATEDIFF(HOUR,GetUTCDate(),GetDate()))

				SELECT
					Q.[{Constant.Sql.ColumnsNames.ImportWorkerQueue.ID}] [ID]
					,DATEADD(HOUR,@offset,Q.[{Constant.Sql.ColumnsNames.ImportWorkerQueue.TimeStampUTC}]) [Added On]
					,Q.[{Constant.Sql.ColumnsNames.ImportWorkerQueue.WorkspaceArtifactID}] [Workspace Artifact ID]
					,C.Name [Workspace Name]
					,CASE Q.[{Constant.Sql.ColumnsNames.ImportWorkerQueue.QueueStatus}]
						WHEN @notStartedStatusId THEN 'Waiting'
						WHEN @inProgressStatusId THEN 'In Progress'
						WHEN @errorStatusId THEN 'Error'
						END [Status]
					,Q.[{Constant.Sql.ColumnsNames.ImportWorkerQueue.AgentID}] [Agent Artifact ID]
					,Q.[{Constant.Sql.ColumnsNames.ImportWorkerQueue.Priority}]
					,COUNT(Q.[{Constant.Sql.ColumnsNames.ImportWorkerQueue.ID}]) [# Records Remaining]
				FROM [EDDSDBO].{Constant.Tables.ImportWorkerQueue} Q
					INNER JOIN EDDS.[EDDSDBO].ExtendedCase C ON Q.WorkspaceArtifactID = C.ArtifactID
				GROUP BY
					Q.[{Constant.Sql.ColumnsNames.ImportWorkerQueue.JobID}]
					,Q.[{Constant.Sql.ColumnsNames.ImportWorkerQueue.TimeStampUTC}]
					,C.Name
					,Q.[{Constant.Sql.ColumnsNames.ImportWorkerQueue.WorkspaceArtifactID}]
					,Q.[{Constant.Sql.ColumnsNames.ImportWorkerQueue.QueueStatus}]
					,Q.[{Constant.Sql.ColumnsNames.ImportWorkerQueue.AgentID}]
					,Q.[{Constant.Sql.ColumnsNames.ImportWorkerQueue.Priority}]
				ORDER BY
					Q.[{Constant.Sql.ColumnsNames.ImportWorkerQueue.Priority}] ASC
					,Q.[{Constant.Sql.ColumnsNames.ImportWorkerQueue.TimeStampUTC}] ASC
					,Q.[{Constant.Sql.ColumnsNames.ImportWorkerQueue.JobID}] ASC
					,Q.[{Constant.Sql.ColumnsNames.ImportWorkerQueue.QueueStatus}] DESC";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@notStartedStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.NOT_STARTED},
				new SqlParameter("@inProgressStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.IN_PROGRESS},
				new SqlParameter("@errorStatusId", SqlDbType.Int) {Value = Constant.Status.Queue.ERROR}
			};

			return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
		}

		public async Task<DataTable> RetrieveOffHoursAsync(IDBContext eddsDbContext)
		{
			String sql = @"
				DECLARE @OffHourStart VARCHAR(100)
				DECLARE @OffHourEndTime VARCHAR(100)

				SET @OffHourStart = (SELECT [VALUE] FROM [EDDS].[EDDSDBO].[Configuration] WITH(NOLOCK) WHERE [SECTION] = 'kCura.EDDS.Agents' AND [NAME] = 'AgentOffHourStartTime')
				SET @OffHourEndTime = (SELECT [VALUE] FROM [EDDS].[EDDSDBO].[Configuration] WITH(NOLOCK) WHERE [SECTION] = 'kCura.EDDS.Agents' AND [NAME] = 'AgentOffHourEndTime')

				SELECT
					@OffHourStart AS [AgentOffHourStartTime],
					@OffHourEndTime AS [AgentOffHourEndTime]
				";

			DataTable dt = await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql));
			return dt.Rows.Count > 0 ? dt : null;
		}

		public async Task<DataTable> QueryChoiceArtifactId(IDBContext eddsDbContext, String codeTypeName, String codeName)
		{
			DataTable retVal = null;
			String sql = @"
				SELECT
					[Code].ArtifactID
				FROM
					[EDDSDBO].[Code] WITH(NOLOCK)
				INNER JOIN
					[EDDSDBO].[CodeType] WITH(NOLOCK)
				ON
					[Code].CodeTypeID = [CodeType].CodeTypeID
				WHERE
					[CodeType].Name = @CodeTypeName AND
					[Code].Name = @CodeName";

			List<SqlParameter> parameters = new List<SqlParameter>()
			{
				new SqlParameter("@CodeTypeName", SqlDbType.NVarChar) {Value = codeTypeName},
				new SqlParameter("@codeName", SqlDbType.NVarChar) {Value = codeName}
			};
			await Task.Run(() => retVal = eddsDbContext.ExecuteSqlStatementAsDataTable(sql, parameters));
			return retVal;
		}

		public async Task<KeywordsNotesModel> RetrieveKeywordsAndNotesForUserAsync(IDBContext eddsDbContext, Int32 userArtifactId)
		{
			String errorContext = $"[{nameof(userArtifactId)} = {userArtifactId}]";

			try
			{
				if (userArtifactId < 0)
				{
					throw new ArgumentException($"{nameof(userArtifactId)} cannot be negative.");
				}

				const String sql = @"
				SELECT
					[Keywords],
					[Notes]
				FROM
					[EDDSDBO].[ExtendedUser] WITH(NOLOCK)
				WHERE
					[ArtifactID] = @userArtifactId
				";

				List<SqlParameter> sqlParams = new List<SqlParameter>
				{
					new SqlParameter("@userArtifactId", SqlDbType.Int) {Value = userArtifactId}
				};

				DataTable dataTable = await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));

				if (dataTable?.Rows == null || dataTable.Rows.Count < 0)
				{
					throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.UserKeywordsNotesError} {errorContext}");
				}

				if (dataTable.Rows.Count == 0)
				{
					throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.UserKeywordsNotesError_UserNotFound} {errorContext}");
				}

				if (dataTable.Rows.Count > 1)
				{
					throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.UserKeywordsNotesError_MultipleUsers} {errorContext}");
				}

				String keywords = dataTable.Rows[0]["Keywords"].ToString();
				String notes = dataTable.Rows[0]["Notes"].ToString();

				KeywordsNotesModel keywordsNotesModel = new KeywordsNotesModel
				{
					Keywords = keywords,
					Notes = notes
				};

				return keywordsNotesModel;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.UserKeywordsNotesError} {errorContext}", ex);
			}
		}

		public async Task InsertRowIntoExportWorkerQueueTableAsync(IDBContext eddsDbContext, Int32 workspaceArtifactId, Int32 exportJobArtifactId, String objectType, Int32? priority, Int32 workspaceResourceGroupArtifactId, Int32 queueStatus, Int32 artifactId, String exportWorkerResultTableName, String metadata, DateTime timeStampUtc)
		{
			String errorContext = $"[{nameof(workspaceArtifactId)} = {workspaceArtifactId}, {nameof(artifactId)} = {artifactId}]";

			try
			{
				String sql = $@"
				INSERT INTO [EDDSDBO].[{Constant.Tables.ExportWorkerQueue}]
				(
					[{Constant.Sql.ColumnsNames.ExportWorkerQueue.AgentId}],
					[{Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceArtifactId}],
					[{Constant.Sql.ColumnsNames.ExportWorkerQueue.ExportJobArtifactId}],
					[{Constant.Sql.ColumnsNames.ExportWorkerQueue.ObjectType}],
					[{Constant.Sql.ColumnsNames.ExportWorkerQueue.Priority}],
					[{Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceResourceGroupArtifactId}],
					[{Constant.Sql.ColumnsNames.ExportWorkerQueue.QueueStatus}],
					[{Constant.Sql.ColumnsNames.ExportWorkerQueue.ArtifactId}],
					[{Constant.Sql.ColumnsNames.ExportWorkerQueue.ResultsTableName}],
					[{Constant.Sql.ColumnsNames.ExportWorkerQueue.MetaData}],
					[{Constant.Sql.ColumnsNames.ExportWorkerQueue.TimeStampUtc}]
				)
				VALUES
				(
					NULL,
					@workspaceArtifactId,
					@exportJobArtifactId,
					@objectType,
					@priority,
					@workspaceResourceGroupArtifactId,
					@queueStatus,
					@artifactId,
					@exportWorkerResultTableName,
					@metadata,
					@timeStampUtc
				)
				";

				List<SqlParameter> sqlParams = new List<SqlParameter>
				{
					new SqlParameter("@workspaceArtifactId", SqlDbType.Int) { Value = workspaceArtifactId},
					new SqlParameter("@exportJobArtifactId", SqlDbType.Int) { Value = exportJobArtifactId},
					new SqlParameter("@objectType", SqlDbType.NVarChar) { Value = objectType},
					new SqlParameter("@priority", SqlDbType.Int) { Value = priority},
					new SqlParameter("@workspaceResourceGroupArtifactId", SqlDbType.Int) { Value = workspaceResourceGroupArtifactId},
					new SqlParameter("@queueStatus", SqlDbType.Int) { Value = queueStatus},
					new SqlParameter("@artifactId", SqlDbType.Int) { Value = artifactId},
					new SqlParameter("@exportWorkerResultTableName", SqlDbType.NVarChar) { Value = exportWorkerResultTableName},
					new SqlParameter("@metadata", SqlDbType.NVarChar) { Value = metadata},
					new SqlParameter("@timeStampUtc", SqlDbType.DateTime) { Value = timeStampUtc}
				};

				await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.InsertingAUserIntoExportWorkerQueueTableError} {errorContext}", ex);
			}
		}

		public async Task UpdateKeywordAndNotes(IDBContext eddsDbContext, Int32 objectArtifactId, String keywords, String notes)
		{
			await Task.Run(() =>
			{
				try
				{
					String sql = @"
					  UPDATE
						[EDDS].[EDDSDBO].[Artifact]
					  SET
						[Keywords] = @keywordsValue,
						[Notes] = @notesValue
					  WHERE
						[ArtifactID] = @artifactID
					";

					List<SqlParameter> sqlParams = new List<SqlParameter>
					{
						new SqlParameter("@keywordsValue", SqlDbType.VarChar) { Value = keywords},
						new SqlParameter("@notesValue", SqlDbType.VarChar) { Value = notes},
						new SqlParameter("@artifactID", SqlDbType.Int) { Value = objectArtifactId}
					};

					Int32 affectedRows = eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams);

					if (affectedRows < 1)
					{
						throw new Exception();
					}
				}
				catch (Exception ex)
				{
					throw new Exception(String.Format(Constant.ErrorMessages.KeywordsAndNotesUpdateError, objectArtifactId), ex);
				}
			});
		}

		public async Task<DataRow> RetrieveWorkspacesWhereAppIsInstalledAsync(IDBContext eddsDbContext)
		{
			var sql = @"
				SELECT
					TOP 1 [CA].[CaseArtifactID] [CaseArtifactId]
				FROM
					[EDDSDBO].[ExtendedCaseApplication] [CA] WITH(NOLOCK)
					INNER JOIN [EDDSDBO].[ArtifactGuid] [AG] WITH (NOLOCK) ON [CA].[ApplicationArtifactID] = [AG].[ArtifactID]
				WHERE
					[AG].[ArtifactGuid] = @applicationGuid AND
					[CA].[CaseArtifactID] != -1 AND
					[CA].[InstalledOn] IS NOT NULL

				ORDER BY
					[CA].[ApplicationArtifactID] ASC";

			var sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@applicationGuid", SqlDbType.UniqueIdentifier) {Value = Constant.Guids.Application.ApplicationGuid},
			};

			var dataTable = await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
			return dataTable.Rows.Count > 0 ? dataTable.Rows[0] : null;
		}

		/// <summary>
		/// Bulk insert data into Sql table
		/// This method assumes that the column names in the provided datatable match the column names in the destination sql table
		/// </summary>
		/// <param name="dbContext">Database that contains the destination table</param>
		/// <param name="sourceDataTable">Datatable used as source for bulk insert</param>
		/// <param name="destinationTableName">Name of the destination table</param>
		/// <param name="batchSize">batchsize to user during insert</param>
		/// <returns></returns>
		public async Task BulkInsertIntoTableAsync(IDBContext dbContext, DataTable sourceDataTable, String destinationTableName, Int32 batchSize)
		{
			List<SqlBulkCopyColumnMapping> columnMappings = sourceDataTable.Columns.Cast<DataColumn>().Select(x => new SqlBulkCopyColumnMapping(x.ColumnName, x.ColumnName)).ToList();
			await BulkInsertIntoTableAsync(dbContext, sourceDataTable, columnMappings, destinationTableName, batchSize);
		}

		/// <summary>
		/// This method assumes that the column names in the provided datatable match the column names in the destination sql table
		/// </summary>
		/// <param name="dbContext">Database that contains the destination table</param>
		/// <param name="sourceDataTable">Datatable used as source for bulk insert</param>
		/// <param name="columnMappings">Lookup that correlates the datatable column names to the name of the columns in the destination sql table</param>
		/// <param name="destinationTableName">Name of the destination table</param>
		/// <param name="batchSize">batchsize to user during insert</param>
		/// <returns></returns>
		public async Task BulkInsertIntoTableAsync(IDBContext dbContext, DataTable sourceDataTable, List<SqlBulkCopyColumnMapping> columnMappings, String destinationTableName, Int32 batchSize)
		{
			await Task.Run(() =>
			{
				var connection = dbContext.GetConnection();
				using (SqlTransaction transaction = connection.BeginTransaction())
				{
					using (var bulkCopy = new SqlBulkCopy(dbContext.GetConnection(), SqlBulkCopyOptions.Default, transaction))
					{
						try
						{
							bulkCopy.BatchSize = batchSize;
							bulkCopy.DestinationTableName = destinationTableName.Contains("eddsdbo") ? destinationTableName : "eddsdbo." + destinationTableName;
							foreach (var columnMapping in columnMappings)
							{
								bulkCopy.ColumnMappings.Add(columnMapping);
							}
							bulkCopy.WriteToServer(sourceDataTable);
							transaction.Commit();
						}
						catch (Exception)
						{
							transaction.Rollback();
							throw;
						}
					}
				}
			});
		}

		public async Task InsertRowIntoExportWorkerResultsTableAsync(IDBContext eddsDbContext, String exportWorkerResultTableName, Int32 workspaceArtifactId, Int32 exportJobArtifactId, String objectType, Int32 workspaceResourceGroupArtifactId, Int32 queueStatus, String metadata, DateTime timeStampUtc)
		{
			String errorContext = $"[{nameof(workspaceArtifactId)} = {workspaceArtifactId}, {nameof(exportJobArtifactId)} = {exportJobArtifactId}]";

			try
			{
				String sql = $@"

				IF NOT OBJECT_ID('[EDDSDBO].[{exportWorkerResultTableName}]') IS NULL
					BEGIN
						BEGIN TRAN
							INSERT INTO [EDDSDBO].[{exportWorkerResultTableName}]
							(
								[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.AgentId}],
								[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.WorkspaceArtifactId}],
								[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.ExportJobArtifactId}],
								[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.ObjectType}],
								[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.WorkspaceResourceGroupArtifactId}],
								[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.QueueStatus}],
								[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.MetaData}],
								[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.TimeStampUtc}]
							)
							VALUES
							(
								NULL,
								@workspaceArtifactId,
								@exportJobArtifactId,
								@objectType,
								@workspaceResourceGroupArtifactId,
								@queueStatus,
								@metadata,
								@timeStampUtc
							)
						COMMIT
					END
				";

				List<SqlParameter> sqlParams = new List<SqlParameter>
				{
					new SqlParameter("@workspaceArtifactId", SqlDbType.Int) { Value = workspaceArtifactId},
					new SqlParameter("@exportJobArtifactId", SqlDbType.Int) { Value = exportJobArtifactId},
					new SqlParameter("@objectType", SqlDbType.NVarChar) { Value = objectType},
					new SqlParameter("@workspaceResourceGroupArtifactId", SqlDbType.Int) { Value = workspaceResourceGroupArtifactId},
					new SqlParameter("@queueStatus", SqlDbType.Int) { Value = queueStatus},
					new SqlParameter("@metadata", SqlDbType.NVarChar) { Value = metadata},
					new SqlParameter("@timeStampUtc", SqlDbType.DateTime) { Value = timeStampUtc}
				};

				await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException($"{Constant.ErrorMessages.InsertIntoExportWorkerResultsTableError} {errorContext}", ex);
			}
		}

		public async Task UpdateResultsTableNameInExportManagerQueueAsync(IDBContext eddsDbContext, Int32 queueRowId, String exportResultsTableName)
		{
			String sql = $@"
					UPDATE
						[EDDSDBO].[{Constant.Tables.ExportManagerQueue}]
					SET
						[{Constant.Sql.ColumnsNames.ExportManagerQueue.ResultsTableName}] = @exportResultsTableName
					WHERE
						[{Constant.Sql.ColumnsNames.ExportManagerQueue.TableRowId}] = @queueRowId";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@queueRowId", SqlDbType.Int) {Value = queueRowId},
				new SqlParameter("@exportResultsTableName", SqlDbType.NVarChar) {Value = exportResultsTableName}
			};

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
		}

		public async Task UpdateStatusAndAgentIdInExportManagerQueueAsync(IDBContext eddsDbContext, Int32 queueRowId, Int32 queueStatus, Int32? agentId)
		{
			String sql = $@"
					UPDATE
						[EDDSDBO].[{Constant.Tables.ExportManagerQueue}]
					SET
						[{Constant.Sql.ColumnsNames.ExportManagerQueue.QueueStatus}] = @queueStatus,
						[{Constant.Sql.ColumnsNames.ExportManagerQueue.AgentId}] = @agentId
					WHERE
						[{Constant.Sql.ColumnsNames.ExportManagerQueue.TableRowId}] = @queueRowId";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@queueRowId", SqlDbType.Int) {Value = queueRowId},
				new SqlParameter("@queueStatus", SqlDbType.Int) {Value = queueStatus},
				new SqlParameter("@agentId", SqlDbType.Int) {Value =(object) agentId ?? DBNull.Value}
			};

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams));
		}

		public async Task<Int32> CountImportWorkerRecordsAsync(IDBContext eddsDbContext, Int32 workspaceArtifactID, Int32 jobID)
		{
			return await Task.Run(() =>
			{
				try
				{
					String sql = $@"
					SELECT
						COUNT([{Constant.Sql.ColumnsNames.ImportWorkerQueue.ID}])
					FROM
						[EDDSDBO].[{Constant.Tables.ImportWorkerQueue}] WITH(NOLOCK)
					WHERE
						[{Constant.Sql.ColumnsNames.ImportWorkerQueue.WorkspaceArtifactID}] = @workspaceArtifactID AND
						[{Constant.Sql.ColumnsNames.ImportWorkerQueue.JobID}] = @jobID
					";
					List<SqlParameter> sqlParams = new List<SqlParameter>
					{
						new SqlParameter("@workspaceArtifactID", SqlDbType.Int) { Value = workspaceArtifactID},
						new SqlParameter("@jobID", SqlDbType.Int) { Value = jobID}
					};
					return eddsDbContext.ExecuteSqlStatementAsScalar<Int32>(sql, sqlParams);
				}
				catch (Exception ex)
				{
					throw new Exception(Constant.ErrorMessages.CountImportWorkerRowsError, ex);
				}
			});
		}

		public async Task UpdateQueueStatusAsync(IDBContext eddsDbContext, Int32 workspaceArtifactID, Int32 jobID, String tableName, String statusColumnName, Int32 status)
		{
			await Task.Run(() =>
			{
				try
				{
					String sql = $@"
						UPDATE
							[EDDSDBO].[{tableName}]
						SET
							[{statusColumnName}] = @updatedStatus
						WHERE
							[{Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceArtifactID}] = @workspaceArtifactID AND
							[{Constant.Sql.ColumnsNames.ImportManagerQueue.JobID}] = @jobID
						";
					List<SqlParameter> sqlParams = new List<SqlParameter>
						{
							new SqlParameter("@updatedStatus", SqlDbType.Int) { Value = status},
							new SqlParameter("@workspaceArtifactID", SqlDbType.Int) { Value = workspaceArtifactID},
							new SqlParameter("@jobID", SqlDbType.Int) { Value = jobID}
						};
					eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams);
				}
				catch (Exception ex)
				{
					throw new Exception(Constant.ErrorMessages.UpdateQueueStatusError, ex);
				}
			});
		}

		public async Task DeleteRecordFromQueueAsync(IDBContext eddsDbContext, String workspaceArtifactIDColumnName, Int32 workspaceArtifactID, String jobIDColumnName, Int32 jobID, String tableName)
		{
			await Task.Run(() =>
			{
				try
				{
					String sql = $@"
						BEGIN TRY
						    BEGIN TRANSACTION

						        DELETE FROM
									[EDDSDBO].[{tableName}]
								WHERE
									[{workspaceArtifactIDColumnName}] = @workspaceArtifactID AND
									[{jobIDColumnName}] = @jobID

						    COMMIT TRANSACTION
						END TRY
						BEGIN CATCH
						    IF @@TRANCOUNT > 0
								BEGIN
									DECLARE @ErrorMessage nvarchar(max), @ErrorSeverity int, @ErrorState int;
									SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
									ROLLBACK TRANSACTION;
									RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
								END
						END CATCH
						";
					List<SqlParameter> sqlParams = new List<SqlParameter>
						{
							new SqlParameter("@workspaceArtifactID", SqlDbType.Int) { Value = workspaceArtifactID},
							new SqlParameter("@jobID", SqlDbType.Int) { Value = jobID}
						};
					eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams);
				}
				catch (Exception ex)
				{
					throw new Exception(Constant.ErrorMessages.DeleteQueueRecordError, ex);
				}
			});
		}

		public async Task DeleteRecordFromQueueAsync(IDBContext eddsDbContext, Int32 agentID, Int32 workspaceArtifactID, Int32 jobID, String tableName)
		{
			await Task.Run(() =>
			{
				try
				{
					string sql = $@"
						BEGIN TRY
						    BEGIN TRANSACTION
									DELETE FROM
										[EDDSDBO].[{tableName}]
									WHERE
										[{Constant.Sql.ColumnsNames.ImportManagerQueue.AgentID}] = @agentID AND
										[{Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceArtifactID}] = @workspaceArtifactID AND
										[{Constant.Sql.ColumnsNames.ImportManagerQueue.JobID}] = @jobID
						    COMMIT TRANSACTION
						END TRY
						BEGIN CATCH
						    IF @@TRANCOUNT > 0
								BEGIN
									DECLARE @ErrorMessage nvarchar(max), @ErrorSeverity int, @ErrorState int;
									SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
									ROLLBACK TRANSACTION;
									RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
								END
						END CATCH
						";

					List<SqlParameter> sqlParams = new List<SqlParameter>
					{
						new SqlParameter("@agentID", SqlDbType.Int) {Value = agentID},
						new SqlParameter("@workspaceArtifactID", SqlDbType.Int) {Value = workspaceArtifactID},
						new SqlParameter("@jobID", SqlDbType.Int) {Value = jobID}
					};
					eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams);
				}
				catch (Exception ex)
				{
					throw new Exception(Constant.ErrorMessages.DeleteQueueRecordError, ex);
				}
			});
		}

		public async Task<DataTable> RetrieveBatchFromExportWorkerResultsTableAsync(IDBContext eddsDbContext, Int32 agentId, Int32 batchSize, String exportResultsTableName)
		{
			String sql = $@"
				BEGIN TRAN
					UPDATE
						[EDDSDBO].[{exportResultsTableName}]
					SET
						[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.AgentId}] = @agentID,
						[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.QueueStatus}] = @inProgressQueueStatus
					WHERE
						[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.TableRowId}] IN
						(
							SELECT
								TOP (@batchSize) [{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.TableRowId}]
							FROM
								[EDDSDBO].[{exportResultsTableName}] WITH(UPDLOCK,READPAST)
							ORDER BY
								[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.TableRowId}] ASC
						)
				COMMIT

				SELECT
					[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.TableRowId}]
					,[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.AgentId}]
					,[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.WorkspaceArtifactId}]
					,[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.ExportJobArtifactId}]
					,[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.ObjectType}]
					,[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.WorkspaceResourceGroupArtifactId}]
					,[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.QueueStatus}]
					,[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.MetaData}]
					,[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.TimeStampUtc}]
				FROM
					[EDDSDBO].[{exportResultsTableName}]
				WHERE
					[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.AgentId}] = @agentID
					AND [{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.QueueStatus}] = @inProgressQueueStatus
				";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@agentID", SqlDbType.Int) {Value = agentId},
				new SqlParameter("@batchSize", SqlDbType.Int) {Value = batchSize},
				new SqlParameter("@inProgressQueueStatus", SqlDbType.Int) {Value = Constant.Status.Queue.IN_PROGRESS}
			};

			return await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams));
		}

		public async Task<Int32> RetrieveTotalRowCountInExportWorkerResultsTableAsync(IDBContext eddsDbContext, String exportResultsTableName)
		{
			String sql = $@"
				SELECT
					COUNT(0)
				FROM
					[EDDSDBO].[{exportResultsTableName}]
				";

			Int32 rowCount = await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsScalar<Int32>(sql));
			return rowCount;
		}

		public async Task DeleteRecordsFromExportWorkerResultsTableAsync(IDBContext eddsDbContext, String exportResultsTableName, List<Int32> tableRowIds)
		{
			String tableRowIdsString = String.Join(Constant.CommaSeparator, tableRowIds);

			String sql = $@"
				DELETE FROM
					[EDDSDBO].[{exportResultsTableName}]
				WHERE
					[{Constant.Sql.ColumnsNames.ExportWorkerResultsTable.TableRowId}] IN ({tableRowIdsString})
				";

			await Task.Run(() => eddsDbContext.ExecuteNonQuerySQLStatement(sql));
		}

		public async Task<Int32> CountNumberOfExportWorkerRecordsAsync(IDBContext eddsDbContext, Int32 workspaceArtifactId, Int32 exportJobArtifactId)
		{
			String sql = $@"
				SELECT
					COUNT(0)
				FROM
					[EDDSDBO].[{Constant.Tables.ExportWorkerQueue}]
				WHERE
					[{Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceArtifactId}] = @workspacdArtifactId AND
					[{Constant.Sql.ColumnsNames.ExportWorkerQueue.ExportJobArtifactId}] = @exportJobArtifactId
				";

			List<SqlParameter> sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@workspacdArtifactId", SqlDbType.Int) {Value = workspaceArtifactId},
				new SqlParameter("@exportJobArtifactId", SqlDbType.Int) {Value = exportJobArtifactId}
			};

			Int32 rowCount = await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsScalar<Int32>(sql, sqlParams));
			return rowCount;
		}

		public async Task<String> RetrieveConfigurationValue(IDBContext eddsDbContext, String sectionName, String name)
		{
			var retVal = String.Empty;
			const String sql = @"SELECT Value
					FROM EDDSDBO.Configuration
					WHERE Section = @sectionName
						AND Name = @name";

			var sqlParams = new SqlParameter[2];
			sqlParams[0] = new SqlParameter("@sectionName", SqlDbType.VarChar) { Value = sectionName };
			sqlParams[1] = new SqlParameter("@name", SqlDbType.VarChar) { Value = name };

			var result = await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsScalar(sql, sqlParams));

			if ((result != null) && (result != DBNull.Value))
			{
				retVal = result.ToString();
			}
			return retVal;
		}

		public DataTable RetrieveJobErrorsFromTempTable(IDBContext dbContext, string tempTableName)
		{
			var sql = $@"SELECT ArtifactID FROM [Resource].[{tempTableName}] WITH(NOLOCK)";

			var dtJobErrors = dbContext.ExecuteSqlStatementAsDataTable(sql);
			return dtJobErrors;
		}

		public async Task<Int32> RetrieveArtifactTypeIdByGuidAsync(IDBContext eddsDbContext, Guid objectTypeGuid)
		{
			try
			{
				String sql = $@"
					SELECT OT.DescriptorArtifactTypeID
					FROM
						EDDSDBO.ArtifactGuid AG WITH(NOLOCK)
						INNER JOIN EDDSDBO.ObjectType OT WITH(NOLOCK) ON AG.ArtifactID = OT.ArtifactID
					WHERE
						AG.ArtifactGuid = @artifactGuid";

				List<SqlParameter> sqlParams = new List<SqlParameter>
				{
					new SqlParameter("@artifactGuid", SqlDbType.UniqueIdentifier) {Value = objectTypeGuid}
				};

				Int32 artifactTypeId = await Task.Run(() => eddsDbContext.ExecuteSqlStatementAsScalar<Int32>(sql, sqlParams));
				return artifactTypeId;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.QueryImportManagerQueueError, ex);
			}
		}

		public async Task<bool> IsLastApplicationInEnvironment(IDBContext dbContextEdds, Int32 currentWorkspaceId)
		{
			try
			{
				const string sql = @"
					DECLARE @applicationID INT = (SELECT [ArtifactID] FROM [EDDSDBO].[LibraryApplication] WITH(NOLOCK) WHERE [GUID] = @applicationGUID)
					SELECT COUNT([CaseApplicationID])
					FROM [EDDSDBO].[CaseApplication]  WITH(NOLOCK)
					WHERE
						[ApplicationID] = @applicationID
						AND [CaseID] <> @workspaceArtifactID";

				List<SqlParameter> sqlParams = new List<SqlParameter>
				{
					new SqlParameter("@applicationGUID", SqlDbType.UniqueIdentifier) {Value = Constant.Guids.Application.ApplicationGuid},
					new SqlParameter("@workspaceArtifactID", SqlDbType.BigInt) {Value = currentWorkspaceId}
				};

				var workspaceCount = await Task.Run(() => dbContextEdds.ExecuteSqlStatementAsScalar<Int32>(sql, sqlParams));

				return workspaceCount > 0;
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.ApplicationCheckforWorkspaceInstallation, ex);
			}
		}

		public async Task RemoveWorkspaceEntriesFromEddsTable(IDBContext dbContextEdds, Int32 currentWorkspaceId, string tableName)
		{
			try
			{
				string sql = $@"DELETE FROM EDDSDBO.[{tableName}] WHERE [WorkspaceArtifactID] = @workspaceArtifactID";

				List<SqlParameter> sqlParams = new List<SqlParameter>
				{
					new SqlParameter("@workspaceArtifactID", SqlDbType.BigInt) {Value = currentWorkspaceId}
				};

				await Task.Run(() => dbContextEdds.ExecuteNonQuerySQLStatement(sql, sqlParams));
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(string.Format(Constant.ErrorMessages.ApplicationPreUninstallRemoveWorkspaceEntries, tableName), ex);
			}
		}

		public async Task DropEddsWorkspaceTableByTableName(IDBContext dbContextEdds, string tableName)
		{
			try
			{
				var sql = $@"IF EXISTS(SELECT 'true' FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = '{tableName}')
											BEGIN
												DROP TABLE EDDSDBO.[{tableName}]
											END";

				await Task.Run(() => dbContextEdds.ExecuteNonQuerySQLStatement(sql));
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(string.Format(Constant.ErrorMessages.ApplicationPreUninstallDropTable, tableName), ex);
			}
		}
	}
}
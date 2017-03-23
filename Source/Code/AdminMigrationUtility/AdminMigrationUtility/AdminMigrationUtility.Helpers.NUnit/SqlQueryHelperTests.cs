using AdminMigrationUtility.Helpers.Exceptions;
using AdminMigrationUtility.Helpers.Models;
using AdminMigrationUtility.Helpers.SQL;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.NUnit
{
	[TestFixture]
	public class SqlQueryHelperTests
	{
		public ISqlQueryHelper Sut { get; set; }
		public Mock<IDBContext> MockEddsDbContext { get; set; }
		public const Int32 AGENT_ID = 123;

		#region Setup and TearDown

		[SetUp]
		public void Setup()
		{
			Sut = new SqlQueryHelper();
			MockEddsDbContext = new Mock<IDBContext>();
		}

		[TearDown]
		public void TearDown()
		{
			Sut = null;
			MockEddsDbContext = null;
		}

		#endregion Setup and TearDown

		#region Tests

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public async Task RetrieveValidKeywordsAndNotesForUserTest_PassesWithValidUser()
		{
			//Arrange
			MockExecuteSqlStatementAsDataTable_ToReturnUserKeywordsAndNotes_ReturnsOneUser();

			//Act
			KeywordsNotesModel keywordsNotesModel = await Sut.RetrieveKeywordsAndNotesForUserAsync(MockEddsDbContext.Object, 123);

			//Assert
			VerifyExecuteSqlStatementAsDataTableWasCalled(1);
			Assert.That(keywordsNotesModel, Is.Not.Null);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void RetrieveValidKeywordsAndNotesForUserTest_FailsWhenDataTableIsNull()
		{
			//Arrange
			MockExecuteSqlStatementAsDataTable_ToReturnUserKeywordsAndNotes_ReturnsNullDataTable();

			//Act
			//Assert
			Exception exception = Assert.Throws<AdminMigrationUtilityException>(async () => await Sut.RetrieveKeywordsAndNotesForUserAsync(MockEddsDbContext.Object, 123));
			Assert.That(exception.ToString(), Is.StringContaining(Constant.ErrorMessages.UserKeywordsNotesError));
			VerifyExecuteSqlStatementAsDataTableWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void RetrieveValidKeywordsAndNotesForUserTest_FailsWhenUserNotFound()
		{
			//Arrange
			MockExecuteSqlStatementAsDataTable_ToReturnUserKeywordsAndNotes_ReturnsZeroUsers();

			//Act
			//Assert
			Exception exception = Assert.Throws<AdminMigrationUtilityException>(async () => await Sut.RetrieveKeywordsAndNotesForUserAsync(MockEddsDbContext.Object, 123));
			Assert.That(exception.ToString(), Is.StringContaining(Constant.ErrorMessages.UserKeywordsNotesError_UserNotFound));
			VerifyExecuteSqlStatementAsDataTableWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void RetrieveValidKeywordsAndNotesForUserTest_FailsForMultipleUsers()
		{
			//Arrange
			MockExecuteSqlStatementAsDataTable_ToReturnUserKeywordsAndNotes_ReturnsTwoUsers();

			//Act
			//Assert
			Exception exception = Assert.Throws<AdminMigrationUtilityException>(async () => await Sut.RetrieveKeywordsAndNotesForUserAsync(MockEddsDbContext.Object, 123));
			Assert.That(exception.ToString(), Is.StringContaining(Constant.ErrorMessages.UserKeywordsNotesError_MultipleUsers));
			VerifyExecuteSqlStatementAsDataTableWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void RetrieveValidKeywordsAndNotesForUserTest_FailsWhenSqlThrowsException()
		{
			//Arrange
			MockExecuteSqlStatementAsDataTable_ToReturnUserKeywordsAndNotes_ThrowsException();

			//Act
			//Assert
			Exception exception = Assert.Throws<AdminMigrationUtilityException>(async () => await Sut.RetrieveKeywordsAndNotesForUserAsync(MockEddsDbContext.Object, 123));
			Assert.That(exception.ToString(), Is.StringContaining(Constant.ErrorMessages.UserKeywordsNotesError));
			VerifyExecuteSqlStatementAsDataTableWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void InsertRowIntoExportWorkerQueueTableAsyncTest_SuccessfullyInserts()
		{
			//Arrange

			//Act

			//Assert
			Assert.DoesNotThrow(async () => await Sut.InsertRowIntoExportWorkerQueueTableAsync(
				eddsDbContext: MockEddsDbContext.Object,
				workspaceArtifactId: 123,
				exportJobArtifactId: 123,
				objectType: Constant.Enums.SupportedObjects.User.ToString(),
				priority: 123,
				workspaceResourceGroupArtifactId: 123,
				queueStatus: Constant.Status.Queue.NOT_STARTED,
				artifactId: 123,
				exportWorkerResultTableName: "resultTableName",
				metadata: "metadata",
				timeStampUtc: DateTime.UtcNow));
			VerifyExecuteNonQuerySqlStatementWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void InsertRowIntoExportWorkerQueueTableAsyncTest_FailsWhenSqlThrowsException()
		{
			//Arrange
			MockExecuteNonQuerySqlStatement_ToInsertUserIntoExportWorkerQueueTable_ThrowsException();

			//Act

			//Assert
			AdminMigrationUtilityException adminMigrationUtilityException = Assert.Throws<AdminMigrationUtilityException>(async () => await Sut.InsertRowIntoExportWorkerQueueTableAsync(
				eddsDbContext: MockEddsDbContext.Object,
				workspaceArtifactId: 123,
				exportJobArtifactId: 123,
				objectType: Constant.Enums.SupportedObjects.User.ToString(),
				priority: 123,
				workspaceResourceGroupArtifactId: 123,
				queueStatus: Constant.Status.Queue.NOT_STARTED,
				artifactId: 123,
				exportWorkerResultTableName: "resultTableName",
				metadata: "metadata",
				timeStampUtc: DateTime.UtcNow));
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining(Constant.ErrorMessages.InsertingAUserIntoExportWorkerQueueTableError));
			VerifyExecuteNonQuerySqlStatementWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void ResetUnfishedExportWorkerJobsAsyncTest_SuccessfullyResets()
		{
			//Arrange
			MockExecuteNonQuerySqlStatement_ResetUnfishedJobsInExportWorkerQueueTable();

			//Act
			//Assert
			Assert.That(async () => await Sut.ResetUnfishedExportWorkerJobsAsync(
				eddsDbContext: MockEddsDbContext.Object,
				agentId: 123),
				Throws.Nothing);
			VerifyExecuteNonQuerySqlStatementWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void ResetUnfishedExportWorkerJobsAsyncTest_FailsWhenSqlThrowsException()
		{
			//Arrange
			MockExecuteNonQuerySqlStatement_ResetUnfishedJobsInExportWorkerQueueTable_ThrowsException();

			//Act
			//Assert
			AdminMigrationUtilityException adminMigrationUtilityException = Assert.Throws<AdminMigrationUtilityException>(async () => await Sut.ResetUnfishedExportWorkerJobsAsync(
				eddsDbContext: MockEddsDbContext.Object,
				agentId: 123));
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining(Constant.ErrorMessages.ResetUnfishedExportWorkerJobsError));
			VerifyExecuteNonQuerySqlStatementWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		[TestCase(0)]
		[TestCase(1)]
		[TestCase(5)]
		public void RetrieveNextBatchInExportWorkerQueueAsyncTest_ReturnsDataTableWithSpecifiedNumberOfDataRows(Int32 dataRowCount)
		{
			//Arrange
			MockExecuteSqlStatementAsDataTable_ToReturnNextBatchInExportWorkerQueueAsync_ReturnsDataRows(dataRowCount);
			DataTable batchDataTable = null;

			//Act
			//Assert
			Assert.That(async () =>
			batchDataTable = await Sut.RetrieveNextBatchInExportWorkerQueueAsync(
				eddsDbContext: MockEddsDbContext.Object,
				agentId: 123,
				batchSize: dataRowCount,
				uniqueBatchTableName: "uniqueBatchTableName",
				commaDelimitedResourceAgentIds: "commaDelimitedResourceAgentIds"),
				Throws.Nothing);
			Assert.That(batchDataTable, Is.Not.Null);
			Assert.That(batchDataTable.Rows.Count, Is.EqualTo(dataRowCount));
			VerifyExecuteSqlStatementAsDataTableWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void RetrieveNextBatchInExportWorkerQueueAsyncTest_FailsWhenSqlThrowsException()
		{
			//Arrange
			MockExecuteSqlStatementAsDataTable_ToReturnNextBatchInExportWorkerQueueAsync_ThrowsException();

			//Act
			//Assert
			AdminMigrationUtilityException adminMigrationUtilityException = Assert.Throws<AdminMigrationUtilityException>(async () => await Sut.RetrieveNextBatchInExportWorkerQueueAsync(
				eddsDbContext: MockEddsDbContext.Object,
				agentId: 123,
				batchSize: 123,
				uniqueBatchTableName: "uniqueBatchTableName",
				commaDelimitedResourceAgentIds: "commaDelimitedResourceAgentIds"));
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining(Constant.ErrorMessages.RetrieveNextBatchInExportWorkerQueueError));
			VerifyExecuteSqlStatementAsDataTableWasCalled(1);
		}

		#endregion Tests

		#region Mocks

		public void MockExecuteSqlStatementAsDataTable_ToReturnUserKeywordsAndNotes_ReturnsOneUser()
		{
			DataTable userKeywordsNotesdataTable = new DataTable();
			userKeywordsNotesdataTable.Columns.Add(new DataColumn("Keywords", typeof(String)));
			userKeywordsNotesdataTable.Columns.Add(new DataColumn("Notes", typeof(String)));
			userKeywordsNotesdataTable.Rows.Add("keywords1", "notes1");

			MockEddsDbContext
				.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<String>(), It.IsAny<List<SqlParameter>>()))
				.Returns(userKeywordsNotesdataTable);
		}

		public void MockExecuteSqlStatementAsDataTable_ToReturnUserKeywordsAndNotes_ReturnsTwoUsers()
		{
			DataTable userKeywordsNotesdataTable = new DataTable();
			userKeywordsNotesdataTable.Columns.Add(new DataColumn("Keywords", typeof(String)));
			userKeywordsNotesdataTable.Columns.Add(new DataColumn("Notes", typeof(String)));
			userKeywordsNotesdataTable.Rows.Add("keywords1", "notes1");
			userKeywordsNotesdataTable.Rows.Add("keywords2", "notes2");

			MockEddsDbContext
				.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<String>(), It.IsAny<List<SqlParameter>>()))
				.Returns(userKeywordsNotesdataTable);
		}

		public void MockExecuteSqlStatementAsDataTable_ToReturnUserKeywordsAndNotes_ReturnsZeroUsers()
		{
			DataTable userKeywordsNotesdataTable = new DataTable();
			userKeywordsNotesdataTable.Columns.Add(new DataColumn("Keywords", typeof(String)));
			userKeywordsNotesdataTable.Columns.Add(new DataColumn("Notes", typeof(String)));

			MockEddsDbContext
				.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<String>(), It.IsAny<List<SqlParameter>>()))
				.Returns(userKeywordsNotesdataTable);
		}

		public void MockExecuteSqlStatementAsDataTable_ToReturnUserKeywordsAndNotes_ReturnsNullDataTable()
		{
			MockEddsDbContext
				.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<String>(), It.IsAny<List<SqlParameter>>()))
				.Returns((DataTable)null);
		}

		public void MockExecuteSqlStatementAsDataTable_ToReturnUserKeywordsAndNotes_ThrowsException()
		{
			MockEddsDbContext
				.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<String>(), It.IsAny<List<SqlParameter>>()))
				.Throws<Exception>();
		}

		public void MockExecuteNonQuerySqlStatement_ToInsertUserIntoExportWorkerQueueTable_ThrowsException()
		{
			MockEddsDbContext
				.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<String>(), It.IsAny<List<SqlParameter>>()))
				.Throws<Exception>();
		}

		public void MockExecuteNonQuerySqlStatement_ResetUnfishedJobsInExportWorkerQueueTable()
		{
			MockEddsDbContext
				.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<String>(), It.IsAny<List<SqlParameter>>()))
				.Returns(123);
		}

		public void MockExecuteNonQuerySqlStatement_ResetUnfishedJobsInExportWorkerQueueTable_ThrowsException()
		{
			MockEddsDbContext
				.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<String>(), It.IsAny<List<SqlParameter>>()))
				.Throws<Exception>();
		}

		public void MockExecuteSqlStatementAsDataTable_ToReturnNextBatchInExportWorkerQueueAsync_ReturnsDataRows(Int32 dataRowCount)
		{
			DataTable batchDataTable = GetExportWorkerQueueDataTableWithDataRows(
				dataRowCount: dataRowCount,
				queueRowId: 123,
				agentId: AGENT_ID,
				workspaceArtifactId: 123,
				exportJobArtifactId: 123,
				objectType: Constant.Enums.SupportedObjects.User.ToString(),
				priority: 123,
				workspaceResourceGroupArtifactId: 123,
				queueStatus: Constant.Status.Queue.NOT_STARTED,
				artifactId: 123,
				resultsTableName: "resultsTableName",
				metaData: "metaData",
				timeStampUtc: DateTime.UtcNow);

			MockEddsDbContext
				.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<String>(), It.IsAny<List<SqlParameter>>()))
				.Returns(batchDataTable);
		}

		public void MockExecuteSqlStatementAsDataTable_ToReturnNextBatchInExportWorkerQueueAsync_ThrowsException()
		{
			MockEddsDbContext
				.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<String>(), It.IsAny<List<SqlParameter>>()))
				.Throws<Exception>();
		}

		#endregion Mocks

		#region Verify

		public void VerifyExecuteSqlStatementAsDataTableWasCalled(Int32 timesCalled)
		{
			MockEddsDbContext
				.Verify(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<String>(), It.IsAny<List<SqlParameter>>()),
				Times.Exactly(timesCalled));
		}

		public void VerifyExecuteNonQuerySqlStatementWasCalled(Int32 timesCalled)
		{
			MockEddsDbContext
				.Verify(x => x.ExecuteNonQuerySQLStatement(It.IsAny<String>(), It.IsAny<List<SqlParameter>>()),
				Times.Exactly(timesCalled));
		}

		#endregion Verify

		#region TestHelpers

		private DataTable GetExportWorkerQueueDataTable()
		{
			DataTable exportWorkerQueueDataTable = new DataTable();
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.TableRowId, typeof(Int32));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.AgentId, typeof(Int32));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceArtifactId, typeof(Int32));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.ExportJobArtifactId, typeof(Int32));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.ObjectType, typeof(String));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.Priority, typeof(Int32));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceResourceGroupArtifactId, typeof(Int32));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.QueueStatus, typeof(Int32));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.ArtifactId, typeof(Int32));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.ResultsTableName, typeof(String));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.MetaData, typeof(String));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.TimeStampUtc, typeof(DateTime));
			return exportWorkerQueueDataTable;
		}

		private DataRow GetSingleExportWorkerQueueDataRow(Int32 queueRowId, Int32 agentId, Int32 workspaceArtifactId, Int32 exportJobArtifactId, String objectType, Int32 priority, Int32 workspaceResourceGroupArtifactId, Int32 queueStatus, Int32 artifactId, String resultsTableName, String metaData, DateTime timeStampUtc, DataTable exportWorkerQueueDataTable)
		{
			DataRow exportWorkerQueueDataRow = exportWorkerQueueDataTable.NewRow();
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.TableRowId] = queueRowId;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.AgentId] = agentId;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceArtifactId] = workspaceArtifactId;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.ExportJobArtifactId] = exportJobArtifactId;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.ObjectType] = objectType;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.Priority] = priority;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceResourceGroupArtifactId] = workspaceResourceGroupArtifactId;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.QueueStatus] = queueStatus;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.ArtifactId] = artifactId;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.ResultsTableName] = resultsTableName;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.MetaData] = metaData;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.TimeStampUtc] = timeStampUtc;
			return exportWorkerQueueDataRow;
		}

		private DataTable GetExportWorkerQueueDataTableWithDataRows(Int32 dataRowCount, Int32 queueRowId, Int32 agentId, Int32 workspaceArtifactId, Int32 exportJobArtifactId, String objectType, Int32 priority, Int32 workspaceResourceGroupArtifactId, Int32 queueStatus, Int32 artifactId, String resultsTableName, String metaData, DateTime timeStampUtc)
		{
			DataTable exportWorkerQueueDataTable = GetExportWorkerQueueDataTable();

			for (Int32 i = 1; i <= dataRowCount; i++)
			{
				DataRow exportWorkerQueueDataRow = GetSingleExportWorkerQueueDataRow(
					queueRowId: queueRowId,
					agentId: agentId,
					workspaceArtifactId: workspaceArtifactId,
					exportJobArtifactId: exportJobArtifactId,
					objectType: objectType,
					priority: priority,
					workspaceResourceGroupArtifactId: workspaceResourceGroupArtifactId,
					queueStatus: queueStatus,
					artifactId: artifactId,
					resultsTableName: resultsTableName,
					metaData: metaData,
					timeStampUtc: timeStampUtc,
					exportWorkerQueueDataTable: exportWorkerQueueDataTable);

				exportWorkerQueueDataTable.Rows.Add(exportWorkerQueueDataRow);
			}

			return exportWorkerQueueDataTable;
		}

		#endregion TestHelpers
	}
}